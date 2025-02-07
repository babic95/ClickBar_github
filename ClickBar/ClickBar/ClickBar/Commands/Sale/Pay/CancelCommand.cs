using ClickBar.ViewModels;
using ClickBar.ViewModels.Sale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClickBar.Commands.Sale.Pay
{
    public class CancelCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ViewModelBase _viewModel;

        public CancelCommand(ViewModelBase viewModel)
        {
            _viewModel = viewModel;
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            if(_viewModel is PaySaleViewModel)
            {
                PaySaleViewModel paySaleViewModel = (PaySaleViewModel)_viewModel;
                paySaleViewModel.Window.Close();
            }
            else if(_viewModel is SplitOrderViewModel)
            {
                SplitOrderViewModel splitOrderViewModel = (SplitOrderViewModel)_viewModel;
                splitOrderViewModel.PaySaleViewModel.SplitOrderWindow.Close();
            }
        }
    }
}