using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using eBay.Service.Core.Soap;
using BerkeleyEntities;
using eBay.Service.Call;
using System.IO;
using NLog;


namespace EbayServices
{
    public class OrderSyncService
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private berkeleyEntities _dataContext = new berkeleyEntities();
        //private List<string> _pendingSync = new List<string>();
        private EbayMarketplace _marketplace;
        private DateTime _currentSyncTime;

        public OrderSyncService(int marketplaceID)
        {
            _marketplace = _dataContext.EbayMarketplaces.Single(p => p.ID == marketplaceID);
        }


        public void MarginalSync()
        {
            _currentSyncTime = DateTime.UtcNow.AddMinutes(-3);

            //var waitingOrders = new StringCollection(_marketplace.GetWaitingSyncOrders().ToArray());

            //if (waitingOrders.Count > 0)
            //{
            //    SyncOrders(waitingOrders);
            //}
            
            DateTime from = _marketplace.OrdersSyncTime.HasValue ? _marketplace.OrdersSyncTime.Value.AddMinutes(-5) : DateTime.UtcNow.AddDays(-29);

            SyncOrdersByModifiedTime(from, _currentSyncTime);

            //_marketplace.SetWaitingSyncOrders(_pendingSync);

            _marketplace.OrdersSyncTime = _currentSyncTime;

            _dataContext.SaveChanges();
        }

        private void SyncOrdersByModifiedTime(DateTime from, DateTime to)
        {
            GetOrdersRequestType request = new GetOrdersRequestType();
            request.OrderStatus = OrderStatusCodeType.All;
            request.Pagination = new PaginationType();
            request.Pagination.EntriesPerPage = 100;
            request.Pagination.PageNumber = 1;
            request.DetailLevel = new DetailLevelCodeTypeCollection();
            request.DetailLevel.Add(DetailLevelCodeType.ReturnAll);

            request.ModTimeFromSpecified = true;
            request.ModTimeToSpecified = true;
            request.ModTimeFrom = from;
            request.ModTimeTo = to;

            ProcessOrderData(ExecuteGetOrders(request));
        }

        private void SyncOrdersByCreatedTime(DateTime from, DateTime to)
        {
            GetOrdersRequestType request = new GetOrdersRequestType();
            request.OrderStatus = OrderStatusCodeType.All;
            request.Pagination = new PaginationType();
            request.Pagination.EntriesPerPage = 100;
            request.Pagination.PageNumber = 1;
            request.DetailLevel = new DetailLevelCodeTypeCollection();
            request.DetailLevel.Add(DetailLevelCodeType.ReturnAll);


            request.CreateTimeFromSpecified = true;
            request.CreateTimeToSpecified = true;
            request.CreateTimeFrom = from;
            request.CreateTimeTo = to;


            ProcessOrderData(ExecuteGetOrders(request));

        }

        private void SyncOrders(StringCollection orderIds)
        {
            GetOrdersRequestType request = new GetOrdersRequestType();
            request.OrderStatus = OrderStatusCodeType.All;
            request.Pagination = new PaginationType();
            request.Pagination.EntriesPerPage = 100;
            request.Pagination.PageNumber = 1;
            request.DetailLevel = new DetailLevelCodeTypeCollection();
            request.DetailLevel.Add(DetailLevelCodeType.ReturnAll);
            request.OrderIDArray = orderIds;


            ProcessOrderData(ExecuteGetOrders(request));

            
        }

        private OrderTypeCollection ExecuteGetOrders(GetOrdersRequestType request)
        {
            GetOrdersCall call = new GetOrdersCall(_marketplace.GetApiContext());

            GetOrdersResponseType response = call.ExecuteRequest(request) as GetOrdersResponseType;

            while (request.Pagination.PageNumber < response.PaginationResult.TotalNumberOfPages)
            {
                request.Pagination.PageNumber++;

                GetOrdersResponseType additionalResponse = call.ExecuteRequest(request) as GetOrdersResponseType;

                response.OrderArray.AddRange(additionalResponse.OrderArray);
            }

            return response.OrderArray;
        }

        private void ProcessOrderData(OrderTypeCollection orders)
        {
            foreach (OrderType orderDto in orders)
            {
                try
                {
                    PersistOrder(orderDto);
                }
                catch (Exception e)
                {
                    _logger.Error(string.Format("Order ( {1} | {0} ) synchronization failed: {2}", orderDto.ShippingDetails.SellingManagerSalesRecordNumber, _marketplace.Code, e.Message));
                }
            }

            _dataContext.SaveChanges();

            var combinedOrders = orders.ToArray().Where(p => p.TransactionArray.Count > 1);

            foreach (OrderType orderDto in combinedOrders)
            {
                RemoveParentOrder(orderDto);
            }

            _dataContext.SaveChanges();
        }

