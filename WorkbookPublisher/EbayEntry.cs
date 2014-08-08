using BerkeleyEntities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace WorkbookPublisher
{
    public class EbayEntry
    {
        private EbayListing _targetListing;
        private string _message;

        public EbayEntry()
        {
            this.IsValid = true;
        }

        public uint RowIndex { get; set; }
        public string Brand { get; set; }
        public string ClassName { get; set; }
        public string Sku { get; set; }
        public int Q { get; set; }
        public decimal P { get; set; }
        public string Format { get; set; }
        public string Title { get; set; }
        public string Condition { get; set; }
        public string FullDescription { get; set; }
        public bool IsValid { get; set; }

        public string Message
        {
            get 
            {
                if (_message != null)
                {
                    return _message;
                }
                else if (_targetListing != null)
                {
                    if (!string.IsNullOrWhiteSpace(_targetListing.ErrorMessage))
                    {
                        return _targetListing.ErrorMessage;
                    }
                    else
                    {
                        if (_targetListing.EntityState.Equals(EntityState.Added))
                        {
                            return "pending creation";
                        }
                        else if (_targetListing.EntityState.Equals(EntityState.Modified))
                        {
                            return "pending update";
                        }
                    }
                }

                return "pending";
            }
            set { _message = value; }
        }

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

