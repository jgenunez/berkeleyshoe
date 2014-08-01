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
using System.Data.Objects;
using System.Text.RegularExpressions;


namespace InventoryApp
{
    public partial class ScanForm : Form
    {
        private BindingList<ProductCount> _productCounts = new BindingList<ProductCount>();
        private ProductCountRepository _productCountRepository;
        
        public ScanForm(string inventoryRef, string bin, string userName)
        {
            InitializeComponent();

            _productCountRepository = new ProductCountRepository(inventoryRef, bin, userName);

            dgvProducts.DataSource = _productCounts;

            dgvProducts.Columns["ExpectedBIN"].Visible = false;

            dgvProducts.Columns["Bin"].HeaderText = "Bin";
            dgvProducts.Columns["Bin"].ReadOnly = true;
            dgvProducts.Columns["Bin"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dgvProducts.Columns["ExpectedBin"].HeaderText = "Expected Bin";
            dgvProducts.Columns["ExpectedBin"].Visible = true;
            dgvProducts.Columns["ExpectedBin"].ReadOnly = true;
            dgvProducts.Columns["ExpectedBin"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dgvProducts.Columns["Counted"].HeaderText = "Count";
            dgvProducts.Columns["Counted"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dgvProducts.Columns["Brand"].ReadOnly = true;
            dgvProducts.Columns["Sku"].ReadOnly = true;          
            dgvProducts.Columns["Department"].ReadOnly = true;
            dgvProducts.Columns["Department"].Visible = false;
            dgvProducts.Columns["Expected"].ReadOnly = true;
            
        }

        private void tbSKU_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) 
            { 
                btnAdd_Click(tbSKU, EventArgs.Empty); 
            }       
        }

        private void dgvProducts_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            UpdateBinTotalQty();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!Regex.IsMatch(tbQuantity.Text, "[0-9]"))
            {
                MessageBox.Show("Qty field must be a numeric value. Try Again !");
                return;
            }

            int count = int.Parse(tbQuantity.Text);

            string code = tbSKU.Text.ToUpper().Trim();

            if (string.IsNullOrWhiteSpace(code))
            {
                MessageBox.Show("Invalid product code. Try again !");

                return;
            }

            ProductCount productCount = _productCounts.SingleOrDefault(p => p.Sku.Equals(code) || p.Aliases.Any(s => s.Equals(code)));

            if (productCount == null)
            {
                try
                {
                    productCount = _productCountRepository.GetProductCount(code);
                    _productCounts.Add(productCount);
                }
                catch (Exception)
                {
                    ShowNotFoundMessage(code);
                    return;
                }
            }

            productCount.Counted = productCount.Counted + count;

            dgvProducts.CurrentCell = dgvProducts.Rows.Cast<DataGridViewRow>().SingleOrDefault(r => r.DataBoundItem.Equals(productCount)).Cells[1];

            UpdateBinTotalQty();

            tbSKU.SelectAll();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to commit changes?", "Confirm", MessageBoxButtons.YesNo);

            if (result.Equals(DialogResult.Yes))
            {
                _productCountRepository.SaveChanges();

                Close();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to cancel changes?", "Confirm", MessageBoxButtons.YesNo);

            if (result.Equals(DialogResult.Yes))
            {
                _productCountRepository.DiscardChanges();

                Close();
            }
        }

        private void ScanForm_Load(object sender, EventArgs e)
        {
            foreach (ProductCount productCount in _productCountRepository.GetExistingCounts())
            {
                _productCounts.Add(productCount);
            }

            UpdateBinTotalQty();

            dgvProducts.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);
        }

        private void dgvProducts_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            DataGridViewRow row = dgvProducts.Rows[e.RowIndex];

            ProductCount productCount = row.DataBoundItem as ProductCount;

            if (productCount.HasChanges())
            {
                dgvProducts.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightYellow;
            }
            else
            {
                dgvProducts.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.White;
            }
        }

        private void dgvProducts_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            //get the current column details
            string strColumnName = dgvProducts.Columns[e.ColumnIndex].Name;
            SortOrder strSortOrder = getSortOrder(e.ColumnIndex);

            IOrderedEnumerable<ProductCount> sortedProducts = null;

            switch (strColumnName)
            {
                case "Sku":
                    if (strSortOrder.Equals(SortOrder.Ascending))
                        sortedProducts = _productCounts.OrderBy(p => p.Sku);
                    else
                        sortedProducts = _productCounts.OrderByDescending(p => p.Sku);
                    break;
                case "Brand":
                    if (strSortOrder.Equals(SortOrder.Ascending))
                        sortedProducts = _productCounts.OrderBy(p => p.Brand);
                    else
                        sortedProducts = _productCounts.OrderByDescending(p => p.Brand);
                    break;
                case "Department":
                    if (strSortOrder.Equals(SortOrder.Ascending))
                        sortedProducts = _productCounts.OrderBy(p => p.Department);
                    else
                        sortedProducts = _productCounts.OrderByDescending(p => p.Department);
                    break;
                case "Counted":
                    if (strSortOrder.Equals(SortOrder.Ascending))
                        sortedProducts = _productCounts.OrderBy(p => p.Counted);
                    else
                        sortedProducts = _productCounts.OrderByDescending(p => p.Counted);
                    break;
                case "Expected":
                    if (strSortOrder.Equals(SortOrder.Ascending))
                        sortedProducts = _productCounts.OrderBy(p => p.Expected);
                    else
                        sortedProducts = _productCounts.OrderByDescending(p => p.Expected);
                    break;
                case "Bin":
                    if (strSortOrder.Equals(SortOrder.Ascending))
                        sortedProducts = _productCounts.OrderBy(p => p.Bin);
                    else
                        sortedProducts = _productCounts.OrderByDescending(p => p.Bin);
                    break;
                case "ExpectedBin":
                    if (strSortOrder.Equals(SortOrder.Ascending))
                        sortedProducts = _productCounts.OrderBy(p => p.ExpectedBin);
                    else
                        sortedProducts = _productCounts.OrderByDescending(p => p.ExpectedBin);
                    break;
            }

            dgvProducts.DataSource = new BindingList<ProductCount>(sortedProducts.ToList());

            dgvProducts.Columns[e.ColumnIndex].HeaderCell.SortGlyphDirection = strSortOrder;
        }

        private void ShowNotFoundMessage(string productID)
        {
            SystemSounds.Exclamation.Play();

            DialogResult dialogResult = DialogResult.Cancel;

            while (dialogResult.Equals(DialogResult.Cancel))
            {
                dialogResult = MessageBox.Show(productID + " Not Found !", "Entry ", MessageBoxButtons.OKCancel, MessageBoxIcon.Stop, MessageBoxDefaultButton.Button2);
            }
        }

        private void UpdateBinTotalQty()
        {
            lbBinQty.Text = _productCountRepository.Bin + " Count : " + _productCounts.Sum(p => p.Counted).ToString();

            dgvProducts.Invalidate();
        }

        private SortOrder getSortOrder(int columnIndex)
        {
            if (dgvProducts.Columns[columnIndex].HeaderCell.SortGlyphDirection == SortOrder.None ||
                dgvProducts.Columns[columnIndex].HeaderCell.SortGlyphDirection == SortOrder.Descending)
            {
                dgvProducts.Columns[columnIndex].HeaderCell.SortGlyphDirection = SortOrder.Ascending;
                return SortOrder.Ascending;
            }
            else
            {
                dgvProducts.Columns[columnIndex].HeaderCell.SortGlyphDirection = SortOrder.Descending;
                return SortOrder.Descending;
            }
        }

       


    }
}
