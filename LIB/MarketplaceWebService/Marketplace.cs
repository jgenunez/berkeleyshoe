using System;
using System.Collections.Generic;
using System.Text;

namespace MarketplaceWebService
{
    public enum FeedType 
    { 
        _POST_PRODUCT_DATA_, 
        _POST_PRODUCT_PRICING_DATA_, 
        _POST_PRODUCT_RELATIONSHIP_DATA_, 
        _POST_INVENTORY_AVAILABILITY_DATA_, 
        _POST_FLAT_FILE_INVLOADER_DATA_ 
    }

    public class Marketplace
    {
        private const string SulAcessKeyID = "AKIAIG7WFTMN2EQZUBKA";
        private const string SulSecretAccessKey = "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";
        private const string SulMerchantID = "A98JY3EWQYV6X";
        private const string HsAcessKeyID = "AKIAJ4W3ALGZPVSGDYCA";
        private const string HsSecretAccessKey = "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";
        private const string HsMerchantID = "AOR65RSIHDNI7";
        private const string UsMarketplaceID = "ATVPDKIKX0DER";

        public Marketplace(int marketplace)
        {
            switch (marketplace)
            {
                case 1: this.AcessKeyID = SulAcessKeyID;
                    this.SecretAcessKeyID = SulSecretAccessKey;
                    this.MerchantID = SulMerchantID;
                    this.MarketplaceID = UsMarketplaceID;
                    break;
            }
        }

        public string AcessKeyID { get; set; }

        public string SecretAcessKeyID { get; set; }

        public string MerchantID { get; set; }

        public string MarketplaceID { get; set; }
    }
}
