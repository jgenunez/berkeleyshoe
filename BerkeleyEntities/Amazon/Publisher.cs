using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BerkeleyEntities;
using MarketplaceWebService.Model;
using System.IO;
using MarketplaceWebService;
using System.Xml.Serialization;
using AmazonServices.Mappers;
using System.Data;
using System.Threading;

namespace BerkeleyEntities.Amazon
{
    public class Publisher
    {
        private berkeleyEntities _dataContext;
        private AmznMarketplace _marketplace;
        private ListingMapper _listingMapper;

        private string _submissionFile = AppDomain.CurrentDomain.BaseDirectory + "submissions.xml";
        private string _envelopesFile = AppDomain.CurrentDomain.BaseDirectory + "envelopes.xml";

        public const string STATUS_WAITING_ASYNCHRONOUS_REPLY = "_AWAITING_ASYNCHRONOUS_REPLY_";
        public const string STATUS_CANCELLED = "_CANCELLED_";
        public const string STATUS_DONE = "_DONE_";
        public const string STATUS_IN_PROGRESS = "_IN_PROGRESS_";
        public const string STATUS_IN_SAFETY_NET = "_IN_SAFETY_NET_";
        public const string STATUS_SUBMITTED = "_SUBMITTED_";
        public const string STATUS_UNCONFIRMED = "_UNCONFIRMED_";

        private const string PRODUCT_DATA = "_POST_PRODUCT_DATA_";
        private const string PRICE_DATA = "_POST_PRODUCT_PRICING_DATA_";
        private const string INVENTORY_DATA = "_POST_INVENTORY_AVAILABILITY_DATA_";
        private const string RELATIONSHIP_DATA = "_POST_PRODUCT_RELATIONSHIP_DATA_";

        private List<AmznListingItem> _addedListingItems = new List<AmznListingItem>();



        public Publisher(berkeleyEntities dataContext, AmznMarketplace marketplace)
        {
            _dataContext = dataContext;
            _marketplace = marketplace;

            _listingMapper = new ListingMapper(dataContext, marketplace);

            this.SubmittedEnvelopes = new Dictionary<string, AmazonEnvelope>();
            this.PendingSubmissions = new List<FeedSubmissionInfo>();
            this.Errors = new List<AmazonEnvelope>();
        }

        public void Publish()
        {
            PublishProductData();

            PollSubmissionStatus();

            PublishInventoryData();
            PublishPriceData();
            PublishRelationshipData();

            PollSubmissionStatus();
        }

        public void Republish()
        {
            foreach (AmazonEnvelope envelope in this.Errors.Where(p => p.MessageType.Equals(AmazonEnvelopeMessageType.Product)))
            {
                SubmitFeed(envelope);
            }
        }

        public List<FeedSubmissionInfo> PendingSubmissions { get; set; }

        public List<AmazonEnvelope> Errors { get; set; }

        public Dictionary<string, AmazonEnvelope> SubmittedEnvelopes { get; set; }

        private void PublishProductData()
        {
            var listingItems = _dataContext.ObjectStateManager.GetObjectStateEntries(EntityState.Added).Select(p => p.Entity).OfType<AmznListingItem>();

            List<AmazonEnvelopeMessage> messages = new List<AmazonEnvelopeMessage>();

            int currentMsg = 1;

            foreach (AmznListingItem listingItem in listingItems.Where(p => p.Item.ItemClass == null))
            {
                Product product = _listingMapper.MapToProductDto(new List<AmznListingItem>() { listingItem }).First();

                messages.Add(BuildMessage(product, currentMsg));

                currentMsg++;
            }

            var classGroups = listingItems.Where(p => p.Item.ItemClass != null).GroupBy(p => p.Item.ItemClass);

            foreach (var classGroup in classGroups)
            {
                var products = _listingMapper.MapToProductDto(classGroup.ToList());

                foreach (Product product in products)
                {
                    messages.Add(BuildMessage(product, currentMsg));

                    currentMsg++;
                }

            }

            if (messages.Count > 0)
            {
                AmazonEnvelope envelope = BuildEnvelope(AmazonEnvelopeMessageType.Product, messages);

                SubmitFeed(envelope); 
            }
        }

