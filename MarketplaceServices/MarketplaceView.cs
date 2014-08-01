using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarketplaceManager
{
    public class MarketplaceView
    {
        public string ID { get; set; }

        public int DbID { get; set; }

        public string Host { get; set; }

        public string Name { get; set; }

        public DateTime? OrdersLastSync { get; set; }

        public DateTime? ListingsLastSync { get; set; }

        public double ActiveListingQty { get; set; }

        public int WaitingPaymentCount { get; set; }

        public List<string> WaitingPayment { get; set; }

        public int WaitingShipmentCount { get; set; }

        public List<string> WaitingShipment { get; set; }
    }
}
