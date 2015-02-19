using BerkeleyEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkbookPublisher
{
    public class AmznEntryUpdater
    {
        private uint _lastRowIndex;
        private string _marketplaceCode;
        private List<AmznEntry> _entries;
        private List<AmznEntry> _addedEntries = new List<AmznEntry>();
        private berkeleyEntities _dataContext = new berkeleyEntities();

        public AmznEntryUpdater(IEnumerable<AmznEntry> entries, string marketplaceCode)
        {
            _marketplaceCode = marketplaceCode;
            _entries = entries.Where(p => !string.IsNullOrWhiteSpace(p.Sku)).ToList();
            _lastRowIndex = entries.Max(p => p.RowIndex);
        }

        public List<AmznEntry> Update()
        {
            var entryGroups = _entries.Where(p => p.Status.Equals(StatusCode.Pending)).GroupBy(p => p.Sku);

            foreach (var group in entryGroups)
            {
                Item item = _dataContext.Items.SingleOrDefault(p => p.ItemLookupCode.Equals(group.Key));

                if (item != null)
                {
                    UpdateGroup(item, group.ToList());

                    foreach (var entry in group)
                    {
                        entry.ClassName = item.ClassName;
                        entry.Brand = item.SubDescription1;

                        if (entry.Status.Equals(StatusCode.Pending))
                        {
                            Validate(item, entry);
                        }
                    }
                }
                else
                {
                    foreach (var entry in group)
                    {
                        entry.Message = "sku not found";
                        entry.Status = StatusCode.Error;
                    }
                }
            }

            return _addedEntries.Concat(_entries).ToList();
 
        }

        private void Validate(Item item, AmznEntry entry)
        {
            if (item.Notes != null)
            {
                if (item.Notes.Contains("PRE") || item.Notes.Contains("NWB") || item.Notes.Contains("NWD"))
                {
                    entry.Message = "only new products allowed";
                    entry.Status = StatusCode.Error;
                }
            }

            if (entry.Q > item.QtyAvailable)
            {
                entry.Message = "out of stock";
                entry.Status = StatusCode.Error;
            }

            if (string.IsNullOrEmpty(item.GTIN))
            {
                entry.Message = "UPC or EAN required";
                entry.Status = StatusCode.Error;
            }

            if (item.Department == null)
            {
                entry.Message = "department classification required";
                entry.Status = StatusCode.Error;
            }
        }

        private void UpdateGroup(Item item, IEnumerable<AmznEntry> group)
        {
            var active = item.AmznListingItems.Where(p => p.IsActive && p.Marketplace.Code.Equals(_marketplaceCode));

            var entries = group.ToList();

            foreach (var listingItem in active)
            {
                var entry = entries.FirstOrDefault();

                if (entry != null)
                {
                    entry.ASIN = listingItem.ASIN;

                    if (listingItem.Quantity == entry.Q && decimal.Compare(listingItem.Price, entry.P) == 0 && entry.GetUpdateFlags().Count == 0)
                    {
                        entry.Status = StatusCode.Completed;

                    }
                    else
                    {
                        entry.Message = string.Format("({0}|{1})", listingItem.Quantity, Math.Round(listingItem.Price, 2));

                        entry.QSpecified = listingItem.Quantity != entry.Q;
                        entry.PSpecified = decimal.Compare(listingItem.Price, entry.P) != 0;
                    }

                    entries.Remove(entry);
                }
                else
                {
                    entry = new AmznEntry();
                    entry.Sku = item.ItemLookupCode;
                    entry.Brand = item.SubDescription1;
                    entry.ClassName = item.ClassName;
                    entry.ASIN = listingItem.ASIN;

                    entry.P = listingItem.Price;
                    entry.Q = listingItem.Quantity;
                    entry.Title = listingItem.Title;
                    entry.Message = "added by program";
                    entry.Status = StatusCode.Completed;

                    entry.ParentRowIndex = group.First().RowIndex;
                    entry.RowIndex = _lastRowIndex + 1;

                    _lastRowIndex = entry.RowIndex;
                    _addedEntries.Add(entry);
                }

            }

            foreach (var entry in entries)
            {
                entry.QSpecified = true;
                entry.PSpecified = true;
            }
        }
    }
}
