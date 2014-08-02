//
// A la Mayor Gloria a Dios
//


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Configuration;

using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;

using Excel = Microsoft.Office.Interop.Excel;

using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using EbayServices;
using AmazonServices;
using BerkeleyEntities;
using BSI_InventoryPreProcessor.berkeleyDataSetTableAdapters;


namespace BSI_InventoryPreProcessor
{
    public partial class Form1 : Form
    {
        public bool DEBUG_MODE = false;
        public const int POSTING_STATUS_ACTIVE   = 0;
        public const int POSTING_STATUS_READY2PUBLISH = 10;
        public const int POSTING_STATUS_BLOCKED  = 100;

        public const int QUANTITY_RECORD_TYPE_POSTING = 0;
        public const int QUANTITY_RECORD_TYPE_VARIATION = 10;

        public const int VARIATIONS_NONE = 0;
        public const int VARIATIONS_SIZE = 1;
        public const int VARIATIONS_WIDTH = 2;
        public const int VARIATIONS_COLOR = 4;

        public static string EXCEL_COLUMN_INITIAL = "A";
        public static string EXCEL_COLUMN_FINAL = "AQ";

        public static int EXCEL_INTCOLUMN_PO = 1;
        public static int EXCEL_INTCOLUMN_LISTUSER = 2;

        public static int EXCEL_INTCOLUMN_BRAND = 3;
        public static int EXCEL_INTCOLUMN_SKU = 4;
        public static int EXCEL_INTCOLUMN_LOOKUPCODE = 5;
        public static int EXCEL_INTCOLUMN_SIZE = 6;
        public static int EXCEL_INTCOLUMN_WIDTH = 7;
        public static int EXCEL_INTCOLUMN_CONDITION = 8;
        public static int EXCEL_INTCOLUMN_CATEGORY = 9;
        public static int EXCEL_INTCOLUMN_STYLE = 10;
        public static int EXCEL_INTCOLUMN_TITLE = 11;
        public static int EXCEL_INTCOLUMN_COUNT = 12;
        public static int EXCEL_INTCOLUMN_FULLD = 13;
        public static int EXCEL_INTCOLUMN_KEYWORDS = 14;
        public static int EXCEL_INTCOLUMN_MATERIAL = 15;
        public static int EXCEL_INTCOLUMN_COLOR = 16;
        public static int EXCEL_INTCOLUMN_SHADE = 17;
        public static int EXCEL_INTCOLUMN_HEEL = 18;
        public static int EXCEL_INTCOLUMN_RMSDESCRIPTION = 19;
        public static int EXCEL_INTCOLUMN_GENDER = 20;
        public static int EXCEL_INTCOLUMN_RECEIVED = 21;
        public static int EXCEL_INTCOLUMN_COST = 22;
        public static int EXCEL_INTCOLUMN_UPC = 23;

        // 2013-Jan-02: New posting sheet format with a single qty/price
        public static int EXCEL_INTCOLUMN_QUANTITY = 24;
        public static int EXCEL_INTCOLUMN_PRICE = 25;

        public static int EXCEL_INTCOLUMN_SELLINGFORMAT = 26; // 37;
        public static int EXCEL_INTCOLUMN_STARTDATE = 27; // 38;

        // Previous format with store info per store 2013-01-02
        public static int EXCEL_INTCOLUMN_MSRP = 19; 
        public static int EXCEL_INTCOLUMN_QTY_AMAZON = 25;
        public static int EXCEL_INTCOLUMN_QTY_HARVARD = 26;
        public static int EXCEL_INTCOLUMN_QTY_MECALZO = 27;
        public static int EXCEL_INTCOLUMN_QTY_1MS = 28;
        public static int EXCEL_INTCOLUMN_QTY_PAS = 29;
        public static int EXCEL_INTCOLUMN_QTY_SA = 30;

        public static int EXCEL_INTCOLUMN_PRICE_AMAZON = 31;
        public static int EXCEL_INTCOLUMN_PRICE_HARVARD = 32;
        public static int EXCEL_INTCOLUMN_PRICE_MECALZO = 33;
        public static int EXCEL_INTCOLUMN_PRICE_1MS = 34;
        public static int EXCEL_INTCOLUMN_PRICE_PAS = 35;
        public static int EXCEL_INTCOLUMN_PRICE_SA = 36;

        //private static ApiContext apiContext = null;
        private string _descriptionHeader, _descriptionFooter, lorginalpathfile, lpicturespath;

        public static int EBAY_STARTINGINDEX = 2;
        public static int WEB_STARTINGINDEX = 6;

        private uint[] MarketPlaces = { ItemMarketplace.MARKETPLACE_AMAZON, 
                                        ItemMarketplace.MARKETPLACE_AMAZON_HARVARD, 
                                        ItemMarketplace.MARKETPLACE_EBAY_MECALZO,
                                        ItemMarketplace.MARKETPLACE_EBAY_1MS,
                                        ItemMarketplace.MARKETPLACE_EBAY_PAS,
                                        ItemMarketplace.MARKETPLACE_EBAY_SA,
                                        ItemMarketplace.MARKETPLACE_WEB_SF };

        //Boolean lstop;

        int gCurrentMarketplace = 0;

        List<EbayEntry> _entries;
        List<EbayEntry> _errors;
        List<EbayEntry> _completed;

        // Products on the marketplaces. Each element is a marketplace
        //List<ItemType>[] itemsOnline = new List<ItemType>[12];

        private EbayMarketplace _marketplace;

        public Form1()
        {
            InitializeComponent();            
        }  // public Form1()

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(txtOriginalFile.Text.Trim()))
            {
                MessageBox.Show("Please select the original Excel file with the inventory to add");
                txtOriginalFile.Focus();
                return;
            }

            if (String.IsNullOrEmpty(txtPicturesPath.Text.Trim()))
            {
                MessageBox.Show("Please specify the path where the pictures are stored");
                txtPicturesPath.Focus();
                return;
            }

            if (MessageBox.Show("ABOUT TO PUBLISH PRODUCTS FOR \r\n\r\n" + cmbMarkets.Text + "\r\n\r\nREADY TO PROCEED?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.No)
                return;

            // Everything is okay to go
            lorginalpathfile = txtOriginalFile.Text.Trim();
            lpicturespath = txtPicturesPath.Text.Trim();

            btnStart.Enabled = false;

            _entries = new List<EbayEntry>();
            _errors = new List<EbayEntry>();
            _completed = new List<EbayEntry>();

            currentMarketPlace = ldsMarkets[cmbMarkets.SelectedIndex];

            ReadExcelEntries(lorginalpathfile);

            CheckAvailablePictures();

            //if (cmbMarkets.SelectedIndex >= EBAY_STARTINGINDEX && cmbMarkets.SelectedIndex < WEB_STARTINGINDEX)
            //{
            //    UpdateMarketplaces();
            //}

            PublishProducts();

            MessageBox.Show("Preprocessor finished with " + _errors.Count.ToString()  + " Errors");

            txtStatus.AppendText("\n\n");

            if (_errors.Count > 0)
            {
                txtStatus.AppendText("--------------- ERRORS -----------------\n\n");
            }

            foreach (EbayEntry excelItem in _errors)
            {
               
                if(excelItem.Variation == EbayEntry.ITEM_TYPE_PARENT)
                {
                    txtStatus.AppendText(excelItem.SKU + "  Error:" + excelItem.Result + "\n");
                    txtStatus.AppendText("\t" + excelItem.ItemLookupCode + " Row: " + excelItem.Row);
                }
                else
                {

                    txtStatus.AppendText(excelItem.ItemLookupCode + " Error:"+excelItem.Result + " Row:" + excelItem.Row);
                }

                txtStatus.AppendText("\n");
            }

            
        } 


        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                lorginalpathfile = openFileDialog1.FileName;
            txtOriginalFile.Text = lorginalpathfile;
        } // btnSearch_Click

