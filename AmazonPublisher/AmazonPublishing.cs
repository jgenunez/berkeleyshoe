using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MarketplaceWebService;
using System.IO;
using System.Xml.Serialization;
using System.Globalization;
using MarketplaceWebService.Model;
using System.Data.SqlClient;
using System.Data.Objects.DataClasses;
using System.Text.RegularExpressions;
using MarketplaceWebServiceProducts;
using MarketplaceWebServiceProducts.Model;
using System.Threading;
using BerkeleyEntities;

namespace AmazonPublisher
{
    

    public partial class AmazonPublishing : Form
    {
        public string productFeed, priceFeed, relationshipFeed, inventoryFeed, imageFeed, inventoryLoader;
        string applicationName = "berkeley";
        string applicationVersion = "1.0";
        string accessKeyId = null, secretAccessKey = null, merchantId = null, marketplaceId = null;

        int currentMessage = 1;

        public static berkeleyEntities DataContext = new berkeleyEntities();

        public static string PRODUCT_TYPE = "_POST_PRODUCT_DATA_";
        public static string PRICING_TYPE = "_POST_PRODUCT_PRICING_DATA_";
        public static string RELATIONSHIP_TYPE = "_POST_PRODUCT_RELATIONSHIP_DATA_";
        public static string INVENTORY_TYPE = "_POST_INVENTORY_AVAILABILITY_DATA_";
        public static string INVENTORY_LOADER = "_POST_FLAT_FILE_INVLOADER_DATA_";

        public List<bsi_quantities_message> messages = new List<bsi_quantities_message>();

        List<bsi_quantities> postDetailsCreate = new List<bsi_quantities>();
        List<bsi_quantities> postDetailsUpdate = new List<bsi_quantities>();
        List<bsi_quantities> postDetailsError = new List<bsi_quantities>();
        List<bsi_quantities> postDetailsCreateFlat = new List<bsi_quantities>();
        List<bsi_quantities> postDetailsCreateXml = new List<bsi_quantities>();

        Dictionary<string, string> feedSubmissions = new Dictionary<string, string>();

        public AmazonPublishing()
        {
            InitializeComponent();

            productFeed = "";
            priceFeed = "";
            relationshipFeed = "";
            inventoryFeed = "";
            imageFeed = "";
            inventoryLoader = "";

            cbMarketplace.SelectedItem = "ShopUsLast";

            List<string> temp6 = DataContext.bsi_posts.Select(p => p.sellingFormat).Distinct().ToList();
            
            List<bsi_quantities> postDetails = DataContext.bsi_quantities.Where(p => p.bsi_posts.status == 60 && p.bsi_posts.marketplace == 1).ToList();

            postDetails.ForEach(p => bsiquantitiesBindingSource.Add(p));

            var temp = postDetails.SelectMany(p => p.bsi_quantities_message).Where(p => p.@checked == false)
                .Select(p => new { p.submissionId, p.submissionType })
                .Distinct();

            foreach (var message in temp)
            {
                feedSubmissions.Add(message.submissionId, message.submissionType);
            }


            messages.AddRange(postDetails.SelectMany(p => p.bsi_quantities_message));

        }

        private void btnPublish_Click(object sender, EventArgs e)
        {
            

            if (postDetailsCreateXml.Count == 0 && postDetailsUpdate.Count == 0)
            {
                MessageBox.Show("Nothing to publish!");
                return;
            }


            btnPublish.Enabled = false;

            //if (postDetailsCreateFlat.Count > 0)
            //{
            //    submitFeed(toAmznInventoryLoader(postDetailsCreateFlat), INVENTORY_LOADER);
            //}
            
           
            createAmznFeeds();

            foreach(bsi_quantities postDetail in postDetailsCreate)
            {
                postDetail.bsi_posts.productFeedId = productFeed;
                postDetail.bsi_posts.relationshipFeedId = relationshipFeed;
                //post.imageFeedId = imagefeedid;
                postDetail.bsi_posts.inventoryFeedId = inventoryFeed;
                postDetail.bsi_posts.priceFeedId = priceFeed;
                postDetail.bsi_posts.status = 60;
                //postDetailsConfirm.Add(postDetail);
            }

            foreach (bsi_quantities postDetail in postDetailsUpdate)
            {
                postDetail.bsi_posts.productFeedId = productFeed;
                postDetail.bsi_posts.relationshipFeedId = relationshipFeed;
                //post.imageFeedId = imagefeedid;
                postDetail.bsi_posts.inventoryFeedId = inventoryFeed;
                postDetail.bsi_posts.priceFeedId = priceFeed;
                postDetail.bsi_posts.status = 60;
                //postDetailsConfirm.Add(postDetail);
            }

            btnPending.Enabled = true;

            DataContext.SaveChanges();
        }

