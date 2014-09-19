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

namespace WorkbookPublisher
{
    public class ExcelWorkbook
    {
        const string COLUMN_SKU = "SKU";
        const string COLUMN_FORMAT = "FORMAT";
        const string COLUMN_MESSAGE = "MESSAGE";

        private SharedStringTablePart _stringTablePart;

        private Dictionary<string, string> _colHeadersToRefs = new Dictionary<string, string>();
        private Dictionary<string, string> _colRefsToHeaders = new Dictionary<string, string>();
        private Dictionary<string, PropertyInfo> _colRefsToProp = new Dictionary<string, PropertyInfo>();


        public ExcelWorkbook(string path)
        {
            this.Path = path;
        }

        public string Path { get; set; }

        public void CreateErrorSheet(string code, List<Entry> entries)
        {
            using (SpreadsheetDocument document = SpreadsheetDocument.Open(this.Path, true))
            {
                _stringTablePart = document.WorkbookPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();

                string parentName = code.Replace("(errors)", "");

                var sheets = document.WorkbookPart.Workbook.Sheets.Elements<Sheet>();

                Sheet parentSheet = sheets.Single(p => p.Name.Value.Equals(parentName));

                WorksheetPart parentWorksheetPart = document.WorkbookPart.GetPartById(parentSheet.Id) as WorksheetPart;

                var newWorksheet = parentWorksheetPart.Worksheet.CloneNode(true) as Worksheet;
                var newSheetData = newWorksheet.GetFirstChild<SheetData>();

                newSheetData.RemoveAllChildren();

                Row newHeaderRow = parentWorksheetPart.Worksheet.Descendants<Row>().Single(p => p.RowIndex.Value == 1).CloneNode(true) as Row;

                AddMessageHeader(newHeaderRow);

                newSheetData.AppendChild<Row>(newHeaderRow);

                var entryType = entries.First().GetType();

                RegisterColumns(entryType, newHeaderRow);

                uint newRowCount = 2;

                foreach (Row row in GetValidRows(parentWorksheetPart.Worksheet))
                {
                    string sku = GetCellValue(GetCell(row, _colHeadersToRefs[COLUMN_SKU]));
                    string format = GetCellValue(GetCell(row, _colHeadersToRefs[COLUMN_FORMAT]));

                    Entry entry = entries.SingleOrDefault(p => p.Sku.Equals(sku) && p.Format.Equals(format));

                    if (entry != null)
                    {
                        Row newRow = row.CloneNode(true) as Row;

                        newRow.RowIndex.Value = newRowCount;

                        foreach (var cell in newRow.Elements<Cell>())                       
                        {
                            cell.CellReference.Value = cell.CellReference.Value.Replace(row.RowIndex.Value.ToString(), newRow.RowIndex.Value.ToString());
                        }

                        Cell msgCell = GetCell(newRow, _colHeadersToRefs[COLUMN_MESSAGE]);
                        msgCell.CellValue = new CellValue(InsertSharedString(entry.Message).ToString());
                        msgCell.DataType = CellValues.SharedString;

                        newSheetData.AppendChild<Row>(newRow);

                        newRowCount++;
                    }
                }

                Sheet targetSheet = sheets.SingleOrDefault(p => p.Name.Value.Equals(code));

                if (targetSheet == null)
                {
                    WorksheetPart worksheetPart = CreateWorksheet(document.WorkbookPart, code);
                    worksheetPart.Worksheet = newWorksheet;
                }
                else
                {
                    WorksheetPart worksheetPart = document.WorkbookPart.GetPartById(targetSheet.Id) as WorksheetPart;
                    worksheetPart.Worksheet = newWorksheet;
                }

                document.Close();
            }
        }

        public List<Entry> ReadEntry(Type entryType, string code)
        {
            List<Entry> entries = new List<Entry>();

            using (SpreadsheetDocument document = SpreadsheetDocument.Open(this.Path, false))
            {
                _stringTablePart = document.WorkbookPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();

                Sheet sheet = document.WorkbookPart.Workbook.Sheets.Descendants<Sheet>().SingleOrDefault(p => p.Name.Value.Equals(code));

                if (sheet != null)
                {
                    WorksheetPart worksheetPart = document.WorkbookPart.GetPartById(sheet.Id) as WorksheetPart;

                    RegisterColumns(entryType, worksheetPart.Worksheet.Descendants<Row>().Single(p => p.RowIndex.Value == 1));

                    foreach (Row row in GetValidRows(worksheetPart.Worksheet))
                    {
                        Entry entry = CreateEntry(row, entryType);
                        entry.RowIndex = row.RowIndex.Value;
                        entries.Add(entry);
                    }
                }

                document.Close();
            }

            return entries;
        }

