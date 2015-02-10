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
            _updateCommand = new UpdateCommand(_workbook, _marketplaceCode);

            _readEntriesCommand.ReadCompleted += _publishCommand.ReadCompletedHandler;
            _readEntriesCommand.ReadCompleted += _updateCommand.ReadCompletedHandler;
            _updateCommand.FixCompleted += _readEntriesCommand.FixCompletedHandler;
            _updateCommand.FixCompleted += _publishCommand.FixCompletedHandler;
        }

    }

    public class EbayReadCommand : ReadCommand
    {
        private EbayMarketplace _marketplace;
        private uint _lastRowIndex;
        private List<ListingEntry> _newEntries;

        public EbayReadCommand(ExcelWorkbook workbook, string marketplaceCode)
            : base(workbook, marketplaceCode)
        {
 
        }

        //public override void UpdateAndValidateEntries(IEnumerable<ListingEntry> entries)
        //{
        //    using (berkeleyEntities dataContext = new berkeleyEntities())
        //    {
        //        EbayMarketplace marketplace = dataContext.EbayMarketplaces.Single(p => p.Code.Equals(_marketplaceCode));

        //        var pendingEntries = entries.Where(p => p.Progress.Equals(StatusCode.Pending)).Cast<EbayEntry>().ToList();

        //        foreach (EbayEntry entry in pendingEntries)
        //        {
        //            Item item = dataContext.Items.SingleOrDefault(p => p.ItemLookupCode.Equals(entry.Sku));

        //            if (item == null)
        //            {
        //                entry.Message = "sku not found";
        //                entry.Progress = StatusCode.Error;
        //                continue;
        //            }

        //            entry.Brand = item.SubDescription1;
        //            entry.ClassName = item.ClassName;
                    
        //            string format = entry.GetFormat();

        //            if (format == null)
        //            {
        //                entry.Message = "invalid format";
        //                entry.Progress = StatusCode.Error;
        //                continue;
        //            }

        //            var active = item.EbayListingItems.Where(p => p.Listing.MarketplaceID == marketplace.ID && p.Listing.Status.Equals(EbayMarketplace.STATUS_ACTIVE));
        //            var target = active.Where(p => p.Listing.Format.Equals(format));

        //            if (target.Count() == 1)
        //            {
        //                EbayListingItem listingItem = target.First();

        //                if (listingItem.Quantity == entry.Q)
        //                {
        //                    if (format.Equals(EbayMarketplace.FORMAT_AUCTION))
        //                    {
        //                        entry.Progress = StatusCode.Completed;
                                
        //                        entry.Message = "Completed";
        //                    }
        //                    else if(decimal.Equals(listingItem.Price, Math.Round(entry.P, 2)))
        //                    {
        //                        entry.Progress = StatusCode.Completed;
        //                        entry.Message = "Completed";
        //                    }
        //                }
        //            }
        //            else if (target.Count() > 1)
        //            {
        //                entry.Message = "duplicate";
        //                entry.Progress = StatusCode.Error;
        //            }

        //            if (entry.Progress.Equals(StatusCode.Pending))
        //            {
                        
        //                if (format.Equals(EbayMarketplace.FORMAT_AUCTION) && entry.Q > 1)
        //                {
        //                    if (entry.Q > 1)
        //                    {
        //                        entry.Message = "auction max qty is 1";
        //                        entry.Progress = StatusCode.Error;
        //                    }
        //                    if(item.AuctionCount >= item.QtyAvailable)
        //                    {
        //                        entry.Message = "out of stock";
        //                        entry.Progress = StatusCode.Error;
        //                    }
        //                }

        //                if (entry.Q > item.QtyAvailable)
        //                {
        //                    entry.Message = "out of stock";
        //                    entry.Progress = StatusCode.Error;
        //                }
        //                if (item.Department == null)
        //                {
        //                    entry.Message = "department required";
        //                    entry.Progress = StatusCode.Error;
        //                }
        //                if (entry.Title.Count() > 80)
        //                {
        //                    entry.Message = "title max characters is 80";
        //                    entry.Progress = StatusCode.Error;
        //                }
        //                if (entry.StartDateSpecified && entry.StartDate < DateTime.UtcNow)
        //                {
        //                    entry.Message = "cannot schedule in the past";
        //                    entry.Progress = StatusCode.Error;
        //                }
        //            }

        //        } 
        //    }
        //}

        public override List<ListingEntry> UpdateAndValidateEntries(List<ListingEntry> entries)
        {
            _lastRowIndex = entries.Max(p => p.RowIndex);
            _newEntries = new List<ListingEntry>();

            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                _marketplace = dataContext.EbayMarketplaces.Single(p => p.Code.Equals(_marketplaceCode));

                var entryGroups = entries.Where(p => p.Status.Equals(StatusCode.Pending)).Cast<EbayEntry>().GroupBy(p => p.Sku);

                foreach (var group in entryGroups)
                {
                    Item item = dataContext.Items.SingleOrDefault(p => p.ItemLookupCode.Equals(group.Key));

                    if (item == null)
                    {
                        foreach (var entry in group)
                        {
                            entry.Message = "sku not found";
                            entry.Status = StatusCode.Error;
                        }
                    }
                    else
                    {
                        UpdateGroup(item, group.ToList());

                        foreach (EbayEntry entry in group)
                        {
                            entry.Brand = item.SubDescription1;
                            entry.ClassName = item.ClassName;

                            if (entry.Status.Equals(StatusCode.Pending))
                            {
                                if (entry.GetFormat() == null)
                                {
                                    entry.Message = "invalid format";
                                    entry.Status = StatusCode.Error;
                                }
                                else
                                {
                                    Validate(item, entry);
                                }
                            }
                        }
                    }
                }
            }

            return _newEntries;
        }

        //public List<ListingEntry> UpdateAndValidateEntries2(List<ListingEntry> entries)
        //{
        //    _lastRowIndex = entries.Max(p => p.RowIndex);
        //    _newEntries = new List<ListingEntry>();

        //    using (berkeleyEntities dataContext = new berkeleyEntities())
        //    {
        //        _marketplace = dataContext.EbayMarketplaces.Single(p => p.Code.Equals(_marketplaceCode));

        //        var entryGroups = entries.Where(p => p.Status.Equals(StatusCode.Pending)).Cast<EbayEntry>().GroupBy(p => p.Sku);

        //        foreach (var group in entryGroups)
        //        {
        //            Item item = dataContext.Items.SingleOrDefault(p => p.ItemLookupCode.Equals(group.Key));

        //            if (item == null)
        //            {
        //                foreach (var entry in group)
        //                {
        //                    entry.Message = "sku not found";
        //                    entry.Status = StatusCode.Error;
        //                }
        //            }
        //            else
        //            {
        //                UpdateGroup(item, group.ToList());

        //                foreach (EbayEntry entry in group)
        //                {
        //                    entry.Brand = item.SubDescription1;
        //                    entry.ClassName = item.ClassName;

        //                    if (entry.Status.Equals(StatusCode.Pending))
        //                    {
        //                        if (entry.GetFormat() == null)
        //                        {
        //                            entry.Message = "invalid format";
        //                            entry.Status = StatusCode.Error;
        //                        }
        //                        else
        //                        {
        //                            Validate(item, entry);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    return _newEntries;
        //}

        private void UpdateGroup(Item item, IEnumerable<EbayEntry> group)
        {
            var active = item.EbayListingItems.Where(p => p.Listing.MarketplaceID == _marketplace.ID && p.Listing.Status.Equals(EbayMarketplace.STATUS_ACTIVE));

            var existingEntries = group.ToList();

            foreach (var listingItem in active)
            {
                string format = listingItem.Listing.Format;

                if (format.Equals("StoresFixedPrice"))
                {
                    format = EbayMarketplace.FORMAT_FIXEDPRICE;
                }

                EbayEntry entry = existingEntries.FirstOrDefault(p => p.GetFormat() != null && p.GetFormat().Equals(format));

                if (entry == null)
                {
                    entry = existingEntries.FirstOrDefault(p => p.GetFormat() == null);

                    if (entry == null)
                    {
                        entry = new EbayEntry();
                        entry.Sku = item.ItemLookupCode;
                        entry.Brand = item.SubDescription1;
                        entry.ClassName = item.ClassName;
                        
                        entry.ParentRowIndex = group.First().RowIndex;
                        entry.RowIndex = _lastRowIndex + 1;
                        _lastRowIndex = entry.RowIndex;
                        _newEntries.Add(entry);
                    }
                    else
                    {
                        existingEntries.Remove(entry);
                    }

                    entry.SetFormat(listingItem.Listing.Format, listingItem.Listing.Duration);
                    entry.P = listingItem.Price;
                    entry.Q = listingItem.Quantity;
                    entry.Title = listingItem.Title;
                    entry.FullDescription = listingItem.Listing.FullDescription;
                    entry.Message = "filled by program";
                    entry.Status = StatusCode.Completed;
                }
                else
                {
                    if (entry.Q == listingItem.Quantity && decimal.Compare(entry.P, listingItem.Price) == 0 && entry.GetUpdateFlags().Count == 0)
                    {
                        entry.Status = StatusCode.Completed;
                    }
                    else
                    {
                        entry.Message = string.Format("({0}|{1}|{2})", listingItem.FormatCode, listingItem.Quantity, Math.Round(listingItem.Price, 2));
                    }

                    existingEntries.Remove(entry);
                }
            }
        }

        private void Validate(Item item, EbayEntry entry)
        {
            if (entry.GetFormat().Equals(EbayMarketplace.FORMAT_AUCTION))
            {
                if (entry.Q > 1)
                {
                    entry.Message = "auction max qty is 1";
                    entry.Status = StatusCode.Error;
                }

                if (item.AuctionCount >= item.QtyAvailable && entry.Q != 0)
                {
                    entry.Message = "out of stock";
                    entry.Status = StatusCode.Error;
                }
            }

            if (entry.Q == 0 && string.IsNullOrWhiteSpace(entry.Command))
            {
                entry.Message = "qty must be greater than 0";
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
            if (entry.Title != null && entry.Title.Count() > 80)
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

        public override Type EntryType
        {
            get { return typeof(EbayEntry); }
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
            var auctions = _marketplace.Listings.Where(p => p.Format.Equals(EbayMarketplace.FORMAT_AUCTION)).Where(p => p.Status.Equals(EbayMarketplace.STATUS_ACTIVE));

            foreach (EbayEntry entry in group)
            {
                if (auctions.Any(p => p.Sku.Equals(entry.Sku)))
                {
                    EbayListing listing = auctions.Single(p => p.Sku.Equals(entry.Sku));

                    TryUpdateListing(listing, new EbayEntry[] { entry });
                }
                else
                {
                    TryCreateListing(new EbayEntry[] { entry });
                }
            }
        }

        private void HandleFixedPriceGroup(IGrouping<string,EbayEntry> group)
        {
            var fixedPrice = _marketplace.Listings.Where(p => p.Status.Equals(EbayMarketplace.STATUS_ACTIVE) && (p.Format.Equals(EbayMarketplace.FORMAT_FIXEDPRICE) || p.Format.Equals("StoresFixedPrice")));

            List<EbayEntry> pending = new List<EbayEntry>();

            foreach (EbayEntry entry in group)
            {
                if (fixedPrice.Any(p => p.Sku.Equals(entry.Sku)))
                {
                    EbayListing listing = fixedPrice.Single(p => p.Sku.Equals(entry.Sku));

                    TryUpdateListing(listing, new EbayEntry[] { entry });
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
                    EbayListing listing = fixedPrice.Single(p => p.Sku.Equals(group.Key));
                    TryUpdateListing(listing, pending);
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

                foreach (var entry in entries)
                {

                    entry.Message = string.Format("{0}|{1}|{2}", entry.Format, entry.Q, Math.Round(entry.P, 2));

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
                    entry.ClearMessages();

                    entry.Message = string.Format("{0}|{1}|{2}", entry.Format, entry.Q, Math.Round(entry.P, 2));

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

            if (entries.Any(p => !string.IsNullOrWhiteSpace(p.FullDescription)))
            {
                listingDto.FullDescription = entries.First(p => !string.IsNullOrWhiteSpace(p.FullDescription)).FullDescription;
            }

            foreach (EbayEntry entry in entries)
            {
                ListingItemDto listingItemDto = new ListingItemDto();
                listingItemDto.Sku = entry.Sku;
                listingItemDto.Qty = entry.Q;
                listingItemDto.QtySpecified = entry.GetUpdateFlags().Any(p => p.Trim().ToUpper().Equals("Q"));
                listingItemDto.Price = entry.P;
                listingItemDto.PriceSpecified = entry.GetUpdateFlags().Any(p => p.Trim().ToUpper().Equals("P"));

                if (entry.GetUpdateFlags().Any(p => p.Trim().ToUpper().Equals("TITLE")))
                {
                    listingItemDto.Title = entry.Title;
                }

                listingDto.Items.Add(listingItemDto);
            }

            var activeSkus = listing.ListingItems.Where(p => p.Quantity != 0).Select(p => p.Item.ItemLookupCode);

            bool mustIncludeProductData = listingDto.Items.Any(p => !activeSkus.Any(s => s.Equals(p.Sku)));

            foreach (EbayListingItem listingItem in listing.ListingItems)
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

            bool includeTemplate = entries.Any(p => p.GetUpdateFlags().Any(s => s.Trim().ToUpper().Equals("TEMPLATE")));

            bool includeProductData = entries.Any(p => p.GetUpdateFlags().Any(s => s.Trim().ToUpper().Equals("PRODUCTDATA"))) || mustIncludeProductData;

            if (listingDto.Items.All(p => p.Qty == 0))
            {
                _ebayServices.End(_marketplace.ID, listing.Code);
            }
            else
            {
                _ebayServices.Revise(listingDto, includeProductData, includeTemplate);
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

            if (entries.Any(p => p.StartDateSpecified))
            {
                listingDto.ScheduleTime = entries.First(p => p.StartDateSpecified).StartDate;
                listingDto.ScheduleTimeSpecified = true;
            }

            if (entries.Count() > 1)
            {
                listingDto.Sku = entries.First(p => !string.IsNullOrWhiteSpace(p.ClassName)).ClassName;

                EbayEntry entry = entries.First(p => !string.IsNullOrWhiteSpace(p.Title));
                listingDto.Title = GetParentTitle(entry); ;
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

            _ebayServices.Publish(listingDto);
        }

        private string GetParentTitle(EbayEntry entry)
        {
            string title = entry.Title;

            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                dataContext.MaterializeAttributes = true;

                Item item = dataContext.Items.Single(p => p.ItemLookupCode.Equals(entry.Sku));

                var wordsToRemove = item.Attributes.Select(p => p.Key.ToString()).Concat(item.Attributes.Select(p => p.Value.Value.ToString()));

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
