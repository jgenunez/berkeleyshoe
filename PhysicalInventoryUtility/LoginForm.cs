using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BerkeleyEntities;
using System.Threading;

namespace InventoryApp
{
    public partial class LoginForm : Form
    {

        public LoginForm()
        {
            InitializeComponent();
        }

        public string UserName { get; set; }

        public string InventoryRef { get; set; }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (cbPhysicalInventory.SelectedItem == null)
            {
                MessageBox.Show("Select physical inventory reference before login");
                return;
            }

            if (string.IsNullOrWhiteSpace(tbUser.Text))
            {
                MessageBox.Show("Enter a user name before login");
                return;
            }

            this.UserName = tbUser.Text.ToUpper().Trim();
            this.InventoryRef = cbPhysicalInventory.SelectedItem as string;

            this.DialogResult = DialogResult.OK;

            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;

            this.Close();
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                cbPhysicalInventory.DataSource = dataContext.PhysicalInventories.Where(p => p.CloseTime == null).Select(p => p.Code);
            }     
        }

        
    }
}
