using eBay.Service.Core.Soap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BerkeleyEntities.Ebay.Mappers
{
    public class ShirtAdapter : ProductMapper
    {

        public ShirtAdapter(Item item)
            : base(item)
        {

        }

        public override List<NameValueListType> GetItemSpecifics()
        {
            List<NameValueListType> nameValueList = new List<NameValueListType>();

            nameValueList.Add(BuildItemSpecific("Brand", new string[1] { this.ToTitleCase(_item.SubDescription1) }));


            nameValueList.Add(BuildItemSpecific("Size Type", new string[1] { "Regular"}));

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

            switch (_item.DimCount)
            {
                case 1: nameValueList.Add(GetSizeItemSpecific()); break;

                case 2: nameValueList.Add(GetColorItemSpecific()); break;

                default: new NotImplementedException(string.Format("{0} dimensions not supported", _item.DimCount.ToString())); break;
            }

            return nameValueList;
        }

        private NameValueListType GetColorItemSpecific()
        {
            NameValueListType nv = new NameValueListType();

            string label = "Color";

            if (_item.Dimensions.ContainsKey(DimensionName.Color))
            {
                nv = BuildItemSpecific(label, new string[1] { _item.Dimensions[DimensionName.Color].Value });
            }

            return nv;
        }

        private NameValueListType GetSizeItemSpecific()
        {
            NameValueListType nv = new NameValueListType();

            string gender = _item.SubDescription3.Trim().ToUpper();

            string label = string.Empty;

            if (_item.Dimensions.ContainsKey(DimensionName.Size))
            {
                switch (gender)
                {
                    case "MENS":
                    case "MEN":
                        label = "Size (Men's)"; break;

                    default: throw new NotImplementedException("could not recognize gender");
                }

                nv = BuildItemSpecific(label, new string[1] { _item.Dimensions[DimensionName.Size].Value });
            }


            return nv;
        }

        public override int GetConditionID()
        {
            int conditionID = 1000;

            if (_item.Notes != null)
            {
                if (_item.Notes.Contains("PRE"))
                {
                    conditionID = 3000;
                }
                else if (_item.Notes.Contains("NWB"))
                {
                    conditionID = 1500;
                }
                else if (_item.Notes.Contains("NWD"))
                {
                    conditionID = 1750;
                }
            }

            return conditionID;
        }
    }
}
