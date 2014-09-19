using BerkeleyEntities;
using eBay.Service.Core.Soap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EbayServices.Mappers
{
    //class UKShoesAdapter : ProductMapper
    //{
    //    public UKShoesAdapter(Item item)
    //        : base(item)
    //    {

    //    }

    //    public override List<NameValueListType> GetItemSpecifics()
    //    {
    //        List<NameValueListType> nameValueList = new List<NameValueListType>();

    //        nameValueList.Add(BuildItemSpecific("Brand", new string[1] { this.ToTitleCase(_item.SubDescription1) }));

    //        if (!string.IsNullOrWhiteSpace(_item.GTIN))
    //        {
    //            nameValueList.Add(BuildItemSpecific("UPC", new string[1] { _item.GTIN }));
    //        }

    //        if (_item.Department.code.Equals("147285"))
    //        {
    //            string gender = string.Empty;

    //            switch (_item.SubDescription3)
    //            {
    //                case "BABY-BOYS":
    //                    gender = "Boys"; break;
    //                case "BABY-GIRLS":
    //                    gender = "Girls"; break;
    //                case "UNISEX-BABY":
    //                    gender = "Unisex"; break;
    //            }

    //            nameValueList.Add(BuildItemSpecific("Gender", new string[1] { gender }));
    //        }

    //        if (_item.Category != null)
    //        {
    //            nameValueList.Add(BuildItemSpecific("Style", new string[1] { _item.Category.Name }));
    //        }

    //        if (!string.IsNullOrWhiteSpace(_item.SubDescription2))
    //        {
    //            nameValueList.Add(BuildItemSpecific("Color", new string[1] { this.ToTitleCase(_item.SubDescription2) }));
    //        }

    //        return nameValueList;
    //    }

    //    public override List<NameValueListType> GetVariationSpecifics()
    //    {
    //        List<NameValueListType> nameValueList = new List<NameValueListType>();

    //        switch (_item.DimCount)
    //        {
    //            case 1 :
    //            case 2 :
    //                nameValueList.Add(GetSizeItemSpecific());
    //                nameValueList.Add(BuildItemSpecific("Width", new string[1] { this.GetFormattedWidth() })); break;

    //            case 3:
    //                nameValueList.Add(GetSizeItemSpecific());
    //                nameValueList.Add(BuildItemSpecific("Width", new string[1] { this.GetFormattedWidth() }));
    //                nameValueList.Add(BuildItemSpecific("Color", new string[1] { _item.Attributes[AttributeLabel.Color].Value })); break;
    //        }

    //        return nameValueList;
    //    }

    //    public override int GetConditionID()
    //    {
    //        int conditionID = 1000;

    //        if (_item.Notes != null)
    //        {
    //            if (_item.Notes.Contains("PRE"))
    //            {
    //                conditionID = 1750;
    //            }
    //            else if (_item.Notes.Contains("NWB"))
    //            {
    //                conditionID = 1500;
    //            }
    //            else if (_item.Notes.Contains("NWD"))
    //            {
    //                conditionID = 3000;
    //            } 
    //        }

    //        return conditionID;
    //    }

    //    //private NameValueListType GetSizeItemSpecific()
    //    //{
    //    //    float ukSize;

    //    //    if (_item.Attributes.ContainsKey(AttributeLabel.EUSize))
    //    //    {
    //    //        nv = BuildItemSpecific("EU Shoe Size", new string[1] { _item.Attributes[AttributeLabel.EUSize].Value });
    //    //    }
    //    //    else if (_item.Attributes.ContainsKey(AttributeLabel.USMenSize))
    //    //    {
    //    //        ukSize = float.Parse(_item.Attributes[AttributeLabel.USMenSize].Value) - 0.5f;
    //    //    }
    //    //    else if (_item.Attributes.ContainsKey(AttributeLabel.USWomenSize))
    //    //    {
    //    //        ukSize = float.Parse(_item.Attributes[AttributeLabel.USWomenSize].Value) - 2;
    //    //    }
    //    //    else if (_item.Attributes.ContainsKey(AttributeLabel.USYouthSize))
    //    //    {
    //    //        ukSize = float.Parse(_item.Attributes[AttributeLabel.USYouthSize].Value) - 1;

    //    //        nv = BuildItemSpecific("US Shoe Size (Youth)", new string[1] { _item.Attributes[AttributeLabel.USYouthSize].Value });
    //    //    }
    //    //    else if (_item.Attributes.ContainsKey(AttributeLabel.USBabySize))
    //    //    {
    //    //        ukSize = float.Parse(_item.Attributes[AttributeLabel.USBabySize].Value) - 1;

    //    //        nv = BuildItemSpecific("US Shoe Size (Baby & Toddler)", new string[1] { _item.Attributes[AttributeLabel.USBabySize].Value });
    //    //    }

    //    //    NameValueListType nv = new NameValueListType();

    //    //    if (string.IsNullOrWhiteSpace(GetFormattedWidth()))
    //    //    {
    //    //        nv = BuildItemSpecific("Shoe Size", new string[1] { "UK " + ukSize.ToString()});
    //    //    }
    //    //    else
    //    //    {
    //    //        nv = BuildItemSpecific("Shoe Size", new string[1] { "UK " + ukSize.ToString() + " " + GetFormattedWidth() });
    //    //    }

            

    //    //    return nv;
    //    //}

    //    private string GetFormattedWidth()
    //    {
    //        string width = _item.Attributes[AttributeLabel.Width].Value;

    //        switch (_item.SubDescription3)
    //        {
    //            case "UNISEX-ADULT":
    //            case "MENS":
    //            case "MEN" :
    //                return this.FormatWidthForMen(width);

    //            case "WOMENS":
    //            case "WOMEN" :
    //                return this.FormatWidthForWomen(width);

    //            case "BABY-BOYS":
    //            case "BABY-GIRLS":
    //            case "UNISEX-BABY":
    //            case "BOYS":
    //            case "GIRLS" :
    //            case "UNISEX-CHILD":
    //                return this.FormatWidthForYouth(width);

    //            default: throw new NotImplementedException("could not recognize gender");
    //        }
    //    }

    //    private string FormatWidthForMen(string width)
    //    {
    //        switch (width)
    //        {
    //            case "XN": return "Extra Narrow";
    //            case "C" :
    //            case "N": return "Narrow";
    //            case "D":
    //            case "M": return string.Empty;
    //            case "E":
    //            case "W": return "Wide";
    //            case "2E" :
    //            case "EE":
    //            case "XW":
    //            case "WW": return "Extra Wide";
    //            case "EEE" :
    //            case "3E": return "2X Extra Wide";
    //            case "EEEE" :
    //            case "4E": return "3X Extra Wide";
    //            case "EEEEE" :
    //            case "5E": return "4X Extra Wide";

    //            default: throw new NotImplementedException("width not supported");
    //        }
    //    }

    //    private string FormatWidthForWomen(string width)
    //    {
    //        switch (width)
    //        {
    //            case "SS" :
    //            case "XN": return "Extra Narrow";
    //            case "N": return "Narrow";
    //            case "M":
    //            case "B": return string.Empty;
    //            case "W":
    //            case "C":
    //            case "D": return "Wide";
    //            case "XW":
    //            case "WW": return "Extra Wide";

    //            default: throw new NotImplementedException("width not supported");
    //        }
    //    }

    //    private string FormatWidthForYouth(string width)
    //    {
    //        switch (width)
    //        {
    //            case "N": return "Narrow";
    //            case "M": return string.Empty;
    //            case "W": return "Wide";
    //            default: throw new NotImplementedException("width not supported");
    //        }
    //    }
    //}
}
