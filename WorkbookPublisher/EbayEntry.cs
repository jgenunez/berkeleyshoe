using BerkeleyEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorkbookPublisher
{
    public class EbayEntry
    {
        private EbayListing _targetListing;


        public uint RowIndex { get; set; }
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public string Brand { get; set; }
        public string ClassName { get; set; }
        public string Sku { get; set; }
        public string Format { get; set; }
        public string Title { get; set; }
        public string Condition { get; set; }
        public string FullDescription { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        

        public bool IsAuction()
        {
            if (this.Format.Contains("A"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public void SetListing(EbayListing listing)
        {
            _targetListing = listing;
        }
    }
}

