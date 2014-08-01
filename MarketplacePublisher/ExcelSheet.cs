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

namespace MarketplacePublisher
{
    public class ExcelSheet
    {
        const string COLUMN_SKU = "SKU";

        private SharedStringTablePart _stringTablePart;
        private WorksheetPart _workSheetPart;

        private Dictionary<string, string> _colHeadersToRefs = new Dictionary<string, string>();
        private Dictionary<string, string> _colRefsToHeaders = new Dictionary<string, string>();
        private Dictionary<string, PropertyInfo> _colRefsToProp = new Dictionary<string, PropertyInfo>();
        private Dictionary<string, uint> _colRefsToStyle = new Dictionary<string, uint>();


        public ExcelSheet(string path)
        {
            this.Path = path;
        }

        public string Path { get; set; }

        public void UpdateSheet()
        {
            EntrySync updater = new EntrySync();

            var skus = ReadEntries().Select(p => p.Sku).Distinct().OrderBy(p => p);

            var updatedEntries = updater.UpdateEntries(skus);

            PersistEntries(updatedEntries);
        }

        public List<Entry> ReadEntries()
        {
            List<Entry> lines = new List<Entry>();

            using (SpreadsheetDocument document = SpreadsheetDocument.Open(this.Path, true))
            {
                Initialize(document);

                //this.InsertImage(50, 100, 20, 20, @"P:\products\ASICS\C038N.jpg");

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

        public void PersistEntries(IEnumerable<Entry> entries)
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
                            prop.SetValue(entry, cellValue, null); break;
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

        private void InsertImage(long x, long y, long? width, long? height, string imagePath)
        {
            try
            {
                DrawingsPart drawingPart;
                ImagePart imagePart;
                WorksheetDrawing workSheetDrawing;
                ImagePartType imagePartType;

                switch (imagePath.Substring(imagePath.LastIndexOf('.') + 1).ToLower())
                {
                    case "png":
                        imagePartType = ImagePartType.Png;
                        break;
                    case "jpg":
                    case "jpeg":
                        imagePartType = ImagePartType.Jpeg;
                        break;
                    case "gif":
                        imagePartType = ImagePartType.Gif;
                        break;
                    default:
                        return;
                }

                if (_workSheetPart.DrawingsPart == null)
                {
                    //----- no drawing part exists, add a new one

                    drawingPart = _workSheetPart.AddNewPart<DrawingsPart>();
                    imagePart = drawingPart.AddImagePart(imagePartType, _workSheetPart.GetIdOfPart(drawingPart));
                    workSheetDrawing = new WorksheetDrawing();
                }
                else
                {
                    //----- use existing drawing part

                    drawingPart = _workSheetPart.DrawingsPart;
                    imagePart = drawingPart.AddImagePart(imagePartType);
                    drawingPart.CreateRelationshipToPart(imagePart);
                    workSheetDrawing = drawingPart.WorksheetDrawing;
                }

                using (System.IO.FileStream fs = new System.IO.FileStream(imagePath, System.IO.FileMode.Open))
                {
                    imagePart.FeedData(fs);
                }

                int imageNumber = drawingPart.ImageParts.Count<ImagePart>();
                if (imageNumber == 1)
                {
                    Drawing drawing = new Drawing();
                    drawing.Id = drawingPart.GetIdOfPart(imagePart);
                    _workSheetPart.Worksheet.Append(drawing);
                }

                NonVisualDrawingProperties nvdp = new NonVisualDrawingProperties();
                nvdp.Id = new UInt32Value((uint)(1024 + imageNumber));
                nvdp.Name = "Picture " + imageNumber.ToString();
                nvdp.Description = "";
                DocumentFormat.OpenXml.Drawing.PictureLocks picLocks = new DocumentFormat.OpenXml.Drawing.PictureLocks();
                picLocks.NoChangeAspect = true;
                picLocks.NoChangeArrowheads = true;
                NonVisualPictureDrawingProperties nvpdp = new NonVisualPictureDrawingProperties();
                nvpdp.PictureLocks = picLocks;
                NonVisualPictureProperties nvpp = new NonVisualPictureProperties();
                nvpp.NonVisualDrawingProperties = nvdp;
                nvpp.NonVisualPictureDrawingProperties = nvpdp;

                DocumentFormat.OpenXml.Drawing.Stretch stretch = new DocumentFormat.OpenXml.Drawing.Stretch();
                stretch.FillRectangle = new DocumentFormat.OpenXml.Drawing.FillRectangle();

                BlipFill blipFill = new BlipFill();
                DocumentFormat.OpenXml.Drawing.Blip blip = new DocumentFormat.OpenXml.Drawing.Blip();
                blip.Embed = drawingPart.GetIdOfPart(imagePart);
                blip.CompressionState = DocumentFormat.OpenXml.Drawing.BlipCompressionValues.Print;
                blipFill.Blip = blip;
                blipFill.SourceRectangle = new DocumentFormat.OpenXml.Drawing.SourceRectangle();
                blipFill.Append(stretch);

                DocumentFormat.OpenXml.Drawing.Transform2D t2d = new DocumentFormat.OpenXml.Drawing.Transform2D();
                DocumentFormat.OpenXml.Drawing.Offset offset = new DocumentFormat.OpenXml.Drawing.Offset();
                offset.X = 0;
                offset.Y = 0;
                t2d.Offset = offset;
                System.Drawing.Bitmap bm = new System.Drawing.Bitmap(imagePath);

                DocumentFormat.OpenXml.Drawing.Extents extents = new DocumentFormat.OpenXml.Drawing.Extents();

                if (width == null)
                    extents.Cx = (long)bm.Width * (long)((float)914400 / bm.HorizontalResolution);
                else
                    extents.Cx = width;

                if (height == null)
                    extents.Cy = (long)bm.Height * (long)((float)914400 / bm.VerticalResolution);
                else
                    extents.Cy = height;

                bm.Dispose();
                t2d.Extents = extents;
                ShapeProperties sp = new ShapeProperties();
                sp.BlackWhiteMode = DocumentFormat.OpenXml.Drawing.BlackWhiteModeValues.Auto;
                sp.Transform2D = t2d;
                DocumentFormat.OpenXml.Drawing.PresetGeometry prstGeom = new DocumentFormat.OpenXml.Drawing.PresetGeometry();
                prstGeom.Preset = DocumentFormat.OpenXml.Drawing.ShapeTypeValues.Rectangle;
                prstGeom.AdjustValueList = new DocumentFormat.OpenXml.Drawing.AdjustValueList();
                sp.Append(prstGeom);
                sp.Append(new DocumentFormat.OpenXml.Drawing.NoFill());

                DocumentFormat.OpenXml.Drawing.Spreadsheet.Picture picture = new DocumentFormat.OpenXml.Drawing.Spreadsheet.Picture();
                picture.NonVisualPictureProperties = nvpp;
                picture.BlipFill = blipFill;
                picture.ShapeProperties = sp;

                Position pos = new Position();
                pos.X = x;
                pos.Y = y;
                Extent ext = new Extent();
                ext.Cx = extents.Cx;
                ext.Cy = extents.Cy;
                AbsoluteAnchor anchor = new AbsoluteAnchor();
                anchor.Position = pos;
                anchor.Extent = ext;
                anchor.Append(picture);
                anchor.Append(new ClientData());
                workSheetDrawing.Append(anchor);
                workSheetDrawing.Save(drawingPart);
            }
            catch (Exception ex)
            {
                throw ex; // or do something more interesting if you want
            }
        }

    }
}
