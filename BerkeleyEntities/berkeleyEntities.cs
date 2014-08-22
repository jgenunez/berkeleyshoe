using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BerkeleyEntities
{
    public partial class berkeleyEntities
    {
        public ProductFactory _productFactory = new ProductFactory();

        partial void OnContextCreated()
        {
            this.ObjectMaterialized += _productFactory.berkeleyEntities_ObjectMaterialized;
        }

    }
}
