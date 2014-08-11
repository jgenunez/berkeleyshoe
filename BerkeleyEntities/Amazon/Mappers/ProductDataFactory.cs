using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BerkeleyEntities;

namespace AmazonServices
{
    public class ProductDataFactory : ProductFactory
    {
        private ProductData _productData;

        public ProductDataFactory(berkeleyEntities dataContext) 
            : base(dataContext)
        {

        }

        public ProductData GetProductData(string sku)
        {
            base.GetProduct(sku);

            return _productData;
        }

        protected override void CreateShoes()
        {
            base.CreateShoes();

            _productData = new ShoesAdapter(_item);
        }
    }
}