        private string removeSize(string ltext)
        {
            StringBuilder lsb = new StringBuilder();

            if (ltext.ToLower().Contains(" size") || ltext.ToLower().Contains(" sz"))
            {
                int lszTextpos = -1;

                if (ltext.ToLower().Contains(" size"))
                {
                    lszTextpos = ltext.ToLower().IndexOf(" size");
                }
                else
                {
                    lszTextpos = ltext.ToLower().IndexOf(" sz");
                }

                if (lszTextpos >= 0)
                {
                    lsb.Append(ltext.Substring(0,lszTextpos));
                    
                    // Let's find the size or white space
                    int li = lszTextpos, lconsecutiveWidthChars=0;
                    bool lflag = false, lcheckingSizeW = false, lwidthfound = false;
                    while ( !lflag && li < ltext.Length )
                    {
                        if ( char.IsNumber(ltext[li]) ) lcheckingSizeW = true;
                        if ( char.IsWhiteSpace(ltext[li]) )
                        {
                            if ( lwidthfound )
                                lflag = true;
                        }
                        if ( char.IsLetter(ltext[li]) )
                        {
                            if ( lcheckingSizeW )
                            {
                                lwidthfound = true;
                                lconsecutiveWidthChars++;
                                if ( lconsecutiveWidthChars > 3 ) // Get out! This is now width!!
                                {
                                    li -= lconsecutiveWidthChars;
                                    lflag = true;
                                }
                            }
                        }
                        li++;
                    } // while
                    lsb.Append(' ');
                    lsb.Append(ltext.Substring(li));
                }
                else
                    lsb.Append(ltext);
            }
            else
            {
                lsb = new StringBuilder(ltext);
            }
            return lsb.ToString();
        } // removeSize

        

        private void ReadExcelEntries(string path)
        {
            Microsoft.Office.Interop.Excel.Application excelApp = new Microsoft.Office.Interop.Excel.Application();
            Microsoft.Office.Interop.Excel.Workbook theWorkbook = excelApp.Workbooks.Open(path, 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true);
            Microsoft.Office.Interop.Excel._Worksheet workSheet = (Microsoft.Office.Interop.Excel._Worksheet)theWorkbook.ActiveSheet;

            currentMarketPlace = ldsMarkets[cmbMarkets.SelectedIndex];

            List<EbayEntry> items = new List<EbayEntry>();

            bool stop = false;
            int currentRow = 2;

            while (!stop)
            {
                Microsoft.Office.Interop.Excel.Range range = workSheet.get_Range(EXCEL_COLUMN_INITIAL + currentRow.ToString(), EXCEL_COLUMN_FINAL + currentRow.ToString());
                System.Array row = (System.Array)range.Cells.Value;

                string firstCol = Convert.ToString(row.GetValue(1, 1));

                if (!String.IsNullOrEmpty(firstCol))
                {
                    try
                    {
                        items.Add(CreateEntry(row, currentRow.ToString()));
                    }
                    catch (Exception)
                    {
                        _errors.Add( new EbayEntry() {  ItemLookupCode = Convert.ToString(row.GetValue(1, EXCEL_INTCOLUMN_LOOKUPCODE)), Result = "Unable to read row" } );
                    }
                }
                else
                {
                    stop = true;
                }

                currentRow++;
            }

            theWorkbook.Close();
            excelApp.Quit();

            releaseObject(theWorkbook);
            releaseObject(excelApp);

            var variations = items.Where(p => p.SellingFormat.Equals("GTC") || p.SellingFormat.Equals("BIN")).GroupBy(p => p.SKU);
            var auctions = items.Where(p => p.SellingFormat.Contains("A"));

            foreach (var variation in variations)
            {
                if (variation.Count() > 1)
                {
                    EbayEntry parent = variation.First();
                    parent.Items.AddRange(variation.ToList());

                    _entries.Add(parent);
                }
                else
                {
                    _entries.Add(variation.First());
                }
                
            }

            _entries.AddRange(auctions);

        }

        private EbayEntry CreateEntry(System.Array row, string currentRow)
        {
            EbayEntry excelItem = new EbayEntry();
            excelItem.Row = currentRow;
            excelItem.SKU = row.GetValue(1, EXCEL_INTCOLUMN_SKU).ToString();
            excelItem.Alias = Convert.ToString(row.GetValue(1, EXCEL_INTCOLUMN_UPC)).Trim();
            excelItem.Brand = Convert.ToString(row.GetValue(1, EXCEL_INTCOLUMN_BRAND)).Trim();
            excelItem.Condition = Convert.ToString(row.GetValue(1, EXCEL_INTCOLUMN_CONDITION)).ToUpper().Trim();
            excelItem.Category = Convert.ToString(row.GetValue(1, EXCEL_INTCOLUMN_CATEGORY)).Trim();
            excelItem.FullDescription = Convert.ToString(row.GetValue(1, EXCEL_INTCOLUMN_FULLD)).Trim();
            excelItem.Cost = decimal.Parse(row.GetValue(1, EXCEL_INTCOLUMN_COST).ToString());
            excelItem.Price = decimal.Parse(row.GetValue(1, EXCEL_INTCOLUMN_PRICE).ToString());
            excelItem.Quantity = int.Parse(row.GetValue(1, EXCEL_INTCOLUMN_QUANTITY).ToString());
            excelItem.Gender = Convert.ToString(row.GetValue(1, EXCEL_INTCOLUMN_GENDER)).ToUpper().Trim();
            excelItem.Keywords = properNameString(Convert.ToString(row.GetValue(1, EXCEL_INTCOLUMN_KEYWORDS))).Trim();
            excelItem.Material = properNameString(Convert.ToString(row.GetValue(1, EXCEL_INTCOLUMN_MATERIAL))).Trim();
            excelItem.Color = properNameString(Convert.ToString(row.GetValue(1, EXCEL_INTCOLUMN_COLOR))).Trim();
            excelItem.Shade = properNameString(Convert.ToString(row.GetValue(1, EXCEL_INTCOLUMN_SHADE))).Trim();
            //excelItem.HeelHeight = Convert.ToString(row.GetValue(1, EXCEL_INTCOLUMN_HEEL)).Trim();
            //excelItem.MSRP = decimal.Parse(row.GetValue(1, EXCEL_INTCOLUMN_MSRP).ToString());
            excelItem.Received = int.Parse(row.GetValue(1, EXCEL_INTCOLUMN_RECEIVED).ToString());
            excelItem.RMS_Description = Convert.ToString(row.GetValue(1, EXCEL_INTCOLUMN_RMSDESCRIPTION)).Trim();
            excelItem.Size = Convert.ToString(row.GetValue(1, EXCEL_INTCOLUMN_SIZE));
            excelItem.Width = Convert.ToString(row.GetValue(1, EXCEL_INTCOLUMN_WIDTH));
            excelItem.Style = properNameString(Convert.ToString(row.GetValue(1, EXCEL_INTCOLUMN_STYLE))).Trim();
            excelItem.SellingFormat = Convert.ToString(row.GetValue(1, EXCEL_INTCOLUMN_SELLINGFORMAT)).ToUpper().Trim();
            excelItem.ItemLookupCode = Convert.ToString(row.GetValue(1, EXCEL_INTCOLUMN_LOOKUPCODE));
            excelItem.Title = Convert.ToString(row.GetValue(1, EXCEL_INTCOLUMN_TITLE));
            excelItem.purchaseOrder = Convert.ToString(row.GetValue(1, EXCEL_INTCOLUMN_PO));
            excelItem.listUser = Convert.ToString(row.GetValue(1, EXCEL_INTCOLUMN_LISTUSER));

            excelItem.MarketPlaces = (uint)currentMarketPlace.maskId;

            if (String.IsNullOrEmpty(excelItem.Condition))
            {
                excelItem.Condition = "NEW";
            }

            string dateString = Convert.ToString(row.GetValue(1, EXCEL_INTCOLUMN_STARTDATE));

            if (!String.IsNullOrEmpty(dateString))
            {
                excelItem.StartDate = DateTime.Parse(dateString);
                excelItem.StartDate = excelItem.StartDate.ToUniversalTime(); // Add 7 hours to convert from PDT to GMT
            };

            switch (excelItem.SellingFormat)
            {
                case "A": excelItem.EndDate = excelItem.StartDate.AddDays(7); break;
                case "A1": excelItem.EndDate = excelItem.StartDate.AddDays(1); break;
                case "A3": excelItem.EndDate = excelItem.StartDate.AddDays(3); break;
                case "A5": excelItem.EndDate = excelItem.StartDate.AddDays(5); break;
                case "BIN": excelItem.EndDate = excelItem.StartDate.AddDays(30); break;
                case "GTC": excelItem.EndDate = new DateTime(2020, 12, 25); break;
            }

            return excelItem;
        }