        private void AddMessageHeader(Row headerRow)
        {
            if (!headerRow.Elements<Cell>().Any(p => GetCellValue(p).Trim().ToUpper().Equals(COLUMN_MESSAGE)))
            {
                Cell msgCell = headerRow.Elements<Cell>().FirstOrDefault(p => string.IsNullOrWhiteSpace(GetCellValue(p)));

                if (msgCell == null)
                {
                    msgCell = new Cell();
                    msgCell.DataType = CellValues.SharedString;
                    msgCell.CellReference = new StringValue(GetColLetterFromIndex(headerRow.Elements<Cell>().Count() + 1) + headerRow.RowIndex.Value.ToString());

                    headerRow.AppendChild<Cell>(msgCell);
                }

                msgCell.CellValue = new CellValue(InsertSharedString(COLUMN_MESSAGE).ToString());
            }
        }

        private List<Row> GetValidRows(Worksheet worksheet)
        {
            return worksheet.Descendants<Cell>().Where(c =>
                string.Compare(ParseColRefs(c.CellReference.Value), _colHeadersToRefs[COLUMN_SKU], true) == 0 &&
                !string.IsNullOrWhiteSpace(GetCellValue(c))
                ).Select(p => p.Parent).Cast<Row>().Where(p => p.RowIndex != 1).ToList();
        }

        private Entry CreateEntry(Row row, Type entryType)
        {
            Entry entry = Activator.CreateInstance(entryType) as Entry;

            foreach (Cell cell in row.OfType<Cell>())
            {
                string colRef = this.ParseColRefs(cell.CellReference);
                string cellValue = this.GetCellValue(cell);
                if (_colRefsToProp.ContainsKey(colRef) && !string.IsNullOrWhiteSpace(cellValue))
                {
                    PropertyInfo prop = _colRefsToProp[colRef];

                    if (prop.Name.Equals("Message"))                
                    {
                        continue;
                    }

                    try
                    {
                        switch (prop.PropertyType.Name)
                        {
                            case "String":
                                prop.SetValue(entry, cellValue, null); break;
                            case "Int32":
                                prop.SetValue(entry, Convert.ToInt32(cellValue), null); break;
                            case "Decimal":
                                prop.SetValue(entry, Convert.ToDecimal(cellValue), null); break;
                            case "DateTime":
                                prop.SetValue(entry, DateTime.FromOADate(Convert.ToDouble(cellValue)).ToUniversalTime(), null); break;
                            default:
                                prop.SetValue(entry, cellValue, null); break;
                        }
                    }
                    catch (FormatException e)
                    {
                        entry.Message = e.Message;
                    }
                }
            }
            return entry;
        }

        private Cell CreateCell(EbayEntry entry, string colRef)
        {
            Cell cell = new Cell();

            cell.CellReference = new StringValue(colRef + entry.RowIndex.ToString());

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

        private void RegisterColumns(Type entryType, Row headerRow)
        {          
            _colRefsToHeaders.Clear();
            _colHeadersToRefs.Clear();
            _colRefsToProp.Clear();

            foreach (Cell cell in headerRow.OfType<Cell>())
            {
                string header = GetCellValue(cell).ToUpper().Trim();
                string columnRef = ParseColRefs(cell.CellReference.Value);

                _colRefsToHeaders.Add(columnRef, header);
                _colHeadersToRefs.Add(header, columnRef);
            }

            PropertyInfo[] props = entryType.GetProperties();

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


            //var missingCols = props.Where(p => !_colRefsToProp.Values.Any(s => s.Equals(p)));
        }

        private WorksheetPart CreateWorksheet(WorkbookPart workbookPart, string name)
        {
            WorksheetPart newWorksheetPart = workbookPart.AddNewPart<WorksheetPart>();

            uint sheetId = 1;

            var sheets = workbookPart.Workbook.Sheets.Elements<Sheet>();

            if (sheets.Count() > 0)
            {
                sheetId = sheets.Select(s => s.SheetId.Value).Max() + 1;
            }

            string relationshipId = workbookPart.GetIdOfPart(newWorksheetPart);

            // Append the new worksheet and associate it with the workbook.
            Sheet sheet = new Sheet() { Id = relationshipId, SheetId = sheetId, Name = name };

            workbookPart.Workbook.Sheets.Append(sheet);

            return newWorksheetPart;
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
                    return _stringTablePart.SharedStringTable.ElementAt(strIndex).InnerText;
                }
                else
                {
                    return cell.CellValue.Text;
                }
            }

            return string.Empty;
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
                    

