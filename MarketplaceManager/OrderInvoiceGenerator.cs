using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using BerkeleyEntities;

namespace MarketplaceManager
{
    public class OrderInvoiceGenerator
    {
        private string _marketplace;

        public OrderInvoiceGenerator(string marketplace)
        {
            _marketplace = marketplace;
        }

        //public void ConvertToHtmlFile(List<EbayOrder> orders)
        //{
        //    StringBuilder sb = new StringBuilder();


        //    sb.Append("<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>");

        //    streamWriter.AutoFlush = true;

        //    streamWriter.Write(+
        //        "<html xmlns='http://www.w3.org/1999/xhtml'><head><meta http-equiv='Content-Type' content='text/html; charset=utf-8' /><title>" +
        //        "Untitled Document</title><style>body {font-family:Tahoma, Geneva, sans-serif;font-size:10px;} p{margin:0px;margin-bottom:5px;} .customerName {font-family:Arial,Helvetica, sans-serif;font-size:large;font-weight:bold;} .titulos {font-family:Arial," +
        //        "Helvetica, sans-serif;font-size:large;font-weight:bold;" +
        //        //((gCurrentMarketplace % 2 == 0) ? "border-style:solid; border-width:1px;" : " ") +
        //        "}.fineprint {font-family:Arial, Helvetica, sans-serif;font-size:x-small;" +
        //        "font-weight: normal;text-align:center;} .repeated-order {border-width: 2px;border-color: black;border-style:solid;padding: 2px; margin-right:2px;font-family:Arial, Helvetica, sans-serif;font-size:15px;float:right;} .day-of-week {padding: 1px;background-color: black;color: white;}</style></head><body><div style='width: 800px'>");

        //    int standardOrders = 0, nextDayOrders = 0, internationalOrders = 0;

        //    foreach (EbayOrder order in orders)
        //    {

        //        String shippingInfo = "", repeatedInfo = "";

        //        if (order.repeated)
        //        {
        //            repeatedInfo = "<p class='repeated-order'><span class='day-of-week'>" +
        //                            order.originalDate.DayOfWeek.ToString().Substring(0, 3).ToUpper() +
        //                            "</span>&nbsp;" + order.originalDate.ToShortDateString() + "&nbsp;</p>";
        //        }
        //        else
        //        {
        //            repeatedInfo = "";
        //        }

        //        standardOrders++;


        //        shippingInfo += " | Zone: " + order.zone;

        //        streamWriter.Write("<table border='0' bordercolor='#000000' cellpadding='1' cellspacing='0' width='100%'><tr><td style='height:5in'><center>");
        //        streamWriter.Write("<table width='800' border='0' cellspacing='0' cellpadding='0'><tr><td align='left' valign='top' width='33%'>");

        //        streamWriter.Write("<tr>" +
        //                 "    <td align='left' valign='top' width='33%'><p class='customerName'>" + order.UserName + "</p><p><strong>" +
        //                 order.BuyerID + "<br />" +
        //                 order.CompanyName + "<br />" +
        //                 order.Street1 + " | " + order.Street2 + "<br />" +
        //                 order.CityName + ", " + order.StateOrProvince + " " + order.PostalCode + "<br />" +
        //                 order.CountryName + "<br />" +
        //                 order.Phone + "</strong></p></td>" +
        //                 "    <td align='center' valign='top' width='34%'><p class='titulos'>" + marketPlaces[gCurrentMarketplace].Website + "</p>" +
        //                 "    <p>" + _marketplace + " | eBay</p>" +
        //                 "    <p><strong>THANK YOU FOR YOUR ORDER</strong></p>" + lrepeatedInfo + "</td>" +
        //                 "    <td align='right' valign='top' width='33%'><p>" + order.CreatedTime.ToShortDateString() + " - " + standardOrders + "</p>" +
        //                 "    <p><h2>" + order.SalesRecordNumber + "</h2></p>" +
        //                 "    <p><font face='Free 3 of 9 Extended' size='+2'>*" + order.SalesRecordNumber + "*</font></p>" +
        //                 "    <p><h2>" + shippingInfo + "</h2></p></td>" +
        //                 "  </tr>");
        //        streamWriter.Write("</table></center>");

        //        // Now write the details of the order
        //        streamWriter.Write("<table width='800' border='1' cellspacing='0' cellpadding='2'><tr><td width='7%'><div align='center'><b>Qty</b></font></div></td><td width='50%'><div align='center'><font><b>Product</b></font></div></td><td width='15%'><div align='center'><font><b>Condition</b></font></div></td> <td width='15%'><div align='center'><font><b>Price</b></font></div></td><td width='10%'><div align='center'><font><b>Total</b></font></div></td></tr>");
        //        //Decimal ltotalShipping = 0, ltotalDiscounts = 0;

        //        double ltotalRefund = 0;
        //        foreach (EbayOrderItem orderItem in order.OrderItems)
        //        {
        //            String ltitle = (orderItem.Variation != null && !String.IsNullOrEmpty(orderItem.Variation.SKU)) ? orderItem.Variation.VariationTitle : orderItem.Item.Title;
        //            String[] lorderline = orderItem.OrderLineItemID.Split(new char[] { '-' });
        //            String ltype = (lorderline.Length > 1 && lorderline[1].Length > 2) ? "" : "[AUCTION] ";

