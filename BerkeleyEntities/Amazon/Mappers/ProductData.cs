using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BerkeleyEntities;
using System.Globalization;
using BerkeleyEntities.Amazon;

namespace AmazonServices
{
    public abstract class ProductData
    {
        protected Item _item;

        public ProductData(Item item)
        {
            _item = item;
        }

        public string Sku 
        {
            get { return _item.ItemLookupCode; }
        }

        public virtual Product GetProductDto(string condition, string title)
        {
            ConditionInfo conditionInfo = new ConditionInfo();
            conditionInfo.ConditionType = ConditionType.New;

            StandardProductID sid = new StandardProductID();
            sid.Type = StandardProductIDType.UPC;
            //sid.Value = 

            ProductDescriptionData descriptiondata = new ProductDescriptionData();
            descriptiondata.Brand = ToTitleCase(_item.SubDescription1);
            //descriptiondata.Description = this.FullDescription;
            
            descriptiondata.Title = title;

            Product product = new Product();
            product.SKU = _item.ItemLookupCode;
            product.StandardProductID = sid;
            product.ItemPackageQuantity = "1";
            product.NumberOfItems = "1";
            product.Condition = conditionInfo;
            product.DescriptionData = descriptiondata;
            //product.ReleaseDateSpecified = true;
            //product.ReleaseDate = post.startDate;
            
            return product;
        }

        public virtual Product GetParentProductDto(string condition, string title)
        {
            ConditionInfo conditionInfo = new ConditionInfo();
            conditionInfo.ConditionType = ConditionType.New;

            ProductDescriptionData descriptiondata = new ProductDescriptionData();
            descriptiondata.Brand = ToTitleCase(_item.SubDescription1);
            //descriptiondata.Description = this.FullDescription;

            descriptiondata.Title = title;

            Product product = new Product();
            product.SKU = _item.ItemClass.ItemLookupCode;
            product.ItemPackageQuantity = "1";
            product.NumberOfItems = "1";
            product.Condition = conditionInfo;
            product.DescriptionData = descriptiondata;
            //product.ReleaseDateSpecified = true;
            //product.ReleaseDate = post.startDate;

            return product;
        }

        public Relationship GetRelationshipDto(int marketplaceID)
        {
            var childSkus = _item.ItemClass.ItemClassComponents.Select(p => p.Item)
                .Where(p => p.AmznListingItems.Any(a => a.IsActive && a.MarketplaceID == marketplaceID)).Select(p => p.ItemLookupCode);

            Relationship relationship = new Relationship();

            relationship.ParentSKU = _item.ItemClass.ItemLookupCode;

            List<RelationshipRelation> relations = new List<RelationshipRelation>();

            foreach (string sku in childSkus)
            {
                RelationshipRelation relation = new RelationshipRelation();
                relation.SKU = sku;
                relation.Type = RelationshipRelationType.Variation;

                relations.Add(relation);
            }

            relationship.Relation = relations.ToArray();


            return relationship;
        }

        private string ToTitleCase(string word)
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            return textInfo.ToTitleCase(word);
        }



        
    }
}
