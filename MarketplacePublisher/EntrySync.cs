using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BerkeleyEntities;

namespace MarketplacePublisher
{
    public class EntrySync
    {
        private berkeleyEntities _dataContext = new berkeleyEntities();
        private uint _lastIndex = 2;
        
        public List<Entry> UpdateEntries(IEnumerable<string> skus)
        {
            List<Entry> entries = new List<Entry>();

            foreach (string sku in skus)
            {
                Item item = _dataContext.Items
                    .Include("AmznListingItems.OrderItems.Order")
                    .Include("EbayListingItems.OrderItems.Order")
                    .SingleOrDefault(p => p.ItemLookupCode.Equals(sku));

                if (item == null)
                {
                    continue;
                }

                int count = entries.Count;

                foreach (EbayListingItem listingItem in item.EbayListingItems.Where(p => p.Listing.Status.Equals("Active")))
                {
                    Entry entry = CreateEntry();
                    entry.SetMarketplace(listingItem.Listing.Marketplace.Name);
                    entry.SetCondition(listingItem.Listing.Condition);
                    entry.ListingID = listingItem.Listing.Code;
                    entry.Qty = listingItem.Quantity;
                    entry.Price = listingItem.Price;
                    entry.Title = listingItem.Listing.Title;
                    entry.SetListingType(listingItem.Listing.Format, listingItem.Listing.Duration, (bool)listingItem.Listing.IsVariation);

                    UpdateProductData(entry, item);

                    entries.Add(entry);
                }


                foreach (AmznListingItem listingItem in item.AmznListingItems)
                {
                    Entry entry = CreateEntry();
                    entry.SetMarketplace(listingItem.Marketplace.Name);
                    entry.Condition = "NEW";
                    entry.Type = "N/A";
                    entry.ListingID = listingItem.ASIN;
                    entry.Qty = listingItem.Quantity;
                    entry.Price = listingItem.Price;
                    entry.Title = listingItem.Title;

                    UpdateProductData(entry, item);

                    entries.Add(entry);
                }

                if (count == entries.Count)
                {
                    Entry entry = CreateEntry();
                    UpdateProductData(entry, item);
                    entries.Add(entry);
                }
            }
            
            return entries;
        }

        private void UpdateProductData(Entry entry, Item item)
        {
            entry.Sku = item.ItemLookupCode;
            entry.Brand = item.SubDescription1;
            entry.OH = (int)item.Quantity;
            entry.Cost = item.Cost;
            entry.Available = (int)item.Quantity - item.OnHold - item.OnActiveListing - item.OnPendingOrder;
        }

        private Entry CreateEntry()
        {
            Entry entry = new Entry();
            entry.RowIndex = _lastIndex;

            _lastIndex++;

            return entry;
        }

        //private void SyncEbayListings(IGrouping<string,Entry> entryGroup)
        //{
        //    foreach (EbayListingItem listingItem in _item.EbayListingItems.Where(p => p.Listing.Status.Equals("Active")))
        //    {
        //        Entry entry = entryGroup.Where(p => p.IsValidListing()).SingleOrDefault(p =>
        //            p.GetMarketplaceName().Equals(listingItem.Listing.Marketplace.Name) &&
        //            p.GetFormat().Equals(listingItem.Listing.Format));

        //        if (entry == null)
        //        {
        //            entry = entryGroup.FirstOrDefault(p => !p.IsValidListing());

        //            if (entry == null)
        //            {
        //                entry = CreateEntry();
        //            }

        //            entry.SetMarketplace(listingItem.Listing.Marketplace.Name);
        //        }

        //        entry.Condition = listingItem.Listing.ConditionLabel;
        //        entry.ListingID = listingItem.Listing.Code;
        //        entry.Qty = listingItem.Quantity;
        //        entry.Price = listingItem.Price;
        //        entry.Title = listingItem.Listing.Title;
        //        entry.SetListingType(listingItem.Listing.Format, listingItem.Listing.Duration, (bool)listingItem.Listing.IsVariation);
        //    }
        //}

        //private void SyncAmznListings(IGrouping<string, Entry> entryGroup)
        //{
        //    foreach (AmznListingItem listingItem in _item.AmznListingItems)
        //    {
        //        Entry entry = entryGroup
        //            .Where(p => p.IsValidListing())
        //            .SingleOrDefault(p => p.GetMarketplaceName().Equals(listingItem.Marketplace.Name));

        //        if (entry == null)
        //        {
        //            entry = entryGroup.FirstOrDefault(p => !p.IsValidListing());

        //            if (entry == null)
        //            {
        //                entry = CreateEntry();
        //            }

        //            entry.SetMarketplace(listingItem.Marketplace.Name);
        //        }

        //        entry.Condition = "NEW";
        //        entry.Type = "N/A";
        //        entry.ListingID = listingItem.ASIN;
        //        entry.Qty = listingItem.Quantity;
        //        entry.Price = listingItem.Price;
        //        entry.Title = listingItem.Title;
        //    }
        //}

        //public List<Entry> SyncEntries2(List<Entry> entries)
        //{
        //    _output.Clear();
        //    _output.AddRange(entries);
        //    _lastIndex = entries.Max(p => p.RowIndex);

        //    var entryGroups = entries.GroupBy(p => p.Sku).ToList();

        //    foreach (var entryGroup in entryGroups)
        //    {
        //        _item = _dataContext.Items.Include("AmznListingItems.OrderItems.Order").Include("EbayListingItems.OrderItems.Order")
        //            .SingleOrDefault(p => p.ItemLookupCode.Equals(entryGroup.Key));

        //        if (_item != null)
        //        {
        //            foreach (Entry entry in entryGroup)
        //            {
        //                UpdateProductData(entry);
        //            }

        //            SyncEbayListings(entryGroup);
        //            SyncAmznListings(entryGroup);
        //        }
        //        else
        //        {
        //            foreach (Entry row in entryGroup)
        //            {
        //                row.Status = "sku not found";
        //            }
        //        }
        //    }


        //    return _output;
        //}

        

       
    }
}
