using AmazonServices;
using MarketplaceWebService;
using MarketplaceWebService.Model;
using MarketplaceWebServiceOrders.Model;
using MarketplaceWebServiceProducts.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Xml;
using System.Xml.Serialization;

namespace BerkeleyEntities.Amazon
{
    public class AmazonServices
    {
        public event PublishingResultHandler Result;

        public List<GetCompetitivePricingForASINResult> GetCompetitivePricingForAsin(int marketplaceID, IEnumerable<string> asins)
        {
            AmznMarketplace marketplace = null;

            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                marketplace = dataContext.AmznMarketplaces.Single(p => p.ID == marketplaceID);
            }

            int max = 20;

            int quota = max;

            System.Timers.Timer timer = new System.Timers.Timer(1000);

            timer.Elapsed += (sender, e) =>
            {
                if (quota < 20)
                {
                    if ((quota + 5) > max)
                    {
                        quota = max;
                    }
                    else
                    {
                        quota += 5;
                    }
                }
            };

            timer.Start();

            List<GetCompetitivePricingForASINResult> results = new List<GetCompetitivePricingForASINResult>();

            Queue<string> pending = new Queue<string>(asins);

            while (pending.Count > 0)
            {
                List<string> current = new List<string>();

                for (int i = 0; i < 20; i++)
                {
                    if (pending.Count > 0)
                    {
                        string asin = pending.Dequeue();

                        if (!current.Contains(asin))
                        {
                            current.Add(asin);
                        }
                    }
                }

                if (current.Count > 0)
                {
                    GetCompetitivePricingForASINRequest request = new GetCompetitivePricingForASINRequest();
                    request.SellerId = marketplace.MerchantId;
                    request.MarketplaceId = marketplace.MarketplaceId;
                    request.ASINList = new ASINListType();
                    request.ASINList.ASIN = current;

                    while (quota < current.Count)
                    {
                        Thread.Sleep(1000);
                    }

                    GetCompetitivePricingForASINResponse response = marketplace.GetMWSProductsClient().GetCompetitivePricingForASIN(request);

                    quota = quota - current.Count;

                    if (response.IsSetGetCompetitivePricingForASINResult())
                    {
                        foreach (var result in response.GetCompetitivePricingForASINResult)
                        {
                            results.Add(result);
                        }
                    }
                }
            }

            return results;
        }

        public List<GetLowestOfferListingsForASINResult> GetLowestOfferListingsForAsin(int marketplaceID, IEnumerable<string> asins)
        {
            AmznMarketplace marketplace = null;

            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                marketplace = dataContext.AmznMarketplaces.Single(p => p.ID == marketplaceID);
            }

            int max = 20;

            int quota = max;

            System.Timers.Timer timer = new System.Timers.Timer(1000);

            timer.Elapsed += (sender, e) =>
            {
                if (quota < 20)
                {
                    if ((quota + 5) > max)
                    {
                        quota = max;
                    }
                    else
                    {
                        quota += 5;
                    }
                }
            };

            timer.Start();

            List<GetLowestOfferListingsForASINResult> results = new List<GetLowestOfferListingsForASINResult>();

            Queue<string> pending = new Queue<string>(asins);

            while (pending.Count > 0)
            {
                List<string> current = new List<string>();

                for (int i = 0; i < 20; i++)
                {
                    if (pending.Count > 0)
                    {
                        string asin = pending.Dequeue();

                        if (!current.Contains(asin))
                        {
                            current.Add(asin);
                        }
                    }
                }

                if (current.Count > 0)
                {
                    GetLowestOfferListingsForASINRequest request = new GetLowestOfferListingsForASINRequest();
                    request.ExcludeMe = true;
                    request.SellerId = marketplace.MerchantId;
                    request.MarketplaceId = marketplace.MarketplaceId;
                    request.ASINList = new ASINListType();
                    request.ASINList.ASIN = current;

                    while (quota < current.Count)
                    {
                        Thread.Sleep(1000);
                    }

                    GetLowestOfferListingsForASINResponse response = marketplace.GetMWSProductsClient().GetLowestOfferListingsForASIN(request);

                    quota = quota - current.Count;

                    if (response.IsSetGetLowestOfferListingsForASINResult())
                    {
                        foreach (var result in response.GetLowestOfferListingsForASINResult)
                        {
                            results.Add(result);
                        }
                    }
                }
            }