                    if (string.Compare(cell.CellReference.Value, cellRef, true) > 0 && cell.CellReference.Value.Count() == cellRef.Count())
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

        public string GetColLetterFromIndex(int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = String.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (int)((dividend - modulo) / 26);
            }

            return columnName;
        }

        //private void InsertImage(long x, long y, long? width, long? height, string imagePath)
        //{
        //    try
        //    {
        //        DrawingsPart drawingPart;
        //        ImagePart imagePart;
        //        WorksheetDrawing workSheetDrawing;
        //        ImagePartType imagePartType;

        //        switch (imagePath.Substring(imagePath.LastIndexOf('.') + 1).ToLower())
        //        {
        //            case "png":
        //                imagePartType = ImagePartType.Png;
        //                break;
        //            case "jpg":
        //            case "jpeg":
        //                imagePartType = ImagePartType.Jpeg;
        //                break;
        //            case "gif":
        //                imagePartType = ImagePartType.Gif;
        //                break;
        //            default:
        //                return;
        //        }

        //        if (_workSheetPart.DrawingsPart == null)
        //        {
        //            //----- no drawing part exists, add a new one

        //            drawingPart = _workSheetPart.AddNewPart<DrawingsPart>();
        //            imagePart = drawingPart.AddImagePart(imagePartType, _workSheetPart.GetIdOfPart(drawingPart));
        //            workSheetDrawing = new WorksheetDrawing();
        //        }
        //        else
        //        {
        //            //----- use existing drawing part

        //            drawingPart = _workSheetPart.DrawingsPart;
        //            imagePart = drawingPart.AddImagePart(imagePartType);
        //            drawingPart.CreateRelationshipToPart(imagePart);
        //            workSheetDrawing = drawingPart.WorksheetDrawing;
        //        }

        //        using (System.IO.FileStream fs = new System.IO.FileStream(imagePath, System.IO.FileMode.Open))
        //        {
        //            imagePart.FeedData(fs);
        //        }

        //        int imageNumber = drawingPart.ImageParts.Count<ImagePart>();
        //        if (imageNumber == 1)
        //        {
        //            Drawing drawing = new Drawing();
        //            drawing.Id = drawingPart.GetIdOfPart(imagePart);
        //            _workSheetPart.Worksheet.Append(drawing);
        //        }

        //        NonVisualDrawingProperties nvdp = new NonVisualDrawingProperties();
        //        nvdp.Id = new UInt32Value((uint)(1024 + imageNumber));
        //        nvdp.Name = "Picture " + imageNumber.ToString();
        //        nvdp.Description = "";
        //        DocumentFormat.OpenXml.Drawing.PictureLocks picLocks = new DocumentFormat.OpenXml.Drawing.PictureLocks();
        //        picLocks.NoChangeAspect = true;
        //        picLocks.NoChangeArrowheads = true;
        //        NonVisualPictureDrawingProperties nvpdp = new NonVisualPictureDrawingProperties();
        //        nvpdp.PictureLocks = picLocks;
        //        NonVisualPictureProperties nvpp = new NonVisualPictureProperties();
        //        nvpp.NonVisualDrawingProperties = nvdp;
        //        nvpp.NonVisualPictureDrawingProperties = nvpdp;

        //        DocumentFormat.OpenXml.Drawing.Stretch stretch = new DocumentFormat.OpenXml.Drawing.Stretch();
        //        stretch.FillRectangle = new DocumentFormat.OpenXml.Drawing.FillRectangle();

        //        BlipFill blipFill = new BlipFill();
        //        DocumentFormat.OpenXml.Drawing.Blip blip = new DocumentFormat.OpenXml.Drawing.Blip();
        //        blip.Embed = drawingPart.GetIdOfPart(imagePart);
        //        blip.CompressionState = DocumentFormat.OpenXml.Drawing.BlipCompressionValues.Print;
        //        blipFill.Blip = blip;
        //        blipFill.SourceRectangle = new DocumentFormat.OpenXml.Drawing.SourceRectangle();
        //        blipFill.Append(stretch);

        //        DocumentFormat.OpenXml.Drawing.Transform2D t2d = new DocumentFormat.OpenXml.Drawing.Transform2D();
        //        DocumentFormat.OpenXml.Drawing.Offset offset = new DocumentFormat.OpenXml.Drawing.Offset();
        //        offset.X = 0;
        //        offset.Y = 0;
        //        t2d.Offset = offset;
        //        System.Drawing.Bitmap bm = new System.Drawing.Bitmap(imagePath);

        //        DocumentFormat.OpenXml.Drawing.Extents extents = new DocumentFormat.OpenXml.Drawing.Extents();

