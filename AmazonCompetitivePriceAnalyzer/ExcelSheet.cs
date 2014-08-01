using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Packaging;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Globalization;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using MarketplaceWebServiceProducts;
using MarketplaceWebServiceProducts.Model;
using BerkeleyEntities;
using System.Timers;
using System.Threading;

namespace AmznPriceComparator
{
    public class ExcelSheet
    {
        const string COLUMN_SKU = "SKU";

        private SharedStringTablePart _stringTablePart;
        private WorksheetPart _workSheetPart;

        private AmznMarketplace _marketplace;

        private Dictionary<string, string> _colHeadersToRefs = new Dictionary<string, string>();
        private Dictionary<string, string> _colRefsToHeaders = new Dictionary<string, string>();
        private Dictionary<string, PropertyInfo> _colRefsToProp = new Dictionary<string, PropertyInfo>();
        private Dictionary<string, uint> _colRefsToStyle = new Dictionary<string, uint>();

        private int _getMatchingProductQuota = 100;
        private int _getCompetitivePriceQuota = 200;

        private System.Timers.Timer _getMatchingProductRestore = new System.Timers.Timer(1000);
        private System.Timers.Timer _getCompetitivePriceRestore = new System.Timers.Timer(1000);


        public ExcelSheet(string path)
        {
            this.Path = path;

            _getCompetitivePriceRestore.Elapsed += new ElapsedEventHandler(getCompetitivePriceRestore_Elapsed);
            _getMatchingProductRestore.Elapsed += new ElapsedEventHandler(getMatchingProductRestore_Elapsed);

            _getCompetitivePriceRestore.Start();
            _getMatchingProductRestore.Start();
        }

        public string Path { get; set; }

        public void UpdateSheet()
        {
            var entries = ReadEntries();

            GetProductData(entries);

            GetASINData(entries.Where(p => p.UPC != null));

            GetCompetitivePricingData(entries.Where(p => p.ASIN != null));

            PersistEntries(entries);
        }

        private void GetProductData(IEnumerable<Entry> entries)
        {
            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                _marketplace = dataContext.AmznMarketplaces.Single(p => p.ID == 1);

                foreach (Entry entry in entries)
                {
                    entry.UPC = dataContext.Items.Single(p => p.ItemLookupCode.Equals(entry.Sku)).GTIN;
                }
            }
        }

        private void GetASINData(IEnumerable<Entry> entries)
        {
            Queue<Entry> pendingList = new Queue<Entry>(entries);

            while (pendingList.Count > 0)
            {
                List<string> idList = new List<string>();

                for (int i = 0; i < 5; i++)
                {
                    if (pendingList.Count > 0)
                    {
                        idList.Add(pendingList.Dequeue().UPC);
                    }
                }

                if (idList.Count > 0)
                {
                    GetMatchingProductForIdRequest request = new GetMatchingProductForIdRequest();
                    request.SellerId = _marketplace.MerchantId;
                    request.MarketplaceId = _marketplace.MarketplaceId;
                    request.IdList = new IdListType();
                    request.IdList.Id = idList;
                    request.IdType = "UPC";


                    while (_getMatchingProductQuota < 5)
                    {
                        Thread.Sleep(2000);
                    }

                    GetMatchingProductForIdResponse response = _marketplace.GetMWSProductsClient().GetMatchingProductForId(request);

                    _getMatchingProductQuota = _getMatchingProductQuota - 5;

                    if (response.IsSetGetMatchingProductForIdResult())
                    {
                        foreach (GetMatchingProductForIdResult result in response.GetMatchingProductForIdResult)
                        {
                            if (result.status.Equals("Success") && result.IsSetProducts() && result.Products.IsSetProduct())
                            {
                                Entry entry = entries.Where(p => p.UPC != null).Single(p => p.UPC.Equals(result.Id));
                                entry.ASIN = result.Products.Product.First().Identifiers.MarketplaceASIN.ASIN;
                            }
                        }
                    }
                }
            }
        }

