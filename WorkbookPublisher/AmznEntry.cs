using BerkeleyEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorkbookPublisher
{
    public class AmznEntry
    {
        private AmznListingItem _targetListing;

        public uint RowIndex { get; set; }
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public string Brand { get; set; }
        public string ClassName { get; set; }
        public string Sku { get; set; }
        public string Format { get; set; }
        public string Title { get; set; }
        public string Condition { get; set; }
        public int Quantity { get; set; }

        public decimal Price { get; set; }

        private void SetListing(AmznListingItem listingItem)
        {
            _targetListing = listingItem;
        }
        
    }
}
