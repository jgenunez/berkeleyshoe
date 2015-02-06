using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using eBay.Service.Core.Sdk;
using eBay.Service.Core.Soap;
using eBay.Service.Util;
using System.IO;
using System.Xml.Serialization;
using eBay.Service.Call;
using System.Net;
using System.Xml;
using BerkeleyEntities.Ebay.Mappers;
using NLog;
using System.Data;
using BerkeleyEntities.Ebay;

namespace BerkeleyEntities
{
    public partial class EbayMarketplace
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public const string FORMAT_FIXEDPRICE = "FixedPriceItem";
        public const string FORMAT_AUCTION = "Chinese";

        public const string STATUS_ACTIVE = "Active";
        public const string STATUS_COMPLETED = "Completed";
        public const string STATUS_DELETED = "Deleted";

        private ProductMapperFactory _productMapperFactory = new ProductMapperFactory();
        private ApiContext _apiContext;
        private string _template;

        public ApiContext GetApiContext()
        {
            if (_apiContext == null)
            {
                ApiContext apiContext = new ApiContext();
                apiContext.SoapApiServerUrl = @"https://api.ebay.com/wsapi";

                ApiCredential apiCredential = new ApiCredential();
                apiCredential.eBayToken = this.Token;
                apiContext.ApiCredential = apiCredential;
                apiContext.Site = SiteCodeType.US;
                apiContext.ApiLogManager = new ApiLogManager();
                apiContext.ApiLogManager.ApiLoggerList.Add(new FileLogger("listing_log.txt", true, true, true));
                apiContext.ApiLogManager.EnableLogging = false;

                _apiContext = apiContext;
            }

            return _apiContext;
        }

        

