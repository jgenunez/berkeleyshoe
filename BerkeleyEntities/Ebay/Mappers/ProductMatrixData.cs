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
        private List<ProductData> _products;

        public ProductMatrixData(List<ProductData> products)
        {
            _products = products;
        }

        public string CategoryID
        {
            get { return _products.First().CategoryID; }
        }

        public List<NameValueListType> GetItemSpecifics()
        {
            var specs = _products.SelectMany(p => p.GetItemSpecifics()).GroupBy(p => p.Name).Select(p => p.First());

            return new List<NameValueListType>(specs);
        }

        public List<NameValueListType> GetVariationSpecificSets()
        {
            var sets = _products.SelectMany(p => p.GetVariationSpecifics()).GroupBy(p => p.Name);

            List<NameValueListType> variationSpecSets = new List<NameValueListType>();

            foreach (var set in sets)
            {
                string[] values = set.SelectMany(p => p.Value.ToArray()).Distinct().ToArray();

                variationSpecSets.Add(BuildItemSpecific(set.Key, values));
            }

            return variationSpecSets;
        }

        public PicturesTypeCollection GetVariationPictures(IEnumerable<EbayPictureServiceUrl> urls)
        {
            PicturesTypeCollection variationPics = new PicturesTypeCollection();

            var attributeGroups = _products.SelectMany(p => p.GetAttributes())
                .GroupBy(p => p.Value.Code).Select(p => p.First()).GroupBy(p => p.Key);

            foreach (var group in attributeGroups)
            {
                PicturesType picByAttribute = new PicturesType();
                picByAttribute.VariationSpecificName = group.Key;
                picByAttribute.VariationSpecificPictureSet = new VariationSpecificPictureSetTypeCollection();

                foreach (var attribute in group)
                {
                    var temp = urls.Where(p => p.GetVariationCode().Equals(attribute.Value.Code));

                    if (temp.Count() > 0)
                    {
                        VariationSpecificPictureSetType picSet = new VariationSpecificPictureSetType();
                        picSet.VariationSpecificValue = attribute.Value.Value;
                        picSet.PictureURL = new StringCollection(temp.Select(p => p.Url).ToArray());
                        picByAttribute.VariationSpecificPictureSet.Add(picSet);
                    }
                }

                if (picByAttribute.VariationSpecificPictureSet.Count > 0)
                {
                    variationPics.Add(picByAttribute);
                }
            }

            return variationPics;
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
