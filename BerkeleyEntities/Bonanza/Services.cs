using eBay.Service.Call;
using eBay.Service.Core.Sdk;
using eBay.Service.Core.Soap;
using BerkeleyEntities.Bonanza.Mapper;
using Newtonsoft.Json;
using NLog;
using Amazon.S3;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Dynamic;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using Amazon.S3.Model;

namespace BerkeleyEntities.Bonanza
{
    public class BonanzaServices
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        private Dictionary<int, Publisher> _publishers = new Dictionary<int, Publisher>();

        //public void SynchronizeListings(int marketplaceID)
        //{
        //    using (berkeleyEntities dataContext = new berkeleyEntities())
        //    {
        //        EbayMarketplace marketplace = dataContext.EbayMarketplaces.Single(p => p.ID == marketplaceID);

        //        ListingSynchronizer synchronizer = new ListingSynchronizer(marketplace, dataContext);

        //        DateTime syncTime = DateTime.UtcNow.AddMinutes(-3);

        //        if (marketplace.ListingSyncTime.HasValue && marketplace.ListingSyncTime.Value > syncTime.AddDays(-2))
        //        {
        //            DateTime from = marketplace.ListingSyncTime.Value.AddMinutes(-5);

        //            synchronizer.SyncByCreatedTime(from, syncTime);

        //            try
        //            {
        //                synchronizer.SyncByModifiedTime(from, syncTime);
        //            }
        //            catch (ApiException e)
        //            {
        //                if (e.Errors.ToArray().Any(p => p.ErrorCode.Equals("21917062")))
        //                {
        //                    synchronizer.SyncActiveListings();
        //                }
        //            }
        //        }
        //        else
        //        {
        //            synchronizer.SyncActiveListings();
        //        }

        //        marketplace.ListingSyncTime = syncTime;

        //        dataContext.SaveChanges();
        //    }
        //}

        //public void SynchronizeOrders(int marketplaceID)
        //{
        //    using (berkeleyEntities dataContext = new berkeleyEntities())
        //    {
        //        EbayMarketplace marketplace = dataContext.EbayMarketplaces.Single(p => p.ID == marketplaceID);

        //        OrderSynchronizer synchronizer = new OrderSynchronizer(marketplace, dataContext);

        //        DateTime syncTime = DateTime.UtcNow.AddMinutes(-3);

        //        DateTime from = marketplace.OrdersSyncTime.HasValue ? marketplace.OrdersSyncTime.Value.AddMinutes(-5) : DateTime.UtcNow.AddDays(-29);

        //        synchronizer.SyncOrdersByModifiedTime(from, syncTime);

        //        marketplace.OrdersSyncTime = syncTime;

        //        dataContext.SaveChanges(); 
        //    }
        //}

        //public void FixOverpublished(int marketplaceID)
        //{
        //    using (berkeleyEntities dataContext = new berkeleyEntities())
        //    {
        //        EbayMarketplace marketplace = dataContext.EbayMarketplaces.Single(p => p.ID == marketplaceID);

        //        if (!marketplace.ListingSyncTime.HasValue || marketplace.ListingSyncTime.Value < DateTime.UtcNow.AddHours(-1))
        //        {
        //            throw new InvalidOperationException(marketplace.Name + " listings must be synchronized in order to fix overpublished");
        //        }

        //        if (!marketplace.OrdersSyncTime.HasValue || marketplace.OrdersSyncTime.Value < DateTime.UtcNow.AddHours(-1))
        //        {
        //            throw new InvalidOperationException(marketplace.Name + " orders must be synchronized in order to fix overpublished");
        //        }

        //        var listings = dataContext.EbayListings.Where(p => p.Status.Equals(EbayMarketplace.STATUS_ACTIVE) && p.MarketplaceID == marketplace.ID).ToList();

        //        foreach (EbayListing listing in listings.Where(p => !p.ListingItems.Any(s => s.Item == null)))
        //        {

        //            if(listing.Format.Equals(EbayMarketplace.FORMAT_AUCTION))
        //            {
        //                var listingItem = listing.ListingItems.First();

