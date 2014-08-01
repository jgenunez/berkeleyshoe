using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MagentoUpload.MagentoApi;
using BerkeleyEntities;

namespace MagentoUpload
{
    public abstract class CatalogProductFactory
    {
        
        protected Mage_Api_Model_Server_V2_HandlerPortTypeClient _proxy = null;
        protected catalogCategoryTree _brandCategoryTree;
        protected string _sessionID = null;

        public virtual void Initiliaze(Mage_Api_Model_Server_V2_HandlerPortTypeClient proxy, string sessionID) 
        {
            _proxy = proxy;
            _sessionID = sessionID;
            _brandCategoryTree = _proxy.catalogCategoryTree(sessionID, "21", "english");
        }

        public abstract catalogProductCreateEntity CreateProductData(bsi_posts post, bsi_quantities postItem);

        public abstract catalogProductCreateEntity CreateParentProductData(bsi_posts post, IEnumerable<string> childrenSkus);

        public abstract string GetAttributeSet();

        public abstract bool IsMatch(bsi_posts post);

    }
}
