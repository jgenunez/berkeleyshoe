using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MagentoUpload
{
    public class PendingWorkOrder
    {
        public PendingWorkOrder(string workOrder, string user)
        {
            this.WorkOrder = workOrder;
            this.User = user;
        }

        public string WorkOrder { get; set; }

        public string User { get; set; }

        public int PendingCount { get; set; }

        public int TotalCount { get; set; }
    }
}
