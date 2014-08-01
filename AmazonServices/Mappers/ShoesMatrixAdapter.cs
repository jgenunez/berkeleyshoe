using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AmazonServices
{
    public class ShoesMatrixAdapter : ProductMatrixData
    {
        public override Product GetProductMatrixDto(string title)
        {
            ConditionInfo conditioninfo = new ConditionInfo();
            conditioninfo.ConditionType = ConditionType.New;

            ProductDescriptionData parentDescriptionData = new ProductDescriptionData();
            //parentDescriptionData.Brand = GetBrand();
            //parentDescriptionData.Description = posting.fullDescription;
            //parentDescriptionData.Title = post.title;
            
            //string itemType = postDetail.AmznType;

            //if (itemType != null)
            //{
            //    parentDescriptionData.ItemType = itemType;
            //}

            Shoes parentShoes = new Shoes();
            parentShoes.ClothingType = ShoesClothingType.Shoes;
            parentShoes.ClassificationData = new ShoesClassificationData();
            parentShoes.VariationData = new ShoesVariationData();

            //parentShoes.ClassificationData.MaterialType = new string[1] { postDetail.AmznMaterial };
            //parentShoes.ClassificationData.Department = postDetail.AmznGender;
            parentShoes.VariationData.VariationTheme = ShoesVariationDataVariationTheme.Size;
            parentShoes.VariationData.VariationThemeSpecified = true;
            parentShoes.VariationData.ParentageSpecified = true;
            parentShoes.VariationData.Parentage = ShoesVariationDataParentage.parent;

            //if (!String.IsNullOrEmpty(postDetail.AmznShade)) { parentShoes.VariationData.Color = postDetail.AmznShade; }

            ProductProductData parentProductData = new ProductProductData();
            parentProductData.Item = parentShoes;

            Product parentProduct = new Product();
            parentProduct.SKU = _itemClass.ItemLookupCode;
            parentProduct.NumberOfItems = "1";
            parentProduct.ItemPackageQuantity = "1";
            parentProduct.Condition = conditioninfo;
            parentProduct.DescriptionData = parentDescriptionData;
            parentProduct.ProductData = parentProductData;

            return parentProduct;
        }

        public override Relationship GetRelationshipDto(int marketplaceID)
        {
            var childSkus = _itemClass.ItemClassComponents
                .Where(p => p.Item.AmznListingItems.Any(s => s.Marketplace.ID == marketplaceID && s.IsActive))
                .Select(p => p.Item.ItemLookupCode);

            Relationship relationship = new Relationship();

            relationship.ParentSKU = _itemClass.ItemLookupCode;

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

    }
}
