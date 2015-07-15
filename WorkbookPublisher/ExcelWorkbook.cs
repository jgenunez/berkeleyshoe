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
using BerkeleyEntities;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.ComponentModel;
using System.IO;
using System.Drawing;

namespace WorkbookPublisher
{
    public class ExcelWorkbook
    {
        const string TITLEMAP_PATH = @"P:\Publishing\TitleMap.xlsx";
        const string COLUMN_SKU = "SKU";
        const string COLUMN_FORMAT = "FORMAT";
        const string COLUMN_MESSAGE = "MESSAGE";

        public ExcelWorkbook(string path)
        {
            this.Path = path;
        }

        public string Path { get; set; }

        public void UpdateSheet(List<BaseEntry> entries, Type entryType, string sheetName)
        {
            using (SpreadsheetDocument document = SpreadsheetDocument.Open(this.Path, true))
            {
                SharedStringTablePart stringTablePart = document.WorkbookPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();

                var sheets = document.WorkbookPart.Workbook.Sheets.Elements<Sheet>();

                Sheet targetSheet = sheets.SingleOrDefault(p => p.Name.Value.ToUpper().Equals(sheetName));

                if (targetSheet != null)
                {
                    WorksheetPart mainWorksheetPart = document.WorkbookPart.GetPartById(targetSheet.Id) as WorksheetPart;

                    //if (mainWorksheetPart.TableDefinitionParts.Count() > 0)
                    //{
                    //    mainWorksheetPart.TableDefinitionParts
                    //}

                    //var test = mainWorksheetPart.TableDefinitionParts;

                    Worksheet worksheet = mainWorksheetPart.Worksheet;

                    Row headerRow = worksheet.Descendants<Row>().Single(p => p.RowIndex.Value == 1);

                    RowMapper mapper = new RowMapper(entryType, headerRow, stringTablePart.SharedStringTable);

                    var sheetData = worksheet.GetFirstChild<SheetData>();

                    //var emptyRows = sheetData.Elements<Row>()
                    //    .Where(p => 
                    //        p.Descendants<Cell>().Count() == 0 || 
                    //        p.Descendants<Cell>().All(s => string.IsNullOrWhiteSpace(mapper.GetCellValue(s)))
                    //        ).ToList();

                    //emptyRows.ForEach(p => sheetData.RemoveChild<Row>(p));

                    var rows = sheetData.Elements<Row>();

                    uint lastRowIndex = rows.Max(p => p.RowIndex.Value);

                    foreach (var entry in entries.OrderBy(p => p.RowIndex))
                    {
                        if (entry.RowIndex != 0)
                        {
                            Row row = rows.SingleOrDefault(p => p.RowIndex.Value == entry.RowIndex);
                            
                            mapper.UpdateRow(entry, row);
                        }
                        else
                        {
                            lastRowIndex++;

                            Row row = rows.Single(p => p.RowIndex.Value == entry.ParentRowIndex).CloneNode(true) as Row;

                            row.RowIndex = new UInt32Value(lastRowIndex);

                            foreach (Cell cell in row.Elements<Cell>())
                            {
                                cell.CellReference.InnerText = cell.CellReference.InnerText.Replace(entry.ParentRowIndex.ToString(), lastRowIndex.ToString());

                                if (cell.CellFormula != null)
                                {
                                    cell.CellFormula = null;
                                }
                            }

                            sheetData.AppendChild<Row>(row);

                            mapper.UpdateRow(entry, row);
                        }
                    }

                }

                document.Close();
            }
        }

        public List<object> ReadSheet(Type entryType, string sheetName)
        {
            List<object> entries = new List<object>();

            using (SpreadsheetDocument document = SpreadsheetDocument.Open(this.Path, false))
            {
                SharedStringTablePart stringTablePart = document.WorkbookPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();

                Sheet sheet = document.WorkbookPart.Workbook.Sheets.Descendants<Sheet>().SingleOrDefault(p => p.Name.Value.ToUpper().Equals(sheetName));

                if (sheet != null)
                {
                    WorksheetPart worksheetPart = document.WorkbookPart.GetPartById(sheet.Id) as WorksheetPart;

                    var headerRow = worksheetPart.Worksheet.Descendants<Row>().Single(p => p.RowIndex.Value == 1);

                    RowMapper mapper = new RowMapper(entryType, headerRow, stringTablePart.SharedStringTable);

                    var targetProps = entryType.GetProperties();

                    foreach (Row row in worksheetPart.Worksheet.Descendants<Row>().Where(p => p.RowIndex.Value != 1))
                    {
                        object entry = mapper.MapRow(row);

                        entries.Add(entry);      
                    }
                }

                document.Close();
            }

            return entries;
        }