        private void GetCompetitivePricingData(IEnumerable<Entry> entries)
        {
            Queue<Entry> pendingList = new Queue<Entry>(entries);

            while (pendingList.Count > 0)
            {
                List<string> idList = new List<string>();

                for (int i = 0; i < 20; i++)
                {
                    if (pendingList.Count > 0)
                    {
                        idList.Add(pendingList.Dequeue().ASIN);
                    }
                }

                if (idList.Count > 0)
                {
                    GetCompetitivePricingForASINRequest request = new GetCompetitivePricingForASINRequest();
                    request.SellerId = _marketplace.MerchantId;
                    request.MarketplaceId = _marketplace.MarketplaceId;
                    request.ASINList = new ASINListType();
                    request.ASINList.ASIN = idList;

                    while (_getCompetitivePriceQuota < 20)
                    {
                        Thread.Sleep(2000);
                    }

                    GetCompetitivePricingForASINResponse response = _marketplace.GetMWSProductsClient().GetCompetitivePricingForASIN(request);

                    _getCompetitivePriceQuota = _getCompetitivePriceQuota - 20;

                    if (response.IsSetGetCompetitivePricingForASINResult())
                    {
                        foreach (GetCompetitivePricingForASINResult result in response.GetCompetitivePricingForASINResult)
                        {
                            if (result.status.Equals("Success") && result.IsSetProduct())
                            {
                                Entry entry = entries.Where(p => p.ASIN != null).Single(p => p.ASIN.Equals(result.ASIN));

                                CompetitivePricingType cpt = result.Product.CompetitivePricing;

                                if (cpt.NumberOfOfferListings.OfferListingCount.Count > 0)
                                {
                                    entry.OfferCount = cpt.NumberOfOfferListings.OfferListingCount.First().Value;
                                }

                               
                                if (cpt.CompetitivePrices.CompetitivePrice.Count > 0)
                                {
                                    CompetitivePriceType cpt2 = cpt.CompetitivePrices.CompetitivePrice.First();

                                    entry.OurPrice = cpt2.belongsToRequester;

                                    entry.LandedPrice = cpt2.Price.LandedPrice.Amount;
                                    entry.ListingPrice = cpt2.Price.ListingPrice.Amount;
                                    entry.Shipping = cpt2.Price.Shipping.Amount; 
                                }
                            }
                        }
                    }
                }
            }
        }

        private List<Entry> ReadEntries()
        {
            List<Entry> lines = new List<Entry>();

            using (SpreadsheetDocument document = SpreadsheetDocument.Open(this.Path, true))
            {
                Initialize(document);

                var rows = _workSheetPart.Worksheet
                    .Descendants<Cell>()
                    .Where(c => string.Compare(ParseColRefs(c.CellReference.Value), _colHeadersToRefs[COLUMN_SKU], true) == 0 && !string.IsNullOrWhiteSpace(GetCellValue(c)))
                    .Select(p => p.Parent).Cast<Row>().Where(p => p.RowIndex != 1);

                foreach (Row row in rows)
                {
                    lines.Add(CreateEntry(row));
                }

                document.Close();
            }

            return lines;
        }

        private void PersistEntries(IEnumerable<Entry> entries)
        {
            using (SpreadsheetDocument document = SpreadsheetDocument.Open(this.Path,true))
            {
                this.Initialize(document);

                SheetData sheetData = new SheetData();

                Row headerRow = _workSheetPart.Worksheet.Descendants<Row>().Single(p => p.RowIndex == 1).Clone() as Row;

                sheetData.Append(headerRow);

                var colRefs = _colRefsToHeaders.Keys.OrderBy(p => p);

                foreach(Entry entry in entries.OrderBy(p => p.RowIndex))
                {
                    Row row = new Row();
                    row.RowIndex = new UInt32Value(entry.RowIndex);

                    foreach (string colRef in colRefs)
                    {
                        row.Append(this.CreateCell(entry, colRef));
                    }

                    sheetData.Append(row);
                }

                _workSheetPart.Worksheet = new Worksheet(sheetData);

                document.Close(); 
            }
        }

