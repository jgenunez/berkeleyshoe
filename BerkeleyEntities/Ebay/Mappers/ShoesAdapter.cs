using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using eBay.Service.Core.Soap;
using BerkeleyEntities;

namespace BerkeleyEntities.Ebay.Mappers
{
    public class ShoesAdapter : ProductMapper
    {

        public ShoesAdapter(Item item)
            : base(item)
        {

        }

        public override List<NameValueListType> GetItemSpecifics()
        {
            List<NameValueListType> nameValueList = new List<NameValueListType>();

            nameValueList.Add(BuildItemSpecific("Brand", new string[1] { this.ToTitleCase(_item.SubDescription1) }));

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
                nameValueList.Add(BuildItemSpecific("Color", new string[1] { this.ToTitleCase(_item.SubDescription2) }));
            }

            return nameValueList;
        }

        public override List<NameValueListType> GetVariationSpecifics()
        {
            List<NameValueListType> nameValueList = new List<NameValueListType>();

            switch (_item.DimCount)
            {
                case 1 :
                case 2 :
                    nameValueList.Add(GetSizeItemSpecific());
                    nameValueList.Add(BuildItemSpecific("Width", new string[1] { this.GetFormattedWidth() })); break;

                case 3:
                    nameValueList.Add(GetSizeItemSpecific());
                    nameValueList.Add(BuildItemSpecific("Width", new string[1] { this.GetFormattedWidth() }));
                    nameValueList.Add(BuildItemSpecific("Color", new string[1] { _item.Dimensions[DimensionName.Color].Value })); break;
            }

            return nameValueList;
        }

        public override int GetConditionID()
        {
            switch (_item.GetConditionCode())
            {
                case "NEW": return 1000;
                case "PRE": return 3000;
                case "NWB": return 1500;
                case "NWD": return 1750;
                default: return 0;
            }
        }

        private NameValueListType GetSizeItemSpecific()
        {
            NameValueListType nv = new NameValueListType();

            if (_item.Dimensions.ContainsKey(DimensionName.EUSize))
            {
                string gender = _item.SubDescription3.Trim().ToUpper();

                string label = string.Empty;

                switch (gender)
                {
                    case "BOYS":
                    case "GIRLS":
                    case "UNISEX-CHILD":
                        label = "US Shoe Size (Youth)"; break;

                    case "BABY-BOYS":
                    case "BABY-GIRLS":
                    case "UNISEX-BABY":
                        label = "US Shoe Size (Baby & Toddler)"; break;

                    case "UNISEX-ADULT":
                    case "MENS":
                    case "MEN":
                        label = "US Shoe Size (Men's)"; break;

                    case "WOMENS":
                    case "WOMEN":
                        label = "US Shoe Size (Women's)"; break;

                    default: throw new NotImplementedException("could not recognize gender");
                }

                nv = BuildItemSpecific(label, new string[1] { _item.Dimensions[DimensionName.EUSize].Value });

            }
            else if (_item.Dimensions.ContainsKey(DimensionName.USMenSize))
            {
                nv = BuildItemSpecific("US Shoe Size (Men's)", new string[1] { _item.Dimensions[DimensionName.USMenSize].Value });
            }
            else if (_item.Dimensions.ContainsKey(DimensionName.USWomenSize))
            {
                nv = BuildItemSpecific("US Shoe Size (Women's)", new string[1] { _item.Dimensions[DimensionName.USWomenSize].Value });
            }
            else if (_item.Dimensions.ContainsKey(DimensionName.USYouthSize))
            {
                nv = BuildItemSpecific("US Shoe Size (Youth)", new string[1] { _item.Dimensions[DimensionName.USYouthSize].Value });
            }
            else if (_item.Dimensions.ContainsKey(DimensionName.USBabySize))
            {
                nv = BuildItemSpecific("US Shoe Size (Baby & Toddler)", new string[1] { _item.Dimensions[DimensionName.USBabySize].Value });
            }

            return nv;
        }

        private string GetFormattedWidth()
        {
            string width = _item.Dimensions[DimensionName.Width].Value;

            switch (_item.SubDescription3)
            {
                case "UNISEX-ADULT":
                case "MENS":
                case "MEN" :
                    return this.FormatWidthForMen(width);

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

        private string FormatWidthForMen(string width)
        {
            switch (width)
            {
                case "A" :
                case "XN": return "Extra Narrow (A+)";
                case "C" :
                case "B" :
                case "N": return "Narrow (C, B)";
                case "D":
                case "R" :
                case "M": return "Medium (D, M)";
                case "E":
                case "W": return "Wide (E,W)";
                case "2E" :
                case "EE":
                case "XW":
                case "EW" :
                case "WW": return "Extra Wide (EE+)";
                case "EEE" :
                case "3E": return "2X Extra Wide (EEE)";
                case "EEEE" :
                case "4E": return "3X Extra Wide (EEEE)";
                case "EEEEE" :
                case "5E": return "4X Extra Wide (EEEEE)";
                case "EEEEEE" :
                case "6E": return "5X Extra Wide (EEEEEE)";

                default: throw new NotImplementedException("width not supported");
            }
        }

        private string FormatWidthForWomen(string width)
        {
            switch (width)
            {
                case "SS" :
                case "XN": return "Extra Narrow (AAA+)";
                case "2A":
                case "AA":
                case "S" :
                case "N": return "Narrow (AA, N)";
                case "M":
                case "B": return "Medium (B, M)";
                case "W":
                case "C":
                case "D": return "Wide (C, D, W)";
                case "E":
                case "XW":
                case "WW": return "Extra Wide (E+)";
                case "2E" :
                case "EE": return "2X Extra Wide (EE)";
                case "3E" :
                case "EEE": return "3X Extra Wide (EEE)";
                case "4E" :
                case "EEEE": return "4X Extra Wide (EEEE)";
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
