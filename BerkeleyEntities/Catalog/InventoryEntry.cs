using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BerkeleyEntities
{
    partial class InventoryEntry
    {

        public string Aisle
        {
            get
            {
                if (this.HasStdLocation)
                {
                    return Regex.Split(this.Bin, "[a-zA-Z]")[0];
                }

                return null;
            }
        }

        public bool HasStdLocation 
        {
            get 
            {
                return Regex.IsMatch(this.Bin, "[0-9]+[a-zA-Z][0-9]+");
            }
        }

    }
}
