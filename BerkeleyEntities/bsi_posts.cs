using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BerkeleyEntities;

namespace BerkeleyEntities
{
    partial class bsi_posts
    {
        public List<int> AmznMessages = new List<int>();

        public bool Confirmed 
        {
            get 
            {
                return this.bsi_quantities.All(p => p.Confirmed);
            }
        }
    }
}