        private void getPendingPostDetail()
        {
            List<bsi_quantities> pendingPostDetails = null;

            if (cbMarketplace.SelectedItem.Equals("ShopUsLast"))
            {
                pendingPostDetails = DataContext.bsi_quantities.Where(p => ((p.bsi_posts.marketplace == 1 && p.bsi_posts.status == 10))).ToList();
            }
            else if (cbMarketplace.SelectedItem.Equals("HarvardStation"))
            {
                pendingPostDetails = DataContext.bsi_quantities.Where(p => ((p.bsi_posts.marketplace == 2 && p.bsi_posts.status == 10))).ToList();
            }

            if (pendingPostDetails.Count == 0) { MessageBox.Show("There are no pending item to publish !"); return; }

            foreach (bsi_quantities postDetail in pendingPostDetails)
            {
                if (postDetail.GTIN == null || postDetail.Item == null || postDetail.bsi_posts == null || postDetail.bsi_posts.bsi_posting == null)
                {
                    DataContext.bsi_quantities.DeleteObject(postDetail);
                }
                else
                {
                    if (postDetail.Exist)
                    {
                        postDetailsUpdate.Add(postDetail);
                        bsiquantitiesBindingSource.Add(postDetail);
                    }
                    else
                    {
                        postDetailsCreate.Add(postDetail);
                        bsiquantitiesBindingSource.Add(postDetail);
                    }
                }              
            }

            foreach(bsi_posts post in DataContext.bsi_posts.Where(p => p.bsi_quantities.Count == 0))
            {
                DataContext.bsi_posts.DeleteObject(post);
            }

            foreach (bsi_quantities postDetail in DataContext.bsi_quantities.Where(p => p.bsi_posts == null))
            {
                DataContext.bsi_quantities.DeleteObject(postDetail);
            }

            DataContext.SaveChanges();

            getAmznProductInfo(postDetailsCreate);

            foreach (bsi_quantities postDetail in postDetailsCreate)
            {
                postDetailsCreateXml.Add(postDetail);
            }


            if (postDetailsError.Count > 0)
            {
                MessageBox.Show(postDetailsError.Count.ToString() + " product(s) don't have GTIN !");
            }
            
        }

        public void getProcessingReport(string feedSubmissionId)
        {
            GetFeedSubmissionResultRequest request = new GetFeedSubmissionResultRequest();

            request.FeedSubmissionId = feedSubmissionId;

            request.Merchant = merchantId;
            MemoryStream stream = new MemoryStream();
            request.FeedSubmissionResult = stream;

            GetFeedSubmissionResultResponse response = getFeedMWS().GetFeedSubmissionResult(request);
            GetFeedSubmissionResultResult result = response.GetFeedSubmissionResultResult;

            

            ASCIIEncoding ascii = new ASCIIEncoding();
            String xmlinstance = ascii.GetString(stream.ToArray());
            AmazonEnvelope envelope = (AmazonEnvelope)Deserialize(typeof(AmazonEnvelope), xmlinstance);

            foreach (AmazonEnvelopeMessage message in envelope.Message)
            {
                ProcessingReport processingReport = (ProcessingReport)message.Item;

                if (processingReport.StatusCode.Equals(ProcessingReportStatusCode.Processing))
                {
                    throw new MarketplaceWebServiceException("Feed is processing!");
                }
                else if (processingReport.StatusCode.Equals(ProcessingReportStatusCode.Rejected))
                {
                    throw new MarketplaceWebServiceException("Feed was rejected!");
                }

                if (processingReport.StatusCode.Equals(ProcessingReportStatusCode.Complete))
                {
                    string successrate = processingReport.ProcessingSummary.MessagesSuccessful + "/" + processingReport.ProcessingSummary.MessagesProcessed;

                    foreach(bsi_quantities_message msg in DataContext.bsi_quantities_message.Where(p => p.submissionId.Equals(feedSubmissionId)))
                    {
                        msg.confirmed = true;
                        msg.@checked = true;
                    }

                    if (processingReport.Result != null)
                    {
                        foreach (ProcessingReportResult processingResult in processingReport.Result)
                        {
                            if (processingResult.ResultCode.Equals(ProcessingReportResultResultCode.Warning)) { continue; }

                            bsi_quantities_message qtyMessage = DataContext.bsi_quantities_message.SingleOrDefault(p => p.messageNumber.Equals(processingResult.MessageID) && p.submissionId.Equals(feedSubmissionId));

                            if (qtyMessage != null)
                            {
                                qtyMessage.confirmed = false;
                                qtyMessage.errorMessage = processingResult.ResultDescription;
                            }
                        } 
                    }

                    //feedSubmissions.Remove(feedSubmissionId);
                }
            }
            
        }

