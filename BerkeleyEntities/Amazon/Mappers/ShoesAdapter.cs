using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BerkeleyEntities;
using BerkeleyEntities.Amazon;

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
            //classificationData.MaterialType = GetMaterial();
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
            //classificationData.MaterialType = GetMaterial();
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
            switch (_item.SubDescription2)
            {
                case "BEIGES": return "beige";
                case "BLACKS": return "black";
                case "BLUES": return "beige";
                case "BROWNS": return "brown";
                case "GRAYS": return "grey";
                case "GREENS": return "green";
                case "IVORIES": return "off-white";
                case "METALLICS": return "metallic";
                case "MULTI-COLOR": return "multicoloured";
                case "ORANGES": return "orange";
                case "PINKS": return "pink";
                case "PURPLES": return "purple";
                case "REDS": return "red";
                case "YELLOWS": return "yellow";
                case "WHITES": return "white";
                default: return string.Empty;
            }
        }

        private string GetColor()
        {
            return _item.Attributes[AttributeLabel.Color].Value;
        }

        private string GetItemType()
        {
            string department = _item.Department.code;
            string category = _item.CategoryName;

            if (department.Equals("53120"))
            {
                if (category.Equals("Loafers & slip ons"))
                {
                    return "loafers-shoes";
                }
                else { return "oxfords-shoes"; }
            }

            else if (department.Equals("11498") || department.Equals("53557"))
            {
                if (category.Equals("Hiking, trail")) { return "hiking-boots"; }

                else if (category.Equals("Rainboots")) { return "rain-boots"; }

                else if (category.Equals("Riding, equestrian")) { return "equestrian-boots"; }

                else if (category.Equals("Work & safety")) { return "work-boots"; }

                else { return "boots"; }
            }

            else if (department.Equals("11504") || department.Equals("62107"))
            {
                if (category.Equals("Mules")) { return "clogs-and-mules-shoes"; }

                else if (category.Equals("Sport sandals")) { return "athletic-sandals"; }

                else { return "sandals"; }
            }

            else if (department.Equals("55793"))
            {
                if (category.Equals("Mules")) { return "clogs-and-mules-shoes"; }

                else { return "pumps-shoes"; }
            }

            else if (department.Equals("24087"))
            {
                if (category.Equals("Boat shoes")) { return "athletic-boating-shoes"; }

                else if (category.Equals("Fashion sneakers")) { return "fashion-sneakers"; }

                else if (category.Equals("Oxfords")) { return "oxfords-shoes"; }

                else { return "loafers-shoes"; }
            }

            else if (department.Equals("45333"))
            {
                if (category.Equals("Boat shoes")) { return "athletic-boating-shoes"; }

                else if (category.Equals("Clogs") || category.Equals("Mules")) { return "clogs-and-mules-shoes"; }

                else if (category.Equals("Loafers & moccasins")) { return "loafers-shoes"; }

                else if (category.Equals("Oxfords")) { return "oxfords-shoes"; }

                else { return "flats-shoes"; }
            }

            else if (department.Equals("15709") || department.Equals("95672"))
            {
                if (category.Equals("Water shoes")) { return "athletic-water-shoes"; }

                else if (category.Equals("Walking")) { return "walking-shoes"; }

                else if (category.Equals("Skateboarding")) { return "skateboarding-shoes"; }

                else if (category.Equals("Running, cross training")) { return "cross-trainer-shoes"; }

                else if (category.Equals("Hiking, trail")) { return "hiking-shoes"; }

                else if (category.Equals("Golf shoes")) { return "golf-shoes"; }

                else if (category.Equals("Dance")) { return "dance-shoes"; }

                else if (category.Equals("Bowling shoes")) { return "bowling-shoes"; }

                else if (category.Equals("Basketball shoes")) { return "basketball-shoes"; }

                else if (category.Equals("Fashion sneakers")) { return "fashion-sneakers"; }

                else { return "cross-trainer-shoes"; }
            }

            else if (department.Equals("11501") || department.Equals("53548"))
            {
                return "work-shoes";
            }

            else if (department.Equals("11505") || department.Equals("11632"))
            {
                return "slippers";
            }

            else { return string.Empty; }
        }
        
        private string GetSize()
        {
            string size = string.Empty;

            string width = _item.Attributes[AttributeLabel.Width].Value;

            if (_item.Attributes.ContainsKey(AttributeLabel.USMenSize))
            {
                if (width.Equals("M") || width.Equals("D"))
                {
                    size = _item.Attributes[AttributeLabel.USMenSize].Value + " D(M) US";
                }
                else if (width.Equals("EE") || width.Equals("W"))
                {
                    size = _item.Attributes[AttributeLabel.USMenSize].Value + " 2E US";
                }
                else if (width.Equals("EEE") || width.Equals("XW"))
                {
                    size = _item.Attributes[AttributeLabel.USMenSize].Value + " 3E US";
                }
                else if (width.Equals("B") || width.Equals("N"))
                {
                    size = _item.Attributes[AttributeLabel.USMenSize].Value + " B(N) US";
                }
            }
            else if (_item.Attributes.ContainsKey(AttributeLabel.USWomenSize))
            {
                if (width.Equals("C") || width.Equals("D") || width.Equals("W"))
                {
                    size = _item.Attributes[AttributeLabel.USWomenSize].Value + " C/D US";
                }
                else if (width.Equals("B") || width.Equals("M"))
                {
                    size = _item.Attributes[AttributeLabel.USWomenSize].Value + " B(M) US";
                }
                else if (width.Equals("2E") || width.Equals("EE") || width.Equals("XW"))
                {
                    size = _item.Attributes[AttributeLabel.USWomenSize].Value + " E US";
                }
                else if (width.Equals("2A") || width.Equals("AA") || width.Equals("N"))
                {
                    size = _item.Attributes[AttributeLabel.USWomenSize].Value + " 2A(N) US";
                }
            }
            else
            {
                size = _item.Attributes.Single(p => !p.Key.Equals(AttributeLabel.Width) && !p.Key.Equals(AttributeLabel.Color)).Value.Value;
            }

            return size;
           
        }

        private string GetDepartment()
        {
            string gender = _item.SubDescription3;

            if (gender.Equals("MENS") || gender.Equals("MEN"))
            {
                gender = "mens";
            }
            else if (gender.Equals("WOMENS") || gender.Equals("WOMEN"))
            {
                gender = "womens";
            }
            else if (gender.Equals("UNISEX") || gender.Equals("UNISEXS"))
            {
                gender = "unisex";
            }

            return gender;
        }


        private string FormatWidth(string width, string gender)
        {
            if (gender.Equals("mens"))
            {
                if (width.Equals("M") || width.Equals("D"))
                {
                    width = "D(M)";
                }
                else if (width.Equals("EE") || width.Equals("W"))
                {
                    width = "2E";
                }
                else if (width.Equals("EEE") || width.Equals("XW"))
                {
                    width = "3E";
                }
                else if (width.Equals("B") || width.Equals("N"))
                {
                    width = "B(N)";
                }

            }
            else if (gender.Equals("womens"))
            {
                if (width.Equals("C") || width.Equals("D") || width.Equals("W"))
                {
                    width = "C/D";
                }
                else if (width.Equals("B") || width.Equals("M"))
                {
                    width = "B(M)";
                }
                else if (width.Equals("2E") || width.Equals("EE") || width.Equals("XW"))
                {
                    width = "E";
                }
                else if (width.Equals("2A") || width.Equals("AA") || width.Equals("N"))
                {
                    width = "2A(N)";
                }

            }

            return width;
        }




        
    }
}
