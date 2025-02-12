using ClickBar.Enums;
using ClickBar.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows.Input;

namespace ClickBar.Commands.Sale
{
    public class TableOverviewCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private readonly SaleViewModel _viewModel;
        private readonly IServiceProvider _serviceProvider;

        public TableOverviewCommand(SaleViewModel viewModel, IServiceProvider serviceProvider)
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
            _viewModel.TableOverviewViewModel = _serviceProvider.GetRequiredService<TableOverviewViewModel>();

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