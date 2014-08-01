using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BerkeleyEntities
{
    public interface IMarketplace
    {
        string ID { get; set; }

        string Host { get; set; }

        string DisplayName { get; set; }
    }
}
