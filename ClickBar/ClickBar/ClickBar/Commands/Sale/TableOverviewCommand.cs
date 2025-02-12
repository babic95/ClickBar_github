using ClickBar.Enums;
using ClickBar.ViewModels;
using ClickBar_Database_Drlja;
using ClickBar_DatabaseSQLManager;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;
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
            AppStateParameter appStateParameter = new AppStateParameter(
                _viewModel.DbContext,
                _viewModel.DrljaDbContext,
                AppStateEnumerable.TableOverview,
                _viewModel.LoggedCashier,
                _viewModel
            );
            _viewModel.UpdateAppViewModelCommand.Execute(appStateParameter);
        }
    }
}