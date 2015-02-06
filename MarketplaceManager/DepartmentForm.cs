using BerkeleyEntities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MarketplaceManager
{
    public partial class DepartmentForm : Form
    {
        private string _selectedPath;
       
        public DepartmentForm()
        {
            InitializeComponent();

             List<DepartmentOption> options = new List<DepartmentOption>();

            using (berkeleyEntities dataContext = new berkeleyEntities())
            {


                foreach(Department dept in dataContext.Departments)
                {
                    options.Add(new DepartmentOption() { Code = dept.code, Name = dept.Name });
                }
            }

            ((ListBox)clbDepartments).DataSource = options;
            ((ListBox)clbDepartments).DisplayMember = "Name";
            ((ListBox)clbDepartments).ValueMember = "IsSelected";


        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.DefaultExt = ".xlsx";

            DialogResult dr = sfd.ShowDialog();

            if (!dr.Equals(DialogResult.Cancel) && !string.IsNullOrWhiteSpace(sfd.FileName))
            {
                _selectedPath = sfd.FileName;

                var codes = clbDepartments.CheckedItems.Cast<DepartmentOption>().Select(p => p.Code);

                BackgroundWorker bw = new BackgroundWorker();

                bw.DoWork += (_, args) =>
                {
                    ReportGenerator reportGenerator = new ReportGenerator(_selectedPath);
                    reportGenerator.GenerateExcelReport();
                };

                MarketplaceForm marketplaceForm = this.Tag as MarketplaceForm;

                bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(marketplaceForm.ReportCompleted);

                bw.RunWorkerAsync();
            }

            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }

    public class DepartmentOption
    {
        public string Name { get; set; }

        public string Code { get; set; }

        public bool IsSelected { get; set; }
    }



}
