using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BerkeleyEntities;
using System.Data;
using BerkeleyEntities.Amazon;

namespace AmazonServices.Mappers
{
    public class ListingMapper
    {
        private ProductMapperFactory _productMapperFactory;
        private berkeleyEntities _dataContext;
        private AmznMarketplace _marketplace;


        public ListingMapper(berkeleyEntities dataContext, AmznMarketplace marketplace)
        {
            _dataContext = dataContext;
            _marketplace = marketplace;
            _productMapperFactory = new ProductMapperFactory();
        }

        public AmazonEnvelopeMessage BuildMessage(object item, int messageID)
        {
            AmazonEnvelopeMessage msg = new AmazonEnvelopeMessage();
            msg.OperationTypeSpecified = true;
            msg.OperationType = AmazonEnvelopeMessageOperationType.Update;
            msg.Item = item;
            msg.MessageID = messageID.ToString();

            return msg;
        }

        public AmazonEnvelope BuildEnvelope(AmazonEnvelopeMessageType msgType, IEnumerable<AmazonEnvelopeMessage> msgs)
        {
            AmazonEnvelope envelope = new AmazonEnvelope();
            envelope.MessageType = msgType;
            envelope.Message = msgs.ToArray();
            envelope.Header = new Header();
            envelope.Header.MerchantIdentifier = _marketplace.MerchantId;
            envelope.Header.DocumentVersion = "1.01";

            return envelope;
        }

        public List<AmznListingItem> SaveProductDataChanges(IEnumerable<AmazonEnvelopeMessage> msgs)
        {
            var added = _dataContext.ObjectStateManager.GetObjectStateEntries(EntityState.Added).Select(p => p.Entity).Cast<AmznListingItem>();

            List<AmznListingItem> listingItems = new List<AmznListingItem>();

            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                foreach (var msg in msgs)
                {
                    Product product = (Product)msg.Item;

                    if (dataContext.Items.Any(p => p.ItemLookupCode.Equals(product.SKU)))
                    {
                        AmznListingItem listingItem = dataContext.AmznListingItems
                            .SingleOrDefault(p => p.Item.ItemLookupCode.Equals(product.SKU) && p.MarketplaceID == _marketplace.ID);

                        listingItems.Add(added.Single(p => p.Item.ItemLookupCode.Equals(product.SKU)));

                        if (listingItem == null)
                        {
                            listingItem = new AmznListingItem();
                            listingItem.Item = dataContext.Items.Single(p => p.ItemLookupCode.Equals(product.SKU));
                            listingItem.Sku = product.SKU;
                            listingItem.Marketplace = dataContext.AmznMarketplaces.Single(p => p.ID == _marketplace.ID);
                            listingItem.OpenDate = DateTime.UtcNow;
                            listingItem.LastSyncTime = DateTime.UtcNow;
                            listingItem.ASIN = "UNKNOWN";
                            listingItem.Quantity = 0;
                            listingItem.Price = 0;
                            listingItem.Condition = product.Condition.ConditionType.ToString();
                            listingItem.Title = product.DescriptionData.Title;
                        }

                        listingItem.IsActive = true;
                    }
                }

                dataContext.SaveChanges();
            }

            return listingItems;
        }

