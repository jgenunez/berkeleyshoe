using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BerkeleyEntities;

namespace EbayServices.Mappers
{
    public class ProductDataFactory : ProductFactory
    {
        private ProductData _productData;
        private PictureSetRepository _pictureSetRepository = new PictureSetRepository();

        public ProductDataFactory(berkeleyEntities dataContext)
            : base(dataContext)
        { 
        }

        public ProductData GetProductData(string sku)
        {
            base.GetProduct(sku);

            return _productData;
        }

        public ProductMatrixData GetProductMatrixData(string className, IEnumerable<string> skus)
        {
            ItemClass itemClass = _dataContext.ItemClasses.Single(p => p.ItemLookupCode.Equals(className));

            List<ProductData> products = new List<ProductData>();

            foreach (string sku in skus)
            {
                products.Add(GetProductData(sku));
            }

            return new ProductMatrixData(itemClass, products);
        }

        protected override void CreateShoes()
        {
            base.CreateShoes();

            _productData = new ShoesAdapter(_item);
        }

        protected override void CreatePants()
        {
            base.CreatePants();

            _productData = new PantsAdapter(_item);
        }

        protected override void CreateJeans()
        {
            base.CreateJeans();

            _productData = new PantsAdapter(_item);
        }
    }
}
