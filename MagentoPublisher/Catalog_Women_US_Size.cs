using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MagentoUpload.MagentoApi;
using BerkeleyEntities;
using System.Globalization;

namespace MagentoUpload
{
    public class Catalog_Women_US_Size : CatalogProductFactory
    {
        private catalogAttributeOptionEntity[] _sizeAttributeOptions;
        private catalogAttributeOptionEntity[] _widthAttributeOptions;


        public override void Initiliaze(Mage_Api_Model_Server_V2_HandlerPortTypeClient proxy, string sessionID) 
        {
            base.Initiliaze(proxy, sessionID);

            _widthAttributeOptions = _proxy.catalogProductAttributeOptions(_sessionID, "534", null);
            _sizeAttributeOptions = _proxy.catalogProductAttributeOptions(_sessionID, "533", null);
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
            return "28";
        }

        public override bool IsMatch(bsi_posts post)
        {
            string gender = post.bsi_posting.gender.ToUpper().Trim();

            if (gender.Equals("WOMENS") || gender.Equals("WOMEN"))
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
            return new associativeEntity() { key = "size_womens_shoes", value = option.value };
        }

        private associativeEntity BuildWidthData(string width)
        {
            catalogAttributeOptionEntity option = _widthAttributeOptions.Single(p => p.label.Equals(this.FormatWidth(width)));
            return new associativeEntity() { key = "width_womens", value = option.value };
        }

        private string FormatWidth(string width)
        {
            string formattedWidth = null;

            switch (width)
            {
                case "M":
                case "B":
                    formattedWidth = "Medium (B, M)"; break;

                case "C":
                case "W":
                case "D":
                    formattedWidth = "Wide (C, D, W)"; break;

                case "AA":
                case "N":
                case "2A":
                    formattedWidth = "Narrow (AA, N)"; break;

                case "AAA":
                case "3A":
                case "4A":
                    formattedWidth = "Extra Narrow (AAA+)"; break;

                case "E":
                case "EE":
                case "2E":
                case "EEE":
                case "3E":
                    formattedWidth = "Extra Wide (E+)"; break;

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
                case "45333": return 29;
                case "95672": return 30;
                case "53557": return 31;
                case "55793": return 28;
                case "62107": return 32;
                case "11632": return 33;
                case "53548": return -1;
                default: return -1;
            }
        }




        
    }
}
