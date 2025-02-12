using ClickBar.ViewModels.AppMain.Statistic;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using System.Windows.Input;

namespace ClickBar.Views.AppMain.AuxiliaryWindows.Statistic.Refaund
{
    /// <summary>
    /// Interaction logic for PayRefaundWindow.xaml
    /// </summary>
    public partial class PayRefaundWindow : Window
    {
        public PayRefaundWindow(PayRefaundViewModel payRefaundViewModel)
        {
            InitializeComponent();
            DataContext = payRefaundViewModel;
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