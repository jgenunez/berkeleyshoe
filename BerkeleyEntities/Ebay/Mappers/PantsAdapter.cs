using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using eBay.Service.Core.Soap;
using BerkeleyEntities;

namespace BerkeleyEntities.Ebay.Mappers
{
    public class PantsAdapter : ProductMapper
    {
        public PantsAdapter(Item item)
            : base(item)
        {

        }

        public override List<NameValueListType> GetItemSpecifics()
        {
            List<NameValueListType> nameValueList = new List<NameValueListType>();

            nameValueList.Add(BuildItemSpecific("Brand", new string[1] { this.ToTitleCase(_item.SubDescription1) }));

            nameValueList.Add(BuildItemSpecific("Size Type", new string[1] { "Regular" }));

            if (!string.IsNullOrWhiteSpace(_item.GTIN))
            {
                nameValueList.Add(BuildItemSpecific("UPC", new string[1] { _item.GTIN }));
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
                case 1:
                case 2:
                    nameValueList.Add(BuildItemSpecific(GetSizeLabel(), new string[1] { _item.Dimensions[DimensionName.Waist].Value }));
                    nameValueList.Add(BuildItemSpecific("Inseam", new string[1] { _item.Dimensions[DimensionName.Inseam].Value })); break;

                case 3:
                    nameValueList.Add(BuildItemSpecific(GetSizeLabel(), new string[1] { _item.Dimensions[DimensionName.Waist].Value }));
                    nameValueList.Add(BuildItemSpecific("Inseam", new string[1] { _item.Dimensions[DimensionName.Inseam].Value })); 
                    nameValueList.Add(BuildItemSpecific("Color", new string[1] { _item.Dimensions[DimensionName.Color].Value })); break;
            }

            return nameValueList;
        }

        private string GetSizeLabel()
        {
            string label = "";
            string gender = _item.SubDescription3.Trim().ToUpper();

            switch (gender)
            {
                case "MENS":
                case "MEN":
                    label = "Bottoms Size (Men's)"; break;

                default: throw new NotImplementedException("could not recognize gender");
            }

            return label;
        }

        public override int GetConditionID()
        {
            int conditionID = 1000;

            if (_item.Notes != null)
            {
                if (_item.Notes.Contains("PRE"))
                {
                    conditionID = 1750;
                }
                else if (_item.Notes.Contains("NWB"))
                {
                    conditionID = 1500;
                }
                else if (_item.Notes.Contains("NWD"))
                {
                    conditionID = 3000;
                }
            }

            return conditionID;
        }
    }
}
