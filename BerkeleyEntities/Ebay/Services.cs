using eBay.Service.Call;
using eBay.Service.Core.Sdk;
using eBay.Service.Core.Soap;
using BerkeleyEntities.Ebay.Mappers;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Data.Objects;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace BerkeleyEntities.Ebay
{
    public class EbayServices
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private Dictionary<int,Publisher> _publishers = new Dictionary<int,Publisher>();

        public GeteBayDetailsResponseType GetEbayDetails(int marketplaceID, DetailNameCodeTypeCollection details)
        {
            GeteBayDetailsRequestType request = new GeteBayDetailsRequestType();

            request.DetailName = details;

            GeteBayDetailsCall call = new GeteBayDetailsCall();

            return call.ExecuteRequest(request) as GeteBayDetailsResponseType;
        }

        public void SynchronizeListings(int marketplaceID)
        {
            ListingSynchronizer listingSync = new ListingSynchronizer(marketplaceID);

            listingSync.Synchronize();
        }

        public void SynchronizeOrders(int marketplaceID)
        {
            OrderSynchronizer orderSync = new OrderSynchronizer(marketplaceID);

            orderSync.Synchronize();
        }

        public void FixOverpublished(int marketplaceID)
        {
            ListingQuantityManager quantityManager = new ListingQuantityManager(marketplaceID, this);

            quantityManager.AdjustQuantities();
        }

        public void Publish(int marketplaceID, ListingDto listingDto,  string source)
        {
            if (!_publishers.ContainsKey(marketplaceID))
            {
                _publishers.Add(marketplaceID, new Publisher(marketplaceID));
            }

            Publisher publisher = _publishers[marketplaceID];

            publisher.Source = source;

            publisher.AddListing(listingDto);
        }

        public void End(int marketplaceID, string code, string source)
        {
            if (!_publishers.ContainsKey(marketplaceID))
            {
                _publishers.Add(marketplaceID, new Publisher(marketplaceID));
            }

            Publisher publisher = _publishers[marketplaceID];

            publisher.Source = source;

            publisher.EndListing(code);
        }

        public void Revise(int marketplaceID, ListingDto listingDto, bool includeProductData, bool includeTemplate, string source)
        {
            if (!_publishers.ContainsKey(marketplaceID))
            {
                _publishers.Add(marketplaceID, new Publisher(marketplaceID));
            }

            Publisher publisher = _publishers[marketplaceID];

            publisher.Source = source;

            publisher.ReviseListing(listingDto, includeProductData, includeTemplate);
        }

    }

    public class Publisher
    {
        private berkeleyEntities _dataContext = new berkeleyEntities();
        private EbayMarketplace _marketplace;
        private PictureSetRepository _picSetRepository = new PictureSetRepository();
        private ProductMapperFactory _productMapperFactory = new ProductMapperFactory();

        private Dictionary<string, ItemType> _cachedTemplates = new Dictionary<string, ItemType>();
        private Dictionary<string, string> _cachedDesigns = new Dictionary<string, string>();

        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public Publisher(int marketplaceID)
        {
            _dataContext.MaterializeAttributes = true;
            _marketplace = _dataContext.EbayMarketplaces.Single(p => p.ID == marketplaceID);
        }

        public string Source { get; set; }

        public void AddListing(ListingDto listingDto)
        {
            var pics = _picSetRepository.GetPictures(listingDto.Brand, listingDto.Items.Select(p => p.Sku).ToList());

            if (pics.Count() == 0)
            {
                throw new InvalidOperationException("picture required");
            }

            listingDto.UrlsIds = GetEPSUrls(pics);

            var ebayDto = MapToEbayDto(listingDto, true, true);

            if (listingDto.IsVariation)
            {
                AddFixedPriceItemCall call = new AddFixedPriceItemCall(_marketplace.GetApiContext());

                AddFixedPriceItemResponseType response = call.ExecuteRequest(new AddFixedPriceItemRequestType() { Item = ebayDto }) as AddFixedPriceItemResponseType;

                listingDto.Code = response.ItemID;
                listingDto.StartTime = response.StartTime;
                listingDto.EndTime = response.EndTime;

            }
            else
            {
                AddItemCall call = new AddItemCall(_marketplace.GetApiContext());

                AddItemResponseType response = call.ExecuteRequest(new AddItemRequestType() { Item = ebayDto }) as AddItemResponseType;

                listingDto.Code = response.ItemID;
                listingDto.StartTime = response.StartTime;
                listingDto.EndTime = response.EndTime;
            }

            Persist(listingDto);       
        }

        public void ReviseListing(ListingDto listingDto, bool includeProductData, bool includeTemplate)
        {
            var ebayDto = MapToEbayDto(listingDto, includeProductData, includeTemplate);

            if (listingDto.IsVariation)
            {
                ReviseFixedPriceItemCall call = new ReviseFixedPriceItemCall(_marketplace.GetApiContext());
                ReviseFixedPriceItemResponseType response = call.ExecuteRequest(new ReviseFixedPriceItemRequestType() { Item = ebayDto }) as ReviseFixedPriceItemResponseType;
            }
            else
            {
                ReviseItemCall call = new ReviseItemCall(_marketplace.GetApiContext());
                ReviseItemResponseType response = call.ExecuteRequest(new ReviseItemRequestType() { Item = ebayDto}) as ReviseItemResponseType;
            }

            Update(listingDto);
        }

        public void EndListing(string code)
        {
            EndItemRequestType request = new EndItemRequestType();
            request.ItemID = code;
            request.EndingReasonSpecified = true;
            request.EndingReason = EndReasonCodeType.NotAvailable;

            EndItemCall call = new EndItemCall(_marketplace.GetApiContext());

            EndItemResponseType response = call.ExecuteRequest(request) as EndItemResponseType;

            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                EbayListing listing = dataContext.EbayListings.Single(p => p.MarketplaceID == _marketplace.ID && p.Code.Equals(code));

                listing.Status = EbayMarketplace.STATUS_COMPLETED;

                foreach (var listingItem in listing.ListingItems)
                {
                    listingItem.Quantity = 0;
                }

                if (response.EndTimeSpecified)
                {
                    listing.EndTime = response.EndTime;
                }

                LogChanges(dataContext);

                dataContext.SaveChanges();
            }
        }

        private void LogChanges(berkeleyEntities dataContext)
        {
            var entries = dataContext.ObjectStateManager.GetObjectStateEntries(EntityState.Added | EntityState.Modified | EntityState.Deleted);

            foreach (var entry in entries.Where(p => p.Entity is EbayListingItem))
            {
                EbayListingItem listingItem = entry.Entity as EbayListingItem;

                switch(entry.State)
                {
                    case EntityState.Added :

                        Bsi_ListingChangesLog logEntryAdded = new Bsi_ListingChangesLog();
                        logEntryAdded.Date = DateTime.Now;
                        logEntryAdded.Item = listingItem.Item;
                        logEntryAdded.ListingCode = listingItem.Listing.Code;
                        logEntryAdded.Marketplace = listingItem.Listing.Marketplace.Code;
                        logEntryAdded.ListingType = listingItem.Listing.Format;
                        logEntryAdded.Source = this.Source;
                        logEntryAdded.Change = listingItem.Quantity;
                        break;

                    case EntityState.Modified :

                        int originalQty = int.Parse(entry.OriginalValues["Quantity"].ToString());
                        int currentyQty = int.Parse(entry.CurrentValues["Quantity"].ToString());

                        int change = currentyQty - originalQty;

                        if (change != 0)
                        {
                            Bsi_ListingChangesLog logEntryModified = new Bsi_ListingChangesLog();
                            logEntryModified.Date = DateTime.Now;
                            logEntryModified.Item = listingItem.Item;
                            logEntryModified.ListingCode = listingItem.Listing.Code;
                            logEntryModified.Marketplace = listingItem.Listing.Marketplace.Code;
                            logEntryModified.ListingType = listingItem.Listing.Format;
                            logEntryModified.Source = this.Source;
                            logEntryModified.Change = change;
                        }

                        break;

                    default: break;

                }
            }
        }

        private void Persist(ListingDto listingDto)
        {
            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                EbayListing listing = new EbayListing();

                listing.MarketplaceID = _marketplace.ID;
                listing.Code = listingDto.Code;
                listing.Duration = listingDto.Duration;
                listing.Format = listingDto.Format;
                listing.FullDescription = listingDto.FullDescription;
                listing.Title = listingDto.Title;
                listing.Status = EbayMarketplace.STATUS_ACTIVE;
                listing.LastSyncTime = DateTime.UtcNow;
                listing.StartTime = listingDto.StartTime;
                listing.EndTime = listingDto.EndTime;
                listing.Sku = listingDto.Sku;
                listing.IsVariation = listingDto.IsVariation;

                if (listingDto.ScheduleTime != null)
                {
                    listing.ScheduleTime = listingDto.ScheduleTime;
                }

                if (listingDto.BinPrice != null)
                {
                    listing.BinPrice = listingDto.BinPrice;
                }

                foreach (int urlID in listingDto.UrlsIds)
                {
                    EbayPictureServiceUrl url = dataContext.EbayPictureServiceUrls.Single(p => p.ID == urlID);
                    new EbayPictureUrlRelation() { PictureServiceUrl = url , Listing = listing, CreatedTime = DateTime.UtcNow };
                }

                foreach (ListingItemDto listingItemDto in listingDto.Items)
                {
                    EbayListingItem listingItem = new EbayListingItem();
                    listingItem.Listing = listing;
                    listingItem.Item = dataContext.Items.Single(p => p.ItemLookupCode.Equals(listingItemDto.Sku));
                    listingItem.Sku = listingItemDto.Sku;

                    listingItem.AvailableQuantity = listingItemDto.AvailableQty;
                    listingItem.DisplayQuantity = listingItemDto.DisplayQty;
                    listingItem.Quantity = listingItemDto.Qty.Value;
                    
                    listingItem.Price = listingItemDto.Price.Value;
                }

                LogChanges(dataContext);

                dataContext.SaveChanges();
            }
        }

        private void Update(ListingDto listingDto)
        {
            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                EbayListing listing = dataContext.EbayListings.Single(p => p.MarketplaceID == _marketplace.ID && p.Code.Equals(listingDto.Code));

                listing.Title = !string.IsNullOrWhiteSpace(listingDto.Title) ? listingDto.Title : listing.Title;

                listing.BinPrice = listingDto.BinPrice.HasValue ? listingDto.BinPrice : listing.BinPrice;

                listing.FullDescription = !string.IsNullOrWhiteSpace(listingDto.FullDescription) ? listingDto.FullDescription : listing.FullDescription;

                foreach (ListingItemDto listingItemDto in listingDto.Items)
                {
                    Item item = dataContext.Items.Single(p => p.ItemLookupCode.Equals(listingItemDto.Sku));

                    EbayListingItem listingItem = listing.ListingItems.SingleOrDefault(p => p.Item.ID == item.ID);

                    if (listingItem == null)
                    {
                        listingItem = new EbayListingItem() { Item = item, Listing = listing, Sku = listingItemDto.Sku };
                    }


                    listingItem.DisplayQuantity = listingItemDto.DisplayQty;
                    listingItem.AvailableQuantity = listingItemDto.AvailableQty;

                    if (listingItemDto.Qty.HasValue)
                    {
                        listingItem.Quantity = listingItemDto.Qty.Value;
                    }

                    if (listingItemDto.Price.HasValue)
                    {
                        listingItem.Price = listingItemDto.Price.Value;
                    }
                }

                listing.LastSyncTime = DateTime.UtcNow;

                LogChanges(dataContext);

                dataContext.SaveChanges();
            }
        }

        private ItemType MapToEbayDto(ListingDto listingDto, bool includeProductData, bool includeTemplate)
        {
            ItemType ebayDto = null;

            if (includeTemplate)
            {
                string templateName = string.IsNullOrWhiteSpace(listingDto.Template) ? "default" : listingDto.Template;

                string designName = string.IsNullOrWhiteSpace(listingDto.Design) ? "default" : listingDto.Design;

                ebayDto = GetTemplate(templateName);

                ebayDto.LookupAttributeArray = null;

                ebayDto.Description = GetDesign(designName).Replace("<!-- INSERT FULL DESCRIPTION -->", listingDto.FullDescription);
            }
            else
            {
                ebayDto = new ItemType();
            }

            ebayDto.ItemID = !string.IsNullOrEmpty(listingDto.Code) ? listingDto.Code : ebayDto.ItemID;
            ebayDto.SKU = !string.IsNullOrEmpty(listingDto.Sku) ? listingDto.Sku : ebayDto.SKU;
            ebayDto.ListingDuration = !string.IsNullOrEmpty(listingDto.Duration) ? listingDto.Duration : ebayDto.ListingDuration;
            ebayDto.Title = !string.IsNullOrEmpty(listingDto.Title) ? listingDto.Title : ebayDto.Title;

            if (!string.IsNullOrEmpty(listingDto.Format))
            {
                ebayDto.ListingTypeSpecified = true;
                ebayDto.ListingType = (ListingTypeCodeType)Enum.Parse(typeof(ListingTypeCodeType), listingDto.Format);
            }

            if (listingDto.ScheduleTime != null)
            {
                ebayDto.ScheduleTimeSpecified = true;
                ebayDto.ScheduleTime = (DateTime)listingDto.ScheduleTime;
            }

            if (listingDto.BinPrice != null)
            {
                ebayDto.BuyItNowPrice = new AmountType() { currencyID = CurrencyCodeType.USD, Value = Convert.ToDouble(listingDto.BinPrice) };
            }

            if (!listingDto.IsVariation)
            {
                ListingItemDto listingItemDto = listingDto.Items.First();

                if (listingItemDto.Qty.HasValue)
                {
                    ebayDto.QuantitySpecified = true;
                    ebayDto.Quantity = listingItemDto.Qty.Value;
                }

                if (listingItemDto.Price.HasValue)
                {
                    ebayDto.StartPrice = new AmountType() { currencyID = CurrencyCodeType.USD, Value = Convert.ToDouble(listingItemDto.Price.Value) };
                }

                if (includeProductData)
                {
                    ProductMapper mapper = _productMapperFactory.GetProductMapper(_dataContext.Items.Single(p => p.ItemLookupCode.Equals(listingItemDto.Sku)));

                    var itemSpecifics = mapper.GetItemSpecifics().Concat(mapper.GetVariationSpecifics()).ToArray();

                    ebayDto.ConditionIDSpecified = true;
                    ebayDto.ConditionID = mapper.GetConditionID();

                    if (!string.IsNullOrWhiteSpace(mapper.GetConditionDescription()))
                    {
                        ebayDto.ConditionDescription = mapper.GetConditionDescription();
                    }

                    ebayDto.PrimaryCategory = new CategoryType() { CategoryID = mapper.CategoryID };

                    ebayDto.ItemSpecifics = new NameValueListTypeCollection(itemSpecifics);

                    if (listingDto.UrlsIds.Count() > 0)
                    {
                        var urls = _dataContext.EbayPictureServiceUrls.Where(p => listingDto.UrlsIds.Contains(p.ID)).OrderBy(p => p.LocalName).Select(p => p.Url);
                        ebayDto.PictureDetails = new PictureDetailsType() { PictureURL = new StringCollection(urls.ToArray()) };
                    }
                }


                if (includeTemplate && listingDto.Format.Equals(EbayMarketplace.FORMAT_FIXEDPRICE))
                {
                    ebayDto.BestOfferEnabled = true;
                    ebayDto.BestOfferEnabledSpecified = true;
                    ebayDto.BestOfferDetails = new BestOfferDetailsType() { BestOfferEnabledSpecified = true, BestOfferEnabled = true };
                }
            }
            else
            {
                ebayDto.Variations = new VariationsType();
                ebayDto.Variations.Variation = new VariationTypeCollection();

                if (includeProductData)
                {
                    var skus = listingDto.Items.Select(p => p.Sku);

                    ProductMatrixMapper matrixMapper = _productMapperFactory.GetProductMatrixData(ebayDto.SKU, _dataContext.Items.Where(p => skus.Contains(p.ItemLookupCode)));

                    ebayDto.ConditionIDSpecified = true;

                    ebayDto.ConditionID = matrixMapper.GetConditionID();

                    if (!string.IsNullOrWhiteSpace(matrixMapper.GetConditionDescription()))
                    {
                        ebayDto.ConditionDescription = matrixMapper.GetConditionDescription();
                    }

                    ebayDto.PrimaryCategory = new CategoryType() { CategoryID = matrixMapper.CategoryID };
                    ebayDto.ItemSpecifics = new NameValueListTypeCollection(matrixMapper.GetItemSpecifics().ToArray());

                    var sets = matrixMapper.GetVariationSpecificSets();

                    ebayDto.Variations.VariationSpecificsSet = new NameValueListTypeCollection(sets.ToArray());

                    if (listingDto.UrlsIds.Count() > 0)
                    {
                        var allUrls = _dataContext.EbayPictureServiceUrls.Where(p => listingDto.UrlsIds.Contains(p.ID));

                        ebayDto.PictureDetails = new PictureDetailsType();
                        ebayDto.PictureDetails.PictureURL = new StringCollection(allUrls.Where(p => !p.LocalName.Contains("_")).OrderBy(p => p.LocalName).Select(p => p.Url).ToArray());

                        if (allUrls.Any(p => p.LocalName.Contains("_")))
                        {
                            ebayDto.Variations.Pictures = matrixMapper.GetVariationPictures(allUrls.Where(p => p.LocalName.Contains("_")));
                        }
                    }
                }



                foreach (ListingItemDto listingItemDto in listingDto.Items)
                {
                    VariationType variationDto = new VariationType();
                    variationDto.SKU = listingItemDto.Sku;

                    if (listingItemDto.Qty.HasValue)
                    {
                        variationDto.QuantitySpecified = true;
                        variationDto.Quantity = listingItemDto.Qty.Value;
                    }

                    if (listingItemDto.Price.HasValue)
                    {
                        variationDto.StartPrice = new AmountType() { currencyID = CurrencyCodeType.USD, Value = Convert.ToDouble(listingItemDto.Price.Value) };
                    }

                    if (includeProductData)
                    {
                        ProductMapper productData = _productMapperFactory.GetProductMapper(_dataContext.Items.Single(p => p.ItemLookupCode.Equals(listingItemDto.Sku)));
                        variationDto.VariationSpecifics = new NameValueListTypeCollection(productData.GetVariationSpecifics().ToArray());
                    }

                    ebayDto.Variations.Variation.Add(variationDto);
                }
            }

            return ebayDto;
        }

        private ItemType GetTemplate(string name)
        {
            ItemType template = null;

            if (_cachedTemplates.ContainsKey(name))
            {
                template = _cachedTemplates[name];
            }
            else
            {
                string path = _marketplace.RootDir + @"\" + _marketplace.Code + @"\" + name + ".xml";

                XmlSerializer serializer = new XmlSerializer(typeof(ItemType));

                template = serializer.Deserialize(new FileStream(path, FileMode.Open)) as ItemType;

                _cachedTemplates.Add(name, template);
            }

            using (Stream stream = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();

                formatter.Serialize(stream, template);

                stream.Seek(0, SeekOrigin.Begin);

                return formatter.Deserialize(stream) as ItemType;
            }
            
        }

        private string GetDesign(string name)
        {
            string design = null;

            if (_cachedDesigns.ContainsKey(name))
            {
                design = _cachedDesigns[name];
            }
            else
            {
                string path = _marketplace.RootDir + @"\" + _marketplace.Code + @"\" + name + ".html";

                StreamReader reader = new StreamReader(File.OpenRead(path));

                design = reader.ReadToEnd();

                _cachedDesigns.Add(name, design);
            }

            return design;
        }

        private IEnumerable<int> GetEPSUrls(IEnumerable<PictureInfo> pics)
        {
            List<EbayPictureServiceUrl> picUrls = new List<EbayPictureServiceUrl>();

            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                foreach (PictureInfo picInfo in pics)
                {
                    var urls = dataContext.EbayPictureServiceUrls.Where(p => p.LocalName.Equals(picInfo.Name)).ToList();

                    EbayPictureServiceUrl url = urls.FirstOrDefault(p => !p.IsExpired() && picInfo.LastModified < p.TimeUploaded);

                    if (url == null)
                    {
                        url = new EbayPictureServiceUrl();
                        url.LocalName = picInfo.Name;
                        url.Path = picInfo.Path;

                        dataContext.EbayPictureServiceUrls.AddObject(url);
                    }

                    picUrls.Add(url);
                }

                var tasks = new List<Task>();

                foreach (var urlData in picUrls.Where(p => p.Url == null))
                {
                    tasks.Add(Task.Run(() => UploadSiteHostedPictures(urlData)));
                }

                Task.WaitAll(tasks.ToArray());

                dataContext.SaveChanges();
            }

            return picUrls.Select(p => p.ID);
        }

        private void UploadSiteHostedPictures(EbayPictureServiceUrl urlData)
        {
            string boundary = "MIME_boundary";
            string CRLF = "\r\n";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.ebay.com/ws/api.dll");
            request.Headers.Add("X-EBAY-API-COMPATIBILITY-LEVEL", "515");
            request.Headers.Add("X-EBAY-API-DEV-NAME", "2ec7ed97-e88e-4868-aeb8-9be51f107a46"); //use your devid
            request.Headers.Add("X-EBAY-API-APP-NAME", "Mecalzoc-a8e6-4947-ac5b-49be352cd2f4"); //use your appid
            request.Headers.Add("X-EBAY-API-CERT-NAME", "Mecalzoc-a8e6-4947-ac5b-49be352cd2f4"); //use your certid
            request.Headers.Add("X-EBAY-API-SITEID", "0");
            request.Headers.Add("X-EBAY-API-DETAIL-LEVEL", "0");
            request.Headers.Add("X-EBAY-API-CALL-NAME", "UploadSiteHostedPictures");
            request.ContentType = "multipart/form-data; boundary=" + boundary;
            request.ProtocolVersion = HttpVersion.Version10;
            request.ContentType = "multipart/form-data; boundary=" + boundary;
            request.Method = "POST";

            string payload1 = "--" + boundary + CRLF +
                "Content-Disposition: form-data; name=document" + CRLF +
                "Content-Type: text/xml; charset=\"UTF-8\"" + CRLF + CRLF +
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<UploadSiteHostedPicturesRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\">" +
                "<PictureSet>Supersize</PictureSet>" +
                "<RequesterCredentials><eBayAuthToken>" + _marketplace.Token + "</eBayAuthToken></RequesterCredentials>" +
                "</UploadSiteHostedPicturesRequest>" +
                CRLF + "--" + boundary + CRLF +
                "Content-Disposition: form-data; name=image; filename=image" + CRLF +
                "Content-Type: application/octet-stream" + CRLF +
                "Content-Transfer-Encoding: binary" + CRLF + CRLF;

            string payload3 = CRLF + "--" + boundary + "--" + CRLF;

            byte[] postDataBytes1 = Encoding.ASCII.GetBytes(payload1);
            byte[] postDataBytes2 = Encoding.ASCII.GetBytes(payload3);
            byte[] image = null;

            using (Stream fileStream = new FileStream(urlData.Path, FileMode.Open, FileAccess.Read))
            {
                fileStream.Seek(0, SeekOrigin.Begin);

                using (BinaryReader reader = new BinaryReader(fileStream))
                {
                    image = reader.ReadBytes((int)fileStream.Length);
                }
            }

            int length = postDataBytes1.Length + image.Length + postDataBytes2.Length;

            request.ContentLength = length;

            using (Stream stream = request.GetRequestStream())
            {
                byte[] bytes = postDataBytes1.Concat(image).Concat(postDataBytes2).ToArray();

                stream.Write(bytes, 0, length);
            }

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            string output = null;

            using (StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
            {
                output = sr.ReadToEnd();
            }

            XmlDocument xmlResponse = new XmlDocument();

            xmlResponse.LoadXml(output);

            XmlNodeList list = xmlResponse.GetElementsByTagName("FullURL", "urn:ebay:apis:eBLBaseComponents");

            if (list[0] == null)
            {
                list = xmlResponse.GetElementsByTagName("Errors");
                ErrorType error = new ErrorType();
                foreach (XmlNode node in list[0])
                {
                    switch (node.Name)
                    {
                        case "ShortMessage": error.ShortMessage = node.InnerText; break;
                        case "LongMessage": error.LongMessage = node.InnerText; break;
                        case "ErrorCode": error.ErrorCode = node.InnerText; break;
                    }
                }

                throw new ApiException(new ErrorTypeCollection(new ErrorType[1] { error }));
            }

            urlData.Url = list[0].InnerText;
            urlData.TimeUploaded = DateTime.UtcNow;
        }
    }

    public class ListingDto
    {
        public ListingDto()
        {
            this.Items = new List<ListingItemDto>();
            this.UrlsIds = new List<int>();
        }

        public string Code { get; set; }

        public string Brand { get; set; }

        public string Sku {get; set;}

        public string Duration { get; set;}

        public string Format {get; set;}

        public string FullDescription {get; set;}

        public DateTime? ScheduleTime {get; set;}

        public decimal? BinPrice { get; set; }

        public string Title {get; set;}

        public bool IsVariation {get; set;}

        public List<ListingItemDto> Items {get; set;}

        public IEnumerable<int> UrlsIds { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public string Template { get; set; }

        public string Design { get; set; }
    }

    public class ListingItemDto
    {
        public string Sku {get; set;}

        public int? Qty {get; set;}

        public decimal? Price {get; set;}

        public int? DisplayQty { get; set; }

        public int? AvailableQty { get; set; }
    }

    public class ListingQuantityManager
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private berkeleyEntities _dataContext = new berkeleyEntities();
        private EbayMarketplace _marketplace;
        private EbayServices _services;

        public ListingQuantityManager(int marketplaceID, EbayServices services)
        {
            _services = services;
            _marketplace = _dataContext.EbayMarketplaces.Single(p => p.ID == marketplaceID);
        }

        public void AdjustQuantities()
        {
            if (!_marketplace.ListingSyncTime.HasValue || _marketplace.ListingSyncTime.Value < DateTime.UtcNow.AddHours(-1))
            {
                throw new InvalidOperationException(_marketplace.Name + " listings must be synchronized in order to fix overpublished");
            }

            if (!_marketplace.OrdersSyncTime.HasValue || _marketplace.OrdersSyncTime.Value < DateTime.UtcNow.AddHours(-1))
            {
                throw new InvalidOperationException(_marketplace.Name + " orders must be synchronized in order to fix overpublished");
            }

            var listings = _dataContext.EbayListings.Where(p => p.Status.Equals(EbayMarketplace.STATUS_ACTIVE) && p.MarketplaceID == _marketplace.ID).ToList();

            foreach (EbayListing listing in listings.Where(p => !p.ListingItems.Any(s => s.Item == null)))
            {
                try 
	            {	        
		            if(listing.Format.Equals(EbayMarketplace.FORMAT_AUCTION))
                    {
                        AdjustAuction(listing);
                    }
                    else
                    {
                        AdjustFixedPrice(listing);
                    }
	            }
                catch (ApiException e)
                {
                    if (e.Errors.ToArray().Any(p => p.ErrorCode.Equals("17")))
                    {
                        HandleErrorCode17(listing.Code);
                    }

                    _logger.Error(string.Format("Error updating {0} - {1} : {2}", _marketplace.Code, listing.Code, e.Message));
                }
	            catch (Exception e)
	            {
		            _logger.Error(string.Format("Error updating {0} - {1} : {2}", _marketplace.Code, listing.Code, e.Message));
	            }
            }
        }

        private void AdjustAuction(EbayListing listing)
        {
            var listingItem = listing.ListingItems.First();

            if(listing.BidCount == 0 && listingItem.Item.QtyAvailable == 0)
            {             
                _services.End(_marketplace.ID, listing.Code, "Overpublished Service");
            }
        }

        private void AdjustFixedPrice(EbayListing listing)
        {
            var listingItems = listing.ListingItems.Where(p => p.Quantity != 0).ToList();

            foreach (var listingItem in listingItems)
            {
                if (listingItem.DisplayQuantity.HasValue && listingItem.AvailableQuantity.HasValue)
                {
                    if (listingItem.DisplayQuantity.Value <= listingItem.AvailableQuantity.Value && 
                        listingItem.DisplayQuantity.Value <= listingItem.Item.QtyAvailable && 
                        listingItem.DisplayQuantity.Value != listingItem.Quantity)
                    {
                        listingItem.Quantity = listingItem.DisplayQuantity.Value;
                    }
                }

                if (listingItem.Quantity > listingItem.Item.QtyAvailable)
                {
                    listingItem.Quantity = listingItem.Item.QtyAvailable;
                }
            }

            if (listingItems.Any(p => p.EntityState.Equals(EntityState.Modified)))
            {
                if (listingItems.All(p => p.Quantity == 0))
                {

                    _services.End(_marketplace.ID, listing.Code, "Overpublished Service");
                   
                }
                else
                {
                    ListingDto listingDto = new ListingDto();
                    listingDto.Code = listing.Code;
                    listingDto.IsVariation = (bool)listing.IsVariation;

                    foreach (var listingItem in listingItems)
                    {
                        listingDto.Items.Add( new ListingItemDto() { 
                                Sku = listingItem.Item.ItemLookupCode, 
                                Qty = listingItem.Quantity, 
                                DisplayQty = listingItem.DisplayQuantity, 
                                AvailableQty = listingItem.AvailableQuantity });
                    }

                    _services.Revise(_marketplace.ID, listingDto, false, false, "Overpublished Service");
                }
            } 
        }

        private void HandleErrorCode17(string code)
        {
            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                EbayListing listing = dataContext.EbayListings.Single(p => p.Marketplace.ID == _marketplace.ID && p.Code.Equals(code));

                listing.Status = EbayMarketplace.STATUS_DELETED;

                dataContext.SaveChanges();
            }
        }
    }

    public class ListingSynchronizer
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private berkeleyEntities _dataContext = new berkeleyEntities();
        private EbayMarketplace _marketplace;
        

        public ListingSynchronizer(int marketplaceID)
        {
            _marketplace = _dataContext.EbayMarketplaces.Single(p => p.ID == marketplaceID);
        }

        public void Synchronize()
        {
            DateTime syncTime = DateTime.UtcNow.AddMinutes(-3);

            if (_marketplace.ListingSyncTime.HasValue && _marketplace.ListingSyncTime.Value > syncTime.AddDays(-2))
            {
                DateTime from = _marketplace.ListingSyncTime.Value.AddMinutes(-5);

                SyncByCreatedTime(from, syncTime);

                try
                {
                    SyncByModifiedTime(from, syncTime);
                }
                catch (ApiException e)
                {
                    if (e.Errors.ToArray().Any(p => p.ErrorCode.Equals("21917062")))
                    {
                        SyncActiveListings();
                    }
                }
            }
            else
            {
                SyncActiveListings();
            }

            _marketplace.ListingSyncTime = syncTime;

            _dataContext.SaveChanges();
        }

        private ItemType SyncListing(string itemId)
        {
            GetItemRequestType request = new GetItemRequestType();
            request.ItemID = itemId;

            SetOutputSelection(request);

            GetItemCall call = new GetItemCall(_marketplace.GetApiContext());

            GetItemResponseType response = call.ExecuteRequest(request) as GetItemResponseType;

            return response.Item;
        }

        private void SyncByCreatedTime(DateTime from, DateTime to)
        {
            GetSellerListRequestType request = new GetSellerListRequestType();

            request.StartTimeFromSpecified = true;
            request.StartTimeToSpecified = true;
            request.StartTimeFrom = from;
            request.StartTimeTo = to;

            ProcessListingData(ExecuteGetSellerList(request));

            LogChanges();

            _dataContext.SaveChanges();
        }

        private void SyncActiveListings()
        {
            DateTime from = DateTime.UtcNow.AddDays(-5);
            DateTime to = DateTime.UtcNow.AddDays(32);

            GetSellerListRequestType request = new GetSellerListRequestType();

            request.EndTimeFromSpecified = true;
            request.EndTimeToSpecified = true;
            request.EndTimeFrom = from;
            request.EndTimeTo = to;

            ProcessListingData(ExecuteGetSellerList(request));

            LogChanges();

            _dataContext.SaveChanges();
        }

        private void SyncByModifiedTime(DateTime from, DateTime to)
        {
            GetSellerEventsRequestType request = new GetSellerEventsRequestType();
            request.DetailLevel = new DetailLevelCodeTypeCollection();
            request.DetailLevel.Add(DetailLevelCodeType.ReturnAll);

            request.HideVariationsSpecified = true;
            request.HideVariations = false;

            request.IncludeVariationSpecificsSpecified = true;
            request.IncludeVariationSpecifics = false;

            request.ModTimeFromSpecified = true;
            request.ModTimeToSpecified = true;
            request.ModTimeFrom = from;
            request.ModTimeTo = to;


            GetSellerEventsCall call = new GetSellerEventsCall(_marketplace.GetApiContext());

            call.ApiCallBase.Timeout = 120000;

            GetSellerEventsResponseType response = call.ExecuteRequest(request) as GetSellerEventsResponseType;

            ProcessListingData(response.ItemArray);

            LogChanges();

            _dataContext.SaveChanges();

        }

        private ItemTypeCollection ExecuteGetSellerList(GetSellerListRequestType request)
        {
            SetOutputSelection(request);

            request.Pagination = new PaginationType();
            request.Pagination.EntriesPerPage = 100;
            request.Pagination.PageNumber = 1;
            request.IncludeVariations = true;
            request.IncludeVariationsSpecified = true;

            GetSellerListCall call = new GetSellerListCall(_marketplace.GetApiContext());

            GetSellerListResponseType response = call.ExecuteRequest(request) as GetSellerListResponseType;

            while (request.Pagination.PageNumber < response.PaginationResult.TotalNumberOfPages)
            {
                request.Pagination.PageNumber++;
                GetSellerListResponseType additionalResponse = call.ExecuteRequest(request) as GetSellerListResponseType;
                response.ItemArray.AddRange(additionalResponse.ItemArray);
            }

            return response.ItemArray;
        }

        private void SetOutputSelection(AbstractRequestType request)
        {
            request.OutputSelector = new StringCollection();
            request.DetailLevel = new DetailLevelCodeTypeCollection();
            request.DetailLevel.Add(DetailLevelCodeType.ReturnAll);


            request.OutputSelector.AddRange(new string[19] 
            { 
                "ItemArray.Item.ItemID", "ItemArray.Item.SellingStatus.QuantitySold", 
                "ItemArray.Item.ListingDetails.EndTime", "ItemArray.Item.ListingDetails.StartTime", 
                "ItemArray.Item.Title", "ItemArray.Item.Quantity" , "ItemArray.Item.ListingType",
                "ItemArray.Item.SKU", "ItemArray.Item.ConditionDisplayName", "ItemArray.Item.ListingDuration",
                "ItemArray.Item.BestOfferEnabled", "ItemArray.Item.Variations", "ItemArray.Item.Variations.Variation",
                "ItemArray.Item.Variations.Variation", "ItemArray.Item.StartPrice", "Pagination.PageNumber", "PaginationResult.TotalNumberOfPages", 
                "ItemArray.Item.ConditionID", "ItemArray.Item.SellingStatus.ListingStatus"
            });
        }

        private void ProcessListingData(ItemTypeCollection listingsDtos)
        {
            foreach (ItemType listingDto in listingsDtos)
            {
                EbayListing listing = _marketplace.Listings.SingleOrDefault(p => p.Code.Equals(listingDto.ItemID));

                if (listing == null)
                {
                    listing = new EbayListing();
                }
                
                try
                {
                    Map(listing, listingDto);
                }
                catch (PropertyConstraintException e)
                {
                    _logger.Error(string.Format("Error updating {0} - {1} : {2} | Listing will syncronize specifying the desire fields", _marketplace.Code, listing.Code, e.Message));

                    Map(listing, SyncListing(listingDto.ItemID));
                }
            }
        }

        private void Map(EbayListing listing, ItemType listingDto)
        {
            listing.Code = listingDto.ItemID;

            listing.Format = listingDto.ListingTypeSpecified ? listingDto.ListingType.ToString() : listing.Format;

            listing.Marketplace = _marketplace;

            listing.Duration = listingDto.ListingDuration != null ? listingDto.ListingDuration : listing.Duration;

            listing.EndTime = listingDto.ListingDetails != null && listingDto.ListingDetails.EndTimeSpecified ?
                listingDto.ListingDetails.EndTime : listing.EndTime;

            listing.StartTime = listingDto.ListingDetails != null && listingDto.ListingDetails.StartTimeSpecified ?
                listingDto.ListingDetails.StartTime : listing.StartTime;

            listing.Title = listingDto.Title != null ? listingDto.Title : listing.Title;

            listing.Sku = listingDto.SKU != null ? listingDto.SKU : listing.Sku != null ? listing.Sku : string.Empty;

            if (listingDto.SellingStatus != null)
            {
                listing.Status = listingDto.SellingStatus.ListingStatusSpecified ? listingDto.SellingStatus.ListingStatus.ToString() : listing.Status;
                listing.BidCount = listingDto.SellingStatus.BidCountSpecified ? listingDto.SellingStatus.BidCount : listing.BidCount;
            }

            listing.LastSyncTime = DateTime.UtcNow;

            if (listingDto.Variations == null)
            {
                listing.IsVariation = false;
                MapListingItem(listing, listingDto);
            }
            else
            {
                listing.IsVariation = true;
                MapListingItem(listing, listingDto.Variations);
            }
        }

        private void MapListingItem(EbayListing listing, ItemType listingDto)
        {
            string sku = listingDto.SKU != null ? listingDto.SKU.ToUpper().Trim() : listing.Sku;

            EbayListingItem listingItem = GetListingItem(listing, sku);

            listingItem.Price = listingDto.StartPrice != null ? decimal.Parse(listingDto.StartPrice.Value.ToString()) : listingItem.Price;

            if (listing.Status.Equals(EbayMarketplace.STATUS_COMPLETED))
            {
                listingItem.Quantity = 0;
            }
            else
            {
                int currentQty = listingDto.Quantity - listingDto.SellingStatus.QuantitySold;

                int qtyChange = currentQty - listingItem.Quantity;

                listingItem.Quantity =
                listingDto.QuantitySpecified &&
                listingDto.SellingStatus != null &&
                listingDto.SellingStatus.QuantitySoldSpecified ? currentQty : listingItem.Quantity;

                if (listingItem.AvailableQuantity.HasValue)
                {
                    listingItem.AvailableQuantity = listingItem.AvailableQuantity.Value + qtyChange;
                }
            }          
        }

        private void MapListingItem(EbayListing listing, VariationsType variationDtos)
        {
            var currentSkus = variationDtos.Variation.ToArray().Select(p => p.SKU);

            foreach (var listingItem in listing.ListingItems)
            {
                if (!currentSkus.Any(p => p.Equals(listingItem.Sku)))
                {
                    listingItem.Quantity = 0;
                }
            }

            foreach (VariationType variationDto in variationDtos.Variation)
            {
                string sku = variationDto.SKU.ToUpper().Trim();

                EbayListingItem listingItem = GetListingItem(listing, sku);

                listingItem.Price = variationDto.StartPrice != null ? Convert.ToDecimal(variationDto.StartPrice.Value) : listingItem.Price;

                if (listing.Status.Equals(EbayMarketplace.STATUS_COMPLETED))
                {
                    listingItem.Quantity = 0;
                }
                else
                {
                    int currentQty = variationDto.Quantity - variationDto.SellingStatus.QuantitySold;

                    int qtyChange = currentQty - listingItem.Quantity;

                    listingItem.Quantity =
                    variationDto.QuantitySpecified &&
                    variationDto.SellingStatus != null &&
                    variationDto.SellingStatus.QuantitySoldSpecified? currentQty : listingItem.Quantity;

                    if (listingItem.AvailableQuantity.HasValue)
                    {
                        listingItem.AvailableQuantity = listingItem.AvailableQuantity.Value + qtyChange;
                    }
                }   
            }
        }

        private EbayListingItem GetListingItem(EbayListing listing, string sku)
        {
            EbayListingItem listingItem = listing.ListingItems.SingleOrDefault(p => p.Sku.Equals(sku));

            if (listingItem == null)
            {
                Item item = _dataContext.Items.SingleOrDefault(p => p.ItemLookupCode.Equals(sku));

                listingItem = new EbayListingItem() { Item = item, Listing = listing, Sku = sku };
            }

            return listingItem;
        }

        private void LogChanges()
        {
            var entries = _dataContext.ObjectStateManager.GetObjectStateEntries(EntityState.Added | EntityState.Modified | EntityState.Deleted);

            foreach (var entry in entries.Where(p => p.Entity is EbayListingItem))
            {
                EbayListingItem listingItem = entry.Entity as EbayListingItem;

                switch (entry.State)
                {
                    case EntityState.Added:

                        Bsi_ListingChangesLog logEntryAdded = new Bsi_ListingChangesLog();
                        logEntryAdded.Date = DateTime.Now;
                        logEntryAdded.Item = listingItem.Item;
                        logEntryAdded.ListingCode = listingItem.Listing.Code;
                        logEntryAdded.Marketplace = listingItem.Listing.Marketplace.Code;
                        logEntryAdded.ListingType = listingItem.Listing.Format;
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
                            logEntryModified.ListingCode = listingItem.Listing.Code;
                            logEntryModified.Marketplace = listingItem.Listing.Marketplace.Code;
                            logEntryModified.ListingType = listingItem.Listing.Format;
                            logEntryModified.Source = "Synchronization Service";
                            logEntryModified.Change = change;
                        }

                        break;

                    default: break;

                }
            }
        }

    }

    public class OrderSynchronizer
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private berkeleyEntities _dataContext = new berkeleyEntities();
        private EbayMarketplace _marketplace;
        

        public OrderSynchronizer(int marketplaceID)
        {
            _marketplace = _dataContext.EbayMarketplaces.Single(p => p.ID == marketplaceID);
        }

        public void Synchronize()
        {
            DateTime syncTime = DateTime.UtcNow.AddMinutes(-3);

            DateTime from = _marketplace.OrdersSyncTime.HasValue ? _marketplace.OrdersSyncTime.Value.AddMinutes(-5) : DateTime.UtcNow.AddDays(-29);

            SyncOrdersByModifiedTime(from, syncTime);

            _marketplace.OrdersSyncTime = syncTime;

            _dataContext.SaveChanges(); 
        }

        private void SyncOrdersByModifiedTime(DateTime from, DateTime to)
        {
            GetOrdersRequestType request = new GetOrdersRequestType();
            request.OrderStatus = OrderStatusCodeType.All;
            request.Pagination = new PaginationType();
            request.Pagination.EntriesPerPage = 100;
            request.Pagination.PageNumber = 1;
            request.DetailLevel = new DetailLevelCodeTypeCollection();
            request.DetailLevel.Add(DetailLevelCodeType.ReturnAll);

            request.ModTimeFromSpecified = true;
            request.ModTimeToSpecified = true;
            request.ModTimeFrom = from;
            request.ModTimeTo = to;

            ProcessOrderData(ExecuteGetOrders(request));
        }

        private void SyncOrdersByCreatedTime(DateTime from, DateTime to)
        {
            GetOrdersRequestType request = new GetOrdersRequestType();
            request.OrderStatus = OrderStatusCodeType.All;
            request.Pagination = new PaginationType();
            request.Pagination.EntriesPerPage = 100;
            request.Pagination.PageNumber = 1;
            request.DetailLevel = new DetailLevelCodeTypeCollection();
            request.DetailLevel.Add(DetailLevelCodeType.ReturnAll);


            request.CreateTimeFromSpecified = true;
            request.CreateTimeToSpecified = true;
            request.CreateTimeFrom = from;
            request.CreateTimeTo = to;


            ProcessOrderData(ExecuteGetOrders(request));

        }

        private void SyncOrders(StringCollection orderIds)
        {
            GetOrdersRequestType request = new GetOrdersRequestType();
            request.OrderStatus = OrderStatusCodeType.All;
            request.Pagination = new PaginationType();
            request.Pagination.EntriesPerPage = 100;
            request.Pagination.PageNumber = 1;
            request.DetailLevel = new DetailLevelCodeTypeCollection();
            request.DetailLevel.Add(DetailLevelCodeType.ReturnAll);
            request.OrderIDArray = orderIds;


            ProcessOrderData(ExecuteGetOrders(request));


        }

        private OrderTypeCollection ExecuteGetOrders(GetOrdersRequestType request)
        {
            GetOrdersCall call = new GetOrdersCall(_marketplace.GetApiContext());

            GetOrdersResponseType response = call.ExecuteRequest(request) as GetOrdersResponseType;

            while (request.Pagination.PageNumber < response.PaginationResult.TotalNumberOfPages)
            {
                request.Pagination.PageNumber++;

                GetOrdersResponseType additionalResponse = call.ExecuteRequest(request) as GetOrdersResponseType;

                response.OrderArray.AddRange(additionalResponse.OrderArray);
            }

            return response.OrderArray;
        }

        private void ProcessOrderData(OrderTypeCollection orders)
        {
            foreach (OrderType orderDto in orders)
            {
                try
                {
                    PersistOrder(orderDto);
                }
                catch (Exception e)
                {
                    _logger.Error(string.Format("Order ( {1} | {0} ) synchronization failed: {2}", orderDto.ShippingDetails.SellingManagerSalesRecordNumber, _marketplace.Code, e.Message));
                }
            }

            _dataContext.SaveChanges();

            var combinedOrders = orders.ToArray().Where(p => p.TransactionArray.Count > 1);

            foreach (OrderType orderDto in combinedOrders)
            {
                RemoveParentOrder(orderDto);
            }

            _dataContext.SaveChanges();
        }

        private void PersistOrder(OrderType orderDto)
        {
            EbayOrder order = _dataContext.EbayOrders.SingleOrDefault(p => p.Code.Equals(orderDto.OrderID) && p.MarketplaceID == _marketplace.ID);

            if (order == null)
            {
                order = new EbayOrder();
                order.Code = orderDto.OrderID;
                order.MarketplaceID = _marketplace.ID;
                order.CreatedTime = orderDto.CreatedTime;
            }

            order.BuyerID = orderDto.BuyerUserID;
            order.OrderStatus = orderDto.OrderStatus.ToString();
            order.SalesRecordNumber = orderDto.ShippingDetails.SellingManagerSalesRecordNumber.ToString();
            order.EbayPaymentStatus = orderDto.CheckoutStatus.eBayPaymentStatus.ToString();
            order.CheckoutStatus = orderDto.CheckoutStatus.Status.ToString();
            order.PaymentMethod = orderDto.CheckoutStatus.PaymentMethod.ToString();
            order.PaidTime = orderDto.PaidTimeSpecified ? (DateTime?)orderDto.PaidTime : null;
            order.ShippedTime = orderDto.ShippedTimeSpecified ? (DateTime?)orderDto.ShippedTime : null;
            order.PaidAmount = Convert.ToDecimal(orderDto.AmountPaid.Value);
            order.CompanyName = orderDto.ShippingAddress.CompanyName;
            order.Street1 = orderDto.ShippingAddress.Street1;
            order.Street2 = orderDto.ShippingAddress.Street2;
            
            order.CityName = orderDto.ShippingAddress.CityName;
            order.StateOrProvince = orderDto.ShippingAddress.StateOrProvince;
            order.PostalCode = orderDto.ShippingAddress.PostalCode;
            order.CountryCode = orderDto.ShippingAddress.Country.ToString();
            order.CountryName = orderDto.ShippingAddress.CountryName;
            order.UserName = orderDto.ShippingAddress.Name;
            order.Phone = orderDto.ShippingAddress.Phone;
            order.Subtotal = Convert.ToDecimal(orderDto.Subtotal.Value);
            order.AdjustmentAmount = Convert.ToDecimal(orderDto.AdjustmentAmount.Value);
            order.Total = Convert.ToDecimal(orderDto.Total.Value);

            order.ShippingService = orderDto.ShippingServiceSelected != null ? orderDto.ShippingServiceSelected.ShippingService : "N/A";

            order.ExpeditedService = orderDto.ShippingServiceSelected != null && orderDto.ShippingServiceSelected.ExpeditedServiceSpecified ?
                orderDto.ShippingServiceSelected.ExpeditedService : order.ExpeditedService;

            order.ShippingInsuranceCost = orderDto.ShippingServiceSelected.ShippingInsuranceCost != null ?
                Convert.ToDecimal(orderDto.ShippingServiceSelected.ShippingInsuranceCost.Value) : 0;

            order.ShippingServiceCost = orderDto.ShippingServiceSelected.ShippingServiceCost != null ?
                Convert.ToDecimal(orderDto.ShippingServiceSelected.ShippingServiceCost.Value) : 0;

            order.ShippingServiceAdditionalCost = orderDto.ShippingServiceSelected.ShippingServiceAdditionalCost != null ?
                Convert.ToDecimal(orderDto.ShippingServiceSelected.ShippingServiceAdditionalCost.Value) : 0;

            order.ShippingSurcharge = orderDto.ShippingServiceSelected.ShippingSurcharge != null ?
                Convert.ToDecimal(orderDto.ShippingServiceSelected.ShippingSurcharge.Value) : 0;

            order.LastSyncTime = DateTime.UtcNow;

            foreach (TransactionType orderItemDto in orderDto.TransactionArray)
            {
                EbayOrderItem orderItem = order.OrderItems.SingleOrDefault(p => p.Code.Equals(orderItemDto.OrderLineItemID));

                if (orderItem == null)
                {
                    orderItem = CreateOrderItem(order, orderItemDto);
                }


                if (orderItemDto.Item != null)
                {
                    if (!string.IsNullOrEmpty(orderItemDto.Item.ConditionDisplayName))
                    {
                        orderItem.ConditionDisplayName = orderItemDto.Item.ConditionDisplayName;
                    }
                    if (!string.IsNullOrEmpty(orderItemDto.Item.ConditionDescription))
                    {
                        orderItem.ConditionDescription = orderItemDto.Item.ConditionDescription;
                    }
                }
                
                orderItem.TransactionPrice = orderItemDto.TransactionPrice != null ? decimal.Parse(orderItemDto.TransactionPrice.Value.ToString()) : orderItem.TransactionPrice;
                orderItem.QuantityPurchased = orderItemDto.QuantityPurchased;
                orderItem.UnpaidItemDisputeStatus = orderItemDto.UnpaidItem != null ? orderItemDto.UnpaidItem.Status.ToString() : "N/A";
                orderItem.UnpaidItemDisputeType = orderItemDto.UnpaidItem != null ? orderItemDto.UnpaidItem.Type.ToString() : "N/A";
            }

        }

        private EbayOrderItem CreateOrderItem(EbayOrder order, TransactionType orderItemDto)
        {
            EbayOrderItem orderItem = new EbayOrderItem();

            string sku = orderItemDto.Variation == null ? orderItemDto.Item.SKU : orderItemDto.Variation.SKU;

            string listingID = orderItemDto.Item.ItemID;

            EbayListing listing = _marketplace.Listings.Single(p => p.Code.Equals(listingID));

            orderItem.ListingItem = listing.ListingItems.Single(p => p.Sku.Equals(sku));
            orderItem.Order = order;
            orderItem.CreatedDate = orderItemDto.CreatedDate;
            orderItem.Code = orderItemDto.OrderLineItemID;


            return orderItem;
        }

        private void RemoveParentOrder(OrderType orderDto)
        {
            if (orderDto.TransactionArray.Count > 1)
            {
                var orderItems = orderDto.TransactionArray.ToArray().Select(p => p.OrderLineItemID).ToList();

                foreach (string orderItemID in orderItems)
                {
                    EbayOrder order = _dataContext.EbayOrders.SingleOrDefault(p => p.MarketplaceID == _marketplace.ID && p.Code.Equals(orderItemID));

                    if (order != null)
                    {
                        _dataContext.EbayOrders.DeleteObject(order);
                    }
                }
            }
        }
    }

}
