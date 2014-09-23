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
using BerkeleyEntities;
using WorkbookPublisher.ViewModel;

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
                lbCurrentWorkbook.Content = ofd.FileName;

                using (berkeleyEntities dataContext = new berkeleyEntities())
                {
                    ExcelWorkbook workbook = new ExcelWorkbook(ofd.FileName);

                    CompositeCollection composite = new CompositeCollection();

                    foreach (var marketplace in dataContext.EbayMarketplaces)
                    {
                        composite.Add(new EbayPublisherViewModel(workbook, marketplace.Code));
                    }

                    foreach (var marketplace in dataContext.AmznMarketplaces)
                    {
                        composite.Add(new AmznPublisherViewModel(workbook, marketplace.Code));
                    }

                    tcSheets.ItemsSource = composite;
                }
                
            }
        }

        private void Test1()
        {
            using (BerkeleyEntities.berkeleyEntities dataContext = new BerkeleyEntities.berkeleyEntities())
            {
                //string result = string.Join("\n", dataContext.Items.Where(p => p.SubDescription1.Equals("NIKE") && !p.Inactive && p.Quantity > 0).ToList().GroupBy(p => p.ClassName).Where(p => p.Sum(s => s.Quantity) < 3).SelectMany(p => p).Select(p => p.SubDescription1 + "," + p.ItemLookupCode + "," + p.Quantity));
                //System.IO.File.WriteAllText(@"c:\Users\Juan\Desktop\testting.txt", result);

                dataContext.MaterializeAttributes = true;

                BerkeleyEntities.Amazon.Publisher publisher = new BerkeleyEntities.Amazon.Publisher(dataContext, dataContext.AmznMarketplaces.First());

                AmazonServices.Mappers.ListingMapper mapper = new AmazonServices.Mappers.ListingMapper(dataContext, dataContext.AmznMarketplaces.First());

                var envelope = mapper.BuildProductData(dataContext.AmznListingItems.Where(p => p.Item.Department != null).Take(3));


                foreach (var test in envelope.Message.ToList())
                {
                    test.ProcessingResult = new BerkeleyEntities.Amazon.ProcessingReportResult();
                    test.ProcessingResult.ResultDescription = "ERRRORRRROROROROROORORORO";
                }


                WorkbookPublisher.View.RepublishDataWindow form = new View.RepublishDataWindow();

                form.DataContext = CollectionViewSource.GetDefaultView(envelope.Message.ToList());

                form.ShowDialog();
            }
        }

        private void Test2()
        {
            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                var listings = dataContext.EbayListingItems.Where(p => p.Listing == null);


                foreach (var listing in listings)
                {
                    dataContext.EbayListingItems.DeleteObject(listing);
                }



                dataContext.SaveChanges();

                var posts1 = bsi_quantities_message.Createbsi_quantities_message(0, 5, "testin1", true);

                dataContext.bsi_quantities_message.AddObject(posts1);

                var posts2 = dataContext.CopyEntity<bsi_quantities_message>(posts1);

                dataContext.bsi_quantities_message.AddObject(posts2);

                dataContext.SaveChanges();

                
            }
        }
    }
}
