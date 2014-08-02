using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using eBay.Service.Core.Soap;
using BerkeleyEntities;

namespace EbayServices.Mappers
{
    public class ShoesAdapter : ProductData
    {
        public ShoesAdapter(Item item)
            : base(item)
        {

        }

        public override List<NameValueListType> GetItemSpecifics()
        {
            List<NameValueListType> nameValueList = new List<NameValueListType>();

            nameValueList.Add(BuildItemSpecific("Brand", new string[1] { _item.Brand }));

            if (!string.IsNullOrWhiteSpace(_item.GTIN))
            {
                nameValueList.Add(BuildItemSpecific("UPC", new string[1] { _item.GTIN }));
            }

            if (_item.Department.code.Equals("147285"))
            {
                string gender = string.Empty;

                switch (_item.SubDescription3)
                {
                    case "BABY-BOYS":
                        gender = "Boys"; break;
                    case "BABY-GIRLS":
                        gender = "Girls"; break;
                    case "UNISEX-BABY":
                        gender = "Unisex"; break;
                }

                nameValueList.Add(BuildItemSpecific("Gender", new string[1] { gender }));
            }

            if (_item.Category != null)
            {
                nameValueList.Add(BuildItemSpecific("Style", new string[1] { _item.Category.Name }));
            }

            if (!string.IsNullOrWhiteSpace(_item.SubDescription2))
            {
                nameValueList.Add(BuildItemSpecific("Color", new string[1] { _item.SubDescription2 }));
            }

            return nameValueList;
        }

        public override List<NameValueListType> GetVariationSpecifics()
        {
            List<NameValueListType> nameValueList = new List<NameValueListType>();

            int dimCount = 0;

            if (_item.ItemClass != null)
            {
                dimCount = _item.ItemClass.Dimensions;
            }
            else
            {
                dimCount = _item.ItemLookupCode.Split(new Char[1] { '-' }).Length - 1;
            }

            switch (dimCount)
            {
                case 1 :
                case 2 :
                    nameValueList.Add(BuildItemSpecific(GetSizeLabel(), new string[1] { _item.Attributes["Size"].Value }));
                    nameValueList.Add(BuildItemSpecific("Width", new string[1] { this.GetFormattedWidth() })); break;

                case 3:
                    nameValueList.Add(BuildItemSpecific(GetSizeLabel(), new string[1] { _item.Attributes["Size"].Value }));
                    nameValueList.Add(BuildItemSpecific("Width", new string[1] { this.GetFormattedWidth() }));
                    nameValueList.Add(BuildItemSpecific("Color", new string[1] { _item.Attributes["Color"].Value })); break;
            }

            return nameValueList;
        }
        
        private string GetSizeLabel()
        {
            string label = "";
            string gender = _item.SubDescription3.Trim().ToUpper();

            switch (gender)
            {
                case "BOYS":
                case "GIRLS":
                case "UNISEX-CHILD":
                    label = "US Shoe Size (Youth)"; break;

                case "BABY-BOYS" :
                case "BABY-GIRLS": 
                case "UNISEX-BABY" :
                    label = "US Shoe Size (Baby & Toddler)"; break;

                case "UNISEX-ADULT":
                case "MENS" :
                case "MEN" :
                    label = "US Shoe Size (Men's)"; break;

                case "WOMENS" :
                case "WOMEN" :
                    label = "US Shoe Size (Women's)"; break;

                default: throw new NotImplementedException("could not recognize gender");
            }

            return label;
        }

        private string GetFormattedWidth()
        {
            string width = _item.Attributes["Width"].Value;

            switch (_item.SubDescription3)
            {
                case "UNISEX-ADULT":
                case "MENS":
                case "MEN" :
                    return this.FormatWomenWidth(width);

                case "WOMENS":
                case "WOMEN" :
                    return this.FormatWidthForWomen(width);

                case "BABY-BOYS":
                case "BABY-GIRLS":
                case "UNISEX-BABY":
                case "BOYS":
                case "GIRLS" :
                case "UNISEX-CHILD":
                    return this.FormatWidthForYouth(width);

                default: throw new NotImplementedException("could not recognize gender");
            }
        }

        private string FormatWomenWidth(string width)
        {
            switch (width)
            {
                case "XN": return "Extra Narrow (A+)";
                case "N": return "Narrow (C, B)";
                case "D":
                case "M": return "Medium (D, M)";
                case "E":
                case "W": return "Wide (E,W)";
                case "XW":
                case "WW": return "Extra Wide (EE+)";
                default: throw new NotImplementedException("width not supported");
            }
        }

        private string FormatWidthForWomen(string width)
        {
            switch (width)
            {
                case "XN": return "Extra Narrow (AAA+)";
                case "N": return "Narrow (AA, N)";
                case "M":
                case "B": return "Medium (B, M)";
                case "W":
                case "C":
                case "D": return "Wide (C, D, W)";
                case "XW":
                case "WW": return "Extra Wide (E+)";
                default: throw new NotImplementedException("width not supported");
            }
        }

        private string FormatWidthForYouth(string width)
        {
            switch (width)
            {
                case "N": return "Narrow";
                case "M": return "Medium";
                case "W": return "Wide";
                default: throw new NotImplementedException("width not supported");
            }
        }

    }
}
