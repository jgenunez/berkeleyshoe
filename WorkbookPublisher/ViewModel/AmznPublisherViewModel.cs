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
using AmazonServices.Mappers;

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
            var pendingEntries = entries.Where(p => p.Status.Equals(StatusCode.Pending));

            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                AmznMarketplace marketplace = dataContext.AmznMarketplaces.Single(p => p.Code.Equals(_marketplaceCode));

                foreach (AmznEntry entry in entries)
                {
                    Item item = dataContext.Items.SingleOrDefault(p => p.ItemLookupCode.Equals(entry.Sku));

                    if (item == null)
                    {
                        entry.Message = "sku not found";
                        entry.Status = StatusCode.Error;
                        continue;
                    }

                    entry.ClassName = item.ClassName;
                    entry.Brand = item.SubDescription1;

                    if (item.Notes != null)
                    {
                        if (item.Notes.Contains("PRE") || item.Notes.Contains("NWB") || item.Notes.Contains("NWD"))
                        {
                            entry.Message = "only new products allowed";
                            entry.Status = StatusCode.Error;
                        }
                    }

                    AmznListingItem listingItem = item.AmznListingItems.SingleOrDefault(p => p.IsActive && p.MarketplaceID == marketplace.ID);

                    if (listingItem != null && listingItem.Quantity == entry.Q)
                    {
                        entry.Status = StatusCode.Completed;
                    }

                    

                    if(entry.Status.Equals(StatusCode.Pending))
                    {
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
        private berkeleyEntities _dataContext;
        private AmznMarketplace _marketplace;
        private Publisher _publisher;

        private string _marketplaceCode;

        public List<AmznEntry> _processingEntries = new List<AmznEntry>();
        private Queue<AmazonEnvelope> _pendingResubmission = new Queue<AmazonEnvelope>();

        

        public AmznPublishCommand(string marketplaceCode)
        {
            _marketplaceCode = marketplaceCode;
        }

        private void Publisher_Result(ResultArgs e)
        {
            switch (e.Envelope.MessageType)
            {
                case AmazonEnvelopeMessageType.Product:
                    HandleProductFeedResult(e.Envelope); break;

                case AmazonEnvelopeMessageType.Price:
                    HandlePriceFeedResult(e.Envelope); break;

                case AmazonEnvelopeMessageType.Inventory:
                    HandleInventoryFeedResult(e.Envelope); break;

                case AmazonEnvelopeMessageType.Relationship :
                    HandleRelationshipFeedResult(e.Envelope); break;
            }
        }

        private void HandleRelationshipFeedResult(AmazonEnvelope envelope)
        {
            foreach (AmazonEnvelopeMessage msg in envelope.Message)
            {
                string sku = ((Relationship)msg.Item).ParentSKU;

                var entries = _processingEntries.Where(p => p.ClassName.Equals(sku));

                if (msg.ProcessingResult != null && msg.ProcessingResult.ResultCode.Equals(ProcessingReportResultResultCode.Error))
                {
                    foreach (AmznEntry entry in entries)
                    {
                        entry.RelationshipFeedStatus = StatusCode.Error;
                        entry.Message = msg.ProcessingResult.ResultDescription;

                        entry.UpdateStatus();
                    }
                }
                else
                {
                    foreach (AmznEntry entry in entries)
                    {
                        entry.RelationshipFeedStatus = StatusCode.Completed;

                        entry.UpdateStatus();
                    }
                }
            }
        }

        private void HandleInventoryFeedResult(AmazonEnvelope envelope)
        {
            foreach (AmazonEnvelopeMessage msg in envelope.Message)
            {
                string sku = ((Inventory)msg.Item).SKU;

                AmznEntry entry = _processingEntries.Single(p => p.Sku.Equals(sku));

                if (msg.ProcessingResult != null && msg.ProcessingResult.ResultCode.Equals(ProcessingReportResultResultCode.Error))
                {
                    entry.InventoryFeedStatus = StatusCode.Error;
                    entry.Message = msg.ProcessingResult.ResultDescription;
                }
                else
                {
                    entry.InventoryFeedStatus = StatusCode.Completed;
                }

                entry.UpdateStatus();
            }
        }

        private void HandlePriceFeedResult(AmazonEnvelope envelope)
        {
            foreach (AmazonEnvelopeMessage msg in envelope.Message)
            {
                string sku = ((Price)msg.Item).SKU;

                AmznEntry entry = _processingEntries.Single(p => p.Sku.Equals(sku));

                if (msg.ProcessingResult != null && msg.ProcessingResult.ResultCode.Equals(ProcessingReportResultResultCode.Error))
                {
                    entry.PriceFeedStatus = StatusCode.Error;
                    entry.Message = msg.ProcessingResult.ResultDescription;
                }
                else
                {
                    entry.PriceFeedStatus = StatusCode.Completed;
                }

                entry.UpdateStatus();
            }
        }

        private void HandleProductFeedResult(AmazonEnvelope envelope)
        {
            List<AmazonEnvelopeMessage> newMsgs = new List<AmazonEnvelopeMessage>();

            int currentMsg = 1;

            foreach (AmazonEnvelopeMessage msg in envelope.Message)
            {
                bool hasError = false;

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


                    hasError = true;

                    currentMsg++;
                }

                string sku = ((Product)msg.Item).SKU;

                if (_processingEntries.Any(p => p.Sku.Equals(sku)))
                {
                    AmznEntry entry = _processingEntries.Single(p => p.Sku.Equals(sku));

                    if (hasError)
                    {
                        entry.ProductFeedStatus = StatusCode.Error;
                    }
                    else
                    {
                        entry.ProductFeedStatus = StatusCode.Completed;
                    }

                    entry.UpdateStatus();
                }
                else if (_processingEntries.Any(p => p.ClassName.Equals(sku)))
                {
                    var entries = _processingEntries.Where(p => p.ClassName.Equals(sku));

                    foreach (AmznEntry entry in entries)
                    {
                        if (hasError)
                        {
                            entry.ParentProductFeedStatus = StatusCode.Error;
                            entry.Message = msg.ProcessingResult.ResultDescription;
                        }
                        else
                        {
                            entry.ParentProductFeedStatus = StatusCode.Completed;
                        }

                        entry.UpdateStatus();
                    }
                }
            }

            if (newMsgs.Count > 0)
            {
                AmazonEnvelope newEnvelope = new AmazonEnvelope();
                newEnvelope.MessageType = envelope.MessageType;
                newEnvelope.Header = envelope.Header;
                newEnvelope.MarketplaceName = envelope.MarketplaceName;
                newEnvelope.Message = newMsgs.ToArray();

                _pendingResubmission.Enqueue(newEnvelope);
            }
        }

        public async override void Publish(IEnumerable<Entry> entries)
        {
            using (_dataContext = new berkeleyEntities())
            {
                _dataContext.MaterializeAttributes = true;
                _marketplace = _dataContext.AmznMarketplaces.Single(p => p.Code.Equals(_marketplaceCode));
                _publisher = new Publisher(_dataContext, _marketplace);
                _publisher.Result += Publisher_Result;

                await Task.Run(() => PublishEntries(entries));

                while (_pendingResubmission.Count > 0)
                {
                    AmazonEnvelope envelope = _pendingResubmission.Dequeue();

                    RepublishDataWindow republishForm = new RepublishDataWindow();

                    republishForm.DataContext = CollectionViewSource.GetDefaultView(envelope.Message);

                    republishForm.ShowDialog();

                    await Task.Run(() => Republish(envelope));
                } 
            }

            this.RaisePublishCompleted();
        }

        private void PublishEntries(IEnumerable<Entry> entries)
        {
            foreach (AmznEntry entry in entries)
            {
                AmznListingItem listingItem = _dataContext.AmznListingItems.Include("Item")
                    .SingleOrDefault(p => p.IsActive && p.MarketplaceID == _marketplace.ID && p.Item.ItemLookupCode.Equals(entry.Sku));

                if (listingItem == null)
                {
                    listingItem = new AmznListingItem();

                    listingItem.Item = _dataContext.Items.Single(p => p.ItemLookupCode.Equals(entry.Sku));
                    listingItem.Title = entry.Title;

                    entry.ProductFeedStatus = StatusCode.Processing;
                    entry.RelationshipFeedStatus = StatusCode.Processing;
                }

                if (listingItem.Quantity != entry.Q)
                {
                    listingItem.Quantity = entry.Q;

                    entry.InventoryFeedStatus = StatusCode.Processing;
                }

                if (listingItem.Price != entry.P)
                {
                    listingItem.Price = entry.P;

                    entry.PriceFeedStatus = StatusCode.Processing;
                }

                entry.UpdateStatus();

                _processingEntries.Add(entry);
            }

            _publisher.Publish(); 

        }

        public void Republish(AmazonEnvelope envelope)
        {
            foreach (var msg in envelope.Message)
            {
                string sku = ((Product)msg.Item).SKU;

                if (_processingEntries.Any(p => p.Sku.Equals(sku)))
                {
                    AmznEntry entry = _processingEntries.Single(p => p.Sku.Equals(sku));
                    entry.ProductFeedStatus = StatusCode.Processing;
                    entry.UpdateStatus();
                }
                else if (_processingEntries.Any(p => p.ClassName.Equals(sku)))
                {
                    var entries = _processingEntries.Where(p => p.ClassName.Equals(sku));

                    foreach (AmznEntry entry in entries)
                    {
                        entry.Status = StatusCode.Processing;
                        entry.UpdateStatus();
                    }
                }
            }

            _publisher.Republish(new List<AmazonEnvelope>() { envelope }); 

        }

    }

}
