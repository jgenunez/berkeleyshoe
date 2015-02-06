using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BerkeleyEntities
{
    public partial class AmznListingItem
    {
        public override string ToString()
        {
            return string.Format("({0}|{1})",this.Marketplace.Code, this.Quantity);
        }
    }
}
