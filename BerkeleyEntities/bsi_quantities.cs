using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MarketplaceWebServiceProducts.Model;
using System.Text.RegularExpressions;
using System.Xml;
using BerkeleyEntities;

namespace BerkeleyEntities
{
    partial class bsi_quantities
    {

        public static string PRODUCT_TYPE = "_POST_PRODUCT_DATA_";
        public static string INVENTORY_LOADER = "_POST_FLAT_FILE_INVLOADER_DATA_";
        public static string RELATIONSHIP_TYPE = "_POST_PRODUCT_RELATIONSHIP_DATA_";

        berkeleyEntities DataContext = new berkeleyEntities();

        public override string ToString()
        {
            return this.Item.ItemLookupCode + " " + String.Join(" ", this.Item.Aliases.Select(p => p.Alias1).ToArray());
        }

        private string idtype = null;

        public string AmznColor
        {
            get
            {



                switch (this.bsi_posts.bsi_posting.color.ToUpper().Trim())
                {
                    case "BEIGES": return "beige";
                    case "BLACKS": return "black";
                    case "BLUES": return "beige";
                    case "BROWNS": return "brown";
                    case "GRAYS": return "grey";
                    case "GREENS": return "green";
                    case "IVORIES": return "off-white";
                    case "METALLICS": return "metallic";
                    case "MULTI-COLOR": return "multicoloured";
                    case "ORANGES": return "orange";
                    case "PINKS": return "pink";
                    case "PURPLES": return "purple";
                    case "REDS": return "red";
                    case "YELLOWS": return "yellow";
                    case "WHITES": return "white";
                }

                return "";
            }
        }

        public string AmznShade
        {
            get
            {
                bsi_quantities_message message = this.bsi_quantities_message
                    .FirstOrDefault(p => p.submissionType.Equals(PRODUCT_TYPE) && !String.IsNullOrEmpty(p.errorMessage));


                if (message != null && !String.IsNullOrEmpty(message.errorMessage))
                {
                    string[] elements = message.errorMessage.Split(new Char[1] { '\'' });

                    if (elements.Length > 2 && elements[1].Equals("color"))
                    {
                        return elements[5];
                    }
                }


                if (this.AmznProductInfo != null && this.AmznProductInfo.status.Equals("Success") && this.AmznProductInfo.Products.Product.Count > 0)
                {
                    foreach (XmlElement element in this.AmznProductInfo.Products.Product[0].AttributeSets.Any)
                    {
                        if (element["ns2:Color"] != null)
                        {
                            return element["ns2:Color"].InnerText;
                        }

                    }
                }               

                return this.bsi_posts.bsi_posting.shade;
            }
        }

        public string AmznGender
        {
            get
            {
                bsi_quantities_message message = this.bsi_quantities_message
                    .FirstOrDefault(p => p.submissionType.Equals(PRODUCT_TYPE) && !String.IsNullOrEmpty(p.errorMessage));


                if (message != null && !String.IsNullOrEmpty(message.errorMessage))
                {
                    string[] elements = message.errorMessage.Split(new Char[1] { '\'' });

                    if (elements.Length > 2 && elements[1].Equals("department"))
                    {
                        return elements[5];
                    }
                }

                if (this.AmznProductInfo != null && this.AmznProductInfo.status.Equals("Success") && this.AmznProductInfo.Products.Product.Count > 0)
                {
                    foreach (XmlElement element in this.AmznProductInfo.Products.Product[0].AttributeSets.Any)
                    {
                        if (element["ns2:Department"] != null)
                        {
                            return element["ns2:Department"].InnerText;
                        }

                    }
                }                    

                string gender = this.bsi_posts.bsi_posting.gender.ToUpper();

                if (gender.Equals("MENS") || gender.Equals("MEN"))
                {
                    gender = "mens";
                }
                else if (gender.Equals("WOMENS") || gender.Equals("WOMEN"))
                {
                    gender = "womens";
                }
                else if (gender.Equals("UNISEX") || gender.Equals("UNISEXS"))
                {
                    gender = "unisex";
                }

                return gender;
            }
        }

