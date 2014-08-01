using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using BerkeleyEntities;
using NLog;
using System.Timers;

namespace SynchronizationService
{
    public partial class AutomaticSyncService : ServiceBase
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private List<BackgroundWorker> _services = new List<BackgroundWorker>();
        private static Timer _timer;


        public AutomaticSyncService()
        {
            InitializeComponent();

            this.ServiceName = "Marketplace Synchronization Service";

            this.EventLog.Log = "Application";

            // These Flags set whether or not to handle that specific
            //  type of event. Set to true if you need it, false otherwise.
            this.CanStop = true;

        }

        private void StartMarketplaceSync()
        {
            _services.Clear();

            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                foreach (AmznMarketplace marketplace in dataContext.AmznMarketplaces)
                {
                    AmazonServices.OrderSyncService orderService = new AmazonServices.OrderSyncService(marketplace.ID);

                    BackgroundWorker bw = new BackgroundWorker();

                    bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_ServiceCompleted);

                    if (IsOffHours() && DateTime.UtcNow.Hour == 19 && (!marketplace.ListingSyncTime.HasValue || marketplace.ListingSyncTime.Value < DateTime.UtcNow.AddHours(-2)))
                    {
                        AmazonServices.ListingSyncService listingService = new AmazonServices.ListingSyncService(marketplace.ID);

                        string result = marketplace.Name + " listings & orders synchronization";

                        bw.DoWork += (_, args) => { args.Result = result; listingService.Synchronize(); orderService.MarginalSync(); };

                        bw.RunWorkerAsync();

                        _services.Add(bw);
                    }
                    else
                    {
                        string result = marketplace.Name + " orders synchronization";

                        bw.DoWork += (_, args) => { args.Result = result; orderService.MarginalSync(); };

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

                    bw.DoWork += (_, args) => { args.Result = result; listingService.MarginalSync(); orderService.MarginalSync(); };

                    bw.RunWorkerAsync();

                    _services.Add(bw);
                } 
            }
        }

        private void bw_ServiceCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                _logger.ErrorException(e.Result.ToString(), e.Error);
            }
            else
            {
                _logger.Info(e.Result.ToString() + " completed at " + DateTime.Now.ToString());
            }
        }

        private void FixOverpublished()
        {
            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                foreach (AmznMarketplace marketplace in dataContext.AmznMarketplaces)
                {
                    AmazonServices.OverpublishedService service = new AmazonServices.OverpublishedService(marketplace.ID);

                    try
                    {
                        service.BalanceQuantities();
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
                    EbayServices.OverpublishedService service = new EbayServices.OverpublishedService(marketplace.ID);

                    try
                    {
                        service.BalanceQuantities();
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

        private bool IsOffHours()
        {
            return DateTime.Now.Hour >= 19 || DateTime.Now.Hour <= 6;
        }

        protected override void OnStart(string[] args)
        {
            base.OnStart(args);

            _timer = new Timer(1200000);
            _timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            _timer.AutoReset = false;
            _timer.Start();
        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            StartMarketplaceSync();

            while (_services.Any(p => p.IsBusy))
            {
                System.Threading.Thread.Sleep(30000);
            }

            if (IsOffHours())
            {
                FixOverpublished();
            }

            _timer.Start();
        }

        protected override void OnStop()
        {
            base.OnStop();

            _timer.Stop();
            _timer.Dispose();
        }

        //protected override void OnPause()
        //{
        //    base.OnPause();

        //    _syncTimer.Stop();
        //}

        //protected override void OnContinue()
        //{
        //    base.OnContinue();

        //    _syncTimer.Start();
        //}

        //protected override void OnShutdown()
        //{
        //    base.OnShutdown();
        //}

        //protected override void OnCustomCommand(int command)
        //{
        //    //  A custom command can be sent to a service by using this method:
        //    //#  int command = 128; //Some Arbitrary number between 128 & 256
        //    //#  ServiceController sc = new ServiceController("NameOfService");
        //    //#  sc.ExecuteCommand(command);

        //    base.OnCustomCommand(command);
        //}

        //protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        //{
        //    return base.OnPowerEvent(powerStatus);
        //}

        //protected override void OnSessionChange(
        //          SessionChangeDescription changeDescription)
        //{
        //    base.OnSessionChange(changeDescription);
        //}

    }
}