        public bool getFlatProcessingReport(string feedSubmissionId)
        {
            GetFeedSubmissionResultRequest request = new GetFeedSubmissionResultRequest();

            request.FeedSubmissionId = feedSubmissionId;


            request.Merchant = merchantId;
            MemoryStream stream = new MemoryStream();
            request.FeedSubmissionResult = stream;

            GetFeedSubmissionResultResponse response = null;

            try
            {
                response = getFeedMWS().GetFeedSubmissionResult(request);
            }
            catch (MarketplaceWebServiceException e)
            {
                MessageBox.Show(e.ErrorCode);
                return false;
            }

            GetFeedSubmissionResultResult result = response.GetFeedSubmissionResultResult;

            ASCIIEncoding ascii = new ASCIIEncoding();
            String document = ascii.GetString(stream.ToArray());

            string[] line = document.Split(new Char[1] { '\n' });

            messages.Where(p => p.submissionId.Equals(feedSubmissionId)).ToList().ForEach(p => p.confirmed = true);

            for (int i = 5; i < line.Length; i++)
            {
                if (String.IsNullOrEmpty(line[i])) { continue; }

                string[] column = line[i].Split(new Char[1] { '\t' });

                string sku = column[1];
                string type = column[3];
                string errorMessage = column[4];

                if (type.Equals("Error"))
                {
                    bsi_quantities_message message = messages
                        .Single(p => p.bsi_quantities.Item.ItemLookupCode.Equals(sku) && p.submissionId.Equals(feedSubmissionId));

                    message.confirmed = false;
                    message.errorMessage = errorMessage;
                    
                }

            }


            return true;
        }

        private MarketplaceWebServiceClient getFeedMWS()
        {
            MarketplaceWebServiceConfig config = new MarketplaceWebServiceConfig();

            config.ServiceURL = "https://mws.amazonservices.com";

            config.SetUserAgentHeader(
              applicationName,
               applicationVersion,
               "C#",
               "<Parameter 1>", "<Parameter 2>");

            MarketplaceWebServiceClient service =
                new MarketplaceWebServiceClient(
                    accessKeyId,
                    secretAccessKey,
                    applicationName,
                    applicationVersion,
                    config);


            return service;
        }

        private MarketplaceWebServiceProductsClient getProductsMWS()
        {
            MarketplaceWebServiceProductsConfig config = new MarketplaceWebServiceProductsConfig();

            config.ServiceURL = "https://mws.amazonservices.com/Products/2011-10-01";

            MarketplaceWebServiceProductsClient service =
                new MarketplaceWebServiceProductsClient(
                    applicationName,
                    applicationVersion,
                    accessKeyId,
                    secretAccessKey,
                    config);


            return service;
        }

        public void getAmznProductInfo(List<bsi_quantities> postDetails)
        {

            foreach (IGrouping<string, bsi_quantities> group in postDetails.GroupBy(p => p.IdType))
            {
                int cursor = 0;



                bsi_quantities[] postDetailArray = group.ToArray();

                while (cursor < postDetailArray.Length)
                {
                    GetMatchingProductForIdRequest request = new GetMatchingProductForIdRequest();
                    request.SellerId = merchantId;
                    request.MarketplaceId = marketplaceId;
                    request.IdList = new IdListType();
                    request.IdList.Id = new List<string>();
                    request.IdType = group.Key;
                    
                    for (int i = 0; i < 5; i++)
                    {
                        if (cursor == postDetailArray.Length) { continue; }

                        bsi_quantities postDetail = postDetailArray[cursor];


                        request.IdList.Id.Add(postDetailArray[cursor].GTIN);
                        //if (postDetail.ASIN == null)
                        //{
                        //    request.IdList.Id.Add(postDetailArray[cursor].GTIN);
                        //}
                        //else
                        //{
                        //    request.IdList.Id.Add(postDetailArray[cursor].ASIN);
                        //    request.IdType = "ASIN";
                        //}

                        cursor++;
                    }

                    GetMatchingProductForIdResponse response = null;

                    try
                    {
                        
                        response = getProductsMWS().GetMatchingProductForId(request);
                    }
                    catch (Exception)
                    {
                        
                        throw;
                    }

                    Thread.Sleep(3000);

                    if (response.IsSetGetMatchingProductForIdResult())
                    {
                        foreach (GetMatchingProductForIdResult result in response.GetMatchingProductForIdResult)
                        {
                            bsi_quantities postDetail = null;

                            var temp = postDetails.Where(p => p.Item.Aliases.Any(a => a.Alias1.Equals(result.Id)));

                            

                            postDetail = temp.First();

                            if (postDetail == null)
                            {
                                postDetail = postDetails.Single(p => p.ASIN.Equals(result.Id));
                            }

                            postDetail.AmznProductInfo = result;
                        }
                    }
                    else
                        MessageBox.Show("GetMatchingProductForIdResult Property is not set"); 
                }               
            }            
        }

