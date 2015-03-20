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
        private List<BonanzaEntry> _entries;
        private List<BonanzaEntry> _addedEntries = new List<BonanzaEntry>();
        private berkeleyEntities _dataContext = new berkeleyEntities();

        public BnzEntryUpdater(IEnumerable<BonanzaEntry> entries, string marketplaceCode)
        {
            _entries = entries.ToList();
            _marketplaceCode = marketplaceCode;
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
                BonanzaEntry entry = existingEntries.FirstOrDefault(p => p.IsValid());

                if (entry != null)
                {
                    entry.Code = listingItem.Listing.Code;

                    if (entry.Q.Value == listingItem.Quantity && decimal.Compare(entry.P.Value, listingItem.Price) == 0 && entry.GetUpdateFlags().Count == 0)
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
                    entry = existingEntries.FirstOrDefault(p => !p.IsValid());

                    if (entry != null)
                    {
                        entry.Message = "modified by program";
                        existingEntries.Remove(entry);
                    }
                    else
                    {
                        entry = new BonanzaEntry();
                        entry.Sku = item.ItemLookupCode;
                        
                        entry.Brand = item.SubDescription1;
                        entry.ClassName = item.ClassName;
                        entry.Message = "added by program";

                        _addedEntries.Add(entry);
                    }

                    entry.Format = "GTC";
                    entry.Code = listingItem.Listing.Code;
                    entry.P = listingItem.Price;
                    entry.Q = listingItem.Quantity;
                    entry.Title = listingItem.Title;
                    entry.FullDescription = listingItem.Listing.FullDescription;
                    
                    entry.Status = StatusCode.Completed;
                }
            }

            foreach (var entry in existingEntries)
            {
                BonanzaListing listing = _dataContext.BonanzaListings.SingleOrDefault(p => p.Sku.Equals(item.ClassName) && !p.Status.Equals(BonanzaMarketplace.STATUS_ENDED));

                if (listing != null)
                {
                    if (!entry.IsValid())
                    {
                        entry.Code = listing.Code;
                        entry.Title = listing.Title;
                        entry.FullDescription = listing.FullDescription;
                        entry.Format = "GTC";
                        entry.Message = "modified by program";
                    }
                    else if (entry.Format.Equals("GTC"))
                    {
                        entry.Code = listing.Code;
                        entry.Message = "modified by program";
                    }
                }
            }
        }

        private void Validate(Item item, BonanzaEntry entry)
        {
            if (string.IsNullOrWhiteSpace(entry.Code))
            {
                if (string.IsNullOrWhiteSpace(entry.FullDescription))
                {
                    entry.Message = "full description required";
                    entry.Status = StatusCode.Error;
                }

                if (entry.Q < 1)
                {
                    entry.Message = "quantity must be greater than zero";
                    entry.Status = StatusCode.Error;
                }

                if (string.IsNullOrWhiteSpace(entry.Title))
                {
                    entry.Message = "title required";
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
        }
    }
}
