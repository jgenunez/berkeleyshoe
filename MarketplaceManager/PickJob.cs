using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BerkeleyEntities;
using System.Data.Entity;
using System.Data;
using System.IO;

namespace MarketplaceManager
{
    public class PickJob
    {
        public string Code { get; set; }

        public List<PickItem> PendingItems { get; set; }

        public List<PickItem> CompletedItems { get; set; }

        //public void FetchUnshippedOrders()
        //{
        //    using (berkeleyEntities dataContext = new berkeleyEntities())
        //    {

        //        var unshippedOrders = dataContext.AmznOrders
        //            .Where(p => p.Status.Equals("Unshipped") || p.Status.Equals("PartiallyShipped")).ToList()
        //            .Where(p => p.OrderItems.All(s => s.ListingItem != null && s.ListingItem.Item != null)).ToList();


        //        foreach (var orderItem in unshippedOrders.SelectMany(p => p.OrderItems))
        //        {
        //            PickItem pickItem = new PickItem();
        //            pickItem.Sku = orderItem.ListingItem.Item.ItemLookupCode;
        //            pickItem.
        //        }
                

        //        var printList = unshippedOrders.Where(p => !p.PrintTime.HasValue);

        //        DateTime printTime = DateTime.Now;

        //        string pickJobID = printTime.Year.ToString() + printTime.Month.ToString().PadLeft(2, '0') + printTime.Day.ToString().PadLeft(2, '0');

        //        int i = 1;

        //        while (File.Exists(fbd.SelectedPath + "//" + string.Format("{0}-{1}({2}).xlsx", pickJobID, i, view.Code)))
        //        {
        //            i++;
        //        }

        //        pickJobID = string.Format("{0}-{1}({2})", pickJobID, i, view.Code);

        //        var auditEntries = printList.SelectMany(p => p.OrderItems)
        //            .Select(p => new PickItem { Sku = p.ListingItem.Sku, Qty = p.QuantityOrdered, Price = p.ItemPrice, OrderID = p.Order.Code });

        //        GeneratePrintFile(view, unshippedOrders, fbd.SelectedPath + "//" + pickJobID + ".html");

        //        ReportGenerator reportGenerator = new ReportGenerator(fbd.SelectedPath + "//" + pickJobID + ".xlsx", auditEntries, typeof(PickItem));

        //        reportGenerator.GenerateExcelReport();

        //        ImportToRMS(pickJobID, printList.ToList());

        //        foreach (var order in printList)
        //        {
        //            order.PrintTime = printTime;
        //        }

        //        dataContext.SaveChanges();
        //    }
        //}

    }

    public class PickItem
    {
        public string MarketplaceCode { get; set; }

        public string OrderID { get; set; }

        public string Sku { get; set; }

        public int Qty { get; set; }

        public decimal Price { get; set; }

        public DateTime? PrintTime { get; set; }
    }
}