            return results;
        }   

        public List<GetMatchingProductForIdResult> GetMatchingProductForId(int marketplaceID, IEnumerable<string> upcs)
        {
            AmznMarketplace marketplace = null;

            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                marketplace = dataContext.AmznMarketplaces.Single(p => p.ID == marketplaceID);
            }

            int max = 19;

            int quota = max;

            System.Timers.Timer timer = new System.Timers.Timer(1000);

            timer.Elapsed += (sender, e) =>
            {
                if (quota < 20)
                {
                    if ((quota + 5) > max)
                    {
                        quota = max;
                    }
                    else
                    {
                        quota += 5;
                    }
                }
            };

            timer.Start();

            List<GetMatchingProductForIdResult> results = new List<GetMatchingProductForIdResult>();

            Queue<string> pending = new Queue<string>(upcs);

            while (pending.Count > 0)
            {
                List<string> current = new List<string>();

                for (int i = 0; i < 5; i++)
                {
                    if (pending.Count > 0)
                    {
                        string upc = pending.Dequeue();

                        if (!current.Contains(upc))
                        {
                            current.Add(upc);
                        }
                    }
                }

                if (current.Count > 0)
                {
                    GetMatchingProductForIdRequest request = new GetMatchingProductForIdRequest();
                    request.SellerId = marketplace.MerchantId;
                    request.MarketplaceId = marketplace.MarketplaceId;
                    request.IdList = new IdListType();
                    request.IdList.Id = current;
                    request.IdType = "UPC";

                    while (quota < current.Count)
                    {
                        Thread.Sleep(1000);
                    }

                    GetMatchingProductForIdResponse response = marketplace.GetMWSProductsClient().GetMatchingProductForId(request);

                    quota = quota - current.Count;

                    if (response.IsSetGetMatchingProductForIdResult())
                    {
                        foreach (GetMatchingProductForIdResult result in response.GetMatchingProductForIdResult)
                        {
                            results.Add(result);
                        }
                    }
                }
            }

            LinkWithAsinCatalog(marketplaceID, results);

            return results;
        }

        private void LinkWithAsinCatalog(int marketplaceID, List<GetMatchingProductForIdResult> catalogData)
        {
            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                AmznMarketplace marketplace = dataContext.AmznMarketplaces.Single(p => p.ID == marketplaceID);

                foreach (var result in catalogData)
                {
                    Item item = dataContext.Items.Single(p => p.Aliases.Any(s => s.Alias1.Equals(result.Id)));

                    if (result.IsSetProducts() && result.Products.IsSetProduct())
                    {
                        MarketplaceWebServiceProducts.Model.Product product = result.Products.Product.First();

                        if (product.IsSetIdentifiers() && product.Identifiers.IsSetMarketplaceASIN() && product.Identifiers.MarketplaceASIN.IsSetASIN())
                        {
                            if (item.AmznListingItems.Any(p => p.MarketplaceID == marketplaceID))
                            {
                                AmznListingItem listingItem = item.AmznListingItems.Single(p => p.MarketplaceID == marketplaceID);

                                listingItem.ASIN = product.Identifiers.MarketplaceASIN.ASIN;
                            }
                            else
                            {
                                AmznListingItem listingItem = new AmznListingItem();
                                listingItem.ASIN = product.Identifiers.MarketplaceASIN.ASIN;
                                listingItem.Item = item;
                                listingItem.IsActive = false;
                                listingItem.Sku = item.ItemLookupCode;
                                listingItem.Title = string.Empty;
                                listingItem.Marketplace = marketplace;
                                listingItem.OpenDate = DateTime.UtcNow;
                                listingItem.LastSyncTime = DateTime.UtcNow;
                            }
                        }
                    }
                }

                dataContext.SaveChanges();
            }
        }

        public void SynchronizeListings(int marketplaceID)
        {
            ListingSynchronizer synchronizer = new ListingSynchronizer(marketplaceID);

            synchronizer.Synchronize();
        }

        public void SynchronizeOrders(int marketplaceID)
        {
            OrderSynchronizer synchronizer = new OrderSynchronizer(marketplaceID);

            synchronizer.Synchronize();
        }

        public void FixOverpublished(int marketplaceID)
        {
            List<ListingItemDto> listingItemDtos = new List<ListingItemDto>();

            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                AmznMarketplace marketplace = dataContext.AmznMarketplaces.Single(p => p.ID == marketplaceID);

                if (!marketplace.OrderSyncTime.HasValue || marketplace.OrderSyncTime.Value < DateTime.UtcNow.AddHours(-1))
                {
                    throw new InvalidOperationException(marketplace.Name + " orders must be synchronized in order to fix overpublished");
                }

                var listingItems = dataContext.AmznListingItems.Where(p => p.MarketplaceID == marketplace.ID && p.IsActive && p.Quantity > 0);

                foreach (AmznListingItem listingItem in listingItems)
                {
                    if (listingItem.Item != null)
                    {
                        if (listingItem.Quantity > listingItem.Item.QtyAvailable)
                        {
                            ListingItemDto listingItemDto = new ListingItemDto();
                            listingItemDto.Sku = listingItem.Item.ItemLookupCode;
                            listingItemDto.Qty = listingItem.Item.QtyAvailable;
                            listingItemDtos.Add(listingItemDto);
                        }
                    }
                }
            }

            Publish(marketplaceID, listingItemDtos, "Overpublished Service");
        }

        public void Publish(int marketplaceID, IEnumerable<ListingItemDto> listingItems, string source)
        {
            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                dataContext.MaterializeAttributes = true;

                AmznMarketplace marketplace = dataContext.AmznMarketplaces.Single(p => p.ID == marketplaceID);

                List<string> needASIN = new List<string>();

                foreach (var listingItemDto in listingItems.Where(p => p.IncludeProductData))
                {
                    Item item = dataContext.Items.Single(p => p.ItemLookupCode.Equals(listingItemDto.Sku));

                    if (!item.AmznListingItems.Any(p => p.MarketplaceID == marketplace.ID && !p.ASIN.Equals("UNKNOWN")))
                    {
                        needASIN.Add(item.GTIN);
                    }
                }

                if (needASIN.Count > 0)
                {
                    GetMatchingProductForId(marketplaceID, needASIN.Distinct());
                }

                Publisher publisher = new Publisher(dataContext, marketplace, source);

                publisher.Result += (e) => { if (this.Result != null) { this.Result(e); } };

                publisher.Publish(listingItems);
            }
        }
    }

    public class Publisher
    {
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

        private ProductMapperFactory _productMapperFactory = new ProductMapperFactory();
        private berkeleyEntities _dataContext;
        private AmznMarketplace _marketplace;
        private string _source;

        private Dictionary<ListingItemDto, List<AmazonEnvelopeMessage>> _pending = new Dictionary<ListingItemDto,List<AmazonEnvelopeMessage>>();

        private List<FeedSubmissionInfo> _waitingProcessingResult = new List<FeedSubmissionInfo>();
        private List<FeedSubmissionInfo> _completedProcessingResult = new List<FeedSubmissionInfo>();
        private Dictionary<string, AmazonEnvelope> _envelopes = new Dictionary<string, AmazonEnvelope>();




        public Publisher(berkeleyEntities dataContext, AmznMarketplace marketplace, string source)
        {
            _source = source;
            _dataContext = dataContext;
            _marketplace = marketplace;
        }

        public void Publish(IEnumerable<ListingItemDto> listingItems)
        {
            List<ListingItemDto> productFeed = new List<ListingItemDto>();
            List<ListingItemDto> noProductFeed = new List<ListingItemDto>();

            foreach (var listingItem in listingItems)
            {
                try
                {
                    _pending.Add(listingItem, new List<AmazonEnvelopeMessage>());

                    Item item = _dataContext.Items.Single(p => p.ItemLookupCode.Equals(listingItem.Sku));

                    if (item.ItemClass == null)
                    {
                        listingItem.ClassName = string.Empty;
                    }
                    else
                    {
                        listingItem.ClassName = item.ItemClass.ItemLookupCode;
                    }

                    if (listingItem.IncludeProductData)
                    {
                        productFeed.Add(listingItem);
                    }
                    else
                    {
                        noProductFeed.Add(listingItem);
                    }
                }
                catch (Exception e)
                {
                    
                    PublishingResult result = new PublishingResult();
                    result.Data = listingItem;
                    result.HasError = true;
                    result.Message = e.Message;

                    Result(new List<PublishingResult>() { result });
                }
            }

            if (productFeed.Count > 0)
            {
                SubmitProductFeed(productFeed);
            }

            var qtyModified = noProductFeed.Where(p => p.Qty.HasValue);
            var priceModified = noProductFeed.Where(p => p.Price.HasValue || p.SaleData != null);

            if (qtyModified.Count() > 0)
            {
                SubmitInventoryFeed(qtyModified);
            }

            if (priceModified.Count() > 0)
            {
                SubmitPriceFeed(priceModified);
            }

            WaitForProcessingResult();
        }

        public event PublishingResultHandler Result;

        private void WaitForProcessingResult()
        {
            while (AnyPendingSubmission)
            {
                Thread.Sleep(60000);

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

                NotifyPublishingStatus();
            }
        }

        private void NotifyPublishingStatus()
        {
            List<PublishingResult> results = new List<PublishingResult>();

            foreach (var listingItem in _pending.Keys.ToList())
            {
                var msgs = _pending[listingItem];

                if (msgs.All(p => p.ProcessingResult != null))
                {
                    var errorMsgs = msgs.Where(p => p.ProcessingResult.ResultCode.Equals(ProcessingReportResultResultCode.Error));

                    PublishingResult result = new PublishingResult();

                    if (errorMsgs.Count() == 0)
                    {
                        result.Data = listingItem;
                        result.HasError = false;
                    }
                    else
                    {
                        var productError = errorMsgs.Where(p => p.Item is Product);

                        if (productError.Count() > 0)
                        {
                            var productMsg = productError.SingleOrDefault(p => ((Product)p.Item).SKU.Equals(listingItem.Sku));

                            if (productMsg != null)
                            {
                                listingItem.ProductData = productMsg.Item as Product;
                            }
                        }

                        var completedMsgs = msgs.Where(p => !p.ProcessingResult.ResultCode.Equals(ProcessingReportResultResultCode.Error));


                        result.Message = string.Join(" ", errorMsgs.Select(p => p.ProcessingResult.ResultDescription).Concat(completedMsgs.Select(p => p.MessageID + " completed")));

                        result.HasError = true;
                        result.Data = listingItem;
                    }

                    results.Add(result);

                    _pending.Remove(listingItem);
                }
            }

            if (results.Count > 0)
            {
                Result(results);
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

            List<AmazonEnvelopeMessage> completedMsgs = new List<AmazonEnvelopeMessage>();
            List<AmazonEnvelopeMessage> errorMsgs = new List<AmazonEnvelopeMessage>();

            foreach (AmazonEnvelopeMessage msg in envelope.Message)
            {

                bool hasError = processingReport.Result == null ? false :
                    processingReport.Result.Any(p => p.MessageID.Equals(msg.MessageID) && p.ResultCode.Equals(ProcessingReportResultResultCode.Error));

                if (hasError)
                {
                    msg.ProcessingResult = processingReport.Result.First(p => p.MessageID.Equals(msg.MessageID) && p.ResultCode.Equals(ProcessingReportResultResultCode.Error));
                    errorMsgs.Add(msg);
                }
                else
                {
                    msg.ProcessingResult = new ProcessingReportResult() { ResultCode = ProcessingReportResultResultCode.Completed };
                    completedMsgs.Add(msg);
                }
            }

            switch (submission.FeedType)
            {
                case PRODUCT_DATA:

                    SaveProductDataChanges(completedMsgs);

                    List<ListingItemDto> productFeedCompleted = new List<ListingItemDto>();

                    var completedSkus = completedMsgs.Select(p => ((Product)p.Item).SKU);

                    foreach (var listingItem in _pending.Keys)
                    {
                        if (completedSkus.Contains(listingItem.Sku))
                        {
                            productFeedCompleted.Add(listingItem);
                        }
                    }

                    if (productFeedCompleted.Count > 0)
                    {
                        SubmitInventoryFeed(productFeedCompleted);
                        SubmitPriceFeed(productFeedCompleted);
                        SubmitRelationshipFeed(productFeedCompleted); 
                    }
                   
                   break;

                case INVENTORY_DATA:
                    SaveInventoryDataChanges(completedMsgs); break;
                case PRICE_DATA:
                    SavePriceDataChanges(completedMsgs); break;
                case RELATIONSHIP_DATA:
                    break;
            }

            //if (this.Result != null)
            //{
            //    this.Result(new ResultArgs() { Envelope = envelope, ProcessingReport = processingReport });
            //}
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

        private void SaveProductDataChanges(IEnumerable<AmazonEnvelopeMessage> msgs)
        {
            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                AmznMarketplace marketplace = dataContext.AmznMarketplaces.Single(p => p.ID == _marketplace.ID);

                foreach (var msg in msgs)
                {
                    Product product = (Product)msg.Item;

                    if (dataContext.Items.Any(p => p.ItemLookupCode.Equals(product.SKU)))
                    {
                        AmznListingItem listingItem = dataContext.AmznListingItems
                            .SingleOrDefault(p => p.Item.ItemLookupCode.Equals(product.SKU) && p.MarketplaceID == marketplace.ID);

                        if (listingItem == null)
                        {
                            listingItem = new AmznListingItem();
                            listingItem.Item = dataContext.Items.Single(p => p.ItemLookupCode.Equals(product.SKU));
                            listingItem.Marketplace = marketplace;
                            listingItem.ASIN = "UNKNOWN";
                            listingItem.Quantity = 0;
                            listingItem.Price = 0;
                        }

                        listingItem.Sku = product.SKU;

                       
                        listingItem.Title = product.DescriptionData.Title;
                        listingItem.OpenDate = DateTime.UtcNow;
                        listingItem.LastSyncTime = DateTime.UtcNow;
                        listingItem.IsActive = true;
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

                LogChanges(dataContext);

                dataContext.SaveChanges();
            }
        }

        private void SubmitProductFeed(IEnumerable<ListingItemDto> listingItems)
        {
            List<AmazonEnvelopeMessage> messages = new List<AmazonEnvelopeMessage>();

            int currentMsg = 1;

            var individualListings = listingItems.Where(p => string.IsNullOrEmpty(p.ClassName));
            var variationListings = listingItems.Where(p => !string.IsNullOrEmpty(p.ClassName));

            foreach (ListingItemDto listingItem in individualListings)
            {
                if (listingItem.ProductData == null)
                {
                    ProductData productDataMapper = _productMapperFactory.GetProductData(_dataContext.Items.Single(p => p.ItemLookupCode.Equals(listingItem.Sku)));
                    listingItem.ProductData = productDataMapper.GetProductDto(listingItem.Title);
                }

                var msg = BuildMessage(listingItem.ProductData, currentMsg);

                _pending[listingItem].Add(msg);

                messages.Add(msg);

                currentMsg++;
            }


            foreach (var classGroup in variationListings.GroupBy(p => p.ClassName))
            {
                if (classGroup.All(p => p.ProductData == null))
                {
                    ListingItemDto first = classGroup.First();

                    ProductData productDataMapper = _productMapperFactory.GetProductData(_dataContext.Items.Single(p => p.ItemLookupCode.Equals(first.Sku)));

                    Product product = productDataMapper.GetParentProductDto(first.Title);

                    if (product.DescriptionData != null)
                    {
                        var msg = BuildMessage(product, currentMsg);

                        foreach (var listingItem in classGroup)
                        {
                            _pending[listingItem].Add(msg);
                        }

                        messages.Add(msg);

                        currentMsg++; 
                    }
                }

                foreach (ListingItemDto listingItem in classGroup)
                {
                    if (listingItem.ProductData == null)
                    {
                        ProductData productDataMapper = _productMapperFactory.GetProductData(_dataContext.Items.Single(p => p.ItemLookupCode.Equals(listingItem.Sku)));

                        listingItem.ProductData = productDataMapper.GetProductDto(listingItem.Title);
                    }

                    var msg = BuildMessage(listingItem.ProductData, currentMsg);

                    _pending[listingItem].Add(msg);

                    messages.Add(msg);

                    currentMsg++;
                }
            }

            SubmitFeed(BuildEnvelope(AmazonEnvelopeMessageType.Product, messages));
        }

        private void SubmitRelationshipFeed(IEnumerable<ListingItemDto> listingItems)
        {
            List<AmazonEnvelopeMessage> messages = new List<AmazonEnvelopeMessage>();

            int currentMsg = 1;

            var classGroups = listingItems.Where(p => p.ClassName != null).GroupBy(p => p.ClassName);

            foreach (var classGroup in classGroups)
            {
                var itemClass = _dataContext.ItemClasses.SingleOrDefault(p => p.ItemLookupCode.Equals(classGroup.Key));

                if (itemClass != null)
                {
                    var siblingSkus = itemClass.ItemClassComponents.Select(p => p.Item)
                        .Where(p => p.AmznListingItems.Any(s => s.IsActive && s.MarketplaceID == _marketplace.ID))
                        .Select(p => p.ItemLookupCode);

                    Relationship relationships = new Relationship();

                    relationships.ParentSKU = classGroup.Key;

                    List<RelationshipRelation> relations = new List<RelationshipRelation>();

                    foreach (string sku in siblingSkus)
                    {
                        relations.Add(new RelationshipRelation() { SKU = sku, Type = RelationshipRelationType.Variation });
                    }

                    foreach (ListingItemDto listingItem in classGroup)
                    {
                        if (!siblingSkus.Contains(listingItem.Sku))
                        {
                            relations.Add(new RelationshipRelation() { SKU = listingItem.Sku, Type = RelationshipRelationType.Variation });
                        }
                    }

                    relationships.Relation = relations.ToArray();

                    var msg = BuildMessage(relationships, currentMsg);

                    foreach (ListingItemDto listingItem in classGroup)
                    {
                        _pending[listingItem].Add(msg);
                    }

                    messages.Add(msg);

                    currentMsg++;
                }
            }


            SubmitFeed(BuildEnvelope(AmazonEnvelopeMessageType.Relationship, messages));
        }

        private void SubmitInventoryFeed(IEnumerable<ListingItemDto> listingItems)
        {
            List<AmazonEnvelopeMessage> messages = new List<AmazonEnvelopeMessage>();

            int currentMsg = 1;

            foreach (ListingItemDto listingItem in listingItems)
            {
                Inventory inventoryData = new Inventory();
                inventoryData.SKU = listingItem.Sku;
                inventoryData.Item = listingItem.Qty.ToString();
                inventoryData.RestockDateSpecified = false;
                inventoryData.SwitchFulfillmentToSpecified = false;

                var msg = BuildMessage(inventoryData, currentMsg);

                _pending[listingItem].Add(msg);

                messages.Add(msg);

                currentMsg++;
            }

            SubmitFeed(BuildEnvelope(AmazonEnvelopeMessageType.Inventory, messages));
        }

        private void SubmitPriceFeed(IEnumerable<ListingItemDto> listingItems)
        {
            List<AmazonEnvelopeMessage> messages = new List<AmazonEnvelopeMessage>();

            int currentMsg = 1;

            foreach (ListingItemDto listingItem in listingItems)
            {
                Price priceData = new Price();
                priceData.SKU = listingItem.Sku;

                if (listingItem.Price.HasValue)
                {
                    OverrideCurrencyAmount oca = new OverrideCurrencyAmount();
                    oca.currency = BaseCurrencyCodeWithDefault.USD;
                    oca.Value = Math.Round(listingItem.Price.Value, 4);

                    priceData.StandardPrice = oca;
                }
                
                if (listingItem.SaleData != null)
                {
                    priceData.Sale = new PriceSale();
                    priceData.Sale.SalePrice = new OverrideCurrencyAmount() { currency = BaseCurrencyCodeWithDefault.USD, Value = listingItem.SaleData.SalePrice };
                    priceData.Sale.StartDate = listingItem.SaleData.StartDate;
                    priceData.Sale.EndDate = listingItem.SaleData.EndDate;
                }

                var msg = BuildMessage(priceData, currentMsg);

                _pending[listingItem].Add(msg);

                messages.Add(msg);

                currentMsg++;
            }

            SubmitFeed(BuildEnvelope(AmazonEnvelopeMessageType.Price, messages));
        }

        private AmazonEnvelopeMessage BuildMessage(object item, int messageID)
        {
            AmazonEnvelopeMessage msg = new AmazonEnvelopeMessage();
            msg.OperationTypeSpecified = true;
            msg.OperationType = AmazonEnvelopeMessageOperationType.Update;
            msg.Item = item;
            msg.MessageID = messageID.ToString();

            return msg;
        }

        private AmazonEnvelope BuildEnvelope(AmazonEnvelopeMessageType msgType, IEnumerable<AmazonEnvelopeMessage> msgs)
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

        private void LogChanges(berkeleyEntities dataContext)
        {
            var entries = dataContext.ObjectStateManager.GetObjectStateEntries(EntityState.Added | EntityState.Modified | EntityState.Deleted);

            foreach (var entry in entries.Where(p => p.Entity is AmznListingItem))
            {
                AmznListingItem listingItem = entry.Entity as AmznListingItem;

                switch (entry.State)
                {
                    case EntityState.Added:

                        Bsi_ListingChangesLog logEntryAdded = new Bsi_ListingChangesLog();
                        logEntryAdded.Date = DateTime.Now;
                        logEntryAdded.Item = listingItem.Item;
                        logEntryAdded.ListingCode = listingItem.ASIN;
                        logEntryAdded.Marketplace = listingItem.Marketplace.Code;
                        logEntryAdded.ListingType = EbayMarketplace.FORMAT_FIXEDPRICE;
                        logEntryAdded.Source = _source;
                        logEntryAdded.Change = listingItem.Quantity;
                        break;

                    case EntityState.Modified:

                        int originalQty = int.Parse(entry.OriginalValues["Quantity"].ToString());
                        int currentyQty = int.Parse(entry.CurrentValues["Quantity"].ToString());

                        int change = currentyQty - originalQty;

                        if (change != 0)
                        {
                            Bsi_ListingChangesLog logEntryModified = new Bsi_ListingChangesLog();
                            logEntryModified.Date = DateTime.Now;
                            logEntryModified.Item = listingItem.Item;
                            logEntryModified.ListingCode = listingItem.ASIN;
                            logEntryModified.Marketplace = listingItem.Marketplace.Code;
                            logEntryModified.ListingType = EbayMarketplace.FORMAT_FIXEDPRICE;
                            logEntryModified.Source = _source;
                            logEntryModified.Change = change;
                        }

                        break;

                    default: break;

                }
            }
        }

    }

    public class ListingItemDto
    {
        public Product ProductData { get; set; }

        public string ClassName { get; set; }

        public string Sku {get; set;}

        public int? Qty { get; set; }

        public decimal? Price { get; set; }

        public SaleData SaleData { get; set; }

        public string Title { get; set; }

        public string Condition { get; set; }

        public bool IncludeProductData { get; set; }
    }

    public class SaleData
    {
        public decimal SalePrice { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }

    public class ListingSynchronizer
    {
        const string FEED_SUBMITTED = "_SUBMITTED_";
        const string FEED_CANCELLED = "_CANCELLED_";
        const string FEED_INPROGRESS = "_IN_PROGRESS_";
        const string FEED_DONE = "_DONE_";
        const string FEED_DONENODATA = "_DONE_NO_DATA_";

        const string REPORT_LISTINGS = "_GET_MERCHANT_LISTINGS_DATA_";

        private berkeleyEntities _dataContext = new berkeleyEntities();
        private AmznMarketplace _marketplace;
        private DateTime _currentSyncTime;

        public ListingSynchronizer(int marketplaceID)
        {
            _marketplace = _dataContext.AmznMarketplaces.Single(p => p.ID == marketplaceID);
        }

        public void Synchronize()
        {
            ReportRequestInfo requestInfo = CheckForExistingRequest(REPORT_LISTINGS);

            if (requestInfo == null)
            {
                requestInfo = RequestReport(REPORT_LISTINGS);
            }

            while (requestInfo.ReportProcessingStatus.Equals(FEED_INPROGRESS) || requestInfo.ReportProcessingStatus.Equals(FEED_SUBMITTED))
            {
                //Wait 5 minutes
                Thread.Sleep(200000);

                requestInfo = Poll(requestInfo.ReportRequestId);
            }

            var lines = GetReport(requestInfo.GeneratedReportId);

            _currentSyncTime = requestInfo.SubmittedDate.ToUniversalTime();

            PersistListings(new Queue<string>(lines));

            _marketplace.ListingSyncTime = _currentSyncTime;

            _dataContext.SaveChanges();
        }

        private ReportRequestInfo RequestReport(string reportType)
        {
            RequestReportRequest request = new RequestReportRequest();
            request.Merchant = _marketplace.MerchantId;
            request.ReportType = reportType;
            //request.StartDate = DateTime.Now;
            //request.EndDate = DateTime.Now;

            RequestReportResponse response = _marketplace.GetMWSClient().RequestReport(request);

            ReportRequestInfo requestInfo = response.RequestReportResult.ReportRequestInfo;

            return requestInfo;
        }

        private ReportRequestInfo Poll(string requestID)
        {
            GetReportRequestListRequest request = new GetReportRequestListRequest();
            request.Merchant = _marketplace.MerchantId;
            request.ReportRequestIdList = new IdList();
            request.ReportRequestIdList.Id = new List<string>() { requestID };

            GetReportRequestListResponse response = _marketplace.GetMWSClient().GetReportRequestList(request);

            return response.GetReportRequestListResult.ReportRequestInfo.First();
        }

        private ReportRequestInfo CheckForExistingRequest(string reportType)
        {
            GetReportRequestListRequest request = new GetReportRequestListRequest();
            request.Merchant = _marketplace.MerchantId;
            request.ReportTypeList = new TypeList();
            request.ReportTypeList.Type = new List<string>() { reportType };

            request.RequestedFromDate = DateTime.Now.AddHours(-1);
            request.RequestedToDate = DateTime.Now;

            GetReportRequestListResponse response = _marketplace.GetMWSClient().GetReportRequestList(request);

            List<ReportRequestInfo> requests = response.GetReportRequestListResult.ReportRequestInfo;

            ReportRequestInfo requestInfo = null;

            if (response.GetReportRequestListResult.ReportRequestInfo.Count > 0)
            {
                DateTime latestSubmission = requests.Max(s => s.SubmittedDate);

                requestInfo = requests.SingleOrDefault(p => p.SubmittedDate.Equals(latestSubmission));
            }

            return requestInfo;
        }

        private IEnumerable<string> GetReport(string reportID)
        {
            MemoryStream ms = new MemoryStream();

            GetReportRequest request = new GetReportRequest();
            request.Merchant = _marketplace.MerchantId;
            request.ReportId = reportID;
            request.Report = ms;

            GetReportResponse response = _marketplace.GetMWSClient().GetReport(request);

            StreamReader reader = new StreamReader(ms);

            return reader.ReadToEnd().Split(new Char[1] { '\n' }).Where(p => !string.IsNullOrWhiteSpace(p));
        }

        private void PersistListings(Queue<string> lines)
        {
            lines.Dequeue();

            while (lines.Count > 0)
            {
                int i = 0;

                while (lines.Count > 0 && i < 500)
                {
                    string line = lines.Dequeue();

                    try
                    {
                        ProccessLine(line);
                    }
                    catch (Exception e)
                    {
                        string sku = line.Split(new Char[1] { '\t' })[3];
                    }

                    i++;
                }

                _dataContext.SaveChanges();
            }

            var deletedListings = _dataContext.AmznListingItems
                    .Where(p => p.MarketplaceID.Equals(_marketplace.ID) && !p.LastSyncTime.Equals(_currentSyncTime));

            foreach (AmznListingItem listingItem in deletedListings)
            {
                listingItem.IsActive = false;
            }

            _marketplace.ListingSyncTime = _currentSyncTime;

            LogChanges();

            _dataContext.SaveChanges();
        }

        private void ProccessLine(string line)
        {
            string[] values = line.Split(new Char[1] { '\t' });

            string sku, title, price, quantity, openDate, asin, condition;

            title = values[0];
            sku = values[3];
            price = values[4];
            quantity = values[5];
            openDate = values[6].Replace("PDT", "").Replace("PST", "");
            condition = values[12];
            asin = values[16];

            if (title.Length > 200)
            {
                title = title.Substring(0, 200);
            }

            AmznListingItem listingItem = _dataContext.AmznListingItems.SingleOrDefault(p => p.Sku.Equals(sku) && p.MarketplaceID.Equals(_marketplace.ID));

            if (listingItem == null)
            {
                listingItem = new AmznListingItem();
                listingItem.MarketplaceID = _marketplace.ID;
                listingItem.Item = _dataContext.Items.SingleOrDefault(p => p.ItemLookupCode.Equals(sku)); ;
                listingItem.Sku = sku;
            }

            int qty = int.Parse(quantity);

            if (listingItem.LastSyncTime < _currentSyncTime && listingItem.Quantity != qty)
            {
                listingItem.Quantity = qty;
            }

            listingItem.Price = decimal.Parse(price);
            listingItem.ASIN = asin;
            listingItem.Title = title;
            listingItem.OpenDate = TimeZoneInfo.ConvertTimeToUtc(DateTime.Parse(openDate), TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"));
            listingItem.LastSyncTime = _currentSyncTime;
            listingItem.IsActive = true;
        }

        private void LogChanges()
        {
            var entries = _dataContext.ObjectStateManager.GetObjectStateEntries(EntityState.Added | EntityState.Modified | EntityState.Deleted);

            foreach (var entry in entries.Where(p => p.Entity is AmznListingItem))
            {
                AmznListingItem listingItem = entry.Entity as AmznListingItem;

                switch (entry.State)
                {
                    case EntityState.Added:

                        Bsi_ListingChangesLog logEntryAdded = new Bsi_ListingChangesLog();
                        logEntryAdded.Date = DateTime.Now;
                        logEntryAdded.Item = listingItem.Item;
                        logEntryAdded.ListingCode = listingItem.ASIN;
                        logEntryAdded.Marketplace = listingItem.Marketplace.Code;
                        logEntryAdded.ListingType = EbayMarketplace.FORMAT_FIXEDPRICE;
                        logEntryAdded.Source = "Synchronization Service";
                        logEntryAdded.Change = listingItem.Quantity;
                        break;

                    case EntityState.Modified:

                        int originalQty = int.Parse(entry.OriginalValues["Quantity"].ToString());
                        int currentyQty = int.Parse(entry.CurrentValues["Quantity"].ToString());

                        int change = currentyQty - originalQty;

                        if (change != 0)
                        {
                            Bsi_ListingChangesLog logEntryModified = new Bsi_ListingChangesLog();
                            logEntryModified.Date = DateTime.Now;
                            logEntryModified.Item = listingItem.Item;
                            logEntryModified.ListingCode = listingItem.ASIN;
                            logEntryModified.Marketplace = listingItem.Marketplace.Code;
                            logEntryModified.ListingType = EbayMarketplace.FORMAT_FIXEDPRICE;
                            logEntryModified.Source = "Synchronization Service";
                            logEntryModified.Change = change;
                        }

                        break;

                    default: break;

                }
            }
        }
    }

    //public class PaymentSynchronizer
    //{
    //    const string FEED_SUBMITTED = "_SUBMITTED_";
    //    const string FEED_CANCELLED = "_CANCELLED_";
    //    const string FEED_INPROGRESS = "_IN_PROGRESS_";
    //    const string FEED_DONE = "_DONE_";
    //    const string FEED_DONENODATA = "_DONE_NO_DATA_";

    //    const string REPORT_PAYMENTS = "_GET_PAYMENT_SETTLEMENT_DATA_";

    //    private berkeleyEntities _dataContext;
    //    private AmznMarketplace _marketplace;
    //    private DateTime _currentSyncTime;

    //    public PaymentSynchronizer(berkeleyEntities dataContext, AmznMarketplace marketplace)
    //    {
    //        _marketplace = marketplace;
    //        _dataContext = dataContext;
    //    }

    //    public DateTime Synchronize()
    //    {

    //        GetReportListRequest request = new GetReportListRequest();
    //        request.ReportTypeList = new TypeList();
    //        request.ReportTypeList.Type = new List<string>() { REPORT_PAYMENTS };
    //        request.Merchant = _marketplace.MerchantId;

    //        ReportRequestInfo requestInfo = CheckForExistingRequest(REPORT_LISTINGS);

    //        if (requestInfo == null)
    //        {
    //            requestInfo = RequestReport(REPORT_LISTINGS);
    //        }

    //        while (requestInfo.ReportProcessingStatus.Equals(FEED_INPROGRESS) || requestInfo.ReportProcessingStatus.Equals(FEED_SUBMITTED))
    //        {
    //            //Wait 5 minutes
    //            Thread.Sleep(200000);

    //            requestInfo = Poll(requestInfo.ReportRequestId);
    //        }


    //        var lines = GetReport(requestInfo.GeneratedReportId);

    //        _currentSyncTime = requestInfo.SubmittedDate.ToUniversalTime();

    //        PersistListings(new Queue<string>(lines));

    //        return _currentSyncTime;
    //    }

    //    private IEnumerable<string> GetReport(string reportID)
    //    {
    //        MemoryStream ms = new MemoryStream();

    //        GetReportRequest request = new GetReportRequest();
    //        request.Merchant = _marketplace.MerchantId;
    //        request.ReportId = reportID;
    //        request.Report = ms;

    //        GetReportResponse response = _marketplace.GetMWSClient().GetReport(request);

    //        StreamReader reader = new StreamReader(ms);

    //        return reader.ReadToEnd().Split(new Char[1] { '\n' }).Where(p => !string.IsNullOrWhiteSpace(p));
    //    }
    //}

    public class OrderSynchronizer
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private berkeleyEntities _dataContext = new berkeleyEntities();
        private AmznMarketplace _marketplace;
        private DateTime _currentSyncTime;

        private System.Timers.Timer _timer2Sec = null;
        private System.Timers.Timer _timer1Min = null;

        private int _listOrderItemsQuota = 30;
        private int _listOrdersQuota = 6;
        private int _getOrdersQuota = 6;

        public OrderSynchronizer(int marketplaceID)
        {
            _marketplace = _dataContext.AmznMarketplaces.Single(p => p.ID == marketplaceID);
        }

        public void Synchronize()
        {
            DateTime syncTime = DateTime.UtcNow.AddMinutes(-5);

            Initilialize();

            _currentSyncTime = syncTime;

            if (!_marketplace.OrderSyncTime.HasValue)
            {
                DateTime from = _currentSyncTime.AddDays(-30).ToLocalTime();
                DateTime to = _currentSyncTime.ToLocalTime();

                SyncOrdersByModifiedTime(from, to);
            }
            else
            {
                DateTime from = _marketplace.OrderSyncTime.Value.AddMinutes(-5).ToLocalTime();
                DateTime to = _currentSyncTime.ToLocalTime();

                SyncOrdersByModifiedTime(from, to);
            }

            _marketplace.OrderSyncTime = syncTime;

            _timer2Sec.Enabled = false;
            _timer1Min.Enabled = false;

            _dataContext.SaveChanges();
        }

        private void Initilialize()
        {
            _timer2Sec = new System.Timers.Timer(2100);
            _timer2Sec.Elapsed += new System.Timers.ElapsedEventHandler(RestoreEvent2Sec);

            _timer1Min = new System.Timers.Timer(60100);
            _timer1Min.Elapsed += new System.Timers.ElapsedEventHandler(RestoreEvent1Min);

            _timer2Sec.Enabled = true;
            _timer1Min.Enabled = true;
        }

        private void SyncOrders(IEnumerable<string> orderIds)
        {
            GetOrderRequest request = new GetOrderRequest();
            request.AmazonOrderId = new OrderIdList();
            request.AmazonOrderId.Id = orderIds.ToList();
            request.SellerId = _marketplace.MerchantId;

            while (_getOrdersQuota < 1)
            {
                Thread.Sleep(1000);
            }

            try
            {
                GetOrderResponse response = _marketplace.GetMWSOrdersClient().GetOrder(request);

                _getOrdersQuota--;

                ProcessOrders(response.GetOrderResult.Orders.Order);
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);
            }
        }

        private void SyncOrdersByModifiedTime(DateTime from, DateTime to)
        {
            ListOrdersRequest request = new ListOrdersRequest();
            request.MarketplaceId = new MarketplaceIdList();
            request.MarketplaceId.Id = new List<string>() { _marketplace.MarketplaceId };
            request.SellerId = _marketplace.MerchantId;
            request.LastUpdatedAfter = from;
            request.LastUpdatedBefore = to;
            
            while (_listOrdersQuota < 1)
            {
                Thread.Sleep(1000);
            }

            
            ListOrdersResponse response = _marketplace.GetMWSOrdersClient().ListOrders(request);

            _listOrdersQuota--;

            ProcessOrders(response.ListOrdersResult.Orders.Order);

            if (response.ListOrdersResult.IsSetNextToken())
            {
                ListOrderByNextToken(response.ListOrdersResult.NextToken);
            }
            
        }

        private void ListOrderByNextToken(string nextToken)
        {
            ListOrdersByNextTokenRequest request = new ListOrdersByNextTokenRequest();
            request.NextToken = nextToken;
            request.SellerId = _marketplace.MerchantId;

            while (_listOrdersQuota < 1)
            {
                Thread.Sleep(1000);
            }

            ListOrdersByNextTokenResponse response = _marketplace.GetMWSOrdersClient().ListOrdersByNextToken(request);

            _listOrdersQuota--;

            ProcessOrders(response.ListOrdersByNextTokenResult.Orders.Order);

            if (response.ListOrdersByNextTokenResult.IsSetNextToken())
            {
                ListOrderByNextToken(response.ListOrdersByNextTokenResult.NextToken);
            }
        }

        private List<OrderItem> ListOrderItems(string orderID)
        {
            ListOrderItemsRequest request = new ListOrderItemsRequest();
            request.AmazonOrderId = orderID;
            request.SellerId = _marketplace.MerchantId;

            while (_listOrderItemsQuota < 1)
            {
                Thread.Sleep(1000);
            }

            ListOrderItemsResponse response = _marketplace.GetMWSOrdersClient().ListOrderItems(request);

            _listOrderItemsQuota--;

            List<OrderItem> orderItems = response.ListOrderItemsResult.OrderItems.OrderItem;

            if (response.ListOrderItemsResult.IsSetNextToken())
            {
                orderItems.AddRange(ListOrderItemsByNextToken(response.ListOrderItemsResult.NextToken));
            }

            return orderItems;
        }

        private List<OrderItem> ListOrderItemsByNextToken(string nextToken)
        {
            ListOrderItemsByNextTokenRequest request = new ListOrderItemsByNextTokenRequest();
            request.NextToken = nextToken;
            request.SellerId = _marketplace.MerchantId;

            while (_listOrderItemsQuota < 1)
            {
                Thread.Sleep(1000);
            }

            ListOrderItemsByNextTokenResponse response = _marketplace.GetMWSOrdersClient().ListOrderItemsByNextToken(request);

            _listOrderItemsQuota--;

            List<OrderItem> orderItems = response.ListOrderItemsByNextTokenResult.OrderItems.OrderItem;

            if (response.ListOrderItemsByNextTokenResult.IsSetNextToken())
            {
                orderItems.AddRange(ListOrderItemsByNextToken(response.ListOrderItemsByNextTokenResult.NextToken));
            }

            return orderItems;
        }

        private void ProcessOrders(List<Order> orders)
        {
            foreach (Order amznOrder in orders)
            {
                

                try
                {
                    List<OrderItem> orderItems = ListOrderItems(amznOrder.AmazonOrderId);

                    PersistOrder(amznOrder, orderItems);
                }
                catch (Exception e)
                {
                    //if (!_pendingSync.Contains(amznOrder.AmazonOrderId))
                    //{
                    //    _pendingSync.Add(amznOrder.AmazonOrderId);
                    //}

                    _logger.Error(string.Format("Order ( {1} | {0} ) synchronization failed: {2}", amznOrder.AmazonOrderId, _marketplace.Code, e.Message));

                }
            }
        }

        private void PersistOrder(Order orderDto, List<OrderItem> orderItemsDto)
        {
            AmznOrder order = _dataContext.AmznOrders
                .SingleOrDefault(p => p.MarketplaceID == _marketplace.ID && p.Code.Equals(orderDto.AmazonOrderId));

            if (order == null)
            {
                order = new AmznOrder();
                order.Code = orderDto.AmazonOrderId;
                order.MarketplaceID = _marketplace.ID;
            }
          
            order.Status = orderDto.OrderStatus.ToString();
            order.LastUpdatedDate = orderDto.LastUpdateDate;
            order.BuyerName = orderDto.BuyerName != null ? orderDto.BuyerName : "";
            order.PaymentMethod = orderDto.PaymentMethod.ToString();
            order.PurchaseDate = orderDto.PurchaseDate;
            order.ShipServiceLevel = orderDto.ShipServiceLevel;
            order.ShipmentServiceLevelCategory = orderDto.ShipmentServiceLevelCategory;
            order.Total = orderDto.IsSetOrderTotal() ? decimal.Parse(orderDto.OrderTotal.Amount) : 0;
            order.LastSyncTime = _currentSyncTime;
            order.AddressLine1 = orderDto.IsSetShippingAddress() ? orderDto.ShippingAddress.AddressLine1 : "";
            order.AddressLine2 = orderDto.IsSetShippingAddress() ? orderDto.ShippingAddress.AddressLine2 : "";
            order.AddressLine3 = orderDto.IsSetShippingAddress() ? orderDto.ShippingAddress.AddressLine3 : "";
            order.City = orderDto.IsSetShippingAddress() ? orderDto.ShippingAddress.City : "";
            order.CountryCode = orderDto.IsSetShippingAddress() ? orderDto.ShippingAddress.CountryCode : "";
            order.County = orderDto.IsSetShippingAddress() ? orderDto.ShippingAddress.County : "";
            order.District = orderDto.IsSetShippingAddress() ? orderDto.ShippingAddress.District : "";
            order.Phone = orderDto.IsSetShippingAddress() ? orderDto.ShippingAddress.Phone : "";
            order.PostalCode = orderDto.IsSetShippingAddress() ? orderDto.ShippingAddress.PostalCode : "";
            order.StateOrRegion = orderDto.IsSetShippingAddress() ? orderDto.ShippingAddress.StateOrRegion : "";

            if (order.Code.Equals("108-3767246-4952227"))
            {
 
            }

            foreach (OrderItem orderItemDto in orderItemsDto)
            {
                AmznOrderItem orderItem = order.OrderItems.SingleOrDefault(p => p.ListingItem.Item.ItemLookupCode.Equals(orderItemDto.SellerSKU));

                if (orderItem == null)
                {
                    orderItem = new AmznOrderItem();
                    orderItem.Code = orderItemDto.OrderItemId;

                    orderItem.ListingItem = _dataContext.AmznListingItems.Single(p => 
                        p.MarketplaceID.Equals(_marketplace.ID) &&
                        p.Item.ItemLookupCode.Equals(orderItemDto.SellerSKU));

                    order.OrderItems.Add(orderItem);
                }

                AmznListingItem listingItem = orderItem.ListingItem;

                if (listingItem.LastSyncTime < orderDto.PurchaseDate)
                {
                    int qtyChange = (int)orderItemDto.QuantityOrdered - orderItem.QuantityOrdered;

                    if (qtyChange > 0)
                    {
                        listingItem.Quantity = listingItem.Quantity - qtyChange;
                        listingItem.LastSyncTime = _currentSyncTime;
                    }
                }

                orderItem.ItemPrice = orderItemDto.IsSetItemPrice() ? decimal.Parse(orderItemDto.ItemPrice.Amount) : 0;
                orderItem.ShippingPrice = orderItemDto.IsSetShippingPrice() ? decimal.Parse(orderItemDto.ShippingPrice.Amount) : 0;
                orderItem.PromotionDiscount = orderItemDto.IsSetPromotionDiscount() ? decimal.Parse(orderItemDto.PromotionDiscount.Amount) : 0;
                orderItem.ShippingDiscount = orderItemDto.IsSetShippingDiscount() ? decimal.Parse(orderItemDto.ShippingDiscount.Amount) : 0;
                orderItem.QuantityOrdered = (int)orderItemDto.QuantityOrdered;
                orderItem.QuantityShipped = (int)orderItemDto.QuantityShipped;
            }
        }

        private void RestoreEvent2Sec(object source, ElapsedEventArgs e)
        {
            if (_listOrderItemsQuota < 30)
            {
                _listOrderItemsQuota++;
            }
        }

        private void RestoreEvent1Min(object source, ElapsedEventArgs e)
        {
            if (_listOrdersQuota < 6)
            {
                _listOrdersQuota++;
            }

            if (_getOrdersQuota < 6)
            {
                _getOrdersQuota++;
            }
        }
    }

    public delegate void PublishingResultHandler(List<PublishingResult> results);

    public class PublishingResult
    {
        public bool HasError { get; set; }

        public ListingItemDto Data { get; set; }

        public string Message { get; set; }
    }

}
