using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using NLog;
using BerkeleyEntities;
using System.Threading.Tasks;

namespace AutomaticSyncConsole
{
    class Program
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private static List<BackgroundWorker> _services = new List<BackgroundWorker>();


        public static void Main(string[] args)
        {
            if (DateTime.Now.Hour == 5 && DateTime.Now.Minute < 35)
            {
                var modified = SyncMarketplaces(true);

                FixOverpublished(modified);
            }
            else
            {
                var modified = SyncMarketplaces(false);

                FixOverpublished(modified);
            }


            _logger.Info("____________");
        }

        private static List<string> SyncMarketplaces(bool syncAmznListing)
        {
            var syncServices = new List<Task<List<string>>>();

            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                foreach (AmznMarketplace marketplace in dataContext.AmznMarketplaces)
                {
                    AmazonServices.OrderSyncService orderService = new AmazonServices.OrderSyncService(marketplace.ID);

                    if (syncAmznListing)
                    {
                        AmazonServices.ListingSyncService listingService = new AmazonServices.ListingSyncService(marketplace.ID);

                        Task<List<string>> task = new Task<List<string>>(() => listingService.Synchronize().Concat(orderService.MarginalSync()).ToList());
                        task.Start();
                        syncServices.Add(task);
                    }
                    else
                    {
                        Task<List<string>> task = new Task<List<string>>(() => orderService.MarginalSync());
                        task.Start();
                        syncServices.Add(task);
                    }
                }

                foreach (EbayMarketplace marketplace in dataContext.EbayMarketplaces)
                {
                    EbayServices.Services.ListingSyncService listingService = new EbayServices.Services.ListingSyncService(marketplace.ID);
                    EbayServices.OrderSyncService orderService = new EbayServices.OrderSyncService(marketplace.ID);

                    Task<List<string>> task = new Task<List<string>>(() => listingService.MarginalSync().Concat(orderService.MarginalSync()).ToList());
                    task.Start();
                    syncServices.Add(task);
                }
            }

            Task.WaitAll(syncServices.ToArray());

            return syncServices.SelectMany(p => p.Result).Distinct().ToList();
        }

        private static void FixOverpublished(List<string> modified)
        {
            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                foreach (AmznMarketplace marketplace in dataContext.AmznMarketplaces)
                {
                    try
                    {
                        BerkeleyEntities.Amazon.Services.OverpublishedService service = new BerkeleyEntities.Amazon.Services.OverpublishedService(marketplace.ID);

                        service.BalanceQuantities(modified);

                        _logger.Info(marketplace.Name + " overpublished service completed at " + DateTime.Now.ToString());
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e.Message);
                    }
                }

                foreach (EbayMarketplace marketplace in dataContext.EbayMarketplaces)
                {
                    try
                    {
                        BerkeleyEntities.Ebay.Services.OverpublishedService service = new BerkeleyEntities.Ebay.Services.OverpublishedService(marketplace.ID);

                        service.BalanceQuantities(modified);

                        _logger.Info(marketplace.Name + " overpublished service completed at " + DateTime.Now.ToString());
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e.Message);
                    }
                }
            }
        }

        private static bool IsOffHours()
        {
            return DateTime.Now.Hour >= 19 || DateTime.Now.Hour <= 6;
        }
    }
}
