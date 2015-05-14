using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;



namespace BerkeleyEntities
{
    public enum DimensionName { USMenSize, USWomenSize, USBabySize, USYouthSize, EUSize, Width, Color, Waist, Inseam, Size, Unknown };

    public class Attribute
    {
        public string Code { get; set; }

        public string Value { get; set; }
    }

    public partial class Item
    {
        public override string ToString()
        {
            return this.ItemLookupCode;
        }

        public string ClassName
        {
            get
            {
                if (this.ItemClass != null)
                {

                    return this.ItemClass.ItemLookupCode;
                }
                else
                {
                    return this.ItemLookupCode.Split(new Char[1] { '-' })[0];
                }
            }
        }

        public ItemClass ItemClass
        {
            get
            {
                if (this.ItemClassComponents.Count > 0)
                {
                    ItemClass itemClass = this.ItemClassComponents.First().ItemClass;
                    return itemClass;
                }
                else
                {
                    return null;
                }
            }
        }

        public string GetConditionCode()
        {
            string conditionCode = string.Empty;

            if (this.Notes != null)
            {
                if (this.Notes.Contains("PRE"))
                {
                    conditionCode = "PRE";
                }
                else if (this.Notes.Contains("NWB"))
                {
                    conditionCode = "NWB";
                }
                else if (this.Notes.Contains("NWD"))
                {
                    conditionCode = "NWD";
                }
                else
                {
                    conditionCode = "NEW";
                }
            }
            else
            {
                conditionCode = "NEW";
            }

            return conditionCode;
        }

        public int QtyAvailable
        {
            get 
            {
                int available = (int)this.Quantity - this.OnPendingOrder - this.OnHold - this.AuctionWithBidQty;

                if (available < 0)
                {
                    available = 0;
                }

                return available;
            }
        }

        public int AuctionWithBidQty
        {
            get 
            {
                var activeAuctions = this.EbayListingItems.Where(p => p.Listing.Status.Equals(EbayMarketplace.STATUS_ACTIVE) && p.Listing.Format.Equals(EbayMarketplace.FORMAT_AUCTION));

                return activeAuctions.Where(p => p.Listing.BidCount > 0).Sum(p => p.Quantity);
            }
        }

        public int OnActiveListing 
        {
            get 
            {
                int ebayQty = this.EbayListingItems.Where(p => p.Listing.Status.Equals("Active")).Sum(p => p.Quantity);
                int amznQty = this.AmznListingItems.Where(p => p.IsActive).Sum(p => p.Quantity);

                return ebayQty + amznQty;
            }
        }

        public int OnHold 
        {
            get 
            {
                return (int)this.TransactionHoldEntries.Sum(p => p.QuantityPurchased);
            }
        }

        //public int OnPurchaseOrder 
        //{
        //    get 
        //    {
        //        return (int)this.PurchaseOrderEntries.Where(p => p.PurchaseOrder.Status == 0).Sum(p => p.QuantityOrdered);
        //    }
        //}

        public int OnPendingOrder
        {
            get
            {
                return EbayWaitingPayment + EbayUnshipped + AmznWaitingPayment + AmznUnshipped;
            }
        }

        public int EbayWaitingPayment
        {
            get 
            {
                return this.EbayListingItems
                    .SelectMany(p => p.OrderItems)
                    .Where(p => p.Order.IsWaitingForPayment())
                    .Sum(p => p.QuantityPurchased);
            }
        }

        public int EbayUnshipped 
        {
            get 
            {
                return this.EbayListingItems
                    .SelectMany(p => p.OrderItems)
                    .Where(p => p.Order.IsWaitingForShipment())
                    .Sum(p => p.QuantityPurchased);
            }
        }

        public int AmznWaitingPayment
        {
            get 
            {
                return this.AmznListingItems.SelectMany(p => p.OrderItems).Where(p => p.Order.Status.Equals("Pending")).Sum(p => p.QuantityOrdered);
            }
        }

        public int AmznUnshipped
        {
            get
            {
                return this.AmznListingItems.SelectMany(p => p.OrderItems)
                    .Where(p => p.Order.Status.Equals("Unshipped") || p.Order.Status.Equals("PartiallyShipped"))
                    .Sum(p => p.QuantityOrdered - p.QuantityShipped);
            }
        }

