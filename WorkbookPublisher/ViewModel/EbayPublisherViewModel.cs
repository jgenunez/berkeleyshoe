using BerkeleyEntities;
using BerkeleyEntities.Ebay;
using Microsoft.TeamFoundation.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
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
            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                EbayMarketplace marketplace = dataContext.EbayMarketplaces.Single(p => p.Code.Equals(_marketplaceCode));

                var pendingEntries = entries.Where(p => p.Status.Equals(StatusCode.Pending)).ToList();

                foreach (EbayEntry entry in pendingEntries)
                {
                    Item item = dataContext.Items.SingleOrDefault(p => p.ItemLookupCode.Equals(entry.Sku));

                    if (item == null)
                    {
                        entry.Message = "sku not found";
                        entry.Status = StatusCode.Error;
                        continue;
                    }

                    entry.Brand = item.SubDescription1;
                    entry.ClassName = item.ClassName;

                    string format = entry.GetFormat();

                    if (format == null)
                    {
                        entry.Message = "invalid format";
                        entry.Status = StatusCode.Error;
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
                            entry.Status = StatusCode.Completed;
                        }
                    }
                    else if (listingItems.Count() > 1)
                    {
                        entry.Message = "duplicate";
                        entry.Status = StatusCode.Error;
                    }

                    if (entry.Status.Equals(StatusCode.Pending))
                    {
                        if (format.Equals(Publisher.FORMAT_AUCTION) && entry.Q > 1)
                        {
                            entry.Message = "auction max qty is 1";
                            entry.Status = StatusCode.Error;
                        }
                        if (entry.Q > item.QtyAvailable)
                        {
                            entry.Message = "out of stock";
                            entry.Status = StatusCode.Error;
                        }
                        if (item.Department == null)
                        {
                            entry.Message = "department required";
                            entry.Status = StatusCode.Error;
                        }
                        if (entry.Title.Count() > 80)
                        {
                            entry.Message = "title max characters is 80";
                            entry.Status = StatusCode.Error;
                        }
                        if (entry.StartDateSpecified && entry.StartDate < DateTime.UtcNow)
                        {
                            entry.Message = "cannot schedule in the past";
                            entry.Status = StatusCode.Error;
                        }
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
        private PictureSetRepository _picSetRepository = new PictureSetRepository();
        private string _marketplaceCode;
        private berkeleyEntities _dataContext;
        private EbayMarketplace _marketplace;
        private Publisher _publisher;


        public EbayPublishCommand(string marketplaceCode)
        {
            _marketplaceCode = marketplaceCode;
        }

        public override async void Publish(IEnumerable<Entry> entries)
        {
            await Task.Run(() => PublishEntries(entries));

            RaisePublishCompleted();
        }

        private void PublishEntries(IEnumerable<Entry> entries)
        {
            var pendingEntries = entries.Cast<EbayEntry>();

            using (_dataContext = new berkeleyEntities())
            {
                _marketplace = _dataContext.EbayMarketplaces.Single(p => p.Code.Equals(_marketplaceCode));

                var auctions = pendingEntries.Where(p => p.IsAuction()).GroupBy(p => p.ClassName);

                var fixedPrice = pendingEntries.Where(p => !p.IsAuction()).GroupBy(p => p.ClassName);

                foreach (var group in auctions)
                {
                    HandleAuctionGroup(group);
                }

                foreach (var group in fixedPrice)
                {
                    HandleFixedPriceGroup(group);
                }
            }
        }

        private void HandleAuctionGroup(IGrouping<string, EbayEntry> group)
        {
            var auctions = _marketplace.Listings.Where(p => p.Format.Equals(Publisher.FORMAT_AUCTION)).Where(p => p.Status.Equals(Publisher.STATUS_ACTIVE));

            foreach (EbayEntry entry in group)
            {
                TryCreateListing(new EbayEntry[] { entry });
            }
        }

        private void HandleFixedPriceGroup(IGrouping<string,EbayEntry> group)
        {
            var fixedPrice = _marketplace.Listings.Where(p => p.Status.Equals(Publisher.STATUS_ACTIVE) && p.Format.Equals(Publisher.FORMAT_FIXEDPRICE));

            List<EbayEntry> pending = new List<EbayEntry>();

            foreach (EbayEntry entry in group)
            {
                if (fixedPrice.Any(p => p.Sku.Equals(entry.Sku)))
                {
                    string code = fixedPrice.Single(p => p.Sku.Equals(entry.Sku)).Code;    
          
                    TryUpdateListing(code, new EbayEntry[] { entry });
                }
                else
                {
                    pending.Add(entry);
                }
            }

            if (pending.Count > 0)
            {
                if (fixedPrice.Any(p => p.Sku.Equals(group.Key)))
                {
                    string code = fixedPrice.Single(p => p.Sku.Equals(group.Key)).Code;
                    TryUpdateListing(code, pending);
                }
                else
                {
                    TryCreateListing(pending);
                }
            }
        }

        private void TryCreateListing(IEnumerable<EbayEntry> entries)
        {
            try
            {
                entries.ToList().ForEach(p => p.Status = StatusCode.Processing);
                CreateListing(entries);
                entries.ToList().ForEach(p => p.Status = StatusCode.Completed);
            }
            catch (Exception e)
            {
                foreach (var entry in entries)
                {
                    entry.Status = StatusCode.Error;
                    entry.Message = e.Message;
                }
            }
        }

        private void TryUpdateListing(string code, IEnumerable<EbayEntry> entries)
        {
            try
            {
                entries.ToList().ForEach(p => p.Status = StatusCode.Processing);
                UpdateListing(code, entries);
                entries.ToList().ForEach(p => p.Status = StatusCode.Completed);
            }
            catch (Exception e)
            {
                foreach (var entry in entries)
                {
                    entry.Message = e.Message;
                    entry.Status = StatusCode.Error;
                }
            }
        }

        private void UpdateListing(string code, IEnumerable<EbayEntry> entries)
        {
            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                dataContext.MaterializeAttributes = true;

                EbayMarketplace marketplace = dataContext.EbayMarketplaces.Single(p => p.ID == _marketplace.ID);

                Publisher publisher = new Publisher(dataContext, marketplace);

                EbayListing listing = dataContext.EbayListings.Single(p => p.MarketplaceID == marketplace.ID && p.Code.Equals(code));

                if (entries.Any(p => !string.IsNullOrWhiteSpace(p.Title)))
                {
                    if ((bool)listing.IsVariation)
                    {
                        EbayEntry entry = entries.First(p => !string.IsNullOrWhiteSpace(p.Title));
                        listing.Title = GetParentTitle(dataContext.Items.Single(p => p.ItemLookupCode.Equals(entry.Sku)), entry);
                    }
                    else
                    {
                        listing.Title = entries.First(p => !string.IsNullOrWhiteSpace(p.Title)).Title;

                        if (entries.First().BIN != 0 && listing.Format.Equals(Publisher.FORMAT_AUCTION))
                        {
                            listing.BinPrice = entries.First().BIN;
                        }
                    }
                }

                if (entries.Any(p => !string.IsNullOrWhiteSpace(p.FullDescription)))
                {
                    listing.FullDescription = entries.First(p => !string.IsNullOrWhiteSpace(p.FullDescription)).FullDescription;
                }

                foreach (EbayEntry entry in entries)
                {
                    Item item = dataContext.Items.Single(p => p.ItemLookupCode.Equals(entry.Sku));

                    EbayListingItem listingItem = listing.ListingItems.SingleOrDefault(p => p.Item.ID == item.ID);

                    if (listingItem == null)
                    {
                        listingItem = new EbayListingItem() { Item = item, Listing = listing, Sku = entry.Sku };
                    }

                    listingItem.Quantity = entry.Q;

                    listingItem.Price = entry.P;

                    listingItem.Title = entry.Title;
                }

                publisher.ReviseListing(listing);

                dataContext.SaveChanges();
            }
        }

        private void CreateListing(IEnumerable<EbayEntry> entries)
        {
            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                dataContext.MaterializeAttributes = true;

                EbayMarketplace marketplace = dataContext.EbayMarketplaces.Single(p => p.ID == _marketplace.ID);

                Publisher publisher = new Publisher(dataContext, marketplace);

                var brand = entries.First(p => !string.IsNullOrWhiteSpace(p.Brand)).Brand;
                var pics = _picSetRepository.GetPictures(brand, new List<string>(entries.Select(p => p.Sku)));

                IEnumerable<EbayPictureServiceUrl> urlData = publisher.UploadToEPS(pics);

                dataContext.SaveChanges();

                EbayListing listing = new EbayListing();

                listing.Marketplace = marketplace;
                listing.Duration = entries.First(p => !string.IsNullOrWhiteSpace(p.Format)).GetDuration();
                listing.Format = entries.First(p => !string.IsNullOrWhiteSpace(p.Format)).GetFormat();
                listing.FullDescription = entries.First(p => !string.IsNullOrWhiteSpace(p.FullDescription)).FullDescription;

                if (entries.Any(p => p.StartDateSpecified))
                {
                    listing.StartTime = entries.First(p => p.StartDateSpecified).StartDate;
                }

                if (entries.Count() > 1)
                {
                    listing.Sku = entries.First(p => !string.IsNullOrWhiteSpace(p.ClassName)).ClassName;

                    EbayEntry entry = entries.First(p => !string.IsNullOrWhiteSpace(p.Title));
                    listing.Title = GetParentTitle(dataContext.Items.Single(p => p.ItemLookupCode.Equals(entry.Sku)), entry); ;
                    listing.IsVariation = true;
                }
                else
                {
                    listing.Sku = entries.First().Sku;
                    listing.Title = entries.First().Title;
                    listing.IsVariation = false;

                    if (entries.First().BIN != 0 && listing.Format.Equals(Publisher.FORMAT_AUCTION))
                    {
                        listing.BinPrice = entries.First().BIN;
                    }
                }

                foreach (var url in urlData)
                {
                    new EbayPictureUrlRelation() { PictureServiceUrl = url, Listing = listing, CreatedTime = DateTime.UtcNow };
                }

                foreach (EbayEntry entry in entries)
                {
                    EbayListingItem listingItem = new EbayListingItem();
                    listingItem.Item = dataContext.Items.Single(p => p.ItemLookupCode.Equals(entry.Sku));
                    listingItem.Sku = entry.Sku;
                    listingItem.Listing = listing;
                    listingItem.Quantity = entry.Q;
                    listingItem.Price = entry.P;
                    listingItem.Title = entry.Title;
                }

                publisher.PublishListing(listing);

                dataContext.SaveChanges();
            }

        }

        private string GetParentTitle(Item item, EbayEntry entry)
        {
            string title = entry.Title;

            var wordsToRemove = item.Attributes.Select(p => p.Key.ToString()).Concat(item.Attributes.Select(p => p.Value.Value.ToString()));

            foreach (string word in wordsToRemove)
            {
                title = title.Replace(" " + word + " ", " ");
            }

            title = title.Replace("Size", "");

            return title;
        }
    }
}
