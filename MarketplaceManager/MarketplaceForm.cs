using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using BerkeleyEntities;
using System.Timers;



namespace MarketplaceManager
{
    public partial class MarketplaceForm : Form
    {
        private MarketplaceViewRepository _marketplaceViewRepository = new MarketplaceViewRepository();
        private BindingList<MarketplaceView> _marketplaces = new BindingList<MarketplaceView>();

        private Dictionary<string, BackgroundWorker> _orderServices = new Dictionary<string, BackgroundWorker>();
        private Dictionary<string, BackgroundWorker> _listingServices = new Dictionary<string, BackgroundWorker>();
        private Dictionary<string, BackgroundWorker> _overPublishedServices = new Dictionary<string, BackgroundWorker>();

        


        public MarketplaceForm()
        {
            InitializeComponent();

            btnFixOverpublished.Click += btnFixOverpublished_Click;
        }

        private void SyncForm_Load(object sender, EventArgs e)
        {
            LoadMarketplaces();
        }

        private void btnSyncListings_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvMarketplaces.SelectedRows)
            {
                MarketplaceView view = row.DataBoundItem as MarketplaceView;

                if (_listingServices.ContainsKey(view.ID))
                {
                    MessageBox.Show(view.Name + " listing synchronization is running !");
                    continue;
                }

                BackgroundWorker bw = new BackgroundWorker();

                bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(ListingSyncServiceCompleted);

                if (view.Host.Equals("Amazon"))
                {
                    BerkeleyEntities.Amazon.AmazonServices services = new BerkeleyEntities.Amazon.AmazonServices();
                    
                    bw.DoWork += (_, args) => { services.SynchronizeListings(view.DbID); args.Result = view; };
                }
                else
                {
                    BerkeleyEntities.Ebay.EbayServices services = new BerkeleyEntities.Ebay.EbayServices();

                    bw.DoWork += (_, args) => { services.SynchronizeListings(view.DbID); args.Result = view; };
                }

                bw.RunWorkerAsync();

                _listingServices.Add(view.ID, bw);
            }
        }

        private void btnSyncOrders_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvMarketplaces.SelectedRows)
            {
                MarketplaceView view = row.DataBoundItem as MarketplaceView;

                if (_orderServices.ContainsKey(view.ID))
                {
                    MessageBox.Show(view.Name + " order synchronization is running !");
                    continue;
                }

                BackgroundWorker bw = new BackgroundWorker();
                bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(OrderSyncServiceCompleted);

                if (view.Host.Equals("Amazon"))
                {
                    BerkeleyEntities.Amazon.AmazonServices services = new BerkeleyEntities.Amazon.AmazonServices();
                    bw.DoWork += (_, args) => { services.SynchronizeOrders(view.DbID); args.Result = view; };    
                }
                else
                {
                    BerkeleyEntities.Ebay.EbayServices services = new BerkeleyEntities.Ebay.EbayServices();
                    bw.DoWork += (_, args) => { services.SynchronizeOrders(view.DbID); args.Result = view; };
                }

                bw.RunWorkerAsync();

                _orderServices.Add(view.ID, bw);
            }
        }

        private void btnFixOverpublished_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvMarketplaces.SelectedRows)
            {
                MarketplaceView view = row.DataBoundItem as MarketplaceView;

                if (_overPublishedServices.ContainsKey(view.ID))
                {
                    MessageBox.Show(view.Name + " order synchronization is running !");
                    continue;
                }

                BackgroundWorker bw = new BackgroundWorker();

                bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(OverpublishedServiceCompleted);

                if (view.Host.Equals("Amazon"))
                {
                    BerkeleyEntities.Amazon.AmazonServices services = new BerkeleyEntities.Amazon.AmazonServices();
                    bw.DoWork += (_, args) => { services.FixOverpublished(view.DbID); args.Result = view; };    
                }
                else
                {
                    BerkeleyEntities.Ebay.EbayServices services = new BerkeleyEntities.Ebay.EbayServices();
                    bw.DoWork += (_, args) => { services.FixOverpublished(view.DbID); args.Result = view; };
                }

                bw.RunWorkerAsync();

                _overPublishedServices.Add(view.ID, bw);
            }
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();

            sfd.DefaultExt = ".xlsx";

            DialogResult dr = sfd.ShowDialog();

            if (!dr.Equals(DialogResult.Cancel) && !string.IsNullOrWhiteSpace(sfd.FileName))
            {
                btnGenerate.Enabled = false;

                BackgroundWorker bw = new BackgroundWorker();

                bw.DoWork += (_, args) =>
                {
                    ReportGenerator reportGenerator = new ReportGenerator(sfd.FileName);
                    reportGenerator.GenerateExcelReport();
                };

                MarketplaceForm marketplaceForm = this.Tag as MarketplaceForm;

                bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(ReportCompleted);

                bw.RunWorkerAsync();
            }
        }

        public void ReportCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                MessageBox.Show("Report generated successfully!");
            }
            else
            {
                MessageBox.Show("Error when generating report: " + e.Error.Message);
            }

            btnGenerate.Enabled = true;
            
        }

        private void ListingSyncServiceCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                MarketplaceView view = e.Result as MarketplaceView;
                
                MessageBox.Show(view.Name + " listing synchronization completed !");
            }
            else
            {
                MessageBox.Show(e.Error.Message);
            }

            var removeList = _listingServices.Where(p => p.Value.IsBusy == false).Select(p => p.Key).ToList();

            foreach (string id in removeList)
            {
                _marketplaceViewRepository.Refresh(_marketplaces.Single(p => p.ID.Equals(id)));
                _listingServices.Remove(id);
            }

            dgvMarketplaces.Invalidate();
        }

        private void OrderSyncServiceCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                MarketplaceView view = e.Result as MarketplaceView;
                
                MessageBox.Show(view.Name + " order synchronization completed !");
            }
            else
            {
                MessageBox.Show(e.Error.Message);
            }

            var removeList = _orderServices.Where(p => p.Value.IsBusy == false).Select(p => p.Key).ToList();

            foreach (string id in removeList)
            {
                _marketplaceViewRepository.Refresh(_marketplaces.Single(p => p.ID.Equals(id)));
                _orderServices.Remove(id);
            }

            dgvMarketplaces.Invalidate();
        }

        private void OverpublishedServiceCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                MarketplaceView view = e.Result as MarketplaceView;
                MessageBox.Show(view.Name + " overpublished service completed !");
            }
            else
            {
                MessageBox.Show(e.Error.Message);
            }

            var removeList = _overPublishedServices.Where(p => p.Value.IsBusy == false).Select(p => p.Key).ToList();

            foreach (string key in removeList)
            {
                _overPublishedServices.Remove(key);
            }
        }

        private void LoadMarketplaces()
        {
            _marketplaces.Clear();

            foreach (MarketplaceView view in _marketplaceViewRepository.GetAllMarketplaceViews())
            {
                _marketplaces.Add(view);
            }

            marketplaceViewBindingSource.DataSource = _marketplaces;

            dgvMarketplaces.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);
        }

        private void btnCheckItem_Click(object sender, EventArgs e)
        {
            string sku = tbCheckItem.Text.ToUpper().Trim();

            try
            {
                CheckItem(sku);
            }
            catch (InvalidOperationException ex)
            {
                if (ex.TargetSite.Name.Equals("Single"))
                {
                    MessageBox.Show(sku + " not found !");
                }
            }
            
        }

        private void btnSummary_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvMarketplaces.SelectedRows)
            {
                MarketplaceView view = row.DataBoundItem as MarketplaceView;

                _marketplaceViewRepository.GetAdditionalData(view);


                StringBuilder strBuilder = new StringBuilder();

                strBuilder.AppendLine(view.Name);
                strBuilder.AppendLine();
                strBuilder.AppendFormat("{0} Acive Listings ({1}qty)", view.ActiveListing, view.ActiveListingQty);
                strBuilder.AppendLine();
                strBuilder.AppendLine();
                strBuilder.AppendFormat("{0} waiting for payment ({1}qty)", view.WaitingPayment.Count, view.WaitingPaymentQty);
                strBuilder.AppendLine();
                strBuilder.AppendLine(string.Join(" | ",view.WaitingPayment));
                strBuilder.AppendLine();
                strBuilder.AppendFormat("{0} waiting for shipment ({1}qty)",view.WaitingShipment.Count, view.WaitingShipmentQty);
                strBuilder.AppendLine();
                strBuilder.AppendLine(string.Join(" | ",view.WaitingShipment));

                MessageBox.Show(strBuilder.ToString(), "Marketplace Summary", MessageBoxButtons.OK);
            }



            //StringBuilder strBuilder = new StringBuilder();

            //var views = _marketplaceViewRepository.GetAllExtendedMarketplaceViews().ToList();

            //foreach (MarketplaceView view in views)
            //{
            //    strBuilder.AppendLine(view.Name + ":  " + view.Published + "% Listed | " + view.WaitingShipmentCount + " Unshipped | " + view.WaitingPaymentCount + " Unpaid");
            //    strBuilder.AppendLine();
            //}

            //strBuilder.AppendLine("TOTALS: " + views.Sum(p => p.Published) + "% Listed | " + views.Sum(p => p.WaitingShipment) + " Unshipped | " + views.Sum(p => p.WaitingPayment) + " Unpaid");

            //MessageBox.Show(strBuilder.ToString(), "Marketplace Summary", MessageBoxButtons.OK);
        }

        private void CheckItem(string sku)
        {
            StringBuilder strBuilder = new StringBuilder();

            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                Item item = dataContext.Items.Single(p => p.ItemLookupCode.Equals(sku));

                strBuilder.AppendLine(item.ItemLookupCode);
                strBuilder.AppendLine("OH: " + item.Quantity);
                strBuilder.AppendLine();

                var statusGroups = item.EbayListingItems.GroupBy(p => p.Listing.Status);

                strBuilder.AppendLine("EBAY");
                strBuilder.AppendLine("----------------------");

                if (statusGroups.Any(p => p.Key.Equals("Active")))
                {
                    foreach (EbayListingItem listingItem in statusGroups.Single(p => p.Key.Equals("Active")))
                    {
                        strBuilder.AppendLine(
                            listingItem.Listing.Marketplace.Code + " | " +
                            listingItem.Listing.Code + " | " +
                            listingItem.Quantity + " | " + listingItem.Price.ToString("C2"));


                        var waitingShipment = listingItem.OrderItems.Where(p => p.Order.IsWaitingForShipment());

                        if (waitingShipment.Count() > 0)
                        {
                            string orders = string.Join(" | ", waitingShipment.Select(p => p.Order.SalesRecordNumber + "(" + p.ListingItem.Quantity + ")"));
                            strBuilder.AppendLine("  " + " awaiting shipment -> " + orders);
                        }

                        var waitingPayment = listingItem.OrderItems.Where(p => p.Order.IsWaitingForPayment());

                        if (waitingPayment.Count() > 0)
                        {
                            string orders = string.Join("-", waitingPayment.Select(p => p.Order.SalesRecordNumber + "(" + p.ListingItem.Quantity) + ")");
                            strBuilder.AppendLine("  " + " awaiting payment -> " + orders);
                        }
                    }
                }
                else
                {
                    strBuilder.AppendLine("NONE");
                }

                strBuilder.AppendLine();
                strBuilder.AppendLine("History:");

                if (statusGroups.Any(p => p.Key.Equals("Completed")))
                {
                    foreach (EbayListingItem listingItem in statusGroups.Single(p => p.Key.Equals("Completed")))
                    {
                        strBuilder.AppendLine(
                            listingItem.Listing.Marketplace.Code + " | " +
                            listingItem.Listing.Code + " | " +
                            listingItem.Quantity + " | " + listingItem.Price.ToString("C2"));
                    }
                }
                else
                {
                    strBuilder.AppendLine("NONE");
                }

                strBuilder.AppendLine();

                strBuilder.AppendLine("AMAZON");

                strBuilder.AppendLine("----------------------");

                var listingItems = item.AmznListingItems.Where(p => p.IsActive);

                if (listingItems.Count() > 0)
                {
                    foreach (AmznListingItem listingItem in listingItems)
                    {
                        strBuilder.AppendLine(
                            listingItem.Marketplace.Code + " | " +
                            listingItem.ASIN + " | " +
                            listingItem.Quantity + " | " + listingItem.Price.ToString("C2"));


                        var waitingShipment = listingItem.OrderItems.Where(p => p.Order.Status.Equals("Unshipped") || p.Order.Status.Equals("PartiallyShipped"));

                        if (waitingShipment.Count() > 0)
                        {
                            string orders = string.Join(" | ", waitingShipment.Select(p => p.Code + "(" + (p.QuantityOrdered - p.QuantityShipped) + ")"));
                            strBuilder.AppendLine("  " + " awaiting shipment -> " + orders);
                        }

                        var waitingPayment = listingItem.OrderItems.Where(p => p.Order.Status.Equals("Pending"));
                        if (waitingPayment.Count() > 0)
                        {
                            string orders = string.Join(" | ", waitingPayment.Select(p => p.Code + "(" + (p.QuantityOrdered - p.QuantityShipped) + ")"));
                            strBuilder.AppendLine("  " + " awaiting payment -> " + orders);
                        }
                    }
                }
                else
                {
                    strBuilder.AppendLine("NONE");
                }

            }

            MessageBox.Show(strBuilder.ToString());
        }

            










        

    }
}
