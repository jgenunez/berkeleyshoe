using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MagentoUpload.MagentoApi;
using BerkeleyEntities;
using System.Globalization;

namespace MagentoUpload
{
    public class Catalog_Men_US_Size : CatalogProductFactory
    {   
        private catalogAttributeOptionEntity[] _sizeAttributeOptions;
        private catalogAttributeOptionEntity[] _widthAttributeOptions;


        public override void Initiliaze(Mage_Api_Model_Server_V2_HandlerPortTypeClient proxy, string sessionID) 
        {
            base.Initiliaze(proxy, sessionID);

            _widthAttributeOptions = _proxy.catalogProductAttributeOptions(_sessionID, "532", null);
            _sizeAttributeOptions = _proxy.catalogProductAttributeOptions(_sessionID, "531", null);
            
        }

        public override catalogProductCreateEntity CreateProductData(bsi_posts post, bsi_quantities postItem)
        {
            int brandID = this.GetBrandID(post.bsi_posting.brand);
            int ebayCategoryID = this.GetEbayCategoryID(post.bsi_posting.category);

            catalogProductCreateEntity productData = new catalogProductCreateEntity();
            productData.visibility = "4";
            productData.stock_data = null;
            productData.additional_attributes = null;
            productData.name = post.title;
            productData.description = post.bsi_posting.fullDescription;
            productData.short_description = post.title;
            productData.price = post.price;
            productData.status = "1";
            productData.websites = new string[1] { "base" };
            productData.has_options = "1";
            productData.categories = new string[2] { ebayCategoryID.ToString(), brandID.ToString() };
            productData.visibility = "1";
            productData.stock_data = new catalogInventoryStockItemUpdateEntity();
            productData.stock_data.is_in_stock = 1;
            productData.stock_data.is_in_stockSpecified = true;

            productData.price = post.price;
            productData.stock_data.qty = postItem.quantity.ToString();
            productData.additional_attributes = new catalogProductAdditionalAttributesEntity();
            productData.additional_attributes.single_data = new associativeEntity[2] 
            { 
                this.BuildSizeData(postItem.size), 
                this.BuildWidthData(postItem.width)
            };

            return productData;
        }

        public override catalogProductCreateEntity CreateParentProductData(bsi_posts post, IEnumerable<string> childrenSkus)
        {
            int brandID = this.GetBrandID(post.bsi_posting.brand);
            int ebayCategoryID = this.GetEbayCategoryID(post.bsi_posting.category);

            catalogProductCreateEntity productData = new catalogProductCreateEntity();
            productData.visibility = "4";
            productData.stock_data = null;
            productData.additional_attributes = null;
            productData.name = post.title;
            productData.description = post.bsi_posting.fullDescription;
            productData.short_description = post.title;
            productData.price = post.price;
            productData.status = "1";
            productData.websites = new string[1] { "base" };
            productData.has_options = "1";
            productData.categories = new string[2] { ebayCategoryID.ToString(), brandID.ToString() };
            productData.associated_skus = childrenSkus.ToArray();

            return productData;
        }

        public override string GetAttributeSet()
        {
            return "26";
        }

        public override bool IsMatch(bsi_posts post)
        {
            string gender = post.bsi_posting.gender.ToUpper().Trim();

            if (gender.Equals("MENS") || gender.Equals("MEN"))
            {
                bsi_quantities postItem = post.bsi_quantities.First();
                string formattedWidth = this.FormatWidth(postItem.width);
                if (_sizeAttributeOptions.Any(p => p.label.Equals(postItem.size)) && _widthAttributeOptions.Any(p => p.label.Equals(formattedWidth)))
                {
                    return true;
                }
            }

            return false;
        }

        private associativeEntity BuildSizeData(string size)
        {
            catalogAttributeOptionEntity option = _sizeAttributeOptions.Single(p => p.label.Equals(size));
            return new associativeEntity() { key = "size_mens_shoes", value = option.value };
        }

        private associativeEntity BuildWidthData(string width)
        {
            catalogAttributeOptionEntity option = _widthAttributeOptions.Single(p => p.label.Equals(this.FormatWidth(width)));
            return new associativeEntity() { key = "width", value = option.value };
        }

        private string FormatWidth(string width)
        {
            string formattedWidth = null;
            switch (width)
            {
                case "A" :
                case "AA":
                case "2A":
                    formattedWidth = "Extra Narrow (A+)"; break;

                case "C":
                case "B":
                case "N":
                    formattedWidth = "Narrow (C, B)"; break;

                case "D":
                case "M":
                    formattedWidth = "Medium (D, M)"; break;

                case "E":
                case "W":
                    formattedWidth = "Wide (E,W)"; break;

                case "EE":
                case "2E":
                case "EEE":
                case "3E":
                case "XW":
                case "WW":
                    formattedWidth = "Extra Wide (EE+)"; break;

                default: formattedWidth = "NF"; break;
            }

            return formattedWidth;
        }

        private int GetBrandID(string brandName)
        {
            CultureInfo cultureInfo = new CultureInfo("en-US", false);
            brandName = cultureInfo.TextInfo.ToTitleCase(brandName.ToLower());

            catalogCategoryEntity category = _brandCategoryTree.children.SingleOrDefault(p => p.name.Equals(brandName));

            if (category == null)
            {
                throw new NotImplementedException("Category " + brandName + " does not exist");
            }
            else
            {
                return category.category_id;
            }
        }

        private int GetEbayCategoryID(string category)
        {
            switch (category)
            {
                case "24087": return 22; 
                case "15709": return 23; 
                case "11498": return 24; 
                case "53120": return 25; 
                case "11504": return 26;
                case "11501": return -1; 
                case "11505": return 27; 
                default: return -1;
            }
        }







        
    }
}