        public string AmznType
        {
            get
            {
                if (this.bsi_posts.bsi_posting.category.Equals("53120"))
                {
                    if (this.bsi_posts.bsi_posting.style.Equals("Loafers & slip ons"))
                    {
                        return "loafers-shoes";
                    }
                    else { return "oxfords-shoes"; }
                }

                else if (this.bsi_posts.bsi_posting.category.Equals("11498") || this.bsi_posts.bsi_posting.category.Equals("53557"))
                {
                    if (this.bsi_posts.bsi_posting.style.Equals("Hiking, trail")) { return "hiking-boots"; }

                    else if (this.bsi_posts.bsi_posting.style.Equals("Rainboots")) { return "rain-boots"; }

                    else if (this.bsi_posts.bsi_posting.style.Equals("Riding, equestrian")) { return "equestrian-boots"; }

                    else if (this.bsi_posts.bsi_posting.style.Equals("Work & safety")) { return "work-boots"; }

                    else { return "boots"; }
                }

                else if (this.bsi_posts.bsi_posting.category.Equals("11504") || this.bsi_posts.bsi_posting.category.Equals("62107"))
                {
                    if (this.bsi_posts.bsi_posting.style.Equals("Mules")) { return "clogs-and-mules-shoes"; }

                    else if (this.bsi_posts.bsi_posting.style.Equals("Sport sandals")) { return "athletic-sandals"; }

                    else { return "sandals"; }
                }

                else if (this.bsi_posts.bsi_posting.category.Equals("55793"))
                {
                    if (this.bsi_posts.bsi_posting.style.Equals("Mules")) { return "clogs-and-mules-shoes"; }

                    else { return "pumps-shoes"; }
                }

                else if (this.bsi_posts.bsi_posting.category.Equals("24087"))
                {
                    if (this.bsi_posts.bsi_posting.style.Equals("Boat shoes")) { return "athletic-boating-shoes"; }

                    else if (this.bsi_posts.bsi_posting.style.Equals("Fashion sneakers")) { return "fashion-sneakers"; }

                    else if (this.bsi_posts.bsi_posting.style.Equals("Oxfords")) { return "oxfords-shoes"; }

                    else { return "loafers-shoes"; }
                }

                else if (this.bsi_posts.bsi_posting.category.Equals("45333"))
                {
                    if (this.bsi_posts.bsi_posting.style.Equals("Boat shoes")) { return "athletic-boating-shoes"; }

                    else if (this.bsi_posts.bsi_posting.style.Equals("Clogs") || this.bsi_posts.bsi_posting.style.Equals("Mules")) { return "clogs-and-mules-shoes"; }

                    else if (this.bsi_posts.bsi_posting.style.Equals("Loafers & moccasins")) { return "loafers-shoes"; }

                    else if (this.bsi_posts.bsi_posting.style.Equals("Oxfords")) { return "oxfords-shoes"; }

                    else { return "flats-shoes"; }
                }

                else if (this.bsi_posts.bsi_posting.category.Equals("15709") || this.bsi_posts.bsi_posting.category.Equals("95672"))
                {
                    if (this.bsi_posts.bsi_posting.style.Equals("Water shoes")) { return "athletic-water-shoes"; }

                    else if (this.bsi_posts.bsi_posting.style.Equals("Walking")) { return "walking-shoes"; }

                    else if (this.bsi_posts.bsi_posting.style.Equals("Skateboarding")) { return "skateboarding-shoes"; }

                    else if (this.bsi_posts.bsi_posting.style.Equals("Running, cross training")) { return "cross-trainer-shoes"; }

                    else if (this.bsi_posts.bsi_posting.style.Equals("Hiking, trail")) { return "hiking-shoes"; }

                    else if (this.bsi_posts.bsi_posting.style.Equals("Golf shoes")) { return "golf-shoes"; }

                    else if (this.bsi_posts.bsi_posting.style.Equals("Dance")) { return "dance-shoes"; }

                    else if (this.bsi_posts.bsi_posting.style.Equals("Bowling shoes")) { return "bowling-shoes"; }

                    else if (this.bsi_posts.bsi_posting.style.Equals("Basketball shoes")) { return "basketball-shoes"; }

                    else if (this.bsi_posts.bsi_posting.style.Equals("Fashion sneakers")) { return "fashion-sneakers"; }

                    else { return "cross-trainer-shoes"; }
                }

                else if (this.bsi_posts.bsi_posting.category.Equals("11501") || this.bsi_posts.bsi_posting.category.Equals("53548"))
                {
                    return "work-shoes";
                }

                else if (this.bsi_posts.bsi_posting.category.Equals("11505") || this.bsi_posts.bsi_posting.category.Equals("11632"))
                {
                    return "slippers";
                }

                else { return null; }
            }
        }

