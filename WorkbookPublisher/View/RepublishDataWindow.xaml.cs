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
using System.Windows.Shapes;

namespace WorkbookPublisher.View
{
    /// <summary>
    /// Interaction logic for ProductDataWindow.xaml
    /// </summary>
    public partial class RepublishDataWindow : Window
    {
        public RepublishDataWindow()
        {
            InitializeComponent();
        }

        private void btnPrevious_Click(object sender, RoutedEventArgs e)
        {
            var source = this.DataContext as CollectionView;

            if (!source.MoveCurrentToPrevious())
            {
                source.MoveCurrentToLast();
            }

        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            var source = this.DataContext as CollectionView;

            if (!source.MoveCurrentToNext())
            {
                source.MoveCurrentToFirst();
            }
        }

        private void btnDone_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DockPanel_Loaded(object sender, RoutedEventArgs e)
        {
            this.MouseDown += delegate { DragMove(); };
        }


    }
}
