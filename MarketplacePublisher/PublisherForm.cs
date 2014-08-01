using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BerkeleyEntities;

namespace MarketplacePublisher
{
    public partial class PublisherForm : Form
    {
        private EntrySync _entrySynchronizer = new EntrySync();
       
        private EbayPublisher _ebayPublisher;
        private AmznPublisher _amznPublisher;
        private ExcelSheet _currentSheet;

        public PublisherForm()
        {
            InitializeComponent();
        }

        private void btnSetExcelSheet_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Excel Files|*.xlsx";
            DialogResult dr = ofd.ShowDialog();


            if (dr.Equals(DialogResult.OK) && !string.IsNullOrWhiteSpace(ofd.FileName))
            {
                _currentSheet = new ExcelSheet(ofd.FileName);
                lbCurrentSheet.Text = ofd.FileName;
            }
        }

        private void btnUpdateSheet_Click(object sender, EventArgs e)
        {
            if (_currentSheet == null)
            {
                MessageBox.Show("No sheet selected !");
                return;
            }

            _currentSheet.UpdateSheet();

            MessageBox.Show("Worksheet updated !");
        }

        private void btnPublish_Click(object sender, EventArgs e)
        {
            if (_currentSheet == null)
            {
                MessageBox.Show("No sheet selected !");
                return;
            }

            var entries = _currentSheet.ReadEntries().Where(p => p.Status != null).Where(p => p.Status.Equals("read"));

            var hostGroups = entries.GroupBy(p => p.GetMarketplaceName());

            foreach (var hostGroup in hostGroups)
            {
                if (hostGroup.Key.Equals("Ebay"))
                {
                    
                }
                else
                {
                    _ebayPublisher = new EbayPublisher(hostGroup.ToList());
                    _ebayPublisher.Publish();
                }
            }

            MessageBox.Show("Publishing completed !");
        }

        private void btnUpdateAmznPrice_Click(object sender, EventArgs e)
        {
            
        }

    }
}
