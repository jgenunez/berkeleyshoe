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
        private string _marketplaceCode;
        private List<EbayEntry> _entries;
        private List<EbayEntry> _addedEntries = new List<EbayEntry>();
        private berkeleyEntities _dataContext = new berkeleyEntities();

        public EbayEntryUpdater(IEnumerable<EbayEntry> entries, string marketplaceCode) 
        {
            _marketplaceCode = marketplaceCode;
            _entries = entries.ToList();
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

            var currentEntries = group.ToList();

            foreach (var listingItem in active)
            {
                string format = listingItem.Listing.Format.Equals(EbayMarketplace.FORMAT_STOREFIXEDPRICE) ? EbayMarketplace.FORMAT_FIXEDPRICE : listingItem.Listing.Format;

                EbayEntry entry = currentEntries.FirstOrDefault(p => p.IsValid() && p.GetFormat().Equals(format));

                if (entry != null)
                {
                    entry.Code = listingItem.Listing.Code;

                    entry.SetFormat(listingItem.Listing.Format, listingItem.Listing.Duration, (bool)listingItem.Listing.IsVariation);

                    if (decimal.Compare(entry.P.Value, listingItem.Price) == 0 && entry.GetUpdateFlags().Count == 0)
                    {
                        if (entry.DisplayQty.HasValue || listingItem.DisplayQuantity.HasValue)
                        {
                            if (entry.Q == listingItem.AvailableQuantity && entry.DisplayQty == listingItem.DisplayQuantity)
                            {
                                entry.Status = StatusCode.Completed;
                            }
                        }
                        else if(entry.Q == listingItem.Quantity)
                        {
                            entry.Status = StatusCode.Completed;
                        }
                        
                    }
                    else
                    {
                        entry.Message = string.Format("({0}|{1}|{2})", listingItem.FormatCode, listingItem.Quantity, Math.Round(listingItem.Price, 2));
                    }

                    currentEntries.Remove(entry);
                }
                else
                {
                    entry = currentEntries.FirstOrDefault(p => p.IsValid() == false);

                    if (entry != null)
                    {
                        entry.Message = "modified by program";
                        currentEntries.Remove(entry);
                    }
                    else
                    {
                        entry = new EbayEntry();
                        entry.Brand = item.SubDescription1;
                        entry.ClassName = item.ClassName;
                        entry.Sku = item.ItemLookupCode;
                        entry.Message = "added by program";

                        entry.ParentRowIndex = group.First().RowIndex;

                        _addedEntries.Add(entry);
                    }


                    if (listingItem.DisplayQuantity.HasValue && listingItem.AvailableQuantity.HasValue)
                    {
                        entry.DisplayQty = listingItem.DisplayQuantity;
                        entry.Q = listingItem.AvailableQuantity;
                    }
                    else
                    {
                        entry.Q = listingItem.Quantity;
                    }

                    entry.Code = listingItem.Listing.Code;
                    entry.SetFormat(listingItem.Listing.Format, listingItem.Listing.Duration, (bool)listingItem.Listing.IsVariation);
                    entry.P = listingItem.Price;
                    
                    
                    entry.Title = listingItem.Listing.Title;
                    entry.FullDescription = listingItem.Listing.FullDescription;               
                    entry.Status = StatusCode.Completed;
                }
            }

            foreach (var entry in currentEntries)
            {
                EbayListing listing = _dataContext.EbayListings.SingleOrDefault(p => p.Sku.Equals(item.ClassName) && p.Status.Equals(EbayMarketplace.STATUS_ACTIVE) && p.Marketplace.Code.Equals(_marketplaceCode));

                if (listing != null)
                {
                    if (!entry.IsValid())
                    {
                        entry.Code = listing.Code;
                        entry.Title = listing.Title;
                        entry.FullDescription = listing.FullDescription;
                        entry.SetFormat(listing.Format, listing.Duration, (bool)listing.IsVariation);
                        entry.Message = "modified by program";
                    }
                    else if (entry.GetFormat().Equals(EbayMarketplace.FORMAT_FIXEDPRICE))
                    {
                        entry.Code = listing.Code;
                        entry.SetFormat(listing.Format, listing.Duration, (bool)listing.IsVariation);
                        entry.Message = "modified by program";
                    } 
                }
            }
        }

        private void Validate(Item item, EbayEntry entry)
        {
            if (entry.GetFormat().Equals(EbayMarketplace.FORMAT_AUCTION))
            {
                if (entry.DisplayQty.HasValue)
                {
                    entry.Message = "cannot set display qty for auctions";
                    entry.Status = StatusCode.Error;
                }

                if (entry.Q > 1)
                {
                    entry.Message = "auction max qty is 1";
                    entry.Status = StatusCode.Error;
                }
            }

            //new listing validation
            if (string.IsNullOrWhiteSpace(entry.Code))
            {
                if (entry.StartDate != null && entry.StartDate < DateTime.UtcNow)
                {
                    entry.Message = "cannot schedule in the past";
                    entry.Status = StatusCode.Error;
                }

                if (string.IsNullOrWhiteSpace(entry.FullDescription))
                {
                    entry.Message = "full description required";
                    entry.Status = StatusCode.Error;
                }

                if (!entry.Q.HasValue || !entry.P.HasValue)
                {
                    entry.Message = "price and quantity required";
                    entry.Status = StatusCode.Error;
                }

                if (entry.Q < 1)
                {
                    entry.Message = "quantity must be greater than zero";
                    entry.Status = StatusCode.Error;
                }

                if (string.IsNullOrWhiteSpace(entry.Title) || entry.Title.Count() < 6)
                {
                    entry.Message = "title required";
                    entry.Status = StatusCode.Error;
                }
            }

            if (entry.P < item.Cost)
            {
                entry.Message = "price must be greater than cost";
                entry.Status = StatusCode.Error;
            }

            if (entry.Q  > item.QtyAvailable)
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
