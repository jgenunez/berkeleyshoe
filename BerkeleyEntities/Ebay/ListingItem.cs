using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BerkeleyEntities
{
    public partial class EbayListingItem
    {
        public override string ToString()
        {
            return string.Format("({0}|{1}|{2})", this.FormatCode, this.Quantity, this.Price);
        }

        public string FormatCode
        {
            get 
            {
                string code = string.Empty;

                if (this.Listing.Format.Equals(EbayMarketplace.FORMAT_AUCTION))
                {
                    switch (this.Listing.Duration)
                    {
                        case "Days_1": code = "A1"; break;
                        case "Days_3": code = "A3"; break;
                        case "Days_5": code = "A5"; break;
                        case "Days_7": code = "A7"; break;
                    }
                }
                else
                {
                    switch (this.Listing.Duration)
                    {
                        case "Days_30": code = "BIN"; break;
                        case "GTC": code = "GTC"; break;
                    }
                }

                return code;
            }
        }

        public int Sold 
        {
            get 
            {
                return this.OrderItems.Where(p => p.Order.MarkedAsShipped()).Sum(p => p.QuantityPurchased);
            }
        }

        
    }
}
