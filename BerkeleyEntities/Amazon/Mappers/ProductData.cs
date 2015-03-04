using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BerkeleyEntities;
using System.Globalization;
using BerkeleyEntities.Amazon;

namespace AmazonServices
{
    public class ProductData
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

        public virtual Product GetProductDto(string title)
        {
            Product product = new Product();

            product.SKU = _item.ItemLookupCode;

            ProductDescriptionData descriptiondata = new ProductDescriptionData();

            if (string.IsNullOrWhiteSpace(title))
            {
                descriptiondata.Title = _item.Description;
            }
            else
            {
                descriptiondata.Title = title;
            }

            StandardProductID sid = new StandardProductID();

            if (_item.HasAsin())
            {
                string asin = _item.AmznListingItems.First(p => !p.ASIN.Equals("UNKNOWN")).ASIN;

                sid.Type = StandardProductIDType.ASIN;
                sid.Value = asin;

                product.DescriptionData = descriptiondata;
            }
            else
            {
                ConditionInfo conditionInfo = new ConditionInfo();

                conditionInfo.ConditionType = ConditionType.New;

                if (_item.GTINType.Equals("UPC"))
                {
                    sid.Type = StandardProductIDType.UPC;
                }
                else
                {
                    sid.Type = StandardProductIDType.EAN;
                }

                sid.Value = _item.GTIN;

                

                descriptiondata.Brand = ToTitleCase(_item.SubDescription1);

                if (_item.Price > _item.Cost)
                {
                    descriptiondata.MSRP = new CurrencyAmount() { currency = BaseCurrencyCode.USD, Value = _item.Price };
                }

                

                product.ItemPackageQuantity = "1";
                product.NumberOfItems = "1";
                product.Condition = conditionInfo;
                product.DescriptionData = descriptiondata;
            }

            product.StandardProductID = sid;
            
            return product;
        }

        public virtual Product GetParentProductDto(string title)
        {
            ConditionInfo conditionInfo = new ConditionInfo();
            conditionInfo.ConditionType = ConditionType.New;

            ProductDescriptionData descriptiondata = new ProductDescriptionData();
            descriptiondata.Brand = ToTitleCase(_item.SubDescription1);
            //descriptiondata.Description = this.FullDescription;

            if (string.IsNullOrWhiteSpace(title))
            {
                descriptiondata.Title = _item.ItemClass.Description;
            }
            else
            {
                descriptiondata.Title = title;
            }

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

        private string ToTitleCase(string word)
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            return textInfo.ToTitleCase(word);
        }



        
    }
}
