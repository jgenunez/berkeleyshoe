using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using eBay.Service.Core.Soap;
using BerkeleyEntities;

namespace EbayServices
{
    public abstract class ProductData
    {
        protected Item _item;

        public ProductData(Item item)
        {
            _item = item;
        }

        public string CategoryID
        {
            get { return _item.Department.code;  }
        }

        public abstract List<NameValueListType> GetItemSpecifics();

        public abstract List<NameValueListType> GetVariationSpecifics();

        protected NameValueListType BuildItemSpecific(string name, string[] values)
        {
            NameValueListType itemSpecific = new NameValueListType();
            itemSpecific.Name = name;
            itemSpecific.Value = new StringCollection(values);

            return itemSpecific;
        }
    }
}
