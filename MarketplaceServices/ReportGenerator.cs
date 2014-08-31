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

        public ReportGenerator(string path)
        {
            _path = path;
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
            var products = GetProductData();

            SheetData sheetData = new SheetData();

            var props = typeof(ReportProductView).GetProperties();

            Row headerRow = new Row();

            foreach (PropertyInfo prop in props)
            {
                headerRow.Append(new Cell() { CellValue = new CellValue(prop.Name), DataType = CellValues.String });
            }

            sheetData.Append(headerRow);

            foreach (ReportProductView product in products)
            {
                Row row = new Row();

                foreach (PropertyInfo prop in props)
                {
                    object value = prop.GetValue(product, null);

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

        private List<ReportProductView> GetProductData()
        {
            List<ReportProductView> products = new List<ReportProductView>();

            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                //var items = dataContext.Items.Include("AmznListingItems.OrderItems.Order").Include("EbayListingItems.OrderItems.Order")
                //    .Where(p => !p.Inactive &&
                //        !p.DepartmentName.Equals("APPAREL") &&
                //        !p.DepartmentName.Equals("ACCESSORIES") &&
                //        !p.DepartmentName.Equals("MIXED ITEMS & LOTS")).ToList()
                //        .Where(p => p.Quantity > 0 || p.OnActiveListing > 0);

                dataContext.CommandTimeout = 0;

                var items = dataContext.Items
                    .Include("AmznListingItems.OrderItems.Order")
                    .Include("EbayListingItems.OrderItems.Order")
                    .ToList().Where(p =>
                        (p.Quantity > 0 || p.OnActiveListing > 0 || p.OnPendingOrder > 0) &&
                        !p.Inactive &&
                        !p.DepartmentName.Equals("APPAREL") &&
                        !p.DepartmentName.Equals("ACCESSORIES") &&
                        !p.DepartmentName.Equals("MIXED ITEMS & LOTS"));

                foreach (BerkeleyEntities.Item item in items)
                {
                    //if ((item.Quantity == 0 && item.OnActiveListing == 0 && item.OnPendingOrder == 0) ||
                    //    item.DepartmentName.Equals("APPAREL") ||
                    //    item.DepartmentName.Equals("ACCESSORIES") ||
                    //    item.DepartmentName.Equals("MIXED ITEMS & LOTS"))
                    //{
                    //    continue;
                    //}

                    ReportProductView product = new ReportProductView(item.ItemLookupCode);
                    product.Brand = item.SubDescription1;
                    product.Cost = item.Cost;
                    product.Department = item.DepartmentName;
                    product.OnPO = item.OnPurchaseOrder;
                    product.OnHand = (int)item.Quantity;
                    product.OnHold = item.OnHold;
                    product.OnPendingOrder = item.OnPendingOrder;

                    product.OrgQty = item.AmznListingItems.Where(p => p.MarketplaceID == 1 && p.IsActive).Sum(p => p.Quantity);

                    product.StgQty = item.EbayListingItems.Where(p => p.Listing.MarketplaceID == 1 && p.Listing.Status.Equals("Active")).Sum(p => p.Quantity);
                    product.OmsQty = item.EbayListingItems.Where(p => p.Listing.MarketplaceID == 2 && p.Listing.Status.Equals("Active")).Sum(p => p.Quantity);
                    product.SavQty = item.EbayListingItems.Where(p => p.Listing.MarketplaceID == 3 && p.Listing.Status.Equals("Active")).Sum(p => p.Quantity);

                    var ebayDuplicates = item.EbayListingItems
                        .Where(p => p.Listing.Status.Equals("Active"))
                        .GroupBy(p => new { Marketplace = p.Listing.Marketplace, Format = p.Listing.Format })
                        .Where(p => p.Count() > 1);

                    if (ebayDuplicates.Count() > 0)
                    {
                        product.Duplicates = string.Join(" ", ebayDuplicates.Select(p => p.Count().ToString() + p.Key.Marketplace.Code));
                    }
                    else
                    {
                        product.Duplicates = string.Empty;
                    }
 
                    product.EbaySold = item.EbayListingItems.SelectMany(p => p.OrderItems).Where(p => p.Order.MarkedAsShipped()).Sum(p => p.QuantityPurchased);
                    product.AmznSold = item.AmznListingItems.SelectMany(p => p.OrderItems).Where(p => p.Order.Status.Equals("Shipped")).Sum(p => p.QuantityOrdered);
                    products.Add(product);
                }
            }

            return products;
        }
    }
}
