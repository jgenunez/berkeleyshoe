using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WorkbookPublisher
{
    /// <summary>
    /// Interaction logic for WorkbookWindow.xaml
    /// </summary>
    public partial class WorkbookWindow : Window
    {

        public WorkbookWindow()
        {
            InitializeComponent();
        }

        private void btnShowAvailableHeader_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSetWorkbook_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = false;
            ofd.ValidateNames = true;
            ofd.DereferenceLinks = false; // Will return .lnk in shortcuts.
            ofd.Filter = "Excel |*.xlsx";

            if ((bool)ofd.ShowDialog())
            {
                this.DataContext =  new ExcelWorkbook(ofd.FileName);
                
               
            }
        }
    }
}
