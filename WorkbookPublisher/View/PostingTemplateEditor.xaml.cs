using eBay.Service.Core.Soap;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WorkbookPublisher.View
{
    /// <summary>
    /// Interaction logic for PostingTemplateEditor.xaml
    /// </summary>
    public partial class PostingTemplateEditor : UserControl
    {
        public PostingTemplateEditor()
        {
            InitializeComponent();

            ItemType template = this.DataContext as ItemType;

            cbCurrency.ItemsSource = Enum.GetValues(typeof(CurrencyCodeType)).Cast<CurrencyCodeType>();
            cbCountry.ItemsSource = Enum.GetValues(typeof(CountryCodeType)).Cast<CountryCodeType>();
            cbPaymentOption.ItemsSource = Enum.GetValues(typeof(BuyerPaymentMethodCodeType)).Cast<BuyerPaymentMethodCodeType>();
            
        }

        private void btnAddPaymentOption_Click(object sender, RoutedEventArgs e)
        {
            BuyerPaymentMethodCodeType currentSelection = (BuyerPaymentMethodCodeType)cbPaymentOption.SelectedItem;

            ObservableCollection<BuyerPaymentMethodCodeType> options = lvPaymentOptions.ItemsSource as ObservableCollection<BuyerPaymentMethodCodeType>;

            if (options.OfType<BuyerPaymentMethodCodeType>().Any(p => p.Equals(currentSelection)))
            {
                MessageBox.Show(currentSelection.ToString() + " already exist");
            }
            else
            {
                options.Add(currentSelection);
                lvPaymentOptions.ItemsSource = options;
            }

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            BuyerPaymentMethodCodeType currentSelection = (BuyerPaymentMethodCodeType)lvPaymentOptions.SelectedItem;

            ObservableCollection<BuyerPaymentMethodCodeType> options = lvPaymentOptions.ItemsSource as ObservableCollection<BuyerPaymentMethodCodeType>;

            options.Remove(currentSelection);
        }

        

    }
}
