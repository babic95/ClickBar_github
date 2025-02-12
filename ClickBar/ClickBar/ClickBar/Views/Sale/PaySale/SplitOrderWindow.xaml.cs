using ClickBar.ViewModels.Sale;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;

namespace ClickBar.Views.Sale.PaySale
{
    /// <summary>
    /// Interaction logic for SplitOrderWindow.xaml
    /// </summary>
    public partial class SplitOrderWindow : Window
    {
        private readonly IServiceProvider _serviceProvider;

        public SplitOrderWindow(IServiceProvider serviceProvider, PaySaleViewModel paySaleViewModel)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;

            // Kreiranje instance SplitOrderViewModel-a koristeći IServiceProvider
            var splitOrderViewModel = _serviceProvider.GetRequiredService<SplitOrderViewModel>();

            splitOrderViewModel.PaySaleViewModel = paySaleViewModel;

            DataContext = splitOrderViewModel;
        }
    }
}