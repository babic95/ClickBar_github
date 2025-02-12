using ClickBar.ViewModels.Sale;
using ClickBar.Views.Sale.PaySale;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows.Input;

namespace ClickBar.Commands.Sale.Pay
{
    public class SplitOrderCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private readonly PaySaleViewModel _viewModel;
        private readonly IServiceProvider _serviceProvider;

        public SplitOrderCommand(PaySaleViewModel viewModel, IServiceProvider serviceProvider)
        {
            _viewModel = viewModel;
            _serviceProvider = serviceProvider;
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _viewModel.SplitOrder = true;

            // Kreiranje instance SplitOrderWindow-a koristeći IServiceProvider
            var splitOrderWindow = _serviceProvider.GetRequiredService<SplitOrderWindow>();

            _viewModel.SplitOrderWindow = splitOrderWindow;
            _viewModel.SplitOrderWindow.ShowDialog();
        }
    }
}