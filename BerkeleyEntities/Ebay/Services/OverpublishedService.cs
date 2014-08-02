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

namespace EbayServices
{
    public class OverpublishedService
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private ListingSyncService _listingSyncService;
        private berkeleyEntities _dataContext = new berkeleyEntities();
        private List<ItemType> _listingDtos = new List<ItemType>();
        private Publisher _publisher;
        private EbayMarketplace _marketplace;
        

        public OverpublishedService(int marketplaceID)
        {
            _marketplace = _dataContext.EbayMarketplaces.Single(p => p.ID == marketplaceID);

            _listingSyncService = new ListingSyncService(_marketplace.ID);

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

            var activeListings = _dataContext.EbayListings
                .Include("ListingItems.OrderItems")
                .Include("ListingItems.Item.AmznListingItems.OrderItems")
                .Where(p => p.MarketplaceID == _marketplace.ID && p.Status.Equals("Active")).ToList();

            foreach (EbayListing listing in activeListings)
            {
                bool hasOverpublished = listing.ListingItems.Any(p => p.Quantity > p.Item.QtyAvailable);

                if (hasOverpublished)
                {
                    foreach (EbayListingItem listingItem in listing.ListingItems)
                    {
                        if (listingItem.Quantity > listingItem.Item.QtyAvailable)
                        {
                            listingItem.Quantity = listingItem.Item.QtyAvailable;
                        }
                    }

                    try
                    {
                        if (listing.ListingItems.All(p => p.Quantity == 0))
                        {
                            _publisher.EndListing(listing.Code);                
                        }
                        else
                        {
                            _publisher.ReviseListing(listing);
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.Error(string.Format("Listing ( {1} | {0} ) modification or ending failed: {2}", listing.Code, _marketplace.Code , e.Message));
                        _listingSyncService.SyncListings(new StringCollection { listing.Code }, DateTime.UtcNow);
                    }
                }
            }

            _dataContext.Dispose();
        }

        //private void DetermineOverpublished()
        //{
        //    var activeListings = _dataContext.EbayListings
        //        .Include("ListingItems.OrderItems")
        //        .Include("ListingItems.Item.AmznListingItems.OrderItems")
        //        .Where(p => p.MarketplaceID == _marketplace.ID && p.Status.Equals("Active")).ToList();
            

        //    foreach (EbayListing listing in activeListings)
        //    {
        //        bool hasOverpublished = listing.ListingItems.Any(p => p.Quantity > p.Item.QtyAvailable);

        //        if (hasOverpublished)
        //        {
        //            if ((bool)listing.IsVariation)
        //            {
        //                _listingDtos.Add(DetermineVariationOverpublished(listing));
        //            }
        //            else
        //            {
        //                ItemType listingDto = new ItemType();
        //                listingDto.ItemID = listing.Code;

        //                EbayListingItem listingItem = listing.ListingItems.First();

        //                listingDto.QuantitySpecified = true;
        //                listingDto.Quantity = listingItem.Item.QtyAvailable;

        //                _listingDtos.Add(listingDto);
        //            }
        //        }
        //    }
        //}

        //private void UpdateIndividual(ItemType listingDto)
        //{
        //    if (listingDto.Quantity == 0)
        //    {
        //        _publisher.EndListing(listingDto.ItemID);
        //    }
        //    else
        //    {
        //        _publisher.ReviseListingDeprecated(listingDto);
        //    }
        //}

        //private void UpdateVariation(ItemType listingDto)
        //{
        //    if (listingDto.Variations.Variation.ToArray().All(p => p.Quantity == 0))
        //    {
        //        _publisher.EndListing(listingDto.ItemID);
        //    }
        //    else
        //    {
        //        _publisher.ReviseListingDeprecated(listingDto);
        //    }
        //}

        //private ItemType DetermineVariationOverpublished(EbayListing listing)
        //{
        //    ItemType listingDto = new ItemType();
        //    listingDto.ItemID = listing.Code;
        //    listingDto.Variations = new VariationsType();
        //    listingDto.Variations.Variation = new VariationTypeCollection();

        //    foreach (EbayListingItem listingItem in listing.ListingItems)
        //    {
        //        VariationType variationDto = new VariationType();
        //        variationDto.SKU = listingItem.Item.ItemLookupCode;
        //        variationDto.QuantitySpecified = true;

        //        if (listingItem.Quantity > listingItem.Item.QtyAvailable)
        //        {
        //            variationDto.Quantity = listingItem.Item.QtyAvailable;
        //        }
        //        else
        //        {
        //            variationDto.Quantity = listingItem.Quantity;
        //        }

        //        listingDto.Variations.Variation.Add(variationDto);
        //    }

        //    return listingDto;
        //}


    }
}
