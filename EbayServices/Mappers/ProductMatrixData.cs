using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using eBay.Service.Core.Soap;
using BerkeleyEntities;

namespace EbayServices.Mappers
{
    public class ProductMatrixData 
    {
        private ItemClass _itemClass;
        private List<ProductData> _products;

        public ProductMatrixData(ItemClass itemClass, List<ProductData> products)
        {
            _products = products;
            _itemClass = itemClass;
        }

        public string CategoryID
        {
            get { return _itemClass.Department.code;  }
        }

        public List<NameValueListType> GetItemSpecifics()
        {
            var specs = _products.SelectMany(p => p.GetItemSpecifics()).GroupBy(p => p.Name).Select(p => p.First());

            return new List<NameValueListType>(specs);
        }

        public List<NameValueListType> GetVariationSpecificSets()
        {
            var sets = this._products.SelectMany(p => p.GetVariationSpecifics()).GroupBy(p => p.Name);

            List<NameValueListType> variationSpecSets = new List<NameValueListType>();

            foreach (var set in sets)
            {
                string[] values = set.SelectMany(p => p.Value.ToArray()).Distinct().ToArray();

                variationSpecSets.Add(BuildItemSpecific(set.Key, values));
            }

            return variationSpecSets;
        }

        protected NameValueListType BuildItemSpecific(string name, string[] values)
        {
            NameValueListType itemSpecific = new NameValueListType();
            itemSpecific.Name = name;
            itemSpecific.Value = new StringCollection(values);

            return itemSpecific;
        }
    }
}
