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
            _readEntriesCommand = new ReadCommand(_workbook, _marketplaceCode, typeof(EbayEntry));
            _publishCommand = new EbayPublishCommand(_marketplaceCode);
            _updateCommand = new UpdateCommand(_workbook, _marketplaceCode);

            _readEntriesCommand.ReadCompleted += _publishCommand.ReadCompletedHandler;
            _readEntriesCommand.ReadCompleted += _updateCommand.ReadCompletedHandler;
            _updateCommand.FixCompleted += _readEntriesCommand.FixCompletedHandler;
            _updateCommand.FixCompleted += _publishCommand.FixCompletedHandler;
        }

    }

    public class EbayPublishCommand : PublishCommand
    {
        private BerkeleyEntities.Ebay.EbayServices _ebayServices = new BerkeleyEntities.Ebay.EbayServices();
        private string _marketplaceCode;
        private berkeleyEntities _dataContext;
        private EbayMarketplace _marketplace;


        public EbayPublishCommand(string marketplaceCode)
        {
            _marketplaceCode = marketplaceCode;
        }

        public override async void Publish(IEnumerable<ListingEntry> entries)
        {
            await Task.Run(() => PublishEntries(entries.Cast<EbayEntry>()));

            RaisePublishCompleted();
        }

        private void PublishEntries(IEnumerable<EbayEntry> entries)
        {
            var pendingEntries = entries.Cast<EbayEntry>();

            using (_dataContext = new berkeleyEntities())
            {
                _marketplace = _dataContext.EbayMarketplaces.Single(p => p.Code.Equals(_marketplaceCode));

                var update = entries.Where(p => !string.IsNullOrWhiteSpace(p.Code)).GroupBy(p => p.Code);

                foreach (var group in update)
                {
                    EbayListing listing = _marketplace.Listings.Single(p => p.Code.Equals(group.Key));

                    TryUpdateListing(listing, group);
                }

                var create = entries.Where(p => string.IsNullOrWhiteSpace(p.Code)).GroupBy(p => p.ClassName);

                foreach (var group in create)
                {
                    TryCreateListing(group);
                }

            }
        }

        private void TryCreateListing(IEnumerable<EbayEntry> entries)
        {
            try
            {
                entries.ToList().ForEach(p => p.Status = StatusCode.Processing);

                CreateListing(entries);

                foreach (var entry in entries)
                {
                    entry.Status = StatusCode.Completed;
                }
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

        private void TryUpdateListing(EbayListing listing, IEnumerable<EbayEntry> entries)
        {
            try
            {
                entries.ToList().ForEach(p => p.Status = StatusCode.Processing);

                UpdateListing(listing, entries);

                foreach (var entry in entries)
                {
                    entry.Command = string.Empty;

                    entry.Status = StatusCode.Completed;
                }
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

        private void UpdateListing(EbayListing listing, IEnumerable<EbayEntry> entries)
        {
            ListingDto listingDto = new ListingDto();

            listingDto.Code = listing.Code;
            listingDto.Format = entries.First(p => !string.IsNullOrWhiteSpace(p.Format)).GetFormat();
            listingDto.MarketplaceID = _marketplace.ID;
            listingDto.Brand = entries.First(p => !string.IsNullOrWhiteSpace(p.Brand)).Brand;
            listingDto.IsVariation = (bool)listing.IsVariation;

            if (entries.Any(p => !string.IsNullOrWhiteSpace(p.Template)))
            {
                listingDto.Template = entries.First(p => !string.IsNullOrWhiteSpace(p.Template)).Template;
            }

            if (entries.Any(p => !string.IsNullOrWhiteSpace(p.Design)))
            {
                listingDto.Design = entries.First(p => !string.IsNullOrWhiteSpace(p.Design)).Design;
            }

            if (entries.Any(p => !string.IsNullOrWhiteSpace(p.FullDescription)))
            {
                listingDto.FullDescription = entries.First(p => !string.IsNullOrWhiteSpace(p.FullDescription)).FullDescription;
            }

            foreach (EbayEntry entry in entries)
            {
                EbayListingItem listingItem = listing.ListingItems.SingleOrDefault(p => p.Item.ItemLookupCode.Equals(entry.Sku));

                ListingItemDto listingItemDto = new ListingItemDto();
                listingItemDto.Sku = entry.Sku;
                listingItemDto.Qty = entry.Q;
                listingItemDto.QtySpecified = entry.QSpecified;
                listingItemDto.Price = entry.P;
                listingItemDto.PriceSpecified = entry.PSpecified;

                if (entry.GetUpdateFlags().Any(p => p.Trim().ToUpper().Equals("TITLE")))
                {
                    listingItemDto.Title = entry.Title;
                }

                listingDto.Items.Add(listingItemDto);
            }

            var activeSkus = listing.ListingItems.Where(p => p.Quantity != 0).Select(p => p.Item.ItemLookupCode);

            bool mustIncludeProductData = listingDto.Items.Any(p => !activeSkus.Any(s => s.Equals(p.Sku)));

            foreach (EbayListingItem listingItem in listing.ListingItems.Where(p => p.Quantity != 0))
            {
                if (!listingDto.Items.Any(p => p.Sku.Equals(listingItem.Sku)))
                {
                    ListingItemDto listingItemDto = new ListingItemDto();
                    listingItemDto.Sku = listingItem.Sku;
                    listingItemDto.Qty = listingItem.Quantity;
                    listingItemDto.QtySpecified = true;
                    listingItemDto.Price = listingItem.Price;
                    listingItemDto.PriceSpecified = true;
                    listingItemDto.Title = listingItem.Title;

                    listingDto.Items.Add(listingItemDto);
                }
            }

            if (entries.Any(p => p.GetUpdateFlags().Any(s => s.Trim().ToUpper().Equals("TITLE"))))
            {
                if ((bool)listing.IsVariation)
                {
                    EbayEntry entry = entries.First(p => !string.IsNullOrWhiteSpace(p.Title));
                    listingDto.Title = GetParentTitle(entry);
                }
                else
                {
                    listingDto.Title = entries.First().Title;
                }
            }

            bool includeTemplate = entries.Any(p => p.GetUpdateFlags().Any(s => s.Trim().ToUpper().Equals("TEMPLATE")));

            bool includeProductData = entries.Any(p => p.GetUpdateFlags().Any(s => s.Trim().ToUpper().Equals("PRODUCTDATA"))) || mustIncludeProductData;

            if (listingDto.Items.All(p => p.Qty == 0))
            {
                _ebayServices.End(_marketplace.ID, listing.Code, "Publisher");
            }
            else
            {
                _ebayServices.Revise(listingDto, includeProductData, includeTemplate, "Publisher");
            }

            
        }

        private void CreateListing(IEnumerable<EbayEntry> entries)
        {
            ListingDto listingDto = new ListingDto();

            listingDto.MarketplaceID = _marketplace.ID;
            listingDto.Brand = entries.First(p => !string.IsNullOrWhiteSpace(p.Brand)).Brand;
            listingDto.Duration = entries.First(p => !string.IsNullOrWhiteSpace(p.Format)).GetDuration();
            listingDto.Format = entries.First(p => !string.IsNullOrWhiteSpace(p.Format)).GetFormat();
            listingDto.FullDescription = entries.First(p => !string.IsNullOrWhiteSpace(p.FullDescription)).FullDescription;

            if (entries.Any(p => !string.IsNullOrWhiteSpace(p.Template)))
            {
                listingDto.Template = entries.First(p => !string.IsNullOrWhiteSpace(p.Template)).Template;
            }

            if (entries.Any(p => !string.IsNullOrWhiteSpace(p.Design)))
            {
                listingDto.Design = entries.First(p => !string.IsNullOrWhiteSpace(p.Design)).Design;
            }

            if (entries.Any(p => p.StartDateSpecified))
            {
                listingDto.ScheduleTime = entries.First(p => p.StartDateSpecified).StartDate;
                listingDto.ScheduleTimeSpecified = true;
            }

            if (entries.Count() > 1)
            {
                listingDto.Sku = entries.First(p => !string.IsNullOrWhiteSpace(p.ClassName)).ClassName;

                EbayEntry entry = entries.First(p => !string.IsNullOrWhiteSpace(p.Title));

                listingDto.Title = GetParentTitle(entry); 
                listingDto.IsVariation = true;
            }
            else
            {
                listingDto.Sku = entries.First().Sku;
                listingDto.Title = entries.First().Title;
                listingDto.IsVariation = false;

                if (entries.First().BIN != 0 && listingDto.Format.Equals(EbayMarketplace.FORMAT_AUCTION))
                {
                    listingDto.BinPrice = entries.First().BIN;
                    listingDto.BinPriceSpecified = true;
                }
            }

            foreach (EbayEntry entry in entries)
            {
                ListingItemDto listingItem = new ListingItemDto();

                listingItem.Sku = entry.Sku;
                listingItem.Qty = entry.Q;
                listingItem.QtySpecified = true;
                listingItem.Price = entry.P;
                listingItem.PriceSpecified = true;
                listingItem.Title = entry.Title;

                listingDto.Items.Add(listingItem);
            }

            _ebayServices.Publish(listingDto, "Publisher");
        }

        private string GetParentTitle(EbayEntry entry)
        {
            string title = entry.Title;

            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                dataContext.MaterializeAttributes = true;

                Item item = dataContext.Items.Single(p => p.ItemLookupCode.Equals(entry.Sku));

                var wordsToRemove = item.Dimensions.Select(p => p.Key.ToString()).Concat(item.Dimensions.Select(p => p.Value.Value.ToString()));

                foreach (string word in wordsToRemove)
                {
                    title = title.Replace(" " + word + " ", " ");
                }

                title = title.Replace("Size", ""); 
            }

            return title;
        }
    }
}
