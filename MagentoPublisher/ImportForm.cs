using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml;
using MagentoUpload.MagentoApi;
using System.IO;
using System.Drawing.Imaging;
using BerkeleyEntities;
using System.Globalization;

namespace MagentoUpload
{
    public partial class MagentoImportForm : Form
    {
        private Mage_Api_Model_Server_V2_HandlerPortTypeClient _proxy = null;
        private List<CatalogProductFactory> _catalogProductFactories;
        private List<PendingWorkOrder> _pendingWorkOrders = null;
        private BackgroundWorker _bckWorker;
        private string _currentWorkOrder;
        private string _sessionID;
        private int _succesfulUploads;
        private int _errorCount;

        public MagentoImportForm()
        {
            InitializeComponent();
            
        }

        private void MagentoImportForm_Load(object sender, EventArgs e)
        {
            _catalogProductFactories = new List<CatalogProductFactory>();
            _catalogProductFactories.Add(new Catalog_Men_US_Size());
            _catalogProductFactories.Add(new Catalog_Women_US_Size());
        }

        private void btnLoadPending_Click(object sender, EventArgs e)
        {
            pendingWorkOrderBindingSource.DataSource = null;
            _pendingWorkOrders = new List<PendingWorkOrder>();

            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                var pendingWorkOrders = dataContext.bsi_posts
                    .Where(p => p.status == 10 && p.marketplace == 512)
                    .Select(p => new { WorkOrder = p.purchaseOrder, User = p.listUser }).Distinct();

                foreach (var wo in pendingWorkOrders)
                {
                    PendingWorkOrder pwo = new PendingWorkOrder(wo.WorkOrder, wo.User);
                    pwo.PendingCount = dataContext.bsi_posts.Where(p => p.status == 10 && p.purchaseOrder.Equals(wo.WorkOrder) && p.marketplace == 512).SelectMany(p => p.bsi_quantities).Count();
                    pwo.TotalCount = dataContext.bsi_posts.Where(p => p.purchaseOrder.Equals(wo.WorkOrder) && p.marketplace == 512).SelectMany(p => p.bsi_quantities).Count();
                    _pendingWorkOrders.Add(pwo);
                }


                pendingWorkOrderBindingSource.DataSource = _pendingWorkOrders;
            }

        }

        public void btnImport_Click(object sender, EventArgs e)
        {
            //TEMP
            //_proxy = new Mage_Api_Model_Server_V2_HandlerPortTypeClient();
            //_sessionID = _proxy.login("berkeleyJuan", "genesis13");
            //catalogProductImageEntity[] images = _proxy.catalogProductAttributeMediaList(_sessionID, "600250WPAT01", null, "sku");


            if (dgvPendingWorkOrder.CurrentRow == null)
            {
                MessageBox.Show("Select a work order and try again !");
                return;
            }

            var pwo =  dgvPendingWorkOrder.CurrentRow.DataBoundItem as PendingWorkOrder;

            _currentWorkOrder = pwo.WorkOrder;

            _bckWorker = new BackgroundWorker();
            _bckWorker.WorkerReportsProgress = true;
            _bckWorker.DoWork += new DoWorkEventHandler(bckWorker_DoWork);
            _bckWorker.ProgressChanged += new ProgressChangedEventHandler(bckWorker_ProgressChanged);
            _bckWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bckWorker_RunWorkerCompleted);

            pbStatus.Visible = true;
            btnImport.Enabled = false;
            lbCurrentUpload.Text = "Uploading " + _currentWorkOrder;