        private string createAmznFeeds()
        {          
            AmazonEnvelope productEnvelope, priceEnvelope, relationshipEnvelope, inventoryEnvelope;

            productEnvelope = new AmazonEnvelope();
            productEnvelope.MessageType = AmazonEnvelopeMessageType.Product;
            productEnvelope.Header = new Header();
            productEnvelope.Header.MerchantIdentifier = merchantId;
            productEnvelope.Header.DocumentVersion = "1.01";

            priceEnvelope = new AmazonEnvelope();
            priceEnvelope.MessageType = AmazonEnvelopeMessageType.Price;
            priceEnvelope.Header = new Header();
            priceEnvelope.Header.MerchantIdentifier = merchantId;
            priceEnvelope.Header.DocumentVersion = "1.01";

            relationshipEnvelope = new AmazonEnvelope();
            relationshipEnvelope.MessageType = AmazonEnvelopeMessageType.Relationship;
            relationshipEnvelope.Header = new Header();
            relationshipEnvelope.Header.MerchantIdentifier = merchantId;
            relationshipEnvelope.Header.DocumentVersion = "1.01";

            inventoryEnvelope = new AmazonEnvelope();
            inventoryEnvelope.MessageType = AmazonEnvelopeMessageType.Inventory;
            inventoryEnvelope.Header = new Header();
            inventoryEnvelope.Header.MerchantIdentifier = merchantId;
            inventoryEnvelope.Header.DocumentVersion = "1.01";

            List<AmazonEnvelopeMessage> productFeedMessages, priceFeedMessages, relationshipFeedMessages, inventoryFeedMessages;

            productFeedMessages = new List<AmazonEnvelopeMessage>();
            priceFeedMessages = new List<AmazonEnvelopeMessage>();
            relationshipFeedMessages = new List<AmazonEnvelopeMessage>();
            inventoryFeedMessages = new List<AmazonEnvelopeMessage>();

            foreach (bsi_quantities postDetail in postDetailsUpdate)
            {
                priceFeedMessages.Add(createAmznPriceMsg(postDetail));
                inventoryFeedMessages.Add(createAmznInventoryMsg(postDetail));
            }

            var groups = postDetailsCreateXml.GroupBy(p => p.postId);

            foreach (IGrouping<int,bsi_quantities> group in groups)
            {
                relationshipFeedMessages.Add(createAmznRelationshipMsg(group.First()));
                productFeedMessages.Add(createAmznParentProductMsg(group.First()));

                foreach (bsi_quantities postDetail in group)
                {
                    priceFeedMessages.Add(createAmznPriceMsg(postDetail));
                    inventoryFeedMessages.Add(createAmznInventoryMsg(postDetail));
                    productFeedMessages.Add(createAmznProductMsg(postDetail));
                }

            }
            
            

            productEnvelope.Message = productFeedMessages.ToArray();
            priceEnvelope.Message = priceFeedMessages.ToArray();
            relationshipEnvelope.Message = relationshipFeedMessages.ToArray();
            inventoryEnvelope.Message = inventoryFeedMessages.ToArray();

            try
            {
                if (productFeedMessages.Count > 0) { submitFeed(Serialize(productEnvelope), PRODUCT_TYPE); }

                if (priceFeedMessages.Count > 0) { submitFeed(Serialize(priceEnvelope), PRICING_TYPE); }

                if (relationshipFeedMessages.Count > 0) { submitFeed(Serialize(relationshipEnvelope), RELATIONSHIP_TYPE); }

                if (inventoryFeedMessages.Count > 0) { submitFeed(Serialize(inventoryEnvelope), INVENTORY_TYPE); }              
                
            }
            catch (MarketplaceWebServiceException e)
            {
                
                MessageBox.Show(e.Message);
            }

            return Serialize(productEnvelope);
        }

        private void submitFeed(string feedContent, string feedType)
        {
            SubmitFeedRequest request = new SubmitFeedRequest();
            request.FeedType = feedType;
            request.Merchant = merchantId;
            request.MarketplaceIdList = new IdList();
            request.MarketplaceIdList.Id = new List<string>(new string[] { marketplaceId });

            string feedcontent = feedContent;

            byte[] byteArray = Encoding.ASCII.GetBytes(feedcontent);
            MemoryStream stream = new MemoryStream(byteArray);


            request.FeedContent = stream;
            request.ContentMD5 = MarketplaceWebServiceClient.CalculateContentMD5(request.FeedContent);
            request.FeedContent.Position = 0;

            SubmitFeedResponse response = getFeedMWS().SubmitFeed(request);

            if (response.IsSetSubmitFeedResult())
            {
                if (response.SubmitFeedResult.IsSetFeedSubmissionInfo())
                {
                    FeedSubmissionInfo info = response.SubmitFeedResult.FeedSubmissionInfo;

                    feedSubmissions.Add(info.FeedSubmissionId, feedType);

                    foreach (bsi_quantities_message message in messages.Where(p => p.submissionType.Equals(feedType) && p.submissionId == null))
                    {
                        message.submissionId = info.FeedSubmissionId;
                        message.submmitedDate = DateTime.Parse(info.SubmittedDate);
                    }

                }

                else { MessageBox.Show("FeedSubmissionInfo Not Set"); }
            }

            else { MessageBox.Show("Feed Result Not Set"); }
        }

