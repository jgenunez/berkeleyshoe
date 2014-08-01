using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BerkeleyEntities
{
    partial class ItemClassComponent
    {
        public string Dimension 
        {
            get
            {
                if (this.Detail1 == null || this.Detail2 == null)
                {
                    return null;
                }
                else
                {
                    return this.Detail1 + " " + this.Detail2;
                }
            }
        }

        public bool ContainsAttribute(string attributeName)
        {
            if (this.Detail1.Equals(attributeName))
            {
                return true;
            }
            else if (this.Detail2.Equals(attributeName))
            {
                return true;
            }
            else if (this.Detail3.Equals(attributeName))
            {
                return true;
            }
            else
            {
                return false;
            }
        }



    }
}
