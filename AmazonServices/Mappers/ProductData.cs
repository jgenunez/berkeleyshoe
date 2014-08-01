using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BerkeleyEntities;

namespace AmazonServices
{
    public abstract class ProductData
    {
        protected Item _item;

        public ProductData(Item item)
        {
            _item = item;
        }

        public abstract Product GetProductDto(string condition, string title); 

       
    }
}