        private void CheckAvailablePictures()
        {
            // Now, it is time to look for the pictures
            DirectoryInfo ldi;
            FileInfo[] ldirEntries = new FileInfo[] { }; ;

            bool lflag = true;

            foreach (EbayEntry lxi in _entries)
            {
                try
                {
                    // Let's see how many pictures are available
                    String lpath = this.txtPicturesPath.Text + '\\' + lxi.Brand;

                    ldi = new DirectoryInfo(lpath);
                    ldirEntries = ldi.GetFiles(lxi.SKU + "*.jpg");
                    foreach (FileInfo lfi in ldirEntries)
                    {
                        byte[] lfilecontents = File.ReadAllBytes(lfi.FullName);
                        if (lfilecontents.Length > 0) lxi.Pictures.Add(lfi.FullName);
                    } // foreach

                    if (lxi.Pictures.Count > 0)
                    {
                        lxi.IsValid = true;
                        lxi.Pictures.Sort();
                    }
                    else
                    {
                        lxi.IsValid = false;
                        lflag = false;
                        lxi.Result = "no pictures found !";
                        _errors.Add(lxi);
                    }

                    txtStatus.Text = "Style " + lxi.ItemLookupCode + " has " + lxi.Pictures.Count + " pictures\r\n" + txtStatus.Text;
                    txtStatus.Update();
                }
                catch (Exception e)
                {
                    lxi.Result = e.Message;
                    lxi.IsValid = false;
                    _errors.Add(lxi);
                }
            } // foreach

            //MessageBox.Show("\n\nPLEASE VERIFY THE STATUS OF THE INITIAL VERIFICATION AND PROCEED\nTO SAVE PRODUCTS IN OUR DATABASES IF EVERYTHING IS CORRECT\n\n", "PROCESSING FINISHED");

            //if (!lflag && (cmbMarkets.SelectedIndex >= EBAY_STARTINGINDEX && cmbMarkets.SelectedIndex < WEB_STARTINGINDEX))
            //{
            //    // We cannot publish on eBay if there's one item without pics
            //    MessageBox.Show("\r\nAT LEAST ONE ITEM DOES NOT HAVE PICTURES. PLEASE CHECK THE LIST AND TRY AGAIN.\r\n" +
            //                    "You won't be able to publish on any eBay market without pictures.\r\n");
            //}
        }

        // ------------------------------------------ Service methods

        // Sort method for 2 items: by brand
        private int sortItems(EbayEntry p1, EbayEntry p2)
        {
            int lres = 0;

            lres = p1.Brand.CompareTo(p2.Brand);
            return lres;
        } // sortItems

        int sortBySize(EbayEntry p1, EbayEntry p2)
        {
            float lsize1, lsize2;
            int lres = 0;

            lsize1 = float.Parse(p1.Size);
            lsize2 = float.Parse(p2.Size);

            if ((lsize1 - lsize2) < 0)
                lres = -1;
            else
                if ((lsize1 - lsize2) > 0)
                    lres = 1;
                else
                    lres = 0;

            if (lres == 0) lres = string.Compare(p1.Width, p2.Width);

            return lres;
        } // sortBySize

        String convertWidth(string pgender, string pwidth)
        {
            string lwidth = "";

            switch (pgender)
            {
                case "MENS":
                    switch (pwidth)
                    {
                        case "XN": lwidth = "Extra Narrow (A+)"; break;
                        case "N": lwidth = "Narrow (C, B)"; break;
                        case "D":
                        case "M": lwidth = "Medium (D, M)"; break;
                        case "E":
                        case "W": lwidth = "Wide (E,W)"; break;
                        case "XW":
                        case "2E":
                        case "3E":
                        case "EEE":
                        case "EE":
                        case "WW": lwidth = "Extra Wide (EE+)"; break;
                    } // swtich
                    break;

                case "WOMENS":
                    switch (pwidth)
                    {
                        case "XN": lwidth = "Extra Narrow (AAA+)"; break;
                        case "N": lwidth = "Narrow (AA, N)"; break;
                        case "M":
                        case "B": lwidth = "Medium (B, M)"; break;
                        case "W":
                        case "C":
                        case "D": lwidth = "Wide (C, D, W)"; break;
                        case "XW":
                        case "WW": lwidth = "Extra Wide (E+)"; break;
                    }
                    break;

                case "YOUTH":
                    switch (pwidth)
                    {
                        case "XN": lwidth = "X Narrow"; break;
                        case "N": lwidth = "Narrow"; break;
                        case "M":
                        case "B": lwidth = "Medium"; break;
                        case "W":
                        case "C":
                        case "D": lwidth = "Wide"; break;
                        case "XW":
                        case "WW": lwidth = "X Wide"; break;
                    }
                    break;
            } // switch(pgender)

            return lwidth;
        } // convertWidth

        String getEbaySizeName(String pgender)
        {
            String lname = "";

            switch (pgender)
            {
                case "MENS": lname = "US Shoe Size (Men's)"; break;
                case "WOMENS": lname = "US Shoe Size (Women's)"; break;
                case "JUNIOR": lname = "US Shoe Size (Youth)"; break;
            } // switch

            return lname;
        } // getEbaySizeName

        private String properNameString(String ps)
        {
            StringBuilder ls;

            ls = new StringBuilder((!String.IsNullOrEmpty(ps)) ? ps.ToLower() : "");
            if (ls.Length > 0) ls[0] = Char.ToUpper(ls[0]);

            return ls.ToString();
        } // properNameString

