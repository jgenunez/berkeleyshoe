using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BerkeleyEntities;
using BerkeleyEntities.Amazon;
using System.Threading.Tasks;
using System.Threading;

namespace BSI_InventoryPreProcessor
{
    
    public class AmznPublisher
    {
        private berkeleyEntities _dataContext = new berkeleyEntities();
        private AmznMarketplace _marketplace;

        public AmznPublisher(int marketplaceID, IEnumerable<AmznEntry> entries)
        {
            _marketplace = _dataContext.AmznMarketplaces.Single(p => p.ID == marketplaceID);
            this.Entries = entries.ToList();
        }

        public List<AmznEntry> Entries { get; set; }

        public async Task<string> PublishAsync()
        {
            Publisher publisher = new Publisher(_dataContext, _marketplace);

            var listingItems = _marketplace.ListingItems.Where(p => p.IsActive).ToList();

            foreach (AmznEntry entry in this.Entries)
            {
                AmznListingItem listingItem = _marketplace.ListingItems.SingleOrDefault(p => p.IsActive && p.Item.ItemLookupCode.Equals(entry.Sku));

                if (listingItem == null)
                {
                    listingItem = new AmznListingItem();
                    listingItem.Item = _dataContext.Items.Single(p => p.ItemLookupCode.Equals(entry.Sku));
                }

                listingItem.Quantity = entry.Quantity;
                listingItem.Price = entry.Price;
                listingItem.Condition = entry.Condition;
                listingItem.Title = entry.Title;
            }

            publisher.Publish();

            string validCount = this.Entries.Where(p => p.IsValid).Count().ToString();
            string total = this.Entries.Count.ToString();

            return string.Format(" {0} / {1} ", validCount, total);
        }

        public string Header { get { return _marketplace.Code; } }
    }
}
