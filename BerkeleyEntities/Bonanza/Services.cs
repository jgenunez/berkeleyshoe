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

namespace BerkeleyEntities.Bonanza
{
    public class BonanzaServices
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public GeteBayDetailsResponseType GetEbayDetails(int marketplaceID, DetailNameCodeTypeCollection details)
        {
            GeteBayDetailsRequestType request = new GeteBayDetailsRequestType();

            request.DetailName = details;

            GeteBayDetailsCall call = new GeteBayDetailsCall();

            return call.ExecuteRequest(request) as GeteBayDetailsResponseType;
        }

        public void SynchronizeListings(int marketplaceID)
        {
            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                EbayMarketplace marketplace = dataContext.EbayMarketplaces.Single(p => p.ID == marketplaceID);

                ListingSynchronizer synchronizer = new ListingSynchronizer(marketplace, dataContext);

                DateTime syncTime = DateTime.UtcNow.AddMinutes(-3);

                if (marketplace.ListingSyncTime.HasValue && marketplace.ListingSyncTime.Value > syncTime.AddDays(-2))
                {
                    DateTime from = marketplace.ListingSyncTime.Value.AddMinutes(-5);

                    synchronizer.SyncByCreatedTime(from, syncTime);

                    try
                    {
                        synchronizer.SyncByModifiedTime(from, syncTime);
                    }
                    catch (ApiException e)
                    {
                        if (e.Errors.ToArray().Any(p => p.ErrorCode.Equals("21917062")))
                        {
                            synchronizer.SyncActiveListings();
                        }
                    }
                }
                else
                {
                    synchronizer.SyncActiveListings();
                }

                marketplace.ListingSyncTime = syncTime;

                dataContext.SaveChanges();
            }
        }

        public void SynchronizeOrders(int marketplaceID)
        {
            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                EbayMarketplace marketplace = dataContext.EbayMarketplaces.Single(p => p.ID == marketplaceID);

                OrderSynchronizer synchronizer = new OrderSynchronizer(marketplace, dataContext);

                DateTime syncTime = DateTime.UtcNow.AddMinutes(-3);

                DateTime from = marketplace.OrdersSyncTime.HasValue ? marketplace.OrdersSyncTime.Value.AddMinutes(-5) : DateTime.UtcNow.AddDays(-29);

                synchronizer.SyncOrdersByModifiedTime(from, syncTime);

                marketplace.OrdersSyncTime = syncTime;

                dataContext.SaveChanges(); 
            }
        }

        public void FixOverpublished(int marketplaceID)
        {
            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                EbayMarketplace marketplace = dataContext.EbayMarketplaces.Single(p => p.ID == marketplaceID);

                if (!marketplace.ListingSyncTime.HasValue || marketplace.ListingSyncTime.Value < DateTime.UtcNow.AddHours(-1))
                {
                    throw new InvalidOperationException(marketplace.Name + " listings must be synchronized in order to fix overpublished");
                }

                if (!marketplace.OrdersSyncTime.HasValue || marketplace.OrdersSyncTime.Value < DateTime.UtcNow.AddHours(-1))
                {
                    throw new InvalidOperationException(marketplace.Name + " orders must be synchronized in order to fix overpublished");
                }

                var listings = dataContext.EbayListings.Where(p => p.Status.Equals(EbayMarketplace.STATUS_ACTIVE) && p.MarketplaceID == marketplace.ID).ToList();

                foreach (EbayListing listing in listings.Where(p => !p.ListingItems.Any(s => s.Item == null)))
                {

                    if(listing.Format.Equals(EbayMarketplace.FORMAT_AUCTION))
                    {
                        var listingItem = listing.ListingItems.First();

                        if(listingItem.Item.AuctionCount > listingItem.Item.QtyAvailable)
                        {
                            try
                            {
                                End(marketplaceID, listing.Code);
                            }
                            catch (Exception e)
                            {
                                _logger.Error(e.Message);
                            }
                        }
                    }
                    else
                    {
                        var listingItems = listing.ListingItems.ToList();

                        if (listingItems.Any(p => p.Quantity > p.Item.QtyAvailable))
                        {
                            List<ListingItemDto> listingItemDtos = new List<ListingItemDto>();

                            foreach (var listingItem in listingItems)
                            {
                                if (listingItem.Quantity > listingItem.Item.QtyAvailable)
                                {
                                    listingItemDtos.Add(new ListingItemDto() { Sku = listingItem.Item.ItemLookupCode, Qty = listingItem.Item.QtyAvailable, QtySpecified = true });
                                }
                                else
                                {
                                    listingItemDtos.Add(new ListingItemDto() { Sku = listingItem.Item.ItemLookupCode, Qty = listingItem.Quantity, QtySpecified = true });
                                }
                            }

                            if (listingItemDtos.All(p => p.Qty == 0))
                            {
                                try
                                {
                                    End(marketplaceID, listing.Code);
                                }
                                catch (Exception e)
                                {
                                    _logger.Error(e.Message);
                                }
                            }
                            else
                            {
                                ListingDto listingDto = new ListingDto();
                                listingDto.MarketplaceID = marketplaceID;
                                listingDto.Code = listing.Code;
                                listingDto.Items.AddRange(listingItemDtos);
                                listingDto.IsVariation = (bool)listing.IsVariation;


                                try
                                {
                                    Revise(listingDto, false, false);
                                }
                                catch (Exception e)
                                {
                                    _logger.Error(e.Message);
                                }

                            }
                            
                        }
                    }
                }
            }                                  
        }

        public void Publish(ListingDto listingDto)
        {
            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                dataContext.MaterializeAttributes = true;

                EbayMarketplace marketplace = dataContext.EbayMarketplaces.Single(p => p.ID == listingDto.MarketplaceID);

                Publisher publisher = new Publisher(dataContext, marketplace);

                publisher.AddListing(listingDto);
            }
        }

        public void End(int marketplaceID, string code)
        {
            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                dataContext.MaterializeAttributes = true;

                EbayMarketplace marketplace = dataContext.EbayMarketplaces.Single(p => p.ID == marketplaceID);

                Publisher publisher = new Publisher(dataContext, marketplace);

                publisher.EndListing(code);
            }
        }

        public void Revise(ListingDto listingDto, bool includeProductData, bool includeTemplate)
        {
            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                dataContext.MaterializeAttributes = true;

                EbayMarketplace marketplace = dataContext.EbayMarketplaces.Single(p => p.ID == listingDto.MarketplaceID);

                Publisher publisher = new Publisher(dataContext, marketplace);

                publisher.ReviseListing(listingDto, includeProductData, includeTemplate);
            }
        }

    }

    public class Publisher
    {
        private berkeleyEntities _dataContext;
        private EbayMarketplace _marketplace;
        private PictureSetRepository _picSetRepository = new PictureSetRepository();
        private ProductMapperFactory _productMapperFactory = new ProductMapperFactory();

        public Publisher(berkeleyEntities dataContext, EbayMarketplace marketplace)
        {
            _dataContext = dataContext;
            _marketplace = marketplace;
        }

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

                AddFixedPriceItemResponseType response = call.ExecuteRequest(new AddFixedPriceItemRequestType() { Item = ebayDto}) as AddFixedPriceItemResponseType;

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

                if (response.EndTimeSpecified)
                {
                    listing.EndTime = response.EndTime;
                }

                dataContext.SaveChanges();
            }
        }

        private void Persist(ListingDto listingDto)
        {
            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                EbayListing listing = new EbayListing();

                listing.MarketplaceID = listingDto.MarketplaceID;
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

                if (listingDto.ScheduleTimeSpecified)
                {
                    listing.ScheduleTime = listingDto.ScheduleTime;
                }

                if (listingDto.BinPriceSpecified)
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
                    listingItem.Quantity = listingItemDto.Qty;
                    listingItem.Price = listingItemDto.Price;
                    listingItem.Title = listingItemDto.Title;
                }

                dataContext.SaveChanges();
            }
        }

        private void Update(ListingDto listingDto)
        {
            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                EbayListing listing = dataContext.EbayListings.Single(p => p.MarketplaceID == _marketplace.ID && p.Code.Equals(listingDto.Code));

                listing.Title = !string.IsNullOrWhiteSpace(listingDto.Title) ? listingDto.Title : listing.Title;

                listing.BinPrice = listingDto.BinPriceSpecified ? listingDto.BinPrice : listing.BinPrice;

                listing.FullDescription = !string.IsNullOrWhiteSpace(listingDto.FullDescription) ? listingDto.FullDescription : listing.FullDescription;

                foreach (ListingItemDto listingItemDto in listingDto.Items)
                {
                    Item item = dataContext.Items.Single(p => p.ItemLookupCode.Equals(listingItemDto.Sku));

                    EbayListingItem listingItem = listing.ListingItems.SingleOrDefault(p => p.Item.ID == item.ID);

                    if (listingItem == null)
                    {
                        listingItem = new EbayListingItem() { Item = item, Listing = listing, Sku = listingItemDto.Sku };
                    }

                    listingItem.Quantity = listingItemDto.QtySpecified ? listingItemDto.Qty : listingItem.Quantity;
                    listingItem.Title = !string.IsNullOrEmpty(listingItemDto.Title) ? listingItemDto.Title : listingItem.Title;
                    listingItem.Price = listingItemDto.PriceSpecified ? listingItemDto.Price : listingItem.Price;
                }

                listing.LastSyncTime = DateTime.UtcNow;

                dataContext.SaveChanges();
            }
        }

        private ItemType MapToEbayDto(ListingDto listingDto, bool includeProductData, bool includeTemplate)
        {
            ItemType ebayDto = new ItemType();

            if (includeTemplate)
            {
                ebayDto.CountrySpecified = true;
                ebayDto.Country = CountryCodeType.US;
                ebayDto.Location = "Clermont, Florida";
                ebayDto.Currency = CurrencyCodeType.USD;
                ebayDto.CurrencySpecified = true;
                ebayDto.HitCounterSpecified = true;
                ebayDto.HitCounter = HitCounterCodeType.BasicStyle;
                ebayDto.PaymentMethods = new BuyerPaymentMethodCodeTypeCollection() { BuyerPaymentMethodCodeType.PayPal };
                ebayDto.ShippingDetails = _marketplace.GetShippingDetails();
                ebayDto.PayPalEmailAddress = _marketplace.PayPalAccount;
                ebayDto.DispatchTimeMaxSpecified = true;
                ebayDto.DispatchTimeMax = 1;
                ebayDto.ReturnPolicy = new ReturnPolicyType()
                {
                    ReturnsAcceptedOption = "ReturnsAccepted",
                    ReturnsWithinOption = "Days_30",
                    ShippingCostPaidByOption = "Buyer"
                }; 
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

            if (!string.IsNullOrEmpty(listingDto.FullDescription))
            {
                ebayDto.Description = _marketplace.GetTemplate().Replace("<!-- INSERT FULL DESCRIPTION -->", listingDto.FullDescription);
            }

            if (listingDto.ScheduleTimeSpecified)
            {
                ebayDto.ScheduleTimeSpecified = true;
                ebayDto.ScheduleTime = listingDto.ScheduleTime;
            }

            if (listingDto.BinPriceSpecified)
            {
                ebayDto.BuyItNowPrice = new AmountType() { currencyID = CurrencyCodeType.USD, Value = Convert.ToDouble(listingDto.BinPrice) };
            }

            
            if (!listingDto.IsVariation)
            {
                ListingItemDto listingItemDto = listingDto.Items.First();

                ebayDto.QuantitySpecified = listingItemDto.QtySpecified;
                ebayDto.Quantity = listingItemDto.QtySpecified ? listingItemDto.Qty : ebayDto.Quantity;

                if (listingItemDto.PriceSpecified)
                {
                    ebayDto.StartPrice = new AmountType() { currencyID = CurrencyCodeType.USD, Value = Convert.ToDouble(listingItemDto.Price) };
                }

                if (includeProductData)
                {
                    ProductMapper mapper = _productMapperFactory.GetProductMapper(_dataContext.Items.Single(p => p.ItemLookupCode.Equals(listingItemDto.Sku)));

                    var itemSpecifics = mapper.GetItemSpecifics().Concat(mapper.GetVariationSpecifics()).ToArray();

                    ebayDto.ConditionIDSpecified = true;
                    ebayDto.ConditionID = mapper.GetConditionID();
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

                    if (ebayDto.ConditionID == 3000)
                    {
                        ebayDto.ConditionDescription = "A brand-new, unused, and unworn item. Cosmetic imperfections range from natural color variations to scuffs, cuts or nicks, hanging threads or missing buttons that occasionally occur during the manufacturing or delivery process.";
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
                    variationDto.QuantitySpecified = listingItemDto.QtySpecified;
                    variationDto.Quantity = listingItemDto.QtySpecified ? listingItemDto.Qty : variationDto.Quantity;

                    if (listingItemDto.PriceSpecified)
                    {
                        variationDto.StartPrice = new AmountType() { currencyID = CurrencyCodeType.USD, Value = Convert.ToDouble(listingItemDto.Price) };
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

        public int MarketplaceID { get; set; }

        public string Code { get; set; }

        public string Brand { get; set; }

        public string Sku {get; set;}

        public string Duration { get; set;}

        public string Format {get; set;}

        public string FullDescription {get; set;}

        public DateTime ScheduleTime {get; set;}

        public bool ScheduleTimeSpecified { get; set; }

        public decimal BinPrice { get; set; }

        public bool BinPriceSpecified { get; set; }

        public string Title {get; set;}

        public bool IsVariation {get; set;}

        public List<ListingItemDto> Items {get; set;}

        public IEnumerable<int> UrlsIds { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }
    }

    public class ListingItemDto
    {
        public string Sku {get; set;}

        public int Qty {get; set;}

        public decimal Price {get; set;}

        public string Title { get; set; }

        public bool PriceSpecified { get; set; }

        public bool QtySpecified { get; set; }
    }

    public class ListingSynchronizer
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private EbayMarketplace _marketplace;
        private berkeleyEntities _dataContext;

        public ListingSynchronizer(EbayMarketplace marketplace, berkeleyEntities dataContext)
        {
            _marketplace = marketplace;
            _dataContext = dataContext;
        }

        public ItemType SyncListing(string itemId)
        {
            GetItemRequestType request = new GetItemRequestType();
            request.ItemID = itemId;

            SetOutputSelection(request);

            GetItemCall call = new GetItemCall(_marketplace.GetApiContext());

            GetItemResponseType response = call.ExecuteRequest(request) as GetItemResponseType;

            return response.Item;
        }

        public void SyncByCreatedTime(DateTime from, DateTime to)
        {
            GetSellerListRequestType request = new GetSellerListRequestType();

            request.StartTimeFromSpecified = true;
            request.StartTimeToSpecified = true;
            request.StartTimeFrom = from;
            request.StartTimeTo = to;

            ProcessListingData(ExecuteGetSellerList(request));

            _dataContext.SaveChanges();
        }

        public void SyncActiveListings()
        {
            DateTime from = DateTime.UtcNow.AddDays(-5);
            DateTime to = DateTime.UtcNow.AddDays(32);

            GetSellerListRequestType request = new GetSellerListRequestType();

            request.EndTimeFromSpecified = true;
            request.EndTimeToSpecified = true;
            request.EndTimeFrom = from;
            request.EndTimeTo = to;

            ProcessListingData(ExecuteGetSellerList(request));


            //var activeListings = _marketplace.Listings.Where(p => p.Status.Equals(EbayMarketplace.STATUS_ACTIVE) && p.EntityState.Equals(EntityState.Unchanged));

            //var listingDtos = new ItemTypeCollection();

            //foreach (var listing in activeListings)
            //{
            //    listingDtos.Add(SyncListing(listing.Code));
            //}

            //if (listingDtos.Count > 0)
            //{
            //    ProcessListingData(listingDtos);
            //}

            _dataContext.SaveChanges();
        }

        public void SyncByModifiedTime(DateTime from, DateTime to)
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

            listing.Status = listingDto.SellingStatus != null && listingDto.SellingStatus.ListingStatusSpecified ?
                listingDto.SellingStatus.ListingStatus.ToString() : listing.Status;

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

            listingItem.Quantity =
                listingDto.QuantitySpecified &&
                listingDto.SellingStatus != null &&
                listingDto.SellingStatus.QuantitySoldSpecified ?
                    listingDto.Quantity - listingDto.SellingStatus.QuantitySold : listingItem.Quantity;
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

                listingItem.Quantity =
                    variationDto.QuantitySpecified &&
                    variationDto.SellingStatus != null &&
                    variationDto.SellingStatus.QuantitySoldSpecified ?
                        variationDto.Quantity - variationDto.SellingStatus.QuantitySold : listingItem.Quantity;
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

    }

    public class OrderSynchronizer
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private EbayMarketplace _marketplace;
        private berkeleyEntities _dataContext;

        public OrderSynchronizer(EbayMarketplace marketplace, berkeleyEntities dataContext)
        {
            _marketplace = marketplace;
            _dataContext = dataContext;
        }

        public void SyncOrdersByModifiedTime(DateTime from, DateTime to)
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

        public void SyncOrdersByCreatedTime(DateTime from, DateTime to)
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

        public void SyncOrders(StringCollection orderIds)
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
