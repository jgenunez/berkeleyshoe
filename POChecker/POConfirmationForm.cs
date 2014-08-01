using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BerkeleyEntities;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;

namespace POConfirmation
{
    public partial class POConfirmationForm : Form
    {
        berkeleyEntities DataContext = new berkeleyEntities();

        public POConfirmationForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            List<bsi_quantities> listings = DataContext.bsi_quantities.Where(p => p.purchaseOrder.Equals(tbPONumber.Text) && p.bsi_posts != null).ToList();

            if (listings.Count == 0)
            {
                MessageBox.Show("No entries found !");
                return;
            }

            generateExcelFile(listings);
            
             
        }

        private void generateExcelFile(List<bsi_quantities> listings)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.DefaultExt = ".xlsx";
            DialogResult dr = dialog.ShowDialog();


            if (!dr.Equals(DialogResult.OK))
            {
                return;
            }

            SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Create(dialog.FileName, SpreadsheetDocumentType.Workbook);


            // Add a WorkbookPart to the document.
            WorkbookPart workbookpart = spreadsheetDocument.AddWorkbookPart();
            workbookpart.Workbook = new Workbook();

            // Add a WorksheetPart to the WorkbookPart.
            WorksheetPart worksheetPart = workbookpart.AddNewPart<WorksheetPart>();


            // Add Sheets to the Workbook.
            Sheets sheets = spreadsheetDocument.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());

            // Append a new worksheet and associate it with the workbook.
            Sheet sheet = new Sheet() { Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "HABANERO" };
            sheets.Append(sheet);

            Worksheet worksheet = new Worksheet();
            SheetData sheetData = new SheetData();

            Row headerRow = new Row();
            headerRow.Append(new Cell() { CellValue = new CellValue("Brand"), DataType = CellValues.String });
            headerRow.Append(new Cell() { CellValue = new CellValue("SKU"), DataType = CellValues.String });
            headerRow.Append(new Cell() { CellValue = new CellValue("Published"), DataType = CellValues.String });
            headerRow.Append(new Cell() { CellValue = new CellValue("Price"), DataType = CellValues.String });
            headerRow.Append(new Cell() { CellValue = new CellValue("Mrkt"), DataType = CellValues.String });
            headerRow.Append(new Cell() { CellValue = new CellValue("ListindID"), DataType = CellValues.String });
            headerRow.Append(new Cell() { CellValue = new CellValue("Status"), DataType = CellValues.String });
            headerRow.Append(new Cell() { CellValue = new CellValue("Format"), DataType = CellValues.String });
            headerRow.Append(new Cell() { CellValue = new CellValue("On Hand"), DataType = CellValues.String });

            
            sheetData.Append(headerRow);

            foreach (bsi_quantities listing in listings)
            {
               
                BerkeleyEntities.Item item = listing.Item;

                Row row = new Row();
                row.Append(new Cell() { CellValue = new CellValue(item.SubDescription1), DataType = CellValues.String });
                row.Append(new Cell() { CellValue = new CellValue(item.ItemLookupCode), DataType = CellValues.String });
                row.Append(new Cell() { CellValue = new CellValue(listing.quantity.ToString()), DataType = CellValues.Number });
                row.Append(new Cell() { CellValue = new CellValue(listing.bsi_posts.price), DataType = CellValues.Number });
                row.Append(new Cell() { CellValue = new CellValue(listing.bsi_posts.marketplace.ToString()), DataType = CellValues.Number });
                row.Append(new Cell() { CellValue = new CellValue(listing.bsi_posts.markerplaceItemID), DataType = CellValues.Number });
                row.Append(new Cell() { CellValue = new CellValue(listing.bsi_posts.status.ToString()), DataType = CellValues.Number });
                row.Append(new Cell() { CellValue = new CellValue(listing.bsi_posts.sellingFormat), DataType = CellValues.String });
                row.Append(new Cell() { CellValue = new CellValue(listing.Item.Quantity.ToString()), DataType = CellValues.Number });
                sheetData.Append(row);

            }


            worksheet.Append(sheetData);
            worksheetPart.Worksheet = worksheet;

            workbookpart.Workbook.Save();

            // Close the document.
            spreadsheetDocument.Close();
        }
    }
}
