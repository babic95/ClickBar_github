using ClickBar.ViewModels.Sale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using ClickBar_Logging;

namespace ClickBar.Commands.Sale.Pay.SplitOrder.ChangePaymentPlace
{
    public class CancelChangePaymentPlaceCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ChangePaymentPlaceViewModel _viewModel;

        public CancelChangePaymentPlaceCommand(ChangePaymentPlaceViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            try
            {
                _viewModel.SplitOrderViewModel.ChangePaymentPlaceWindow.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Desila se greška!",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Log.Error("CancelChangePaymentPlaceCommand -> Greška u vracanju u nazad", ex);
            }
        }
    }
}