        //                if(listingItem.Item.AuctionCount > listingItem.Item.QtyAvailable)
        //                {
        //                    try
        //                    {
        //                        End(marketplaceID, listing.Code);
        //                    }
        //                    catch (Exception e)
        //                    {
        //                        _logger.Error(e.Message);
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                var listingItems = listing.ListingItems.ToList();

        //                if (listingItems.Any(p => p.Quantity > p.Item.QtyAvailable))
        //                {
        //                    List<ListingItemDto> listingItemDtos = new List<ListingItemDto>();

        //                    foreach (var listingItem in listingItems)
        //                    {
        //                        if (listingItem.Quantity > listingItem.Item.QtyAvailable)
        //                        {
        //                            listingItemDtos.Add(new ListingItemDto() { Sku = listingItem.Item.ItemLookupCode, Qty = listingItem.Item.QtyAvailable, QtySpecified = true });
        //                        }
        //                        else
        //                        {
        //                            listingItemDtos.Add(new ListingItemDto() { Sku = listingItem.Item.ItemLookupCode, Qty = listingItem.Quantity, QtySpecified = true });
        //                        }
        //                    }

        //                    if (listingItemDtos.All(p => p.Qty == 0))
        //                    {
        //                        try
        //                        {
        //                            End(marketplaceID, listing.Code);
        //                        }
        //                        catch (Exception e)
        //                        {
        //                            _logger.Error(e.Message);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        ListingDto listingDto = new ListingDto();
        //                        listingDto.MarketplaceID = marketplaceID;
        //                        listingDto.Code = listing.Code;
        //                        listingDto.Items.AddRange(listingItemDtos);
        //                        listingDto.IsVariation = (bool)listing.IsVariation;


        //                        try
        //                        {
        //                            Revise(listingDto, false, false);
        //                        }
        //                        catch (Exception e)
        //                        {
        //                            _logger.Error(e.Message);
        //                        }

        //                    }
                            
        //                }
        //            }
        //        }
        //    }                                  
        //}


