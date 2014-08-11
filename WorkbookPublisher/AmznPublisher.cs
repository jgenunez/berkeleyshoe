using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BerkeleyEntities;
using BerkeleyEntities.Amazon;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Input;
using Microsoft.TeamFoundation.MVVM;

namespace WorkbookPublisher
{
    
    public class AmznPublisher
    {
        private berkeleyEntities _dataContext = new berkeleyEntities();
        private AmznMarketplace _marketplace;

        private RelayCommand _publish;

        public AmznPublisher(int marketplaceID, IEnumerable<AmznEntry> entries)
        {
            _marketplace = _dataContext.AmznMarketplaces.Single(p => p.ID == marketplaceID);
            this.Entries = entries.ToList();
            this.CanPublish = true;
        }

        public List<AmznEntry> Entries { get; set; }

        public bool CanPublish { get; set; }

        public ICommand Publish
        {
            get
            {
                if (_publish == null)
                {
                    _publish = new RelayCommand(PublishAsync);
                }

                return _publish;
            }
        }

        private async void PublishAsync()
        {
            this.CanPublish = false;

            Publisher publisher = new Publisher(_dataContext, _marketplace);

            foreach (AmznEntry entry in this.Entries)
            {
                AmznListingItem listingItem = _dataContext.AmznListingItems.SingleOrDefault(p => p.IsActive && p.MarketplaceID == _marketplace.ID &&  p.Item.ItemLookupCode.Equals(entry.Sku));

                if (listingItem == null)
                {
                    listingItem = new AmznListingItem();
                    listingItem.Item = _dataContext.Items.Single(p => p.ItemLookupCode.Equals(entry.Sku));
                }

                listingItem.Quantity = entry.Q;
                listingItem.Price = entry.P;
                listingItem.Condition = entry.Condition;
                listingItem.Title = entry.Title;
            }

            publisher.Publish();

            string validCount = this.Entries.Where(p => p.IsValid).Count().ToString();
            string total = this.Entries.Count.ToString();

            this.CanPublish = true;

            // string.Format(" {0} / {1} ", validCount, total);
        }

        public string Header { get { return _marketplace.Code; } }
    }
}
