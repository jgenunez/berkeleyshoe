using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WorkbookPublisher
{
    public class RowMapper
    {
        private Dictionary<string, string> _colHeadersToRefs = new Dictionary<string, string>();
        private Dictionary<string, string> _colRefsToHeaders = new Dictionary<string, string>();
        private Dictionary<string, PropertyInfo> _colRefsToProp = new Dictionary<string, PropertyInfo>();

        private Type _type;
        private SharedStringTable _stringTable;

        public RowMapper(Type type, Row headerRow, SharedStringTable stringTable)
        {
            _type = type;
            _stringTable = stringTable;

            foreach (Cell cell in headerRow.OfType<Cell>())
            {
                string header = GetCellValue(cell).ToUpper().Trim();
                string columnRef = ParseColName(cell.CellReference.Value);

                _colRefsToHeaders.Add(columnRef, header);
                _colHeadersToRefs.Add(header, columnRef);
            }

            PropertyInfo[] props = type.GetProperties();

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

        public BaseEntry MapRow(Row source)
        {
            BaseEntry target = Activator.CreateInstance(_type) as BaseEntry;

            target.RowIndex = source.RowIndex.Value;

            foreach (Cell cell in source.OfType<Cell>())
            {
                string colRef = this.ParseColName(cell.CellReference);
                string cellValue = this.GetCellValue(cell);

                if (_colRefsToProp.ContainsKey(colRef) && !string.IsNullOrWhiteSpace(cellValue))
                {
                    PropertyInfo prop = _colRefsToProp[colRef];

                    string propertyType = prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? 
                        prop.PropertyType.GetGenericArguments()[0].Name : prop.PropertyType.Name;

                    if (prop.Name.Equals("Message") || prop.Name.Equals("Status"))
                    {
                        continue;
                    }

                    try
                    {
                        switch (propertyType)
                        {
                            case "String":
                                prop.SetValue(target, cellValue, null); break;
                            case "Int32":
                                prop.SetValue(target, Convert.ToInt32(cellValue), null); break;
                            case "Decimal":
                                prop.SetValue(target, Math.Round(Convert.ToDecimal(cellValue), 2), null); break;
                            case "DateTime":
                                prop.SetValue(target, DateTime.FromOADate(Convert.ToDouble(cellValue)).ToUniversalTime(), null); break;
                            default:
                                prop.SetValue(target, cellValue, null); break;
                        }
                    }
                    catch (Exception e)
                    {
                        var props = _type.GetProperties();

                        var msgProp = props.SingleOrDefault(p => p.Name.Equals("Message"));
                        var statusProp = props.SingleOrDefault(p => p.Name.Equals("Status"));

                        string msg = string.Format("{0} column does not have a valid format", prop.Name);

                        if (msgProp != null)
                            msgProp.SetValue(target, msg, null);

                        if(statusProp != null)
                            statusProp.SetValue(target, StatusCode.Error, null);

                    }
                }
            }

            return target;
        }

        public void UpdateRow(object source, Row target)
        {
            foreach (string header in _colHeadersToRefs.Keys)
            {
                string colRef = _colHeadersToRefs[header];

                if (_colRefsToProp.ContainsKey(colRef))
                {
                    var prop = _colRefsToProp[colRef];

                    object value = prop.GetValue(source, null);

                    if (value != null)
                    {
                        Cell cell = GetCell(target, colRef);

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
                }
            }
        }

        public string GetValue(Row row, string column)
        {
            return GetCellValue(GetCell(row, _colHeadersToRefs[column])); ;
        }

        private Cell GetCell(Row row, string column)
        {
            string cellRef = column + row.RowIndex.Value.ToString();


            if (row.Elements<Cell>().Count() == 0)
            {
                Cell newCell = new Cell() { CellReference = cellRef, StyleIndex = 0 };

                row.Append(newCell);

                return newCell;
            }
            else if (row.Elements<Cell>().Where(c => c.CellReference.Value == cellRef).Count() > 0)
            {
                return row.Elements<Cell>().Where(c => c.CellReference.Value == cellRef).First();
            }
            else
            {
                // Cells must be in sequential order according to CellReference. Determine where to insert the new cell.
                Cell refCell = null;

                foreach (Cell cell in row.Elements<Cell>())
                {
                    string columnRef= ParseColName(cell.CellReference.Value);

                    if (string.Compare(columnRef, column, StringComparison.OrdinalIgnoreCase) > 0 && column.Count() == columnRef.Count())
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

        public string GetCellValue(Cell cell)
        {
            if (cell != null && cell.CellValue != null)
            {
                if (cell.DataType != null && cell.DataType.Value.Equals(CellValues.SharedString))
                {
                    int strIndex = int.Parse(cell.CellValue.Text);
                    return _stringTable.ElementAt(strIndex).InnerText;
                }
                else
                {
                    return cell.CellValue.Text;
                }
            }

            return string.Empty;
        }

        private int InsertSharedString(string text)
        {
            int i = 0;
            // Iterate through all the items in the SharedStringTable. If the text already exists, return its index.
            foreach (SharedStringItem item in _stringTable.Elements<SharedStringItem>())
            {
                if (item.InnerText == text)
                {
                    return i;
                }

                i++;
            }
            // The text does not exist in the part. Create the SharedStringItem and return its index.
            _stringTable.AppendChild(new SharedStringItem(new Text(text)));
            return i;
        }

        private string ParseColName(string cellRef)
        {
            // Create a regular expression to match the column name portion of the cell name.
            Regex regex = new Regex("[A-Za-z]+");

            Match match = regex.Match(cellRef);

            return match.Value;
        }
    }
}
