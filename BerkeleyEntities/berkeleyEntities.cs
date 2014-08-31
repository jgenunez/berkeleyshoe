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
        private ProductFactory _productFactory = new ProductFactory();

        public bool MaterializeAttributes { get; set; }

        partial void OnContextCreated()
        {
            this.ObjectMaterialized += berkeleyEntities_ObjectMaterialized;
        }

        private void berkeleyEntities_ObjectMaterialized(object sender, ObjectMaterializedEventArgs e)
        {
            if (e.Entity is Item && this.MaterializeAttributes)
            {
                _productFactory.GetProductData(e.Entity as Item);
            }
        }

    }
}
