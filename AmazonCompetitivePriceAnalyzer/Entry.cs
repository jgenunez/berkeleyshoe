using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AmznPriceComparator
{
    public class Entry
    {
        public uint RowIndex { get; set; }

        public string Brand { get; set; }

        public string UPC { get; set; }

        public string ASIN { get; set; }

        public decimal Cost { get; set; }

        public int OH { get; set; }

        public string ClassName { get; set; }

        public string Sku { get; set; }

        public decimal OfferCount { get; set; }

        public bool OurPrice { get; set; }

        public decimal LandedPrice { get; set; }

        public decimal ListingPrice { get; set; }

        public decimal Shipping { get; set; }
    }
}
