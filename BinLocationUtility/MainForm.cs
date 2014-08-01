using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BerkeleyEntities;


namespace LocationApp
{
    public partial class MainForm : Form
    {

        private Form _scanForm = null;
        private string _userName = null;

        public MainForm(string userName)
        {
            InitializeComponent();
            _userName = userName.ToUpper().Trim();
            lbUser.Text = _userName;
        }


        private void LoadLog()
        {
            tvHistory.Nodes.Clear();

            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                var entries = dataContext.Bsi_LocationLog.Where(p => p.User.Equals(_userName) && p.UpdateDate.Day == DateTime.Now.Day);

                var entryGroups = entries.GroupBy(p => p.Location);

                foreach (var entryGroup in entryGroups)
                {
                    TreeNode parentNode = new TreeNode();
                    parentNode.Name = entryGroup.Key;
                    parentNode.Text = entryGroup.Key;


                    foreach (Bsi_LocationLog entry in entryGroup)
                    {
                        TreeNode node = new TreeNode();
                        node.Name = entry.Item.ItemLookupCode + "  (" + entry.Quantity.ToString() + ")  (" + entry.BeforeChange + ")";
                        node.Text = entry.Item.ItemLookupCode + "  (" + entry.Quantity.ToString() + ")  (" + entry.BeforeChange + ")";

                        parentNode.Nodes.Add(node);
                    }

                    tvHistory.Nodes.Add(parentNode);
                }
            }
        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            string currentLoc = tbCurrentLoc.Text;

            if (!String.IsNullOrWhiteSpace(currentLoc))
            {
                _scanForm = new ScanForm(currentLoc, _userName);
                _scanForm.ShowDialog();
            }
            else
            {
                MessageBox.Show("Not a valid location");
            }
            

        }

        private void btnCleanLoc_Click(object sender, EventArgs e)
        {
            new DeleteLocationForm(_userName).ShowDialog();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void btnUpdateLog_Click(object sender, EventArgs e)
        {
            this.LoadLog();
        }
    }
}
