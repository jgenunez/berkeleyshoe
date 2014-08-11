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
            ShoesVariationData variationData = new ShoesVariationData();
            variationData.ParentageSpecified = true;
            variationData.Parentage = ShoesVariationDataParentage.child;
            
            switch (_item.DimCount)
            {
                case 1:
                case 2:
                    variationData.Size = GetSize(); break;

                case 3:
                    variationData.Size = GetSize(); 
                    variationData.Color = GetColor(); break;
            }

            ShoesClassificationData classificationData = new ShoesClassificationData();
            classificationData.Department = GetDepartment();
            classificationData.MaterialType = GetMaterial();
            classificationData.ColorMap = GetColorMap();

            Shoes shoes = new Shoes();
            shoes.ClothingType = ShoesClothingType.Shoes;
            shoes.ClassificationData = classificationData;
            shoes.VariationData = variationData;

            Product product = base.GetProductDto(condition, title);
            product.DescriptionData.ItemType = GetItemType();
            product.ProductData = new ProductProductData() { Item = shoes };

            return product;
        }

        public override Product GetParentProductDto(string condition, string title)
        {
            ShoesVariationData variationData = new ShoesVariationData();
            variationData.ParentageSpecified = true;
            variationData.Parentage = ShoesVariationDataParentage.parent;

            switch (_item.DimCount)
            {
                case 1:
                case 2:
                    variationData.VariationTheme = ShoesVariationDataVariationTheme.Size; break;
                case 3:
                    variationData.VariationTheme = ShoesVariationDataVariationTheme.SizeColor; break;

            }

            variationData.VariationThemeSpecified = true;


            ShoesClassificationData classificationData = new ShoesClassificationData();
            classificationData.MaterialType = GetMaterial();
            classificationData.Department = GetDepartment();

            Shoes parentShoes = new Shoes();
            parentShoes.ClothingType = ShoesClothingType.Shoes;
            parentShoes.ClassificationData = classificationData;
            parentShoes.VariationData = variationData;


            Product parentProduct = base.GetParentProductDto(condition, title);

            parentProduct.ProductData = new ProductProductData() { Item = parentShoes };

            return parentProduct;
        }

        private string GetColorMap()
        {
            throw new NotImplementedException();
        }

        private string GetColor()
        {
            throw new NotImplementedException();
        }

        private string GetItemType()
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
