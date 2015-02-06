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
    public enum ShippingType { Flat, Calculated};

    public class TemplateViewModel
    {
        private ItemType _item;
        private ShippingType _shippingType;
        private ShippingType _intShippingType;

        public TemplateViewModel(ItemType item)
        {
            _item = item;
            
            this.CurrencyOptions = new ObservableCollection<CurrencyCodeType>(Enum.GetValues(typeof(CurrencyCodeType)).Cast<CurrencyCodeType>());
            this.CountryOptions = new ObservableCollection<CountryCodeType>(Enum.GetValues(typeof(CountryCodeType)).Cast<CountryCodeType>());
            this.PaymentOptions = new ObservableCollection<BuyerPaymentMethodCodeType>(Enum.GetValues(typeof(BuyerPaymentMethodCodeType)).Cast<BuyerPaymentMethodCodeType>());
            this.ShippingTypeOptions = new ObservableCollection<ShippingType>(Enum.GetValues(typeof(ShippingType)).Cast<ShippingType>());
            
            this.CurrentPayments = new ObservableCollection<BuyerPaymentMethodCodeType>(item.PaymentMethods.OfType<BuyerPaymentMethodCodeType>());
        }

        public ObservableCollection<BuyerPaymentMethodCodeType> PaymentOptions { get; set; }

        public ObservableCollection<BuyerPaymentMethodCodeType> CurrentPayments { get; set; }

        public void AddPayment(BuyerPaymentMethodCodeType payment)
        {
            _item.PaymentMethods.Add(payment);
            this.CurrentPayments.Add(payment);
        }

        public void RemovePayment(BuyerPaymentMethodCodeType payment)
        {
            _item.PaymentMethods.Remove(payment);
            this.CurrentPayments.Remove(payment);
        }



        public ObservableCollection<ShippingType> ShippingTypeOptions { get; set; }



        public ShippingType CurrentShippingType
        {
            get 
            {
                switch (_item.ShippingDetails.ShippingType)
                {
                    case ShippingTypeCodeType.Calculated:
                    case ShippingTypeCodeType.CalculatedDomesticFlatInternational: return ShippingType.Calculated;

                    case ShippingTypeCodeType.Flat :
                    case ShippingTypeCodeType.FlatDomesticCalculatedInternational: return ShippingType.Flat;

                    default: return ShippingType.Flat;
                }
            }
            set
            {
                if (this.CurrentInternationalShippingType.Equals(ShippingType.Flat))
                {
                    if (value.Equals(ShippingType.Flat))
                    {
                        _item.ShippingDetails.ShippingType = ShippingTypeCodeType.Flat;
                    }
                    else
                    {
                        _item.ShippingDetails.ShippingType = ShippingTypeCodeType.CalculatedDomesticFlatInternational;
                    }
                }
                else
                {
                    if (value.Equals(ShippingType.Flat))
                    {
                        _item.ShippingDetails.ShippingType = ShippingTypeCodeType.FlatDomesticCalculatedInternational;
                    }
                    else
                    {
                        _item.ShippingDetails.ShippingType = ShippingTypeCodeType.Calculated;
                    }
                }
            }
        }

        public ObservableCollection<ShippingServiceOptionsType> ShippingServicesOptions { get; set; }

        public ObservableCollection<ShippingServiceOptionsType> ShippingServices { get; set; }




        public ShippingType CurrentInternationalShippingType
        {
            get 
            {
                switch (_item.ShippingDetails.ShippingType)
                {
                    case ShippingTypeCodeType.Calculated:
                    case ShippingTypeCodeType.FlatDomesticCalculatedInternational: return ShippingType.Calculated;

                    case ShippingTypeCodeType.Flat:
                    case ShippingTypeCodeType.CalculatedDomesticFlatInternational: return ShippingType.Flat;

                    default: return ShippingType.Flat;
                }
            }
            set 
            {
                if (this.CurrentShippingType.Equals(ShippingType.Flat))
                {
                    if (value.Equals(ShippingType.Flat))
                    {
                        _item.ShippingDetails.ShippingType = ShippingTypeCodeType.Flat;
                    }
                    else
                    {
                        _item.ShippingDetails.ShippingType = ShippingTypeCodeType.FlatDomesticCalculatedInternational;
                    }
                }
                else
                {
                    if (value.Equals(ShippingType.Flat))
                    {
                        _item.ShippingDetails.ShippingType = ShippingTypeCodeType.CalculatedDomesticFlatInternational;
                    }
                    else
                    {
                        _item.ShippingDetails.ShippingType = ShippingTypeCodeType.Calculated;
                    }
                }
            }
        }

        public ObservableCollection<InternationalShippingServiceOptionsType> InternationalShippingServices { get; set; }



        public string PayPalEmailAddress 
        {
            get { return _item.PayPalEmailAddress; }
            set { _item.PayPalEmailAddress = value; }
        }

        public int DispatchTimeMax 
        {
            get { return _item.DispatchTimeMax; }
            set { _item.DispatchTimeMax = value; }
        }

        public ObservableCollection<CountryCodeType> CountryOptions { get; set; }

        public CountryCodeType Country 
        {
            get { return _item.Country; }
            set { _item.Country = value; }
        }

        public ObservableCollection<CurrencyCodeType> CurrencyOptions { get; set; }

        public CurrencyCodeType Currency
        {
            get { return _item.Currency; }
            set { _item.Currency = value; }
        }

        public string Location 
        {
            get { return _item.Location; }
            set { _item.Location = value; }
        }

        


    }
}