        //        if (width == null)
        //            extents.Cx = (long)bm.Width * (long)((float)914400 / bm.HorizontalResolution);
        //        else
        //            extents.Cx = width;

        //        if (height == null)
        //            extents.Cy = (long)bm.Height * (long)((float)914400 / bm.VerticalResolution);
        //        else
        //            extents.Cy = height;

        //        bm.Dispose();
        //        t2d.Extents = extents;
        //        ShapeProperties sp = new ShapeProperties();
        //        sp.BlackWhiteMode = DocumentFormat.OpenXml.Drawing.BlackWhiteModeValues.Auto;
        //        sp.Transform2D = t2d;
        //        DocumentFormat.OpenXml.Drawing.PresetGeometry prstGeom = new DocumentFormat.OpenXml.Drawing.PresetGeometry();
        //        prstGeom.Preset = DocumentFormat.OpenXml.Drawing.ShapeTypeValues.Rectangle;
        //        prstGeom.AdjustValueList = new DocumentFormat.OpenXml.Drawing.AdjustValueList();
        //        sp.Append(prstGeom);
        //        sp.Append(new DocumentFormat.OpenXml.Drawing.NoFill());

        //        DocumentFormat.OpenXml.Drawing.Spreadsheet.Picture picture = new DocumentFormat.OpenXml.Drawing.Spreadsheet.Picture();
        //        picture.NonVisualPictureProperties = nvpp;
        //        picture.BlipFill = blipFill;
        //        picture.ShapeProperties = sp;

        //        Position pos = new Position();
        //        pos.X = x;
        //        pos.Y = y;
        //        Extent ext = new Extent();
        //        ext.Cx = extents.Cx;
        //        ext.Cy = extents.Cy;
        //        AbsoluteAnchor anchor = new AbsoluteAnchor();
        //        anchor.Position = pos;
        //        anchor.Extent = ext;
        //        anchor.Append(picture);
        //        anchor.Append(new ClientData());
        //        workSheetDrawing.Append(anchor);
        //        workSheetDrawing.Save(drawingPart);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex; // or do something more interesting if you want
        //    }
        //}

    }

    public enum StatusCode { Pending , Processing, Error, Completed };

    public abstract class Entry : INotifyPropertyChanged
    {
        
        private List<string> _messages = new List<string>();
        private StatusCode _status;

        public event PropertyChangedEventHandler PropertyChanged;

        public Entry()
        {
            this.Format = string.Empty;
            this.Title = string.Empty;
            this.Status = StatusCode.Pending;
        }

        public int ListingID { get; set; }

        public uint RowIndex { get; set; }
        public string Brand { get; set; }
        public string ClassName { get; set; }
        public string Sku { get; set; }
        public string Format { get; set; }
        public int Q { get; set; }
        public decimal P { get; set; }
        public string Title { get; set; }

        
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

    public class EbayEntry : Entry
    {
        private DateTime _startDate;

        public EbayEntry()
        {

        }

        public DateTime StartDate
        {
            get { return _startDate; }
            set { StartDateSpecified = true; _startDate = value; }
        }

        public bool StartDateSpecified { get; set; }

        public string FullDescription { get; set; }

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
                    return BerkeleyEntities.Ebay.Publisher.FORMAT_FIXEDPRICE;
                case "A1":
                case "A3":
                case "A5":
                case "A7":
                    return BerkeleyEntities.Ebay.Publisher.FORMAT_AUCTION;
                default:
                    return null;
            }
        }

        public decimal BIN { get; set; }
    }

    public class AmznEntry : Entry
    {
        public StatusCode InventoryFeedStatus { get; set; }

        public StatusCode ProductFeedStatus { get; set; }

        public StatusCode ParentProductFeedStatus { get; set; }

        public StatusCode PriceFeedStatus { get; set; }

        public StatusCode RelationshipFeedStatus { get; set; }

        public void UpdateStatus()
        {
            if (this.ProductFeedStatus.Equals(StatusCode.Processing) || this.ParentProductFeedStatus.Equals(StatusCode.Processing) || 
                this.InventoryFeedStatus.Equals(StatusCode.Processing) || this.PriceFeedStatus.Equals(StatusCode.Processing))
            {
                this.Status = StatusCode.Processing;
            }
            else if (this.ProductFeedStatus.Equals(StatusCode.Error) || this.ParentProductFeedStatus.Equals(StatusCode.Error) ||
                this.InventoryFeedStatus.Equals(StatusCode.Error) || this.PriceFeedStatus.Equals(StatusCode.Error))
            {
                this.Status = StatusCode.Error;
            }
            else
            {
                this.Status = StatusCode.Completed;
            }
        }
    }
}
