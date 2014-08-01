using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BerkeleyEntities;
using EbayServices;


namespace MarketplacePublisher
{
    public class EbayPublisher
    {
        private berkeleyEntities _dataContext = new berkeleyEntities();
        private PictureSetRepository _pictureSetRepository = new PictureSetRepository();

        private ProductFactory _productFactory;

        private List<Entry> _entries;

        private Publisher _currentPublisher;
        private EbayMarketplace _currentMarketplace;


        public EbayPublisher(List<Entry> entries)
        {
            _entries = entries;
            _productFactory = new ProductFactory(_dataContext);
        }

        public void Publish()
        {
            foreach (Entry entry in _entries)
            {
                Item item = _dataContext.Items.Single(p => p.ItemLookupCode.Equals(entry.Sku));
                entry.ClassName = item.ClassName;
                entry.Brand = item.Brand;
            }

            var marketplaceGroups = _entries.GroupBy(p => p.GetMarketplaceName());

            foreach (var marketplaceGroup in marketplaceGroups)
            {
                _currentMarketplace  = _dataContext.EbayMarketplaces.Single(p => p.Name.Equals(marketplaceGroup.Key));
                _currentPublisher = new Publisher(_dataContext, _currentMarketplace);

                var variationEntries = marketplaceGroup.Where(p => p.IsVariation());
                HandleVariations(variationEntries);

                var individualEntries = marketplaceGroup.Where(p => !p.IsVariation());
                HandleIndividuals(individualEntries);
            }

        }

        private void HandleIndividuals(IEnumerable<Entry> entries)
        {
            foreach (Entry entry in entries)
            {
                if (entry.IsListed())
                {
                    EbayListing listing = UpdateListing(entry);
                    _currentPublisher.ReviseListing(listing);
                }
                else
                {
                    EbayListing listing = CreateListing(entry);
                    _currentPublisher.PublishListing(listing);
                }
            }
        }

        private void HandleVariations(IEnumerable<Entry> entries)
        {
            var entryGroups = entries.GroupBy(p => p.ClassName );

            foreach (var entryGroup in entryGroups)
            {
                if (entryGroup.Any(p => p.IsListed()))
                {
                    string listingID = entryGroup.First(p => p.IsListed()).ListingID;
                    EbayListing listing = UpdateVariationListing(listingID, entryGroup);
                    _currentPublisher.ReviseListing(listing);
                }
                else if (_dataContext.EbayListings.Any(p => p.MarketplaceID == _currentMarketplace.ID && p.Status.Equals("Active") && p.Sku.Equals(entryGroup.Key)))
                {
                    string listingID = _dataContext.EbayListings.Single(p =>
                        p.MarketplaceID == _currentMarketplace.ID &&
                        p.Sku.Equals(entryGroup.Key)).Code;

                    EbayListing listing = UpdateVariationListing(listingID, entryGroup);
                    _currentPublisher.ReviseListing(listing);
                }
                else
                {
                    EbayListing listing = CreateVariationListing(entryGroup);
                    _currentPublisher.PublishListing(listing);
                }
            }
        }

        private EbayListing CreateListing(Entry entry)
        {
            EbayListing listing = new EbayListing();
            listing.Marketplace = _currentMarketplace;
            listing.Title = entry.Title;
            listing.Condition = entry.GetConditionID();
            listing.Format = entry.GetFormat();
            listing.Duration = entry.GetDuration();
            listing.IsVariation = false;
            listing.FullDescription = entry.FullDescription;

            EbayListingItem listingItem = new EbayListingItem();
            listingItem.Listing = listing;
            listingItem.Item = _productFactory.GetProduct(entry.Sku);
            listingItem.Quantity = entry.Qty;
            listingItem.Price = entry.Price;

            PictureSet pictureSet = _pictureSetRepository.GetPictureSet(listingItem.Item.SubDescription1, listingItem.Item.ClassName);

            var picGroups = pictureSet.Pictures.GroupBy(p => p.VariationAttributeValue);

            if(picGroups.Any(p => !p.Key.Equals("N/A")))
            {
                var picGroup = picGroups.Single(p => listingItem.Item.Attributes.Values.Any(s => s.Value.Equals(p.Key)));
                AssignPictures(listing, picGroup.ToList());
            }
            else
            {
                AssignPictures(listing, picGroups.Single(p => p.Key.Equals("N/A")).ToList());
            }

            return listing;
        }

        private EbayListing UpdateListing(Entry entry)
        {
            EbayListing listing = _dataContext.EbayListings.Single(p => 
                p.MarketplaceID == _currentMarketplace.ID && 
                p.Code.Equals(entry.ListingID));

            listing.Title = entry.Title;
            listing.Condition = entry.GetConditionID();
            listing.Format = entry.GetFormat();
            listing.Duration = entry.GetDuration();

            EbayListingItem listingItem = listing.ListingItems.First();

            listingItem.Quantity = entry.Qty;
            listingItem.Price = entry.Price;

            return listing;
        }


