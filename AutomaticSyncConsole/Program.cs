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

        public static void Main(string[] args)
        {
            try
            {
                if (DateTime.Now.Hour == 5 && DateTime.Now.Minute < 35)
                {
                    SyncMarketplaces(true);
                    FixOverpublished();
                }
                else
                {
                    SyncMarketplaces(false);
                    FixOverpublished();
                }
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
            }

            _logger.Info("____________");
        }

        private static void SyncMarketplaces(bool syncAmznListing)
        {
            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                foreach (AmznMarketplace marketplace in dataContext.AmznMarketplaces)
                {
                    AmazonServices.OrderSyncService orderService = new AmazonServices.OrderSyncService(marketplace.ID);

                    if (syncAmznListing)
                    {
                        AmazonServices.ListingSyncService listingService = new AmazonServices.ListingSyncService(marketplace.ID);

                        listingService.Synchronize();

                        _logger.Info(marketplace.Name + " listing synchronization completed");

                        orderService.MarginalSync();

                        _logger.Info(marketplace.Name + " order synchronization completed");
                    }
                    else
                    {
                        orderService.MarginalSync();

                        _logger.Info(marketplace.Name + " order synchronization completed");
                    }
                }

                foreach (EbayMarketplace marketplace in dataContext.EbayMarketplaces)
                {
                    EbayServices.Services.ListingSyncService listingService = new EbayServices.Services.ListingSyncService(marketplace.ID);
                    EbayServices.OrderSyncService orderService = new EbayServices.OrderSyncService(marketplace.ID);

                    listingService.MarginalSync();

                    _logger.Info(marketplace.Name + " listing synchronization completed");

                    orderService.MarginalSync();

                    _logger.Info(marketplace.Name + " order synchronization completed");
                }
            }
        }

        private static void FixOverpublished()
        {
            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                foreach (AmznMarketplace marketplace in dataContext.AmznMarketplaces)
                {
                    BerkeleyEntities.Amazon.Services.OverpublishedService service = new BerkeleyEntities.Amazon.Services.OverpublishedService(marketplace.ID);

                    service.BalanceQuantities();

                    _logger.Info(marketplace.Name + " fixing overpublished completed");
                }

                foreach (EbayMarketplace marketplace in dataContext.EbayMarketplaces)
                {
                    BerkeleyEntities.Ebay.Services.OverpublishedService service = new BerkeleyEntities.Ebay.Services.OverpublishedService(marketplace.ID);

                    service.BalanceQuantities();

                    _logger.Info(marketplace.Name + " fixing overpublished completed");
                }
            }
        }

    }
}
