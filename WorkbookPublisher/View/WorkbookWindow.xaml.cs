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
using WorkbookPublisher.View;
using System.IO;

namespace WorkbookPublisher
{
    /// <summary>
    /// Interaction logic for WorkbookWindow.xaml
    /// </summary>
    public partial class WorkbookWindow : Window
    {
        private ExcelWorkbook _workbook;


        public WorkbookWindow()
        {
            InitializeComponent();
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
                    _workbook = new ExcelWorkbook(ofd.FileName);

                    CompositeCollection composite = new CompositeCollection();

                    foreach (var marketplace in dataContext.EbayMarketplaces)
                    {
                        composite.Add(new EbayPublisherViewModel(_workbook, marketplace.Code));
                    }

                    foreach (var marketplace in dataContext.AmznMarketplaces)
                    {
                        composite.Add(new AmznPublisherViewModel(_workbook, marketplace.Code));
                    }

                    tcSheets.ItemsSource = composite;
                }
                
            }
        }

        private async void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            btnUpdate.IsEnabled = false;

            if (_workbook == null)
            {
                MessageBox.Show("no workbook selected !");
                return;
            }

            try
            {
                var entries = await Task.Run<List<Entry>>(() => _workbook.ReadEntry(typeof(Entry), "MAIN"));

                await Task.Run(() => UpdateEntries(entries));

                await Task.Run(() => _workbook.UpdateMainSheet(entries));
            }
            catch (IOException x)
            {
                MessageBox.Show(x.Message);
            }

            btnUpdate.IsEnabled = true;
        }

        public void UpdateEntries(IEnumerable<Entry> entries)
        {
            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                foreach (Entry entry in entries)
                {
                    Item item = dataContext.Items.SingleOrDefault(p => p.ItemLookupCode.Equals(entry.Sku));

                    if (item == null)
                    {
                        entry.Message = "sku not found";
                        entry.Status = StatusCode.Error;
                        continue;
                    }

                    entry.Brand = item.SubDescription1;
                    entry.ClassName = item.ClassName;
                    entry.Category = item.CategoryName;
                    entry.Department = item.DepartmentName;
                    entry.Color = item.SubDescription2;
                    entry.Gender = item.SubDescription3;
                    entry.Description = item.Description;
                    entry.Notes = item.Notes;
                    entry.UPC = item.GTIN;
                    entry.Location = item.BinLocation;
                    entry.Qty = item.QtyAvailable;
                    entry.Cost = item.Cost;

                    if (item.EbayListingItems.Count > 0)
                    {
                        EbayListingItem listingItem = item.EbayListingItems.Single(p => p.ID == item.EbayListingItems.Max(s => s.ID));
                        entry.Title = listingItem.Title;
                        entry.FullDescription = listingItem.Listing.FullDescription;
                    }
                    else if (dataContext.bsi_quantities.Any(p => p.itemLookupCode.Equals(entry.Sku)))
                    {
                        var postDetails = dataContext.bsi_quantities.Where(p => p.itemLookupCode.Equals(entry.Sku));

                        int lastPostDetailID = postDetails.Max(p => p.id);

                        bsi_quantities postDetail = postDetails.Single(p => p.id == lastPostDetailID);

                        entry.Title = postDetail.title;
                        entry.FullDescription = postDetail.bsi_posts.bsi_posting.fullDescription;
                    }

                }
            }
        }

        private async void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            btnPrint.IsEnabled = false;

            if (_workbook == null)
            {
                MessageBox.Show("no workbook selected !");
                return;
            }

            try
            {
                var entries = await Task.Run<List<Entry>>(() => _workbook.ReadEntry(typeof(Entry), "MAIN"));

                await Task.Run(() => UpdateEntries(entries));

                string htmlTable = CreateHtmlTable(entries);

                File.WriteAllText(@"C:\Users\JUAN\Desktop\test.html", htmlTable);

            }
            catch (IOException x)
            {
                MessageBox.Show(x.Message);
            }

            btnPrint.IsEnabled = true;
        }

        private string CreateHtmlTable(IEnumerable<Entry> entries)
        {
            StringBuilder table = new StringBuilder();

            table.Append("<html><body><table border=\"2\">");

            PictureSetRepository picRepository = new PictureSetRepository();

            var classGroups = entries.GroupBy(p => p.ClassName).OrderBy(p => p.Key);

            table.Append("<tr><th>Picture</th><th>Brand</th><th>Gender</th><th>Sku</th><th>Qty</th><th>Cost</th><th>UPC</th><th>ORI</th><th>STG</th><th>OMS</th><th>LCS</th></tr>");

            foreach (var classGroup in classGroups)
            {
                var pics = picRepository.GetPictures(classGroup.First().Brand, classGroup.Select(p => p.Sku).ToList());

                var mainPic = pics.OrderBy(p => p.Name).First();

                bool hasImage = false;

                string rowSpan = classGroup.Count().ToString();

                foreach (Entry entry in classGroup)
                {
                    table.Append("<tr>");

                    if (!hasImage)
                    {
                        table.AppendFormat(@"<td rowspan=""{1}""><img src=""{0}"" style=""height:100px;max-width:100px;width: expression(this.width > 500 ? 500: true);""></td>", mainPic.Path, rowSpan);
                        table.AppendFormat(@"<td rowspan=""{1}"">{0}</td>", entry.Brand, rowSpan);
                        table.AppendFormat(@"<td rowspan=""{1}"">{0}</td>", entry.Gender, rowSpan);

                        
                    }

                    
                    table.AppendFormat("<td>{0}</td>", entry.Sku);               
                    table.AppendFormat("<td>{0}</td>", entry.Qty);
                    table.AppendFormat("<td>{0:C}</td>", entry.Cost);

                    

                    table.AppendFormat("<td>{0}</td>", string.IsNullOrEmpty(entry.UPC) ? "No" : "Yes");


                    if (!hasImage)
                    {
                        table.AppendFormat(@"<td rowspan=""{0}"">     </td>", rowSpan);
                        table.AppendFormat(@"<td rowspan=""{0}"">     </td>", rowSpan);
                        table.AppendFormat(@"<td rowspan=""{0}"">     </td>", rowSpan);
                        table.AppendFormat(@"<td rowspan=""{0}"">     </td>", rowSpan);

                        hasImage = true;
                    }
 
                    table.Append("</tr>");
                }

            }

            table.Append("</table></body></html>");

            return table.ToString();
        }

        private void Test1()
        {
            PostingTemplateEditor test = new PostingTemplateEditor();

            eBay.Service.Core.Soap.ItemType template = new eBay.Service.Core.Soap.ItemType();

            template.PaymentMethods = new eBay.Service.Core.Soap.BuyerPaymentMethodCodeTypeCollection();

            test.DataContext = new TemplateViewModel(template);

            Window testWindow = new Window();
            testWindow.Content = test;
            testWindow.ShowDialog();
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