        public List<ListingEntry> UpdateEntries(IEnumerable<ListingEntry> entries, Type entryType, string sheetName)
        {
            List<ListingEntry> newEntries = new List<ListingEntry>();

            switch (entryType.Name)
            {
                case "BonanzaEntry":

                    var bnzUpdater = new BnzEntryUpdater(entries.Cast<BonanzaEntry>().ToList(), sheetName);
                    newEntries = bnzUpdater.Update().Cast<ListingEntry>().ToList(); break;

                case "EbayEntry":

                    var ebayUpdater = new EbayEntryUpdater(entries.Cast<EbayEntry>().ToList(), sheetName);
                    newEntries = ebayUpdater.Update().Cast<ListingEntry>().ToList(); break;

                case "AmznEntry":

                    var amznUpdater = new AmznEntryUpdater(entries.Cast<AmznEntry>().ToList(), sheetName);
                    newEntries = amznUpdater.Update().Cast<ListingEntry>().ToList(); break;
            }

            return newEntries;
        }

        public List<TitleMapRule> ReadTitleMapRules()
        {
            List<TitleMapRule> rules = new List<TitleMapRule>();

            using (SpreadsheetDocument document = SpreadsheetDocument.Open(TITLEMAP_PATH, false))
            {
                SharedStringTablePart stringTablePart = document.WorkbookPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();

                Sheet sheet = document.WorkbookPart.Workbook.Sheets.Descendants<Sheet>().First();

                if (sheet != null)
                {
                    WorksheetPart worksheetPart = document.WorkbookPart.GetPartById(sheet.Id) as WorksheetPart;

                    var headerRow = worksheetPart.Worksheet.Descendants<Row>().Single(p => p.RowIndex.Value == 1);

                    RowMapper mapper = new RowMapper(typeof(TitleMapRule), headerRow, stringTablePart.SharedStringTable );

                    foreach (Row row in worksheetPart.Worksheet.Descendants<Row>().Where(p => p.RowIndex.Value != 1))
                    {
                        TitleMapRule rule = mapper.MapRow(row) as TitleMapRule;

                        rules.Add(rule);
                    }
                }

                document.Close();
            }

            return rules;
        }

        //private WorksheetPart CreateWorksheet(WorkbookPart workbookPart, string name)
        //{
        //    WorksheetPart newWorksheetPart = workbookPart.AddNewPart<WorksheetPart>();

        //    uint sheetId = 1;

        //    var sheets = workbookPart.Workbook.Sheets.Elements<Sheet>();

        //    if (sheets.Count() > 0)
        //    {
        //        sheetId = sheets.Select(s => s.SheetId.Value).Max() + 1;
        //    }

        //    string relationshipId = workbookPart.GetIdOfPart(newWorksheetPart);

        //    // Append the new worksheet and associate it with the workbook.
        //    Sheet sheet = new Sheet() { Id = relationshipId, SheetId = sheetId, Name = name };

        //    workbookPart.Workbook.Sheets.Append(sheet);

        //    return newWorksheetPart;
        //}

    }

    public class TitleMapRule : BaseEntry
    {
        public string Department { get; set; }

        public string Category { get; set; }

        public string Map { get; set; }
    }

    public enum StatusCode { Pending , Processing, Error, Completed };

    public abstract class BaseEntry
    {
        public uint RowIndex { get; set; }

        public uint ParentRowIndex { get; set; }
    }

    public class MainEntry : BaseEntry
    {
        public string Brand { get; set; }        
        public string ClassName { get; set; }
        public string Sku { get; set; }

        public int PictureCount { get; set; }
        public int Qty { get; set; }
        public decimal Cost { get; set; }

        public string UPC { get; set; }

        public string Title { get; set; }
        public string TitleFormula { get; set; }

        public string AmznDescription { get; set; }
        public string AmznTitle { get; set; }
        public decimal AmznPrice { get; set; }

        public string FullDescription { get; set; }

        public string Message { get; set; }

        public string Status { get; set; }

        public string Department { get; set; }
        public string Category { get; set; }
        public string Gender { get; set; }
        public string Color { get; set; }
        public string Notes { get; set; }
        public decimal Price { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }

        public int StgQ { get; set; }
        public decimal StgP { get; set; }

        public int OmsQ {get; set;}
        public decimal OmsP {get; set;}

        public int LcsQ {get; set;}
        public decimal LcsP {get; set;}

