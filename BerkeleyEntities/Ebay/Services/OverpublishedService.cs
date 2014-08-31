using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BerkeleyEntities;
using eBay.Service.Core.Soap;
using System.Data.Objects;
using System.Data;
using NLog;
using EbayServices.Services;

namespace BerkeleyEntities.Ebay.Services
{
    public class OverpublishedService
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private berkeleyEntities _dataContext = new berkeleyEntities();
        private Publisher _publisher;
        private EbayMarketplace _marketplace;
        

        public OverpublishedService(int marketplaceID)
        {
            _marketplace = _dataContext.EbayMarketplaces.Single(p => p.ID == marketplaceID);
            _publisher = new Publisher(_dataContext, _marketplace);
        }

        public void BalanceQuantities()
        {
            if (!_marketplace.ListingSyncTime.HasValue || _marketplace.ListingSyncTime.Value < DateTime.UtcNow.AddHours(-1))
            {
                throw new InvalidOperationException(_marketplace.Name + " listings must be synchronized in order to fix overpublished");
            }

            if (!_marketplace.OrdersSyncTime.HasValue || _marketplace.OrdersSyncTime.Value < DateTime.UtcNow.AddHours(-1))
            {
                throw new InvalidOperationException(_marketplace.Name + " orders must be synchronized in order to fix overpublished");
            }

            var listings = _dataContext.EbayListings.Where(p => p.Status.Equals(Publisher.STATUS_ACTIVE) && p.MarketplaceID == _marketplace.ID);

            foreach (EbayListing listing in listings)
            {
                foreach (EbayListingItem listingItem in listing.ListingItems)
                {
                    if (listingItem.Item != null)
                    {
                        if (listingItem.Quantity > listingItem.Item.QtyAvailable)
                        {
                            listing.Status = listing.Status;
                            listingItem.Quantity = listingItem.Item.QtyAvailable;
                        }
                    }
                }
            }

            _publisher.Publish();
        }
    }
}
