using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BerkeleyEntities;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;
using System.Reflection;
using System.IO;

namespace MarketplaceManager
{
    public class ReportGenerator
    {
        private string _path;
        private SheetData _sheetData;
        private IEnumerable<object> _entries;
        private Type _entryType;

        public ReportGenerator(string path, IEnumerable<object> entries, Type entryType)
        {
            _path = path;
            _entries = entries;
            _entryType = entryType;
        }

        public void GenerateExcelReport()
        {
            if (_sheetData == null)
            {
                _sheetData = GenerateSheetData();
            }

            CreateExcelFile();
        }

        private void CreateExcelFile()
        {
            using (SpreadsheetDocument document = SpreadsheetDocument.Create(_path, SpreadsheetDocumentType.Workbook))
            {
                // Add a WorkbookPart to the document.
                WorkbookPart workbookpart = document.AddWorkbookPart();

                workbookpart.Workbook = new Workbook();

                // Add a WorksheetPart to the WorkbookPart.
                WorksheetPart worksheetPart = workbookpart.AddNewPart<WorksheetPart>();

                // Add Sheets to the Workbook.
                Sheets sheets = document.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());

                // Append a new worksheet and associate it with the workbook.

                Sheet sheet = new Sheet() { Id = document.WorkbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "HABANERO" };

                sheets.Append(sheet);

                Worksheet worksheet = new Worksheet();

                worksheet.Append(_sheetData);

                worksheetPart.Worksheet = worksheet;

                workbookpart.Workbook.Save();

                document.Close();
            }
        }

        private SheetData GenerateSheetData()
        {
            SheetData sheetData = new SheetData();

            var props = _entryType.GetProperties();

            Row headerRow = new Row();

            foreach (PropertyInfo prop in props)
            {
                headerRow.Append(new Cell() { CellValue = new CellValue(prop.Name), DataType = CellValues.String });
            }

            sheetData.Append(headerRow);

            foreach (object entry in _entries)
            {
                Row row = new Row();

                foreach (PropertyInfo prop in props)
                {
                    object value = prop.GetValue(entry, null);

                    switch (prop.PropertyType.Name)
                    {
                        case "String":
                            row.Append(new Cell() { CellValue = new CellValue(value.ToString()), DataType = CellValues.String }); 
                            break;
                        case "Int32":
                        case "Decimal":
                            row.Append(new Cell() { CellValue = new CellValue(value.ToString()), DataType = CellValues.Number }); 
                            break;
                        default:
                            row.Append(new Cell() { CellValue = new CellValue(value.ToString()), DataType = CellValues.String }); 
                            break;
                    }
                }

                sheetData.Append(row);
            }

            return sheetData;
        }
    }

    
}
