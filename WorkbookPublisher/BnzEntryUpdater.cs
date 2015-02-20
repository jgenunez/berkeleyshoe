using BerkeleyEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkbookPublisher
{
    public class BnzEntryUpdater
    {

        private string _marketplaceCode;
        private uint _lastRowIndex;
        private List<BonanzaEntry> _entries;
        private List<BonanzaEntry> _addedEntries = new List<BonanzaEntry>();
        private berkeleyEntities _dataContext = new berkeleyEntities();

        public BnzEntryUpdater(IEnumerable<BonanzaEntry> entries, string marketplaceCode)
        {
            _entries = entries.ToList();
            _marketplaceCode = marketplaceCode;
            _lastRowIndex = entries.Max(p => p.RowIndex);
        }

        public List<BonanzaEntry> Update()
        {
            var entryGroups = _entries.Where(p => p.Status.Equals(StatusCode.Pending)).GroupBy(p => p.Sku);

            foreach (var group in entryGroups)
            {
                try
                {
                    Item item = _dataContext.Items.Single(p => p.ItemLookupCode.Equals(group.Key));

                    UpdateGroup(item, group.ToList());

                    foreach (BonanzaEntry entry in group)
                    {
                        entry.Brand = item.SubDescription1;
                        entry.ClassName = item.ClassName;

                        if (entry.Status.Equals(StatusCode.Pending))
                        {
                            Validate(item, entry);
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

        private void UpdateGroup(Item item, IEnumerable<BonanzaEntry> group)
        {
            var active = item.BonanzaListingItems.Where(p => p.Listing.Marketplace.Code.Equals(_marketplaceCode) && !p.Listing.Status.Equals(BonanzaMarketplace.STATUS_ENDED));

            var existingEntries = group.ToList();

            foreach (var listingItem in active)
            {
                BonanzaEntry entry = existingEntries.FirstOrDefault();

                if (entry != null)
                {
                    entry.Code = listingItem.Listing.Code;

                    if (entry.Q == listingItem.Quantity && decimal.Compare(entry.P, listingItem.Price) == 0 && entry.GetUpdateFlags().Count == 0)
                    {
                        entry.Status = StatusCode.Completed;
                    }
                    else
                    {
                        entry.Message = string.Format("({0}|{1})", listingItem.Quantity, Math.Round(listingItem.Price, 2));
                    }

                    existingEntries.Remove(entry);
                }
                else
                {
                    CreateNewEntry(listingItem);
                }
            }

            foreach (var entry in existingEntries)
            {
                if (_dataContext.BonanzaListings.Any(p => p.Sku.Equals(item.ClassName) && !p.Status.Equals(BonanzaMarketplace.STATUS_ENDED)))
                {
                    BonanzaListing listing = _dataContext.BonanzaListings.Single(p => p.Sku.Equals(item.ClassName) && !p.Status.Equals(BonanzaMarketplace.STATUS_ENDED));

                    entry.Code = listing.Code;
                }
            }
        }

        private void CreateNewEntry(BonanzaListingItem listingItem)
        {
            Item item = listingItem.Item;

            BonanzaEntry entry = new BonanzaEntry();

            entry.Sku = item.ItemLookupCode;
            entry.Code = listingItem.Listing.Code;
            entry.Brand = item.SubDescription1;
            entry.ClassName = item.ClassName;

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

        private void Validate(Item item, BonanzaEntry entry)
        {
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
        }
    }
}
