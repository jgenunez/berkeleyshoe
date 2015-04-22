//
// AMGD
//

using System;
using System.Collections;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

namespace BSI_eBayOrders
{
    public class Brand : IComparer
    {
        public String brand = "";
        public Decimal count = 0;
        public Decimal subtotal = 0;

        public int Compare(object x, object y)
        {
            Brand brand1 = x as Brand;
            Brand brand2 = y as Brand;

            return brand1.brand.CompareTo(brand2.brand);
        }
    } // class Brand
} // BSI_eBayOrders
