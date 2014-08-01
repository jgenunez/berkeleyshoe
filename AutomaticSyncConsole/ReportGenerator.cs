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

namespace AutomaticSyncConsole
{
    public class ReportGenerator
    {
        private string _path;

        public ReportGenerator(string path)
        {
            _path = path;
        }

        public void GenerateExcelReport()
        {
            var products = GetProductData();

            using (SpreadsheetDocument document = SpreadsheetDocument.Create(_path, SpreadsheetDocumentType.Workbook))
            {
                // Add a WorkbookPart to the document.
                WorkbookPart workbookpart = document.AddWorkbookPart();
                workbookpart.Workbook = new Workbook();

                // Add a WorksheetPart to the WorkbookPart.
                WorksheetPart itemWorkSheetPart = workbookpart.AddNewPart<WorksheetPart>();
                WorksheetPart brandWorkSheetPart = workbookpart.AddNewPart<WorksheetPart>();
                WorksheetPart supplierWorkSheetPart = workbookpart.AddNewPart<WorksheetPart>();

                // Add Sheets to the Workbook.
                Sheets sheets = document.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());

                // Append a new worksheet and associate it with the workbook.
                Sheet itemSheet = new Sheet() { Id = document.WorkbookPart.GetIdOfPart(itemWorkSheetPart), SheetId = 1, Name = "Items" };
                Sheet brandSheet = new Sheet() { Id = document.WorkbookPart.GetIdOfPart(brandWorkSheetPart), SheetId = 2, Name = "Brands" };
                Sheet supplierSheet = new Sheet() { Id = document.WorkbookPart.GetIdOfPart(supplierWorkSheetPart), SheetId = 3, Name = "Suppliers" };

                sheets.Append(itemSheet);
                sheets.Append(brandSheet);
                sheets.Append(supplierSheet);

                itemWorkSheetPart.Worksheet = new Worksheet(CreateItemsSheet(products));
                brandWorkSheetPart.Worksheet = new Worksheet(CreateBrandsSheet(products));
                supplierWorkSheetPart.Worksheet = new Worksheet(CreateSuppliersSheet(products));

                workbookpart.Workbook.Save();

                document.Close();
            }
        }

