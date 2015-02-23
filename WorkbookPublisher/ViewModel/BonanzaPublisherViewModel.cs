using BerkeleyEntities;
using BerkeleyEntities.Bonanza;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkbookPublisher.ViewModel
{
    class BonanzaPublisherViewModel : PublisherViewModel
    {
        public BonanzaPublisherViewModel(ExcelWorkbook workbook, string marketplaceCode)
            : base(workbook, marketplaceCode)
        {
            _readEntriesCommand = new ReadCommand(_workbook, _marketplaceCode, typeof(BonanzaEntry));
            _publishCommand = new BonanzaPublishCommand(_marketplaceCode);
            _updateCommand = new UpdateCommand(_workbook, _marketplaceCode);

            _readEntriesCommand.ReadCompleted += _publishCommand.ReadCompletedHandler;
            _readEntriesCommand.ReadCompleted += _updateCommand.ReadCompletedHandler;
            _updateCommand.FixCompleted += _readEntriesCommand.FixCompletedHandler;
            _updateCommand.FixCompleted += _publishCommand.FixCompletedHandler;
        }

        public class BonanzaPublishCommand : PublishCommand
        {
            private BerkeleyEntities.Bonanza.BonanzaServices _services = new BerkeleyEntities.Bonanza.BonanzaServices();
            private string _marketplaceCode;
            private berkeleyEntities _dataContext;
            private BonanzaMarketplace _marketplace;


            public BonanzaPublishCommand(string marketplaceCode)
            {
                _marketplaceCode = marketplaceCode;
            }

            public override async void Publish(IEnumerable<ListingEntry> entries)
            {
                await Task.Run(() => PublishEntries(entries.Cast<BonanzaEntry>()));

                RaisePublishCompleted();
            }

            private void PublishEntries(IEnumerable<BonanzaEntry> entries)
            {
                using (_dataContext = new berkeleyEntities())
                {
                    _marketplace = _dataContext.BonanzaMarketplaces.Single(p => p.Code.Equals(_marketplaceCode));

                    var update = entries.Where(p => !string.IsNullOrWhiteSpace(p.Code)).GroupBy(p => p.Code);

                    foreach (var group in update)
                    {
                        BonanzaListing listing = _marketplace.Listings.Single(p => p.Code.Equals(group.Key));

                        TryUpdateListing(listing, group);
                    }

                    var create = entries.Where(p => string.IsNullOrWhiteSpace(p.Code)).GroupBy(p => p.ClassName);

                    foreach (var group in create)
                    {
                        TryCreateListing(group);
                    }
                }
            }

            private void TryCreateListing(IEnumerable<BonanzaEntry> entries)
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

            private void TryUpdateListing(BonanzaListing listing, IEnumerable<BonanzaEntry> entries)
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

            private void UpdateListing(BonanzaListing listing, IEnumerable<BonanzaEntry> entries)
            {
                ListingDto listingDto = new ListingDto();

                listingDto.Code = listing.Code;
                listingDto.MarketplaceID = _marketplace.ID;
                listingDto.Brand = entries.First(p => !string.IsNullOrWhiteSpace(p.Brand)).Brand;
                listingDto.IsVariation = (bool)listing.IsVariation;

                if (entries.Any(p => !string.IsNullOrWhiteSpace(p.FullDescription)))
                {
                    listingDto.FullDescription = entries.First(p => !string.IsNullOrWhiteSpace(p.FullDescription)).FullDescription;
                }

                foreach (BonanzaEntry entry in entries)
                {
                    BonanzaListingItem listingItem = listing.ListingItems.SingleOrDefault(p => p.Item.ItemLookupCode.Equals(entry.Sku));

                    ListingItemDto listingItemDto = new ListingItemDto();
                    listingItemDto.Sku = entry.Sku;
                    listingItemDto.Qty = entry.Q;
                    listingItemDto.QtySpecified = listingItem == null || listingItem.Quantity != entry.Q ;
                    listingItemDto.Price = entry.P;
                    listingItemDto.PriceSpecified = listingItem == null || decimal.Compare(entry.P, listingItem.Price) != 0;

                    listingItemDto.Title = entry.Title;

                    listingDto.Items.Add(listingItemDto);
                }

                var activeSkus = listing.ListingItems.Where(p => p.Quantity != 0).Select(p => p.Item.ItemLookupCode);

                bool mustIncludeProductData = listingDto.Items.Any(p => !activeSkus.Any(s => s.Equals(p.Sku)));

                foreach (BonanzaListingItem listingItem in listing.ListingItems)
                {
                    if (!listingDto.Items.Any(p => p.Sku.Equals(listingItem.Item.ItemLookupCode)))
                    {
                        ListingItemDto listingItemDto = new ListingItemDto();
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
                    _services.End(_marketplace.ID, listing.Code);
                }
                else
                {
                    _services.Revise(listingDto, includeProductData, includeTemplate);
                }


            }

            private void CreateListing(IEnumerable<BonanzaEntry> entries)
            {
                ListingDto listingDto = new ListingDto();

                listingDto.MarketplaceID = _marketplace.ID;
                listingDto.Brand = entries.First(p => !string.IsNullOrWhiteSpace(p.Brand)).Brand;
                listingDto.FullDescription = entries.First(p => !string.IsNullOrWhiteSpace(p.FullDescription)).FullDescription;

                if (entries.Count() > 1)
                {
                    listingDto.Sku = entries.First(p => !string.IsNullOrWhiteSpace(p.ClassName)).ClassName;

                    BonanzaEntry entry = entries.First(p => !string.IsNullOrWhiteSpace(p.Title));
                    listingDto.Title = GetParentTitle(entry); ;
                    listingDto.IsVariation = true;
                }
                else
                {
                    listingDto.Sku = entries.First().Sku;
                    listingDto.Title = entries.First().Title;
                    listingDto.IsVariation = false;
                }

                foreach (BonanzaEntry entry in entries)
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

                _services.Publish(listingDto);
            }

            private string GetParentTitle(BonanzaEntry entry)
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
}
