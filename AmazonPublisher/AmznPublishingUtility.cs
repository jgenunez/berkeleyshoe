using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;
using MarketplaceWebService.Model;
using System.IO;
using MarketplaceWebService;
using System.Xml.Serialization;

namespace Berkeley2
{
    public partial class AmznPublishingUtility : Form
    {
        string merchantId = "A98JY3EWQYV6X";
        string marketplaceId = "ATVPDKIKX0DER";
        string applicationName = "berkeley";
        string applicationVersion = "1.0";
        string accessKeyId = "AKIAIG7WFTMN2EQZUBKA";
        string secretAccessKey = "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";

        public BerkeleyEntities dataContext = new BerkeleyEntities();

        public AmznPublishingUtility()
        {
            InitializeComponent();
        }

        private void btnPublish_Click(object sender, EventArgs e)
        {
            var pendingPost = dataContext.bsi_posts.Where(p => p.marketplace == 1 && p.status == 10);

            createAmznFeeds(pendingPost.ToList());
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            var waitingPosts = dataContext.bsi_posts.Where(p => p.marketplace == 1 && p.status == 50);

            var productFeeds = waitingPosts.Select(p => new { p.productFeedId, FeedType = "_POST_PRODUCT_DATA_" }).Distinct();
            var priceFeeds = waitingPosts.Select(p => new { p.priceFeedId, FeedType = "_POST_PRODUCT_PRICING_DATA_" }).Distinct();
            var relationshipFeeds = waitingPosts.Select(p => new { p.relationshipFeedId, FeedType = "_POST_PRODUCT_RELATIONSHIP_DATA_" }).Distinct();


            foreach (var productFeed in productFeeds)
            {
                getProcessingReport(productFeed.FeedType, productFeed.productFeedId);
            }

            foreach (var priceFeed in priceFeeds)
            {
                getProcessingReport(priceFeed.FeedType, priceFeed.priceFeedId);
            }

            foreach (var relationshipFeed in relationshipFeeds)
            {
                getProcessingReport(relationshipFeed.FeedType, relationshipFeed.relationshipFeedId);
            }
        }

        private void getReadyToPublishPost()
        {
            


        }

        public bool getProcessingReport(string FeedType, string feedSubmissionID)
        {
            GetFeedSubmissionResultRequest request = new GetFeedSubmissionResultRequest();

            switch (FeedType)
            {
                case "_POST_PRODUCT_DATA_": request.FeedSubmissionId = feedSubmissionID;
                    break;
                case "_POST_PRODUCT_RELATIONSHIP_DATA_": request.FeedSubmissionId = feedSubmissionID;
                    break;
                case "_POST_PRODUCT_PRICING_DATA_": request.FeedSubmissionId = feedSubmissionID;
                    break;
                case "_POST_FLAT_FILE_INVLOADER_DATA_": request.FeedSubmissionId = feedSubmissionID;
                    break;
                case "_POST_PRODUCT_IMAGE_DATA_": request.FeedSubmissionId = feedSubmissionID;

                    break;
            }

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
                    MessageBox.Show(FeedType + " is Processing !");
                    return false;
                }
                else if (processingReport.StatusCode.Equals(ProcessingReportStatusCode.Rejected))
                {
                    MessageBox.Show(FeedType + " was rejected !");
                    return false;
                }
                if (processingReport.ProcessingSummary.MessagesProcessed.Equals("0"))
                {
                    MessageBox.Show(FeedType + " was Cancel");
                    UserInput.ForEach(p => p.ErrorMessage.Add(FeedType + " was Cancel"));
                    return false;
                }

