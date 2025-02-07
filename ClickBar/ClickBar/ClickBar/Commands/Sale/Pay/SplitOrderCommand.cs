using ClickBar.ViewModels.Sale;
using ClickBar.Views.Sale.PaySale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClickBar.Commands.Sale.Pay
{
    public class SplitOrderCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private PaySaleViewModel _viewModel;

        public SplitOrderCommand(PaySaleViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            _viewModel.SplitOrder = true;
            _viewModel.SplitOrderWindow = new SplitOrderWindow(_viewModel);
            _viewModel.SplitOrderWindow.ShowDialog();
        }
    }
}