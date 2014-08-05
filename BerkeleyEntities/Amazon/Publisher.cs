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

        

        public Publisher(berkeleyEntities dataContext, AmznMarketplace marketplace)
        {
            _dataContext = dataContext;
            _marketplace = marketplace;

            _listingMapper = new ListingMapper(dataContext, marketplace);

            this.SubmittedEnvelopes = new Dictionary<string, AmazonEnvelope>();
            this.Submissions = new Queue<FeedSubmissionInfo>();
        }

        public void Publish()
        {
            PublishProductData();

            PollSubmissionStatus();

            PublishInventoryData();
            PublishPriceData();
            PublishRelationshipData();

            PollSubmissionStatus();

            _dataContext.SaveChanges();
        }

        private void PublishProductData()
        {
            var listingItems = _dataContext.ObjectStateManager.GetObjectStateEntries(EntityState.Added).Select(p => p.Entity).OfType<AmznListingItem>();

            List<AmazonEnvelopeMessage> messages = new List<AmazonEnvelopeMessage>();

            int currentMsg = 1;

            foreach (AmznListingItem listingItem in listingItems.Where(p => p.Item.ItemClass == null))
            {
                Product product = _listingMapper.MapToProductDto(listingItem);

                messages.Add(BuildMessage(product, currentMsg));

                currentMsg++;
            }

            var classGroups = listingItems.Where(p => p.Item.ItemClass != null).GroupBy(p => p.Item.ItemClass);

            foreach (var classGroup in classGroups)
            {
                string title = "";

                Product product = _listingMapper.MapToParentProductDto(classGroup.Key, title);

                messages.Add(BuildMessage(product, currentMsg));

                currentMsg++;

                foreach (AmznListingItem listingItem in classGroup)
                {
                    product = _listingMapper.MapToProductDto(listingItem);

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

            var classGroups = listingItems.Where(p => p.Item.ItemClass != null).GroupBy(p => p.Item.ItemClass);

            foreach (var classGroup in classGroups)
            {
                Relationship relationship = _listingMapper.MapToRelationshipDto(classGroup.Key, _marketplace.ID);

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
            this.Submissions.Enqueue(info);
        }

        private void PollSubmissionStatus()
        {
            while (AnyPendingSubmission)
            {
                Thread.Sleep(20000);

                GetFeedSubmissionListRequest request = new GetFeedSubmissionListRequest();
                request.FeedSubmissionIdList = new IdList();
                request.Merchant = _marketplace.MerchantId;
                request.FeedSubmissionIdList.Id.AddRange(this.Submissions.Select(p => p.FeedSubmissionId));

                GetFeedSubmissionListResponse response = _marketplace.GetMWSClient().GetFeedSubmissionList(request);

                foreach (FeedSubmissionInfo updatedSubmission in response.GetFeedSubmissionListResult.FeedSubmissionInfo)
                {
                    FeedSubmissionInfo submission = this.Submissions.Single(p => p.FeedSubmissionId.Equals(updatedSubmission.FeedSubmissionId));
                    submission.FeedProcessingStatus = updatedSubmission.FeedProcessingStatus;
                    submission.SubmittedDate = updatedSubmission.SubmittedDate;
                    submission.CompletedProcessingDate = updatedSubmission.CompletedProcessingDate;
                    submission.StartedProcessingDate = updatedSubmission.StartedProcessingDate;
                }

                while (this.Submissions.Peek().FeedProcessingStatus.Equals(STATUS_DONE))
                {
                    HandleProcessingResult(this.Submissions.Dequeue());
                }
            }
        }

        private bool AnyPendingSubmission 
        { 
            get 
            {
                return this.Submissions.Any(p =>
                    !p.FeedProcessingStatus.Equals(Publisher.STATUS_CANCELLED) &&
                    !p.FeedProcessingStatus.Equals(Publisher.STATUS_DONE));
            } 
        }

        public Queue<FeedSubmissionInfo> Submissions { get; set; }

        public Dictionary<string, AmazonEnvelope> SubmittedEnvelopes { get; set; }
     
        private void HandleProcessingResult(FeedSubmissionInfo submission)
        {
            ProcessingReport processingReport = GetProcessingReport(submission.FeedSubmissionId);

            if (processingReport.ProcessingSummary.MessagesProcessed.Equals("0"))
            {
                throw new InvalidOperationException("Error processing feed: " + submission.FeedSubmissionId);
            }

            AmazonEnvelope envelope = this.SubmittedEnvelopes[submission.FeedSubmissionId];
            if (processingReport.Result != null)
            {
                var changedEntities = _dataContext.ObjectStateManager
                    .GetObjectStateEntries(EntityState.Added | EntityState.Modified).Select(p => p.Entity).OfType<AmznListingItem>();

                foreach (ProcessingReportResult result in processingReport.Result)
                {
                    AmazonEnvelopeMessage msg = envelope.Message.SingleOrDefault(p => p.MessageID.Equals(result.MessageID));

                    if (msg == null)
                        continue;

                    msg.ProcessingResult = result;

                    if (submission.FeedType.Equals(PRODUCT_DATA))
                    {
                        Product product = (Product)msg.Item;

                        if (!_dataContext.ItemClasses.Any(p => p.ItemLookupCode.Equals(product.SKU)))
                        {
                            AmznListingItem listingItem = changedEntities.Single(p => p.Item.ItemLookupCode.Equals(product.SKU));
                            listingItem.ProductData = product;
                            _dataContext.Detach(listingItem);
                        }    
                    }
                    else if (submission.FeedType.Equals(PRICE_DATA))
                    {
                        string sku = ((Price)msg.Item).SKU;
                        AmznListingItem listingItem = changedEntities.Single(p => p.Item.ItemLookupCode.Equals(sku));
                        listingItem.Price = 0;
                    }
                    else if (submission.FeedType.Equals(INVENTORY_DATA))
                    {
                        string sku = ((Inventory)msg.Item).SKU;
                        AmznListingItem listingItem = changedEntities.Single(p => p.Item.ItemLookupCode.Equals(sku));
                        listingItem.Quantity = 0;
                    }
                } 
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