        public ShippingDetailsType GetShippingDetails()
        {
            ShippingDetailsType shippingDetails = null;

            if (this.ID == 1 || this.ID == 3)
            {
                shippingDetails = new ShippingDetailsType()
                {
                    ShippingType = ShippingTypeCodeType.Flat,
                    ShippingServiceOptions = new ShippingServiceOptionsTypeCollection() { 
                    new ShippingServiceOptionsType() { 
                        ShippingServicePrioritySpecified = true,
                        ShippingServicePriority = 1,
                        ShippingService = ShippingServiceCodeType.UPSGround.ToString(),
                       
                        FreeShipping = true,
                        FreeShippingSpecified = true},
                    new ShippingServiceOptionsType(){
                        ShippingServicePrioritySpecified = true,
                        ShippingServicePriority = 2,
                        ShippingService = ShippingServiceCodeType.UPSNextDay.ToString(),
                        ShippingServiceCost = new AmountType() { currencyID = CurrencyCodeType.USD, Value = 29.95 },
                        ShippingServiceAdditionalCost = new AmountType() { currencyID = CurrencyCodeType.USD, Value = 10.00 }}},
                    InternationalShippingServiceOption = new InternationalShippingServiceOptionsTypeCollection() {
                    new InternationalShippingServiceOptionsType(){
                        ShippingServicePrioritySpecified = true,
                        ShippingServicePriority = 1,
                        ShippingService =  ShippingServiceCodeType.USPSPriorityMailInternational.ToString(),
                        ShippingServiceCost = new AmountType() { currencyID = CurrencyCodeType.USD, Value = 17.99 } ,
                        ShippingServiceAdditionalCost = new AmountType() { currencyID = CurrencyCodeType.USD, Value = 5.00 } ,
                        ShipToLocation = new StringCollection { CountryCodeType.CA.ToString() }},
                    new InternationalShippingServiceOptionsType(){
                        ShippingServicePrioritySpecified = true,
                        ShippingServicePriority = 2,
                        ShippingService =  ShippingServiceCodeType.USPSPriorityMailInternational.ToString(),
                        ShippingServiceCost = new AmountType() { currencyID = CurrencyCodeType.USD, Value = 40.00 } ,
                        ShippingServiceAdditionalCost = new AmountType() { currencyID = CurrencyCodeType.USD, Value = 10.00 } ,
                        ShipToLocation = new StringCollection { ShippingRegionCodeType.Worldwide.ToString() }},
                    new InternationalShippingServiceOptionsType(){
                        ShippingServicePrioritySpecified = true,
                        ShippingServicePriority = 3,
                        ShippingService =  ShippingServiceCodeType.UPSWorldWideExpedited.ToString(),
                        ShippingServiceCost = new AmountType() { currencyID = CurrencyCodeType.USD, Value = 60.00 } ,
                        ShippingServiceAdditionalCost = new AmountType() { currencyID = CurrencyCodeType.USD, Value = 10.00 } ,
                        ShipToLocation = new StringCollection { ShippingRegionCodeType.Worldwide.ToString() }}}
                };
            }
            else
            {
                shippingDetails = new ShippingDetailsType()
                {
                    ShippingType = ShippingTypeCodeType.Flat,
                    ShippingServiceOptions = new ShippingServiceOptionsTypeCollection() { 
                    new ShippingServiceOptionsType() { 
                        ShippingServicePrioritySpecified = true,
                        ShippingServicePriority = 1,
                        ShippingService = ShippingServiceCodeType.UPSGround.ToString(),
                        ShippingServiceCost = new AmountType() { currencyID = CurrencyCodeType.USD, Value = 7.95 },
                        ShippingServiceAdditionalCost = new AmountType() { currencyID = CurrencyCodeType.USD, Value = 2.50 },
                        ShippingSurcharge = new AmountType() { currencyID = CurrencyCodeType.USD, Value = 0 }},
                    new ShippingServiceOptionsType(){
                        ShippingServicePrioritySpecified = true,
                        ShippingServicePriority = 2,
                        ShippingService = ShippingServiceCodeType.UPSNextDay.ToString(),
                        ShippingServiceCost = new AmountType() { currencyID = CurrencyCodeType.USD, Value = 29.95 },
                        ShippingServiceAdditionalCost = new AmountType() { currencyID = CurrencyCodeType.USD, Value = 10.00 }}},
                    InternationalShippingServiceOption = new InternationalShippingServiceOptionsTypeCollection() {
                    new InternationalShippingServiceOptionsType(){
                        ShippingServicePrioritySpecified = true,
                        ShippingServicePriority = 1,
                        ShippingService =  ShippingServiceCodeType.USPSPriorityMailInternational.ToString(),
                        ShippingServiceCost = new AmountType() { currencyID = CurrencyCodeType.USD, Value = 17.99 } ,
                        ShippingServiceAdditionalCost = new AmountType() { currencyID = CurrencyCodeType.USD, Value = 5.00 } ,
                        ShipToLocation = new StringCollection { CountryCodeType.CA.ToString() }},
                    new InternationalShippingServiceOptionsType(){
                        ShippingServicePrioritySpecified = true,
                        ShippingServicePriority = 2,
                        ShippingService =  ShippingServiceCodeType.USPSPriorityMailInternational.ToString(),
                        ShippingServiceCost = new AmountType() { currencyID = CurrencyCodeType.USD, Value = 40.00 } ,
                        ShippingServiceAdditionalCost = new AmountType() { currencyID = CurrencyCodeType.USD, Value = 10.00 } ,
                        ShipToLocation = new StringCollection { ShippingRegionCodeType.Worldwide.ToString() }},
                    new InternationalShippingServiceOptionsType(){
                        ShippingServicePrioritySpecified = true,
                        ShippingServicePriority = 3,
                        ShippingService =  ShippingServiceCodeType.UPSWorldWideExpedited.ToString(),
                        ShippingServiceCost = new AmountType() { currencyID = CurrencyCodeType.USD, Value = 60.00 } ,
                        ShippingServiceAdditionalCost = new AmountType() { currencyID = CurrencyCodeType.USD, Value = 10.00 } ,
                        ShipToLocation = new StringCollection { ShippingRegionCodeType.Worldwide.ToString() }}}
                };
            }

            return shippingDetails;
        }

        public string GetTemplate()
        {
            if (string.IsNullOrEmpty(_template))
            {
                string path = this.RootDir + @"\" + this.Code + @"\Template.html";

                StreamReader reader = new StreamReader(File.OpenRead(path));

                _template = reader.ReadToEnd();
            }

            return _template;
        }

    }

    

    
}
