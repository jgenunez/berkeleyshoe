using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using BerkeleyEntities;
using System.IO;

namespace MarketplaceManager
{
    public class MarketplaceViewRepository
    {
        private berkeleyEntities _dataContext;
        private double _totalOH = -1;

        public void Refresh(MarketplaceView view)
        {
            using (_dataContext = new berkeleyEntities())
            {
                if (view.Host.Equals("Ebay"))
                {
                    int marketplaceID = int.Parse(view.ID.Split(new Char[1] { '-' })[1]);

                    MarketplaceView newView = CreateMarketplaceView(_dataContext.EbayMarketplaces.Single(p => p.ID == marketplaceID));
                    view.ListingsLastSync = newView.ListingsLastSync;
                    view.OrdersLastSync = newView.OrdersLastSync;
                    view.Name = newView.Name;
                    view.Host = newView.Host;
                }
                else if (view.Host.Equals("Amazon"))
                {
                    int marketplaceID = int.Parse(view.ID.Split(new Char[1] { '-' })[1]);

                    MarketplaceView newView = CreateMarketplaceView(_dataContext.AmznMarketplaces.Single(p => p.ID == marketplaceID));
                    view.ListingsLastSync = newView.ListingsLastSync;
                    view.OrdersLastSync = newView.OrdersLastSync;
                    view.Name = newView.Name;
                    view.Host = newView.Host;
                } 
            }
        }

        public IEnumerable<MarketplaceView> GetAllMarketplaceViews()
        {
            List<MarketplaceView> views = new List<MarketplaceView>();

            using (_dataContext = new berkeleyEntities())
            {
                foreach (EbayMarketplace marketplace in _dataContext.EbayMarketplaces)
                {
                    views.Add(CreateMarketplaceView(marketplace));
                }


                foreach (AmznMarketplace marketplace in _dataContext.AmznMarketplaces)
                {
                    views.Add(CreateMarketplaceView(marketplace));
                } 
            }

            return views;
        }

        //public IEnumerable<MarketplaceView> GetAllExtendedMarketplaceViews()
        //{
        //    List<MarketplaceView> views = new List<MarketplaceView>();


        //    using (_dataContext = new berkeleyEntities())
        //    {
        //        foreach (EbayMarketplace marketplace in _dataContext.EbayMarketplaces)
        //        {
        //            views.Add(CreateExtendedMarketplaceView(marketplace));
        //        }


        //        foreach (AmznMarketplace marketplace in _dataContext.AmznMarketplaces)
        //        {
        //            views.Add(CreateExtendedMarketplaceView(marketplace));
        //        } 
        //    }

        //    return views;
        //}

        public void GetAdditionalData(MarketplaceView view)
        {
            using (_dataContext = new berkeleyEntities())
            {
                if (view.Host.Equals("Amazon"))
                {

                    view.ActiveListingQty = _dataContext.AmznListingItems.Where(p => p.MarketplaceID == view.DbID && p.IsActive).Sum(p => p.Quantity);

                    var orderItems = _dataContext.AmznOrderItems.Where(p => p.Order.MarketplaceID == view.DbID);

                    var waitingShipments = orderItems.Where(p => p.Order.Status.Equals("Unshipped") || p.Order.Status.Equals("PartiallyShipped"));

                    view.WaitingShipmentCount = waitingShipments.Sum(p => p.QuantityOrdered - p.QuantityShipped);
                    view.WaitingShipment = waitingShipments.Select(p => p.Order.Code).ToList();

                    var waitingPayments = orderItems.Where(p => p.Order.Status.Equals("Pending"));

                    view.WaitingPaymentCount = waitingPayments.Sum(p => p.QuantityOrdered);
                    view.WaitingPayment = waitingPayments.Select(p => p.Order.Code).ToList();

                }
                else if (view.Host.Equals("Ebay"))
                {
                    view.ActiveListingQty = _dataContext.EbayListingItems.Where(p => p.Listing.MarketplaceID == view.DbID && p.Listing.Status.Equals("Active")).Sum(p => p.Quantity);

                    var orderItems = _dataContext.EbayOrderItems.Where(p => p.Order.MarketplaceID == view.DbID).ToList();

                    var waitingShipments = orderItems.Where(p => p.Order.IsWaitingForShipment());

                    view.WaitingShipmentCount = waitingShipments.Sum(p => p.QuantityPurchased);
                    view.WaitingShipment = waitingShipments.Select(p => p.Order.SalesRecordNumber).ToList();


                    var waitingPayments = orderItems.Where(p => p.Order.IsWaitingForPayment());

                    view.WaitingPaymentCount = waitingPayments.Sum(p => p.QuantityPurchased);
                    view.WaitingPayment = waitingPayments.Select(p => p.Order.SalesRecordNumber).ToList();

                } 
            }
        }

        private MarketplaceView CreateMarketplaceView(EbayMarketplace marketplace)
        {
            MarketplaceView view = new MarketplaceView();
            view.ID = "E-" + marketplace.ID;
            view.DbID = marketplace.ID;
            view.Host = "Ebay";
            view.ListingsLastSync = marketplace.ListingSyncTime.HasValue ? marketplace.ListingSyncTime.Value.ToLocalTime() : marketplace.ListingSyncTime;
            view.OrdersLastSync = marketplace.OrdersSyncTime.HasValue ? marketplace.OrdersSyncTime.Value.ToLocalTime() : marketplace.OrdersSyncTime;
            view.Name = marketplace.Name;

            return view; 
        }

        private MarketplaceView CreateMarketplaceView(AmznMarketplace marketplace)
        {
            MarketplaceView view = new MarketplaceView();
            view.ID = "A-" + marketplace.ID;
            view.DbID = marketplace.ID;
            view.Host = "Amazon";
            view.ListingsLastSync = marketplace.ListingSyncTime.HasValue ? marketplace.ListingSyncTime.Value.ToLocalTime() : marketplace.ListingSyncTime;
            view.OrdersLastSync = marketplace.OrderSyncTime.HasValue ? marketplace.OrderSyncTime.Value.ToLocalTime() : marketplace.OrderSyncTime;
            view.Name = marketplace.Name;

            return view;
        }

        private double GetTotalOH()
        {
            if (_totalOH == -1)
            {
                _totalOH = _dataContext.Items
                .Where(p => p.Quantity > 0)
                .ToList().Where(p =>
                    !p.DepartmentName.Equals("APPAREL") &&
                    !p.Inactive && !p.DepartmentName.Equals("ACCESSORIES") &&
                    !p.DepartmentName.Equals("MIXED ITEMS & LOTS")).Sum(p => p.Quantity); 
            }

            return _totalOH;
        }
    }
}
