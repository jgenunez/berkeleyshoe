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

        public List<PendingOrder> PendingOrders { get; set; }

        public List<PendingOrder> CompletedOrders { get; set; }

        public void FetchUnshippedOrders()
        {
            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                var amznUnshippedOrders = dataContext.AmznOrders
                    .Where(p => p.Status.Equals("Unshipped") || p.Status.Equals("PartiallyShipped")).ToList()
                    .Where(p => p.OrderItems.All(s => s.ListingItem != null && s.ListingItem.Item != null)).ToList();

                foreach (var order in amznUnshippedOrders)
                {
                    PendingOrder pendingOrder = new PendingOrder();
                    pendingOrder.OrderID = order.Code;
                    pendingOrder.MarketplaceCode = dataContext.AmznMarketplaces.Single(p => p.ID == order.MarketplaceID).Code;
                    pendingOrder.PrintTime = order.PrintTime;
                    pendingOrder.ShippingPrice = order.OrderItems.Sum(p => p.ShippingPrice - p.ShippingDiscount);

                    foreach (var orderItem in order.OrderItems)
                    {
                        PendingOrderItem pendingOrderItem = new PendingOrderItem();
                        pendingOrderItem.Sku = orderItem.ListingItem.Item.ItemLookupCode;
                        pendingOrderItem.Price = orderItem.ItemPrice;
                        pendingOrderItem.Qty = orderItem.QuantityOrdered;
                        pendingOrder.Items.Add(pendingOrderItem);
                    }

                    this.PendingOrders.Add(pendingOrder);
                }
                
                var ebayUnshippedOrders = dataContext.EbayOrders.ToList()
                    .Where(p => p.IsWaitingForShipment()).ToList()
                    .Where(p => p.OrderItems.All(s => s.ListingItem != null && s.ListingItem.Item != null)).ToList();

                foreach (var order in ebayUnshippedOrders)
                {
                    PendingOrder pendingOrder = new PendingOrder();
                    pendingOrder.OrderID = order.Code;
                    pendingOrder.MarketplaceCode = dataContext.EbayMarketplaces.Single(p => p.ID == order.MarketplaceID).Code;
                    pendingOrder.PrintTime = order.PrintTime;
                    pendingOrder.ShippingPrice = order.ShippingServiceCost;

                    foreach (var orderItem in order.OrderItems)
                    {
                        PendingOrderItem pendingOrderItem = new PendingOrderItem();
                        pendingOrderItem.Sku = orderItem.ListingItem.Item.ItemLookupCode;
                        pendingOrderItem.Price = orderItem.TransactionPrice;
                        pendingOrderItem.Qty = orderItem.QuantityPurchased;
                        pendingOrder.Items.Add(pendingOrderItem);
                    }

                    this.PendingOrders.Add(pendingOrder);
                }
            }
        }

        //private void GeneratePrintFile(List<PendingOrder> orders, string file)
        //{
        //    StreamWriter stream = File.CreateText(file);

        //    stream.AutoFlush = true;

        //    stream.Write("<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>" +
        //        "<html xmlns='http://www.w3.org/1999/xhtml'><head><meta http-equiv='Content-Type' content='text/html; charset=utf-8' /><title>" +
        //        "Untitled Document</title><style>body {font-family:Tahoma, Geneva, sans-serif;font-size:10px;} p{margin:0px;margin-bottom:5px;} .customerName {font-family:Arial,Helvetica, sans-serif;font-size:large;font-weight:bold;} .titulos {font-family:Arial," +
        //        "Helvetica, sans-serif;font-size:large;font-weight:bold;" +
        //        //((gCurrentMarketplace % 2 == 0) ? "border-style:solid; border-width:1px;" : " ") +
        //        "}.fineprint {font-family:Arial, Helvetica, sans-serif;font-size:x-small;" +
        //        "font-weight: normal;text-align:center;} .repeated-order {border-width: 2px;border-color: black;border-style:solid;padding: 2px; margin-right:2px;font-family:Arial, Helvetica, sans-serif;font-size:15px;float:right;} .day-of-week {padding: 1px;background-color: black;color: white;}</style></head><body><div style='width: 800px'>");


        //    int index = 0;

        //    foreach (PendingOrder order in orders.OrderBy(p => p.Items.First().BinLocation))
        //    {
        //        index++;

        //        stream.Write("<table border='0' bordercolor='#000000' cellpadding='1' cellspacing='0' width='100%'><tr><td style='height:5in'><center>");
        //        stream.Write("<table width='800' border='0' cellspacing='0' cellpadding='0'><tr><td align='left' valign='top' width='33%'>");

        //        stream.Write("<tr>" +
        //                "    <td align='left' valign='top' width='33%'><p class='customerName'>" + order.UserName + "</p><p><strong>" +
        //                order.BuyerID + "<br />" +
        //                order.CompanyName + "<br />" +
        //                order.Street1 + "<br />" +
        //                order.Street2 + "<br />" +
        //                order.CityName + ", " + order.StateOrProvince + " " + order.PostalCode + "<br />" +
        //                order.CountryName + "<br />" +
        //                order.Phone + "</strong></p></td>" +
        //                "    <td align='center' valign='top' width='34%'><p class='titulos'>" + view.Name + "</p>" +
        //                "    <p>" + view.Name + " | eBay</p>" +
        //                "    <p><strong>THANK YOU FOR YOUR ORDER</strong></p>" + GetRepeatedInfo(order.PrintTime) + "</td>" +
        //                "    <td align='right' valign='top' width='33%'><p>" + order.CreatedTime.ToShortDateString() + "</p>" +
        //                "    <p><h2>" + order.SalesRecordNumber + "</h2></p>" +
        //                "    <p><font face='Free 3 of 9 Extended' size='+2'>*" + order.SalesRecordNumber + "*</font></p>" +
        //                "    <p><h2>" + GetShippingInfo(order) + "</h2></p></td>" +
        //                "  </tr>");

        //        stream.Write("</table></center>");

        //        stream.Write("<table width='800' border='1' cellspacing='0' cellpadding='2'><tr><td width='7%'><div align='center'><b>Qty</b></font></div></td><td width='50%'><div align='center'><font><b>Product</b></font></div></td><td width='15%'><div align='center'><font><b>Condition</b></font></div></td> <td width='15%'><div align='center'><font><b>Price</b></font></div></td><td width='10%'><div align='center'><font><b>Total</b></font></div></td></tr>");

        //        foreach (EbayOrderItem orderItem in order.OrderItems)
        //        {
        //            decimal totalPrice = orderItem.QuantityPurchased * orderItem.TransactionPrice;
        //            String listingFormat = orderItem.ListingItem.Listing.Format.Equals("Chinese") ? "[AUCTION] " : "";

        //            string location = orderItem.ListingItem.Item != null ? orderItem.ListingItem.Item.BinLocation : "";
        //            string conditionDisplayName = orderItem.ListingItem.Item != null ? orderItem.ListingItem.Item.GetConditionCode() : "";
        //            string conditionDescription = orderItem.ListingItem.Item != null ? orderItem.ListingItem.Item.ExtendedDescription : "";

        //            stream.Write("  <tr>" +
        //                        "    <td><div align='center'>" + orderItem.QuantityPurchased + "</div></td>" +
        //                        "    <td><div align='left'><b>" + listingFormat + orderItem.ListingItem.Listing.Title + "</b><br />" +
        //                        "        <strong>eBay Item: " + orderItem.Code + " | SKU: " + orderItem.ListingItem.Sku + " | " + location + "</strong></div></td>" +
        //                        "    <td align='center'><b>" + conditionDisplayName + ": " + conditionDescription + "</b></td>" +
        //                        "    <td><div align='right'>" + orderItem.TransactionPrice.ToString("C") + "</div></td>" +
        //                        "    <td><div align='right'>" + totalPrice.ToString("C") + "</div></td>" +
        //                        "  </tr>");
        //        }

        //        stream.Write("<tr><td><div align='center'></div></td><td><div align='left'></div></td><td><div align='left'></div></td>" +
        //                "<td><div align='right'>Shipping:</div></td><td><div align='right'>" +
        //                order.ShippingServiceCost.ToString("C") +
        //                "</div></td></tr>");

        //        if (order.AdjustmentAmount != 0)
        //        {
        //            stream.Write("<tr><td><div align='center'></div></td><td><div align='left'></div></td>" +
        //                        "<td><div align='right'>Discounts:</div></td><td><div align='right'>$" +
        //                        order.AdjustmentAmount + "</div></td></tr>");
        //        }

        //        stream.Write("<tr><td><div align='center'></div></td><td><div align='left'></div></td><td><div align='left'></div></td>" +
        //                    "<td><div align='right'>Order total:</div></td><td><div align='right'>" +
        //                    order.Total.ToString("C") +
        //                    "</div></td></tr></table>");

        //        stream.Write("<table width='100%' align='center' cellpadding='10' cellspacing='0' border='0'>" +
        //                    "<tr><td align='left' valign='top' width='50%'><p><strong>Exchanges:</strong> " +
        //                    "Looking to exchange for a different size? Contact us through eBay messages to  " +
        //                    "see if we can find the right pair for you.</p><p><strong>Returns:</strong> Our return " +
        //                    "policies are stated in our profile page at <strong>eBay.com</strong>. Please contact us through eBay messages <b>first</b> for a Return Authorization number. " +
        //                    "We accept returns within <strong>30 days</strong> after receiving the product. " +
        //                    "The items <strong>must not be worn</strong> and should be returned in their <b>original box</b> and  packaging. " +
        //                    "<u>Buyer is responsible for returning shipping costs</u>.</p>" +
        //                    "<p align='center'><b>PLEASE CONTACT OUR CUSTOMER SERVICE</b></p>" +
        //                    "<p align='center'><b>DEPARTMENT FOR RETURN INSTRUCTIONS</b></p>" +
        //                    "We guarantee a reply to your return request within 24hrs. If you do not receive anything, we recommend checking your spam/junk mail." +
        //                    "</td><td align='left' valign='top' width='50%'>" +
        //                    "<p>To better serve our customers in the future, please write down the reason for returning the item:</p>" +
        //                    "<p>&nbsp;</p><p>&nbsp;</p><p>&nbsp;</p><p>&nbsp;</p><p>&nbsp;</p><p><strong>Once we receive the shoes we will refund your order as stated in the eBay page of the product. " +
        //                    "Inspection and processing usually take </strong><strong>3-5 business days</strong><strong>.</strong><br /><strong>" +
        //                    "We don't refund original  shipping charges.</strong></p></td></tr></table><p class='fineprint'>" +
        //                    "*You MUST INCLUDE THIS PAGE in your return; otherwise we will have trouble  processing your order. " +
        //                    "Contact us through eBay messages for any questions.</p><p align='center'><strong>VISIT OUR WEBSITE</strong></p>");

        //        stream.Write("</td></tr></table>");

        //        if (index % 2 == 0)
        //        {
        //            stream.Write("<p style='page-break-before: always;'>&nbsp;</p>");
        //        }
        //        else
        //        {
        //            stream.Write("<hr style='margin:2px;padding:0px;' />");
        //        }

        //        stream.Flush();
        //    }

        //    var printOrders = orders.Where(p => p.PrintTime.HasValue == false);

        //    string POBoxPattern = @"(?i)\b(?:p(?:ost)?\.?\s*[o0](?:ffice)?\.?\s*b(?:[o0]x)?|b[o0]x)";

        //    var poBoxOrders = printOrders.Where(p => Regex.IsMatch(p.Street1 ?? "", POBoxPattern) || Regex.IsMatch(p.Street2 ?? "", POBoxPattern));

        //    stream.Write("<h1>eBay " + view.Name + " orders summary " + DateTime.Now.ToShortDateString() + "</h1><hr>" +
        //                "<p>&nbsp;<b>TOTAL ORDERS       :" + printOrders.Count() + "</b></p>" +
        //                "<p>&nbsp;<b>TOTAL NEXT DAY     :" + printOrders.Where(p => p.ShippingService.Contains("Overnight") || p.ShippingService.Contains("NextDay")).Count() + "</b></p>" +
        //                "<p>&nbsp;<b>TOTAL PO BOX       :" + poBoxOrders.Count() + "</b></p>" +
        //                "<p>&nbsp;<b>TOTAL INTERNATIONAL:" + printOrders.Where(p => !p.CountryName.Contains("United States")).Count() + "</b></p>" +
        //                "<p>&nbsp;<b>TOTAL ITEMS        :" + printOrders.Sum(p => p.OrderItems.Sum(s => s.QuantityPurchased)) + "</b></p>" +
        //                "<p>&nbsp;<b>TOTAL SHIPPING     :" + printOrders.Sum(p => p.ShippingServiceCost).ToString("C") + "</b></p>" +
        //                "<p>&nbsp;<b>GRAND TOTAL        :" + printOrders.Sum(p => p.Total).ToString("C") + "</b></p>" +
        //                "<p>&nbsp;</p><p>&nbsp;</p><p>DETAILS</p><ul>");


        //    var brands = printOrders.SelectMany(p => p.OrderItems).GroupBy(p => p.ListingItem.Item.SubDescription1).OrderBy(p => p.Key);

        //    foreach (var brand in brands)
        //    {
        //        stream.Write("<li>" + brand.Key + " (" + brand.Sum(s => s.QuantityPurchased) + ") " + string.Format("{0:C}", brand.Sum(s => s.TransactionPrice)) + "</li>");
        //    };

        //    stream.Write("</ul></div></body></html>");

        //    stream.Flush();

        //    stream.Close();
        //}

    }

    public class PendingOrder
    {
        public string MarketplaceCode { get; set; }

        public string OrderID { get; set; }

        public List<PendingOrderItem> Items { get; set; }

        public DateTime? PrintTime { get; set; }

        public decimal ShippingPrice { get; set; }
    }

    public class PendingOrderItem
    {
        public string Sku {get; set;}

        public string BinLocation { get; set; }

        public string Condition { get; set; }

        public string ConditionDescription { get; set; }

        public int Qty {get; set;}

        public decimal Price {get; set;}
    }
}