            _bckWorker.RunWorkerAsync();
        }

        public void bckWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            pbStatus.Visible = false;
            btnImport.Enabled = true;
            pbStatus.Value = 0;
            lbCurrentUpload.Text = "";

            MessageBox.Show(
                _succesfulUploads.ToString() + 
                " products were uploaded and " + 
                _errorCount.ToString() + 
                " products could not be uploaded. Check today's log for more information."
                );
        }

        public void bckWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pbStatus.Value = e.ProgressPercentage;
        }

        public void bckWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            _proxy = new Mage_Api_Model_Server_V2_HandlerPortTypeClient();
            _sessionID = _proxy.login("berkeleyJuan", "genesis13");
            _catalogProductFactories.ForEach(p => p.Initiliaze(_proxy, _sessionID));

            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                var posts = dataContext.bsi_posts.Where(p => p.marketplace == 512 && p.status == 10 && p.purchaseOrder.Equals(_currentWorkOrder)).ToList();

                double processed = 0;
                double totalPending = posts.Count();
                _succesfulUploads = 0;
                _errorCount = 0;

                if (totalPending > 0)
                {
                    foreach (bsi_posts post in posts)
                    {
                        try
                        {
                            CreateOrUpdateCatalogProduct(post);
                            _succesfulUploads += post.bsi_quantities.Count;
                            post.status = 0;
                            
                        }
                        catch (NotImplementedException ex)
                        {
                            foreach (bsi_quantities postItem in post.bsi_quantities)
                            {
                                _errorCount++;
                                this.Log(postItem.itemLookupCode + ": " + ex.Message);
                            }
                        }
                        dataContext.SaveChanges();
                        processed++;
                        _bckWorker.ReportProgress((int)((processed / totalPending) * 100));
                    }

                    
                }
            }
        }
     
        private void CreateOrUpdateCatalogProduct(bsi_posts post)
        {
            CatalogProductFactory catalogProductFactory = _catalogProductFactories.SingleOrDefault(p => p.IsMatch(post));

            if (catalogProductFactory == null)
                throw new NotImplementedException("This type of product is currently not supported");

            List<string> childSkus = new List<string>();

            foreach (bsi_quantities postItem in post.bsi_quantities)
            {
                string sku = postItem.itemLookupCode;
                catalogProductCreateEntity simpleProductData = catalogProductFactory.CreateProductData(post, postItem);
                try
                {
                    int ID = _proxy.catalogProductCreate(_sessionID, "simple", catalogProductFactory.GetAttributeSet(), sku, simpleProductData, null);
                    childSkus.Add(sku);
                }
                catch (Exception ex)
                {
                    childSkus.Add(sku);
                    if (!_proxy.catalogProductUpdate(_sessionID, sku, simpleProductData, null, "sku"))
                    {
                        this.Log(sku + ": " + ex.Message);
                    }
                }
            }

            catalogProductCreateEntity configurableProductData = catalogProductFactory.CreateParentProductData(post, childSkus);
            try
            {
                int parentID = _proxy.catalogProductCreate(_sessionID, "configurable", catalogProductFactory.GetAttributeSet(), post.sku, configurableProductData, null);

                AssignImages(post, parentID.ToString());
            }
            catch (Exception ex)
            {
                if (!_proxy.catalogProductUpdate(_sessionID, post.sku, configurableProductData, null, "sku"))
                {
                    this.Log(post.sku + ": " + ex.Message);
                }  
            }    
        }

        private void AssignImages(bsi_posts post, string parentID)
        {
            //Upload associated images
            string index = "";
            int count = 1;
            string imagePath = @"P:/products/" + post.bsi_posting.brand + @"/" + post.sku + ".jpg";

            while (File.Exists(imagePath))
            {
                catalogProductAttributeMediaCreateEntity media = UploadProductImage(imagePath, count, post.sku + index);
                _proxy.catalogProductAttributeMediaCreate(_sessionID, parentID.ToString(), media, null, "product ID");

                count++;
                index = "-" + count.ToString();
                imagePath = @"P:/products/" + post.bsi_posting.brand + @"/" + post.sku + index + ".jpg";
            }
        }

        private catalogProductAttributeMediaCreateEntity UploadProductImage(string path, int count, string label)
        {
            var imageStream = new MemoryStream();

            using (var i = Image.FromFile(path))
            {
                i.Save(imageStream, ImageFormat.Jpeg);
            }
            byte[] encbuff = imageStream.ToArray();

            string enc = Convert.ToBase64String(encbuff, 0, encbuff.Length);


            var imageEntity = new catalogProductImageFileEntity();
            imageEntity.content = enc;
            imageEntity.mime = "image/jpeg";

            imageStream.Close();

            string[] types = null;

            if (count == 1)
                types = new string[] { "image", "small_image", "thumbnail" };
            else
                types = new string[] { "0", "0", "0" };

            //"image", "small_image", "thumbnail"

            var entityP = new catalogProductAttributeMediaCreateEntity();
            entityP.label = label;
            entityP.file = imageEntity;
            entityP.types = types;
            entityP.position = "0";
            entityP.exclude = "0";

            return entityP;
        }

        //private catalogProductCreateEntity BuildProductData(bsi_posts post, bsi_quantities postItem)
        //{
        //    int brandID = this.GetBrandID(post.bsi_posting.brand);
        //    int ebayCategoryID = this.GetEbayCategoryID(post.bsi_posting.gender, post.bsi_posting.category);

        //    catalogProductCreateEntity productData = new catalogProductCreateEntity();
        //    productData.visibility = "4";
        //    productData.stock_data = null;
        //    productData.additional_attributes = null;
        //    productData.name = post.title;
        //    productData.description = post.bsi_posting.fullDescription;
        //    productData.short_description = post.title;
        //    productData.price = post.price;
        //    productData.status = "1";
        //    productData.websites = new string[1] { "base" };
        //    productData.has_options = "1";
        //    productData.categories = new string[2] { ebayCategoryID.ToString(), brandID.ToString() };
        //    productData.visibility = "1";
        //    productData.stock_data = new catalogInventoryStockItemUpdateEntity();
        //    productData.stock_data.is_in_stock = 1;
        //    productData.stock_data.is_in_stockSpecified = true;

        //    productData.price = post.price;
        //    productData.stock_data.qty = postItem.quantity.ToString();
        //    productData.additional_attributes = new catalogProductAdditionalAttributesEntity();
        //    productData.additional_attributes.single_data = new associativeEntity[2] 
        //    { 
        //        this.BuildSizeData(post.bsi_posting.gender, postItem.size), 
        //        this.BuildWidthData(post.bsi_posting.gender, postItem.width)
        //    };

        //    return productData;
        //}

        //private catalogProductCreateEntity BuildParentProductData(bsi_posts post, IEnumerable<string> childSkus)
        //{
        //    int brandID = this.GetBrandID(post.bsi_posting.brand);
        //    int ebayCategoryID = this.GetEbayCategoryID(post.bsi_posting.gender, post.bsi_posting.category);

        //    catalogProductCreateEntity productData = new catalogProductCreateEntity();
        //    productData.visibility = "4";
        //    productData.stock_data = null;
        //    productData.additional_attributes = null;
        //    productData.name = post.title;
        //    productData.description = post.bsi_posting.fullDescription;
        //    productData.short_description = post.title;
        //    productData.price = post.price;
        //    productData.status = "1";
        //    productData.websites = new string[1] { "base" };
        //    productData.has_options = "1";
        //    productData.categories = new string[2] { ebayCategoryID.ToString(), brandID.ToString() };
        //    productData.associated_skus = childSkus.ToArray();

        //    return productData;
        //}

        //private int GetBrandID(string brandName)
        //{
        //    CultureInfo cultureInfo = new CultureInfo("en-US", false);
        //    brandName = cultureInfo.TextInfo.ToTitleCase(brandName.ToLower());

        //    if (_cachedBrandCategoryTree == null)
        //        _cachedBrandCategoryTree = _proxy.catalogCategoryTree(_sessionID, "21", "english");

        //    catalogCategoryEntity category = _cachedBrandCategoryTree.children.SingleOrDefault(p => p.name.Equals(brandName));

        //    if (category == null)
        //    {
        //        throw new NotImplementedException("Category "+brandName+" does not exist");
        //    }
        //    else
        //    {
        //        return category.category_id;
        //    }
        //}

        //private associativeEntity BuildSizeData(string gender, string size)
        //{
        //    gender = gender.Trim().ToUpper();

        //    if (gender.Equals("MEN") || gender.Equals("MENS"))
        //    {
        //        catalogAttributeOptionEntity[] attributes = _proxy.catalogProductAttributeOptions(_sessionID, "531", null);
        //        catalogAttributeOptionEntity attribute = attributes.Single(p => p.label.Equals(size));
        //        return new associativeEntity() { key = "size_mens_shoes", value = attribute.value };
        //    }
        //    else if (gender.Equals("WOMEN") || gender.Equals("WOMENS"))
        //    {
        //        catalogAttributeOptionEntity[] attributes = _proxy.catalogProductAttributeOptions(_sessionID, "533", null);
        //        catalogAttributeOptionEntity attribute = attributes.Single(p => p.label.Equals(size));
        //        return new associativeEntity() { key = "size_womens_shoes", value = attribute.value };
        //    }
        //    else
        //    {
        //        throw new NotImplementedException("Could not recognize gender: " + gender);
        //    }
        //}

        //private associativeEntity BuildWidthData(string gender, string width)
        //{
        //    gender = gender.Trim().ToUpper();

        //    if (gender.Equals("MEN") || gender.Equals("MENS"))
        //    {
        //        catalogAttributeOptionEntity[] attributes = _proxy.catalogProductAttributeOptions(_sessionID, "532", null);
        //        catalogAttributeOptionEntity attribute = attributes.Single(p => p.label.Equals(this.FormatMenWidth(width)));
        //        return new associativeEntity() { key = "width", value = attribute.value };
        //    }
        //    else if (gender.Equals("WOMEN") || gender.Equals("WOMENS"))
        //    {
        //        catalogAttributeOptionEntity[] attributes = _proxy.catalogProductAttributeOptions(_sessionID, "534", null);
        //        catalogAttributeOptionEntity attribute = attributes.Single(p => p.label.Equals(this.FormatWomenWidth(width)));
        //        return new associativeEntity() { key = "width_womens", value = attribute.value };
        //    }
        //    else
        //    {
        //        throw new NotImplementedException("Could not recognize gender: " + gender);
        //    }
        //}

        //private string FormatMenWidth(string width)
        //{
        //    string formattedWidth = null;

        //    if (width.Equals("A") || width.Equals("AA") || width.Equals("2A"))
        //    {
        //        formattedWidth = "Extra Narrow (A+)";
        //    }
        //    else if (width.Equals("C") || width.Equals("B"))
        //    {
        //        formattedWidth = "Narrow (C, B)";
        //    }
        //    else if (width.Equals("D") || width.Equals("M"))
        //    {
        //        formattedWidth = "Medium (D, M)";
        //    }
        //    else if (width.Equals("E") || width.Equals("W"))
        //    {
        //        formattedWidth = "Wide (E,W)";
        //    }
        //    else if (width.Equals("EE") || width.Equals("2E") || width.Equals("EEE") || width.Equals("3E") || width.Equals("XW"))
        //    {
        //        formattedWidth = "Extra Wide (EE+)";
        //    }

        //    return formattedWidth;
        //}

        //private string FormatWomenWidth(string width)
        //{
        //    string formattedWidth = null;

        //    if (width.Equals("M") || width.Equals("B"))
        //    {
        //        formattedWidth = "Medium (B, M)";
        //    }
        //    else if (width.Equals("C") || width.Equals("W") || width.Equals("D"))
        //    {
        //        formattedWidth = "Wide (C, D, W)";
        //    }
        //    else if (width.Equals("AA") || width.Equals("N") || width.Equals("2A"))
        //    {
        //        formattedWidth = "Narrow (AA, N)";
        //    }
        //    else if (width.Equals("AAA") || width.Equals("3A") || width.Equals("4A"))
        //    {
        //        formattedWidth = "Extra Narrow (AAA+)";
        //    }
        //    else if (width.Equals("E") || width.Equals("EE") || width.Equals("2E") || width.Equals("EEE") || width.Equals("3E"))
        //    {
        //        formattedWidth = "Extra Wide (E+)";
        //    }
        //    return formattedWidth;
        //}

        //private int GetEbayCategoryID(string gender, string category)
        //{
        //    gender = gender.Trim().ToUpper();

        //    int categoryID;

        //    if (gender.Equals("MEN") || gender.Equals("MENS"))
        //    {
        //        switch (category)
        //        {
        //            case "24087": categoryID = 22; break;
        //            case "15709": categoryID = 23; break;
        //            case "11498": categoryID = 24; break;
        //            case "53120": categoryID = 25; break;
        //            case "11504": categoryID = 26; break;
        //            case "11501": categoryID = -1; break;
        //            case "11505": categoryID = 27; break;
        //            default: categoryID = -1; break;
        //        }
        //    }
        //    else if (gender.Equals("WOMEN") || gender.Equals("WOMENS"))
        //    {
        //        switch (category)
        //        {
        //            case "45333": categoryID = 29; break;
        //            case "95672": categoryID = 30; break;
        //            case "53557": categoryID = 31; break;
        //            case "55793": categoryID = 28; break;
        //            case "62107": categoryID = 32; break;
        //            case "11632": categoryID = 33; break;
        //            case "53548": categoryID = -1; break;
        //            default: categoryID = -1; break;
        //        }
        //    }
        //    else
        //    {
        //        throw new NotImplementedException("Could not recognize gender: " + gender);
        //    }

        //    return categoryID;
        //}

        

        private void Log(string line)
        {
            // Write the string to a file.append mode is enabled so that the log
            // lines get appended to  test.txt than wiping content and writing the log
            string logFileName = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString().PadLeft(2, '0') + DateTime.Now.Day.ToString().PadLeft(2, '0') + ".txt";

            System.IO.StreamWriter file = new System.IO.StreamWriter(@"P:\PUBLISHING\Log\" + logFileName, true);
            file.WriteLine(line + "\n");

            file.Close();

        }

        

        
    }
}
