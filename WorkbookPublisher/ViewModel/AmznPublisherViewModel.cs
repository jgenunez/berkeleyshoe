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
using System.ComponentModel;
using System.Windows.Data;

namespace WorkbookPublisher
{
    
    public class AmznPublisherViewModel
    {
        private berkeleyEntities _dataContext = new berkeleyEntities();
        private List<AmazonEnvelope> _needUserInput = new List<AmazonEnvelope>();
        private Dictionary<AmznListingItem, AmznEntry> _targetListings = new Dictionary<AmznListingItem, AmznEntry>();
        private Publisher _publisher;
        private AmznMarketplace _marketplace;
        private RelayCommand _publish;

        public AmznPublisherViewModel(int marketplaceID, IEnumerable<AmznEntry> entries)
        {
            this.Entries = entries.ToList();
            this.CanPublish = true;

            _marketplace = _dataContext.AmznMarketplaces.Single(p => p.ID == marketplaceID);
            _publisher = new Publisher(_dataContext, _marketplace);
            _publisher.Error += Publisher_Error;

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
            var envelopeGroups = _needUserInput.GroupBy(p => p.MessageType);

            foreach (var group in envelopeGroups)
            {
                var msgs = group.SelectMany(p => p.Message);

                switch (group.Key)
                {
                    case AmazonEnvelopeMessageType.Product :
                        RepublishDataWindow republishForm = new RepublishDataWindow();
                        republishForm.DataContext = CollectionViewSource.GetDefaultView(msgs);
                        republishForm.ShowDialog();
                        _publisher.Republish(group.ToList());
                        break;

                }
            }
        }

        private void Publisher_Error(ErrorArgs e)
        {
            var errors = e.Envelope.Message.Where(p => p.ProcessingResult != null && p.ProcessingResult.Equals(ProcessingReportResultResultCode.Error));

            AmazonEnvelope newEnvelope = new AmazonEnvelope();
            newEnvelope.MessageType = e.Envelope.MessageType;
            newEnvelope.Header = e.Envelope.Header;
            newEnvelope.MarketplaceName = e.Envelope.MarketplaceName;

            List<AmazonEnvelopeMessage> newMsgs = new List<AmazonEnvelopeMessage>();

            int currentMsg = 1;

            foreach(AmazonEnvelopeMessage msg in errors)
            {
                string sku = msg.Item.GetType().GetProperty("SKU").GetValue(msg.Item) as string;

                newMsgs.Add(
                    new AmazonEnvelopeMessage() 
                    { 
                        Item = msg.Item, MessageID = currentMsg.ToString(), OperationType = msg.OperationType, 
                        OperationTypeSpecified = msg.OperationTypeSpecified, ProcessingResult = msg.ProcessingResult
                    });


                AmznListingItem listingItem = _dataContext.AmznListingItems
                    .Single(p => p.Item.ItemLookupCode.Equals(sku) && p.MarketplaceID == _marketplace.ID && p.IsActive);

                AmznEntry entry = _targetListings[listingItem];

                var existingErrors = entry.Message.Split(new Char[1] { '|' }).ToList();
                existingErrors.Add(msg.ProcessingResult.ResultDescription);

                entry.Message = string.Join("|", existingErrors.Distinct());
                entry.Completed = false;
            }

            newEnvelope.Message = newMsgs.ToArray();

            _needUserInput.Add(newEnvelope);
        }

        private void UpdateCompetedStatus()
        {
            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                foreach (AmznEntry entry in this.Entries)
                {
                    AmznListingItem listingItem = _dataContext.AmznListingItems.SingleOrDefault(p => p.IsActive && p.MarketplaceID == _marketplace.ID && p.Item.ItemLookupCode.Equals(entry.Sku));

                    if (listingItem != null && listingItem.Quantity == entry.Q)
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

    public class AmznEntry : INotifyPropertyChanged
    {
        private bool _completed;
        private string _message;

        public uint RowIndex { get; set; }

        public string Brand { get; set; }
        public string ClassName { get; set; }
        public string Sku { get; set; }
        public int Q { get; set; }
        public decimal P { get; set; }
        public string Title { get; set; }
        public string Condition { get; set; }

        public bool Completed
        {
            get { return _completed; }
            set 
            {
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Completed"));
                }

                _completed = value; 
            }
        }
        public string Message
        {
            get { return _message; }
            set 
            {
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Message")); 
                }
                
                _message = value; 
            }
        }

        public string Status
        {
            get
            {
                if (this.Completed)
                {
                    return "completed";
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(this.Message))
                    {
                        return "waiting";
                    }
                    else
                    {
                        return "error";
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
