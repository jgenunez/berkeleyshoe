using eBay.Service.Core.Soap;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkbookPublisher.ViewModel
{
    public class TemplateViewModel
    {
        private ItemType _template;

        public TemplateViewModel(ItemType template)
        {
            _template = template;
            
            
            this.PaymentMethods = new ObservableCollection<BuyerPaymentMethodCodeType>(template.PaymentMethods.OfType<BuyerPaymentMethodCodeType>());
            this.PaymentMethods.CollectionChanged += PaymentMethods_CollectionChanged;
        }

        public void PaymentMethods_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch(e.Action)
            {
                case NotifyCollectionChangedAction.Add :

                    foreach (BuyerPaymentMethodCodeType option in e.NewItems)
                    {
                        _template.PaymentMethods.Add(option);
                    }
                    
                    break;

                case NotifyCollectionChangedAction.Remove :

                    foreach (BuyerPaymentMethodCodeType option in e.OldItems)
                    {
                        _template.PaymentMethods.Remove(option);
                    }

                    break;
            }

            
        }

        public ObservableCollection<BuyerPaymentMethodCodeType> PaymentMethods { get; set; }

        public ShippingTypeCodeType ShippingType { get; set; }

        public ObservableCollection<ShippingServiceOptionsType> ShippingServiceOptions { get; set; }

        public ObservableCollection<InternationalShippingServiceOptionsType> InternationalShippingServiceOptions { get; set; }

        public string PayPalEmailAddress 
        {
            get { return _template.PayPalEmailAddress; }
            set { _template.PayPalEmailAddress = value; }
        }

        public int DispatchTimeMax 
        {
            get { return _template.DispatchTimeMax; }
            set { _template.DispatchTimeMax = value; }
        }

        public CountryCodeType Country 
        {
            get { return _template.Country; }
            set { _template.Country = value; }
        }

        public CurrencyCodeType Currency
        {
            get { return _template.Currency; }
            set { _template.Currency = value; }
        }

        public string Location 
        {
            get { return _template.Location; }
            set { _template.Location = value; }
        }

        


    }
}
