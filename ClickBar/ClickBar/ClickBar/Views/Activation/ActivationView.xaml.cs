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

namespace ClickBar.Views.Activation
{
    /// <summary>
    /// Interaction logic for ActivationView.xaml
    /// </summary>
    public partial class ActivationView : UserControl
    {
        public ActivationView()
        {
            InitializeComponent();
        }
        private void FirstPart_GotFocus(object sender, RoutedEventArgs e)
        {
            FirstPart.SelectAll();
        }

        private void SecondPart_GotFocus(object sender, RoutedEventArgs e)
        {
            SecondPart.SelectAll();
        }

        private void ThirdPart_GotFocus(object sender, RoutedEventArgs e)
        {
            ThirdPart.SelectAll();
        }

        private void FourPart_GotFocus(object sender, RoutedEventArgs e)
        {
            FourPart.SelectAll();
        }

        private void FivePart_GotFocus(object sender, RoutedEventArgs e)
        {
            FivePart.SelectAll();
        }
    }
}
