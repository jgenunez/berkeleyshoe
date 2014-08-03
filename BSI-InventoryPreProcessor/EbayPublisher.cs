using BerkeleyEntities;
using BerkeleyEntities.Ebay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BSI_InventoryPreProcessor
{
    public class EbayPublisher
    {
        private const string STATUS_ACTIVE = "Active";
        private const string FORMAT_FIXEDPRICE = "FixedPrice";
        private const string FORMAT_AUCTION = "Chinese";

        private EbayMarketplace _marketplace;
        private berkeleyEntities _dataContext;

        private PictureSetRepository _picSetRepository = new PictureSetRepository();


        public void Publish(int marketplaceID, IEnumerable<Entry> entries)
        {
            using (_dataContext = new berkeleyEntities())
            {
                _marketplace = _dataContext.EbayMarketplaces.Single(p => p.ID == marketplaceID);

                Publisher publisher = new Publisher(_dataContext, _marketplace);

                HandleFixedPrice(entries.Where(p => !p.IsAuction()));

                HandleAuctions(entries.Where(p => p.IsAuction()));

                publisher.SaveChanges();

            }
        }

        private void HandleFixedPrice(IEnumerable<Entry> entries)
        {
            var fixedPrice = _marketplace.Listings.Where(p => p.Status.Equals(STATUS_ACTIVE) && p.Format.Equals(FORMAT_FIXEDPRICE)).ToList();

            foreach (var group in entries.GroupBy(p => p.ClassName))
            {
                List<Entry> pending = new List<Entry>();

                foreach (Entry entry in group)
                {
                    if (fixedPrice.Any(p => p.Sku.Equals(entry.Sku)))
                    {
                        EbayListing listing = fixedPrice.Single(p => p.Sku.Equals(entry.Sku));
                        EbayListingItem listingItem = listing.ListingItems.First();
                        listingItem.Quantity = entry.Quantity;
                        listingItem.Price = entry.Price;
                    }
                    else
                    {
                        pending.Add(entry);
                    }
                }

                if (fixedPrice.Any(p => p.Sku.Equals(group.Key)) && pending.Count > 0)
                {
                    EbayListing listing = fixedPrice.Single(p => p.Sku.Equals(group.Key));
                    listing.Title = listing.Title;

                    foreach (Entry entry in pending)
                    {
                        Item item = _dataContext.Items.Single(p => p.ItemLookupCode.Equals(entry.Sku));
                        EbayListingItem listingItem = listing.ListingItems.SingleOrDefault(p => p.Item.ID == item.ID);

                        if (listingItem == null)
                        {
                            listingItem = new EbayListingItem() { Item = item, Listing = listing };
                        }

                        listingItem.Quantity = entry.Quantity;
                        listingItem.Price = entry.Price;
                    }
                }

                if (pending.Count() == 1)
                {
                    Entry entry = pending.First();

                    EbayListing listing = new EbayListing();
                    listing.Sku = group.Key;
                    listing.Marketplace = _marketplace;
                    listing.Condition = GetConditionID(entry.Condition);
                    listing.Duration = GetDuration(entry.Format);
                    listing.Format = FORMAT_FIXEDPRICE;
                    listing.FullDescription = entry.FullDescription;
                    listing.Title = entry.Title;
                    listing.IsVariation = false;

                    EbayListingItem listingItem = new EbayListingItem();
                    listingItem.Item = _dataContext.Items.Single(p => p.ItemLookupCode.Equals(entry.Sku));
                    listingItem.Listing = listing;
                    listingItem.Quantity = entry.Quantity;
                    listingItem.Price = entry.Price;

                    AssignPictures(listing);
                }
                else if (pending.Count() > 1)
                {
                    EbayListing listing = new EbayListing();
                    listing.Sku = group.Key;
                    listing.Marketplace = _marketplace;
                    listing.Condition = GetConditionID(pending.First(p => !string.IsNullOrWhiteSpace(p.Condition)).Condition);
                    listing.Duration = GetDuration(pending.First(p => !string.IsNullOrWhiteSpace(p.Format)).Format);
                    listing.Format = FORMAT_FIXEDPRICE;
                    listing.FullDescription = pending.First(p => !string.IsNullOrWhiteSpace(p.FullDescription)).FullDescription;
                    listing.Title = pending.First(p => !string.IsNullOrWhiteSpace(p.Title)).Title;
                    listing.IsVariation = true;

                    foreach (Entry entry in pending)
                    {
                        EbayListingItem listingItem = new EbayListingItem();
                        listingItem.Item = _dataContext.Items.Single(p => p.ItemLookupCode.Equals(entry.Sku));
                        listingItem.Listing = listing;
                        listingItem.Quantity = entry.Quantity;
                        listingItem.Price = entry.Price;
                    }

                    AssignPictures(listing);
                }
            }
        }

        private void HandleAuctions(IEnumerable<Entry> entries)
        {
            var auctions = _marketplace.Listings.Where(p => p.Status.Equals(STATUS_ACTIVE) && p.Format.Equals(FORMAT_AUCTION)).ToList();

            foreach (Entry entry in entries)
            {
                if (!auctions.Any(p => p.Sku.Equals(entry.Sku)))
                {
                    EbayListing listing = new EbayListing();
                    listing.Sku = entry.Sku;
                    listing.Marketplace = _marketplace;
                    listing.Condition = GetConditionID(entry.Condition);
                    listing.Duration = GetDuration(entry.Format);
                    listing.Format = FORMAT_AUCTION;
                    listing.FullDescription = entry.FullDescription;
                    listing.Title = entry.Title;
                    listing.IsVariation = false;

                    EbayListingItem listingItem = new EbayListingItem();
                    listingItem.Item = _dataContext.Items.Single(p => p.ItemLookupCode.Equals(entry.Sku));
                    listingItem.Listing = listing;
                    listingItem.Quantity = entry.Quantity;
                    listingItem.Price = entry.Price;


                    AssignPictures(listing);

                }
                else
                {
                    entry.IsValid = false;
                    entry.Result = "cannot modify auctions";
                }
            }

        }

        private void AssignPictures(EbayListing listing)
        {
            string brand = listing.ListingItems.First().Item.SubDescription1;
            var skus = listing.ListingItems.Select(p => p.Item.ItemLookupCode);

            var pics = _picSetRepository.GetPictures(brand, skus.ToList());

            foreach (PictureInfo picInfo in pics)
            {
                var urls = _dataContext.EbayPictureServiceUrls.Where(p => p.LocalName.Equals(picInfo.Name)).ToList();

                EbayPictureServiceUrl url = urls.SingleOrDefault(p => !p.IsExpired() && picInfo.LastModified > p.TimeUploaded);

                if (url == null)
                {
                    url = new EbayPictureServiceUrl();
                    url.LocalName = picInfo.Name;
                    url.Path = picInfo.Path;

                    new EbayPictureUrlRelation() { PictureServiceUrl = url, Listing = listing, CreatedTime = DateTime.UtcNow };
                }
                else
                {
                    if (!listing.Relations.Any(p => p.PictureServiceUrl.ID == url.ID))
                    {
                        new EbayPictureUrlRelation() { PictureServiceUrl = url, Listing = listing, CreatedTime = DateTime.UtcNow };
                    }
                }
            }
        }

        private string GetDuration(string format)
        {
            switch (format)
            {
                case "GTC": return format;

                case "BIN": return "Days_30";

                default: return null;
            }
        }

        private string GetConditionID(string condition)
        {
            switch(condition)
            {
                case "NEW" : return "1000";
                case "NWB": return "1500";
                case "PRE": return "1750";
                case "NWD": return "3000";

                default : return null;
            }
        }
    }
}
