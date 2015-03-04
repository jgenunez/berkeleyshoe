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
using MarketplaceWebServiceProducts.Model;
using System.Xml;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using eBay.Service.Core.Soap;
using BerkeleyEntities.Ebay;

namespace WorkbookPublisher
{
    /// <summary>
    /// Interaction logic for WorkbookWindow.xaml
    /// </summary>
    public partial class WorkbookWindow : Window
    {
        private const string MAIN_SHEET = "MAIN";
        private ExcelWorkbook _workbook;


        public WorkbookWindow()
        {
            InitializeComponent();
        }

        private void TestTemplateEditor()
        {
            //using (berkeleyEntities dataContext = new berkeleyEntities())
            //{
            //    dataContext.MaterializeAttributes = true;

            //    EbayMarketplace marketplace = dataContext.EbayMarketplaces.Single(p => p.ID == 1);

            //    Publisher publisher = new Publisher(dataContext, marketplace, "testing");

            //    ListingDto listing = new ListingDto();

            //    ListingItemDto listingItem = new ListingItemDto();
            //    listingItem.Sku = "10061-10-M";

            //    listing.Items.Add(listingItem);

            //    listing.Sku = "10061-10-M";

            //    ItemType ebayDto = publisher.AddListing(listing);

            //    System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(ItemType));
            //    ebayDto = serializer.Deserialize(new FileStream(@"C:\Users\JUAN\Desktop\testing.xml", FileMode.Open)) as ItemType;
            //    serializer.Serialize(new FileStream(@"C:\Users\JUAN\Desktop\testing.xml", FileMode.Create), ebayDto);
            //}


            //Window window = new Window();
            
            //PostingTemplateEditor editor = new PostingTemplateEditor();
            //editor.DataContext = new TemplateViewModel(new ItemType() { PaymentMethods = new BuyerPaymentMethodCodeTypeCollection() });

            //StackPanel sp = new StackPanel();
            //sp.Children.Add(editor);

            //window.Content = sp;

            //window.ShowDialog();
        }

        private void TestBonanza()
        {
            //BerkeleyEntities.Bonanza.BonanzaServices services = new BerkeleyEntities.Bonanza.BonanzaServices();

            //services.FetchToken(1);

            //using (berkeleyEntities dataContext = new berkeleyEntities())
            //{
            //    var marketplace = dataContext.BonanzaMarketplaces.Single(p => p.ID == 1);
            //    marketplace.Token = "OVuUeDliFG";
            //    dataContext.SaveChanges();
            //}
        }

        private void lbCurrentWorkbook_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Process.Start(lbCurrentWorkbook.Content as string);
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

                    foreach (var marketplace in dataContext.BonanzaMarketplaces)
                    {
                        composite.Add(new BonanzaPublisherViewModel(_workbook, marketplace.Code));
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
                var entries = await Task.Run<List<object>>(() => _workbook.ReadSheet(typeof(MainEntry), MAIN_SHEET));

                var mainEntries = entries.Cast<MainEntry>().Where(p => !string.IsNullOrEmpty(p.Sku));

                await Task.Run(() => UpdateMainEntries(mainEntries));

                await Task.Run(() => _workbook.UpdateSheet(mainEntries.Cast<BaseEntry>().ToList(), typeof(MainEntry), MAIN_SHEET));
            }
            catch (IOException x)
            {
                MessageBox.Show(x.Message);
            }
            catch (FormatException x)
            {
                MessageBox.Show("Invalid columns");
            }



            MessageBox.Show("main sheet updated");

            btnUpdate.IsEnabled = true;

            
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
                var entries = await Task.Run<List<object>>(() => _workbook.ReadSheet(typeof(PrintEntry), "MAIN"));

                var printEntries = entries.Cast<PrintEntry>().Where(p => !string.IsNullOrEmpty(p.Sku));

                await Task.Run(() => UpdatePrintEntries(printEntries));

                string htmlTable = CreateHtmlTable(printEntries);

                File.WriteAllText(_workbook.Path.Replace(".xlsx", ".html"), htmlTable);

            }
            catch (IOException x)
            {
                MessageBox.Show(x.Message);
            }
            catch (FormatException x)
            {
                MessageBox.Show("Invalid columns");
            }

