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
    public partial class DeleteLocationForm : Form
    {
        private string _user = null;

        public DeleteLocationForm(string user)
        {
            InitializeComponent();
            _user = user;
        }

        private void btnAddLocation_Click(object sender, EventArgs e)
        {
            string location = tbLocation.Text.ToUpper().Trim();

            if (!lbDeleteList.Items.Contains(location))
            {
                if (!String.IsNullOrWhiteSpace(location))
                {
                    lbDeleteList.Items.Add(location);                
                }              
            }
            else
            {
                MessageBox.Show(location + " is already in delete list");
            }

            tbLocation.SelectAll();
        }

        private void btnAccept_Click(object sender, EventArgs e)
        {
            var locations = lbDeleteList.Items.Cast<string>();

            if (locations.Count() == 0)
            {
                MessageBox.Show("delete list is empty");
                return;
            }

            DialogResult dr = MessageBox.Show("Are you sure you want to clean all locations in delete list ?", "Confirm", MessageBoxButtons.OKCancel);

            if (dr.Equals(DialogResult.Cancel))
            {
                return;
            }

            int deleteCount = 0;

            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                foreach (string location in locations)
                {
                    var items  = dataContext.Items.Where(p => p.BinLocation.Contains(location));

                    foreach (Item item in items)
                    {
                        List<string> currentBins = item.BinLocation.Split(new Char[1] { ' ' }).ToList();

                        currentBins.ForEach(p => p = p.ToUpper().Trim());

                        if (currentBins.Any(p => p.Equals(location)))
                        {
                            Bsi_LocationLog logEntry = new Bsi_LocationLog();
                            logEntry.Item = item;
                            logEntry.Location = "- " + location;
                            logEntry.Quantity = 0;
                            logEntry.User = _user;
                            logEntry.UpdateDate = DateTime.Now;
                            logEntry.BeforeChange = item.BinLocation;

                            deleteCount += currentBins.RemoveAll(p => p.Equals(location));
                            item.BinLocation = String.Join(" ", currentBins).Trim();
                        }                    
                    }
                }
                dataContext.SaveChanges();
            }

            MessageBox.Show(deleteCount.ToString() + " locations deleted");


            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void tbLocation_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode.Equals(Keys.Enter))
            {
                this.btnAddLocation_Click(this, EventArgs.Empty);

            }
        }
    }
}
