using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BerkeleyEntities;

namespace AmazonServices.Mappers
{
    public class ListingMapper
    {
        private ProductDataFactory _productDataFactory;
        private berkeleyEntities _dataContext;
        private AmznMarketplace _marketplace;


        public ListingMapper(berkeleyEntities dataContext, AmznMarketplace marketplace)
        {
            _dataContext = dataContext;
            _marketplace = marketplace;
            _productDataFactory = new ProductDataFactory(_dataContext);
        }

        public Product MapToParentProductDto(ItemClass itemClass, string title)
        {
            ProductMatrixData productMatrixData = _productDataFactory.CreateProductMatrix(itemClass);

            return productMatrixData.GetProductMatrixDto(title);
        }

        public Relationship MapToRelationshipDto(ItemClass itemClass, int marketplaceID)
        {
            ProductMatrixData productMatrixData = _productDataFactory.CreateProductMatrix(itemClass);

            return productMatrixData.GetRelationshipDto(marketplaceID);
        }

        public Product MapToProductDto(AmznListingItem listingItem)
        {
            ProductData productData = _productDataFactory.CreateProductData(listingItem.Item);

            return productData.GetProductDto(listingItem.Condition, listingItem.Title);
        }

        public Price MapToPriceDto(AmznListingItem listingItem)
        {
            OverrideCurrencyAmount oca = new OverrideCurrencyAmount();
            oca.currency = BaseCurrencyCodeWithDefault.USD;
            oca.Value = listingItem.Price;

            Price priceData = new Price();
            priceData.SKU = listingItem.Item.ItemLookupCode;
            priceData.StandardPrice = oca;

            return priceData;
        }

        public Inventory MapToInventoryDto(AmznListingItem listingItem)
        {
            Inventory inventoryData = new Inventory();
            inventoryData.SKU = listingItem.Item.ItemLookupCode;
            inventoryData.Item = listingItem.Quantity.ToString();
            inventoryData.RestockDateSpecified = false;
            inventoryData.SwitchFulfillmentToSpecified = false;

            return inventoryData;
        }

        public void Map(Inventory inventoryData)
        {
            AmznListingItem listingItem = _dataContext.AmznListingItems.SingleOrDefault(p =>
                p.MarketplaceID == _marketplace.ID &&
                p.Item.ItemLookupCode.Equals(inventoryData.SKU));

            listingItem.Quantity = Convert.ToInt32(inventoryData.Item);
        }

        public AmznListingItem Map(Product productData)
        {
            AmznListingItem listingItem = _dataContext.AmznListingItems.SingleOrDefault(p =>
                p.MarketplaceID == _marketplace.ID &&
                p.Item.ItemLookupCode.Equals(productData.SKU));

            if (listingItem == null)
            {
                listingItem = new AmznListingItem();
                listingItem.MarketplaceID = _marketplace.ID;
                listingItem.ASIN = "Unknown";
                listingItem.Item = _dataContext.Items.Single(p => p.ItemLookupCode.Equals(productData.SKU));
                listingItem.Price = 0;
                listingItem.Quantity = 0;
            }

            listingItem.Condition = productData.Condition.ConditionType.ToString();
            listingItem.Title = productData.DescriptionData.Title;
            listingItem.IsActive = true;

            return listingItem;
        }

        public void Map(Price priceData)
        {
            AmznListingItem listingItem = _dataContext.AmznListingItems.SingleOrDefault(p =>
                p.MarketplaceID == _marketplace.ID &&
                p.Item.ItemLookupCode.Equals(priceData.SKU));

            listingItem.Price = priceData.StandardPrice.Value;
        }
    }
}
