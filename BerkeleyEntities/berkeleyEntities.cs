using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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

        public  T CopyEntity<T>(T entity, bool copyKeys = false) where T : EntityObject
        {
            T clone = this.CreateObject<T>();

            PropertyInfo[] pis = entity.GetType().GetProperties();

            foreach (PropertyInfo pi in pis)
            {
                EdmScalarPropertyAttribute[] attrs = (EdmScalarPropertyAttribute[])
                              pi.GetCustomAttributes(typeof(EdmScalarPropertyAttribute), false);

                foreach (EdmScalarPropertyAttribute attr in attrs)
                {
                    if (!copyKeys && attr.EntityKeyProperty)
                        continue;

                    pi.SetValue(clone, pi.GetValue(entity, null), null);
                }
            }

            return clone;
        }

    }

    

}