        private AmazonEnvelopeMessage createAmznProductMsg(bsi_quantities postDetail)
        {
            BerkeleyEntities.Item item = postDetail.Item;
            bsi_posting posting = postDetail.bsi_posts.bsi_posting;
            bsi_posts post = postDetail.bsi_posts;

            if (postDetail.Item == null || postDetail.GTIN == null)
            {
                //post.status = ;
                return null;
            }

            StandardProductID sid = new StandardProductID();

            string asin = postDetail.ASIN;

            if (asin == null)
            {
                sid.Value = postDetail.GTIN;

                switch (postDetail.IdType)
                {
                    case "UPC": sid.Type = StandardProductIDType.UPC; break;
                    case "EAN": sid.Type = StandardProductIDType.EAN; break;
                }
            }
            else
            {
                sid.Value = asin;
                sid.Type = StandardProductIDType.ASIN;
            }

            AmazonEnvelopeMessage message = new AmazonEnvelopeMessage();
            message.OperationType = AmazonEnvelopeMessageOperationType.Update;

            ConditionInfo conditioninfo = new ConditionInfo();
            conditioninfo.ConditionType = ConditionType.New;

            ProductDescriptionData descriptiondata = new ProductDescriptionData();
            descriptiondata.Brand = postDetail.AmznBrand;
            descriptiondata.Description = posting.fullDescription;
            descriptiondata.Title = postDetail.title;

            string itemType = postDetail.AmznType;

            if (itemType != null)
            {
                descriptiondata.ItemType = itemType;
            }
            

            Shoes shoes = new Shoes();

            shoes.ClothingType = ShoesClothingType.Shoes;
            shoes.ClassificationData = new ShoesClassificationData();
            shoes.VariationData = new ShoesVariationData();
            shoes.ClassificationData.Department = postDetail.AmznGender;
            shoes.ClassificationData.MaterialType = new string[1] { postDetail.AmznMaterial };
            shoes.VariationData.Size = postDetail.AmznSize;
            shoes.VariationData.ParentageSpecified = true;
            shoes.VariationData.Parentage = ShoesVariationDataParentage.child;


            if (!String.IsNullOrEmpty(postDetail.AmznShade))
            {
                shoes.VariationData.Color = postDetail.AmznShade;

                if (!String.IsNullOrEmpty(postDetail.AmznColor)) { shoes.ClassificationData.ColorMap = postDetail.AmznColor; }
            }
            else
            {
                if (!String.IsNullOrEmpty(postDetail.AmznColor)) { shoes.VariationData.Color = postDetail.AmznColor; }
            }

            ProductProductData productdata = new ProductProductData();
            productdata.Item = shoes;

            Product product = new Product();
            product.SKU = postDetail.Item.ItemLookupCode;
            product.StandardProductID = sid;
            product.ItemPackageQuantity = "1";
            product.NumberOfItems = "1";
            product.LaunchDate = post.startDate;
            //product.ReleaseDateSpecified = true;
            //product.ReleaseDate = post.startDate;
            product.Condition = conditioninfo;
            product.DescriptionData = descriptiondata;
            product.ProductData = productdata;

            message.Item = product;
            message.OperationTypeSpecified = true;
            message.MessageID = currentMessage.ToString();

            bsi_quantities_message postDetailMessage = new bsi_quantities_message() { confirmed = false, messageNumber = currentMessage.ToString(), submissionType = PRODUCT_TYPE, @checked = false };
            messages.Add(postDetailMessage);
            postDetail.bsi_quantities_message.Add(postDetailMessage);

            currentMessage++;

            return message;
        }

