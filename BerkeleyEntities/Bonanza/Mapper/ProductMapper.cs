using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BerkeleyEntities.Bonanza.Mapper
{

    public abstract class ProductMapper
    {
        protected Item _item;

        public ProductMapper(Item item)
        {
            _item = item;

            if (_item.Notes != null && _item.Notes.Contains("DNP"))
            {
                throw new InvalidOperationException("cannot publish this product");
            }
        }

        public abstract string GetCategory();

        public abstract List<NameValuePair> GetItemSpecifics();

        public abstract List<NameValuePair> GetVariationSpecifics();

        public List<KeyValuePair<DimensionName, BerkeleyEntities.Attribute>> GetAttributes()
        {
            return _item.Dimensions.ToList();
        }

        protected string ToTitleCase(string word)
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            return textInfo.ToTitleCase(word);
        }
    }

    public class NameValuePair
    {
        public string Name { get; set; }

        public string Value { get; set; }
    }

}
