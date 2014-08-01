using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MarketplaceWebServiceProducts;
using MarketplaceWebServiceProducts.Model;
using BerkeleyEntities;

namespace AmazonServices.Services
{
    public class GetCompetivePriceService
    {
        private berkeleyEntities _dataContext = new berkeleyEntities();
        private AmznMarketplace _marketplace;

        public GetCompetivePriceService(int marketplaceID)
        {
            _marketplace = _dataContext.AmznMarketplaces.Single(p => p.ID.Equals(marketplaceID));
        }

        public void GetCompetivePrice(IEnumerable<string> upcs)
        {
            Queue<string> pendingList = new Queue<string>(upcs);

            while(pendingList.Count > 0)
            {
                GetMatchingProductForIdRequest request = new GetMatchingProductForIdRequest();
                request.SellerId = _marketplace.MerchantId;
                request.MarketplaceId = _marketplace.MarketplaceId;
                request.IdList = new IdListType();
                request.IdList.Id = new List<string>(pendingList.Take(5));
                request.IdType = "UPC";

                GetMatchingProductForIdResponse response = _marketplace.GetMWSProductsClient().GetMatchingProductForId(request);

                if (response.IsSetGetMatchingProductForIdResult())
                {
                    foreach (GetMatchingProductForIdResult result in response.GetMatchingProductForIdResult)
                    {
                        
                    }
                }
            }


            
        }
        
    }

    
}