        public bool HasAsin()
        {
            if (this.AmznListingItems.Any(p => !p.ASIN.Equals("UNKNOWN")))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public Dictionary<DimensionName, Attribute> Dimensions { get; set; }

        public void SetAttribute(string name, string value)
        {
            //XmlSerializer serializer = new XmlSerializer(dynamic);
            //dynamic attributes = 

            //var root = XElement.Parse(this.Notes);

            //var target = root.Element(name);

            //if (target != null)
            //{
            //    target.Value = value;
            //}
            //else
            //{
            //    target = new XElement(XName.Get(name, null));
            //}

            
        }

        public string GetAttribute(string name)
        {
            return null;
        }

        public string ProductType 
        {
            get
            {
                if (this.Department != null)
                {
                    Division productType = Department.DepartmentDivisions
                        .Select(p => p.Division)
                        .SingleOrDefault(p => p.Type.Equals("Product Type"));

                    if (productType != null)
                    {
                        return productType.Name;
                    }

                }

                return "UNKNOWN";
            }
        }

        public string Gender 
        { 
            get      
            {
                if (Department != null)
                {
                    return Department.DepartmentDivisions
                    .Select(p => p.Division)
                    .Single(p => p.Type.Equals("Gender")).Name;
                }
                else
                {
                    return null;
                }            
            }
        
        }

        public string DepartmentName 
        {
            get 
            {
                if (this.Department == null)
                    return "";
                else
                    return this.Department.Name;
            }
        }

        public string CategoryName 
        {
            get 
            {
                if (this.Category == null)
                    return "";

                else
                    return this.Category.Name;
            }
        }

        public string AgeGroup
        {
            get
            {
                if (Department != null)
                {
                    return Department.DepartmentDivisions
                        .Select(p => p.Division)
                        .Single(p => p.Type.Equals("Age Group")).Name;
                }
                else
                {
                    return null;
                }
                
            }
        }

        public string GTIN
        {
            get
            {
                foreach (Alias alias in Aliases)
                {
                    if (IsValidGTIN(alias.Alias1))
                    {
                        return alias.Alias1;
                    }
                }

                return null;
            }
        }

        public string GTINType
        {
            get
            {
                string gtin = GTIN;

                if (gtin != null)
                {
                    switch (gtin.Length)
                    {
                        case 8:
                            return "UPC";
                        case 12:
                            return "UPC";
                        case 13:
                            return "EAN";
                        case 14:
                            return "EAN";
                        default:
                            // wrong number of digits
                            return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            
        }

        public string PictureBasePath 
        {
            get 
            {
                return @"P:\products" + @"\" + SubDescription1 + @"\";
            }
        }

        private string[] GetAttributes() 
        {
            string[] attributes = null;

            if (this.ItemClass == null)
            {
                string[] skuDetails = ItemLookupCode.Split(new Char[1] {'-'});
                attributes = new string[skuDetails.Length - 1];
                for (int i = 0; i < attributes.Length; i++)
                {
                    attributes[i] = skuDetails[i + 1];
                }
            }
            else
            {
                ItemClassComponent component = this.ItemClassComponents.First();

                switch (this.ItemClass.Dimensions)
                {
                    case 1: attributes = new string[1] { component.Detail1 }; break;
                    case 2: attributes = new string[2] { component.Detail1, component.Detail2 }; break;
                    case 3: attributes = new string[3] { component.Detail1, component.Detail2, component.Detail3 }; break;
                }
            }

            return attributes;
        }

        private bool IsValidGTIN(string code)
        {
            if (code != (new Regex("[^0-9]")).Replace(code, ""))
            {
                // is not numeric
                return false;
            }
            // pad with zeros to lengthen to 14 digits
            switch (code.Length)
            {
                case 8:
                    code = "000000" + code;
                    break;
                case 12:
                    code = "00" + code;
                    break;
                case 13:
                    code = "0" + code;
                    break;
                case 14:
                    break;
                default:
                    // wrong number of digits
                    return false;
            }

            // calculate check digit
            int[] a = new int[13];
            a[0] = int.Parse(code[0].ToString()) * 3;
            a[1] = int.Parse(code[1].ToString());
            a[2] = int.Parse(code[2].ToString()) * 3;
            a[3] = int.Parse(code[3].ToString());
            a[4] = int.Parse(code[4].ToString()) * 3;
            a[5] = int.Parse(code[5].ToString());
            a[6] = int.Parse(code[6].ToString()) * 3;
            a[7] = int.Parse(code[7].ToString());
            a[8] = int.Parse(code[8].ToString()) * 3;
            a[9] = int.Parse(code[9].ToString());
            a[10] = int.Parse(code[10].ToString()) * 3;
            a[11] = int.Parse(code[11].ToString());
            a[12] = int.Parse(code[12].ToString()) * 3;
            int sum = a[0] + a[1] + a[2] + a[3] + a[4] + a[5] + a[6] + a[7] + a[8] + a[9] + a[10] + a[11] + a[12];
            int check = (10 - (sum % 10)) % 10;
            // evaluate check digit
            int last = int.Parse(code[13].ToString());

            return check == last;
        }

        public override bool Equals(object obj)
        {
            Item item = obj as Item;

            return this.ID == item.ID;
        }

        public int DimCount
        {
            get 
            {
                int dimCount = 0;

                if (this.ItemClass != null)
                {
                    dimCount = this.ItemClass.Dimensions;
                }
                else
                {
                    dimCount = this.ItemLookupCode.Split(new Char[1] { '-' }).Length - 1;
                }

                return dimCount;
            }
            
        }
    }

    
}