        public void SavePriceDataChanges(IEnumerable<AmazonEnvelopeMessage> msgs)
        {
            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                foreach (AmazonEnvelopeMessage msg in msgs)
                {
                    Price priceData = (Price)msg.Item;

                    AmznListingItem listingItem = dataContext.AmznListingItems
                        .Single(p => p.Item.ItemLookupCode.Equals(priceData.SKU) && p.MarketplaceID == _marketplace.ID && p.IsActive);

                    listingItem.Price = priceData.StandardPrice.Value;
                }

                dataContext.SaveChanges();
            }
        }

        public void SaveInventoryDataChanges(IEnumerable<AmazonEnvelopeMessage> msgs)
        {
            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                foreach (AmazonEnvelopeMessage msg in msgs)
                {
                    Inventory inventoryData = (Inventory)msg.Item;

                    AmznListingItem listingItem = dataContext.AmznListingItems
                        .Single(p => p.Item.ItemLookupCode.Equals(inventoryData.SKU) && p.MarketplaceID == _marketplace.ID && p.IsActive);

                    listingItem.Quantity = Convert.ToInt32(inventoryData.Item);
                }

                dataContext.SaveChanges();
            }
        }

        public AmazonEnvelope BuildProductData(IEnumerable<AmznListingItem> listingItems)
        {
            List<AmazonEnvelopeMessage> messages = new List<AmazonEnvelopeMessage>();

            int currentMsg = 1;

            foreach (AmznListingItem listingItem in listingItems.Where(p => p.Item.ItemClass == null))
            {
                Product product = MapToProductDto(new List<AmznListingItem>() { listingItem }).First();

                messages.Add(BuildMessage(product, currentMsg));

                currentMsg++;
            }

            var classGroups = listingItems.Where(p => p.Item.ItemClass != null).GroupBy(p => p.Item.ItemClass);

            foreach (var classGroup in classGroups)
            {
                var products = MapToProductDto(classGroup.ToList());

                foreach (Product product in products)
                {
                    messages.Add(BuildMessage(product, currentMsg));

                    currentMsg++;
                }

            }

            return BuildEnvelope(AmazonEnvelopeMessageType.Product, messages);
        }

        public AmazonEnvelope BuildRelationshipData(IEnumerable<AmznListingItem> listingItems)
        {
            List<AmazonEnvelopeMessage> messages = new List<AmazonEnvelopeMessage>();

            int currentMsg = 1;

            var classGroups = listingItems.Where(p => p.Item.ItemClass != null).GroupBy(p => p.Item.ItemClass.ItemLookupCode);

            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                foreach (var classGroup in classGroups)
                {
                    var siblingSkus = dataContext.AmznListingItems
                        .Where(p => p.IsActive && p.MarketplaceID == _marketplace.ID &&  p.Item.ItemClassComponents.First().ItemClass.ItemLookupCode.Equals(classGroup.Key)).Select(p => p.Sku);

                    Relationship relationships = new Relationship();
                    relationships.ParentSKU = classGroup.Key;

                    List<RelationshipRelation> relations = new List<RelationshipRelation>();

                    foreach(string sku in siblingSkus)
                    {
                        relations.Add(new RelationshipRelation() { SKU = sku, Type = RelationshipRelationType.Variation } );
                    }

                    relationships.Relation = relations.ToArray();

                    messages.Add(BuildMessage(relationships, currentMsg));

                    currentMsg++;
                } 
            }

            return BuildEnvelope(AmazonEnvelopeMessageType.Relationship, messages);
        }

        public AmazonEnvelope BuildInventoryData(IEnumerable<AmznListingItem> listingItems)
        {
            List<AmazonEnvelopeMessage> messages = new List<AmazonEnvelopeMessage>();

            int currentMsg = 1;

            foreach (AmznListingItem listingItem in listingItems)
            {
                Inventory inventoryData = MapToInventoryDto(listingItem);

                messages.Add(BuildMessage(inventoryData, currentMsg));

                currentMsg++;
            }

            return BuildEnvelope(AmazonEnvelopeMessageType.Inventory, messages);
        }

        public AmazonEnvelope BuildPriceData(IEnumerable<AmznListingItem> listingItems)
        {
            List<AmazonEnvelopeMessage> messages = new List<AmazonEnvelopeMessage>();

            int currentMsg = 1;

            foreach (AmznListingItem listingItem in listingItems)
            {
                Price priceData = MapToPriceDto(listingItem);
                messages.Add(BuildMessage(priceData, currentMsg));
                currentMsg++;
            }

            return BuildEnvelope(AmazonEnvelopeMessageType.Price, messages);
        }

        private List<Product> MapToProductDto(List<AmznListingItem> listingItems)
        {
            List<Product> products = new List<Product>();

            for(int i = 0; i < listingItems.Count; i++)
            {
                AmznListingItem listingItem = listingItems[i];

                ProductData productData = _productMapperFactory.GetProductData(listingItem.Item);
               
                products.Add(productData.GetProductDto(listingItem.Condition, listingItem.Title));

                if(i == 0 && listingItem.Item.ItemClass != null && !listingItem.Item.ItemClass.AnyActiveListing(_marketplace.ID))
                {
                    products.Add(productData.GetParentProductDto(listingItem.Condition, listingItem.Title));
                }
            }

            return products;
        }

        private Price MapToPriceDto(AmznListingItem listingItem)
        {
            OverrideCurrencyAmount oca = new OverrideCurrencyAmount();
            oca.currency = BaseCurrencyCodeWithDefault.USD;
            oca.Value = Math.Round(listingItem.Price,4);
           
            Price priceData = new Price();
            priceData.SKU = listingItem.Item.ItemLookupCode;
            priceData.StandardPrice = oca;

            return priceData;
        }

        private Inventory MapToInventoryDto(AmznListingItem listingItem)
        {
            Inventory inventoryData = new Inventory();
            inventoryData.SKU = listingItem.Item.ItemLookupCode;
            inventoryData.Item = listingItem.Quantity.ToString();
            inventoryData.RestockDateSpecified = false;
            inventoryData.SwitchFulfillmentToSpecified = false;

            return inventoryData;
        }

        

        //public void Map(Inventory inventoryData)
        //{
        //    AmznListingItem listingItem = _dataContext.AmznListingItems.SingleOrDefault(p =>
        //        p.MarketplaceID == _marketplace.ID &&
        //        p.Item.ItemLookupCode.Equals(inventoryData.SKU));

        //    listingItem.Quantity = Convert.ToInt32(inventoryData.Item);
        //}

        //public AmznListingItem Map(Product productData)
        //{
        //    AmznListingItem listingItem = _dataContext.AmznListingItems.SingleOrDefault(p =>
        //        p.MarketplaceID == _marketplace.ID &&
        //        p.Item.ItemLookupCode.Equals(productData.SKU));

        //    if (listingItem == null)
        //    {
        //        listingItem = new AmznListingItem();
        //        listingItem.MarketplaceID = _marketplace.ID;
        //        listingItem.ASIN = "Unknown";
        //        listingItem.Item = _dataContext.Items.Single(p => p.ItemLookupCode.Equals(productData.SKU));
        //        listingItem.Price = 0;
        //        listingItem.Quantity = 0;
        //    }

        //    listingItem.Condition = productData.Condition.ConditionType.ToString();
        //    listingItem.Title = productData.DescriptionData.Title;
        //    listingItem.IsActive = true;

        //    return listingItem;
        //}

        //public void Map(Price priceData)
        //{
        //    AmznListingItem listingItem = _dataContext.AmznListingItems.SingleOrDefault(p =>
        //        p.MarketplaceID == _marketplace.ID &&
        //        p.Item.ItemLookupCode.Equals(priceData.SKU));

        //    listingItem.Price = priceData.StandardPrice.Value;
        //}
    }
}