        public void SynchronizeListing(int marketplaceID)
        {
            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                BonanzaMarketplace marketplace = dataContext.BonanzaMarketplaces.Single(p => p.ID == marketplaceID);

                ListingSynchronizer synchronizer = new ListingSynchronizer(marketplace, dataContext);

                string[] status = new string[] { "for_sale", "ready_to_post", "missing_fields", "sold", "pending_pickup", "reserved" };

                marketplace.ListingSyncTime = DateTime.UtcNow;

                synchronizer.SyncByStatus(status);

                dataContext.SaveChanges();
            }
        }

        public void FetchToken(int marketplaceID)
        {
            HttpWebRequest request = WebRequest.CreateHttp("https://api.bonanza.com/api_requests/secure_request");

            request.Headers.Add("X-BONANZLE-API-DEV-NAME", "vWhzo4w8l7sKDUT");
            request.Headers.Add("X-BONANZLE-API-CERT-NAME", "YOL7ZWkbcBJGKTI");
            //request.Headers.Add("Accept","application/json");

            request.Method = "POST";

            string payload = "fetchTokenRequest";

            byte[] binData = Encoding.ASCII.GetBytes(payload);

            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(binData, 0, binData.Length);
            }

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            string output = null;

            using (StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
            {
                output = sr.ReadToEnd();
            }

            //using (berkeleyEntities dataContext = new berkeleyEntities())
            //{
            //    BonanzaMarketplace marketplace = dataContext.BonanzaMarketplaces.Single(p => p.ID == marketplaceID);

            //    marketplace.Toke
            //}
        }

        public void Publish(int marketplaceID, ListingDto listingDto)
        {
         
            if (!_publishers.ContainsKey(marketplaceID))
            {
                _publishers.Add(marketplaceID, new Publisher(marketplaceID));
            }

            Publisher publisher = _publishers[marketplaceID];

            publisher.AddListing(listingDto);
        }

        public void End(int marketplaceID, string code)
        {
            if (!_publishers.ContainsKey(marketplaceID))
            {
                _publishers.Add(marketplaceID, new Publisher(marketplaceID));
            }

            Publisher publisher = _publishers[marketplaceID];

            publisher.EndListing(code);
        }

        public void Revise(int marketplaceID, ListingDto listingDto, bool includeProductData, bool includeTemplate)
        {
            if (!_publishers.ContainsKey(marketplaceID))
            {
                _publishers.Add(marketplaceID, new Publisher(marketplaceID));
            }

            Publisher publisher = _publishers[marketplaceID];

            publisher.ReviseListing(listingDto, includeProductData, includeTemplate);
        }

    }

    public class Publisher
    {
        private const string S3_PICTURE_URL_ROOT = "https://s3.amazonaws.com/berkeleybackup/picture-backups";
        private const string LOCAL_PICTURE_ROOT = @"P:/products";

        private berkeleyEntities _dataContext = new berkeleyEntities();
        private BonanzaMarketplace _marketplace;
        private PictureSetRepository _picSetRepository = new PictureSetRepository();
        private ProductMapperFactory _productMapperFactory = new ProductMapperFactory();
        private Dictionary<string, string> _cachedDesigns = new Dictionary<string, string>();
        private readonly Encoding encoding = Encoding.UTF8;

        public Publisher(int marketplaceID)
        {
            _dataContext.MaterializeAttributes = true;
            _marketplace = _dataContext.BonanzaMarketplaces.Single(p => p.ID == marketplaceID);
        }

        public void AddListing(ListingDto listingDto)
        {
            var pics = _picSetRepository.GetPictures(listingDto.Brand, listingDto.Items.Select(p => p.Sku).ToList()).Take(4).OrderBy(p => p.Name);

            if (pics.Count() == 0)
            {
                throw new InvalidOperationException("picture required");
            }

            foreach(var pic in pics)
            {
                listingDto.PicUrls.Add(pic.Path.Replace("\\","/").Replace(LOCAL_PICTURE_ROOT,S3_PICTURE_URL_ROOT));
            }

            HttpWebRequest request = WebRequest.CreateHttp("https://api.bonanza.com/api_requests/secure_request");

            request.Headers.Add("X-BONANZLE-API-DEV-NAME", "vWhzo4w8l7sKDUT");
            request.Headers.Add("X-BONANZLE-API-CERT-NAME", "YOL7ZWkbcBJGKTI");

            request.ContentType = "application/json";

            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(AcceptAllCertifications);

            request.Method = "POST";

            dynamic addFixedPriceItemRequest = new ExpandoObject();
            addFixedPriceItemRequest.requesterCredentials = new ExpandoObject();
            addFixedPriceItemRequest.requesterCredentials.bonanzleAuthToken = _marketplace.Token;
            addFixedPriceItemRequest.item = MapToBonanzaDto(listingDto, true, true);

            dynamic jsonPayload = new ExpandoObject();
            jsonPayload.addFixedPriceItemRequest = addFixedPriceItemRequest;

            byte[] contentBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jsonPayload));

            request.ContentLength = contentBytes.Length;

            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(contentBytes, 0, contentBytes.Length);
            }

            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                string output = null;

                using (StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    output = sr.ReadToEnd();
                }

                dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(output);

                string ack = result.ack;

                if (ack.Equals("Success"))
                {
                    string itemID = result.addFixedPriceItemResponse.itemId;
                    string sellingState = result.addFixedPriceItemResponse.sellingState;

                    listingDto.Code = itemID;

                    Persist(listingDto, sellingState);
                }
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    var httpResponse = (HttpWebResponse)response;

                    using (Stream data = response.GetResponseStream())
                    {
                        StreamReader sr = new StreamReader(data);
                        throw new Exception(sr.ReadToEnd());
                    }
                }
            }
        }

        public void ReviseListing(ListingDto listingDto, bool includeProductData, bool includeTemplate)
        {
            HttpWebRequest request = WebRequest.CreateHttp("https://api.bonanza.com/api_requests/secure_request");

            request.Headers.Add("X-BONANZLE-API-DEV-NAME", "vWhzo4w8l7sKDUT");
            request.Headers.Add("X-BONANZLE-API-CERT-NAME", "YOL7ZWkbcBJGKTI");

            request.ContentType = "application/json";

            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(AcceptAllCertifications);

            request.Method = "POST";

            dynamic reviseFixedPriceItemRequest = new ExpandoObject();
            reviseFixedPriceItemRequest.requesterCredentials = new ExpandoObject();
            reviseFixedPriceItemRequest.requesterCredentials.bonanzleAuthToken = _marketplace.Token;
            reviseFixedPriceItemRequest.item = MapToBonanzaDto(listingDto, includeProductData, includeTemplate);
            reviseFixedPriceItemRequest.itemId = int.Parse(listingDto.Code);

            dynamic jsonPayload = new ExpandoObject();
            jsonPayload.reviseFixedPriceItemRequest = reviseFixedPriceItemRequest;

            byte[] contentBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jsonPayload));

            request.ContentLength = contentBytes.Length;

            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(contentBytes, 0, contentBytes.Length);
            }

            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                string output = null;

                using (StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    output = sr.ReadToEnd();
                }

                dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(output);

                string ack = result.ack;

                if (ack.Equals("Success"))
                {
                    Update(listingDto);
                }
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    var httpResponse = (HttpWebResponse)response;

                    using (Stream data = response.GetResponseStream())
                    {
                        StreamReader sr = new StreamReader(data);
                        throw new Exception(sr.ReadToEnd());
                    }
                }
            }
        }

        public void EndListing(string code)
        {
            HttpWebRequest request = WebRequest.CreateHttp("https://api.bonanza.com/api_requests/secure_request");

            request.Headers.Add("X-BONANZLE-API-DEV-NAME", "vWhzo4w8l7sKDUT");
            request.Headers.Add("X-BONANZLE-API-CERT-NAME", "YOL7ZWkbcBJGKTI");

            request.ContentType = "application/json";

            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(AcceptAllCertifications);

            request.Method = "POST";

            dynamic endFixedPriceItemRequest = new ExpandoObject();
            endFixedPriceItemRequest.requesterCredentials = new ExpandoObject();
            endFixedPriceItemRequest.requesterCredentials.bonanzleAuthToken = _marketplace.Token;
            endFixedPriceItemRequest.itemID = int.Parse(code);

            dynamic jsonPayload = new ExpandoObject();
            jsonPayload.endFixedPriceItemRequest = endFixedPriceItemRequest;

            byte[] contentBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jsonPayload));

            request.ContentLength = contentBytes.Length;

            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(contentBytes, 0, contentBytes.Length);
            }

            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                string output = null;

                using (StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    output = sr.ReadToEnd();
                }

                dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(output);

                string ack = result.ack;

                if (ack.Equals("Success"))
                {
                    Update(code, "Ended");
                }
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    var httpResponse = (HttpWebResponse)response;

                    using (Stream data = response.GetResponseStream())
                    {
                        StreamReader sr = new StreamReader(data);
                        throw new Exception(sr.ReadToEnd());
                    }
                }
            }
        }

        private void Persist(ListingDto listingDto, string sellingState)
        {
            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                BonanzaListing listing = new BonanzaListing();

                listing.MarketplaceID = _marketplace.ID;
                listing.Code = listingDto.Code;
                listing.FullDescription = listingDto.FullDescription;
                listing.Title = listingDto.Title;
                listing.Status = sellingState;
                listing.LastSyncTime = DateTime.UtcNow;
                listing.Sku = listingDto.Sku;
                listing.IsVariation = listingDto.IsVariation;
               
                foreach (ListingItemDto listingItemDto in listingDto.Items)
                {
                    BonanzaListingItem listingItem = new BonanzaListingItem();
                    listingItem.Listing = listing;
                    listingItem.Item = dataContext.Items.Single(p => p.ItemLookupCode.Equals(listingItemDto.Sku));
                    listingItem.Quantity = listingItemDto.Qty.Value;
                    listingItem.Price = listingItemDto.Price.Value;
                }

                dataContext.SaveChanges();
            }
        }

        private void Update(ListingDto listingDto)
        {
            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                BonanzaListing listing = dataContext.BonanzaListings.Single(p => p.MarketplaceID == _marketplace.ID && p.Code.Equals(listingDto.Code));

                if(!string.IsNullOrWhiteSpace(listingDto.FullDescription))
                {
                    listing.FullDescription = listingDto.FullDescription;
                }

                if(!string.IsNullOrWhiteSpace(listingDto.Title))
                {
                    listing.Title = listingDto.Title;
                }

                foreach (ListingItemDto listingItemDto in listingDto.Items)
                {
                    Item item = dataContext.Items.Single(p => p.ItemLookupCode.Equals(listingItemDto.Sku));

                    BonanzaListingItem listingItem = listing.ListingItems.SingleOrDefault(p => p.Item.ID == item.ID);

                    if (listingItem == null)
                    {
                        listingItem = new BonanzaListingItem() { Item = item, Listing = listing };
                    }

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

                dataContext.SaveChanges();
            }
        }

        private void Update(string code, string status)
        {
            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                BonanzaListing listing = dataContext.BonanzaListings.Single(p => p.MarketplaceID == _marketplace.ID && p.Code.Equals(code));

                listing.Status = status;

                listing.LastSyncTime = DateTime.UtcNow;

                dataContext.SaveChanges();
            }
        }

        private dynamic MapToBonanzaDto(ListingDto listingDto, bool includeProductData, bool includeTemplate)
        {
            dynamic item = new ExpandoObject();

            if (!string.IsNullOrEmpty(listingDto.Sku))
            {
                item.sku = listingDto.Sku;
            }

            if (!string.IsNullOrEmpty(listingDto.Title))
            {
                item.title = listingDto.Title;
            }

            if (includeTemplate)
            {
                string designName = string.IsNullOrWhiteSpace(listingDto.Design) ? "default" : listingDto.Design;

                item.description = GetDesign(designName).Replace("<!-- INSERT FULL DESCRIPTION -->", listingDto.FullDescription);
            }

            item.primaryCategory = new ExpandoObject();
            item.primaryCategory.categoryId = "Guess";

            if (listingDto.PicUrls.Count > 0)
            {
                item.pictureDetails = new ExpandoObject();
                item.pictureDetails.pictureURL = listingDto.PicUrls.ToArray();
            }

            if (includeTemplate)
            {
                item.shippingDetails = new ExpandoObject();

                dynamic localOption = new ExpandoObject();
                localOption.shippingType = "Free";
                localOption.freeShipping = true;

                item.shippingDetails.shippingServiceOptions = new[] { localOption };

                dynamic worldwideOption = new ExpandoObject();
                worldwideOption.shippingType = "Fixed";
                worldwideOption.shippingServiceCost = 29.95;
                worldwideOption.shipToLocation = "Worldwide";

                dynamic canadaOption = new ExpandoObject();
                canadaOption.shippingType = "Fixed";
                canadaOption.shippingServiceCost = 17.99;
                canadaOption.shipToLocation = "Canada";
                
                item.shippingDetails.internationalShippingServiceOption = new[] { worldwideOption, canadaOption };


                item.returnPolicy = new ExpandoObject();
                item.returnPolicy.returnsAcceptedOption = "ReturnsAccepted";
                item.returnPolicy.description = "Items must not be worn and should be returned in their original box and packaging. Buyer is responsible for return shipping costs. Once we receive you return, we will refund the cost of the product only. We don&#39;t refund original shipping charges. Also, when returning an article of clothing please return it with its respective tags, unworn, and in its original packaging.";
                item.returnPolicy.returnsWithinOption = 30;
                item.returnPolicy.shippingCostPaidByOption = "buyer";
            }

            if (listingDto.Items.Any(p => p.Price.HasValue))
            {
                item.price = Convert.ToDouble(listingDto.Items.First(p => p.Price.HasValue).Price.Value);
            }

            if (!listingDto.IsVariation)
            {
                ListingItemDto listingItemDto = listingDto.Items.First();

                if (listingItemDto.Qty.HasValue)
                {
                    item.quantity = listingItemDto.Qty.Value ;
                }

                if (includeProductData)
                {
                    ProductMapper mapper = _productMapperFactory.GetProductMapper(_dataContext.Items.Single(p => p.ItemLookupCode.Equals(listingItemDto.Sku)));

                    var itemSpecifics = mapper.GetItemSpecifics().Concat(mapper.GetVariationSpecifics()).ToArray();

                    item.itemSpecifics = itemSpecifics.Select(p => new []{  p.Name, p.Value }).ToArray();
                }
            }
            else
            {

                if (includeProductData)
                {
                    string sku = listingDto.Items.First().Sku;

                    ProductMapper mapper = _productMapperFactory.GetProductMapper(_dataContext.Items.Single(p => p.ItemLookupCode.Equals(sku)));

                    var itemSpecifics = mapper.GetItemSpecifics().ToArray();

                    item.itemSpecifics = itemSpecifics.Select(p => new[] { p.Name, p.Value }).ToArray();
                }

                List<ExpandoObject> variations = new List<ExpandoObject>();

                foreach (ListingItemDto listingItemDto in listingDto.Items)
                {
                    dynamic variationDto = new ExpandoObject();

                    if (listingItemDto.Qty.HasValue)
                    {
                        variationDto.quantity = listingItemDto.Qty.Value ;
                    }
                    
                    if (includeProductData)
                    {
                        ProductMapper productData = _productMapperFactory.GetProductMapper(_dataContext.Items.Single(p => p.ItemLookupCode.Equals(listingItemDto.Sku)));
                        variationDto.nameValueList = productData.GetVariationSpecifics().Select(p => new { name = p.Name , value = p.Value}).ToArray();
                    }

                    variations.Add(variationDto);
                }

                item.variations = variations.ToArray();
            }

            return item;
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

        private bool AcceptAllCertifications(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

    }

    public class FileParameter
    {
        public byte[] File { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public FileParameter(byte[] file) : this(file, null) { }
        public FileParameter(byte[] file, string filename) : this(file, filename, null) { }
        public FileParameter(byte[] file, string filename, string contenttype)
        {
            File = file;
            FileName = filename;
            ContentType = contenttype;
        }
    }

    public class ListingDto
    {
        public ListingDto()
        {
            this.Items = new List<ListingItemDto>();
            this.PicUrls = new List<string>();
        }

        public string Code { get; set; }

        public string Brand { get; set; }

        public string Sku {get; set;}

        public string FullDescription {get; set;}

        public string Title {get; set;}

        public bool IsVariation {get; set;}

        public List<string> PicUrls { get; set; }

        public List<ListingItemDto> Items {get; set;}

        public string Template { get; set; }

        public string Design { get; set; }
    }

    public class ListingItemDto
    {
        public string Sku {get; set;}

        public int? Qty {get; set;}

        public decimal? Price {get; set;}
    }

    public class ListingSynchronizer
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BonanzaMarketplace _marketplace;
        private berkeleyEntities _dataContext;

        public ListingSynchronizer(BonanzaMarketplace marketplace, berkeleyEntities dataContext)
        {
            _marketplace = marketplace;
            _dataContext = dataContext;
        }


        public void SyncByStatus(string[] status)
        {
            dynamic getBoothItemsRequest = new ExpandoObject();
            getBoothItemsRequest.requesterCredentials = new ExpandoObject();
            getBoothItemsRequest.requesterCredentials.bonanzleAuthToken = _marketplace.Token;
            getBoothItemsRequest.boothId = _marketplace.Name;
            getBoothItemsRequest.itemStatus = status;

            dynamic jsonPayload = new ExpandoObject();
            jsonPayload.getBoothItemsRequest = getBoothItemsRequest;


            dynamic response = SendJsonRequest(jsonPayload);

            string ack = response.ack;

            if (ack.Equals("Success"))
            {
                foreach (var item in response.getBoothItemsResponse.items)
                {
 
                }
            }

        }

        private dynamic SendJsonRequest(dynamic jsonPayload)
        {
            HttpWebRequest request = WebRequest.CreateHttp("https://api.bonanza.com/api_requests/secure_request");

            request.Headers.Add("X-BONANZLE-API-DEV-NAME", "vWhzo4w8l7sKDUT");
            request.Headers.Add("X-BONANZLE-API-CERT-NAME", "YOL7ZWkbcBJGKTI");

            request.ContentType = "application/json";

            request.Method = "POST";

            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(AcceptAllCertifications);

            byte[] contentBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jsonPayload));

            request.ContentLength = contentBytes.Length;

            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(contentBytes, 0, contentBytes.Length);
            }

            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                string output = null;

                using (StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    output = sr.ReadToEnd();
                }

                dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(output);


                return result;
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    var httpResponse = (HttpWebResponse)response;

                    using (Stream data = response.GetResponseStream())
                    {
                        StreamReader sr = new StreamReader(data);
                        throw new Exception(sr.ReadToEnd());
                    }
                }
            }
        }

        private void ProcessListingData(ItemTypeCollection listingsDtos)
        {
            foreach (ItemType listingDto in listingsDtos)
            {
                BonanzaListing listing = _marketplace.Listings.SingleOrDefault(p => p.Code.Equals(listingDto.ItemID));

                if (listing == null)
                {
                    listing = new BonanzaListing();
                }
                
                try
                {
                    Map(listing, listingDto);
                }
                catch (PropertyConstraintException e)
                {
                    //Map(listing, SyncListing(listingDto.ItemID));
                }
            }
        }

        private void Map(BonanzaListing listing, dynamic listingDto)
        {
            listing.Code = listingDto.ItemID;

            listing.Marketplace = _marketplace;

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

        private void MapListingItem(BonanzaListing listing, ItemType listingDto)
        {
            string sku = listingDto.SKU != null ? listingDto.SKU.ToUpper().Trim() : listing.Sku;

            BonanzaListingItem listingItem = GetListingItem(listing, sku);

            listingItem.Price = listingDto.StartPrice != null ? decimal.Parse(listingDto.StartPrice.Value.ToString()) : listingItem.Price;

            listingItem.Quantity =
                listingDto.QuantitySpecified &&
                listingDto.SellingStatus != null &&
                listingDto.SellingStatus.QuantitySoldSpecified ?
                    listingDto.Quantity - listingDto.SellingStatus.QuantitySold : listingItem.Quantity;
        }

        private void MapListingItem(BonanzaListing listing, dynamic variationDtos)
        {
            //var currentSkus = variationDtos.Variation.ToArray().Select(p => p.SKU);

            //foreach (var listingItem in listing.ListingItems)
            //{
            //    if (!currentSkus.Any(p => p.Equals(listingItem.Sku)))
            //    {
            //        listingItem.Quantity = 0;
            //    }
            //}

            //foreach (VariationType variationDto in variationDtos.Variation)
            //{
            //    string sku = variationDto.SKU.ToUpper().Trim();

            //    EbayListingItem listingItem = GetListingItem(listing, sku);

            //    listingItem.Price = variationDto.StartPrice != null ? Convert.ToDecimal(variationDto.StartPrice.Value) : listingItem.Price;

            //    listingItem.Quantity =
            //        variationDto.QuantitySpecified &&
            //        variationDto.SellingStatus != null &&
            //        variationDto.SellingStatus.QuantitySoldSpecified ?
            //            variationDto.Quantity - variationDto.SellingStatus.QuantitySold : listingItem.Quantity;
            //}
        }

        private BonanzaListingItem GetListingItem(BonanzaListing listing, string sku)
        {
            //BonanzaListingItem listingItem = listing.ListingItems.SingleOrDefault(p => p.Sku.Equals(sku));

            //if (listingItem == null)
            //{
            //    Item item = _dataContext.Items.SingleOrDefault(p => p.ItemLookupCode.Equals(sku));

            //    listingItem = new EbayListingItem() { Item = item, Listing = listing, Sku = sku };
            //}

            //return listingItem;

            return null;
        }

        private bool AcceptAllCertifications(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
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