        private Entry CreateEntry(Row row)
        {
            Entry entry = new Entry();
            entry.RowIndex = row.RowIndex.Value;

            foreach (Cell cell in row.OfType<Cell>())
            {
                string colRef = this.ParseColRefs(cell.CellReference);
                string cellValue = this.GetCellValue(cell);

                if (_colRefsToProp.ContainsKey(colRef) && !string.IsNullOrWhiteSpace(cellValue))
                {
                    PropertyInfo prop = _colRefsToProp[colRef];

                    switch (prop.PropertyType.Name)
                    {
                        case "String":
                            prop.SetValue(entry, cellValue, null); break;
                        case "Int32":
                            prop.SetValue(entry, Convert.ToInt32(cellValue), null); break;
                        case "Decimal":
                            prop.SetValue(entry, Convert.ToDecimal(cellValue), null); break;
                        default:
                             break;
                    }
                }               
            }

            return entry;
        }

        private Cell CreateCell(Entry entry, string colRef)
        {
            Cell cell = new Cell();
            cell.CellReference = new StringValue(colRef + entry.RowIndex.ToString());

            if(_colRefsToStyle.ContainsKey(colRef))
            {
                cell.StyleIndex = new UInt32Value(_colRefsToStyle[colRef]);
            }

            if (_colRefsToProp.ContainsKey(colRef))
            {
                PropertyInfo prop = _colRefsToProp[colRef];
                object value = prop.GetValue(entry, null);

                if (value != null)
                {
                    switch (prop.PropertyType.Name)
                    {
                        case "String":
                            cell.CellValue = new CellValue(InsertSharedString(value.ToString()).ToString());
                            cell.DataType = CellValues.SharedString; break;
                        case "Int32":
                        case "Decimal":
                            cell.CellValue = new CellValue(value.ToString());
                            cell.DataType = CellValues.Number; break;
                        case "DateTime":
                            cell.CellValue = new CellValue(value.ToString());
                            cell.DataType = CellValues.Date; break;
                        default:
                            cell.CellValue = new CellValue(InsertSharedString(value.ToString()).ToString());
                            cell.DataType = CellValues.SharedString; break; ;
                    }
                }
                else
                {
                    cell.CellValue = new CellValue("");
                    cell.DataType = CellValues.String;
                }
            }
            else
            {
                cell.CellValue = new CellValue("");
                cell.DataType = CellValues.String;
            }

            return cell;
        }

        private void Initialize(SpreadsheetDocument doc)
        {
            WorkbookPart wbPart = doc.WorkbookPart;
            Sheet sheet = doc.WorkbookPart.Workbook.Sheets.GetFirstChild<Sheet>();

            _workSheetPart = wbPart.GetPartById(sheet.Id) as WorksheetPart;
            _stringTablePart = wbPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();

            this.RegisterColumns(_workSheetPart.Worksheet.Descendants<Row>().Single(p => p.RowIndex == 1));

            Row sampleRow = _workSheetPart.Worksheet.Descendants<Row>().Single(p => p.RowIndex == 2);

            foreach (Cell cell in sampleRow.Elements<Cell>())
            {
                string colRef = this.ParseColRefs(cell.CellReference.Value);
                _colRefsToStyle.Add(colRef, cell.StyleIndex != null ? cell.StyleIndex.Value : 0);
            }

        }

