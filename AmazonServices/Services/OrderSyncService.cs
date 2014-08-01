using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MarketplaceWebServiceOrders.Model;
using System.Threading;
using BerkeleyEntities;
using System.Timers;
using NLog;

namespace AmazonServices
{
    public class OrderSyncService
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private berkeleyEntities _dataContext = new berkeleyEntities();
        //private List<string> _pendingSync = new List<string>();
        private AmznMarketplace _marketplace;
        private DateTime _currentSyncTime;

        private System.Timers.Timer _timer2Sec = null;
        private System.Timers.Timer _timer1Min = null;

        private int _listOrderItemsQuota = 30;
        private int _listOrdersQuota = 6;
        private int _getOrdersQuota = 6;

        public OrderSyncService(int marketplaceID)
        {
            _marketplace = _dataContext.AmznMarketplaces.Single(p => p.ID == marketplaceID);
        }

        public void MarginalSync()
        {
            Initilialize();

            _currentSyncTime = DateTime.UtcNow.AddMinutes(-5);

            //var waitingOrders = _marketplace.GetWaitingSyncOrders();

            //if (waitingOrders.Count > 0)
            //{
            //    SyncOrders(waitingOrders);
            //}

            if (!_marketplace.OrderSyncTime.HasValue)
            {
                DateTime from = _currentSyncTime.AddDays(-30).ToLocalTime();
                DateTime to = _currentSyncTime.ToLocalTime();

                SyncOrdersByModifiedTime(from, to);
            }
            else
            {
                DateTime from = _marketplace.OrderSyncTime.Value.AddMinutes(-5).ToLocalTime();
                DateTime to = _currentSyncTime.ToLocalTime();

                SyncOrdersByModifiedTime(from, to);
            }

            //_marketplace.SetWaitingSyncOrders(_pendingSync);

            _marketplace.OrderSyncTime = _currentSyncTime;

            _dataContext.SaveChanges();

            _dataContext.Dispose();

            _timer2Sec.Enabled = false;
            _timer1Min.Enabled = false;
        }

        private void Initilialize()
        {
            _timer2Sec = new System.Timers.Timer(2100);
            _timer2Sec.Elapsed += new System.Timers.ElapsedEventHandler(RestoreEvent2Sec);

            _timer1Min = new System.Timers.Timer(60100);
            _timer1Min.Elapsed += new System.Timers.ElapsedEventHandler(RestoreEvent1Min);

            _timer2Sec.Enabled = true;
            _timer1Min.Enabled = true;
        }

