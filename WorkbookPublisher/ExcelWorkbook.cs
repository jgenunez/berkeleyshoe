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

        //public void CreateErrorSheet(string code, List<ListingEntry> entries)
        //{
        //    using (SpreadsheetDocument document = SpreadsheetDocument.Open(this.Path, true))
        //    {
        //        SharedStringTablePart stringTablePart = document.WorkbookPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();

        //        string parentName = code.Replace("(errors)", "");

        //        var sheets = document.WorkbookPart.Workbook.Sheets.Elements<Sheet>();

        //        Sheet parentSheet = sheets.Single(p => p.Name.Value.Equals(parentName));
        //        WorksheetPart parentWorksheetPart = document.WorkbookPart.GetPartById(parentSheet.Id) as WorksheetPart;
        //        Row header = parentWorksheetPart.Worksheet.Descendants<Row>().Single(p => p.RowIndex.Value == 1).CloneNode(true) as Row;

        //        var worksheet = new Worksheet();
        //        var sheetData = worksheet.AppendChild<SheetData>(new SheetData());

                

        //        sheetData.AppendChild<Row>(header);

        //        var entryType = entries.First().GetType();

        //        RowMapper mapper = new RowMapper(entryType, header, stringTablePart.SharedStringTable);

        //        uint newRowCount = 2;

        //        foreach (Row row in parentWorksheetPart.Worksheet.Descendants<Row>().Where(p => p.RowIndex.Value != 1))
        //        {
        //            string sku = mapper.GetValue(row, COLUMN_SKU);
        //            string format = mapper.GetValue(row, COLUMN_FORMAT); ;

        //            ListingEntry entry = entries.SingleOrDefault(p => p.Code.Equals(sku + format));

        //            if (entry != null)
        //            {
        //                Row newRow = row.CloneNode(true) as Row;
        //                newRow.RowIndex.Value = newRowCount;

        //                foreach (var cell in newRow.Elements<Cell>())                       
        //                {
        //                    cell.CellReference.Value = cell.CellReference.Value.Replace(row.RowIndex.Value.ToString(), newRow.RowIndex.Value.ToString());
        //                }

        //                mapper.UpdateRow(entry, newRow);
        //                sheetData.AppendChild<Row>(newRow);
        //                newRowCount++;
        //            }
        //        }

        //        Sheet targetSheet = sheets.SingleOrDefault(p => p.Name.Value.Equals(code));

        //        if (targetSheet == null)
        //        {
        //            WorksheetPart worksheetPart = CreateWorksheet(document.WorkbookPart, code);
        //            worksheetPart.Worksheet = worksheet;
        //        }
        //        else
        //        {
        //            WorksheetPart worksheetPart = document.WorkbookPart.GetPartById(targetSheet.Id) as WorksheetPart;
        //            worksheetPart.Worksheet = worksheet;
        //        }

        //        document.Close();
        //    }
        //}

        public void UpdateSheet(List<BaseEntry> entries, Type entryType, string sheetName)
        {
            using (SpreadsheetDocument document = SpreadsheetDocument.Open(this.Path, true))
            {
                SharedStringTablePart stringTablePart = document.WorkbookPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();

                var sheets = document.WorkbookPart.Workbook.Sheets.Elements<Sheet>();

                Sheet mainSheet = sheets.SingleOrDefault(p => p.Name.Value.ToUpper().Equals(sheetName));

                if (mainSheet != null)
                {
                    WorksheetPart mainWorksheetPart = document.WorkbookPart.GetPartById(mainSheet.Id) as WorksheetPart;

                    Worksheet worksheet = mainWorksheetPart.Worksheet;

                    Row headerRow = worksheet.Descendants<Row>().Single(p => p.RowIndex.Value == 1);

                    RowMapper mapper = new RowMapper(entryType, headerRow, stringTablePart.SharedStringTable);

                    var sheetData = worksheet.GetFirstChild<SheetData>();
                    var rows = sheetData.Elements<Row>();

                    foreach (var entry in entries)
                    {
                        Row row = rows.SingleOrDefault(p => p.RowIndex.Value == entry.RowIndex);

                        if (row == null)
                        {
                            Row parentRow = rows.Single(p => p.RowIndex.Value == entry.ParentRowIndex).Clone() as Row;
                            row = new Row() { RowIndex = new UInt32Value(entry.RowIndex) };
                            sheetData.AppendChild<Row>(row);
                        }

                        mapper.UpdateRow(entry, row);
                    }

                }

                document.Close();
            }
        }

        //public void UpdateMainSheet(List<MainEntry> entries)
        //{
        //    using (SpreadsheetDocument document = SpreadsheetDocument.Open(this.Path, true))
        //    {
        //        SharedStringTablePart stringTablePart = document.WorkbookPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();

        //        var sheets = document.WorkbookPart.Workbook.Sheets.Elements<Sheet>();

        //        Sheet mainSheet = sheets.SingleOrDefault(p => p.Name.Value.ToUpper().Equals("MAIN"));

        //        if (mainSheet != null)
        //        {
        //            WorksheetPart mainWorksheetPart = document.WorkbookPart.GetPartById(mainSheet.Id) as WorksheetPart;

        //            Worksheet worksheet = mainWorksheetPart.Worksheet;

        //            Row headerRow = worksheet.Descendants<Row>().Single(p => p.RowIndex.Value == 1);

        //            RowMapper mapper = new RowMapper(typeof(MainEntry), headerRow, stringTablePart.SharedStringTable);

        //            var rows = worksheet.GetFirstChild<SheetData>().Elements<Row>();

        //            foreach (MainEntry entry in entries)
        //            {
        //                Row row = rows.Single(p => p.RowIndex.Value == entry.RowIndex);

        //                mapper.UpdateRow(entry, row);
        //            }

        //        }
        //        document.Close();
        //    }
        //}

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

    public class TitleMapRule
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
    }

    public class PrintEntry : BaseEntry
    {
        public string Brand { get; set; }
        public string ClassName { get; set; }
        public string Sku { get; set; }
        public int Qty { get; set; }
        public decimal Cost { get; set; }
        public string Gender { get; set; }
        public string UPC { get; set; }

        public string Active { get; set; }
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

        public int Q { get; set; }
        public bool QSpecified { get; set; }

        public decimal P { get; set; }
        public bool PSpecified { get; set; }

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

        public void ClearMessages()
        {
            _messages.Clear();

            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs("Message"));
            }
        }
    }

    public class EbayEntry : ListingEntry
    {
        private DateTime _startDate;

        public EbayEntry()
        {

        }

        public string Code { get; set; }

        public decimal BIN { get; set; }

        public string FullDescription { get; set; }

        public DateTime StartDate
        {
            get { return _startDate; }
            set { StartDateSpecified = true; _startDate = value; }
        }

        public bool StartDateSpecified { get; set; }

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
                case "GTC": return this.Format;

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

        public void SetFormat(string format, string duration)
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
                    case "Days_30": this.Format = "BIN"; break;
                    case "GTC": this.Format = "GTC"; break;
                }
            }
        }

    }

    public class AmznEntry : ListingEntry
    {
        private decimal _salePrice;

        public string ASIN { get; set; }

        public decimal SalePrice
        {
            get { return _salePrice; }
            set 
            {
                this.SalePriceSpecified = true;
                _salePrice = value;
            }
        }

        public bool SalePriceSpecified { get; set; }

        public DateTime SaleStart { get; set; }

        public DateTime SaleEnd { get; set; }
    }

    public class BonanzaEntry : ListingEntry
    {
        public string Code { get; set; }

        public string FullDescription { get; set; }
    }
}
