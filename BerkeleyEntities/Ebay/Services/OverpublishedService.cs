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
using eBay.Service.Core.Sdk;

namespace BerkeleyEntities.Ebay.Services
{
    public class OverpublishedService
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private berkeleyEntities _dataContext = new berkeleyEntities();
        private EbayMarketplace _marketplace;
        

        public OverpublishedService(int marketplaceID)
        {
            _marketplace = _dataContext.EbayMarketplaces.Single(p => p.ID == marketplaceID);
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

            var listings = _dataContext.EbayListings.Where(p => p.Status.Equals(Publisher.STATUS_ACTIVE) && p.MarketplaceID == _marketplace.ID).ToList();

            foreach (EbayListing listing in listings)
            {
                foreach (EbayListingItem listingItem in listing.ListingItems)
                {
                    if (listingItem.Item != null)
                    {
                        if (listingItem.Quantity > listingItem.Item.QtyAvailable)
                        {
                            listingItem.Quantity = listingItem.Item.QtyAvailable;
                        }
                    }
                }

                if(listing.ListingItems.Any(p => p.EntityState.Equals(EntityState.Modified)))
                {
                    ReviseOrEndListing(listing);
                }
            }

            
        }

        public void ReviseOrEndListing(EbayListing targetListing)
        {
            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                EbayMarketplace marketplace = dataContext.EbayMarketplaces.Single(P => P.ID == _marketplace.ID);

                Publisher publisher = new Publisher(dataContext, marketplace);

                EbayListing listing = dataContext.EbayListings.Single(p => p.ID == targetListing.ID);

                listing.Status = targetListing.Status;

                foreach (var targetListingItem in targetListing.ListingItems.Where(p => p.EntityState.Equals(EntityState.Modified)))
                {
                    var listingItem = listing.ListingItems.Single(p => p.ID == targetListingItem.ID);

                    listingItem.Quantity = targetListingItem.Quantity;
                }

                try
                {
                    if (listing.ListingItems.All(p => p.Quantity == 0))
                    {
                        publisher.EndListing(listing);
                    }
                    else
                    {
                        publisher.ReviseListing(listing);
                    }

                    dataContext.SaveChanges();
                }
                catch (ApiException e)
                {
                    if (e.Errors.ToArray().Any(p => p.ErrorCode.Equals("17")))
                    {
                        listing.Status = Publisher.STATUS_DELETED;
                        dataContext.SaveChanges();
                    }

                    _logger.Error(e.ToString());
                }
            }
        }
    }
}