        private void PublishRelationshipData()
        {
            var listingItems = _dataContext.ObjectStateManager.GetObjectStateEntries(EntityState.Added).Select(p => p.Entity).OfType<AmznListingItem>();

            List<AmazonEnvelopeMessage> messages = new List<AmazonEnvelopeMessage>();

            int currentMsg = 1;

            var classGroups = listingItems.Where(p => p.Item.ItemClass != null).GroupBy(p => p.Item.ItemClass.ItemLookupCode);

            foreach (var classGroup in classGroups)
            {
                Relationship relationship = _listingMapper.MapToRelationshipDto(classGroup.ToList());

                messages.Add(BuildMessage(relationship, currentMsg));

                currentMsg++;
            }

            if (messages.Count > 0)
            {
                AmazonEnvelope envelope = BuildEnvelope(AmazonEnvelopeMessageType.Relationship, messages);

                SubmitFeed(envelope); 
            }
        }

        private void PublishInventoryData()
        {
            var entries = _dataContext.ObjectStateManager.GetObjectStateEntries(EntityState.Modified)
                .Where(p => p.GetModifiedProperties().Contains("Quantity"))
                .Select(p => p.Entity).OfType<AmznListingItem>();


            this.Errors.Where(p => p.MessageType.Equals(AmazonEnvelopeMessageType.Product));

            var updateList = entries.Concat(_dataContext.ObjectStateManager.GetObjectStateEntries(EntityState.Added).Select(p => p.Entity).Cast<AmznListingItem>());

            List<AmazonEnvelopeMessage> messages = new List<AmazonEnvelopeMessage>();

            int currentMsg = 1;

            foreach (AmznListingItem listingItem in updateList)
            {
                Inventory inventoryData = _listingMapper.MapToInventoryDto(listingItem);

                messages.Add(BuildMessage(inventoryData, currentMsg));

                currentMsg++;
            }

            if (messages.Count > 0)
            {
                AmazonEnvelope envelope = BuildEnvelope(AmazonEnvelopeMessageType.Inventory, messages);

                SubmitFeed(envelope);
            }
        }

        private void PublishPriceData()
        {
            var entries = _dataContext.ObjectStateManager.GetObjectStateEntries(EntityState.Modified)
                .Where(p => p.GetModifiedProperties().Contains("Price"))
                .Select(p => p.Entity).OfType<AmznListingItem>();

            var updateList = entries.Concat(_dataContext.ObjectStateManager.GetObjectStateEntries(EntityState.Added).Select(p => p.Entity).Cast<AmznListingItem>());

            List<AmazonEnvelopeMessage> messages = new List<AmazonEnvelopeMessage>();

            int currentMsg = 1;

            foreach (AmznListingItem listingItem in updateList)
            {
                Price priceData = _listingMapper.MapToPriceDto(listingItem);

                messages.Add(BuildMessage(priceData, currentMsg));

                currentMsg++;
            }

            if (messages.Count > 0)
            {
                AmazonEnvelope envelope = BuildEnvelope(AmazonEnvelopeMessageType.Price, messages);

                SubmitFeed(envelope); 
            }
        }

        private void PollSubmissionStatus()
        {
            while (AnyPendingSubmission)
            {
                Thread.Sleep(20000);

                GetFeedSubmissionListRequest request = new GetFeedSubmissionListRequest();
                request.FeedSubmissionIdList = new IdList();
                request.Merchant = _marketplace.MerchantId;
                request.FeedSubmissionIdList.Id.AddRange(this.PendingSubmissions.Select(p => p.FeedSubmissionId));

                GetFeedSubmissionListResponse response = _marketplace.GetMWSClient().GetFeedSubmissionList(request);

                foreach (FeedSubmissionInfo updatedSubmission in response.GetFeedSubmissionListResult.FeedSubmissionInfo)
                {
                    FeedSubmissionInfo submission = this.PendingSubmissions.Single(p => p.FeedSubmissionId.Equals(updatedSubmission.FeedSubmissionId));
                    submission.FeedProcessingStatus = updatedSubmission.FeedProcessingStatus;
                    submission.SubmittedDate = updatedSubmission.SubmittedDate;
                    submission.CompletedProcessingDate = updatedSubmission.CompletedProcessingDate;
                    submission.StartedProcessingDate = updatedSubmission.StartedProcessingDate;
                }

                var completed = this.PendingSubmissions.Where(p => p.FeedProcessingStatus.Equals(STATUS_DONE)).ToList();

                foreach (var submission in completed)
                {
                    HandleProcessingResult(submission);
                    this.PendingSubmissions.Remove(submission);
                }
            }
        }

