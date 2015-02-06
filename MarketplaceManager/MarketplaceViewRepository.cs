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
                    var activeListings = _dataContext.AmznListingItems.Where(p => p.MarketplaceID == view.DbID && p.IsActive);

                    view.ActiveListingQty = activeListings.Sum(p => p.Quantity);
                    view.ActiveListing = activeListings.Count();

                    var orders = _dataContext.AmznOrders.Where(p => p.MarketplaceID == view.DbID);

                    var unshippedOrders = orders.Where(p => p.Status.Equals("Unshipped") || p.Status.Equals("PartiallyShipped"));
                    var unpaidOrders = orders.Where(p => p.Status.Equals("Pending"));

                    if (unshippedOrders.Count() > 0)
                    {
                        view.WaitingShipmentQty = unshippedOrders.Sum(p => p.OrderItems.Sum(s => s.QuantityOrdered - s.QuantityShipped));
                        view.WaitingShipment = unshippedOrders.Select(p => p.Code).ToList();
                    }


                    if (unpaidOrders.Count() > 0)
                    {
                        view.WaitingPaymentQty = unpaidOrders.Sum(p => p.OrderItems.Sum(s => s.QuantityOrdered - s.QuantityShipped));
                        view.WaitingPayment = unpaidOrders.Select(p => p.Code).ToList();
                    }

                }
                else if (view.Host.Equals("Ebay"))
                {
                    var activeListings = _dataContext.EbayListings.Where(p => p.MarketplaceID == view.DbID && p.Status.Equals(EbayMarketplace.STATUS_ACTIVE));

                    view.ActiveListingQty = activeListings.Sum(p => p.ListingItems.Sum(s => s.Quantity));
                    view.ActiveListing = activeListings.Count();


                    var orders = _dataContext.EbayOrders.Where(p => p.MarketplaceID == view.DbID).ToList();

                    var unshippedOrders = orders.Where(p => p.IsWaitingForShipment());
                    var unpaidOrders = orders.Where(p => p.IsWaitingForPayment());



                    view.WaitingShipmentQty = unshippedOrders.Sum(p => p.OrderItems.Sum(s => s.QuantityPurchased));
                    view.WaitingShipment = unshippedOrders.Select(p => p.SalesRecordNumber).ToList();

                    view.WaitingPaymentQty = unpaidOrders.Sum(p => p.OrderItems.Sum(s => s.QuantityPurchased));
                    view.WaitingPayment = unpaidOrders.Select(p => p.SalesRecordNumber).ToList();

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
