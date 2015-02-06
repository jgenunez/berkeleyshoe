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
using WorkbookPublisher.ViewModel;

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
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            TemplateViewModel template = this.DataContext as TemplateViewModel;

            BuyerPaymentMethodCodeType currentSelection = (BuyerPaymentMethodCodeType)cbPaymentOptions.SelectedItem;

            if (template.CurrentPayments.Contains(currentSelection))
            {
                MessageBox.Show("cannot add duplicate payment options");
            }
            else
            {
                template.AddPayment(currentSelection);
            }
        }

        private void miRemove_Click(object sender, RoutedEventArgs e)
        {
            TemplateViewModel template = this.DataContext as TemplateViewModel;

            BuyerPaymentMethodCodeType currentSelection = (BuyerPaymentMethodCodeType)lvPaymentOptions.SelectedItem;

            template.RemovePayment(currentSelection);
        }

        

    }
}
