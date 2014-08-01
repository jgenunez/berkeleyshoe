using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LocationApp
{
    public interface IProductDataRepository
    {

        ProductStock GetProduct(string id);
    }
}
