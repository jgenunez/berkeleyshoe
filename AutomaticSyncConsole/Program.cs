using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using NLog;
using BerkeleyEntities;

namespace AutomaticSyncConsole
{
    class Program
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private static List<BackgroundWorker> _services = new List<BackgroundWorker>();


        public static void Main(string[] args)
        {
            string fileName = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString().PadLeft(2, '0') + DateTime.Now.Day.ToString().PadLeft(2, '0') + ".xlsx";

            string path = @"P:\Unpublished Reports\" + fileName;

            ReportGenerator reportGenerator = new ReportGenerator(path);
            reportGenerator.GenerateExcelReport();

            if (DateTime.Now.Hour == 5 && DateTime.Now.Minute < 35)
            {
                StartMarketplaceSync(true);

                while (_services.Any(p => p.IsBusy))
                {
                    System.Threading.Thread.Sleep(30000);
                }

                FixOverpublished();

                //string fileName = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString().PadLeft(2,'0') + DateTime.Now.Day.ToString().PadLeft(2,'0') + ".xlsx";

                //string path = @"P:\Unpublished Reports\" + fileName;

                //ReportGenerator reportGenerator = new ReportGenerator(path);
                //reportGenerator.GenerateExcelReport();
            }
            else
            {
                StartMarketplaceSync(false);

                while (_services.Any(p => p.IsBusy))
                {
                    System.Threading.Thread.Sleep(30000);
                }

                FixOverpublished();
            }


            _logger.Info("____________");
        }

        private static void StartMarketplaceSync(bool syncAmznListing)
        {
            _services.Clear();

            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                foreach (AmznMarketplace marketplace in dataContext.AmznMarketplaces)
                {
                    AmazonServices.OrderSyncService orderService = new AmazonServices.OrderSyncService(marketplace.ID);

                    BackgroundWorker bw = new BackgroundWorker();

                    bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_ServiceCompleted);

                    if (syncAmznListing)
                    {
                        AmazonServices.ListingSyncService listingService = new AmazonServices.ListingSyncService(marketplace.ID);

                        string result = marketplace.Name + " listings & orders synchronization";

                        bw.DoWork += (_, p) => { p.Result = result; listingService.Synchronize(); orderService.MarginalSync(); };

                        bw.RunWorkerAsync();

                        _services.Add(bw);
                    }
                    else
                    {
                        string result = marketplace.Name + " orders synchronization";

                        bw.DoWork += (_, p) => { p.Result = result; orderService.MarginalSync(); };

                        bw.RunWorkerAsync();

                        _services.Add(bw);
                    }
                }

                foreach (EbayMarketplace marketplace in dataContext.EbayMarketplaces)
                {
                    BackgroundWorker bw = new BackgroundWorker();

                    bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_ServiceCompleted);

                    EbayServices.Services.ListingSyncService listingService = new EbayServices.Services.ListingSyncService(marketplace.ID);

                    EbayServices.OrderSyncService orderService = new EbayServices.OrderSyncService(marketplace.ID);

                    string result = marketplace.Name + " listings & orders synchronization";

                    bw.DoWork += (_, p) => { p.Result = result; listingService.MarginalSync(); orderService.MarginalSync(); };

                    bw.RunWorkerAsync();

                    _services.Add(bw);
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

                    try
                    {
                        service.BalanceQuantities();

                        _logger.Info(marketplace.Name + " overpublished service completed at " + DateTime.Now.ToString());
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e.Message);

                        System.Threading.Thread.Sleep(30000);

                        service.BalanceQuantities();
                    }
                }

                foreach (EbayMarketplace marketplace in dataContext.EbayMarketplaces)
                {
                    BerkeleyEntities.Ebay.Services.OverpublishedService service = new BerkeleyEntities.Ebay.Services.OverpublishedService(marketplace.ID);

                    try
                    {
                        service.BalanceQuantities();

                        _logger.Info(marketplace.Name + " overpublished service completed at " + DateTime.Now.ToString());
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e.Message);

                        System.Threading.Thread.Sleep(30000);

                        service.BalanceQuantities();
                    }
                }
            }
        }

        private static void bw_ServiceCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _logger.Info(e.Result.ToString() + " completed at " + DateTime.Now.ToString());
        }

        private static bool IsOffHours()
        {
            return DateTime.Now.Hour >= 19 || DateTime.Now.Hour <= 6;
        }
    }
}
