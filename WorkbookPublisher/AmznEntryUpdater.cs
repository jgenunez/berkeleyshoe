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
        private string _marketplaceCode;
        private List<AmznEntry> _entries;
        private List<AmznEntry> _addedEntries = new List<AmznEntry>();
        private berkeleyEntities _dataContext = new berkeleyEntities();

        public AmznEntryUpdater(IEnumerable<AmznEntry> entries, string marketplaceCode)
        {
            _marketplaceCode = marketplaceCode;
            _entries = entries.ToList();
        }

        public List<AmznEntry> Update()
        {
            var entryGroups = _entries.Where(p => p.Status.Equals(StatusCode.Pending)).GroupBy(p => p.Sku);

            foreach (var group in entryGroups)
            {
                try
                {
                    Item item = _dataContext.Items.Single(p => p.ItemLookupCode.Equals(group.Key));

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

            if (string.IsNullOrWhiteSpace(entry.Format))
            {
                entry.Message = "invalid format";
                entry.Status = StatusCode.Error;
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
                var entry = entries.FirstOrDefault(p => p.IsValid() && p.Format.Equals("GTC"));

                if (entry != null)
                {
                    entry.ASIN = listingItem.ASIN;

                    if (listingItem.Quantity == entry.Q.Value && decimal.Compare(listingItem.Price, entry.P.Value) == 0 && entry.GetUpdateFlags().Count == 0)
                    {
                        entry.Status = StatusCode.Completed;
                    }
                    else
                    {
                        entry.Message = string.Format("({0}|{1})", listingItem.Quantity, Math.Round(listingItem.Price, 2));

                        if (listingItem.Quantity == entry.Q.Value)
                        {
                            entry.Q = null;
                        }

                        if(decimal.Compare(listingItem.Price, entry.P.Value) == 0)
                        {
                            entry.P = null;
                        }
                    }

                    entries.Remove(entry);
                }
                else
                {
                    entry = entries.FirstOrDefault(p => !p.IsValid());

                    if (entry != null)
                    {
                        entry.Message = "modified by program";
                        entries.Remove(entry);
                    }
                    else
                    {
                        entry = new AmznEntry();
                        entry.Brand = item.SubDescription1;
                        entry.ClassName = item.ClassName;
                        entry.Sku = item.ItemLookupCode;
                        entry.Message = "added by program";

                        entry.ParentRowIndex = group.First().RowIndex;

                        _addedEntries.Add(entry);
                    }

                    entry.Format = "GTC";
                    entry.ASIN = listingItem.ASIN;
                    entry.P = listingItem.Price;
                    entry.Q = listingItem.Quantity;
                    entry.Title = listingItem.Title;               
                    entry.Status = StatusCode.Completed;
                }
            }
        }
    }
}
