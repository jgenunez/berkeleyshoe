using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Media;
using BerkeleyEntities;
using System.Text.RegularExpressions;
using System.Threading;
using System.IO;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;


namespace InventoryApp
{
    public partial class MainForm : Form
    {
        private string _inventoryRef;
        private string _userName;
        
        public MainForm()
        {
            InitializeComponent();

        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(tbBin.Text)) { SystemSounds.Exclamation.Play(); MessageBox.Show("Invalid Bin !"); return; }

            string binLocation = tbBin.Text.Trim().ToUpper();

            ScanForm scanForm = new ScanForm(_inventoryRef, binLocation, _userName);
            scanForm.ShowDialog();         
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            
            LoginForm loginForm = new LoginForm();
            loginForm.ShowDialog();

            if (loginForm.DialogResult.Equals(DialogResult.OK))
            {
                _inventoryRef = loginForm.InventoryRef;
                _userName = loginForm.UserName;

                tbRefNumber.Text =  _inventoryRef;
                tbUser.Text = _userName;

            }
            else
            {
                this.Close();
            }

            
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            this.ExportCountingData();
        }

        private void btnUpdateBin_Click(object sender, EventArgs e)
        {
            List<BinData> bins = new List<BinData>();

            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                var entries = dataContext.InventoryEntries.Where(p => p.PhysicalInventory.Code.Equals(_inventoryRef)).ToList();

                var entryGroups = entries.GroupBy(p => p.Item);

                foreach (var entryGroup in entryGroups)
                {
                    BinData bin = new BinData();
                    bin.Sku = entryGroup.Key.ItemLookupCode;

                    var binGroups = entryGroup.GroupBy(p => p.Bin);

                    StringBuilder sb = new StringBuilder();

                    foreach (var binGroup in binGroups)
                    {
                        if (binGroup.Sum(s => s.Counted) > 0)
                        {
                            sb.AppendLine(binGroup.Key + "(" + binGroup.Sum(s => s.Counted) + ") ");
                        }
                    }

                    bin.Bin = sb.ToString();

                    bins.Add(bin);
                }

            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.ShowDialog();

            GenerateExcelFile(sfd.FileName, bins);
        }

        private void GenerateExcelFile(string path, List<BinData> bins)
        {
            SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Create(path, SpreadsheetDocumentType.Workbook);

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
            headerRow.Append(new Cell() { CellValue = new CellValue("SKU"), DataType = CellValues.String });
            headerRow.Append(new Cell() { CellValue = new CellValue("Bin"), DataType = CellValues.String });

            sheetData.Append(headerRow);

            foreach (BinData bin in bins)
            {
                Row row = new Row();
                row.Append(new Cell() { CellValue = new CellValue(bin.Sku), DataType = CellValues.String });
                row.Append(new Cell() { CellValue = new CellValue(bin.Bin), DataType = CellValues.String });
                sheetData.Append(row);

            }


            worksheet.Append(sheetData);
            worksheetPart.Worksheet = worksheet;

            workbookpart.Workbook.Save();

            // Close the document.
            spreadsheetDocument.Close();
        }

        private void ExportCountingData()
        {
            StringBuilder sb = new StringBuilder();

            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                var entries = dataContext.InventoryEntries.Where(p => p.PhysicalInventory.Code.Equals(_inventoryRef));

                foreach (InventoryEntry entry in entries)
                {
                    sb.AppendLine(string.Format("{0}\t{1}\t{2}", entry.Item.ItemLookupCode, entry.Counted, entry.LastModified));            
                }
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.AddExtension = true;
            sfd.Filter = "(*.txt)|*.txt|All Files (*.*)|*.*";
            sfd.DefaultExt = ".txt";
            sfd.ShowDialog();

            if (!string.IsNullOrWhiteSpace(sfd.FileName))
            {
                File.WriteAllText(sfd.FileName, sb.ToString());
            }

        }






        



      
        
    }
}
