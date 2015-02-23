using BerkeleyEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkbookPublisher
{
    public class EbayEntryUpdater
    {
        private uint _lastRowIndex;
        private string _marketplaceCode;
        private List<EbayEntry> _entries;
        private List<EbayEntry> _addedEntries = new List<EbayEntry>();
        private berkeleyEntities _dataContext = new berkeleyEntities();

        public EbayEntryUpdater(IEnumerable<EbayEntry> entries, string marketplaceCode) 
        {
            _marketplaceCode = marketplaceCode;
            _entries = entries.ToList();
            _lastRowIndex = entries.Max(p => p.RowIndex);
        }

        public List<EbayEntry> Update()
        {
            var entryGroups = _entries.Where(p => p.Status.Equals(StatusCode.Pending)).GroupBy(p => p.Sku);

            foreach (var group in entryGroups)
            {
                try
                {
                    Item item = _dataContext.Items.Single(p => p.ItemLookupCode.Equals(group.Key));

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
                catch (InvalidOperationException e)
                {
                    foreach (var entry in group)
                    {
                        entry.Message = e.Message;
                        entry.Status = StatusCode.Error;
                    }
                }
            }

            return _addedEntries;
        }

        private void UpdateGroup(Item item, IEnumerable<EbayEntry> group)
        {
            var active = item.EbayListingItems.Where(p => p.Listing.Marketplace.Code.Equals(_marketplaceCode) && p.Listing.Status.Equals(EbayMarketplace.STATUS_ACTIVE));

            var entries = group.ToList();

            foreach (var listingItem in active)
            {
                string format = listingItem.Listing.Format.Equals(EbayMarketplace.FORMAT_STOREFIXEDPRICE) ? EbayMarketplace.FORMAT_FIXEDPRICE : listingItem.Listing.Format;

                EbayEntry entry = entries.FirstOrDefault(p => p.GetFormat() != null && p.GetFormat().Equals(format));

                if (entry != null)
                {
                    entry.Code = listingItem.Listing.Code;

                    if (entry.Q == listingItem.Quantity && decimal.Compare(entry.P, listingItem.Price) == 0 && entry.GetUpdateFlags().Count == 0)
                    {
                        entry.Status = StatusCode.Completed;
                    }
                    else
                    {
                        entry.Message = string.Format("({0}|{1}|{2})", listingItem.FormatCode, listingItem.Quantity, Math.Round(listingItem.Price, 2));

                        entry.PSpecified = true;
                        entry.QSpecified = true;
                    }

                    entries.Remove(entry);
                }
                else
                {
                    entry = entries.FirstOrDefault(p => p.GetFormat() == null);

                    if (entry != null)
                    {
                        entry.SetFormat(listingItem.Listing.Format, listingItem.Listing.Duration);
                        entry.P = listingItem.Price;
                        entry.Q = listingItem.Quantity;
                        entry.Title = listingItem.Title;
                        entry.FullDescription = listingItem.Listing.FullDescription;
                        entry.Message = "modified by program";
                        entry.Status = StatusCode.Completed;

                        entries.Remove(entry);
                    }
                    else
                    {
                        CreateNewEntry(listingItem);
                    }
                }
            }

            foreach (var entry in entries)
            {
                entry.QSpecified = true;
                entry.PSpecified = true;

                if (_dataContext.EbayListings.Any(p => p.Sku.Equals(item.ClassName) && p.Status.Equals(EbayMarketplace.STATUS_ACTIVE)))
                {
                    EbayListing listing = _dataContext.EbayListings.Single(p => p.Sku.Equals(item.ClassName) && p.Status.Equals(EbayMarketplace.STATUS_ACTIVE));

                    entry.Code = listing.Code;
                }
            }
        }

        private void CreateNewEntry(EbayListingItem listingItem)
        {
            Item item = listingItem.Item;

            EbayEntry entry = new EbayEntry();

            entry.Sku = item.ItemLookupCode;
            entry.Code = listingItem.Listing.Code;
            entry.Brand = item.SubDescription1;
            entry.ClassName = item.ClassName;

            entry.SetFormat(listingItem.Listing.Format, listingItem.Listing.Duration);

            entry.P = listingItem.Price;
            entry.Q = listingItem.Quantity;
            entry.Title = listingItem.Title;
            entry.FullDescription = listingItem.Listing.FullDescription;
            entry.Message = "added by program";
            entry.Status = StatusCode.Completed;

            entry.RowIndex = _lastRowIndex + 1;

            _lastRowIndex = entry.RowIndex;

            _addedEntries.Add(entry);
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

                if (item.AuctionWithBidQty >= item.QtyAvailable && entry.Q != 0)
                {
                    entry.Message = "out of stock";
                    entry.Status = StatusCode.Error;
                }
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
    }
}