        public string AmznMaterial
        {
            get
            {
                if (this.AmznProductInfo != null && this.AmznProductInfo.status.Equals("Success") && this.AmznProductInfo.Products.Product.Count > 0)
                {
                    foreach (XmlElement element in this.AmznProductInfo.Products.Product[0].AttributeSets.Any)
                    {
                        if (element["ns2:MaterialType"] != null)
                        {
                            return element["ns2:MaterialType"].InnerText;
                        }
                    }
                }   
               

                return this.bsi_posts.bsi_posting.material.ToLower();
            }
        }

        public string AmznBrand
        {
            get
            {
                if (this.AmznProductInfo != null && this.AmznProductInfo.status.Equals("Success") && this.AmznProductInfo.Products.Product.Count > 0)
                {
                    foreach (XmlElement element in this.AmznProductInfo.Products.Product[0].AttributeSets.Any)
                    {
                        if (element["ns2:Brand"] != null)
                        {
                            return element["ns2:Brand"].InnerText;
                        }
                    }
                }                

                return ToTitleCase(this.bsi_posts.bsi_posting.brand);
            }
        }      

        public string AmznWidth
        {
            get 
            {
                string width = this.width.ToUpper();
                string gender = this.AmznGender;

                if (gender.Equals("mens"))
                {
                    if (width.Equals("M") || width.Equals("D"))
                    {
                        width = "D(M)";
                    }
                    else if (width.Equals("EE") || width.Equals("W"))
                    {
                        width = "2E";
                    }
                    else if (width.Equals("EEE") || width.Equals("XW"))
                    {
                        width = "3E";
                    }
                    else if (width.Equals("B") || width.Equals("N"))
                    {
                        width = "B(N)";
                    }
                    
                }
                else if (gender.Equals("womens"))
                {
                    if (width.Equals("C") || width.Equals("D") || width.Equals("W"))
                    {
                        width = "C/D";
                    }
                    else if (width.Equals("B") || width.Equals("M"))
                    {
                        width = "B(M)";
                    }
                    else if (width.Equals("2E") || width.Equals("EE") || width.Equals("XW"))
                    {
                        width = "E";
                    }
                    else if (width.Equals("2A") || width.Equals("AA") || width.Equals("N"))
                    {
                        width = "2A(N)";
                    } 

                }

                return width;
            }
        }

        public string AmznSize 
        {
            get 
            {
                bsi_quantities_message message = this.bsi_quantities_message
                    .FirstOrDefault(p => p.submissionType.Equals(PRODUCT_TYPE) && !String.IsNullOrEmpty(p.errorMessage));


                if (message != null && !String.IsNullOrEmpty(message.errorMessage))
                {
                    string[] elements = message.errorMessage.Split(new Char[1] { '\'' });

                    if (elements.Length > 2 && elements[1].Equals("size"))
                    {
                        return elements[5];
                    }
                }

                if (this.AmznProductInfo != null && this.AmznProductInfo.status.Equals("Success") && this.AmznProductInfo.Products.Product.Count > 0)
                {
                    foreach (XmlElement element in this.AmznProductInfo.Products.Product[0].AttributeSets.Any)
                    {
                        if (element["ns2:Size"] != null)
                        {
                            return element["ns2:Size"].InnerText;
                        }
                        
                    }
                }

                bsi_posting posting = this.bsi_posts.bsi_posting;

                if (this.AmznGender.Equals("unisex"))
                {
                    string womenSize = (double.Parse(this.size) + 1.5).ToString();

                    return this.size + " " + this.AmznWidth + "US Men / " + womenSize + " " + toAmznWidth(this.width, "WOMEN") + " US Women";
                }

                else
                {
                    return this.size + " " + this.AmznWidth + " US";
                }

            }
        }

        public string PurchaseOrder 
        {
            get 
            {
                return this.bsi_posts.purchaseOrder;
            }
        }

        public string Brand 
        {
            get 
            {
                return this.Item.SubDescription1;
            }
        }

        public string GTIN 
        { 
            get 
            {
                foreach (Alias alias in this.Item.Aliases)
                {
                    if (IsValidGtin(alias.Alias1))
                    {
                        return alias.Alias1;
                    }
                }

                return null;
            }
        }