        //            int lqty = orderItem.QuantityPurchased;

        //            if (orderItem.RefundArray != null && orderItem.RefundArray.Count > 0)
        //            {
        //                foreach (RefundType lrefu in orderItem.RefundArray)
        //                {
        //                    ltotalRefund += lrefu.TotalRefundToBuyer.Value;
        //                } // foreach

        //            }

        //            double ltotalprice = lqty * orderItem.TransactionPrice.Value;

        //            // Let's write to the file
        //            streamWriter.Write("  <tr>" +
        //                     "    <td><div align='center'>" + lqty + "</div></td>" +
        //                     "    <td><div align='left'><b>" + ltype + ltitle + "</b><br />" +
        //                     "        <strong>eBay Item: " + orderItem.Code + " | SKU: " + orderItem.ListingItem.Item.ItemLookupCode + "</strong></div></td>" +
        //                     "    <td align='center'><b>" + orderItem.ListingItem.Listing.Condition + "</b></td>" +
        //                     "    <td><div align='right'>" + orderItem.TransactionPrice + "</div></td>" +
        //                     "    <td><div align='right'>" + ltotalprice.ToString("C") + "</div></td>" +
        //                     "  </tr>");
        //        } // foreach orderitem

        //        streamWriter.Write("<tr><td><div align='center'></div></td><td><div align='left'></div></td><td><div align='left'></div></td>" +
        //                 "<td><div align='right'>Shipping:</div></td><td><div align='right'>" +
        //                 order.ShippingServiceSelected.ShippingServiceCost.Value.ToString("C") +
        //                 "</div></td></tr>");

        //        if (ltotalRefund > 0)
        //            streamWriter.Write("<tr><td><div align='center'></div></td><td><div align='left'></div></td>" +
        //                     "<td><div align='right'>Discounts:</div></td><td><div align='right'>$" +
        //                     ltotalRefund +
        //                     "</div></td></tr>");

        //        streamWriter.Write("<tr><td><div align='center'></div></td><td><div align='left'></div></td><td><div align='left'></div></td>" +
        //                 "<td><div align='right'>Order total:</div></td><td><div align='right'>" +
        //                 order.Total.Value.ToString("C") +
        //                 "</div></td></tr></table>");

        //        if (!order.repeated) lgrandShipping += order.ShippingServiceSelected.ShippingServiceCost.Value;
        //        if (!order.repeated) lgrandTotal += order.Total.Value;

        //        // Now write the bottom of the invoice
        //        streamWriter.Write("<table width='100%' align='center' cellpadding='10' cellspacing='0' border='0'>" +
        //                 "<tr><td align='left' valign='top' width='50%'><p><strong>Exchanges:</strong> " +
        //                 "Looking to exchange for a different size? Contact us through eBay messages to  " +
        //                 "see if we can find the right pair for you.</p><p><strong>Returns:</strong> Our return " +
        //                 "policies are stated in our profile page at <strong>eBay.com</strong>. Please contact us through eBay messages <b>first</b> for a Return Authorization number. " +
        //                 "We accept returns within <strong>30 days</strong> after receiving the product. " +
        //                 "The items <strong>must not be worn</strong> and should be returned in their <b>original box</b> and  packaging. " +
        //                 "<u>Buyer is responsible for returning shipping costs</u>.</p>" +
        //                 "<p align='center'><b>PLEASE CONTACT OUR CUSTOMER SERVICE</b></p>" +
        //                 "<p align='center'><b>DEPARTMENT FOR RETURN INSTRUCTIONS</b></p>" +
        //                 "We guarantee a reply to your return request within 24hrs. If you do not receive anything, we recommend checking your spam/junk mail." +
        //                 "</td><td align='left' valign='top' width='50%'>" +
        //                 "<p>To better serve our customers in the future, please write down the reason for returning the item:</p>" +
        //                 "<p>&nbsp;</p><p>&nbsp;</p><p>&nbsp;</p><p>&nbsp;</p><p>&nbsp;</p><p><strong>Once we receive the shoes we will refund your order as stated in the eBay page of the product. " +
        //                 "Inspection and processing usually take </strong><strong>3-5 business days</strong><strong>.</strong><br /><strong>" +
        //                 "We don't refund original  shipping charges.</strong></p></td></tr></table><p class='fineprint'>" +
        //                 "*You MUST INCLUDE THIS PAGE in your return; otherwise we will have trouble  processing your order. " +
        //                 "Contact us through eBay messages for any questions.</p><p align='center'><strong>VISIT OUR WEBSITE " +
        //                 marketPlaces[gCurrentMarketplace].Website + "  " + marketPlaces[gCurrentMarketplace].SpecialOffer + "</strong></p>");

        //        streamWriter.Write("</td></tr></table>");

        //        if (standardOrders % 2 == 0)
        //            streamWriter.Write("<p style='page-break-before: always;'>&nbsp;</p>");
        //        else
        //            streamWriter.Write("<hr style='margin:2px;padding:0px;' />");

        //        streamWriter.Flush();
        //    }; // foreach
        //}

    }
}
