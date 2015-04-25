using BerkeleyEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketplaceManager
{
    public class ProductViewRepository
    {
        public List<ReportProductView> GetProductView()
        {
            List<ReportProductView> products = new List<ReportProductView>();

            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                dataContext.CommandTimeout = 0;

                //var items = dataContext.Items
                //    .Include("AmznListingItems.OrderItems.Order")
                //    .Include("EbayListingItems.OrderItems.Order")
                //    .ToList().Where(p =>
                //        (p.Quantity > 0 || p.OnActiveListing > 0 || p.OnPendingOrder > 0) &&
                //        !p.Inactive &&
                //        !p.DepartmentName.Equals("APPAREL") &&
                //        !p.DepartmentName.Equals("ACCESSORIES") &&
                //        !p.DepartmentName.Equals("MIXED ITEMS & LOTS"));

                var items = dataContext.Items
                    .Include("AmznListingItems.OrderItems.Order")
                    .Include("EbayListingItems.OrderItems.Order")
                    .ToList().Where(p =>
                        (p.Quantity > 0 || p.OnActiveListing > 0 || p.OnPendingOrder > 0) && !p.Inactive && p.Department != null);

                foreach (BerkeleyEntities.Item item in items)
                {
                    ReportProductView product = new ReportProductView(item.ItemLookupCode);
                    product.Supplier = string.Join(" ", item.SupplierLists.Select(p => p.Supplier.SupplierName));
                    product.Brand = item.SubDescription1;
                    product.Cost = item.Cost;
                    product.Department = item.DepartmentName;
                    product.OnPO = item.OnPurchaseOrder;
                    product.OnHand = (int)item.Quantity;
                    product.OnHold = item.OnHold;
                    product.OnPendingOrder = item.OnPendingOrder;

                    product.Org = item.AmznListingItems.Where(p => p.MarketplaceID == 1 && p.IsActive).Sum(p => p.Quantity);

                    product.StgAuc = item.EbayListingItems.Where(p => p.Listing.MarketplaceID == 1 && p.Listing.Status.Equals(EbayMarketplace.STATUS_ACTIVE) && p.Listing.Format.Equals(EbayMarketplace.FORMAT_AUCTION)).Sum(p => p.Quantity);
                    product.Stg = item.EbayListingItems.Where(p => p.Listing.MarketplaceID == 1 && p.Listing.Status.Equals(EbayMarketplace.STATUS_ACTIVE) && p.Listing.Format.Equals(EbayMarketplace.FORMAT_FIXEDPRICE)).Sum(p => p.Quantity);

                    product.OmsAuc = item.EbayListingItems.Where(p => p.Listing.MarketplaceID == 2 && p.Listing.Status.Equals(EbayMarketplace.STATUS_ACTIVE) && p.Listing.Format.Equals(EbayMarketplace.FORMAT_AUCTION)).Sum(p => p.Quantity);
                    product.Oms = item.EbayListingItems.Where(p => p.Listing.MarketplaceID == 2 && p.Listing.Status.Equals(EbayMarketplace.STATUS_ACTIVE) && p.Listing.Format.Equals(EbayMarketplace.FORMAT_FIXEDPRICE)).Sum(p => p.Quantity);

                    product.SavAuc = item.EbayListingItems.Where(p => p.Listing.MarketplaceID == 3 && p.Listing.Status.Equals(EbayMarketplace.STATUS_ACTIVE) && p.Listing.Format.Equals(EbayMarketplace.FORMAT_AUCTION)).Sum(p => p.Quantity);
                    product.Sav = item.EbayListingItems.Where(p => p.Listing.MarketplaceID == 3 && p.Listing.Status.Equals(EbayMarketplace.STATUS_ACTIVE) && p.Listing.Format.Equals(EbayMarketplace.FORMAT_FIXEDPRICE)).Sum(p => p.Quantity);

                    var ebayDuplicates = item.EbayListingItems
                        .Where(p => p.Listing.Status.Equals(EbayMarketplace.STATUS_ACTIVE))
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

    public class ReportProductView
    {
        public ReportProductView(string sku)
        {
            this.Sku = sku;
        }

        public string Brand { get; set; }

        public string Sku { get; set; }

        public string Department { get; set; }

        public decimal Cost { get; set; }

        public int OnPO { get; set; }

        public int OnHand { get; set; }

        public int OnHold { get; set; }

        public int OnPendingOrder { get; set; }

        public int Org { get; set; }

        public int Stg { get; set; }

        public int StgAuc { get; set; }

        public int Oms { get; set; }

        public int OmsAuc { get; set; }

        public int Sav { get; set; }

        public int SavAuc { get; set; }

        public string Duplicates { get; set; }

        public int EbaySold { get; set; }

        public int AmznSold { get; set; }

        public string Supplier { get; set; }
    }
}
