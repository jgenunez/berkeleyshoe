using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MarketplaceWebService;
using MarketplaceWebServiceOrders;
using MarketplaceWebServiceProducts;
using System.Xml.Serialization;
using System.IO;
using MarketplaceWebServiceProducts.Model;
using System.Timers;

namespace BerkeleyEntities
{
    public partial class AmznMarketplace
    {
        public List<GetMatchingProductForIdResult> GetCatalogData(IEnumerable<string> upcs)
        {
            int quota = 100;

            Timer timer = new Timer(1000);

            timer.Elapsed += (sender, e) => 
            {
                if (quota < 100)
                {
                    if ((quota + 5) > 100)
                    {
                        quota = 100;
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
                    if (pending.Count > 0 && quota > 0)
                    {
                        quota--;

                        current.Add(pending.Dequeue());
                    }
                }

                GetMatchingProductForIdRequest request = new GetMatchingProductForIdRequest();
                request.SellerId = this.MerchantId;
                request.MarketplaceId = this.MarketplaceId;
                request.IdList = new IdListType();
                request.IdList.Id = current;
                request.IdType = "UPC";

                GetMatchingProductForIdResponse response = GetMWSProductsClient().GetMatchingProductForId(request);

                if (response.IsSetGetMatchingProductForIdResult())
                {
                    foreach (GetMatchingProductForIdResult result in response.GetMatchingProductForIdResult)
                    {
                        results.Add(result);
                    }
                }
            }

            return results;
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            throw new NotImplementedException();
        }

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