        private AmazonEnvelopeMessage createAmznParentProductMsg(bsi_quantities postDetail)
        {
                bsi_posting posting = postDetail.bsi_posts.bsi_posting;
                bsi_posts post = postDetail.bsi_posts;
              
                AmazonEnvelopeMessage message = new AmazonEnvelopeMessage();
                message.OperationType = AmazonEnvelopeMessageOperationType.Update;

                ConditionInfo conditioninfo = new ConditionInfo();
                conditioninfo.ConditionType = ConditionType.New;

                ProductDescriptionData parentDescriptionData = new ProductDescriptionData();
                parentDescriptionData.Brand = postDetail.AmznBrand;
                parentDescriptionData.Description = posting.fullDescription;
                parentDescriptionData.Title = post.title;

                string itemType = postDetail.AmznType;

                if (itemType != null)
                {
                    parentDescriptionData.ItemType = itemType;
                }
                
                Shoes parentShoes = new Shoes();
                parentShoes.ClothingType = ShoesClothingType.Shoes;
                parentShoes.ClassificationData = new ShoesClassificationData();
                parentShoes.VariationData = new ShoesVariationData();

                parentShoes.ClassificationData.MaterialType = new string[1] { postDetail.AmznMaterial };
                parentShoes.ClassificationData.Department = postDetail.AmznGender;
                parentShoes.VariationData.VariationTheme = ShoesVariationDataVariationTheme.Size;
                parentShoes.VariationData.VariationThemeSpecified = true;
                parentShoes.VariationData.ParentageSpecified = true;
                parentShoes.VariationData.Parentage = ShoesVariationDataParentage.parent;

                if (!String.IsNullOrEmpty(postDetail.AmznShade)) { parentShoes.VariationData.Color = postDetail.AmznShade; }

                ProductProductData parentProductData = new ProductProductData();
                parentProductData.Item = parentShoes;

                Product parentProduct = new Product();
                parentProduct.SKU = posting.sku;
                parentProduct.NumberOfItems = "1";
                parentProduct.ItemPackageQuantity = "1";
                parentProduct.Condition = conditioninfo;
                parentProduct.DescriptionData = parentDescriptionData;
                parentProduct.ProductData = parentProductData;


                message.Item = parentProduct;
                message.OperationTypeSpecified = true;
                message.MessageID = currentMessage.ToString();

                bsi_quantities_message postDetailMessage = new bsi_quantities_message() { confirmed = false, messageNumber = currentMessage.ToString(), submissionType = PRODUCT_TYPE };
                messages.Add(postDetailMessage);

                postDetail.bsi_quantities_message.Add(postDetailMessage);

                currentMessage++;

                return message;
        }

        private AmazonEnvelopeMessage createAmznPriceMsg(bsi_quantities postDetail)
        {
            AmazonEnvelopeMessage message = new AmazonEnvelopeMessage();
            message.OperationType = AmazonEnvelopeMessageOperationType.Update;

            OverrideCurrencyAmount oca = new OverrideCurrencyAmount();
            oca.currency = BaseCurrencyCodeWithDefault.USD;
            oca.Value = decimal.Parse(postDetail.bsi_posts.price);

            Price price = new Price();
            price.SKU = postDetail.Item.ItemLookupCode;
            price.StandardPrice = oca;

            message.Item = price;
            message.OperationTypeSpecified = true;
            message.MessageID = currentMessage.ToString();

            bsi_quantities_message postDetailMessage = new bsi_quantities_message() { confirmed = false, messageNumber = currentMessage.ToString(), submissionType = PRICING_TYPE };
            messages.Add(postDetailMessage);

            postDetail.bsi_quantities_message.Add(postDetailMessage);

            currentMessage++;

            return message;
        }

        private AmazonEnvelopeMessage createAmznRelationshipMsg(bsi_quantities postDetail)
        {
            bsi_posts post = postDetail.bsi_posts;

            List<RelationshipRelation> relations;

            Relationship relationship = new Relationship();

            relationship.ParentSKU = post.sku;

            relations = new List<RelationshipRelation>();
            AmazonEnvelopeMessage message = new AmazonEnvelopeMessage();

            foreach (bsi_quantities postdetail in post.bsi_quantities)
            {
                RelationshipRelation relation = new RelationshipRelation();
                relation.SKU = postdetail.Item.ItemLookupCode;
                relation.Type = RelationshipRelationType.Variation;

                relations.Add(relation);
            }

            relationship.Relation = relations.ToArray();

            message.OperationType = AmazonEnvelopeMessageOperationType.Update;
            message.Item = relationship;
            message.OperationTypeSpecified = true;
            message.MessageID = currentMessage.ToString();

            bsi_quantities_message postDetailMessage = new bsi_quantities_message() { confirmed = false, messageNumber = currentMessage.ToString(), submissionType = RELATIONSHIP_TYPE };
            messages.Add(postDetailMessage);

            postDetail.bsi_quantities_message.Add(postDetailMessage);

            currentMessage++;

            return message;
        }

