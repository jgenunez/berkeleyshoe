using BerkeleyEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BSI_InventoryPreProcessor
{
    public class EbayPublisher
    {
        private berkeleyEntities _dataContext = new berkeleyEntities();
        private EbayMarketplace _marketplace;

        public EbayPublisher(int marketplaceID)
        {
            _marketplace = _dataContext.EbayMarketplaces.Single(p => p.ID == marketplaceID);
        }


        public void Publish(IEnumerable<EbayEntry> entries)
        {
            foreach(EbayEntry entry in entries)
            {
                
            }
        }
    }
}
