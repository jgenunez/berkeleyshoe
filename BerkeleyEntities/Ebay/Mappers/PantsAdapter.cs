using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using eBay.Service.Core.Soap;
using BerkeleyEntities;

namespace EbayServices.Mappers
{
    public class PantsAdapter : ProductData
    {
        public PantsAdapter(Item item)
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
                case 1:
                case 2:
                    nameValueList.Add(BuildItemSpecific(GetSizeLabel(), new string[1] { _item.Attributes["Waist"].Value }));
                    nameValueList.Add(BuildItemSpecific("Inseam", new string[1] { _item.Attributes["Inseam"].Value })); break;

                case 3:
                    nameValueList.Add(BuildItemSpecific(GetSizeLabel(), new string[1] { _item.Attributes["Waist"].Value }));
                    nameValueList.Add(BuildItemSpecific("Inseam", new string[1] { _item.Attributes["Inseam"].Value })); 
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
                case "MENS":
                case "MEN":
                    label = "Bottoms Size (Men's)"; break;

                default: throw new NotImplementedException("could not recognize gender");
            }

            return label;
        }
    }
}