        private EbayListing UpdateVariationListing(string listingID, IGrouping<string, Entry> entryGroup)
        {
            string title = entryGroup.Select(p => p.Title).First();
            string conditionID = entryGroup.Select(p => p.GetConditionID()).First();
            string format = entryGroup.Select(p => p.GetFormat()).First();
            string duration = entryGroup.Select(p => p.GetDuration()).First();

            EbayListing listing = _dataContext.EbayListings.Single(p => 
                p.MarketplaceID == _currentMarketplace.ID && 
                p.Code.Equals(listingID));

            listing.Title = title;
            listing.Condition = conditionID;
            listing.Format = format;
            listing.Duration = duration;

            foreach (Entry entry in entryGroup)
            {
                EbayListingItem listingItem = listing.ListingItems.SingleOrDefault(p => p.Item.ItemLookupCode.Equals(entry.Sku));

                if (listingItem == null)
                {
                    listingItem = new EbayListingItem();
                    listingItem.Item = _dataContext.Items.Single(p => p.ItemLookupCode.Equals(entry.Sku));
                    listingItem.Listing = listing;
                }

                listingItem.Quantity = entry.Qty;
                listingItem.Price = entry.Price;
            }

            return listing;
        }

        private EbayListing CreateVariationListing(IGrouping<string, Entry> entryGroup)
        {
            string title = entryGroup.Select(p => p.Title).First();
            string conditionID = entryGroup.Select(p => p.GetConditionID()).First();
            string format = entryGroup.Select(p => p.GetFormat()).First();
            string duration = entryGroup.Select(p => p.GetDuration()).First();
            string fullDescription = entryGroup.Select(p => p.FullDescription).First();

            EbayListing listing = new EbayListing();
            listing.Marketplace = _currentMarketplace;
            listing.Sku = entryGroup.Key;
            listing.Title = title;
            listing.Condition = conditionID;
            listing.Format = format;
            listing.Duration = duration;
            listing.IsVariation = true;
            listing.FullDescription = fullDescription;

            foreach (Entry entry in entryGroup)
            {
                EbayListingItem listingItem = new EbayListingItem();
                listingItem.Item = _productFactory.GetProduct(entry.Sku);
                listingItem.Listing = listing;
                listingItem.Quantity = entry.Qty;
                listingItem.Price = entry.Price;
            }

            PictureSet pictureSet = _pictureSetRepository
                .GetPictureSet(_dataContext.ItemClasses.Single(p => p.ItemLookupCode.Equals(entryGroup.Key)).SubDescription1, entryGroup.Key);

            var attributes = listing.ListingItems.SelectMany(p => p.Item.Attributes).Select(p => p.Value.Code);

            var pics = pictureSet.Pictures.Where(p => p.VariationAttributeValue.Equals("N/A") || attributes.Any(d => d.Equals(p.VariationAttributeValue)));

            AssignPictures(listing, pics.ToList());


            return listing;
        }

        private void AssignPictures(EbayListing listing, List<PictureInfo> pics)
        {
            foreach(PictureInfo picInfo in pics)
            {
                var urls = _dataContext.EbayPictureServiceUrls.Where(p => p.LocalName.Equals(picInfo.Name)).ToList();

                EbayPictureServiceUrl url = urls.SingleOrDefault(p => !p.IsExpired() && picInfo.LastModified > p.TimeUploaded);

                if(url == null)
                {
                    url = new EbayPictureServiceUrl();
                    url.LocalName = picInfo.Name;
                    url.VariationAttributeValue = picInfo.VariationAttributeValue;
                    url.Path = picInfo.Path;

                    new EbayPictureUrlRelation() { PictureServiceUrl = url, Listing = listing, CreatedTime = DateTime.UtcNow };
                }
                else
                {
                    if(!listing.Relations.Any(p => p.PictureServiceUrl.ID == url.ID))
                    {
                        new EbayPictureUrlRelation() { PictureServiceUrl = url, Listing = listing, CreatedTime = DateTime.UtcNow };
                    }
                }
            }
        }

        //private void UploadLocalPictures()
        //{
        //    var entryGroups = _entries.GroupBy(p => new { PictureSet = p.ClassName, Brand = p.Brand } );

        //    foreach (var entryGroup in entryGroups)
        //    {
        //        try
        //        {
        //            _pictureService.UploadPictureSet(entryGroup.Key.Brand, entryGroup.Key.PictureSet);
        //        }
        //        catch (Exception)
        //        {
        //            entryGroup.ToList().ForEach(p => p.Status = "error uploading images");
        //        }
        //    }
        //}  
    }
}
