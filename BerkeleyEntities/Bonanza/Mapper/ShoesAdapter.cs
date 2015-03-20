using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BerkeleyEntities.Bonanza.Mapper
{
    public class ShoesAdapter : ProductMapper
    {
        public ShoesAdapter(Item item)
            : base(item)
        {

        }

        public override List<NameValuePair> GetItemSpecifics()
        {
            List<NameValuePair> nameValueList = new List<NameValuePair>();

            if (_item.Notes != null)
            {
                if (_item.Notes.Contains("PRE"))
                {
                    nameValueList.Add(new NameValuePair() { Name = "Condition", Value = "Pre-owned" });
                }
                else if (_item.Notes.Contains("NWB"))
                {
                    nameValueList.Add(new NameValuePair() { Name = "Condition", Value = "New without box" });
                }
                else if (_item.Notes.Contains("NWD"))
                {
                    nameValueList.Add(new NameValuePair() { Name = "Condition", Value = "New with defects" });
                }
                else
                {
                    nameValueList.Add(new NameValuePair() { Name = "Condition", Value = "New with box" });
                }
            }
            else
            {
                nameValueList.Add(new NameValuePair() { Name = "Condition", Value = "New with box" });
            }

            nameValueList.Add(new NameValuePair() { Name = "Brand", Value = _item.SubDescription1 });

            if (!string.IsNullOrWhiteSpace(_item.GTIN))
            {
                nameValueList.Add(new NameValuePair() { Name = "UPC", Value = _item.GTIN });
            }

            if (_item.Category != null)
            {
                nameValueList.Add(new NameValuePair() { Name = "Style", Value = _item.Category.Name });
            }

            if (!string.IsNullOrWhiteSpace(_item.SubDescription2))
            {
                nameValueList.Add(new NameValuePair() { Name = "Color", Value = this.ToTitleCase(_item.SubDescription2) });
            }

            return nameValueList;
        }

        public override List<NameValuePair> GetVariationSpecifics()
        {
            List<NameValuePair> nameValueList = new List<NameValuePair>();

            switch (_item.DimCount)
            {
                case 1:
                case 2:
                    nameValueList.Add(GetSizeItemSpecific());
                    nameValueList.Add(new NameValuePair() { Name = "Width", Value = GetFormattedWidth()}); break;

                case 3:
                    nameValueList.Add(GetSizeItemSpecific());
                    nameValueList.Add(new NameValuePair() { Name = "Width", Value = GetFormattedWidth()});
                    nameValueList.Add(new NameValuePair() { Name = "Color", Value = _item.Dimensions[DimensionName.Color].Value }); break;
            }

            return nameValueList;
        }

        private NameValuePair GetSizeItemSpecific()
        {
            NameValuePair nv = new NameValuePair();

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

                nv.Name = label;
                nv.Value = _item.Dimensions[DimensionName.EUSize].Value;

            }
            else if (_item.Dimensions.ContainsKey(DimensionName.USMenSize))
            {
                nv.Name = "US Shoe Size (Men's)";
                nv.Value = _item.Dimensions[DimensionName.USMenSize].Value;
            }
            else if (_item.Dimensions.ContainsKey(DimensionName.USWomenSize))
            {
                nv.Name = "US Shoe Size (Women's)";
                nv.Value = _item.Dimensions[DimensionName.USWomenSize].Value;
            }
            else if (_item.Dimensions.ContainsKey(DimensionName.USYouthSize))
            {
                nv.Name = "US Shoe Size (Youth)";
                nv.Value = _item.Dimensions[DimensionName.USYouthSize].Value;
            }
            else if (_item.Dimensions.ContainsKey(DimensionName.USBabySize))
            {
                nv.Name = "US Shoe Size (Baby & Toddler)";
                nv.Value = _item.Dimensions[DimensionName.USBabySize].Value;
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
                case "XN": return "Extra Narrow (A+)";
                case "C" :
                case "N": return "Narrow (C, B)";
                case "D":
                case "M": return "Medium (D, M)";
                case "E":
                case "W": return "Wide (E,W)";
                case "2E" :
                case "EE":
                case "XW":
                case "WW": return "Extra Wide (EE+)";
                case "EEE" :
                case "3E": return "2X Extra Wide (EEE)";
                case "EEEE" :
                case "4E": return "3X Extra Wide (EEEE)";
                case "EEEEE" :
                case "5E": return "4X Extra Wide (EEEEE)";

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