        private void HandleProcessingResult(FeedSubmissionInfo submission)
        {
            ProcessingReport processingReport = GetProcessingReport(submission.FeedSubmissionId);
            AmazonEnvelope envelope = this.SubmittedEnvelopes[submission.FeedSubmissionId];
            
            if (processingReport.ProcessingSummary.MessagesProcessed.Equals("0"))
            {
                throw new InvalidOperationException("Error processing feed: " + submission.FeedSubmissionId);
            }

            foreach (var result in processingReport.Result)
            {
                var msg = envelope.Message.Single(p => p.MessageID.Equals(result.MessageID));

                msg.ProcessingResult = result;
            }

            var completed = envelope.Message.Where(p => p.ProcessingResult == null || p.ProcessingResult.ResultCode.Equals(ProcessingReportResultResultCode.Warning)).ToList();

            switch (submission.FeedType)
            {
                case PRODUCT_DATA:
                    SaveProductDataChanges(completed); break;
                case INVENTORY_DATA:
                    SaveInventoryDataChanges(completed); break;
                case PRICE_DATA:
                    SavePriceDataChanges(completed); break;
                case RELATIONSHIP_DATA:
                    break;
            }

            envelope.Message = envelope.Message.Where(p => p.ProcessingResult != null && p.ProcessingResult.ResultCode.Equals(ProcessingReportResultResultCode.Error)).ToArray();

            if (envelope.Message.Count() > 0)
            {
                this.Errors.Add(envelope);
            }

            this.PendingSubmissions.Remove(submission);
        }

        private void SaveProductDataChanges(IEnumerable<AmazonEnvelopeMessage> msgs)
        {
            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                foreach (var msg in msgs)
                {
                    Product product = (Product)msg.Item;

                    if (dataContext.Items.Any(p => p.ItemLookupCode.Equals(product.SKU)))
                    {
                        AmznListingItem listingItem = dataContext.AmznListingItems
                            .SingleOrDefault(p => p.Item.ItemLookupCode.Equals(product.SKU) && p.MarketplaceID == _marketplace.ID && p.IsActive);

                        if (listingItem == null)
                        {
                            listingItem = new AmznListingItem();
                            listingItem.Item = dataContext.Items.Single(p => p.ItemLookupCode.Equals(product.SKU));
                            listingItem.Marketplace = dataContext.AmznMarketplaces.Single(p => p.ID == _marketplace.ID);
                            listingItem.OpenDate = DateTime.UtcNow;
                            listingItem.LastSyncTime = DateTime.UtcNow;
                            listingItem.ASIN = "UNKNOWN";
                            listingItem.IsActive = true;
                            listingItem.Quantity = 0;
                            listingItem.Price = 0;
                            listingItem.Condition = product.Condition.ConditionType.ToString();
                            listingItem.Title = product.DescriptionData.Title;
                        }
                    } 
                }

                dataContext.SaveChanges();
            }
            
        }

