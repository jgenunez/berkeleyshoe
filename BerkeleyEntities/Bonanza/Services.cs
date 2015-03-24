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

        public void Publish(ListingDto listingDto)
        {
            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                dataContext.MaterializeAttributes = true;

                BonanzaMarketplace marketplace = dataContext.BonanzaMarketplaces.Single(p => p.ID == listingDto.MarketplaceID);

                Publisher publisher = new Publisher(dataContext, marketplace);

                publisher.AddListing(listingDto);
            }
        }

        public void End(int marketplaceID, string code)
        {
            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                dataContext.MaterializeAttributes = true;

                BonanzaMarketplace marketplace = dataContext.BonanzaMarketplaces.Single(p => p.ID == marketplaceID);

                Publisher publisher = new Publisher(dataContext, marketplace);

                publisher.EndListing(code);
            }
        }

        public void Revise(ListingDto listingDto, bool includeProductData, bool includeTemplate)
        {
            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                dataContext.MaterializeAttributes = true;

                BonanzaMarketplace marketplace = dataContext.BonanzaMarketplaces.Single(p => p.ID == listingDto.MarketplaceID);

                Publisher publisher = new Publisher(dataContext, marketplace);

                publisher.ReviseListing(listingDto, includeProductData, includeTemplate);
            }
        }

    }

    public class Publisher
    {
        private const string S3_PICTURE_URL_ROOT = "https://s3.amazonaws.com/berkeleybackup/picture-backups";
        private const string LOCAL_PICTURE_ROOT = @"P:/products";

        private berkeleyEntities _dataContext;
        private BonanzaMarketplace _marketplace;
        private PictureSetRepository _picSetRepository = new PictureSetRepository();
        private ProductMapperFactory _productMapperFactory = new ProductMapperFactory();
        private Dictionary<string, string> _cachedDesigns = new Dictionary<string, string>();
        private readonly Encoding encoding = Encoding.UTF8;

        public Publisher(berkeleyEntities dataContext, BonanzaMarketplace marketplace)
        {
            _dataContext = dataContext;
            _marketplace = marketplace;
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

            string boundary = "MIME_boundary";
            string CRLF = "\r\n";

            HttpWebRequest request = WebRequest.CreateHttp("https://api.bonanza.com/api_requests/secure_request");

            request.Headers.Add("X-BONANZLE-API-DEV-NAME", "vWhzo4w8l7sKDUT");
            request.Headers.Add("X-BONANZLE-API-CERT-NAME", "YOL7ZWkbcBJGKTI");

            request.ContentType = "application/json";

            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(AcceptAllCertifications);

            request.Method = "POST";

            string jsonPayload = JsonConvert.SerializeObject(MapToBonanzaDto(listingDto, true, true));

            byte[] contentBytes = Encoding.UTF8.GetBytes(jsonPayload);

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

        //public void AddListing(ListingDto listingDto)
        //{
        //    Dictionary<string, object> postParameters = new Dictionary<string, object>();

            
        //    var pics = _picSetRepository.GetPictures(listingDto.Brand, listingDto.Items.Select(p => p.Sku).ToList());

        //    if (pics.Count() == 0)
        //    {
        //        throw new InvalidOperationException("picture required");
        //    }

                
        //    foreach(var pic in pics.Take(4).OrderBy(p => p.Name))
        //    {
        //        using(FileStream fs = new FileStream(pic.Path, FileMode.Open, FileAccess.Read))
        //        {
                    

        //            byte[] data = new byte[fs.Length];
        //            fs.Read(data, 0, data.Length);

        //            var base64data = Convert.ToBase64String(data);

        //            string fileName = Path.GetFileName(pic.Path);

        //            listingDto.PictureFiles.Add(fileName);

        //            postParameters.Add(pic.Name, new FileParameter(data, fileName, "image/jpeg"));
        //        }
        //    }

           

        //    try
        //    {
        //        string boundary = "--Mime_Boundary--";
        //        string contentType = "multipart/form-data; boundary=" + boundary;
        //        string CRLF = "\r\n";

        //        string jsonPart = boundary + CRLF +
        //            "Content-Disposition: form-data; name=addFixedPriceItemRequest" + CRLF +
        //            "Content-Type: application/json; charset=\"UTF-8\"" + CRLF + CRLF +
        //           ((string)JsonConvert.SerializeObject(MapToBonanzaDto(listingDto, true, true))) + CRLF;

        //        byte[] jsonData = Encoding.UTF8.GetBytes(jsonPart);

        //        byte[] imageData = GetMultipartFormData(postParameters, boundary);

        //        using (FileStream stream = new FileStream(@"C:\Users\JUAN\Desktop\testing.txt", FileMode.OpenOrCreate))
        //        {
        //            stream.Write(jsonData.Concat(imageData).ToArray(), 0, jsonData.Concat(imageData).ToArray().Length);
        //        }
                
        //        HttpWebResponse response = PostForm("https://api.bonanza.com/api_requests/secure_request", contentType, jsonData.Concat(imageData).ToArray());


        //        string output = null;

        //        using (StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
        //        {
        //            output = sr.ReadToEnd();
        //        }

        //        dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(output);

        //        if (result.ack.Equals("Success"))
        //        {
        //            string itemID = result.addFixedPriceItemResponse.itemId;
        //            string sellingState = result.addFixedPriceItemResponse.sellingState;

        //            listingDto.Code = itemID;

        //            Persist(listingDto, sellingState);    
        //        }
        //    }
        //    catch (WebException e)
        //    {
                
        //        using (WebResponse response = e.Response)
        //        {
        //            var httpResponse = (HttpWebResponse)response;

        //            using (Stream data = response.GetResponseStream())
        //            {
        //                StreamReader sr = new StreamReader(data);
        //                throw new Exception(sr.ReadToEnd());
        //            }
        //        }
        //    }


            
        //}

        private bool AcceptAllCertifications(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public void ReviseListing(ListingDto listingDto, bool includeProductData, bool includeTemplate)
        {
            var ebayDto = MapToBonanzaDto(listingDto, includeProductData, includeTemplate);

            //if (listingDto.IsVariation)
            //{
            //    ReviseFixedPriceItemCall call = new ReviseFixedPriceItemCall(_marketplace.GetApiContext());
            //    ReviseFixedPriceItemResponseType response = call.ExecuteRequest(new ReviseFixedPriceItemRequestType() { Item = ebayDto }) as ReviseFixedPriceItemResponseType;
            //}
            //else
            //{
            //    ReviseItemCall call = new ReviseItemCall(_marketplace.GetApiContext());
            //    ReviseItemResponseType response = call.ExecuteRequest(new ReviseItemRequestType() { Item = ebayDto}) as ReviseItemResponseType;
            //}

            Update(listingDto);
        }

        public void EndListing(string code)
        {
            //EndItemRequestType request = new EndItemRequestType();
            //request.ItemID = code;
            //request.EndingReasonSpecified = true;
            //request.EndingReason = EndReasonCodeType.NotAvailable;

            //EndItemCall call = new EndItemCall(_marketplace.GetApiContext());

            //EndItemResponseType response = call.ExecuteRequest(request) as EndItemResponseType;

            //using (berkeleyEntities dataContext = new berkeleyEntities())
            //{
            //    EbayListing listing = dataContext.EbayListings.Single(p => p.MarketplaceID == _marketplace.ID && p.Code.Equals(code));

            //    listing.Status = EbayMarketplace.STATUS_COMPLETED;

            //    if (response.EndTimeSpecified)
            //    {
            //        listing.EndTime = response.EndTime;
            //    }

            //    dataContext.SaveChanges();
            //}
        }

        private void Persist(ListingDto listingDto, string sellingState)
        {
            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                BonanzaListing listing = new BonanzaListing();

                listing.MarketplaceID = listingDto.MarketplaceID;
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
                    listingItem.Title = listingItemDto.Title;
                }

                dataContext.SaveChanges();
            }
        }

        private void Update(ListingDto listingDto)
        {
            //using (berkeleyEntities dataContext = new berkeleyEntities())
            //{
            //    EbayListing listing = dataContext.EbayListings.Single(p => p.MarketplaceID == _marketplace.ID && p.Code.Equals(listingDto.Code));

            //    listing.Title = !string.IsNullOrWhiteSpace(listingDto.Title) ? listingDto.Title : listing.Title;

            //    listing.BinPrice = listingDto.BinPriceSpecified ? listingDto.BinPrice : listing.BinPrice;

            //    listing.FullDescription = !string.IsNullOrWhiteSpace(listingDto.FullDescription) ? listingDto.FullDescription : listing.FullDescription;

            //    foreach (ListingItemDto listingItemDto in listingDto.Items)
            //    {
            //        Item item = dataContext.Items.Single(p => p.ItemLookupCode.Equals(listingItemDto.Sku));

            //        EbayListingItem listingItem = listing.ListingItems.SingleOrDefault(p => p.Item.ID == item.ID);

            //        if (listingItem == null)
            //        {
            //            listingItem = new EbayListingItem() { Item = item, Listing = listing, Sku = listingItemDto.Sku };
            //        }

            //        listingItem.Quantity = listingItemDto.QtySpecified ? listingItemDto.Qty : listingItem.Quantity;
            //        listingItem.Title = !string.IsNullOrEmpty(listingItemDto.Title) ? listingItemDto.Title : listingItem.Title;
            //        listingItem.Price = listingItemDto.PriceSpecified ? listingItemDto.Price : listingItem.Price;
            //    }

            //    listing.LastSyncTime = DateTime.UtcNow;

            //    dataContext.SaveChanges();
            //}
        }

        private dynamic MapToBonanzaDto(ListingDto listingDto, bool includeProductData, bool includeTemplate)
        {
            dynamic jsonPayload = new ExpandoObject(); 

            dynamic addFixedPriceItemRequest = new ExpandoObject();

            jsonPayload.addFixedPriceItemRequest = addFixedPriceItemRequest;

            addFixedPriceItemRequest.requesterCredentials = new ExpandoObject();
            addFixedPriceItemRequest.requesterCredentials.bonanzleAuthToken = _marketplace.Token;

            dynamic item = new ExpandoObject();

            addFixedPriceItemRequest.item = item;

            if (!string.IsNullOrEmpty(listingDto.Code))
            {
                item.itemId = listingDto.Code;
            }

            if(!string.IsNullOrEmpty(listingDto.Sku))
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

                dynamic shippingServiceOptions = new ExpandoObject();
                shippingServiceOptions.shippingType = "Free";
                shippingServiceOptions.freeShipping = true;

                item.shippingDetails.shippingServiceOptions = new[] { shippingServiceOptions };

                dynamic internationalShippingServiceOptions = new ExpandoObject();
                internationalShippingServiceOptions.shippingType = "Fixed";
                internationalShippingServiceOptions.shippingServiceCost = 30.00;
                internationalShippingServiceOptions.shipToLocation = "Worldwide";

                item.shippingDetails.internationalShippingServiceOptions = new[] { internationalShippingServiceOptions };


                item.returnPolicy = new ExpandoObject();
                item.returnPolicy.returnsAcceptedOption = "ReturnsAccepted";

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

            return jsonPayload;
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

        //private HttpWebResponse PostForm(string postUrl, string contentType, byte[] formData)
        //{
        //    HttpWebRequest request = WebRequest.Create(postUrl) as HttpWebRequest;

        //    if (request == null)
        //    {
        //        throw new NullReferenceException("request is not a http request");
        //    }

        //    request.Headers.Add("X-BONANZLE-API-DEV-NAME", "vWhzo4w8l7sKDUT");
        //    request.Headers.Add("X-BONANZLE-API-CERT-NAME", "YOL7ZWkbcBJGKTI");

        //    // Set up the request properties.
        //    request.Method = "POST";
        //    request.ContentType = contentType;
        //    request.ContentLength = formData.Length;

        //    ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(AcceptAllCertifications);

           

        //    // You could add authentication here as well if needed:
        //    // request.PreAuthenticate = true;
        //    // request.AuthenticationLevel = System.Net.Security.AuthenticationLevel.MutualAuthRequested;
        //    // request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(System.Text.Encoding.Default.GetBytes("username" + ":" + "password")));

        //    // Send the form data to the request.
        //    using (Stream requestStream = request.GetRequestStream())
        //    {
        //        requestStream.Write(formData, 0, formData.Length);
        //        requestStream.Close();
        //    }

        //    return request.GetResponse() as HttpWebResponse;
        //}

        //private byte[] GetMultipartFormData(Dictionary<string, object> postParameters, string boundary)
        //{
        //    Stream formDataStream = new System.IO.MemoryStream();

        //    foreach (var param in postParameters)
        //    {
        //        if (param.Value is FileParameter)
        //        {
        //            FileParameter fileToUpload = (FileParameter)param.Value;

        //            string header = string.Format("{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\";\r\nContent-Type: {3};\r\nContent-Transfer-Encoding: base64\r\n\r\n",
        //                 boundary,
        //                 param.Key,
        //                 fileToUpload.FileName ?? param.Key,
        //                 fileToUpload.ContentType ?? "application/octet-stream");
                    
        //            formDataStream.Write(encoding.GetBytes(header), 0, encoding.GetByteCount(header));

        //            // Write the file data directly to the Stream, rather than serializing it to a string.
        //            formDataStream.Write(fileToUpload.File, 0, fileToUpload.File.Length);
        //        }
        //        else
        //        {
        //            string postData = string.Format("{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}",
        //                boundary,
        //                param.Key,
        //                param.Value);

        //            formDataStream.Write(encoding.GetBytes(postData), 0, encoding.GetByteCount(postData));
        //        }

        //        formDataStream.Write(encoding.GetBytes("\r\n"), 0, encoding.GetByteCount("\r\n"));
        //    }

        //    // Add the end of the request.  Start with a newline
        //    string footer =  boundary + "\r\n";
        //    formDataStream.Write(encoding.GetBytes(footer), 0, encoding.GetByteCount(footer));

        //    // Dump the Stream into a byte[]
        //    formDataStream.Position = 0;
        //    byte[] formData = new byte[formDataStream.Length];
        //    formDataStream.Read(formData, 0, formData.Length);
        //    formDataStream.Close();

            

        //    return formData;
        //}
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

        public int MarketplaceID { get; set; }

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

        public string Title { get; set; }
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
