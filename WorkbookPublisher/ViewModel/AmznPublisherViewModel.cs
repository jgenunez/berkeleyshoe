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
using System.Collections.ObjectModel;
using WorkbookPublisher.ViewModel;

namespace WorkbookPublisher.ViewModel
{
    
    public class AmznPublisherViewModel : PublisherViewModel
    {
        public AmznPublisherViewModel(ExcelWorkbook workbook, string marketplaceCode) : base(workbook, marketplaceCode)
        {
            _readEntriesCommand = new AmznReadCommand(_workbook, marketplaceCode);
            _publishCommand = new AmznPublishCommand(marketplaceCode);
            _fixErrorsCommand = new FixCommand(_workbook, marketplaceCode);

            _readEntriesCommand.ReadCompleted += _publishCommand.ReadCompletedHandler;
            _publishCommand.PublishCompleted += _fixErrorsCommand.PublishCompletedHandler;
            _fixErrorsCommand.FixCompleted += _readEntriesCommand.FixCompletedHandler;
        }
    }

    public class AmznReadCommand : ReadCommand
    {
        public AmznReadCommand(ExcelWorkbook workbook, string marketplaceCode)
            : base(workbook, marketplaceCode)
        {

        }

        public override void UpdateEntries(IEnumerable<Entry> entries)
        {
            AmznMarketplace marketplace = _dataContext.AmznMarketplaces.Single(p => p.Code.Equals(_marketplaceCode));

            foreach (AmznEntry entry in entries)
            {
                Item item = _dataContext.Items.SingleOrDefault(p => p.ItemLookupCode.Equals(entry.Sku));

                if (item == null)
                {
                    entry.Message = "sku not found";
                    continue;
                }

                AmznListingItem listingItem = item.AmznListingItems.SingleOrDefault(p => p.IsActive && p.MarketplaceID == marketplace.ID);

                if (listingItem != null && listingItem.Quantity == entry.Q)
                {
                    entry.Completed = true;
                }
                else
                {
                    if (entry.Q > item.QtyAvailable)
                    {
                        entry.Message = "out of stock";
                    }

                    if (string.IsNullOrEmpty(item.GTIN))
                    {
                        entry.Message = "UPC or EAN required";
                    }

                    if (item.Department == null)
                    {
                        entry.Message = "department classification required";
                    }
                }
            }
        }

        public override Type EntryType
        {
            get { return typeof(AmznEntry); }
        }
    }

    public class AmznPublishCommand : PublishCommand
    {
        private berkeleyEntities _dataContext = new berkeleyEntities();
        private Dictionary<AmznListingItem, AmznEntry> _targetListings = new Dictionary<AmznListingItem, AmznEntry>();
        private List<AmazonEnvelope> _needUserInput = new List<AmazonEnvelope>();
        private AmznMarketplace _marketplace;
        private Publisher _publisher;

        public AmznPublishCommand(string marketplaceCode)
        {
            _dataContext.MaterializeAttributes = true;

            _marketplace = _dataContext.AmznMarketplaces.Single(p => p.Code.Equals(marketplaceCode));

            _publisher = new Publisher(_dataContext, _marketplace);

            _publisher.Result += Publisher_Result;
        }

        private void Publisher_Result(ResultArgs e)
        {
            List<AmazonEnvelopeMessage> newMsgs = new List<AmazonEnvelopeMessage>();

            int currentMsg = 1;

            foreach (AmazonEnvelopeMessage msg in e.Envelope.Message)
            {
                string sku = msg.Item.GetType().GetProperty("SKU").GetValue(msg.Item) as string;
                AmznEntry entry = _targetListings.Single(p => p.Key.Item.ItemLookupCode.Equals(sku)).Value;

                if (msg.ProcessingResult != null && msg.ProcessingResult.ResultCode.Equals(ProcessingReportResultResultCode.Error))
                {
                    newMsgs.Add(
                        new AmazonEnvelopeMessage()
                        {
                            Item = msg.Item,
                            MessageID = currentMsg.ToString(),
                            OperationType = msg.OperationType,
                            OperationTypeSpecified = msg.OperationTypeSpecified,
                            ProcessingResult = msg.ProcessingResult
                        });


                    entry.Message = msg.ProcessingResult.ResultDescription;
                    currentMsg++;
                }
                else
                {
                    entry.Completed = true;
                }
            }


            if (newMsgs.Count > 0)
            {
                AmazonEnvelope newEnvelope = new AmazonEnvelope();
                newEnvelope.MessageType = e.Envelope.MessageType;
                newEnvelope.Header = e.Envelope.Header;
                newEnvelope.MarketplaceName = e.Envelope.MarketplaceName;
                newEnvelope.Message = newMsgs.ToArray();

                _needUserInput.Add(newEnvelope);
            }
        }

        public async override void Publish(IEnumerable<Entry> entries)
        {
            foreach (AmznEntry entry in entries)
            {
                AmznListingItem listingItem = _dataContext.AmznListingItems.SingleOrDefault(p => p.IsActive && p.MarketplaceID == _marketplace.ID && p.Item.ItemLookupCode.Equals(entry.Sku));

                if (listingItem == null)
                {
                    listingItem = new AmznListingItem();
                    listingItem.Item = _dataContext.Items.Single(p => p.ItemLookupCode.Equals(entry.Sku));
                }

                listingItem.Quantity = entry.Q;
                listingItem.Price = entry.P;
                listingItem.Condition = entry.Condition;
                listingItem.Title = entry.Title;

                _targetListings.Add(listingItem, entry);
            }

            await Task.Run(() => _publisher.Publish());
        }

        private async void FixErrors()
        {
            var envelopeGroups = _needUserInput.GroupBy(p => p.MessageType).ToList();

            _needUserInput.Clear();

            foreach (var group in envelopeGroups)
            {
                var msgs = group.SelectMany(p => p.Message);

                switch (group.Key)
                {
                    case AmazonEnvelopeMessageType.Product:
                        RepublishDataWindow republishForm = new RepublishDataWindow();
                        republishForm.DataContext = CollectionViewSource.GetDefaultView(msgs);
                        republishForm.ShowDialog();
                        await Task.Run(() => _publisher.Republish(group.ToList()));
                        break;

                }
            }
        }
    }

}