        public string IdType
        { 
            get 
            {
                string gtin = this.GTIN;
                return idtype;
            }

            set
            {
                idtype = value;
            }
        }

        public String toAmznWidth(String width, String gender)
        {
            width = width.ToUpper();

            if (gender.ToUpper().Equals("MENS") || gender.ToUpper().Equals("MEN"))
            {
                if (width.Equals("M") || width.Equals("D"))
                {
                    width = "D(M)";
                }
                else if (width.Equals("EE") || width.Equals("W"))
                {
                    width = "2E";
                }
                else if (width.Equals("EEE") || width.Equals("XW"))
                {
                    width = "3E";
                }
                else if (width.Equals("B") || width.Equals("N"))
                {
                    width = "B(N)";
                }
                gender = "mens";
            }
            else if (gender.ToUpper().Equals("WOMENS") || gender.ToUpper().Equals("WOMEN"))
            {
                if (width.Equals("C") || width.Equals("D") || width.Equals("W"))
                {
                    width = "C/D";
                }
                else if (width.Equals("B") || width.Equals("M"))
                {
                    width = "B(M)";
                }
                else if (width.Equals("2E") || width.Equals("EE") || width.Equals("XW"))
                {
                    width = "E";
                }
                else if (width.Equals("2A") || width.Equals("AA") || width.Equals("N"))
                {
                    width = "2A(N)";
                }
                gender = "womens";

            }

            return width;
        }

        //public int Qty { get { return this.quantity - this.Exist; } }

        public GetMatchingProductForIdResult AmznProductInfo { get; set; }

        public int ErrorCount { 
            get 
            {
                return this.bsi_quantities_message.Count(p => p.errorMessage != null);
            } 
        }

        public bool Confirmed 
        {
            get 
            {
                if (this.bsi_quantities_message.Count == 0) { return false; }

                else
                {
                    if (this.bsi_quantities_message.Any(p => p.submissionType.Equals(INVENTORY_LOADER) && p.confirmed == true))
                    {
                        return true;
                    }

                    else 
                    {
                        return this.bsi_quantities_message
                        .GroupBy(p => p.submissionType)
                        .Where(p => !p.Key.Equals(INVENTORY_LOADER) && !p.Key.Equals(RELATIONSHIP_TYPE))
                        .All(p => p.Any(s => s.confirmed == true));
                    }
                    
                }
                
            }
        }
        
        public bool Exist 
        {
            get 
            {
                return DataContext.bsi_quantities.Any(p => 
                    p.itemId == this.itemId && 
                    p.bsi_posts.marketplace == this.bsi_posts.marketplace && 
                    p.bsi_posts.status == 0);

                //if (postDetails.Count() == 0) { return 0; }

                //else 
                //{
                //    return postDetails.Single(p => p.id == postDetails.Max(m => m.id)).quantity;
                //}
               
            }
        }

        public bool IsValidGtin(string code)
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
                    this.IdType = "UPC";
                    break;
                case 13:
                    code = "0" + code;
                    this.IdType = "EAN";
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

        public string ASIN 
        {
            get 
            {
                if (this.AmznProductInfo != null && this.AmznProductInfo.status.Equals("Success"))
                {
                    if (this.AmznProductInfo.Products.Product.Count > 0 && this.AmznProductInfo.Products.Product[0].Identifiers.IsSetMarketplaceASIN())
                    {
                        return this.AmznProductInfo.Products.Product[0].Identifiers.MarketplaceASIN.ASIN;
                    }
                    else { return null; }
                }

                return null;
            }
        }

        private String ToTitleCase(String str)
        {
            List<String> newstr = new List<String>();

            foreach (String str2 in str.Split(new Char[1] { ' ' }))
            {
                if (str2.Length < 2) { newstr.Add(str2); continue; }

                char[] chararray = str2.ToCharArray();

                for (int i = 0; i < chararray.Length; i++)
                {
                    if (i == 0)
                    {
                        chararray[i] = chararray[i].ToString().ToUpper().ToCharArray()[0];
                    }
                    else
                    {
                        chararray[i] = chararray[i].ToString().ToLower().ToCharArray()[0];
                    }
                }
                newstr.Add(new String(chararray));
            }

            return String.Join(" ", newstr.ToArray());
        }

   

        

       
        
    }
}
