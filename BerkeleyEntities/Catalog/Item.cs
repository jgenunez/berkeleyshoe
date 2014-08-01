using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Globalization;



namespace BerkeleyEntities
{


    public partial class Item
    {


        public override string ToString()
        {
            return this.ItemLookupCode;
        }

        public int QtyAvailable
        {
            get 
            {
                int available = (int)this.Quantity - this.OnPendingOrder - this.OnHold;

                if (available < 0)
                {
                    available = 0;
                }

                return available;
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

        public int OnPurchaseOrder 
        {
            get 
            {
                return (int)this.PurchaseOrderEntries.Where(p => p.PurchaseOrder.Status == 0).Sum(p => p.QuantityOrdered);
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

        public int OnPendingOrder 
        {
            get
            {
                return EbayWaitingPayment + EbayUnshipped + AmznWaitingPayment + AmznUnshipped;
            }
        }

        public string Brand 
        {
            get 
            {
                return this.SubDescription1;
            }
        }

        public Dictionary<string, Attribute> Attributes { get; set; }

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
                    return this.ItemLookupCode.Split(new Char[1] {'-'})[0];
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

        //public int ActiveListingQty
        //{
        //    get
        //    {
        //        return SyncListings.Where(p => p.Active == 1).Sum(p => p.Quantity);
        //    }
        //}

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
                    if (IsValidGtin(alias.Alias1))
                    {
                        return alias.Alias1;
                    }
                }

                return null;
            }
        }

        public string ASIN 
        {
            get 
            {
                return null;
            }
        }

        public string GtinType
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

        //public string AmznItemType 
        //{
        //    get 
        //    {
        //        if (Category == null || Department == null)
        //        {
        //            return null;
        //        }

        //        string departmentCode = Department.code;
        //        string categoryName = Category.Name;

        //        if (departmentCode.Equals("53120"))
        //        {
        //            if (categoryName.Equals("Loafers & slip ons"))
        //            {
        //                return "loafers-shoes";
        //            }
        //            else { return "oxfords-shoes"; }
        //        }

        //        else if (departmentCode.Equals("11498") || departmentCode.Equals("53557"))
        //        {
        //            if (categoryName.Equals("Hiking, trail")) { return "hiking-boots"; }

        //            else if (categoryName.Equals("Rainboots")) { return "rain-boots"; }

        //            else if (categoryName.Equals("Riding, equestrian")) { return "equestrian-boots"; }

        //            else if (categoryName.Equals("Work & safety")) { return "work-boots"; }

        //            else { return "boots"; }
        //        }

        //        else if (departmentCode.Equals("11504") || departmentCode.Equals("62107"))
        //        {
        //            if (categoryName.Equals("Mules")) { return "clogs-and-mules-shoes"; }

        //            else if (categoryName.Equals("Sport sandals")) { return "athletic-sandals"; }

        //            else { return "sandals"; }
        //        }

        //        else if (departmentCode.Equals("55793"))
        //        {
        //            if (categoryName.Equals("Mules")) { return "clogs-and-mules-shoes"; }

        //            else { return "pumps-shoes"; }
        //        }

        //        else if (departmentCode.Equals("24087"))
        //        {
        //            if (categoryName.Equals("Boat shoes")) { return "athletic-boating-shoes"; }

        //            else if (categoryName.Equals("Fashion sneakers")) { return "fashion-sneakers"; }

        //            else if (categoryName.Equals("Oxfords")) { return "oxfords-shoes"; }

        //            else { return "loafers-shoes"; }
        //        }

        //        else if (departmentCode.Equals("45333"))
        //        {
        //            if (categoryName.Equals("Boat shoes")) { return "athletic-boating-shoes"; }

        //            else if (categoryName.Equals("Clogs") || categoryName.Equals("Mules")) { return "clogs-and-mules-shoes"; }

        //            else if (categoryName.Equals("Loafers & moccasins")) { return "loafers-shoes"; }

        //            else if (categoryName.Equals("Oxfords")) { return "oxfords-shoes"; }

        //            else { return "flats-shoes"; }
        //        }

        //        else if (departmentCode.Equals("15709") || departmentCode.Equals("95672"))
        //        {
        //            if (categoryName.Equals("Water shoes")) { return "athletic-water-shoes"; }

        //            else if (categoryName.Equals("Walking")) { return "walking-shoes"; }

        //            else if (categoryName.Equals("Skateboarding")) { return "skateboarding-shoes"; }

        //            else if (categoryName.Equals("Running, cross training")) { return "cross-trainer-shoes"; }

        //            else if (categoryName.Equals("Hiking, trail")) { return "hiking-shoes"; }

        //            else if (categoryName.Equals("Golf shoes")) { return "golf-shoes"; }

        //            else if (categoryName.Equals("Dance")) { return "dance-shoes"; }

        //            else if (categoryName.Equals("Bowling shoes")) { return "bowling-shoes"; }

        //            else if (categoryName.Equals("Basketball shoes")) { return "basketball-shoes"; }

        //            else if (categoryName.Equals("Fashion sneakers")) { return "fashion-sneakers"; }

        //            else { return "cross-trainer-shoes"; }
        //        }

        //        else if (departmentCode.Equals("11501") || departmentCode.Equals("53548"))
        //        {
        //            return "work-shoes";
        //        }

        //        else if (departmentCode.Equals("11505") || departmentCode.Equals("11632"))
        //        {
        //            return "slippers";
        //        }

        //        else { return null; }
        //    }
        //}

        //public string AmznGender 
        //{
        //    get 
        //    {
        //        string gender = Gender;

        //        if (gender.Equals("MENS") || gender.Equals("MEN"))
        //        {
        //            gender = "mens";
        //        }
        //        else if (gender.Equals("WOMENS") || gender.Equals("WOMEN"))
        //        {
        //            gender = "womens";
        //        }
        //        else if (gender.Equals("UNISEX") || gender.Equals("UNISEXS"))
        //        {
        //            gender = "unisex";
        //        }
        //        else
        //        {
        //            gender = null;
        //        }

        //        return gender;
        //    }
        //}

        //public string AmznSize 
        //{
        //    get 
        //    {
        //        string size = Dimensions.Single(p => p.Name.Equals(DimensionName.ShoeSize)).Value;

        //        if (AmznGender.Equals("unisex"))
        //        {
        //            //string womenSize = (double.Parse(size) + 1.5).ToString();

        //            //return size + " " + this.AmznWidth + "US Men / " + womenSize + " " + toAmznWidth(this.width, "WOMEN") + " US Women";

        //            throw new NotImplementedException("Amzn unisex gender not supported");
        //        }

        //        else
        //        {
        //            return size + " " + AmznWidth + " US";
        //        }
        //    }
        //}

        //public string AmznWidth
        //{
        //    get
        //    {
        //        string width = Dimensions.Single(p => p.Name.Equals(DimensionName.Width)).Value;
        //        string gender = AmznGender;

        //        if (gender.Equals("mens"))
        //        {
        //            if (width.Equals("M") || width.Equals("D"))
        //            {
        //                width = "D(M)";
        //            }
        //            else if (width.Equals("EE") || width.Equals("W"))
        //            {
        //                width = "2E";
        //            }
        //            else if (width.Equals("EEE") || width.Equals("XW"))
        //            {
        //                width = "3E";
        //            }
        //            else if (width.Equals("B") || width.Equals("N"))
        //            {
        //                width = "B(N)";
        //            }

        //        }
        //        else if (gender.Equals("womens"))
        //        {
        //            if (width.Equals("C") || width.Equals("D") || width.Equals("W"))
        //            {
        //                width = "C/D";
        //            }
        //            else if (width.Equals("B") || width.Equals("M"))
        //            {
        //                width = "B(M)";
        //            }
        //            else if (width.Equals("2E") || width.Equals("EE") || width.Equals("XW"))
        //            {
        //                width = "E";
        //            }
        //            else if (width.Equals("2A") || width.Equals("AA") || width.Equals("N"))
        //            {
        //                width = "2A(N)";
        //            }

        //        }

        //        return width;
        //    }
        //}

        //public Dimension[] Dimensions 
        //{
        //    get 
        //    {
        //        List<Dimension> dimensions = new List<Dimension>();

        //        string[] attributes = GetAttributes();

        //        if (ProductType.Equals("CLOTHING"))
        //        {
        //            string departmentCode = Department.code;

        //            if (departmentCode.Equals("15689") && departmentCode.Equals("11507"))
        //            {
        //                switch (attributes.Length)
        //                {
        //                    case 1: dimensions.Add(CreateDimension(DimensionName.Waist, attributes[0])); break;

        //                    case 2: dimensions.Add(CreateDimension(DimensionName.Color, attributes[0]));
        //                        dimensions.Add(CreateDimension(DimensionName.Waist, attributes[1])); break;
        //                }
        //            }
        //            else
        //            {
        //                switch (attributes.Length)
        //                {
        //                    case 1: dimensions.Add(CreateDimension(DimensionName.Waist, attributes[0])); break;

        //                    case 2: dimensions.Add(CreateDimension(DimensionName.Waist, attributes[0]));
        //                        dimensions.Add(CreateDimension(DimensionName.Inseam, attributes[1])); break;

        //                    case 3: dimensions.Add(CreateDimension(DimensionName.Color, attributes[0]));
        //                        dimensions.Add(CreateDimension(DimensionName.Waist, attributes[1]));
        //                        dimensions.Add(CreateDimension(DimensionName.Inseam, attributes[2])); break;
        //                }
        //            }
        //        }
        //        else if (ProductType.Equals("SHOES"))
        //        {
        //            switch (attributes.Length)
        //            {
        //                case 1: dimensions.Add(CreateDimension(DimensionName.ShoeSize, attributes[0])); break;

        //                case 2: dimensions.Add(CreateDimension(DimensionName.ShoeSize, attributes[0]));
        //                    dimensions.Add(CreateDimension(DimensionName.Width, attributes[1])); break;

        //                case 3: dimensions.Add(CreateDimension(DimensionName.Color, attributes[0]));
        //                    dimensions.Add(CreateDimension(DimensionName.ShoeSize, attributes[1]));
        //                    dimensions.Add(CreateDimension(DimensionName.Width, attributes[2])); break;
        //            }
        //        }
        //        else if (!ProductType.Equals("WATCHES") && !ProductType.Equals("SUNGLASSES"))
        //        {
        //            throw new NotImplementedException("department not recognized");
        //        }

        //        return dimensions.ToArray();
        //    }
        //}

        public string PictureBasePath 
        {
            get 
            {
                return @"P:\products" + @"\" + SubDescription1 + @"\";
            }
        }

        //private Dimension CreateDimension(DimensionName name, string value)
        //{
        //    Dimension dimension = new Dimension();
        //    dimension.Name = name;
        //    dimension.Value = value;


        //    if (ItemClass != null)
        //    {
        //        dimension.Code = ItemClass.GetAttributeCode(value);
        //    }

        //    return dimension;
        //}

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

        private bool IsValidGtin(string code)
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



        
    }
}
