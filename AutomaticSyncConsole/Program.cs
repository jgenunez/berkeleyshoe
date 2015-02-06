using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using NLog;
using BerkeleyEntities;
using System.Threading.Tasks;
using System.Data;

namespace AutomaticSyncConsole
{
    class Program
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private static BerkeleyEntities.Amazon.AmazonServices _amznServices = new BerkeleyEntities.Amazon.AmazonServices();
        private static BerkeleyEntities.Ebay.EbayServices _ebayServices = new BerkeleyEntities.Ebay.EbayServices();


        public static void Main(string[] args)
        {

            try
            {
                if (args[0].Equals("1"))
                {
                    SyncEbayAndAmznOrders();
                }
                else if (args[0].Equals("2"))
                {
                    FixOverpublished();
                }
                else if (args[0].Equals("3"))
                {
                    SyncAmazonListings();
                }
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
            }

            _logger.Info("____________");
        }

        private static void SyncEbayAndAmznOrders()
        {
            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                foreach (AmznMarketplace marketplace in dataContext.AmznMarketplaces)
                {
                    _amznServices.SynchronizeOrders(marketplace.ID);

                    _logger.Info(marketplace.Name + " order synchronization completed");
                }

                foreach (EbayMarketplace marketplace in dataContext.EbayMarketplaces)
                {
                    _ebayServices.SynchronizeListings(marketplace.ID);

                    _logger.Info(marketplace.Name + " listing synchronization completed");

                    _ebayServices.SynchronizeOrders(marketplace.ID);

                    _logger.Info(marketplace.Name + " order synchronization completed");
                }
            }
        }

        private static void SyncAmazonListings()
        {
            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                foreach (AmznMarketplace marketplace in dataContext.AmznMarketplaces)
                {
                    _amznServices.SynchronizeListings(marketplace.ID);

                    _logger.Info(marketplace.Name + " listings synchronization completed");
                }
            }
        }

        private static void FixOverpublished()
        {
            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                foreach (AmznMarketplace marketplace in dataContext.AmznMarketplaces)
                {
                    _amznServices.FixOverpublished(marketplace.ID);

                    _logger.Info(marketplace.Name + " fixing overpublished completed");
                }

                foreach (EbayMarketplace marketplace in dataContext.EbayMarketplaces)
                {
                    _ebayServices.FixOverpublished(marketplace.ID);

                    _logger.Info(marketplace.Name + " fixing overpublished completed");
                }
            }
        }

    }
}
