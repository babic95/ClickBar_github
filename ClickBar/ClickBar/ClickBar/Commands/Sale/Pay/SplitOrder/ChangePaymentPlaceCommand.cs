using ClickBar.Enums;
using ClickBar.ViewModels.Sale;
using ClickBar.Views.Sale.PaySale;
using ClickBar_Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ClickBar.Commands.Sale.Pay.SplitOrder
{
    public class ChangePaymentPlaceCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private SplitOrderViewModel _viewModel;

        public ChangePaymentPlaceCommand(SplitOrderViewModel viewModel)
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
                ChangePaymentPlaceViewModel changePaymentPlaceViewModel = new ChangePaymentPlaceViewModel(_viewModel);
                _viewModel.ChangePaymentPlaceWindow = new ChangePaymentPlaceWindow(changePaymentPlaceViewModel);
                _viewModel.ChangePaymentPlaceWindow.ShowDialog();

                _viewModel.PaySaleViewModel.SplitOrderWindow.Close();

                _viewModel.PaySaleViewModel.SaleViewModel.Reset();

                AppStateParameter appStateParameter = new AppStateParameter(AppStateEnumerable.TableOverview,
                    _viewModel.PaySaleViewModel.SaleViewModel.LoggedCashier,
                    _viewModel.PaySaleViewModel.SaleViewModel);
                _viewModel.PaySaleViewModel.SaleViewModel.UpdateAppViewModelCommand.Execute(appStateParameter);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Desila se greška prilikom prebacivanja stola!",
                    "Greška u prebacivanju stola",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Log.Error("ChangePaymentPlaceCommand -> Greška u prebacivanju stola", ex);
            }
        }
    }
}