        private AmazonEnvelopeMessage createAmznInventoryMsg(bsi_quantities postDetail)
        {
            AmazonEnvelopeMessage message = new AmazonEnvelopeMessage();

            Inventory inventory = new Inventory();

            inventory.SKU = postDetail.Item.ItemLookupCode;
            inventory.Item = postDetail.quantity.ToString();
            inventory.RestockDateSpecified = false;
            inventory.SwitchFulfillmentToSpecified = false;

            message.Item = inventory;
            message.MessageID = currentMessage.ToString();
            message.OperationType = AmazonEnvelopeMessageOperationType.Update;
            message.OperationTypeSpecified = true;

            bsi_quantities_message postDetailMessage = new bsi_quantities_message() { confirmed = false, messageNumber = currentMessage.ToString(), submissionType = INVENTORY_TYPE };
            messages.Add(postDetailMessage);

            postDetail.bsi_quantities_message.Add(postDetailMessage);

            currentMessage++;

            return message;
        }

        private AmazonEnvelopeMessage createAmznImageMsg(bsi_quantities postDetail)
        {
            return null;
        }
        

        //private string toAmazonImageSchema()
        //{
        //    AmazonEnvelope envelope = new AmazonEnvelope();
        //    envelope.MessageType = AmazonEnvelopeMessageType.ProductImage;
        //    envelope.Header = new Header();
        //    envelope.Header.MerchantIdentifier = merchantId;
        //    envelope.Header.DocumentVersion = "1.01";


        //    AmazonEnvelopeMessage message;

        //    List<AmazonEnvelopeMessage> messages = new List<AmazonEnvelopeMessage>();

        //    foreach (IGrouping<string, ExcelData> item in StyleGroups)
        //    {
        //        string url = "";
        //        ProductImage image = null;

        //        if (File.Exists(@"P:\products\" + item.ToList()[0].Brand + @"\" + item.Key + ".jpg"))
        //        {
        //            url = @"http://192.168.1.19/products/" + item.ToList()[0].Brand + @"/" + item.Key + ".jpg";
        //            image = new ProductImage();
        //            image.ImageType = ProductImageImageType.Main;
        //            image.SKU = item.Key;
        //            image.ImageLocation = url;
        //            message = new AmazonEnvelopeMessage();
        //            message.OperationType = AmazonEnvelopeMessageOperationType.Update;
        //            message.OperationTypeSpecified = true;
        //            message.Item = image;
        //            message.MessageID = item.ToList()[0].RowNumber;
        //            messages.Add(message);
        //        }

        //        for (int i = 2; i < 8; i++)
        //        {
        //            string temp = "-" + i.ToString();

        //            if (File.Exists(@"P:\products\" + item.ToList()[0].Brand + @"\" + item.Key + temp + ".jpg"))
        //            {

        //                url = @"http://192.168.1.19/products/" + item.ToList()[0].Brand + @"/" + item.Key + temp + ".jpg";
        //                image = new ProductImage();
        //                image.SKU = item.Key;
        //                image.ImageLocation = url;
        //                switch (i)
        //                {
        //                    case 2: image.ImageType = ProductImageImageType.PT1;
        //                        break;
        //                    case 3: image.ImageType = ProductImageImageType.PT2;
        //                        break;
        //                    case 4: image.ImageType = ProductImageImageType.PT3;
        //                        break;
        //                    case 5: image.ImageType = ProductImageImageType.PT4;
        //                        break;
        //                    case 6: image.ImageType = ProductImageImageType.PT5;
        //                        break;
        //                    case 7: image.ImageType = ProductImageImageType.PT6;
        //                        break;
        //                }
        //                message = new AmazonEnvelopeMessage();
        //                message.OperationType = AmazonEnvelopeMessageOperationType.Update;
        //                message.OperationTypeSpecified = true;
        //                message.Item = image;
        //                message.MessageID = item.ToList()[0].RowNumber;
        //                messages.Add(message);
        //            }
        //        }

        //    }

        //    envelope.Message = messages.ToArray();

        //    return Serialize(envelope);
        //}


        private string toAmznInventoryLoader(List<bsi_quantities> postDetails)
        {
            List<string> tabfile = new List<string>();

            string[] fields = new string[14] { "sku", "product-id", "product-id-type", 
                                                "price", "item-condition", "quantity", "add-delete", 
                                                "will-ship-internationally", "expedited-shipping", 
                                                "standard-plus", "item-note", "fulfillment-center-id", 
                                                "product-tax-code", "leadtime-to-ship" };

            tabfile.Add(String.Join("\t", fields));


            foreach (bsi_quantities postDetail in postDetails)
            {
                bsi_quantities_message message = new bsi_quantities_message();
                message.submissionType = INVENTORY_LOADER;
                message.messageNumber = currentMessage.ToString();
                message.bsi_quantities = postDetail;
                messages.Add(message);

                string[] data = new string[14] { postDetail.Item.ItemLookupCode, postDetail.GTIN, postDetail.IdType, postDetail.bsi_posts.price, "11", 
                                                postDetail.quantity.ToString(), "a", 
                                                "", "", "", "", "", "", "" };

                tabfile.Add(String.Join("\t", data));
                currentMessage++;
            }


            return String.Join("\n", tabfile.ToArray());
        }        

