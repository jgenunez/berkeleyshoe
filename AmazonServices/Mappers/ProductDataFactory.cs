using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BerkeleyEntities;

namespace AmazonServices
{
    public class ProductDataFactory : ProductFactory
    {
        public ProductDataFactory(berkeleyEntities dataContext) 
            : base(dataContext)
        {

        }

        public ProductData CreateProductData(Item item)
        {
            return null;
        }

        public ProductMatrixData CreateProductMatrix(ItemClass itemClass)
        {
            return null;
        }

        protected override void CreateShoes()
        {
           
        }
    }
}