        private void SyncOrders(IEnumerable<string> orderIds)
        {
            GetOrderRequest request = new GetOrderRequest();
            request.AmazonOrderId = new OrderIdList();
            request.AmazonOrderId.Id = orderIds.ToList();
            request.SellerId = _marketplace.MerchantId;

            while (_getOrdersQuota < 1)
            {
                Thread.Sleep(1000);
            }

            try
            {
                GetOrderResponse response = _marketplace.GetMWSOrdersClient().GetOrder(request);

                _getOrdersQuota--;

                ProcessOrders(response.GetOrderResult.Orders.Order);
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);
            }
        }

        private void SyncOrdersByModifiedTime(DateTime from, DateTime to)
        {
            ListOrdersRequest request = new ListOrdersRequest();
            request.MarketplaceId = new MarketplaceIdList();
            request.MarketplaceId.Id = new List<string>() { _marketplace.MarketplaceId };
            request.SellerId = _marketplace.MerchantId;
            request.LastUpdatedAfter = from;
            request.LastUpdatedBefore = to;
            
            while (_listOrdersQuota < 1)
            {
                Thread.Sleep(1000);
            }

            
            ListOrdersResponse response = _marketplace.GetMWSOrdersClient().ListOrders(request);

            _listOrdersQuota--;

            ProcessOrders(response.ListOrdersResult.Orders.Order);

            if (response.ListOrdersResult.IsSetNextToken())
            {
                ListOrderByNextToken(response.ListOrdersResult.NextToken);
            }
            
        }

        private void ListOrderByNextToken(string nextToken)
        {
            ListOrdersByNextTokenRequest request = new ListOrdersByNextTokenRequest();
            request.NextToken = nextToken;
            request.SellerId = _marketplace.MerchantId;

            while (_listOrdersQuota < 1)
            {
                Thread.Sleep(1000);
            }

            ListOrdersByNextTokenResponse response = _marketplace.GetMWSOrdersClient().ListOrdersByNextToken(request);

            _listOrdersQuota--;

            ProcessOrders(response.ListOrdersByNextTokenResult.Orders.Order);

            if (response.ListOrdersByNextTokenResult.IsSetNextToken())
            {
                ListOrderByNextToken(response.ListOrdersByNextTokenResult.NextToken);
            }
        }

        private List<OrderItem> ListOrderItems(string orderID)
        {
            ListOrderItemsRequest request = new ListOrderItemsRequest();
            request.AmazonOrderId = orderID;
            request.SellerId = _marketplace.MerchantId;

            while (_listOrderItemsQuota < 1)
            {
                Thread.Sleep(1000);
            }

            ListOrderItemsResponse response = _marketplace.GetMWSOrdersClient().ListOrderItems(request);

            _listOrderItemsQuota--;

            List<OrderItem> orderItems = response.ListOrderItemsResult.OrderItems.OrderItem;

            if (response.ListOrderItemsResult.IsSetNextToken())
            {
                orderItems.AddRange(ListOrderItemsByNextToken(response.ListOrderItemsResult.NextToken));
            }

            return orderItems;
        }

        private List<OrderItem> ListOrderItemsByNextToken(string nextToken)
        {
            ListOrderItemsByNextTokenRequest request = new ListOrderItemsByNextTokenRequest();
            request.NextToken = nextToken;
            request.SellerId = _marketplace.MerchantId;

            while (_listOrderItemsQuota < 1)
            {
                Thread.Sleep(1000);
            }

            ListOrderItemsByNextTokenResponse response = _marketplace.GetMWSOrdersClient().ListOrderItemsByNextToken(request);

            _listOrderItemsQuota--;

            List<OrderItem> orderItems = response.ListOrderItemsByNextTokenResult.OrderItems.OrderItem;

            if (response.ListOrderItemsByNextTokenResult.IsSetNextToken())
            {
                orderItems.AddRange(ListOrderItemsByNextToken(response.ListOrderItemsByNextTokenResult.NextToken));
            }

            return orderItems;
        }

        private void ProcessOrders(List<Order> orders)
        {
            foreach (Order amznOrder in orders)
            {
                try
                {
                    List<OrderItem> orderItems = ListOrderItems(amznOrder.AmazonOrderId);

                    PersistOrder(amznOrder, orderItems);
                }
                catch (Exception e)
                {
                    //if (!_pendingSync.Contains(amznOrder.AmazonOrderId))
                    //{
                    //    _pendingSync.Add(amznOrder.AmazonOrderId);
                    //}

                    _logger.Error(string.Format("Order ( {1} | {0} ) synchronization failed: {2}", amznOrder.AmazonOrderId, _marketplace.Code, e.Message));

                }
            }
        }

        private void PersistOrder(Order orderDto, List<OrderItem> orderItemsDto)
        {
            AmznOrder order = _dataContext.AmznOrders
                .SingleOrDefault(p => p.MarketplaceID == _marketplace.ID && p.Code.Equals(orderDto.AmazonOrderId));

            if (order == null)
            {
                order = new AmznOrder();
                order.Code = orderDto.AmazonOrderId;
                order.MarketplaceID = _marketplace.ID;
            }

            order.Status = orderDto.OrderStatus.ToString();
            order.LastUpdatedDate = orderDto.LastUpdateDate;
            order.BuyerName = orderDto.BuyerName != null ? orderDto.BuyerName : "";
            order.PaymentMethod = orderDto.PaymentMethod.ToString();
            order.PurchaseDate = orderDto.PurchaseDate;
            order.ShipServiceLevel = orderDto.ShipServiceLevel;
            order.Total = orderDto.IsSetOrderTotal() ? decimal.Parse(orderDto.OrderTotal.Amount) : 0;
            order.LastSyncTime = _currentSyncTime;
            order.AddressLine1 = orderDto.IsSetShippingAddress() ? orderDto.ShippingAddress.AddressLine1 : "";
            order.AddressLine2 = orderDto.IsSetShippingAddress() ? orderDto.ShippingAddress.AddressLine2 : "";
            order.AddressLine3 = orderDto.IsSetShippingAddress() ? orderDto.ShippingAddress.AddressLine3 : "";
            order.City = orderDto.IsSetShippingAddress() ? orderDto.ShippingAddress.City : "";
            order.CountryCode = orderDto.IsSetShippingAddress() ? orderDto.ShippingAddress.CountryCode : "";
            order.County = orderDto.IsSetShippingAddress() ? orderDto.ShippingAddress.County : "";
            order.District = orderDto.IsSetShippingAddress() ? orderDto.ShippingAddress.District : "";
            order.Phone = orderDto.IsSetShippingAddress() ? orderDto.ShippingAddress.Phone : "";
            order.PostalCode = orderDto.IsSetShippingAddress() ? orderDto.ShippingAddress.PostalCode : "";
            order.StateOrRegion = orderDto.IsSetShippingAddress() ? orderDto.ShippingAddress.StateOrRegion : "";

            foreach (OrderItem orderItemDto in orderItemsDto)
            {
                AmznOrderItem orderItem = order.OrderItems.SingleOrDefault(p => p.ListingItem.Item.ItemLookupCode.Equals(orderItemDto.SellerSKU));

                if (orderItem == null)
                {
                    orderItem = new AmznOrderItem();
                    orderItem.Code = orderItemDto.OrderItemId;

                    orderItem.ListingItem = _dataContext.AmznListingItems.Single(p => 
                        p.MarketplaceID.Equals(_marketplace.ID) &&
                        p.Item.ItemLookupCode.Equals(orderItemDto.SellerSKU));

                    order.OrderItems.Add(orderItem);
                }

                AmznListingItem listingItem = orderItem.ListingItem;

                if (listingItem.LastSyncTime < orderDto.PurchaseDate)
                {
                    int qtyChange = (int)orderItemDto.QuantityOrdered - orderItem.QuantityOrdered;

                    if (qtyChange > 0)
                    {
                        listingItem.Quantity = listingItem.Quantity - qtyChange;
                        listingItem.LastSyncTime = _currentSyncTime;
                    }
                }

                orderItem.ItemPrice = orderItemDto.IsSetItemPrice() ? decimal.Parse(orderItemDto.ItemPrice.Amount) : 0;
                orderItem.ShippingPrice = orderItemDto.IsSetShippingPrice() ? decimal.Parse(orderItemDto.ShippingPrice.Amount) : 0;
                orderItem.PromotionDiscount = orderItemDto.IsSetPromotionDiscount() ? decimal.Parse(orderItemDto.PromotionDiscount.Amount) : 0;
                orderItem.ShippingDiscount = orderItemDto.IsSetShippingDiscount() ? decimal.Parse(orderItemDto.ShippingDiscount.Amount) : 0;
                orderItem.QuantityOrdered = (int)orderItemDto.QuantityOrdered;
                orderItem.QuantityShipped = (int)orderItemDto.QuantityShipped;
            }
        }

        private void RestoreEvent2Sec(object source, ElapsedEventArgs e)
        {
            if (_listOrderItemsQuota < 30)
            {
                _listOrderItemsQuota++;
            }
        }

        private void RestoreEvent1Min(object source, ElapsedEventArgs e)
        {
            if (_listOrdersQuota < 6)
            {
                _listOrdersQuota++;
            }

            if (_getOrdersQuota < 6)
            {
                _getOrdersQuota++;
            }
        }
    }
}
