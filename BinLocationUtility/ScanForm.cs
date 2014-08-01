using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Media;
using BerkeleyEntities;

namespace LocationApp
{
    public partial class ScanForm : Form
    {
        private string _scanBin = null;
        private string _user = null;

        private berkeleyEntities _dataContext = new berkeleyEntities();
        private BindingList<ProductStock> _scanProducts = new BindingList<ProductStock>();

        public ScanForm(string location, string user)
        {
            InitializeComponent();

            _scanBin = location.Trim().ToUpper();
            _user = user;

            this.Text = "Current Location : " + _scanBin;
            lbLocation.Text = _scanBin;
            productBindingSource.DataSource = _scanProducts;
            
        }


        private void tbSKU_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.btnAdd_Click(this, EventArgs.Empty);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            string productID = tbSKU.Text.ToUpper().Trim();

            this.ProcessScannedEntry(productID);

            lbCount.Text = "Total : " + _scanProducts.Sum(p => p.QtyScan).ToString();

            tbSKU.SelectAll();
        }

        private void btnDone_Click(object sender, EventArgs e)
        {

            DialogResult dr = MessageBox.Show("Are you sure you want to commit changes ?", "Confirm", MessageBoxButtons.YesNo);

            if (dr.Equals(DialogResult.No))
            {
                return;
            }


            foreach (ProductStock product in _scanProducts)
            {
                product.ApplyChanges();

                Bsi_LocationLog logEntry = new Bsi_LocationLog();
                logEntry.Item = product.Item;
                logEntry.User = _user;
                logEntry.BeforeChange = product.PreviousBin;
                logEntry.Location = "+ " + _scanBin;
                logEntry.UpdateDate = DateTime.Now;
                logEntry.Quantity = product.QtyScan;
            }

            _dataContext.SaveChanges();

            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("Are you sure you want to cancel ?", "Confirm", MessageBoxButtons.YesNo);

            if (dr.Equals(DialogResult.Yes))
            {
                this.Close();
            }

        }

        private void dgvScanList_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex == -1)
            {
                return;
            }

            DataGridViewRow currentRow = dgvScanList.Rows[e.RowIndex];

            ProductStock product = currentRow.DataBoundItem as ProductStock;

            if (product.QtyScan == product.OnHand)
            {
                currentRow.DefaultCellStyle.BackColor = Color.LightGreen;
            }
            else if (product.QtyScan < product.OnHand)
            {
                currentRow.DefaultCellStyle.BackColor = Color.LightYellow;
            }
            else
            {
                currentRow.DefaultCellStyle.BackColor = Color.Red;
            }
        }

        private void ProcessScannedEntry(string productID)
        {
            Item item = this.FindItem(productID);

            if (item == null)
            {
                this.ShowNoFoundMsg(productID);
            }
            else
            {
                List<string> locations = item.BinLocation.Split(new Char[1] { ' ' }).ToList();
                locations.Add(_scanBin);
                string updatedBin = String.Join(" ", locations.Distinct()).Trim();
                if (updatedBin.Length <= 20)
                {
                    if (!_scanProducts.Any(p => p.ID.Equals(item.ItemLookupCode)))
                    {
                        _scanProducts.Add(new ProductStock(item, _scanBin));
                    }
                    else
                    {
                        _scanProducts.Single(p => p.ID.Equals(item.ItemLookupCode)).QtyScan++;
                    }
                }
                else
                {
                    this.ShowFullBinMsg(item.BinLocation);
                }
            }
        }

        private Item FindItem(string productID)
        {
            Item item = _dataContext.Items.SingleOrDefault(p => p.ItemLookupCode.Equals(productID));

            if (item == null)
            {
                Alias alias = _dataContext.Aliases.SingleOrDefault(p => p.Alias1.Equals(productID));

                if (alias != null)
                {
                    item = alias.Item;
                }
            }

            return item;
        }

        private void ShowNoFoundMsg(string productID)
        {
            SystemSounds.Exclamation.Play();
            DialogResult dialogResult = DialogResult.Cancel;
            while (dialogResult.Equals(DialogResult.Cancel))
            {
                dialogResult = MessageBox.Show(
                    productID + " not found !",
                    "Invalid Input",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Stop,
                    MessageBoxDefaultButton.Button2);
            }
        }

        private void ShowFullBinMsg(string binLocation)
        {
            SystemSounds.Exclamation.Play();
            DialogResult dialogResult = DialogResult.Cancel;
            while (dialogResult.Equals(DialogResult.Cancel))
            {
                dialogResult = MessageBox.Show(
                    "Bin location character limit exceeded. Available locations : " + binLocation,
                    "Invalid Input",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Stop,
                    MessageBoxDefaultButton.Button2);
            }
        }

        

        




    }
}
