using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BerkeleyEntities
{
    public enum DimensionName { ShoeSize , Width, Waist, Color, Inseam};

    partial class SyncListing
    {

        //public bool Active
        //{
        //    get
        //    {


        //        if (this.EndTime == null && this.Quantity == 0)
        //        {
        //            return false;
        //        }
        //        else if (this.EndTime < DateTime.UtcNow)
        //        {
        //            return false;
        //        }
        //        else
        //        {
        //            return true;
        //        }
        //    }
        //}

        public bool Processed { get; set; }

        public IEnumerable<SyncListing> ListingItems 
        {
            get 
            {
                if (Item.ItemClass != null)
                {
                    return this.Item.ItemClass.GetActiveVariationListings(this.Marketplace);
                }
                else
                {
                    return null;
                }               
            }
        }

        public string Abrv
        {
            get
            {
                string abrv = null;

                switch (this.Marketplace)
                {
                    case "Mecalzo": abrv = "MZ"; break;
                    case "OneMillionShoes": abrv = "OM"; break;
                    case "SmallAvenue": abrv = "SA"; break;
                    case "PickaShoe": abrv = "PS"; break;
                    case "ShopUsLast": abrv = "SL"; break;
                    default: abrv = "UNKNOWN"; break;
                }

                if (Format.Equals("FixedPrice"))
                {
                    if(Variation)
                    {
                        abrv = abrv + "_" + "FPV";
                    }
                    else
                    {
                        abrv = abrv + "_" + "FPI";
                    }
                }
                else
                {
                    abrv = abrv + "_" + "AUC";
                }

                return abrv;
            }
        }

        public int PendingOrderQty 
        {
            get 
            {
                return this.SyncOrderItems
                    .Where(p => 
                        p.SyncOrder.Status.Equals("Unshipped") || 
                        p.SyncOrder.Status.Equals("PartiallyShipped") || 
                        (p.SyncOrder.Status.Equals("Pending") && p.SyncOrder.CreatedTime > DateTime.UtcNow.AddDays(-14)))
                    .Sum(p => p.QuantityOrdered - p.QuantityShipped);
            }
        }



    }
}
