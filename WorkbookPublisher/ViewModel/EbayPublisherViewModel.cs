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
            _updateCommand = new UpdateCommand(_workbook, _marketplaceCode, typeof(EbayEntry));

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

                foreach (var entryGroup in update)
                {
                    EbayListing listing = _marketplace.Listings.Single(p => p.Code.Equals(entryGroup.Key));

                    TryUpdateListing(listing, entryGroup);
                }

                var pendingCreate = entries.Where(p => string.IsNullOrWhiteSpace(p.Code));

                var individuals = pendingCreate.Where(p => !p.IsVariation());
                var variations = pendingCreate.Where(p => p.IsVariation()).GroupBy(p => p.ClassName);

                foreach (var entryGroup in variations)
                {
                    TryCreateListing(entryGroup, true);
                }

                foreach (var entry in individuals)
                {
                    TryCreateListing(new List<EbayEntry>() {entry}, false);
                }

            }
        }

        private void TryCreateListing(IEnumerable<EbayEntry> entries, bool isVariation)
        {
            try
            {
                entries.ToList().ForEach(p => p.Status = StatusCode.Processing);

                CreateListing(entries, isVariation);

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

            if (entries.Any(p => !string.IsNullOrWhiteSpace(p.Title)))
            {
                if ((bool)listing.IsVariation)
                {
                    listingDto.Title = GetParentTitle(entries.First(p => !string.IsNullOrWhiteSpace(p.Title)));
                }
                else
                {
                    listingDto.Title = entries.First(p => !string.IsNullOrWhiteSpace(p.Title)).Title;
                } 
            }

            foreach (EbayEntry entry in entries)
            {
                ListingItemDto listingItemDto = new ListingItemDto();

                listingItemDto.Sku = entry.Sku;

                if (entry.DisplayQty.HasValue)
                {
                    listingItemDto.AvailableQty = entry.Q;
                    listingItemDto.DisplayQty = entry.DisplayQty;
                    listingItemDto.Qty = entry.DisplayQty;
                }
                else
                {
                    listingItemDto.AvailableQty = null;
                    listingItemDto.DisplayQty = null;
                    listingItemDto.Qty = entry.Q;
                }

                listingItemDto.Price = entry.P;

                listingDto.Items.Add(listingItemDto);
            }

            var activeListingItems = listing.ListingItems.Where(p => p.Quantity != 0);

            foreach (EbayListingItem listingItem in activeListingItems)
            {
                if (!listingDto.Items.Any(p => p.Sku.Equals(listingItem.Sku)))
                {
                    ListingItemDto listingItemDto = new ListingItemDto();
                    listingItemDto.Sku = listingItem.Sku;
                    listingItemDto.Qty = listingItem.Quantity;
                    listingItemDto.DisplayQty = listingItem.DisplayQuantity;
                    listingItemDto.AvailableQty = listingItem.AvailableQuantity;
                    listingItemDto.Price = listingItem.Price;
                    listingDto.Items.Add(listingItemDto);
                }
            }

            var activeSkus = activeListingItems.Select(p => p.Item.ItemLookupCode);

            bool mustIncludeProductData = listingDto.Items.Any(p => !activeSkus.Any(s => s.Equals(p.Sku)));

            bool includeTemplate = entries.Any(p => p.GetUpdateFlags().Any(s => s.Trim().ToUpper().Equals("TEMPLATE")));

            bool includeProductData = entries.Any(p => p.GetUpdateFlags().Any(s => s.Trim().ToUpper().Equals("PRODUCTDATA"))) || mustIncludeProductData;

            if (listingDto.Items.All(p => p.Qty == 0))
            {
                _ebayServices.End(_marketplace.ID, listing.Code, "Publisher");
            }
            else
            {
                _ebayServices.Revise(_marketplace.ID, listingDto, includeProductData, includeTemplate, "Publisher");
            }

            
        }

        private void CreateListing(IEnumerable<EbayEntry> entries, bool isVariation)
        {
            ListingDto listingDto = new ListingDto();

            listingDto.Brand = entries.First(p => !string.IsNullOrWhiteSpace(p.Brand)).Brand;
            listingDto.Duration = entries.First(p => !string.IsNullOrWhiteSpace(p.Format)).GetDuration();
            listingDto.Format = entries.First(p => !string.IsNullOrWhiteSpace(p.Format)).GetFormat();
            listingDto.FullDescription = entries.First(p => !string.IsNullOrWhiteSpace(p.FullDescription)).FullDescription;
            listingDto.IsVariation = isVariation;
            

            if (isVariation)
            {
                listingDto.Title = GetParentTitle(entries.First(p => !string.IsNullOrWhiteSpace(p.Title)));
            }
            else
            {
                listingDto.Title = entries.First(p => !string.IsNullOrWhiteSpace(p.Title)).Title;
            }

            if (entries.Any(p => !string.IsNullOrWhiteSpace(p.Template)))
            {
                listingDto.Template = entries.First(p => !string.IsNullOrWhiteSpace(p.Template)).Template;
            }

            if (entries.Any(p => !string.IsNullOrWhiteSpace(p.Design)))
            {
                listingDto.Design = entries.First(p => !string.IsNullOrWhiteSpace(p.Design)).Design;
            }

            if (entries.Any(p => p.StartDate != null))
            {
                listingDto.ScheduleTime = entries.First(p => p.StartDate != null).StartDate;
            }



            if (isVariation)
            {
                listingDto.Sku = entries.First(p => !string.IsNullOrWhiteSpace(p.ClassName)).ClassName;
            }
            else
            {
                listingDto.Sku = entries.First().Sku;

                if (entries.First().BIN != null && listingDto.Format.Equals(EbayMarketplace.FORMAT_AUCTION))
                {
                    listingDto.BinPrice = entries.First().BIN;
                }
            }

            foreach (EbayEntry entry in entries)
            {
                ListingItemDto listingItem = new ListingItemDto();
                listingItem.Sku = entry.Sku;

                if (entry.DisplayQty.HasValue)
                {
                    listingItem.AvailableQty = entry.Q;
                    listingItem.Qty = entry.DisplayQty;
                    listingItem.DisplayQty = entry.DisplayQty;
                }
                else
                {
                    listingItem.Qty = entry.Q;
                }

                listingItem.Price = entry.P;

                listingDto.Items.Add(listingItem);
            }

            _ebayServices.Publish(_marketplace.ID, listingDto, "Publisher");
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