        private void releaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;
                MessageBox.Show("Unable to release the Object " + ex.ToString());
            }
            finally
            {
                GC.Collect();
            }
        } // releaseObject

        private void PublishProducts()
        {
            uint[] mktPlaces = { 
                                   ItemMarketplace.MARKETPLACE_AMAZON, 
                                   ItemMarketplace.MARKETPLACE_AMAZON_HARVARD, 
                                   ItemMarketplace.MARKETPLACE_EBAY_MECALZO, 
                                   ItemMarketplace.MARKETPLACE_EBAY_1MS,
                                   ItemMarketplace.MARKETPLACE_EBAY_PAS,
                                   ItemMarketplace.MARKETPLACE_EBAY_SA,
                                   ItemMarketplace.MARKETPLACE_WEB_SF
                               };

            bool lstop = false;

            SqlConnection lconn = null;
            bsi_postingTableAdapter lda;
            berkeleyDataSetTableAdapters.bsi_postsTableAdapter lposts_da;
            berkeleyDataSetTableAdapters.bsi_quantitiesTableAdapter lqtys_da;

                lconn = new SqlConnection(Properties.Settings.Default.berkeleyConnectionString.ToString());
                lconn.Open();

                lda = new berkeleyDataSetTableAdapters.bsi_postingTableAdapter();
                lposts_da = new berkeleyDataSetTableAdapters.bsi_postsTableAdapter();
                lqtys_da = new berkeleyDataSetTableAdapters.bsi_quantitiesTableAdapter();

                lda.Connection = lconn;
                lposts_da.Connection = lconn;
                lqtys_da.Connection = lconn;

                {
                    currentMarketPlace = (berkeleyDataSet.bsi_marketplacesRow)ldsMarkets.Rows[cmbMarkets.SelectedIndex]; // lmarketPlace;


                    _descriptionHeader = currentMarketPlace.template_header;
                    _descriptionFooter = currentMarketPlace.template_footer;

                    //txtStatus.Text = "Publishing products for " + currentMarketPlace.name + "\r\n" + txtStatus.Text;
                    txtStatus.Update();
                    foreach (EbayEntry xlProduct in _entries)
                    {
                        //if ((xlProduct.MarketPlaces & currentMarketPlace.maskId) == 0) continue; // Skip items that do not belong to this marketplace
                        if (!xlProduct.IsValid && !this.chkPublishWOPics.Checked ) continue;

                        if (xlProduct.Items.Count > 1)
                            xlProduct.Title = removeSize(xlProduct.Title);

                        

                        txtStatus.Text = "Publishing " + xlProduct.Title + " [" + 
                                         xlProduct.ItemLookupCode + " | " + xlProduct.SellingFormat + 
                                         "]\r\n" + txtStatus.Text;
                        txtStatus.Update();

                        ItemType lproduct = lproduct = BuildItem(xlProduct);
                        
                        if (xlProduct.Items.Count == 0) // ONLY set price and QTY for individual products, not for Parents with children
                        {
                            // Set a price and Q temporal
                            lproduct.Quantity = 1;
                            lproduct.StartPrice.Value = 99.99;

                        }

                        try
                        {
                            FeeTypeCollection fees;
                            txtStatus.Text = " ...API call started..." + txtStatus.Text;
                            txtStatus.Update();

                            // Set one picture to the eBay product
                            lproduct.PictureDetails = new PictureDetailsType();
                            lproduct.PictureDetails.PictureURL = new StringCollection(new string[] { "http://www.tools4inet.com/0/products/tim/10061.jpg" });

                            // Choose the correct API call. AddItemCall works for auctions and for single items with best offer

                            if (!DEBUG_MODE && (currentMarketPlace.maskId > 8 && currentMarketPlace.maskId < 512)  ) // Publish only those who '8 < mask id < 512' (Not Amazons, nor websites)
                            {
                                if (xlProduct.SellingFormat == "A" || xlProduct.Items.Count == 0)
                                {
                                    VerifyAddItemCall api_AUCTION_Call = new VerifyAddItemCall(apiContext);
                                    fees = api_AUCTION_Call.VerifyAddItem(lproduct);
                                }
                                else
                                {
                                    VerifyAddFixedPriceItemCall api_FP_Call = new VerifyAddFixedPriceItemCall(apiContext);
                                    fees = api_FP_Call.VerifyAddFixedPriceItem(lproduct);
                                }

                                double listingFee = 0.0;
                                foreach (FeeType fee in fees)
                                {
                                    if (fee.Name == "ListingFee")
                                    {
                                        listingFee = fee.Fee.Value;
                                    }
                                }
                            }

                            // txtStatus.Text = "Listing fee is: " + listingFee + "\r\n" + txtStatus.Text;
                            txtStatus.Text = "\r\nThe item was listed successfully! " + txtStatus.Text + " ";
                            txtStatus.Update();

                            // Let's see if the posting already exists, if not save this posting. Later we'll save the pictures
                            String lpostingID = null;
                            SqlCommand lcmd = new SqlCommand("SELECT * FROM bsi_posting WHERE sku='" + xlProduct.SKU + "'", lconn);
                            SqlDataReader ldr = lcmd.ExecuteReader();
                            if (ldr.Read())
                            {
                                lpostingID = ldr["id"].ToString();
                            }
                            ldr.Close();
                            lcmd.Cancel();

                            if (chkOverridePosting.Checked && lpostingID != null)
                            {
                                // Override and overwrite the product info of the product
                                lcmd = new SqlCommand("UPDATE bsi_posting SET creationDate=@creationDate,gender=@gender,brand=@brand,style=@style,fullDescription=@fullDescription,keywords=@keywords,material=@material,color=@color,shade=@shade,heelHeight=@heelHeight WHERE id=" + lpostingID, lconn);
                                lcmd.Parameters.Add("@creationDate", SqlDbType.DateTime).Value = DateTime.Now;
                                lcmd.Parameters.Add("@brand", SqlDbType.NVarChar).Value = xlProduct.Brand;
                                lcmd.Parameters.Add("@gender", SqlDbType.NVarChar).Value = xlProduct.Gender;
                                lcmd.Parameters.Add("@style", SqlDbType.NVarChar).Value = xlProduct.Style;
                                lcmd.Parameters.Add("@fullDescription", SqlDbType.NVarChar).Value = xlProduct.FullDescription;
                                lcmd.Parameters.Add("@keywords", SqlDbType.NVarChar).Value = xlProduct.Keywords;
                                lcmd.Parameters.Add("@material", SqlDbType.NVarChar).Value = xlProduct.Material;
                                lcmd.Parameters.Add("@color", SqlDbType.NVarChar).Value = xlProduct.Color;
                                lcmd.Parameters.Add("@shade", SqlDbType.NVarChar).Value = xlProduct.Shade;
                                lcmd.Parameters.Add("@heelHeight", SqlDbType.NVarChar).Value = xlProduct.HeelHeight;

                                lcmd.ExecuteNonQuery();
                                lpostingID = null;
                            }

                            if ( lpostingID == null )
                               lpostingID = lda.InsertQuery(POSTING_STATUS_ACTIVE, xlProduct.SKU, DateTime.Now,
                                       "", xlProduct.Gender, xlProduct.Brand,
                                       xlProduct.Size, xlProduct.Width, xlProduct.Condition, xlProduct.Category, xlProduct.Style, xlProduct.FullDescription,
                                       xlProduct.Keywords, xlProduct.Material, xlProduct.Color, xlProduct.Shade, xlProduct.HeelHeight, 
                                       xlProduct.Title, (int)xlProduct.MarketPlaces, xlProduct.Variation,xlProduct.Widths,"","","","").ToString();

                            // Create posts
                            int lx = -1, litemID = -1;

                            // Do we need to create variations or do we need to make an individual post? 
                            if (xlProduct.Variation != VARIATIONS_NONE) 
                            {
                                // Create a post for each marketplace
                                lx = 0;                                
                                for (int li = 1; lx < ItemMarketplace.MARKETPLACE_MAXMARKETS; lx++, li <<= 1)
                                {
                                    if ((xlProduct.MarketPlaces & li) != 0)
                                    {
                                            // We'll create a single post for this item
                                            // Decimal lprice = xlProduct.getPriceForMarketplace(lx);
                                            Decimal lprice = xlProduct.Price; // 2013-01-02
                                            if (lprice == 0)
                                            {
                                                // The item was created without price, use the price of the first sibling
                                                foreach (EbayEntry lixl in xlProduct.Items)
                                                {
                                                    //if (lixl.getPriceForMarketplace(lx) > 0) 2013-01-02
                                                    if (lixl.Price > 0)
                                                    {
                                                        //lprice = lixl.getPriceForMarketplace(lx);
                                                        lprice = lixl.Price; // 2013-01-02
                                                        break;
                                                    }
                                                } // foreach
                                            }

                                            String lpostID = lposts_da.InsertQuery(Int32.Parse(lpostingID), li,
                                                             "", POSTING_STATUS_READY2PUBLISH, xlProduct.SKU,
                                                             xlProduct.Title,
                                                             lprice.ToString(),
                                                             xlProduct.StartDate, xlProduct.EndDate,
                                                             xlProduct.SellingFormat, 
                                                             "", "", "", "", "", "", "",
                                                             xlProduct.purchaseOrder,
                                                             xlProduct.listUser).ToString();

                                            // Now, let's create the quantities
                                            foreach (EbayEntry liex in xlProduct.Items)
                                            {
                                                // We need the item ID of the product
                                                // if (liex.getQuantityForMarketplace(lx) > 0) 2013-01-02
                                                if (liex.Quantity > 0)
                                                {
                                                    litemID = -1;
                                                    lcmd = new SqlCommand("SELECT ID,ItemLookupCode FROM item WHERE ItemLookupCode='" + liex.ItemLookupCode + "'", lconn);
                                                    ldr = lcmd.ExecuteReader();
                                                    if (ldr.Read())
                                                    {
                                                        litemID = int.Parse(ldr["ID"].ToString());
                                                    }
                                                    ldr.Close();
                                                    lcmd.Cancel();

                                                    // Create the quantity
                                                    lqtys_da.Insert(int.Parse(lpostID), litemID,
                                                                    liex.ItemLookupCode, liex.Title,
                                                                    liex.Size, liex.Width, liex.Color,
                                                                    /*liex.getQuantityForMarketplace(lx),
                                                                    liex.getPriceForMarketplace(lx), 2013-01-02*/
                                                                    liex.Quantity,
                                                                    liex.Price,
                                                                    xlProduct.purchaseOrder,
                                                                    xlProduct.listUser);
                                                }
                                            } // foreach (ItemExcel liex in xlProduct.Items)

                                    } // if ((xlProduct.MarketPlaces & li) != 0)
                                } // for (int li = 1; lx < ItemMarketplace.MARKETPLACE_MAXMARKETS; lx++, li <<= 1)

                            }
                            else
                            {
                                // Create an individual post for each market this item goes to
                                lx = 0;
                                for (int li = 1; lx < ItemMarketplace.MARKETPLACE_MAXMARKETS; lx++, li <<= 1)
                                {
                                    if ((xlProduct.MarketPlaces & li) != 0)
                                    {
                                        // Create the post in the marketplace
                                        String lpostID = lposts_da.InsertQuery(Int32.Parse(lpostingID), li,
                                                                               "", POSTING_STATUS_READY2PUBLISH, 
                                                                               xlProduct.SKU, xlProduct.Title,
                                                                               /*xlProduct.getPriceForMarketplace(lx).ToString(), 2013-01-02*/
                                                                               xlProduct.Price.ToString(),
                                                                               xlProduct.StartDate, xlProduct.EndDate,
                                                                               xlProduct.SellingFormat,
                                                                               "", "", "", "", "", "", "",
                                                                               xlProduct.purchaseOrder,
                                                                               xlProduct.listUser).ToString();

                                        // We need the item ID of the product
                                        litemID = -1;
                                        lcmd = new SqlCommand("SELECT ID,ItemLookupCode FROM item WHERE ItemLookupCode='" + xlProduct.ItemLookupCode + "'", lconn);
                                        ldr = lcmd.ExecuteReader();
                                        if (ldr.Read())
                                        {
                                            litemID = int.Parse(ldr["ID"].ToString());
                                        }
                                        ldr.Close();
                                        lcmd.Cancel();

                                        // Create the quantity
                                        lqtys_da.Insert(int.Parse(lpostID), litemID, 
                                                        xlProduct.ItemLookupCode, xlProduct.Title,
                                                        xlProduct.Size, xlProduct.Width, xlProduct.Color,
                                                        /*xlProduct.getQuantityForMarketplace(lx),
                                                        xlProduct.getPriceForMarketplace(lx),*/
                                                        xlProduct.Quantity,
                                                        xlProduct.Price,
                                                        xlProduct.purchaseOrder,
                                                        xlProduct.listUser);
                                    }
                                } // for

                            } // if (xlProduct.Variation != VARIATIONS_NONE)

                            // Finally, if everything went OK we'll upload the pictures to eBay. Then
                            // we'll update the posting information with the urls of the pictures

                            // Use this code to upload pictures to the eBay server
                            // first let's put all the pix URL in a single, comma delimited, string
                            if (currentMarketPlace.maskId > 8 && currentMarketPlace.maskId < 512)
                            {
                                String lpix = "pictures";

                                //SqlCommand lc2 = new SqlCommand("SELECT PICTURES FROM BSI_POSTING SET PICTURES='" + lpix + "' WHERE ID=" + lpostingID, lconn);

                                foreach (String lpic in xlProduct.Pictures)
                                {
                                    String lpicURL = uploadPicture(lpic);


                                    if (!String.IsNullOrEmpty(lpicURL))
                                    {
                                        lpix += " | " + lpicURL;
                                    }
                                } // foreach (String lpic in xlProduct.Pictures)

                                lpix = lpix.Replace("pictures | ", "");

                                SqlCommand lc2 = new SqlCommand("UPDATE BSI_POSTING SET PICTURES='" + lpix + "' WHERE ID=" + lpostingID, lconn);
                                txtStatus.Text = "\r\nPictures upload status:" +
                                                 lc2.ExecuteNonQuery().ToString() + " " +
                                                 txtStatus.Text;
                            }


                        }
                        catch (Exception pe)
                        {
                            _errors.Add(xlProduct);
                            //txtStatus.Text = pe.ToString() + "\r\n" + txtStatus.Text;
                            //if (MessageBox.Show("ERROR WHILE PUBLISHING ITEM:\r\n\r\n\r\n" + pe.ToString() +
                            //                    "\r\n\r\nDO YOU WANT TO STOP THE PROCESS?\r\n\r\n",
                            //                    "Error", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            //{
                            //    lstop = true;
                            //    break;
                            //};
                        };

                        txtStatus.Text = "\r\n" + txtStatus.Text;
                        _completed.Add(xlProduct);
                    } // foreach (ItemExcel in lproducts)
                } // foreach (uint lmarket in mktPlaces)

            btnStart.Enabled = true;

        } // publishProducts

        private void PublishEbayEntries()
        {
            berkeleyEntities dataContext = new berkeleyEntities();

            foreach (EbayEntry entry in _entries.Where(p => p.IsValid))
            {



                if(entry.Items.Count > 0)
                {
                    entry.Title = removeSize(entry.Title);

                    EbayListing listing = dataContext.EbayListings
                        .SingleOrDefault(p => p.Sku.Equals(entry.SKU) && p.Marketplace.ID == _marketplace.ID && p.Status.Equals("Active"));

                    if (listing == null)
                    {
                        listing = new EbayListing();
                        listing.Marketplace = _marketplace;
                        listing.Condition = entry.Condition;
                        listing.Duration
                    }
                }
                else
                {

                }

                
                ItemType listingDto = BuildItem(entry);

                if (entry.Items.Count == 0) // ONLY set price and QTY for individual products, not for Parents with children
                {
                    // Set a price and Q temporal
                    listingDto.Quantity = 1;
                    listingDto.StartPrice.Value = 99.99;

                }

                // Set one picture to the eBay product
                listingDto.PictureDetails = new PictureDetailsType();
                listingDto.PictureDetails.PictureURL = new StringCollection(new string[] { "http://www.tools4inet.com/0/products/tim/10061.jpg" });

                if ((currentMarketPlace.maskId > 8 && currentMarketPlace.maskId < 512)) // Publish only those who '8 < mask id < 512' (Not Amazons, nor websites)
                {
                    if (entry.SellingFormat == "A" || entry.Items.Count == 0)
                    {
                        VerifyAddItemCall api_AUCTION_Call = new VerifyAddItemCall(apiContext);
                        api_AUCTION_Call.VerifyAddItem(listingDto);
                    }
                    else
                    {
                        VerifyAddFixedPriceItemCall api_FP_Call = new VerifyAddFixedPriceItemCall(apiContext);
                        api_FP_Call.VerifyAddFixedPriceItem(listingDto);
                    }

                }

                


            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            btnStart.Enabled = true;

            // Read all the marketplaces
            SqlConnection lconn = null;
            berkeleyDataSetTableAdapters.bsi_marketplacesTableAdapter lda;
            
            try
            {
                lconn = new SqlConnection(Properties.Settings.Default.berkeleyConnectionString.ToString());
                lconn.Open();
                lda = new berkeleyDataSetTableAdapters.bsi_marketplacesTableAdapter();
                lda.Connection = lconn;
                lda.Fill(ldsMarkets); // This will fill sort by maskid
                foreach (berkeleyDataSet.bsi_marketplacesRow lmarketPlace in ldsMarkets.Rows)
                {
                    cmbMarkets.Items.Add(lmarketPlace);
                } // foreach
                cmbMarkets.DisplayMember = "name";
                cmbMarkets.SelectedIndex = EBAY_STARTINGINDEX; // Select the first one from ebay
            }
            catch (Exception pe)
            {
                MessageBox.Show("\nERROR WHILE READING MARKETPLACES: " + pe.ToString() + "\n", " Error on Load ");
            }
            finally
            {
                if ( lconn != null ) 
                {
                    lconn.Close();
                };
            }
        } // Form1_Load

        private string uploadPicture(string pfname)
        {
            string lurlpic = null;

            //read the image file as a byte array
            if (DEBUG_MODE) return null;

            System.IO.FileStream fs = new System.IO.FileStream(pfname, FileMode.Open, FileAccess.Read);
            fs.Seek(0, SeekOrigin.Begin);
            System.IO.BinaryReader br = new System.IO.BinaryReader(fs);

            byte[] image = br.ReadBytes((int)fs.Length);
            br.Close();
            fs.Close();

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://api.ebay.com/ws/api.dll");
            HttpWebResponse resp = null;

            string boundary = "MIME_boundary";
            string CRLF = "\r\n";

            //Add the request headers
            req.Headers.Add("X-EBAY-API-COMPATIBILITY-LEVEL", "515");
            req.Headers.Add("X-EBAY-API-DEV-NAME", Properties.Settings.Default.eBayDevID); //use your devid
            req.Headers.Add("X-EBAY-API-APP-NAME", Properties.Settings.Default.eBayAppID); //use your appid
            req.Headers.Add("X-EBAY-API-CERT-NAME", Properties.Settings.Default.eBayCertID); //use your certid
            req.Headers.Add("X-EBAY-API-SITEID", "0");
            req.Headers.Add("X-EBAY-API-DETAIL-LEVEL", "0");
            req.Headers.Add("X-EBAY-API-CALL-NAME", "UploadSiteHostedPictures");
            req.ContentType = "multipart/form-data; boundary=" + boundary;

            //set the method to POST
            req.Method = "POST";

            //set the HTTP version to 1.0
            req.ProtocolVersion = HttpVersion.Version10;

            //replace token with your own token
            string token = currentMarketPlace.eBayToken;

            //Construct the request
            string strReq1 = "--" + boundary + CRLF
                             + "Content-Disposition: form-data; name=document" + CRLF
                             + "Content-Type: text/xml; charset=\"UTF-8\"" + CRLF + CRLF
                             + "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
                             + "<UploadSiteHostedPicturesRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\">"
                             + "<RequesterCredentials>"
                             + "<eBayAuthToken>" + token + "</eBayAuthToken>"
                             + "</RequesterCredentials>"
                             + "<PictureSet>Supersize</PictureSet>"
                             + "</UploadSiteHostedPicturesRequest>"
                             + CRLF + "--" + boundary + CRLF
                             + "Content-Disposition: form-data; name=image; filename=image" + CRLF
                             + "Content-Type: application/octet-stream" + CRLF
                             + "Content-Transfer-Encoding: binary" + CRLF + CRLF;

            string strReq2 = CRLF + "--" + boundary + "--" + CRLF;

            //Convert the string to a byte array
            byte[] postDataBytes1 = System.Text.Encoding.ASCII.GetBytes(strReq1);
            byte[] postDataBytes2 = System.Text.Encoding.ASCII.GetBytes(strReq2);

            int len = postDataBytes1.Length + postDataBytes2.Length + image.Length;
            req.ContentLength = len;

            //Post the request to eBay
            System.IO.Stream requestStream = req.GetRequestStream();
            requestStream.Write(postDataBytes1, 0, postDataBytes1.Length);
            requestStream.Write(image, 0, image.Length);
            requestStream.Write(postDataBytes2, 0, postDataBytes2.Length);
            requestStream.Close();

            string response;
            try
            {
                // get response and write to console
                resp = (HttpWebResponse)req.GetResponse();

                StreamReader responseReader = new StreamReader(resp.GetResponseStream(), Encoding.UTF8);
                response = responseReader.ReadToEnd();
                resp.Close();

                //response contains our pictures url
                System.Xml.XmlDocument xml = new System.Xml.XmlDocument();
                xml.LoadXml(response);

                //Extract the FullURL from the response
                System.Xml.XmlNodeList list = xml.GetElementsByTagName("FullURL", "urn:ebay:apis:eBLBaseComponents");
                lurlpic = list[0].InnerText;

                /*
                 * Get the other elements from the response if required
                list = xml.GetElementsByTagName("PictureSet", "urn:ebay:apis:eBLBaseComponents");
                if ( list != null )
                    txtStatus.Text = "Result of PictureSet:" + list[0].InnerText + "\r\n" + txtStatus.Text; 
                */
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return lurlpic;
        } // uploadPicture

        bool deleteItemOnEbay(string pitemid)
        {
            bool lreturn = false;

            try
            {
                GetApiContext();
                EndItemCall lendItemCall = new EndItemCall(apiContext);
                lendItemCall.EndingReason = EndReasonCodeType.Incorrect;
                lendItemCall.ItemID = pitemid;
                lendItemCall.Execute();
                lreturn = true;
            }
            catch (Exception pe)
            {
                txtStatus.Text = "Error while deleting the product: " + pe.ToString() + "\r\n" + txtStatus.Text;
            }

            return lreturn;
        } // deleteItemOnEbay

        bool isTheProductOnWebsite(EbayEntry lix)
        {
            bool lreturn = true;

            using (SqlConnection lconn = new SqlConnection(Properties.Settings.Default.berkeleyConnectionString.ToString()))
            {
                lconn.Open();
                try
                {
                    String lcmdS = "SELECT thePost.id, thePost.marketplace,thePost.markerplaceitemid,thePost.status," +
                                   "theQ.postid,theQ.itemlookupcode,theQ.Title,theQ.size,theQ.width,theQ.quantity " +
                                   "FROM bsi_posts as thePost, " +
                                   "bsi_quantities as theQ " +
                                   "where theQ.postid=thePost.id and thePost.sku='" + lix.SKU +
                                   "' AND thePost.marketplace=512 AND (thePost.status=0 OR thePost.status=10)";
                    SqlCommand lc = new SqlCommand(lcmdS, lconn);
                    lc.Connection = lconn;

                    SqlDataReader lr = lc.ExecuteReader();
                    if (lr.Read())
                    {
                        // We need to create at least one item
                        String lpostid = lr["id"].ToString().Trim();
                        EbayEntry laux = new EbayEntry();
                        laux.copyNewItem(lix);
                        do
                        {
                            EbayEntry ltempxl = new EbayEntry(laux);
                            ltempxl.ItemLookupCode = lr["itemlookupcode"].ToString().Trim();
                            ltempxl.Title = lr["Title"].ToString().Trim();
                            ltempxl.Size = lr["Size"].ToString().Trim();
                            ltempxl.Width = lr["Width"].ToString().Trim();
                            // ltempxl.setPriceForMarketplace(lix.getPriceForMarketplace(lmarketplace), lmarketplace); 2013-01-02
                            ltempxl.Price = lix.Price;
                            int lq = 0;
                            int.TryParse(lr["Quantity"].ToString().Trim(), out lq);
                            // ltempxl.setQuantityForMarketplace(lq, lmarketplace); 2013-01-02
                            ltempxl.Quantity = lq;
                            laux.Items.Add(ltempxl);
                        } while (lr.Read());
                        lr.Close();

                        // Cancel the actual product and update its status
                        lc.CommandText = "UPDATE bsi_posts SET status=110 WHERE id=" + lpostid;
                        lc.ExecuteNonQuery();

                        // TO-DO: Cancel on Amazon...?

                        // Now combine both items... but first check if this item is single...
                        if ((lix.Items == null || lix.Items.Count < 1)) // If so, then we need to make it father w/1 child
                        {
                            EbayEntry ltempxl = new EbayEntry();
                            if (lix.Items.Count > 0)
                                ltempxl.copyNewItem(lix.Items[0]); // Copy from the first item
                            else
                                ltempxl.copyNewItem(lix);
                            lix.Title = removeSize(lix.Title);
                            //ltempxl.setPriceForMarketplace(lix.getPriceForMarketplace(lmarketplace), lmarketplace); 2013-01-02
                            ltempxl.Price = lix.Price;
                            lix.Items.Add(ltempxl);
                        }

                        foreach (EbayEntry lax in laux.Items)
                        {
                            EbayEntry lu = lix.Items.Find(delegate(EbayEntry pi)
                            {
                                return pi.ItemLookupCode == lax.ItemLookupCode;
                            });
                            if (lu != null)
                            {   /* 2013-01-02
                                int lnewQty = lu.getQuantityForMarketplace(lmarketplace) + 
                                               lax.getQuantityForMarketplace(lmarketplace);
                                lu.setQuantityForMarketplace(lnewQty, lmarketplace);
                                */
                                lu.Quantity = lu.Quantity + lax.Quantity;
                            }
                            else
                            {
                                lix.Items.Add(lax);
                            }
                        } // foreach
                        lix.Items.Sort(sortBySize);
                    }
                    lr.Close();
                }
                catch (Exception pe)
                {
                    txtStatus.Text = "Error while checking on Website: " + pe.ToString() + "\r\n" + txtStatus.Text;
                }
            } // using
            return lreturn;
        } // isTheProductOnWebsite

        bool isTheProductOnAmazon(EbayEntry lix)
        {
            bool lreturn = true;
            int lmarketplace = cmbMarkets.SelectedIndex;

            using (SqlConnection lconn = new SqlConnection(Properties.Settings.Default.berkeleyConnectionString.ToString()))
            {
                lconn.Open();
                try
                {
                    String lcmdS = "SELECT thePost.id, thePost.marketplace,thePost.markerplaceitemid,thePost.status," +
                                   "theQ.postid,theQ.itemlookupcode,theQ.Title,theQ.size,theQ.width,theQ.quantity " + 
                                   "FROM bsi_posts as thePost, " +
                                   "bsi_quantities as theQ " +
                                   "where theQ.postid=thePost.id and thePost.sku='" + lix.SKU +
                                   "' AND thePost.marketplace=1 AND (thePost.status=0 OR thePost.status=10)";
                    SqlCommand lc = new SqlCommand(lcmdS, lconn);
                    lc.Connection = lconn;

                    SqlDataReader lr = lc.ExecuteReader();
                    if (lr.Read())
                    {
                        // We need to create at least one item
                        String lpostid = lr["id"].ToString().Trim();
                        EbayEntry laux = new EbayEntry();
                        laux.copyNewItem(lix);
                        do
                        {
                            EbayEntry ltempxl = new EbayEntry(laux);
                            ltempxl.ItemLookupCode = lr["itemlookupcode"].ToString().Trim();
                            ltempxl.Title = lr["Title"].ToString().Trim();
                            ltempxl.Size = lr["Size"].ToString().Trim();
                            ltempxl.Width = lr["Width"].ToString().Trim();
                            // ltempxl.setPriceForMarketplace(lix.getPriceForMarketplace(lmarketplace), lmarketplace); 2013-01-02
                            ltempxl.Price = lix.Price;
                            int lq = 0;
                            int.TryParse(lr["Quantity"].ToString().Trim(), out lq);
                            // ltempxl.setQuantityForMarketplace(lq, lmarketplace); 2013-01-02
                            ltempxl.Quantity = lq;
                            laux.Items.Add(ltempxl);
                        } while (lr.Read());
                        lr.Close();

                        // Cancel the actual product and update its status
                        lc.CommandText = "UPDATE bsi_posts SET status=110 WHERE id=" + lpostid;
                        lc.ExecuteNonQuery();

                        // TO-DO: Cancel on Amazon...?

                        // Now combine both items... but first check if this item is single...
                        if ((lix.Items == null || lix.Items.Count < 1)) // If so, then we need to make it father w/1 child
                        {
                            EbayEntry ltempxl = new EbayEntry();
                            if (lix.Items.Count > 0)
                                ltempxl.copyNewItem(lix.Items[0]); // Copy from the first item
                            else
                                ltempxl.copyNewItem(lix);
                            lix.Title = removeSize(lix.Title);
                            //ltempxl.setPriceForMarketplace(lix.getPriceForMarketplace(lmarketplace), lmarketplace); 2013-01-02
                            ltempxl.Price = lix.Price;
                            lix.Items.Add(ltempxl);
                        }

                        foreach (EbayEntry lax in laux.Items)
                        {
                            EbayEntry lu = lix.Items.Find(delegate(EbayEntry pi)
                                               {
                                                   return pi.ItemLookupCode == lax.ItemLookupCode;
                                               });
                            if (lu != null)
                            {   /* 2013-01-02
                                int lnewQty = lu.getQuantityForMarketplace(lmarketplace) + 
                                               lax.getQuantityForMarketplace(lmarketplace);
                                lu.setQuantityForMarketplace(lnewQty, lmarketplace);
                                */
                                lu.Quantity = lu.Quantity + lax.Quantity;
                            }
                            else
                            {
                                lix.Items.Add(lax);
                            }
                        } // foreach
                        lix.Items.Sort(sortBySize);
                    }
                    lr.Close();
                }
                catch (Exception pe)
                {
                    txtStatus.Text = "Error while checking on Amazon: " + pe.ToString() + "\r\n" + txtStatus.Text;
                }
            } // using

            return lreturn;
        } // isTheProductOnAmazon

        bool isTheProductOnEbay(EbayEntry lix)
        {
            bool lreturn = false;
            SqlConnection lconn = null;
            SqlCommand lcmd = null;

            // Look for the item
            ItemType litem = null;
            int lmarketplace = EBAY_STARTINGINDEX + cmbMarkets.SelectedIndex;

            litem = itemsOnline[cmbMarkets.SelectedIndex].
                                Find(
                                      delegate(ItemType pi)
                                      {
                                          bool lf = pi.SKU == lix.SKU;
                                          if (!lf)
                                          {
                                              // Maybe we are a parent (SKU w/o size) and this one is a child (SKU w/size)
                                              try
                                              {
                                                  String[] lsplittedsku = pi.SKU.Split(new char[] { '-' });
                                                  lf = (lix.SKU == lsplittedsku[0]);
                                              }
                                              catch (Exception pe)
                                              {
                                                  String lsku = (pi.SKU != null) ? pi.SKU : pi.ItemID;
                                                  MessageBox.Show("Error searching: " + lix.SKU + " caused by: " + lsku + " - " + pe.ToString());
                                              }
                                          }
                                          return lf;
                                      }
                                    );

            if (litem == null)
            {
                return lreturn;
            }


            txtStatus.Text = "Product is already listed... " + txtStatus.Text;
            lreturn = true;

            // If we found the item on eBay then we need to make our item a father if it is a single product
            if ((lix.Items == null || lix.Items.Count < 1) && !lix.SellingFormat.Contains('A'))
            {
                EbayEntry ltempxl = new EbayEntry();
                if (lix.Items.Count > 0)
                    ltempxl.copyNewItem(lix.Items[0]); // Copy from the first item
                else
                    ltempxl.copyNewItem(lix);
                lix.Title = removeSize(lix.Title);
                //ltempxl.setPriceForMarketplace(lix.getPriceForMarketplace(lmarketplace), lmarketplace); 2013-01-02
                ltempxl.Price = lix.Price;
                lix.Items.Add(ltempxl);
            }

            switch (lix.SellingFormat)
            {
                case "A":
                case "A3":
                case "A5":
                    // Simply add qty to current item on our DB, nothing else.

                    if (lix.ItemLookupCode == litem.SKU && litem.ListingType == ListingTypeCodeType.Chinese)
                    {
                        txtStatus.Text = "\r\n\r\nWARNING! PLEASE NOTE: eBAY ITEM [" + lix.ItemLookupCode + "] IS ALREADY IN AUCTION. REVIEW AND TRY TO PUBLISH IT AGAIN.\r\n\r\n" + txtStatus.Text;
                    }

                    break;
                default:
                    bool lfoundFlag = false; // We'll use this to look for items
                    if (litem.ListingType != ListingTypeCodeType.Chinese)
                    {
                        // Cancel the eBay product
                        deleteItemOnEbay(litem.ItemID);

                        // Update the current item published with the eBay item id
                        try
                        {
                            string lscmd = "UPDATE bsi_posts SET status=110 WHERE markerplaceItemID='" + litem.ItemID + "'";
                            lconn = new SqlConnection(Properties.Settings.Default.berkeleyConnectionString.ToString());
                            lconn.Open();

                            lcmd = new SqlCommand(lscmd, lconn);
                            lcmd.ExecuteNonQuery();
                            lcmd.Cancel();
                        }
                        catch (Exception pe)
                        {
                            txtStatus.Text = "Error while trying to update our database: " + pe.ToString() + "\r\n" + txtStatus.Text;
                        }
                        finally
                        {
                            if ( lcmd != null ) lcmd.Cancel();
                            if ( lconn != null ) lconn.Close();
                        }

                        // Combine both products into one
                        // Ours is a parent with children, let's add the kid(s) of the found one
                        if (litem.Variations == null)
                        {
                            // Let's see if we already have this item
                            lfoundFlag = false;
                            foreach (EbayEntry lax in lix.Items)
                            {
                                if (lax.ItemLookupCode == litem.SKU.Trim())
                                {
                                    lfoundFlag = true;
                                    /*int lqx = lax.getQuantityForMarketplace(lmarketplace); 2013-01-02
                                    lax.setQuantityForMarketplace(lqx+litem.Quantity - litem.SellingStatus.QuantitySold,lmarketplace);*/
                                    lax.Quantity = lax.Quantity+litem.Quantity - litem.SellingStatus.QuantitySold;
                                    break; // Get out of the loop
                                }
                            } // foreach

                            if (!lfoundFlag) // Create and add the size
                            {
                                EbayEntry ltempxl = new EbayEntry();
                                if (lix.Items.Count > 0)
                                    ltempxl.copyNewItem(lix.Items[0]); // Copy from the first item
                                else
                                    ltempxl.copyNewItem(lix);

                                // Set the size and width
                                ltempxl.ItemLookupCode = litem.SKU;
                                String[] lprodinfo = litem.SKU.Split(new char[] { '-' });
                                if (lprodinfo.Length > 2)
                                {
                                    ltempxl.Title = litem.Title.Trim();
                                    ltempxl.Size = lprodinfo[1];
                                    ltempxl.Width = lprodinfo[2];

                                    /* 2013-01-02
                                    ltempxl.setPriceForMarketplace(lix.getPriceForMarketplace(lmarketplace), lmarketplace);
                                    ltempxl.setQuantityForMarketplace(litem.Quantity - litem.SellingStatus.QuantitySold,lmarketplace);
                                    */
                                    ltempxl.Price = lix.Price;
                                    ltempxl.Quantity = litem.Quantity - litem.SellingStatus.QuantitySold;

                                    lix.Items.Add(ltempxl);
                                    lix.Items.Sort(sortBySize);
                                }
                            }
                        }
                        else
                        {
                            // We have a parent with kids and we'll add more kids. We need to check one by one
                            foreach (VariationType lnewKidOneBay in litem.Variations.Variation)
                            {
                                lfoundFlag = false;
                                foreach (EbayEntry lax in lix.Items)
                                {
                                    if (lax.ItemLookupCode == lnewKidOneBay.SKU.Trim())
                                    {
                                        lfoundFlag = true;
                                        /* 2013-01-02
                                        int lqx = lax.getQuantityForMarketplace(lmarketplace);
                                        lax.setQuantityForMarketplace(lqx+lnewKidOneBay.Quantity - lnewKidOneBay.SellingStatus.QuantitySold,lmarketplace);
                                        */
                                        lax.Quantity = lax.Quantity + (lnewKidOneBay.Quantity - lnewKidOneBay.SellingStatus.QuantitySold);
                                        break; // Get out of the loop
                                    }
                                } // foreach

                                if (!lfoundFlag) // Create the size
                                {
                                    EbayEntry ltempxl = new EbayEntry();
                                    if ( lix.Items != null && lix.Items.Count > 0 )
                                        ltempxl.copyNewItem(lix.Items[0]); // lix.Items[0].clone(); 
                                    else
                                        ltempxl.copyNewItem(lix);

                                    // Set the size and width
                                    String[] lprodinfo = lnewKidOneBay.SKU.Split(new char[] { '-' });
                                    if (lprodinfo.Length > 2)
                                    {
                                        ltempxl.ItemLookupCode = lnewKidOneBay.SKU;
                                        ltempxl.Title = (lnewKidOneBay.VariationTitle != null) ? lnewKidOneBay.VariationTitle.Trim() : "";
                                        ltempxl.Size = lprodinfo[1];
                                        ltempxl.Width = lprodinfo[2];
                                        /*
                                        ltempxl.setPriceForMarketplace(lix.getPriceForMarketplace(lmarketplace),lmarketplace);                                            
                                        ltempxl.setQuantityForMarketplace(lnewKidOneBay.Quantity - lnewKidOneBay.SellingStatus.QuantitySold,lmarketplace);
                                        */
                                        ltempxl.Price = lix.Price;
                                        ltempxl.Quantity = lnewKidOneBay.Quantity - lnewKidOneBay.SellingStatus.QuantitySold;
                                        lix.Items.Add(ltempxl);
                                        lix.Items.Sort(sortBySize);
                                    }
                                }
                            } // for each
                        } // if (litem.Variations != null)
                    } // if (litem.ListingType != ListingTypeCodeType.Chinese)
                    break;
            } // switch

            return lreturn;

        } // isTheProductOnEbay

        private void btnUpdateMarketplaces_Click(object sender, EventArgs e)
        {
            UpdateMarketplaces();
        } // btnUpdateMarketplaces_Click


        private void cmbMarkets_SelectedIndexChanged(object sender, EventArgs e)
        {
            chkPublishWOPics.Enabled = (cmbMarkets.SelectedIndex < EBAY_STARTINGINDEX || cmbMarkets.SelectedIndex >= WEB_STARTINGINDEX);
            if (cmbMarkets.SelectedIndex >= EBAY_STARTINGINDEX && cmbMarkets.SelectedIndex < WEB_STARTINGINDEX) chkPublishWOPics.Checked = false;
        } // cmbMarkets_SelectedIndexChanged

        private void eBayPageSizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmEbayPageSize lf = new frmEbayPageSize();

            lf.ShowDialog();
        } // eBayPageSizeToolStripMenuItem_Click

    } // partial class Form1
} // namespace BSI_InventoryPreProcessor
