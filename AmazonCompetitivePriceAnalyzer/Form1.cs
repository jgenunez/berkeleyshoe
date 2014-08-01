using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AmznPriceComparator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnUpdateSheet_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Excel Files|*.xlsx";
            DialogResult dr = ofd.ShowDialog();


            if (dr.Equals(DialogResult.OK) && !string.IsNullOrWhiteSpace(ofd.FileName))
            {
                ExcelSheet _currentSheet = new ExcelSheet(ofd.FileName);

                _currentSheet.UpdateSheet();
            }
        }
    }
}
