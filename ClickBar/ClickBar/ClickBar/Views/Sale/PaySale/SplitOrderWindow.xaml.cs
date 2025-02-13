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
        public SplitOrderWindow(SplitOrderViewModel splitOrderViewModel)
        {
            InitializeComponent();

            DataContext = splitOrderViewModel;
        }
    }
}