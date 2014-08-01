using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BerkeleyEntities
{
    partial class PictureUrl
    {

        public bool HasActiveUrl 
        {
            get 
            {
                if(Url != null)
                {
                    var items = ItemPictureUrls.Select(p => p.Item);

                    var activeEbayListings = items.SelectMany(p => p.SyncListings).ToList()
                        .Where(p => !p.Marketplace.Equals("ShopUsLast") && p.Active);

                    if(activeEbayListings.Count() > 0 || LastUpdated > DateTime.Now.AddHours(-15))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

    }
}
