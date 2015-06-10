using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OperationsDashboard
{
    public class WorkDay
    {

        public WorkDay()
        {
 
        }

        public DateTime Date { get; set; }

        public double ModelPhotographed { get; set; }

        public double QtyPhotographed { get; set; }

        public double ItemShipped { get; set; }

        public double QtyShipped { get; set; }

        public double ItemReturned { get; set; }

        public double QtyReturned { get; set; }

        public double ItemReceived { get; set; }

        public double QtyReceived { get; set; }

        public double ItemPublished { get; set; }

        public double QtyPublished { get; set; }

        public double ItemUnpublished { get; set; }

        public double QtyUnpublished { get; set; }
    }

    
}
