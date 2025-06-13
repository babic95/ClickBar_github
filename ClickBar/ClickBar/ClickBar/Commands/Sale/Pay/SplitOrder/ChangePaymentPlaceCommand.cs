using ClickBar.Enums;
using ClickBar.ViewModels.Sale;
using ClickBar.Views.Sale.PaySale;
using ClickBar_Common.Enums;
using ClickBar_Logging;
using ClickBar_Settings;
using Microsoft.Extensions.DependencyInjection;
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
        private IServiceProvider _serviceProvider;

        public ChangePaymentPlaceCommand(IServiceProvider serviceProvider, SplitOrderViewModel viewModel)
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
            try
            {
                int tableId1 = _viewModel.PaySaleViewModel.SaleViewModel.TableId;
                ChangePaymentPlaceViewModel changePaymentPlaceViewModel = _serviceProvider.GetRequiredService<ChangePaymentPlaceViewModel>();
                changePaymentPlaceViewModel.SplitOrderViewModel = _viewModel;
                _viewModel.ChangePaymentPlaceWindow = new ChangePaymentPlaceWindow(changePaymentPlaceViewModel);
                _viewModel.ChangePaymentPlaceWindow.ShowDialog();

                _viewModel.PaySaleViewModel.SplitOrderWindow.Close();

                int tableId2 = changePaymentPlaceViewModel.TableId;

                _viewModel.PaySaleViewModel.SaleViewModel.Reset();

                var typeApp = SettingsManager.Instance.GetTypeApp();

                if (typeApp == TypeAppEnumeration.Sale)
                {
                    AppStateParameter appStateParameter = new AppStateParameter(AppStateEnumerable.Sale,
                    _viewModel.PaySaleViewModel.SaleViewModel.LoggedCashier,
                    tableId1, //preveri azuriranje stola
                    _viewModel.PaySaleViewModel.SaleViewModel,
                    tableId2);
                    _viewModel.PaySaleViewModel.SaleViewModel.UpdateAppViewModelCommand.Execute(appStateParameter);
                }
                else
                {
                    AppStateParameter appStateParameter = new AppStateParameter(AppStateEnumerable.TableOverview,
                    _viewModel.PaySaleViewModel.SaleViewModel.LoggedCashier,
                    tableId1, //preveri azuriranje stola
                    _viewModel.PaySaleViewModel.SaleViewModel,
                    tableId2);
                    _viewModel.PaySaleViewModel.SaleViewModel.UpdateAppViewModelCommand.Execute(appStateParameter);
                }
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