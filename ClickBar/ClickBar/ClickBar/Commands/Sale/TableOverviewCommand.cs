using ClickBar.Enums;
using ClickBar.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClickBar.Commands.Sale
{
    public class TableOverviewCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private SaleViewModel _viewModel;

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
            _viewModel.TableOverviewViewModel = new TableOverviewViewModel(_viewModel);
            //_viewModel.CurrentOrder = null;

            AppStateParameter appStateParameter = new AppStateParameter(AppStateEnumerable.TableOverview,
                _viewModel.LoggedCashier,
                _viewModel);
            _viewModel.UpdateAppViewModelCommand.Execute(appStateParameter);
        }
    }
}
