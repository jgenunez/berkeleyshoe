using eBay.Service.Core.Soap;
using System;
using System.Collections.Generic;
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

            cbCurrency.ItemsSource = Enum.GetValues(typeof(CurrencyCodeType)).Cast<CurrencyCodeType>();
            cbCountry.ItemsSource = Enum.GetValues(typeof(CountryCodeType)).Cast<CountryCodeType>();
        }
    }
}