        private void RegisterColumns(Row headerRow)
        {
            _colRefsToHeaders.Clear();
            _colHeadersToRefs.Clear();
            _colRefsToProp.Clear();
            _colRefsToStyle.Clear();

            //var targetCols = new List<string>()
            //{
            //    COLUMN_MARKETPLACE, COLUMN_PRICE, COLUMN_TYPE, COLUMN_QTY, COLUMN_TITLE,
            //    COLUMN_QTYAVAIL, COLUMN_STATUS, COLUMN_CONDITION, COLUMN_LISTINGID,
            //    COLUMN_BRAND, COLUMN_COST, COLUMN_SKU, COLUMN_ONHAND
            //};

            foreach (Cell cell in headerRow.OfType<Cell>())
            {
                string header = this.GetCellValue(cell).ToUpper().Trim();
                string columnRef = this.ParseColRefs(cell.CellReference.Value);

                _colRefsToHeaders.Add(columnRef, header);
                _colHeadersToRefs.Add(header, columnRef);
            }

            PropertyInfo[] props = typeof(Entry).GetProperties();

            foreach (string colRef in _colRefsToHeaders.Keys)
            {
                PropertyInfo prop = props.SingleOrDefault(p => string.Compare(
                    p.Name, _colRefsToHeaders[colRef],
                    CultureInfo.CurrentCulture,
                    CompareOptions.IgnoreSymbols | CompareOptions.IgnoreCase
                    ) == 0);

                if (prop != null)
                {
                    _colRefsToProp.Add(colRef, prop);
                }
            }

        }

        private int InsertSharedString(string text)
        {
            int i = 0;
            // Iterate through all the items in the SharedStringTable. If the text already exists, return its index.
            foreach (SharedStringItem item in _stringTablePart.SharedStringTable.Elements<SharedStringItem>())
            {
                if (item.InnerText == text)
                {
                    return i;
                }

                i++;
            }
            // The text does not exist in the part. Create the SharedStringItem and return its index.
            _stringTablePart.SharedStringTable.AppendChild(new SharedStringItem(new Text(text)));
            return i;
        }

        private string GetCellValue(Cell cell)
        {
            if (cell != null && cell.CellValue != null)
            {
                if (cell.DataType != null && cell.DataType.Value.Equals(CellValues.SharedString))
                {
                    int strIndex = int.Parse(cell.CellValue.Text);
                    string value = _stringTablePart.SharedStringTable.ElementAt(strIndex).InnerText;
                    return value;
                }
                else
                {
                    return cell.CellValue.Text;
                }
            }

            return null;
        }

        private string ParseColRefs(string cellRef)
        {
            // Create a regular expression to match the column name portion of the cell name.
            Regex regex = new Regex("[A-Za-z]+");
            Match match = regex.Match(cellRef);

            return match.Value;
        }

        private Cell GetCell(Row row, string colRef)
        {
            SheetData sheetData = _workSheetPart.Worksheet.GetFirstChild<SheetData>();
            string cellRef = colRef + row.RowIndex.Value.ToString();

            // If there is not a cell with the specified column name, insert one.
            if (row.Elements<Cell>().Where(c => c.CellReference.Value == cellRef).Count() > 0)
            {
                return row.Elements<Cell>().Where(c => c.CellReference.Value == cellRef).First();
            }
            else
            {
                // Cells must be in sequential order according to CellReference. Determine where to insert the new cell.
                Cell refCell = null;
                foreach (Cell cell in row.Elements<Cell>())
                {
                    if (string.Compare(cell.CellReference.Value, cellRef, true) > 0)
                    {
                        refCell = cell;
                        break;
                    }
                }

                Cell newCell = new Cell() { CellReference = cellRef, StyleIndex = 0 };
                row.InsertBefore(newCell, refCell);

                return newCell;
            }
        }

        private void getMatchingProductRestore_Elapsed(object sender, ElapsedEventArgs e)
        {

            if (_getMatchingProductQuota < 100)
            {
                int temp = _getMatchingProductQuota + 5;

                if(temp > 100)
                {
                    temp = 100;
                }

                _getMatchingProductQuota = temp;
            }
          

            
        }

        private void getCompetitivePriceRestore_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_getCompetitivePriceQuota < 200)
            {
                int temp = _getCompetitivePriceQuota + 10;

                if (temp > 200)
                {
                    temp = 200;
                }

                _getCompetitivePriceQuota = temp;
            }
        }

    }
}
