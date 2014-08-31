using BerkeleyEntities;
using BerkeleyEntities.Ebay;
using Microsoft.TeamFoundation.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace WorkbookPublisher.ViewModel
{

    public class EbayPublisherViewModel : PublisherViewModel
    {

        public EbayPublisherViewModel(ExcelWorkbook workbook, string marketplaceCode) : base(workbook, marketplaceCode)
        {
            _readEntriesCommand = new EbayReadCommand(_workbook, _marketplaceCode);
            _publishCommand = new EbayPublishCommand(_marketplaceCode);
            _fixErrorsCommand = new FixCommand(_workbook, _marketplaceCode);

            _readEntriesCommand.ReadCompleted += _publishCommand.ReadCompletedHandler;
            _publishCommand.PublishCompleted += _fixErrorsCommand.PublishCompletedHandler;
            _fixErrorsCommand.FixCompleted += _readEntriesCommand.FixCompletedHandler;
        }

    }

    public class EbayReadCommand : ReadCommand
    {
        public EbayReadCommand(ExcelWorkbook workbook, string marketplaceCode)
            : base(workbook, marketplaceCode)
        {
 
        }

        public override void UpdateEntries(IEnumerable<Entry> entries)
        {
            EbayMarketplace marketplace = _dataContext.EbayMarketplaces.Single(p => p.Code.Equals(_marketplaceCode));

            foreach (EbayEntry entry in entries)
            {
                Item item = _dataContext.Items.SingleOrDefault(p => p.ItemLookupCode.Equals(entry.Sku));

                if (item == null)
                {
                    entry.Message = "sku not found";
                    continue;
                }

                entry.Brand = item.SubDescription1;
                entry.ClassName = item.ClassName;

                string format = entry.GetFormat();

                if (format == null)
                {
                    entry.Message = "invalid format";
                    continue;
                }

                var listingItems = item.EbayListingItems.Where(p =>
                    p.Listing.MarketplaceID == marketplace.ID &&
                    p.Listing.Status.Equals(Publisher.STATUS_ACTIVE) &&
                    p.Listing.Format.Equals(format));


                if (listingItems.Count() == 1)
                {
                    EbayListingItem listingItem = listingItems.First();

                    if (listingItem.Quantity == entry.Q)
                    {
                        entry.Completed = true;
                    }
                }
                else if (listingItems.Count() > 1)
                {
                    entry.Message = "duplicate";
                }

                if (entry.Status.Equals("waiting"))
                {
                    if (format.Equals(Publisher.FORMAT_AUCTION) && entry.Q > 1)
                    {
                        entry.Message = "auction max qty is 1";
                    }
                    if (entry.Q > item.QtyAvailable)
                    {
                        entry.Message = "out of stock";
                    }
                    if (item.Department == null)
                    {
                        entry.Message = "department required";
                    }
                    if (entry.Title.Count() > 80)
                    {
                        entry.Message = "title max characters is 80";
                    }
                }

            }
        }

        public override Type EntryType
        {
            get { return typeof(EbayEntry); }
        }
    }

    public class EbayPublishCommand : PublishCommand
    {
        private berkeleyEntities _dataContext = new berkeleyEntities();
        private Dictionary<EbayListing, List<EbayEntry>> _targetListings = new Dictionary<EbayListing, List<EbayEntry>>();
        private PictureSetRepository _picSetRepository = new PictureSetRepository();
        private EbayMarketplace _marketplace;
        private Publisher _publisher;


        public EbayPublishCommand(string marketplaceCode)
        {
            _dataContext.MaterializeAttributes = true;

            _marketplace = _dataContext.EbayMarketplaces.Single(p => p.Code.Equals(marketplaceCode));

            _publisher = new Publisher(_dataContext, _marketplace);

            _publisher.Result += Publisher_Result;
        }

        public override async void Publish(IEnumerable<Entry> entries)
        {
            var pendingEntries = entries.Cast<EbayEntry>();

            await Task.Run(() => HandleFixedPriceEntries(pendingEntries.Where(p => !p.IsAuction())));

            await Task.Run(() => HandleAuctionEntries(pendingEntries.Where(p => p.IsAuction())));

            await Task.Run(() => _publisher.Publish());
            
        }

        private void Publisher_Result(ResultArgs e)
        {
            foreach (EbayEntry entry in _targetListings[e.Listing])
            {
                if (e.IsError)
                {
                    entry.Message = e.Message;
                }
                else
                {
                    entry.Completed = true;
                }
            }
        }

        private void HandleAuctionEntries(IEnumerable<EbayEntry> entries)
        {
            var auctions = _marketplace.Listings.Where(p => p.Format.Equals(Publisher.FORMAT_AUCTION)).Where(p => p.Status.Equals(Publisher.STATUS_ACTIVE)).ToList();

            foreach (EbayEntry entry in entries)
            {
                if (!auctions.Any(p => p.Sku.Equals(entry.Sku)))
                {
                    CreateAuction(entry);
                }
                else
                {
                    entry.Message = "cannot modify auctions";
                }
            }

        }

        private void CreateAuction(EbayEntry entry)
        {
            EbayListing listing = new EbayListing();
            listing.Sku = entry.Sku;
            listing.Marketplace = _marketplace;
            listing.Condition = entry.GetConditionID();
            listing.Duration = entry.GetDuration();
            listing.Format = Publisher.FORMAT_AUCTION;
            listing.FullDescription = entry.FullDescription;
            listing.Title = entry.Title;
            listing.IsVariation = false;

            if (entry.StartDateSpecified)
            {
                listing.StartTime = entry.StartDate;
            }

            EbayListingItem listingItem = new EbayListingItem();
            listingItem.Item = _dataContext.Items.Single(p => p.ItemLookupCode.Equals(entry.Sku));
            listingItem.Sku = entry.Sku;
            listingItem.Listing = listing;
            listingItem.Quantity = entry.Q;
            listingItem.Price = entry.P;

            _targetListings.Add(listing, new List<EbayEntry>() { entry });

            try
            {
                AssignPictures(listing);
            }
            catch (FileNotFoundException e)
            {
                _publisher.Revert(listing);
                entry.Message = e.Message;
            }
        }

        private void HandleFixedPriceEntries(IEnumerable<EbayEntry> entries)
        {
            var fixedPrice = _marketplace.Listings.Where(p => p.Status.Equals(Publisher.STATUS_ACTIVE) && p.Format.Equals(Publisher.FORMAT_FIXEDPRICE)).ToList();

            foreach (var group in entries.GroupBy(p => p.ClassName))
            {
                List<EbayEntry> pending = new List<EbayEntry>();

                foreach (EbayEntry entry in group)
                {
                    if (fixedPrice.Any(p => p.Sku.Equals(entry.Sku)))
                    {
                        UpdateIndividualListing(fixedPrice.Single(p => p.Sku.Equals(entry.Sku)), entry);
                    }
                    else
                    {
                        pending.Add(entry);
                    }
                }

                if (pending.Count > 0 && fixedPrice.Any(p => p.Sku.Equals(group.Key)))
                {
                    UpdateVariationListing(fixedPrice.Single(p => p.Sku.Equals(group.Key)), pending);
                }
                else if (pending.Count() == 1)
                {
                    CreateIndividualListing(pending.First());
                }
                else if (pending.Count() > 1)
                {
                    CreateVariationListing(pending, group.Key);
                }
            }
        }

        private void UpdateIndividualListing(EbayListing listing, EbayEntry entry)
        {
            EbayListingItem listingItem = listing.ListingItems.First();

            listingItem.Quantity = entry.Q;
            listingItem.Price = entry.P;

            if (!string.IsNullOrWhiteSpace(entry.Title))
            {
                listing.Title = entry.Title;
            }
            if (!string.IsNullOrWhiteSpace(entry.FullDescription))
            {
                listing.FullDescription = entry.FullDescription;
            }

            _targetListings.Add(listing, new List<EbayEntry>() { entry });
        }

        private void CreateIndividualListing(EbayEntry entry)
        {
            EbayListing listing = new EbayListing();
            listing.Sku = entry.Sku;
            listing.Marketplace = _marketplace;
            listing.Condition = entry.GetConditionID();
            listing.Duration = entry.GetDuration();
            listing.Format = Publisher.FORMAT_FIXEDPRICE;
            listing.FullDescription = entry.FullDescription;
            listing.Title = entry.Title;
            listing.IsVariation = false;

            EbayListingItem listingItem = new EbayListingItem();
            listingItem.Sku = entry.Sku;
            listingItem.Item = _dataContext.Items.Single(p => p.ItemLookupCode.Equals(entry.Sku));
            listingItem.Listing = listing;
            listingItem.Quantity = entry.Q;
            listingItem.Price = entry.P;

            try
            {
                AssignPictures(listing);
            }
            catch (FileNotFoundException e)
            {
                entry.Message = e.Message;
            }

            if (listing.Relations.Count < 1)
            {
                entry.Message = "no picture found";
                _publisher.Revert(listing);
            }

            _targetListings.Add(listing, new List<EbayEntry>() { entry });
        }

        private void UpdateVariationListing(EbayListing listing, IEnumerable<EbayEntry> entries)
        {
            if (entries.Any(p => !string.IsNullOrWhiteSpace(p.Title)))
            {
                listing.Title = GetParentTitle(entries.First(p => !string.IsNullOrWhiteSpace(p.Title)));
            }
            if (entries.Any(p => !string.IsNullOrWhiteSpace(p.FullDescription)))
            {
                listing.FullDescription = entries.First(p => !string.IsNullOrWhiteSpace(p.FullDescription)).FullDescription;
            }

            listing.Title = listing.Title;

            foreach (EbayEntry entry in entries)
            {
                Item item = _dataContext.Items.Single(p => p.ItemLookupCode.Equals(entry.Sku));

                EbayListingItem listingItem = listing.ListingItems.SingleOrDefault(p => p.Item.ID == item.ID);

                if (listingItem == null)
                {
                    listingItem = new EbayListingItem() { Item = item, Listing = listing };
                }

                listingItem.Quantity = entry.Q;
                listingItem.Price = entry.P;
            }

            _targetListings.Add(listing, entries.ToList());
        }

        private void CreateVariationListing(IEnumerable<EbayEntry> entries, string className)
        {
            EbayListing listing = new EbayListing();
            listing.Sku = className;
            listing.Marketplace = _marketplace;
            listing.Condition = entries.First(p => !string.IsNullOrWhiteSpace(p.Condition)).GetConditionID();
            listing.Duration = entries.First(p => !string.IsNullOrWhiteSpace(p.Format)).GetDuration();
            listing.Format = Publisher.FORMAT_FIXEDPRICE;
            listing.FullDescription = entries.First(p => !string.IsNullOrWhiteSpace(p.FullDescription)).FullDescription;
            listing.Title = GetParentTitle(entries.First(p => !string.IsNullOrWhiteSpace(p.Title)));
            listing.IsVariation = true;

            foreach (EbayEntry entry in entries)
            {
                EbayListingItem listingItem = new EbayListingItem();
                listingItem.Item = _dataContext.Items.Single(p => p.ItemLookupCode.Equals(entry.Sku));
                listingItem.Sku = entry.Sku;
                listingItem.Listing = listing;
                listingItem.Quantity = entry.Q;
                listingItem.Price = entry.P;
            }

            try
            {
                AssignPictures(listing);
            }
            catch (FileNotFoundException e)
            {
                entries.ToList().ForEach(p => p.Message = e.Message);
            }

            if (listing.Relations.Count < 1)
            {
                entries.ToList().ForEach(p => p.Message = "no picture found");
                _publisher.Revert(listing);
            }

            _targetListings.Add(listing, entries.ToList());
        }

        private void AssignPictures(EbayListing listing)
        {
            string brand = listing.ListingItems.First().Item.SubDescription1;
            var skus = listing.ListingItems.Select(p => p.Item.ItemLookupCode);

            var pics = _picSetRepository.GetPictures(brand, skus.ToList());

            foreach (PictureInfo picInfo in pics)
            {
                var urls = _dataContext.EbayPictureServiceUrls.Where(p => p.LocalName.Equals(picInfo.Name)).ToList();

                EbayPictureServiceUrl url = urls.SingleOrDefault(p => !p.IsExpired() && picInfo.LastModified < p.TimeUploaded);

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

        private string GetParentTitle(EbayEntry entry)
        {
            string title = entry.Title;

            Item item = _dataContext.Items.Single(p => p.ItemLookupCode.Equals(entry.Sku));

            var wordsToRemove = item.Attributes.Select(p => p.Key).Concat(item.Attributes.Select(p => p.Value.Value));

            foreach (string word in wordsToRemove)
            {
                title = title.Replace(" " + word + " ", " ");
            }

            return title;
        }
    }
}
