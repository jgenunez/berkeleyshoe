using Microsoft.Win32;
using BerkeleyEntities;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;

namespace WorkbookPublisher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class WorkbookWindow : Window
    {   
        public WorkbookWindow()
        {
            InitializeComponent();
        }

        private void btnSetWorkbook_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.Filter = "Excel Files|*.xlsx";
            
            if ((bool)ofd.ShowDialog())
            {     
                this.DataContext = new ExcelWorkbook(ofd.FileName);
            }
        }

        private void btnShowAvailableHeader_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();

            var ebayProps = typeof(EbayEntry).GetProperties();

            sb.AppendLine("=========== Ebay valid columns =========== ");

            foreach (var prop in ebayProps)
            {
                if (!prop.Name.Equals("RowIndex"))
                {
                    sb.AppendFormat("-{0}\t", prop.Name);
                }
            }

            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine();

            sb.AppendLine("=========== Amazon valid columns =========== ");

            var amznProps = typeof(AmznEntry).GetProperties();

            foreach (var prop in amznProps)
            {
                if (!prop.Name.Equals("RowIndex"))
                {
                    sb.AppendFormat("-{0}\t", prop.Name);
                }
            }

            MessageBox.Show(sb.ToString());
        }


    }
}
