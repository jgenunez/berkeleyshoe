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
using WorkbookPublisher.View;
using System.Windows.Data;

namespace WorkbookPublisher
{
    
    public class AmznPublisher
    {
        private berkeleyEntities _dataContext = new berkeleyEntities();
        private Publisher _publisher;
        private AmznMarketplace _marketplace;

        private RelayCommand _publish;

        public AmznPublisher(int marketplaceID, IEnumerable<AmznEntry> entries)
        {
            _marketplace = _dataContext.AmznMarketplaces.Single(p => p.ID == marketplaceID);
            _publisher = new Publisher(_dataContext, _marketplace);
            this.Entries = entries.ToList();
            this.CanPublish = true;
        }

        public string Header { get { return _marketplace.Code; } }

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

            await _publisher.Publish();

            //string validCount = this.Entries.Where(p => p.Completed).Count().ToString();
            //string total = this.Entries.Count.ToString();

            this.CanPublish = true;

            // string.Format(" {0} / {1} ", validCount, total);
        }

        private async void Republish()
        {
            
            var envelopeGroups = _publisher.WaitingRepublishing.GroupBy(p => p.MessageType);


            foreach (var group in envelopeGroups)
            {
                switch (group.Key)
                {
                    case AmazonEnvelopeMessageType.Product:
                        ShoesDataWindow productDataForm = new ShoesDataWindow();
                        productDataForm.DataContext = CollectionViewSource.GetDefaultView(group.ToList().SelectMany(p => p.Message));
                        productDataForm.ShowDialog(); break;
                }
            }
        }

        

        
    }
}
