using ClickBar.Enums;
using ClickBar.ViewModels;
using ClickBar_Common.Enums;
using ClickBar_DatabaseSQLManager;
using ClickBar_Logging;
using ClickBar_Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using System.Windows.Input;

namespace ClickBar.Commands.Sale
{
    public class TableOverviewCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private readonly SaleViewModel _viewModel;

        public TableOverviewCommand(SaleViewModel viewModel)
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
                int tableId = _viewModel.TableId;

                _viewModel.Reset();

                var typeApp = SettingsManager.Instance.GetTypeApp();

                if (typeApp == TypeAppEnumeration.Sale)
                {
                    AppStateParameter appStateParameter = new AppStateParameter(AppStateEnumerable.Sale,
                    _viewModel.LoggedCashier,
                    tableId,
                    _viewModel);
                    _viewModel.UpdateAppViewModelCommand.Execute(appStateParameter);
                }
                else
                {
                    AppStateParameter appStateParameter = new AppStateParameter(AppStateEnumerable.TableOverview,
                    _viewModel.LoggedCashier,
                    tableId,
                    _viewModel);
                    _viewModel.UpdateAppViewModelCommand.Execute(appStateParameter);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"TableOverviewCommand -> Greška prilikom otvaranja pregleda sale: ", ex);
                MessageBox.Show("Greška prilikom otvaranja pregleda sale!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}