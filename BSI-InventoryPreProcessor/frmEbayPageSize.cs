//
// AMGD
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BSI_InventoryPreProcessor
{
    public partial class frmEbayPageSize : Form
    {
        int gPageSize = 200;

        public frmEbayPageSize()
        {
            InitializeComponent();
            gPageSize = Properties.Settings.Default.eBayPageSize;

            int li = cmbPageSize.Items.IndexOf("" + gPageSize);
            if (li >= 0) cmbPageSize.SelectedIndex = li;
        } // frmEbayPageSize

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        } // btnCancel_Click

        private void btnSave_Click(object sender, EventArgs e)
        {
            gPageSize = int.Parse(cmbPageSize.Text);
            Properties.Settings.Default.eBayPageSize = gPageSize;
            Properties.Settings.Default.Save();
            this.Close();
        } // btnSave_Click
    } // class frmEbayPageSize
} // BSI_InventoryPreProcessor
