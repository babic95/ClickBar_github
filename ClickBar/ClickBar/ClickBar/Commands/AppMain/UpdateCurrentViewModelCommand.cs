using ClickBar.State.Navigators;
using ClickBar.ViewModels;
using ClickBar.ViewModels.AppMain;
using ClickBar_Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClickBar.Commands.AppMain
{
    public class UpdateCurrentViewModelCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private AppMainViewModel _currentViewModel;

        public UpdateCurrentViewModelCommand(AppMainViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is CashierViewType)
            {
                CashierViewType viewType = (CashierViewType)parameter;
                switch (viewType)
                {
                    case CashierViewType.Report:
                        if (_currentViewModel.CurrentViewModel is not ReportViewModel)
                        {
                            _currentViewModel.CheckedReport();
                        }
                        break;
                    case CashierViewType.Statistics:
                        if (_currentViewModel.CurrentViewModel is not StatisticsViewModel)
                        {
                            _currentViewModel.CheckedStatistics();
                        }
                        break;
                    case CashierViewType.Settings:
                        if (_currentViewModel.LoggedCashier.Type == CashierTypeEnumeration.Admin)
                        {
                            if (_currentViewModel.CurrentViewModel is not SettingsViewModel)
                            {
                                _currentViewModel.CheckedSettings();
                            }
                        }
                        break;
                    case CashierViewType.Admin:
                        if (_currentViewModel.LoggedCashier.Type == CashierTypeEnumeration.Admin)
                        {
                            if (_currentViewModel.CurrentViewModel is not AdminViewModel)
                            {
                                _currentViewModel.CheckedAdmin();
                            }
                        }
                        break;
                    default:
                        break;
                }
                return;
            }
        }
    }
}
