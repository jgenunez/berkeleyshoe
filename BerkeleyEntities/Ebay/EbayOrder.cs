using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BerkeleyEntities
{
    public partial class EbayOrder
    {
        public bool IsWaitingForPayment()
        {
            bool isWaiting = false;

            if (!MarkedAsShipped() && !MarkedAsPaid() && !HasUnpaidDispute() && IsValid() && this.AdjustmentAmount == decimal.Zero && this.CreatedTime > DateTime.UtcNow.AddDays(-20))
            {
                isWaiting = true;
            }
           
            return isWaiting;
        }

        public bool IsWaitingForShipment()
        {
            bool isWaiting = false;

            if (!MarkedAsShipped() &&  MarkedAsPaid() &&  IsValid())
            {
                isWaiting = true;
            }

            return isWaiting;
        }

        public int TotalQty 
        {
            get 
            {
                return this.OrderItems.Sum(p => p.QuantityPurchased);
            }
        }

        private bool HasCompletedCheckout()
        {
            return this.CheckoutStatus.Equals("Complete") && this.EbayPaymentStatus.Equals("NoPaymentFailure") ? true : false;
        }

        private bool HasUnpaidDispute()
        {
            return this.OrderItems.Any(p => p.UnpaidItemDisputeStatus.Equals("Open") || p.UnpaidItemDisputeStatus.Equals("ClosedWithoutPayment")) ? true : false;
        }

        private bool IsValid()
        {
            return this.OrderStatus.Equals("Active") || this.OrderStatus.Equals("Completed");
        }

        private bool MarkedAsPaid()
        {
            return this.PaidTime != null ? true : false;
        }

        public bool MarkedAsShipped()
        {
            return this.ShippedTime != null ? true : false;
        }
    }
}
