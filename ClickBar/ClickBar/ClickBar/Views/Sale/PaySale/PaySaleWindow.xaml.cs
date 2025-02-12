using ClickBar.ViewModels;
using ClickBar.ViewModels.Sale;
using Microsoft.Extensions.DependencyInjection;
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

namespace ClickBar.Views.Sale.PaySale
{
    /// <summary>
    /// Interaction logic for PaySaleWindow.xaml
    /// </summary>
    public partial class PaySaleWindow : Window
    {
        private readonly IServiceProvider _serviceProvider;

        public PaySaleWindow(IServiceProvider serviceProvider, SaleViewModel saleViewModel)
        {
            _serviceProvider = serviceProvider;
            InitializeComponent();


            // Kreiranje instance SplitOrderViewModel-a koristeći IServiceProvider
            var paySaleViewModel = _serviceProvider.GetRequiredService<PaySaleViewModel>();
            paySaleViewModel.SaleViewModel = saleViewModel;

            DataContext = paySaleViewModel;

            Focusable = true;

            //Loaded += (s, e) => Keyboard.Focus(Cash);
        }

        //private void Cash_GotFocus(object sender, RoutedEventArgs e)
        //{
        //    Cash.SelectAll();
        //}

        private void BuyerId_GotFocus(object sender, RoutedEventArgs e)
        {
            buyerId.SelectAll();
        }


        //private void Card_GotFocus(object sender, RoutedEventArgs e)
        //{
        //    Card.SelectAll();
        //}

        //private void Check_GotFocus(object sender, RoutedEventArgs e)
        //{
        //    Check.SelectAll();
        //}

        //private void Other_GotFocus(object sender, RoutedEventArgs e)
        //{
        //    Other.SelectAll();
        //}

        //private void Voucher_GotFocus(object sender, RoutedEventArgs e)
        //{
        //    Voucher.SelectAll();
        //}

        //private void WireTransfer_GotFocus(object sender, RoutedEventArgs e)
        //{
        //    WireTransfer.SelectAll();
        //}

        //private void MobileMoney_GotFocus(object sender, RoutedEventArgs e)
        //{
        //    MobileMoney.SelectAll();
        //}
    }
}
