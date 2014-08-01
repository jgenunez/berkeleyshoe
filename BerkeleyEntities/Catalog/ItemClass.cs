using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;

namespace BerkeleyEntities
{
    partial class ItemClass
    {

        public string Brand 
        {
            get 
            {
                return this.SubDescription1;
            }
        }

        public IEnumerable<SyncListing> GetActiveVariationListings(string marketplace)
        {
            return this.ItemClassComponents.SelectMany(p => p.Item.SyncListings).ToList().Where(p =>
                p.Marketplace.Equals(marketplace) &&
                p.Active == 1 &&
                p.Variation == true);
        }


        public string GetAttributeCode(string attribute)
        {
            return this.MatrixAttributeDisplayOrders.First(p => p.Attribute.Equals(attribute)).Code;
        }

        public string GetAttribute(string code)
        {
            return MatrixAttributeDisplayOrders.First(p => p.Code.Equals(code)).Attribute;
        }

        
       
    }
}
