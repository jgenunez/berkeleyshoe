using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarketplaceManager
{
    public class MarketplaceView
    {
        public MarketplaceView()
        {
            this.WaitingPaymentQty = 0;
            this.WaitingShipmentQty = 0;
            this.WaitingPayment = new List<string>();
            this.WaitingShipment = new List<string>();
        }

        public string ID { get; set; }

        public int DbID { get; set; }

        public string Host { get; set; }

        public string Name { get; set; }

        public DateTime? OrdersLastSync { get; set; }

        public DateTime? ListingsLastSync { get; set; }

        public double ActiveListingQty { get; set; }

        public int WaitingPaymentQty { get; set; }

        public List<string> WaitingPayment { get; set; }

        public int WaitingShipmentQty { get; set; }

        public List<string> WaitingShipment { get; set; }

        public int ActiveListing { get; set; }
    }
}