        private SheetData CreateItemsSheet(IEnumerable<ReportProductView> products)
        {
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

        private SheetData CreateBrandsSheet(IEnumerable<ReportProductView> products)
        {
            var brands = products.GroupBy(p => p.Brand).Select(p => new { 
                Brand = p.Key,  
                OnHand = p.Sum(s => s.OnHand), 

                Unpublished = p.Sum(s => {
                    int unpublished = s.OnHand + s.OnPO - s.OrgQty - s.OmsQty - s.SavQty - s.StgQty - s.OnPendingOrder - s.OnHold;
                    if (unpublished < 0)
                        unpublished = 0;
                    return unpublished;
                }),

                OrgQty = p.Sum(s => s.OrgQty),
                StgQty = p.Sum(s => s.StgQty),
                OmsQty = p.Sum(s => s.OmsQty),
                SavQty = p.Sum(s => s.SavQty),

                OrgPending = p.Sum(s => s.OrgPending),
                StgPending = p.Sum(s => s.StgPending),
                OmsPending = p.Sum(s => s.OmsPending),
                SavPending = p.Sum(s => s.SavPending),

                OrgSold = p.Sum(s => s.OrgSold),
                StgSold = p.Sum(s => s.StgSold),
                OmsSold = p.Sum(s => s.OmsSold),
                SavSold = p.Sum(s => s.SavSold)
            });

            SheetData sheetData = new SheetData();

            var props = brands.First().GetType().GetProperties();

            Row headerRow = new Row();

            foreach (PropertyInfo prop in props)
            {
                headerRow.Append(new Cell() { CellValue = new CellValue(prop.Name), DataType = CellValues.String });
            }

            sheetData.Append(headerRow);

            foreach (var brand in brands)
            {
                Row row = new Row();
                foreach (PropertyInfo prop in props)
                {
                    object value = prop.GetValue(brand, null);
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

        private SheetData CreateSuppliersSheet(IEnumerable<ReportProductView> products)
        {
            var suppliers = products.GroupBy(p => p.Supplier).Select(p => new
            {
                Supplier = p.Key,
                OnHand = p.Sum(s => s.OnHand),

                Unpublished = p.Sum(s =>
                {
                    int unpublished = s.OnHand + s.OnPO - s.OrgQty - s.OmsQty - s.SavQty - s.StgQty - s.OnPendingOrder - s.OnHold;
                    if (unpublished < 0)
                        unpublished = 0;
                    return unpublished;
                }),

                OrgQty = p.Sum(s => s.OrgQty),
                StgQty = p.Sum(s => s.StgQty),
                OmsQty = p.Sum(s => s.OmsQty),
                SavQty = p.Sum(s => s.SavQty),

                OrgPending = p.Sum(s => s.OrgPending),
                StgPending = p.Sum(s => s.StgPending),
                OmsPending = p.Sum(s => s.OmsPending),
                SavPending = p.Sum(s => s.SavPending),

                OrgSold = p.Sum(s => s.OrgSold),
                StgSold = p.Sum(s => s.StgSold),
                OmsSold = p.Sum(s => s.OmsSold),
                SavSold = p.Sum(s => s.SavSold)
            });

            SheetData sheetData = new SheetData();

            var props = suppliers.First().GetType().GetProperties();

            Row headerRow = new Row();

            foreach (PropertyInfo prop in props)
            {
                headerRow.Append(new Cell() { CellValue = new CellValue(prop.Name), DataType = CellValues.String });
            }

            sheetData.Append(headerRow);

            foreach (var supplier in suppliers)
            {
                Row row = new Row();
                foreach (PropertyInfo prop in props)
                {
                    object value = prop.GetValue(supplier, null);
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
                    ReportProductView product = new ReportProductView(item.ItemLookupCode);

                    if (item.SupplierLists.Any())
                    {
                        product.Supplier = item.SupplierLists.First().Supplier.SupplierName;
                    }
                    else
                    {
                        product.Supplier = "NONE";
                    }

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

                    product.StgPending = item.EbayListingItems.Where(p => p.Listing.MarketplaceID == 1)
                        .SelectMany(p => p.OrderItems).Where(p => p.Order.IsWaitingForPayment() || p.Order.IsWaitingForShipment())
                        .Sum(p => p.QuantityPurchased);

                    product.OmsPending = item.EbayListingItems.Where(p => p.Listing.MarketplaceID == 2)
                        .SelectMany(p => p.OrderItems).Where(p => p.Order.IsWaitingForPayment() || p.Order.IsWaitingForShipment())
                        .Sum(p => p.QuantityPurchased);

                    product.SavPending = item.EbayListingItems.Where(p => p.Listing.MarketplaceID == 3)
                        .SelectMany(p => p.OrderItems).Where(p => p.Order.IsWaitingForPayment() || p.Order.IsWaitingForShipment())
                        .Sum(p => p.QuantityPurchased);

                    product.OrgPending = item.AmznListingItems.Where(p => p.MarketplaceID == 1)
                        .SelectMany(p => p.OrderItems).Where(p => p.Order.Status.Equals("Unshipped") || p.Order.Status.Equals("Pending"))
                        .Sum(p => p.QuantityOrdered);

                    product.StgSold = item.EbayListingItems.Where(p => p.Listing.MarketplaceID == 1)
                        .SelectMany(p => p.OrderItems).Where(p => p.Order.MarkedAsShipped()).Sum(p => p.QuantityPurchased);
                    product.OmsSold = item.EbayListingItems.Where(p => p.Listing.MarketplaceID == 2)
                        .SelectMany(p => p.OrderItems).Where(p => p.Order.MarkedAsShipped()).Sum(p => p.QuantityPurchased);
                    product.SavSold = item.EbayListingItems.Where(p => p.Listing.MarketplaceID == 3)
                        .SelectMany(p => p.OrderItems).Where(p => p.Order.MarkedAsShipped()).Sum(p => p.QuantityPurchased);
                    product.OrgSold = item.AmznListingItems.Where(p => p.MarketplaceID == 1)
                        .SelectMany(p => p.OrderItems).Where(p => p.Order.Status.Equals("Shipped")).Sum(p => p.QuantityOrdered);

                    products.Add(product);
                }
            }

            return products;
        }

        private MergeCell MergeTwoCells(Worksheet worksheet, string cell1Name, string cell2Name)
        {
            MergeCells mergeCells;

            if (worksheet.Elements<MergeCells>().Count() > 0)
            {
                mergeCells = worksheet.Elements<MergeCells>().First();
            }
            else
            {
                mergeCells = new MergeCells();

                // Insert a MergeCells object into the specified position.
                if (worksheet.Elements<CustomSheetView>().Count() > 0)
                {
                    worksheet.InsertAfter(mergeCells, worksheet.Elements<CustomSheetView>().First());
                }
                else if (worksheet.Elements<DataConsolidate>().Count() > 0)
                {
                    worksheet.InsertAfter(mergeCells, worksheet.Elements<DataConsolidate>().First());
                }
                else if (worksheet.Elements<SortState>().Count() > 0)
                {
                    worksheet.InsertAfter(mergeCells, worksheet.Elements<SortState>().First());
                }
                else if (worksheet.Elements<AutoFilter>().Count() > 0)
                {
                    worksheet.InsertAfter(mergeCells, worksheet.Elements<AutoFilter>().First());
                }
                else if (worksheet.Elements<Scenarios>().Count() > 0)
                {
                    worksheet.InsertAfter(mergeCells, worksheet.Elements<Scenarios>().First());
                }
                else if (worksheet.Elements<ProtectedRanges>().Count() > 0)
                {
                    worksheet.InsertAfter(mergeCells, worksheet.Elements<ProtectedRanges>().First());
                }
                else if (worksheet.Elements<SheetProtection>().Count() > 0)
                {
                    worksheet.InsertAfter(mergeCells, worksheet.Elements<SheetProtection>().First());
                }
                else if (worksheet.Elements<SheetCalculationProperties>().Count() > 0)
                {
                    worksheet.InsertAfter(mergeCells, worksheet.Elements<SheetCalculationProperties>().First());
                }
                else
                {
                    worksheet.InsertAfter(mergeCells, worksheet.Elements<SheetData>().First());
                }
            }

            // Create the merged cell and append it to the MergeCells collection.
            MergeCell mergeCell = new MergeCell() { Reference = new StringValue(cell1Name + ":" + cell2Name) };

            mergeCells.Append(mergeCell);

            return mergeCell;

        }
    }
}
