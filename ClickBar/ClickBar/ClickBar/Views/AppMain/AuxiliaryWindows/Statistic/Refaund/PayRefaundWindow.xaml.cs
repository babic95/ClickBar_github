using ClickBar.ViewModels.AppMain.Statistic;
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

namespace ClickBar.Views.AppMain.AuxiliaryWindows.Statistic.Refaund
{
    /// <summary>
    /// Interaction logic for PayRefaundWindow.xaml
    /// </summary>
    public partial class PayRefaundWindow : Window
    {
        public PayRefaundWindow(RefaundViewModel refaundViewModel)
        {
            InitializeComponent();
            DataContext = new PayRefaundViewModel(this, refaundViewModel);
            Loaded += (s, e) => Keyboard.Focus(Cash);
        }


        private void Cash_GotFocus(object sender, RoutedEventArgs e)
        {
            Cash.SelectAll();
        }

        private void BuyerId_GotFocus(object sender, RoutedEventArgs e)
        {
            BuyerId.SelectAll();
        }

        private void Card_GotFocus(object sender, RoutedEventArgs e)
        {
            Card.SelectAll();
        }

        private void WireTransfer_GotFocus(object sender, RoutedEventArgs e)
        {
            WireTransfer.SelectAll();
        }
    }
}