        private void PersistOrder(OrderType orderDto)
        {
            EbayOrder order = _dataContext.EbayOrders.SingleOrDefault(p => p.Code.Equals(orderDto.OrderID) && p.MarketplaceID == _marketplace.ID);

            if (order == null)
            {
                order = new EbayOrder();
                order.Code = orderDto.OrderID;
                order.MarketplaceID = _marketplace.ID;
                order.CreatedTime = orderDto.CreatedTime;
            }

            order.BuyerID = orderDto.BuyerUserID;
            order.OrderStatus = orderDto.OrderStatus.ToString();
            order.SalesRecordNumber = orderDto.ShippingDetails.SellingManagerSalesRecordNumber.ToString();
            order.EbayPaymentStatus = orderDto.CheckoutStatus.eBayPaymentStatus.ToString();
            order.CheckoutStatus = orderDto.CheckoutStatus.Status.ToString();
            order.PaymentMethod = orderDto.CheckoutStatus.PaymentMethod.ToString();
            order.PaidTime = orderDto.PaidTimeSpecified ? (DateTime?)orderDto.PaidTime : null;
            order.ShippedTime = orderDto.ShippedTimeSpecified ? (DateTime?)orderDto.ShippedTime : null;
            order.PaidAmount = Convert.ToDecimal(orderDto.AmountPaid.Value);
            order.CompanyName = orderDto.ShippingAddress.CompanyName;
            order.Street1 = orderDto.ShippingAddress.Street1;
            order.Street2 = orderDto.ShippingAddress.Street2;
            order.CityName = orderDto.ShippingAddress.CityName;
            order.StateOrProvince = orderDto.ShippingAddress.StateOrProvince;
            order.PostalCode = orderDto.ShippingAddress.PostalCode;
            order.CountryCode = orderDto.ShippingAddress.Country.ToString();
            order.CountryName = orderDto.ShippingAddress.CountryName;
            order.UserName = orderDto.ShippingAddress.Name;
            order.Phone = orderDto.ShippingAddress.Phone;
            order.Subtotal = Convert.ToDecimal(orderDto.Subtotal.Value);
            order.AdjustmentAmount = Convert.ToDecimal(orderDto.AdjustmentAmount.Value);
            order.Total = Convert.ToDecimal(orderDto.Total.Value);
            


            order.ShippingService = orderDto.ShippingServiceSelected != null ? orderDto.ShippingServiceSelected.ShippingService : "N/A";

            order.ExpeditedService = orderDto.ShippingServiceSelected != null && orderDto.ShippingServiceSelected.ExpeditedServiceSpecified ?
                orderDto.ShippingServiceSelected.ExpeditedService : order.ExpeditedService;

            order.ShippingInsuranceCost = orderDto.ShippingServiceSelected.ShippingInsuranceCost != null ?
                Convert.ToDecimal(orderDto.ShippingServiceSelected.ShippingInsuranceCost.Value) : 0;

            order.ShippingServiceCost = orderDto.ShippingServiceSelected.ShippingServiceCost != null ?
                Convert.ToDecimal(orderDto.ShippingServiceSelected.ShippingServiceCost.Value) : 0;

            order.ShippingServiceAdditionalCost = orderDto.ShippingServiceSelected.ShippingServiceAdditionalCost != null ?
                Convert.ToDecimal(orderDto.ShippingServiceSelected.ShippingServiceAdditionalCost.Value) : 0;

            order.ShippingSurcharge = orderDto.ShippingServiceSelected.ShippingSurcharge != null ?
                Convert.ToDecimal(orderDto.ShippingServiceSelected.ShippingSurcharge.Value) : 0;

            order.LastSyncTime = _currentSyncTime;

            foreach (TransactionType orderItemDto in orderDto.TransactionArray)
            {
                EbayOrderItem orderItem = order.OrderItems.SingleOrDefault(p => p.Code.Equals(orderItemDto.OrderLineItemID));

                if (orderItem == null)
                {
                    orderItem = CreateOrderItem(order, orderItemDto);
                }

                orderItem.TransactionPrice = orderItemDto.TransactionPrice != null ? decimal.Parse(orderItemDto.TransactionPrice.Value.ToString()) : orderItem.TransactionPrice;
                orderItem.QuantityPurchased = orderItemDto.QuantityPurchased;
                orderItem.UnpaidItemDisputeStatus = orderItemDto.UnpaidItem != null ? orderItemDto.UnpaidItem.Status.ToString() : "N/A";
                orderItem.UnpaidItemDisputeType = orderItemDto.UnpaidItem != null ? orderItemDto.UnpaidItem.Type.ToString() : "N/A";
            }

        }

        private EbayOrderItem CreateOrderItem(EbayOrder order, TransactionType orderItemDto)
        {
            EbayOrderItem orderItem = new EbayOrderItem();

            string sku = orderItemDto.Variation == null ? orderItemDto.Item.SKU : orderItemDto.Variation.SKU;
            string listingID = orderItemDto.Item.ItemID;

            
            EbayListing listing = _dataContext.EbayListings.Single(p => p.MarketplaceID.Equals(_marketplace.ID) && p.Code.Equals(listingID));
            orderItem.ListingItem = listing.ListingItems.Single(p => p.Item.ItemLookupCode.Equals(sku));
            orderItem.Order = order;
            orderItem.CreatedDate = orderItemDto.CreatedDate;
            orderItem.Code = orderItemDto.OrderLineItemID;
            

            return orderItem;
        }

        private void RemoveParentOrder(OrderType orderDto)
        {
            if (orderDto.TransactionArray.Count > 1)
            {
                var orderItems = orderDto.TransactionArray.ToArray().Select(p => p.OrderLineItemID).ToList();

                foreach (string orderItemID in orderItems)
                {
                    EbayOrder order = _dataContext.EbayOrders.SingleOrDefault(p => p.MarketplaceID == _marketplace.ID && p.Code.Equals(orderItemID));

                    if (order != null)
                    {
                        _dataContext.EbayOrders.DeleteObject(order);
                    }
                }
            }
        }
    }
}
