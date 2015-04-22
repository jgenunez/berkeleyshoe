using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BerkeleyEntities;
using System.IO;

namespace OperationsDashboard
{
    public partial class DashboardForm : Form
    {
        private berkeleyEntities _dataContext = new berkeleyEntities();
        private List<DirectoryInfo> _dirs = new List<DirectoryInfo>();

        public DashboardForm()
        {
            InitializeComponent();

            DirectoryInfo mainDir = new DirectoryInfo(@"P:\products\");

            _dirs.AddRange(mainDir.GetDirectories());
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            DateTime from = dtpFrom.Value.Date;
            DateTime to = dtpTo.Value.Date;

            List<WorkDay> workDays = new List<WorkDay>();

            using (_dataContext = new berkeleyEntities())
            {
                for (DateTime currentDate = from; currentDate <= to; currentDate += TimeSpan.FromDays(1))
                {
                    WorkDay workDay = new WorkDay();

                    workDay.Date = currentDate;

                    GetReceivedCounts(workDay);

                    GetPictureCounts(workDay);

                    GetShippingCounts(workDay);

                    GetReturnCounts(workDay);

                    workDays.Add(workDay);
                } 
            }

            dgvWorkByDay.DataSource = workDays;
        }

        private void GetReturnCounts(WorkDay workDay)
        {
            DateTime lowLimit = workDay.Date;
            DateTime highLimit = workDay.Date.AddDays(1);

            var transactions = _dataContext.Transactions.Where(p => p.Time >= lowLimit && p.Time <= highLimit);

            var returns = transactions.Where(p => p.Customer.LastName.Equals("INTERNET RETURNS")).SelectMany(p => p.TransactionEntries);

            if (returns.Count() > 0)
            {
                workDay.ItemReturned = returns.GroupBy(p => p.ItemID).Count();
                workDay.QtyReturned = returns.Sum(p => p.Quantity) * -1;
            }
        }

        private void GetShippingCounts(WorkDay workDay)
        {
            DateTime lowLimit = workDay.Date;
            DateTime highLimit = workDay.Date.AddDays(1);

            var transactions = _dataContext.Transactions.Where(p => p.Time >= lowLimit && p.Time <= highLimit);

            var onlineTransactions = transactions.Where(p => 
                p.Customer.LastName.Equals("ONE MILLION SHOES") || 
                p.Customer.LastName.Equals("SMALL AVENUE") || 
                p.Customer.LastName.Equals("SHOESTOGO24/7") ||
                p.Customer.LastName.Equals("AMAZON US")).SelectMany(p => p.TransactionEntries);


            var entryGroups = onlineTransactions.GroupBy(p => p.ItemID);

            if (onlineTransactions.Count() > 0)
            {
                workDay.ItemShipped = onlineTransactions.GroupBy(p => p.ItemID).Count();
                workDay.QtyShipped = onlineTransactions.Sum(p => p.Quantity); 
            }
        }

        private void GetPictureCounts(WorkDay workDay)
        {
            double pictureCount = 0;
            double productCount = 0;

            foreach (DirectoryInfo dirInfo in _dirs)
            {
                if (dirInfo.LastWriteTime > workDay.Date)
                {
                    var fileGroups = dirInfo.GetFiles().Where(p => p.LastWriteTime.Date.Equals(workDay.Date)).GroupBy(p => p.Name.Split(new Char[1] {'-'})[0].Replace(".jpg",""));

                    productCount += fileGroups.Count();
                    pictureCount += fileGroups.Sum(s => s.Count());
                }
            }

            workDay.QtyPhotographed = pictureCount;
            workDay.ModelPhotographed = productCount;
        }

        private void GetReceivedCounts(WorkDay workDay)
        {
            DateTime lowLimit = workDay.Date;
            DateTime highLimit = workDay.Date.AddDays(1);

            var entryGroups = _dataContext.PurchaseOrderEntries
                .Where(p => p.LastReceivedDate.HasValue && p.LastReceivedDate.Value >= lowLimit && p.LastReceivedDate.Value <= highLimit)
                .GroupBy(p => p.ItemID);

            if (entryGroups.Count() > 0)
            {
                workDay.ItemReceived = entryGroups.Count();
                workDay.QtyReceived = entryGroups.Sum(p => p.Sum(s => s.LastQuantityReceived));
            }
        }

        private void GetPublishedCounts(WorkDay workDay)
        {
            DateTime lowLimit = workDay.Date;
            DateTime highLimit = workDay.Date.AddDays(1);

            
        }

    }
}
