using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MarketplaceWebService;
using MarketplaceWebServiceOrders;
using MarketplaceWebServiceProducts;
using System.Xml.Serialization;
using System.IO;

namespace BerkeleyEntities
{
    public partial class AmznMarketplace
    {
        public MarketplaceWebServiceClient GetMWSClient()
        {
            MarketplaceWebServiceConfig config = new MarketplaceWebServiceConfig();

            config.ServiceURL = "https://mws.amazonservices.com";

            config.SetUserAgentHeader(
              "berkeley",
               "1.0",
               "C#",
               "<Parameter 1>", "<Parameter 2>");


            MarketplaceWebServiceClient service =
                new MarketplaceWebServiceClient(
                    this.AccessKeyId,
                    this.SecretAccessKey,
                    "berkeley",
                    "1.0",
                    config);



            return service;
        }

        public MarketplaceWebServiceProductsClient GetMWSProductsClient()
        {
            MarketplaceWebServiceProductsConfig config = new MarketplaceWebServiceProductsConfig();

            config.ServiceURL = "https://mws.amazonservices.com/Products/2011-10-01";

            MarketplaceWebServiceProductsClient service =
                new MarketplaceWebServiceProductsClient(
                    "berkeley",
                    "1.0",
                    this.AccessKeyId,
                    this.SecretAccessKey,
                    config);


            return service;
        }

        public MarketplaceWebServiceOrdersClient GetMWSOrdersClient()
        {
            MarketplaceWebServiceOrdersConfig config = new MarketplaceWebServiceOrdersConfig();

            //
            // IMPORTANT: Uncomment out the appropriate line for the country you wish 
            // to sell in:
            // 
            // United States:
            config.ServiceURL = "https://mws.amazonservices.com/Orders/2011-01-01";
            //
            // Canada:
            // config.ServiceURL = "https://mws.amazonservices.ca/Orders/2011-01-01";
            //
            // Europe:
            // config.ServiceURL = "https://mws-eu.amazonservices.com/Orders/2011-01-01";
            //
            // Japan:
            // config.ServiceURL = "https://mws.amazonservices.jp/Orders/2011-01-01";
            //
            // China:
            // config.ServiceURL = "https://mws.amazonservices.com.cn/Orders/2011-01-01";
            //

            /************************************************************************
            * Instantiate  Implementation of Marketplace Web Service Orders  
            ***********************************************************************/

            MarketplaceWebServiceOrdersClient service = new MarketplaceWebServiceOrdersClient(
                "SyncManager", "N/A", this.AccessKeyId, this.SecretAccessKey, config);

            return service;
        }

        public List<string> GetWaitingSyncOrders()
        {
            string path = this.RootDir + @"/" + this.Code + @"/waitingOrders.xml";

            return ReadXmlFile(path);
        }

        public List<string> GetWaitingSyncListings()
        {
            string path = this.RootDir + @"/" + this.Code + @"/waitingListings.xml";

            return ReadXmlFile(path);
        }

        public void SetWaitingSyncOrders(List<string> orders)
        {
            string path = this.RootDir + @"/" + this.Code + @"/waitingOrders.xml";

            if (orders.Count == 0)
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            else
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<string>));
                serializer.Serialize(File.Open(path, FileMode.Create), orders);
            }
        }

        public void SetWaitingSyncListings(List<string> listings)
        {
            string path = this.RootDir + @"\" + this.Code + @"\waitingListings.xml";

            if (listings.Count == 0)
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            else
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<string>));
                serializer.Serialize(File.Open(path, FileMode.Create), listings);
            }
        }

        private List<string> ReadXmlFile(string path)
        {
            List<string> waitingSync;

            if (File.Exists(path))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<string>));
                waitingSync = (List<string>)serializer.Deserialize(File.OpenRead(path));
            }
            else
            {
                waitingSync = new List<string>();
            }

            return waitingSync;
        }
    }
}
