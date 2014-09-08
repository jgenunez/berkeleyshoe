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
using System.Data.Objects;
using BerkeleyEntities.Amazon;

namespace BerkeleyEntities.Amazon
{
    public delegate void PublishingResultHandler(ResultArgs e);

    public class Publisher
    {
        private berkeleyEntities _dataContext;
        private AmznMarketplace _marketplace;
        private ListingMapper _listingMapper;

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

        private List<AmznListingItem> _productFeedCompleted = new List<AmznListingItem>();
        private List<FeedSubmissionInfo> _waitingProcessingResult = new List<FeedSubmissionInfo>();
        private List<FeedSubmissionInfo> _completedProcessingResult = new List<FeedSubmissionInfo>();
        private Dictionary<string, AmazonEnvelope> _envelopes = new Dictionary<string,AmazonEnvelope>();

        

        public Publisher(berkeleyEntities dataContext, AmznMarketplace marketplace)
        {
            _dataContext = dataContext;
            _marketplace = marketplace;
            _listingMapper = new ListingMapper(dataContext, marketplace);
        }

        public void Publish()
        {
            var added = _dataContext.ObjectStateManager.GetObjectStateEntries(EntityState.Added).Select(p => p.Entity).OfType<AmznListingItem>();

            var priceModified = _dataContext.ObjectStateManager.GetObjectStateEntries(EntityState.Modified)
                .Where(p => p.GetModifiedProperties().Contains("Price")).Select(p => p.Entity).OfType<AmznListingItem>();

            var qtyModified = _dataContext.ObjectStateManager.GetObjectStateEntries(EntityState.Modified)
                .Where(p => p.GetModifiedProperties().Contains("Quantity")).Select(p => p.Entity).OfType<AmznListingItem>();

            SubmitFeed(_listingMapper.BuildProductData(added));
            SubmitFeed(_listingMapper.BuildInventoryData(qtyModified));
            SubmitFeed(_listingMapper.BuildPriceData(priceModified));

            WaitForProcessingResult();

            SubmitFeed(_listingMapper.BuildInventoryData(_productFeedCompleted));
            SubmitFeed(_listingMapper.BuildPriceData(_productFeedCompleted));
            SubmitFeed(_listingMapper.BuildRelationshipData(_productFeedCompleted));

            _productFeedCompleted.Clear();

            WaitForProcessingResult();
        }

        public void Republish(IEnumerable<AmazonEnvelope> envelopes)
        {
            foreach (var envelope in envelopes)
            {
                SubmitFeed(envelope);
            }

            WaitForProcessingResult();

            SubmitFeed(_listingMapper.BuildInventoryData(_productFeedCompleted));
            SubmitFeed(_listingMapper.BuildPriceData(_productFeedCompleted));
            SubmitFeed(_listingMapper.BuildRelationshipData(_productFeedCompleted));

            _productFeedCompleted.Clear();

            WaitForProcessingResult();
        }

        public event PublishingResultHandler Result;

        private void WaitForProcessingResult()
        {
            while (AnyPendingSubmission)
            {
                Thread.Sleep(20000);

                GetFeedSubmissionListRequest request = new GetFeedSubmissionListRequest();
                request.FeedSubmissionIdList = new IdList();
                request.Merchant = _marketplace.MerchantId;
                request.FeedSubmissionIdList.Id.AddRange(_waitingProcessingResult.Select(p => p.FeedSubmissionId));

                GetFeedSubmissionListResponse response = _marketplace.GetMWSClient().GetFeedSubmissionList(request);

                foreach (FeedSubmissionInfo updatedSubmission in response.GetFeedSubmissionListResult.FeedSubmissionInfo)
                {
                    FeedSubmissionInfo submission = _waitingProcessingResult.Single(p => p.FeedSubmissionId.Equals(updatedSubmission.FeedSubmissionId));
                    submission.FeedProcessingStatus = updatedSubmission.FeedProcessingStatus;
                    submission.SubmittedDate = updatedSubmission.SubmittedDate;
                    submission.CompletedProcessingDate = updatedSubmission.CompletedProcessingDate;
                    submission.StartedProcessingDate = updatedSubmission.StartedProcessingDate;
                }

                var completed = _waitingProcessingResult.Where(p => p.FeedProcessingStatus.Equals(STATUS_DONE)).ToList();

                foreach (var submission in completed)
                {
                    HandleProcessingResult(submission);
                    _completedProcessingResult.Add(submission);
                    _waitingProcessingResult.Remove(submission);
                }
            }
        }

        private void HandleProcessingResult(FeedSubmissionInfo submission)
        {
            ProcessingReport processingReport = GetProcessingReport(submission.FeedSubmissionId);
            AmazonEnvelope envelope = _envelopes[submission.FeedSubmissionId];
            
            if (processingReport.ProcessingSummary.MessagesProcessed.Equals("0"))
            {
                throw new InvalidOperationException("Error processing feed: " + submission.FeedSubmissionId);
            }

            if (processingReport.Result != null)
            {
                foreach (var result in processingReport.Result)
                {
                    var msg = envelope.Message.Single(p => p.MessageID.Equals(result.MessageID));
                    msg.ProcessingResult = result;
                } 
            }

            var completed = envelope.Message.Where(p => p.ProcessingResult == null || p.ProcessingResult.ResultCode.Equals(ProcessingReportResultResultCode.Warning)).ToList();

            switch (submission.FeedType)
            {
                case PRODUCT_DATA:
                    _productFeedCompleted.AddRange(_listingMapper.SaveProductDataChanges(completed)); break;
                case INVENTORY_DATA:
                    _listingMapper.SaveInventoryDataChanges(completed); break;
                case PRICE_DATA:
                    _listingMapper.SavePriceDataChanges(completed); break;
                case RELATIONSHIP_DATA:
                    break;
            }

            if (this.Result != null)
            {
                this.Result(new ResultArgs() { Envelope = envelope });
            }
        }

        private void SubmitFeed(AmazonEnvelope envelope)
        {
            if (envelope.Message.Count() == 0)
            {
                return;
            }

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

            _envelopes.Add(info.FeedSubmissionId, envelope);
            _waitingProcessingResult.Add(info);
        }

        private bool AnyPendingSubmission 
        { 
            get 
            {
                return _waitingProcessingResult.Any(p =>
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

    public class ResultArgs
    {
        public AmazonEnvelope Envelope { get; set; }
    }
}
