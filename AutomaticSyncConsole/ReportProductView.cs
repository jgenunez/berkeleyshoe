using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutomaticSyncConsole
{
    public class ReportProductView
    {
        public ReportProductView(string sku)
        {
            this.Sku = sku;
        }

        public string Brand { get; set; }

        public string Sku { get; set; }

        public string Department { get; set; }

        public decimal Cost { get; set; }

        public int OnPO { get; set; }

        public int OnHand { get; set; }

        public int OnHold { get; set; }

        public int OnPendingOrder { get; set; }

        public int OrgQty { get; set; }

        public int StgQty { get; set; }

        public int OmsQty { get; set; }

        public int SavQty { get; set; }

        public int OrgPending { get; set; }

        public int StgPending { get; set; }

        public int OmsPending{ get; set; }

        public int SavPending { get; set; }

        public int OrgSold { get; set; }

        public int StgSold { get; set; }

        public int OmsSold { get; set; }

        public int SavSold { get; set; }

        public string Duplicates { get; set; }

        public string Supplier { get; set; }
    }
}
