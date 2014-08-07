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
    public partial class MainWindow : Window
    {
        private ExcelWorkbook _workbook;
        private berkeleyEntities _dataContext = new berkeleyEntities();
        private ObservableCollection<EbayPublisher> _ebayPublishers = new ObservableCollection<EbayPublisher>();
        private ObservableCollection<AmznPublisher> _amznPublisher = new ObservableCollection<AmznPublisher>();


        public MainWindow()
        {
            InitializeComponent();
            tcSheets.ItemsSource = new CompositeCollection() {
                new CollectionContainer() { Collection = _ebayPublishers} ,
                new CollectionContainer() { Collection = _amznPublisher }
            };

            
        }

        private void btnSetWorkbook_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Excel Files|*.xlsx";
            
            if ((bool)ofd.ShowDialog())
            {
                _workbook = new ExcelWorkbook(ofd.FileName);

                _workbook.ReadEntries();

                lbCurrentWorkbook.Content = ofd.FileName;

                foreach (string code in _workbook.EbayEntries.Keys)
                {
                    _ebayPublishers.Add(new EbayPublisher(_dataContext.EbayMarketplaces.Single(p => p.Code.Equals(code)).ID, _workbook.EbayEntries[code]));
                }

                foreach (string code in _workbook.AmznEntries.Keys)
                {
                    _amznPublisher.Add(new AmznPublisher(_dataContext.AmznMarketplaces.Single(p => p.Code.Equals(code)).ID, _workbook.AmznEntries[code]));
                }
            }
        }

        private void btnShowAvailableHeader_Click(object sender, RoutedEventArgs e)
        {
            var ebayProps = typeof(EbayEntry).GetProperties();



            var amznProps = typeof(AmznEntry).GetProperties();
        }


    }
}