        public int OrgQ {get; set;}
        public decimal OrgP {get; set;}

        public string Asin { get; set; }
    }

    public class PrintEntry : BaseEntry
    {
        public string Brand { get; set; }
        public string ClassName { get; set; }
        public string Sku { get; set; }
        public int Qty { get; set; }
        public decimal MSRP { get; set; }
        public decimal Cost { get; set; }
        public string Gender { get; set; }
        public string UPC { get; set; }

        public string ORG { get; set; }
        public string STG { get; set; }
        public string LCS { get; set; }
        public string OMS { get; set; }
    }

    public abstract class ListingEntry : BaseEntry, INotifyPropertyChanged
    {
        private StatusCode _status;

        private List<string> _messages = new List<string>();

        public event PropertyChangedEventHandler PropertyChanged;

        public ListingEntry()
        {
            this.Title = string.Empty;
            this.Status = StatusCode.Pending;
        }

        public string Format { get; set; }
        public string Brand { get; set; }
        public string ClassName { get; set; }
        public string Sku { get; set; }

        public int? Q { get; set; }
        
        public decimal? P { get; set; }

        public int? DisplayQty { get; set; }
        
        public string Title { get; set; }

        public string Command { get; set; }

        public List<string> GetUpdateFlags()
        {
            List<string> flags;

            if (!string.IsNullOrWhiteSpace(this.Command))
            {
                flags = this.Command.Split(new Char[1] { '|' }).Select(p => p.Trim().ToUpper()).ToList();
            }
            else
            {
                flags = new List<string>();
            }

            return flags;
        }

        public bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(this.Sku) || this.Q == null || this.P == null || string.IsNullOrWhiteSpace(this.Format))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public string Message
        {
            get { return string.Join(" | ", _messages); }
            set
            {
                _messages.Add(value);

                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Message"));
                }
            }
        }

        public StatusCode Status
        {
            get { return _status; }
            set
            {
                _status = value;

                if (PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Status"));
                }
            }
        }

        
    }

    public class EbayEntry : ListingEntry
    {
        

        public EbayEntry()
        {

        }

        public string Code { get; set; }

        public string Template { get; set; }

        public decimal? BIN { get; set; }

        public string FullDescription { get; set; }

        public DateTime? StartDate { get; set; }

        public bool IsAuction()
        {
            if (this.Format.Contains("A"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public string GetDuration()
        {
            switch (this.Format)
            {
                case "I-GTC" :
                case "GTC": return "GTC";

                case "I-BIN" :
                case "BIN": return "Days_30";

                case "A7": return "Days_7";

                case "A1": return "Days_1";

                case "A3": return "Days_3";

                case "A5": return "Days_5";

                default: return string.Empty;
            }
        }

        public string GetFormat()
        {
            switch (this.Format)
            {
                case "I-BIN":
                case "I-GTC":
                case "BIN":
                case "GTC":
                    return EbayMarketplace.FORMAT_FIXEDPRICE;
                case "A1":
                case "A3":
                case "A5":
                case "A7":
                    return EbayMarketplace.FORMAT_AUCTION;
                default:
                    return null;
            }
        }

        public bool IsVariation()
        {
            if (this.Format.Contains("I-") || this.Format.Contains("A"))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public void SetFormat(string format, string duration, bool isVariation)
        {
            if (format.Equals(EbayMarketplace.FORMAT_AUCTION))
            {
                switch (duration)
                {
                    case "Days_1": this.Format = "A1"; break;
                    case "Days_3": this.Format = "A3"; break;
                    case "Days_5": this.Format = "A5"; break;
                    case "Days_7": this.Format = "A7"; break;
                }
            }
            else
            {
                switch (duration)
                {
                    case "Days_30":

                        if (isVariation)
                        {
                            this.Format = "BIN"; 
                        }
                        else
                        {
                            this.Format = "I-BIN";
                        }

                        break;

                    case "GTC":

                        if (isVariation)
                        {
                            this.Format = "GTC";
                        }
                        else
                        {
                            this.Format = "I-GTC";
                        }

                        break;
                }
            }
        }

        public string Design { get; set; }
    }

    public class AmznEntry : ListingEntry
    {
        public string ASIN { get; set; }

        public decimal? SalePrice { get; set; }

        //public bool PSpecified { get; set; }

        //public bool QSpecified { get; set; }

        public DateTime? SaleStart { get; set; }

        public DateTime? SaleEnd { get; set; }
    }

    public class BonanzaEntry : ListingEntry
    {
        public string Code { get; set; }

        public string FullDescription { get; set; }
    }
}
