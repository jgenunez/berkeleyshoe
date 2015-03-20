using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BerkeleyEntities.Bonanza.Mapper
{
    public class ProductMapperFactory
    {
        public ProductMapper GetProductMapper(Item item)
        {
            string deptCode = item.Department.code;

            switch (deptCode)
            {
                //case "57990":
                //case "15687":
                //case "11484":
                //    return new ShirtAdapter(item);

                case "11632":
                case "62107":
                case "53548":
                case "55793":
                case "45333":
                case "53557":
                case "95672":
                case "155202":
                case "155196":
                case "57929":
                case "147285":
                case "11505":
                case "11504":
                case "11501":
                case "53120":
                case "24087":
                case "11498":
                case "15709":
                case "57974":
                    return new ShoesAdapter(item);

                //case "63852":
                //    return new HandbagAdapter(item);

                //case "11483":
                //case "57989":
                //    return new PantsAdapter(item);

                default: throw new NotImplementedException(item.Department.Name + " not supported for Bonanza");
            }

        }

        //public ProductMatrixMapper GetProductMatrixData(string className, IEnumerable<Item> items)
        //{
        //    List<ProductMapper> products = new List<ProductMapper>();

        //    foreach (Item item in items)
        //    {
        //        products.Add(GetProductMapper(item));
        //    }

        //    return new ProductMatrixMapper(products);
        //}
    }
}
