using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MarketplacePublisher
{
    public class Entry
    {
        public override string ToString()
        {
            return this.RowIndex.ToString() + " " + this.Status;
        }

        public uint RowIndex { get; set; }

        public string Brand { get; set; }

        public decimal Cost { get; set; }

        public int OH { get; set; }

        public string ClassName { get; set; }

        public string Sku { get; set; }

        public string ListingID { get; set; }

        public string Status { get; set; }

        public int Qty { get; set; }

        public int Available { get; set; }

        public decimal Price { get; set; }

        public string Condition { get; set; }

        public string FullDescription { get; set; }

        public string Title { get; set; }

        public string Marketplace { get; set; }

        public string Type { get; set; }

        public void SetListingType(string format, string duration, bool isVariation)
        {
            if (format.Equals("FixedPriceItem"))
            {
                switch (duration)
                {
                    case "GTC" :
                        if (isVariation)
                            this.Type = "GTC";
                        else
                            this.Type = "I-GTC";
                        break;
                    case "Days_30" :
                        if (isVariation)
                            this.Type = "FP30";
                        else
                            this.Type = "I-FP30";
                        break;
                }
            }
            else
            {
                switch (duration)
                {
                    case "Days_1": this.Type = "A1"; break;
                    case "Days_3": this.Type = "A3"; break;
                    case "Days_5": this.Type = "A5"; break;
                    case "Days_7": this.Type = "A7"; break;
                    case "Days_10": this.Type = "A10"; break; 
                }
            }
        }

        public void SetCondition(string conditionID)
        {
            switch (conditionID)
            {
                case "1000": this.Condition = "NEW"; break;
                case "1500": this.Condition = "NWB"; break;
                case "1750": this.Condition = "NWD"; break;
                case "3000": this.Condition = "PRE"; break;
                default: break;
            }
        }

        public void SetMarketplace(string marketplaceID)
        {
            switch (marketplaceID)
            {
                case "ShoesToGo24/7": this.Marketplace = "STG"; break;
                case "OneMillionShoes": this.Marketplace = "OMS"; break;
                case "SmallAvenue": this.Marketplace = "SAV"; break;
                case "Originals": this.Marketplace = "ORG"; break;
            }
        }

        public string GetConditionID()
        {
            switch (this.Condition)
            {
                case "NEW": return "1000";
                case "NWB": return "1500";
                case "NWD": return "1750";
                case "PRE": return "3000";
                default: return null;
            }
        }

        public string GetFormat()
        {
            if (this.Type.Contains("A"))
            {
                return "Chinese";
            }
            else
            {
                return "FixedPriceItem";
            }
        }

        public string GetDuration()
        {
            if (this.Type.Contains("GTC"))
            {
                return "GTC";
            }
            else
            {
                return "Days_" + Regex.Match(this.Type, "[0-9]").Value;
            }
        }

        public string GetMarketplaceName()
        {
            switch (this.Marketplace)
            {
                case "STG": return "ShoesToGo24/7";
                case "OMS": return "OneMillionShoes";
                case "SAV": return "SmallAvenue";
                case "ORG": return "Originals";
                default: return null;
            }
        }

        public bool IsVariation()
        {
            if (this.Type.Contains("I-"))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool IsValidListing()
        {
            if (!string.IsNullOrWhiteSpace(Sku) && !string.IsNullOrWhiteSpace(this.GetMarketplaceName()))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsListed()
        {
            if (string.IsNullOrWhiteSpace(this.ListingID))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        

      
    }
}