                if (processingReport.Result != null)
                {
                    string successrate = processingReport.ProcessingSummary.MessagesSuccessful + "/" + processingReport.ProcessingSummary.MessagesProcessed;
                    currentNode.Text += " " + successrate;

                    foreach (ProcessingReportResult presult in processingReport.Result)
                    {
                        if (presult.ResultCode.Equals(ProcessingReportResultResultCode.Warning)) { continue; }

                        ExcelData item = UserInput.SingleOrDefault(p => p.RowNumber.Equals(presult.MessageID));
                        currentNode.Nodes.Add(item.SKU);
                        item.addErrorMessage(FeedType + ": " + presult.ResultDescription);

                    }
                }
            }
            return true;
        }

        private string createAmznFeeds(List<bsi_posts> pendingPosts)
        {
            List<bsi_posts> processedPosts = new List<bsi_posts>();

            AmazonEnvelope productEnvelope, priceEnvelope, relationshipEnvelope;

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

            List<AmazonEnvelopeMessage> productFeedMessages, priceFeedMessages, relationshipFeedMessages;

            productFeedMessages = new List<AmazonEnvelopeMessage>();
            priceFeedMessages = new List<AmazonEnvelopeMessage>();
            relationshipFeedMessages = new List<AmazonEnvelopeMessage>();

            int messageID = 1;

            foreach (bsi_posts post in pendingPosts)
            {

                if (!processedPosts.Contains(post))
                {
                    productFeedMessages.Add(buildAmznParentProductMessage(post, messageID));

                    messageID++;

                    relationshipFeedMessages.Add(buildAmznRelationshipMessage(post, messageID));

                    processedPosts.Add(post);

                    messageID++;
                }

                foreach (bsi_quantities listing in post.bsi_quantities)
                {
                    productFeedMessages.Add(buildAmznProductMessage(listing, messageID));

                    messageID++;

                    priceFeedMessages.Add(buildAmznPriceMessage(listing, messageID));

                    messageID++;
                }
                
            }

            productEnvelope.Message = productFeedMessages.ToArray();
            priceEnvelope.Message = priceFeedMessages.ToArray();
            relationshipEnvelope.Message = relationshipFeedMessages.ToArray();

            submitFeed(productEnvelope,  "_POST_PRODUCT_DATA_", pendingPosts);
            submitFeed(relationshipEnvelope, "_POST_PRODUCT_RELATIONSHIP_DATA_", pendingPosts);
            submitFeed(priceEnvelope,  "_POST_PRODUCT_PRICING_DATA_", pendingPosts);
            submitInventoryFeed(pendingPosts);

            pendingPosts.ForEach(p => p.status = 50);

            dataContext.SaveChanges();

            return Serialize(productEnvelope);
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

        private AmazonEnvelopeMessage buildAmznProductMessage(bsi_quantities listing, int messageID)
        {
            bsi_posts post = listing.bsi_posts;
            bsi_posting posting = post.bsi_posting;


            AmazonEnvelopeMessage message = new AmazonEnvelopeMessage();
            message.OperationType = AmazonEnvelopeMessageOperationType.Update;
            message.OperationTypeSpecified = true;
            message.MessageID = messageID.ToString();

            StandardProductID sid = new StandardProductID();
            sid.Type = StandardProductIDType.UPC;

            ConditionInfo conditionInfo = new ConditionInfo();
            conditionInfo.ConditionType = ConditionType.New;

            if (listing.Item.Aliases.Count > 0)
                sid.Value = listing.Item.Aliases.First().Alias1;
            else
                return null;

            ProductDescriptionData descriptionData = new ProductDescriptionData();
            descriptionData.Brand = ToTitleCase(listing.Item.SubDescription1);
            descriptionData.Description = posting.fullDescription;
            descriptionData.Title = post.title;
            descriptionData.ItemType = toAmznType(posting.category, posting.style);

            Shoes shoes = new Shoes();
            shoes.ClothingType = ShoesClothingType.Shoes;
            shoes.ClassificationData = new ShoesClassificationData();
            shoes.VariationData = new ShoesVariationData();
            shoes.ClassificationData.Department = getAmznGender(listing.Item.SubDescription3);
            shoes.ClassificationData.MaterialType = new string[1] { posting.material.ToLower() };
            shoes.ClassificationData.ColorMap = posting.color;
            shoes.VariationData.Size = getAmznSize(listing.size, listing.width, posting.gender);

            shoes.VariationData.ParentageSpecified = true;
            shoes.VariationData.Parentage = ShoesVariationDataParentage.child;

            if (!String.IsNullOrEmpty(posting.shade))
                shoes.VariationData.Color = posting.shade;

            ProductProductData productdata = new ProductProductData();
            productdata.Item = shoes;

            Product product = new Product();
            product.SKU = listing.Item.ItemLookupCode;
            product.StandardProductID = sid;
            product.ItemPackageQuantity = "1";
            product.NumberOfItems = "1";
            product.LaunchDate = DateTime.Now;
            product.ReleaseDateSpecified = true;
            product.ReleaseDate = DateTime.Now;
            product.Condition = conditionInfo;
            product.DescriptionData = descriptionData;
            product.ProductData = productdata;

            message.Item = product;

            return message;
        }

        private AmazonEnvelopeMessage buildAmznParentProductMessage(bsi_posts post, int messageID)
        {
            bsi_posting posting = post.bsi_posting;

            AmazonEnvelopeMessage message = new AmazonEnvelopeMessage();
            message.OperationType = AmazonEnvelopeMessageOperationType.Update;
            message.OperationTypeSpecified = true;
            message.MessageID = messageID.ToString();

 

            ConditionInfo conditionInfo = new ConditionInfo();
            conditionInfo.ConditionType = ConditionType.New;

            ProductDescriptionData parentDescriptionData = new ProductDescriptionData();
            parentDescriptionData.Brand = ToTitleCase(posting.brand);
            parentDescriptionData.Description = posting.fullDescription;
            parentDescriptionData.Title = post.title;
            parentDescriptionData.ItemType = toAmznType(posting.category, posting.style);
            

            Shoes parentShoes = new Shoes();
            parentShoes.ClothingType = ShoesClothingType.Shoes;
            parentShoes.ClassificationData = new ShoesClassificationData();
            parentShoes.VariationData = new ShoesVariationData();

            parentShoes.ClassificationData.MaterialType = new string[1] { posting.material.ToLower() };
            parentShoes.ClassificationData.Department = getAmznGender(posting.gender);
            parentShoes.VariationData.VariationTheme = ShoesVariationDataVariationTheme.Size;
            parentShoes.VariationData.VariationThemeSpecified = true;
            parentShoes.VariationData.ParentageSpecified = true;
            parentShoes.VariationData.Parentage = ShoesVariationDataParentage.parent;


            ProductProductData parentProductData = new ProductProductData();
            parentProductData.Item = parentShoes;

            Product parentProduct = new Product();
            parentProduct.SKU = post.sku;
            parentProduct.NumberOfItems = "1";
            parentProduct.ItemPackageQuantity = "1";
            parentProduct.Condition = conditionInfo;
            parentProduct.DescriptionData = parentDescriptionData;
            parentProduct.ProductData = parentProductData;

            message.Item = parentProduct;

            return message;
        }

        private AmazonEnvelopeMessage buildAmznRelationshipMessage(bsi_posts post, int messageID)
        {
            AmazonEnvelopeMessage message = new AmazonEnvelopeMessage();
            message.OperationType = AmazonEnvelopeMessageOperationType.Update;
            message.OperationTypeSpecified = true;
            message.MessageID = messageID.ToString();

            Relationship relationship = new Relationship();
            relationship.ParentSKU = post.sku;

            List<RelationshipRelation> relations = new List<RelationshipRelation>();

            foreach (bsi_quantities listing in post.bsi_quantities)
            {
                RelationshipRelation relation = new RelationshipRelation();
                relation.SKU = listing.Item.ItemLookupCode;
                relation.Type = RelationshipRelationType.Variation;

                relations.Add(relation);
            }

            relationship.Relation = relations.ToArray();

            message.Item = relationship;

            return message;
        }

        private AmazonEnvelopeMessage buildAmznPriceMessage(bsi_quantities listing, int messageID)
        {
            AmazonEnvelopeMessage message = new AmazonEnvelopeMessage();
            message.OperationType = AmazonEnvelopeMessageOperationType.Update;
            message.OperationTypeSpecified = true;
            message.MessageID = messageID.ToString();

            OverrideCurrencyAmount oca = new OverrideCurrencyAmount() { currency = BaseCurrencyCodeWithDefault.USD, Value = decimal.Parse(listing.price.ToString()) };
            message.Item = new Price() { SKU = listing.Item.ItemLookupCode, StandardPrice = oca };

            return message;
        }

        private string toInventoryLoader(List<bsi_posts> pendingPosts)
        {
            List<string> tabfile = new List<string>();

            string[] fields = new string[14] { "sku", "product-id", "product-id-type", 
                                                "price", "item-condition", "quantity", "add-delete", 
                                                "will-ship-internationally", "expedited-shipping", 
                                                "standard-plus", "item-note", "fulfillment-center-id", 
                                                "product-tax-code", "leadtime-to-ship" };

            tabfile.Add(String.Join("\t", fields));

            foreach (bsi_posts post in pendingPosts)
            {
                foreach (bsi_quantities listing in post.bsi_quantities)
                {
                    string[] data = new string[14] { listing.itemLookupCode, "", "", "", "", 
                                                listing.quantity.ToString(), "a", 
                                                "", "", "", "", "", "", "" };

                    tabfile.Add(String.Join("\t", data));
                }
            }




            return String.Join("\n", tabfile.ToArray());
        }

        public void submitFeed(AmazonEnvelope envelope, string feedType, List<bsi_posts> pendingPost)
        {
            SubmitFeedRequest request = new SubmitFeedRequest();

            request.FeedType = feedType;
            request.Merchant = merchantId;
            request.MarketplaceIdList = new IdList();
            request.MarketplaceIdList.Id = new List<string>(new string[] { marketplaceId });

            string feedcontent = Serialize(envelope);

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

                    switch (feedType)
                    {
                        case "_POST_PRODUCT_DATA_": pendingPost.ForEach(p => p.productFeedId = info.FeedSubmissionId);
                            break;
                        case "_POST_PRODUCT_RELATIONSHIP_DATA_": pendingPost.ForEach(p => p.relationshipFeedId = info.FeedSubmissionId);
                            break;
                        case "_POST_PRODUCT_PRICING_DATA_" : pendingPost.ForEach(p => p.priceFeedId = info.FeedSubmissionId);
                            break;
                    }

                }

                else { MessageBox.Show("FeedSubmissionInfo Not Set"); }
            }

            else { MessageBox.Show("Feed Result Not Set"); }
        }

        public void submitInventoryFeed(List<bsi_posts> pendingPosts)
        {
            SubmitFeedRequest request = new SubmitFeedRequest();

            request.FeedType = "_POST_FLAT_FILE_INVLOADER_DATA_";
            request.Merchant = merchantId;
            request.MarketplaceIdList = new IdList();
            request.MarketplaceIdList.Id = new List<string>(new string[] { marketplaceId });

            string feedcontent = this.toInventoryLoader(pendingPosts);

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

                    pendingPosts.ForEach(p => p.inventoryFeedId = info.FeedSubmissionId);
                }
            }
        }     

        private string getAmznSize(string size, string width, string gender)
        {
            //int size = int.Parse(component.Detail1);
            //string gender = component.Item.SubDescription3;
            //string width = component.Detail2;

            if (gender.Equals("UNISEX"))
            {
                string womenSize = (size + 1.5).ToString();

                string unisexSize =
                    size + " " + getAmznWidth(width, "MEN")
                    + "US Men / " + womenSize + " "
                    + getAmznWidth(width, "WOMEN") + " US Women";

                return unisexSize;
            }
            else
            {
                return size + " " + getAmznWidth(width, gender) + " US";
            }
        }

        public string getAmznWidth(string width, string gender)
        {
            width = width.ToUpper();

            if (gender.ToUpper().Equals("MENS") || gender.ToUpper().Equals("MEN"))
            {
                if (width.Equals("M") || width.Equals("D"))
                {
                    width = "D(M)";
                }
                else if (width.Equals("EE") || width.Equals("W"))
                {
                    width = "2E";
                }
                else if (width.Equals("EEE") || width.Equals("XW"))
                {
                    width = "3E";
                }
                else if (width.Equals("B") || width.Equals("N"))
                {
                    width = "B(N)";
                }
                gender = "mens";
            }
            else if (gender.ToUpper().Equals("WOMENS") || gender.ToUpper().Equals("WOMEN"))
            {
                if (width.Equals("C") || width.Equals("D") || width.Equals("W"))
                {
                    width = "C/D";
                }
                else if (width.Equals("B") || width.Equals("M"))
                {
                    width = "B(M)";
                }
                else if (width.Equals("2E") || width.Equals("EE") || width.Equals("XW"))
                {
                    width = "E";
                }
                else if (width.Equals("2A") || width.Equals("AA") || width.Equals("N"))
                {
                    width = "2A(N)";
                }
                gender = "womens";

            }

            return width;
        }

        public string getAmznGender(string gender)
        {
            gender = gender.ToUpper().Trim();

            if (gender.Equals("MENS") || gender.Equals("MEN"))
            {
                gender = "mens";
            }
            else if (gender.Equals("WOMENS") || gender.Equals("WOMEN"))
            {
                gender = "womens";
            }
            else if (gender.Equals("UNISEX") || gender.Equals("UNISEXS"))
            {
                gender = "unisex";
            }
            return gender;
        }

        private String ToTitleCase(String str)
        {
            List<String> newstr = new List<String>();

            foreach (String str2 in str.Split(new Char[1] { ' ' }))
            {
                if (str2.Length < 2) { newstr.Add(str2); continue; }

                char[] chararray = str2.ToCharArray();

                for (int i = 0; i < chararray.Length; i++)
                {
                    if (i == 0)
                    {
                        chararray[i] = chararray[i].ToString().ToUpper().ToCharArray()[0];
                    }
                    else
                    {
                        chararray[i] = chararray[i].ToString().ToLower().ToCharArray()[0];
                    }
                }
                newstr.Add(new String(chararray));
            }

            return String.Join(" ", newstr.ToArray());
        }

        public string toAmznType(String ebaycategory, String ebaystyle)
        {
            string EXCEL_COLUMN_INITIAL = "A";
            string EXCEL_COLUMN_FINAL = "J";

            string type = "";

            Excel.Application excelapp = new Microsoft.Office.Interop.Excel.Application();

            Excel.Workbook workbook = excelapp.Workbooks.Open(@"C:\Users\tc\Documents\CategoryMap.xlsx", 0, true, 5, "", "", true, Excel.XlPlatform.xlWindows, "\t", false, false, 0, true);

            Excel._Worksheet worksheet = (Excel._Worksheet)workbook.ActiveSheet;

            Excel.Range range = worksheet.Columns.get_Range("B:B");

            int currentrow = range.Find(ebaycategory).Row;

            bool stop = false;

            while (!stop)
            {
                range = worksheet.get_Range(EXCEL_COLUMN_INITIAL + currentrow.ToString(),
                                                        EXCEL_COLUMN_FINAL + currentrow.ToString());

                Array values = (Array)range.Cells.Value;
                if (Convert.ToString(values.GetValue(1, 2)).Contains(ebaycategory))
                {
                    type = Convert.ToString(values.GetValue(1, 3));
                }

                if (Convert.ToString(values.GetValue(1, 2)).Equals(ebaystyle))
                {
                    for (int i = 3; i <= 10; i++)
                    {
                        if (i == 3) { type = Convert.ToString(values.GetValue(1, i)); }
                    }
                    stop = true;
                }
                else if (String.IsNullOrEmpty(Convert.ToString(values.GetValue(1, 2))))
                {
                    stop = true;
                }

                currentrow++;

            }
            workbook.Close(System.Type.Missing, System.Type.Missing, System.Type.Missing);
            releaseObject(workbook);
            return type;
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

        
    }
}
