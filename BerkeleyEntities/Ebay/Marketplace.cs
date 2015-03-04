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
        public const string FORMAT_STOREFIXEDPRICE = "StoresFixedPrice";
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

    }

    

    
}