        private void SavePriceDataChanges(IEnumerable<AmazonEnvelopeMessage> msgs)
        {
            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                foreach (AmazonEnvelopeMessage msg in msgs)
                {
                    Price priceData = (Price)msg.Item;

                    AmznListingItem listingItem = dataContext.AmznListingItems
                        .Single(p => p.Item.ItemLookupCode.Equals(priceData.SKU) && p.MarketplaceID == _marketplace.ID && p.IsActive);

                    listingItem.Price = priceData.StandardPrice.Value;
                }

                dataContext.SaveChanges();
            }
        }

        private void SaveInventoryDataChanges(IEnumerable<AmazonEnvelopeMessage> msgs)
        {
            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                foreach (AmazonEnvelopeMessage msg in msgs)
                {
                    Inventory inventoryData = (Inventory)msg.Item;

                    AmznListingItem listingItem = dataContext.AmznListingItems
                        .Single(p => p.Item.ItemLookupCode.Equals(inventoryData.SKU) && p.MarketplaceID == _marketplace.ID && p.IsActive);

                    listingItem.Quantity = Convert.ToInt32(inventoryData.Item);
                }

                dataContext.SaveChanges();
            }
        }

        private void SubmitFeed(AmazonEnvelope envelope)
        {
            SubmitFeedRequest request = new SubmitFeedRequest();
            request.Merchant = _marketplace.MerchantId;
            request.MarketplaceIdList = new IdList();
            request.MarketplaceIdList.Id = new List<string>(new string[] { _marketplace.MarketplaceId });
            request.FeedContent = new MemoryStream(Encoding.ASCII.GetBytes(Serialize(envelope)));
            request.FeedContent.Position = 0;
            request.ContentMD5 = MarketplaceWebServiceClient.CalculateContentMD5(request.FeedContent);

            switch (envelope.MessageType)
            {
                case AmazonEnvelopeMessageType.Product: request.FeedType = PRODUCT_DATA; break;
                case AmazonEnvelopeMessageType.Inventory: request.FeedType = INVENTORY_DATA; break;
                case AmazonEnvelopeMessageType.Price: request.FeedType = PRICE_DATA; break;
                case AmazonEnvelopeMessageType.Relationship: request.FeedType = RELATIONSHIP_DATA; break;
                default: throw new NotImplementedException("report type not supported");
            }

            SubmitFeedResponse response = _marketplace.GetMWSClient().SubmitFeed(request);

            FeedSubmissionInfo info = response.SubmitFeedResult.FeedSubmissionInfo;

            this.SubmittedEnvelopes.Add(info.FeedSubmissionId, envelope);
            this.PendingSubmissions.Add(info);
        }

        private bool AnyPendingSubmission 
        { 
            get 
            {
                return this.PendingSubmissions.Any(p =>
                    !p.FeedProcessingStatus.Equals(Publisher.STATUS_CANCELLED) &&
                    !p.FeedProcessingStatus.Equals(Publisher.STATUS_DONE));
            } 
        }

        private ProcessingReport GetProcessingReport(string submissionID)
        {
            GetFeedSubmissionResultRequest request = new GetFeedSubmissionResultRequest();
            request.FeedSubmissionId = submissionID;
            request.Merchant = _marketplace.MerchantId;

            MemoryStream stream = new MemoryStream();
            request.FeedSubmissionResult = stream;

            GetFeedSubmissionResultResponse response = _marketplace.GetMWSClient().GetFeedSubmissionResult(request);
            GetFeedSubmissionResultResult result = response.GetFeedSubmissionResultResult;

            ASCIIEncoding ascii = new ASCIIEncoding();
            String xmlinstance = ascii.GetString(stream.ToArray());
            AmazonEnvelope envelope = (AmazonEnvelope)Deserialize(typeof(AmazonEnvelope), xmlinstance);

            AmazonEnvelopeMessage msg = envelope.Message.First();

            ProcessingReport processingReport = (ProcessingReport)msg.Item;

            return processingReport;
        }

        public AmazonEnvelopeMessage BuildMessage(object item, int messageID)
        {
            AmazonEnvelopeMessage msg = new AmazonEnvelopeMessage();
            msg.OperationTypeSpecified = true;
            msg.OperationType = AmazonEnvelopeMessageOperationType.Update;
            msg.Item = item;
            msg.MessageID = messageID.ToString();

            return msg;
        }

        public AmazonEnvelope BuildEnvelope(AmazonEnvelopeMessageType msgType, IEnumerable<AmazonEnvelopeMessage> msgs)
        {
            AmazonEnvelope envelope = new AmazonEnvelope();
            envelope.MessageType = msgType;
            envelope.Message = msgs.ToArray();
            envelope.Header = new Header();
            envelope.Header.MerchantIdentifier = _marketplace.MerchantId;
            envelope.Header.DocumentVersion = "1.01";

            return envelope;
        }

        private string Serialize(object objectToSerialize)
        {
            MemoryStream mem = new MemoryStream();
            XmlSerializer ser = new XmlSerializer(objectToSerialize.GetType());
            ser.Serialize(mem, objectToSerialize);
            ASCIIEncoding ascii = new ASCIIEncoding();
            return ascii.GetString(mem.ToArray());
        }

        private object Deserialize(Type typeToDeserialize, string xmlString)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(xmlString);
            MemoryStream mem = new MemoryStream(bytes);
            XmlSerializer ser = new XmlSerializer(typeToDeserialize);
            return ser.Deserialize(mem);
        }

        //private void SaveSubmissionData()
        //{
        //    XmlSerializer serializer = new XmlSerializer(typeof(List<FeedSubmissionInfo>));
        //    serializer.Serialize(new FileStream(_submissionFile, FileMode.Create), this.Submissions);

        //    serializer = new XmlSerializer(typeof(Dictionary<string, AmazonEnvelope>));
        //    serializer.Serialize(new FileStream(_envelopesFile, FileMode.Create), this.SubmittedEnvelopes);
        //}

        //private void RetrieveSubmissionData()
        //{
        //    XmlSerializer serializer = new XmlSerializer(typeof(List<FeedSubmissionInfo>));
        //    this.Submissions = serializer.Deserialize(new StreamReader(_submissionFile)) as List<FeedSubmissionInfo>;


        //    serializer = new XmlSerializer(typeof(Dictionary<string, AmazonEnvelope>));
        //    this.SubmittedEnvelopes = serializer.Deserialize(new StreamReader(_envelopesFile)) as Dictionary<string,AmazonEnvelope>;
        //}
    }
}
