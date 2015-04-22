//
// AMGD
//

using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

using eBay.Service.Call;
using eBay.Service.Core.Sdk;
using eBay.Service.Core.Soap;

namespace BSI_eBayOrders
{
    public class eBayOrder : OrderType
    {
        public String zone { get; set; }
        public int shippingPriority { get; set; }
        public bool repeated { get; set; }
        public DateTime originalDate { get; set; }

        public eBayOrder() : base()
        {
        } // eBayOrder

        public eBayOrder(OrderType po)
            : base()
        {
            this.AdjustmentAmount = po.AdjustmentAmount;
            this.AmountPaid = po.AmountPaid;
            this.AmountSaved = po.AmountSaved;
            this.Any = po.Any;
            this.BundlePurchase = po.BundlePurchase;
            this.BundlePurchaseSpecified = po.BundlePurchaseSpecified;
            this.BuyerCheckoutMessage = po.BuyerCheckoutMessage;
            this.BuyerUserID = po.BuyerUserID;
            this.CheckoutStatus = po.CheckoutStatus;
            this.CreatedTime = po.CreatedTime;
            this.CreatedTimeSpecified = po.CreatedTimeSpecified;
            this.CreatingUserRole = po.CreatingUserRole;
            this.CreatingUserRoleSpecified = po.CreatingUserRoleSpecified;
            this.EIASToken = po.EIASToken;
            this.ExternalTransaction = po.ExternalTransaction;
            this.IntegratedMerchantCreditCardEnabled = po.IntegratedMerchantCreditCardEnabled;
            this.IntegratedMerchantCreditCardEnabledSpecified = po.IntegratedMerchantCreditCardEnabledSpecified;
            this.OrderID = po.OrderID;
            this.OrderStatus = po.OrderStatus;
            this.OrderStatusSpecified = po.OrderStatusSpecified;
            this.PaidTime = po.PaidTime;
            this.PaidTimeSpecified = po.PaidTimeSpecified;
            this.PaymentHoldDetails = po.PaymentHoldDetails;
            this.PaymentHoldStatus = po.PaymentHoldStatus;
            this.PaymentHoldStatusSpecified = po.PaymentHoldStatusSpecified;
            this.PaymentMethods = po.PaymentMethods;
            this.SellerEmail = po.SellerEmail;
            this.ShippedTime = po.ShippedTime;
            this.ShippedTimeSpecified = po.ShippedTimeSpecified;
            this.ShippingAddress = po.ShippingAddress;
            this.ShippingDetails = po.ShippingDetails;
            this.ShippingServiceSelected = po.ShippingServiceSelected;
            this.Subtotal = po.Subtotal;
            this.Total = po.Total;
            this.TransactionArray = new TransactionTypeCollection();
            this.repeated = false;
            this.TransactionArray.AddRange(po.TransactionArray);
            this.originalDate = DateTime.Now;
        }

    } // eBayOrder

} // BSI_eBayOrders