        public void errorHandler(int marketplace)
        {
            List<bsi_posts> postDetails = messages.Select(s => s.bsi_quantities.bsi_posts).Distinct().ToList();

            foreach (bsi_posts post in postDetails)
            {
                if (post.Confirmed == true) { post.status = 0; }

                else 
                {
                    if (post.bsi_quantities.All(p => p.Confirmed == false)) 
                    {
                        List<bsi_quantities> deleteList = post.bsi_quantities.ToList();
                        foreach (bsi_quantities postDetail in deleteList)
                        {
                            DataContext.bsi_quantities.DeleteObject(postDetail);

                        }
                        DataContext.bsi_posts.DeleteObject(post);
                    }
                    else
                    {
                        List<bsi_quantities> deleteList = post.bsi_quantities.Where(p => p.Confirmed == false).ToList();
                        foreach (bsi_quantities postDetail in deleteList)
                        {
                                DataContext.bsi_quantities.DeleteObject(postDetail);
                        }
                        post.status = 0;
                    }                              
                }             
            }
            messages.Clear();
            DataContext.SaveChanges();
            
        }

        public static void releaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;
                MessageBox.Show("Unable to release the Object " + ex.ToString());
            }
            finally
            {
                GC.Collect();
            }

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

        private void btnConfirm_Click(object sender, EventArgs e)
        {           
            try
            {

                List<string> keys = feedSubmissions.Keys.ToList();

               

                if (checkSubmissionStatus(keys))
                {
                    foreach (string feedSubmissionId in keys)
                    {
                        if (feedSubmissions[feedSubmissionId].Equals(INVENTORY_LOADER))
                        {
                            getFlatProcessingReport(feedSubmissionId);
                        }
                        else
                        {
                            getProcessingReport(feedSubmissionId);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Processing Report has not finish processing");
                    return;
                }
            }
            catch (MarketplaceWebServiceException x)
            {

                MessageBox.Show(x.Message);
                //DataContext.SaveChanges();
                return;
            }

            MessageBox.Show("Confirmation completed");
            DataContext.SaveChanges();

           
          
        }

        private bool checkSubmissionStatus(List<string> submissionIds)

        {
            GetFeedSubmissionListRequest request = new GetFeedSubmissionListRequest();
            request.FeedSubmissionIdList = new IdList();
            request.FeedSubmissionIdList.Id.AddRange(submissionIds);
            request.Merchant = merchantId;

            GetFeedSubmissionListResponse response = getFeedMWS().GetFeedSubmissionList(request);

            return response.GetFeedSubmissionListResult.FeedSubmissionInfo.All(p => p.FeedProcessingStatus.Equals("_DONE_"));
        }

        private void cbMarketplace_SelectedValueChanged(object sender, EventArgs e)
        {
            if (cbMarketplace.SelectedItem.Equals("ShopUsLast"))
            {
                accessKeyId = "AKIAIG7WFTMN2EQZUBKA";
                secretAccessKey = "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";
                merchantId = "A98JY3EWQYV6X";
                marketplaceId = "ATVPDKIKX0DER";
            }
            else if (cbMarketplace.SelectedItem.Equals("HarvardStation"))
            {
                accessKeyId = "AKIAJ4W3ALGZPVSGDYCA";
                secretAccessKey = "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";
                merchantId = "AOR65RSIHDNI7";
                marketplaceId = "ATVPDKIKX0DER";
            }
        }

        private void btnPending_Click(object sender, EventArgs e)
        {
            getPendingPostDetail();
        }

        private void dgvResult_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            bsi_quantities postDetail = dgvResult.Rows[e.RowIndex].DataBoundItem as bsi_quantities;

            bsiquantitiesmessageBindingSource.DataSource = postDetail.bsi_quantities_message;
        }

        private void btnRepublish_Click(object sender, EventArgs e)
        {
            string value = "";

            DialogResult dr = InputBox("Select Marketplace", "Choose Marketplace Number",ref value);

            if (dr.Equals(DialogResult.OK))
            {

                    errorHandler(int.Parse(value));
                    bsiquantitiesBindingSource.DataSource = null;



            }

            
        }

        public static DialogResult InputBox(string title, string promptText, ref string value)
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;


            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, 372, 13);
            textBox.SetBounds(12, 36, 372, 20);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right | AnchorStyles.Bottom;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 107);
            form.Controls.AddRange(new System.Windows.Forms.Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = true;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            value = textBox.Text;
            return dialogResult;
        }       
    }
}
