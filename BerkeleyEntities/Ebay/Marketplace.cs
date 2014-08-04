using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using eBay.Service.Core.Sdk;
using eBay.Service.Core.Soap;
using eBay.Service.Util;
using System.IO;
using System.Xml.Serialization;

namespace BerkeleyEntities
{
    public partial class EbayMarketplace
    {
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

        public string GetTemplate()
        {
            if (string.IsNullOrEmpty(_template))
            {
                string path = this.RootDir +  @"\" + this.Code + @"\Template.html";

                StreamReader reader = new StreamReader(File.OpenRead(path));

                _template = reader.ReadToEnd();
            }

            return _template;
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
            string path = this.RootDir + @"/" + this.Code + @"/waitingListings.xml";

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