            btnPrint.IsEnabled = true;
        }

        private async void btnImportAmznData_Click(object sender, RoutedEventArgs e)
        {
            btnImportAmznData.IsEnabled = false;

            try
            {
                var entries = await Task.Run<List<object>>(() => _workbook.ReadSheet(typeof(MainEntry), MAIN_SHEET));

                var mainEntries = entries.Cast<MainEntry>().Where(p => !string.IsNullOrEmpty(p.Sku));

                await Task.Run(() => ImportAmznData(mainEntries));

                await Task.Run(() => _workbook.UpdateSheet(mainEntries.Cast<BaseEntry>().ToList(), typeof(MainEntry), MAIN_SHEET));
            }
            catch (IOException x)
            {
                MessageBox.Show(x.Message);
            }

            btnImportAmznData.IsEnabled = true;
        }

        private void rbtnPending_Checked(object sender, RoutedEventArgs e)
        {
            var composite = (CompositeCollection)tcSheets.ItemsSource;

            foreach (PublisherViewModel publisher in composite)
            {
                publisher.Entries.Filter = p => ((ListingEntry)p).Status.Equals(StatusCode.Pending);

                publisher.Entries.GroupDescriptions.Clear();

                publisher.Entries.GroupDescriptions.Add(new PropertyGroupDescription("Brand"));
            }
        }

        private void rbtnProcessing_Checked(object sender, RoutedEventArgs e)
        {
            var composite = (CompositeCollection)tcSheets.ItemsSource;

            foreach (PublisherViewModel publisher in composite)
            {
                publisher.Entries.Filter = p => ((ListingEntry)p).Status.Equals(StatusCode.Processing);

                publisher.Entries.GroupDescriptions.Clear();

                publisher.Entries.GroupDescriptions.Add(new PropertyGroupDescription("Brand"));
            }
        }

        private void rbtnCompleted_Checked(object sender, RoutedEventArgs e)
        {
            var composite = (CompositeCollection)tcSheets.ItemsSource;

            foreach (PublisherViewModel publisher in composite)
            {
                publisher.Entries.Filter = p => ((ListingEntry)p).Status.Equals(StatusCode.Completed);

                publisher.Entries.GroupDescriptions.Clear();

                publisher.Entries.GroupDescriptions.Add(new PropertyGroupDescription("Brand"));
            }
        }

        private void rbtnError_Checked(object sender, RoutedEventArgs e)
        {
            var composite = (CompositeCollection)tcSheets.ItemsSource;

            foreach (PublisherViewModel publisher in composite)
            {
                publisher.Entries.Filter = p => ((ListingEntry)p).Status.Equals(StatusCode.Error);

                publisher.Entries.GroupDescriptions.Clear();

                publisher.Entries.GroupDescriptions.Add(new PropertyGroupDescription("Message"));
            }
        }

        public void UpdateMainEntries(IEnumerable<MainEntry> entries)
        {
            var titleMaps = _workbook.ReadTitleMapRules();
            TextInfo textInfo = Thread.CurrentThread.CurrentCulture.TextInfo;
            PictureSetRepository picRepository = new PictureSetRepository();

            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                dataContext.MaterializeAttributes = true;

                foreach (MainEntry entry in entries)
                {
                    try
                    {
                        Item item = dataContext.Items.Include("EbayListingItems").Include("AmznListingItems").SingleOrDefault(p => p.ItemLookupCode.Equals(entry.Sku));

                        if (item == null)
                        {
                            entry.Message = "sku not found";
                            continue;
                        }

                        entry.Brand = item.SubDescription1;
                        entry.ClassName = item.ClassName;
                        entry.Qty = item.QtyAvailable;
                        entry.Cost = item.Cost;

                        entry.Department = item.DepartmentName;
                        entry.Category = item.CategoryName;
                        entry.Gender = item.SubDescription3;
                        entry.Color = item.SubDescription2;
                        entry.Notes = item.Notes;
                        entry.Price = item.Price;
                        entry.Location = item.BinLocation;
                        entry.Cost = item.Cost;
                        entry.Qty = item.QtyAvailable;
                        entry.Description = item.Description;
                        entry.UPC = item.GTIN;

                        var pics = picRepository.GetPictures(entry.Brand, new List<string>() { entry.Sku });

                        entry.PictureCount = pics.Count;

                        var titleMap = titleMaps.SingleOrDefault(p => p.Department.Equals(item.DepartmentName) && p.Category.Equals(item.CategoryName));

                        if (titleMap == null)
                        {
                            titleMap = new TitleMapRule();
                            titleMap.Map = "";
                        }

                        string description = item.Description;
                        string dims = string.Empty;

                        foreach (var attribute in item.Dimensions)
                        {
                            description = description.Replace(attribute.Value.Value, "");
                            dims += attribute.Value.Value + " ";
                        }

                        entry.TitleFormula = textInfo.ToTitleCase((entry.Brand + " " + titleMap.Map + " Size " + dims + description).ToLower());

                        var ebayHistory = string.Join(" ", item.EbayListingItems.Where(p => p.Listing.Status.Equals(EbayMarketplace.STATUS_ACTIVE)).Select(p => p.ToString()));
                        var amznHistory = string.Join(" ", item.AmznListingItems.Where(p => p.IsActive).Select(p => p.ToString()));

                        entry.Status = ebayHistory + " " + amznHistory;

                        if (item.EbayListingItems.Where(w => w.Title != null).Count() > 0)
                        {
                            EbayListingItem listingItem = item.EbayListingItems.Single(p => p.ID == item.EbayListingItems.Where(w => w.Title != null).Max(s => s.ID));
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
                    catch (Exception e)
                    {
                        entry.Message = e.Message;
                    }

                }
            }
        }

        public void UpdatePrintEntries(IEnumerable<PrintEntry> entries)
        {
            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                foreach (PrintEntry entry in entries)
                {
                    try
                    {
                        Item item = dataContext.Items.Include("EbayListingItems").Include("AmznListingItems").SingleOrDefault(p => p.ItemLookupCode.Equals(entry.Sku));

                        if (item == null)
                        {
                            continue;
                        }

                        entry.Brand = item.SubDescription1;
                        entry.ClassName = item.ClassName;
                        entry.Qty = item.QtyAvailable;
                        entry.Cost = item.Cost;

                        StringBuilder sb = new StringBuilder();

                        var marketplaces = item.EbayListingItems.GroupBy(p => p.Listing.Marketplace);

                        foreach (var marketplace in marketplaces)
                        {
                            var actives = marketplace.Where(p => p.Listing.Status.Equals("Active"));

                            foreach (var active in actives)
                            {
                                sb.AppendLine(active.ToString());
                            }
                        }

                        entry.Active = sb.ToString();

                        //var ebayHistory = string.Join("<br>",item.EbayListingItems.Select(p => p.ToString()));
                        //var amznHistory = string.Join("<br>", item.AmznListingItems.Select(p => p.ToString()));
                        //entry.History = ebayHistory + "<br>" + amznHistory;

                    }
                    catch (Exception e)
                    {

                    }

                }
            }
        }

        private string CreateHtmlTable(IEnumerable<PrintEntry> entries)
        {
            StringBuilder table = new StringBuilder();

            string style = @"<style> @media print{thead{display: table-header-group;}} img { height:100px; max-width:100px; width: expression(this.width > 500 ? 500: true);} td { text-align: center;} .input { padding: 50px; }  </style>";

            table.AppendFormat(@"<html><head>{0}</head><body><table border=""2"">", style);

            PictureSetRepository picRepository = new PictureSetRepository();

            var classGroups = entries.GroupBy(p => p.ClassName).OrderBy(p => p.Key);

            table.Append("<thead><tr><th>Picture</th><th>Brand</th><th>Gender</th><th>Sku</th><th>Qty</th><th>Cost</th><th>UPC</th><th>Active</th><th>ORI</th><th>STG</th><th>OMS</th><th>LCS</th></tr></thead><tbody>");

            foreach (var classGroup in classGroups)
            {
                var pics = picRepository.GetPictures(classGroup.First().Brand, classGroup.Select(p => p.Sku).ToList());

                var mainPic = pics.OrderBy(p => p.Name).FirstOrDefault();

                bool hasImage = false;

                string rowSpan = classGroup.Count().ToString();

                foreach (PrintEntry entry in classGroup.OrderBy(p => p.Sku))
                {
                    table.Append("<tr>");

                    if (!hasImage)
                    {
                        if (mainPic != null)
                        {
                            table.AppendFormat(@"<td rowspan=""{1}""><img src=""{0}""></td>", mainPic.Path, rowSpan);
                        }
                        else
                        {
                            table.AppendFormat(@"<td rowspan=""{1}""><img src=""{0}""></td>", string.Empty, rowSpan);
                        }

                        
                        table.AppendFormat(@"<td rowspan=""{1}"">{0}</td>", entry.Brand, rowSpan);
                        table.AppendFormat(@"<td rowspan=""{1}"">{0}</td>", entry.Gender, rowSpan);
                    }

                    
                    table.AppendFormat("<td>{0}</td>", entry.Sku);               
                    table.AppendFormat("<td>{0}</td>", entry.Qty);
                    table.AppendFormat("<td>{0:C}</td>", entry.Cost);

                    

                    table.AppendFormat("<td>{0}</td>", string.IsNullOrEmpty(entry.UPC) ? "No" : "Yes");

                    table.AppendFormat(@"<td class=""history"">{0}</td>", entry.Active);


                    if (!hasImage)
                    {
                        table.AppendFormat(@"<td class=""input"" rowspan=""{0}"">     </td>", rowSpan);
                        table.AppendFormat(@"<td class=""input""  rowspan=""{0}"">     </td>", rowSpan);
                        table.AppendFormat(@"<td class=""input""  rowspan=""{0}"">     </td>", rowSpan);
                        table.AppendFormat(@"<td class=""input""  rowspan=""{0}"">     </td>", rowSpan);

                        hasImage = true;
                    }
 
                    table.Append("</tr>");
                }

            }

            table.Append("</tbody></table></body></html>");

            return table.ToString();
        }

        private void ImportAmznData(IEnumerable<MainEntry> entries)
        {
            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                BerkeleyEntities.Amazon.AmazonServices amznServices = new BerkeleyEntities.Amazon.AmazonServices();

                foreach (MainEntry entry in entries)
                {
                    Item item = dataContext.Items.Single(p => p.ItemLookupCode.Equals(entry.Sku));
                    entry.UPC = item.GTIN;
                }

                AmznMarketplace marketplace = dataContext.AmznMarketplaces.First();

                var entriesWithUPC = entries.Where(p => !string.IsNullOrEmpty(p.UPC));

                var results = amznServices.GetMatchingProductForId(marketplace.ID, entriesWithUPC.Select(p => p.UPC));

                foreach (var result in results)
                {
                    MainEntry entry = entriesWithUPC.First(p => p.UPC.Equals(result.Id));

                    if (result.IsSetProducts() && result.Products.IsSetProduct())
                    {
                        Product product = result.Products.Product.First();

                        foreach (var attributeSet in product.AttributeSets.Any.Cast<XmlElement>())
                        {
                            var nodes = attributeSet.ChildNodes.OfType<XmlNode>();

                            if (nodes.Any(p => p.LocalName.Equals("Feature")))
                            {
                                StringBuilder features = new StringBuilder();

                                features.Append("<ul>");

                                foreach (XmlNode node in nodes.Where(p => p.LocalName.Equals("Feature")))
                                {
                                    features.AppendFormat("<li>{0}</li>", node.InnerText);
                                }

                                features.Append("</ul>");

                                entry.AmznDescription = features.ToString();
                            }


                            if (nodes.Any(p => p.LocalName.Equals("Title")))
                            {
                                XmlNode node = nodes.Single(p => p.LocalName.Equals("Title"));

                                entry.AmznTitle = node.InnerText;
                            }
                        }
                    }
                }

                foreach (MainEntry entry in entries)
                {
                    Item item = dataContext.Items.Single(p => p.ItemLookupCode.Equals(entry.Sku));

                    AmznListingItem listingItem = item.AmznListingItems.FirstOrDefault();

                    if (listingItem != null)
                    {
                        entry.Asin = listingItem.ASIN;
                    }
                }

                var competitivePriceResults = amznServices.GetLowestOfferListingsForAsin(marketplace.ID, entries.Where(p => !string.IsNullOrWhiteSpace(p.Asin)).Select(p => p.Asin));

                foreach (var result in competitivePriceResults)
                {
                    MainEntry entry = entries.First(p => p.Asin.Equals(result.ASIN));

                    if (result.IsSetProduct() && result.Product.IsSetLowestOfferListings() && result.Product.LowestOfferListings.IsSetLowestOfferListing())
                    {
                        var competitivePrice = result.Product.LowestOfferListings.LowestOfferListing.First();
                        entry.AmznPrice = competitivePrice.Price.LandedPrice.Amount;
                    }
                }
            }
        }



        

        

        

        



        


        

        

        

        

        
    }

    
}
