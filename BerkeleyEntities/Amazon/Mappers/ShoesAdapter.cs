using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BerkeleyEntities;

namespace AmazonServices
{
    public class ShoesAdapter : ProductData
    {
        public ShoesAdapter(Item item)
            : base(item)
        {
 
        }

        public override Product GetProductDto(string condition, string title)
        {
            ConditionInfo conditionInfo = new ConditionInfo();
            conditionInfo.ConditionType = ConditionType.New;

            StandardProductID sid = new StandardProductID();
            sid.Type = StandardProductIDType.UPC;
            //sid.Value = 

            ProductDescriptionData descriptiondata = new ProductDescriptionData();
            descriptiondata.Brand = GetBrand();
            //descriptiondata.Description = this.FullDescription;
            descriptiondata.ItemType = GetItemType();
            descriptiondata.Title = title;

            ShoesVariationData variationData = new ShoesVariationData();
            variationData.Size = GetSize();
            variationData.ParentageSpecified = true;
            variationData.Parentage = ShoesVariationDataParentage.child;

            ShoesClassificationData classificationData = new ShoesClassificationData();
            classificationData.Department = GetDepartment();
            classificationData.MaterialType = GetMaterial();

            Shoes shoes = new Shoes();
            shoes.ClothingType = ShoesClothingType.Shoes;
            shoes.ClassificationData = classificationData;
            shoes.VariationData = variationData;

            //if (!String.IsNullOrEmpty(postDetail.AmznColor)) { shoes.ClassificationData.ColorMap = postDetail.AmznColor; }
            //if (!String.IsNullOrEmpty(postDetail.AmznShade)) { shoes.VariationData.Color = postDetail.AmznShade; }

            ProductProductData productdata = new ProductProductData();
            productdata.Item = shoes;

            Product product = new Product();
            product.SKU = _item.ItemLookupCode;
            product.StandardProductID = sid;
            product.ItemPackageQuantity = "1";
            product.NumberOfItems = "1";
            product.Condition = conditionInfo;
            product.DescriptionData = descriptiondata;
            product.ProductData = productdata;
            //product.ReleaseDateSpecified = true;
            //product.ReleaseDate = post.startDate;

            return product;
        }

        private string GetItemType()
        {
            throw new NotImplementedException();
        }

        private string GetBrand()
        {
            throw new NotImplementedException();
        }

        private string GetSize()
        {
            throw new NotImplementedException();
        }

        private string GetDepartment()
        {
            return null;
        }

        private string[] GetMaterial()
        {
            return null;
        }

        
    }
}
