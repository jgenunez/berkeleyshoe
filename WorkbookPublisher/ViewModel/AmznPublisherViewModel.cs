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
using BerkeleyEntities.Amazon;

namespace WorkbookPublisher
{
    
    public class AmznPublisherViewModel
    {
        private berkeleyEntities _dataContext = new berkeleyEntities();
        private Publisher _publisher;
        private AmznMarketplace _marketplace;

        private RelayCommand _publish;

        public AmznPublisherViewModel(int marketplaceID, IEnumerable<AmznEntry> entries)
        {
            this.Entries = entries.ToList();
            this.CanPublish = true;

            _marketplace = _dataContext.AmznMarketplaces.Single(p => p.ID == marketplaceID);
            _publisher = new Publisher(_dataContext, _marketplace);

            UpdateCompetedStatus();
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

        private void PublishAsync()
        {
            this.CanPublish = false;

            foreach (AmznEntry entry in this.Entries.Where(p => p.Completed == false))
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

            _publisher.Publish();

            string validCount = this.Entries.Where(p => p.Completed).Count().ToString();
            string total = this.Entries.Count.ToString();

            this.CanPublish = true;

            // string.Format(" {0} / {1} ", validCount, total);
        }

        private void Republish()
        {
            var envelopeGroups = _publisher.WaitingRepublishing.GroupBy(p => p.MessageType);

            foreach (var group in envelopeGroups)
            {
                var msgs = group.SelectMany(p => p.Message);

                switch (group.Key)
                {
                    case AmazonEnvelopeMessageType.Product :
                        RepublishDataWindow republishForm = new RepublishDataWindow();
                        republishForm.DataContext = msgs.First().ProcessingResult.ResultDescription;
                        break;

                }
            }
        }

        private void UpdateCompetedStatus()
        {
            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                foreach (AmznEntry entry in this.Entries)
                {
                    AmznListingItem listingItem = _dataContext.AmznListingItems.SingleOrDefault(p => p.IsActive && p.MarketplaceID == _marketplace.ID && p.Item.ItemLookupCode.Equals(entry.Sku));

                    if (listingItem != null && listingItem.Quantity == entry.Q && listingItem.Price == entry.P)
                    {
                        entry.Completed = true;
                    }
                    else
                    {
                        entry.Completed = false;
                    }
                }
            }
        }

    }

    public class AmznEntry
    {
        public uint RowIndex { get; set; }

        public string Brand { get; set; }
        public string ClassName { get; set; }
        public string Sku { get; set; }
        public int Q { get; set; }
        public decimal P { get; set; }
        public string Title { get; set; }
        public string Condition { get; set; }
        public bool Completed { get; set; }
        public string Status { get; set; }
    }
}
