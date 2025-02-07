using ClickBar.Enums;
using ClickBar.State.Navigators;
using ClickBar.ViewModels;
using ClickBar.ViewModels.AppMain;
using ClickBar.ViewModels.Login;
using ClickBar.ViewModels.Sale;
using ClickBar_Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClickBar.Commands
{
    public class UpdateCurrentAppStateViewModelCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private INavigator _navigator;

        public UpdateCurrentAppStateViewModelCommand(INavigator navigator)
        {
            _navigator = navigator;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is AppStateParameter)
            {
                AppStateParameter appState = (AppStateParameter)parameter;
                switch (appState.State)
                {
                    case AppStateEnumerable.Login:
                        if (SettingsManager.Instance.EnableSmartCard())
                        {
                            _navigator.CurrentViewModel = new LoginCardViewModel(_navigator.UpdateCurrentViewModelCommand);
                        }
                        else
                        {
                            _navigator.CurrentViewModel = new LoginViewModel(_navigator.UpdateCurrentViewModelCommand);
                        }
                        break;
                    case AppStateEnumerable.Main:
                        _navigator.CurrentViewModel = new AppMainViewModel(appState.LoggedCashier, _navigator.UpdateCurrentViewModelCommand);
                        break;
                    case AppStateEnumerable.Sale:
                        if (appState.ViewModel is not null && appState.ViewModel is SaleViewModel)
                        {
                            SaleViewModel saleViewModel = (SaleViewModel)appState.ViewModel;
                            _navigator.CurrentViewModel = saleViewModel;
                        }
                        else
                        {
                            _navigator.CurrentViewModel = new SaleViewModel(appState.LoggedCashier, 
                                _navigator.UpdateCurrentViewModelCommand);
                        }
                        break;
                    case AppStateEnumerable.TableOverview:
                        if (appState.ViewModel is not null && appState.ViewModel is SaleViewModel)
                        {
                            SaleViewModel saleViewModel = (SaleViewModel)appState.ViewModel;
                            _navigator.CurrentViewModel = saleViewModel.TableOverviewViewModel;
                        }
                        else
                        {
                            SaleViewModel saleViewModel = new SaleViewModel(appState.LoggedCashier,
                                _navigator.UpdateCurrentViewModelCommand);
                            _navigator.CurrentViewModel = saleViewModel.TableOverviewViewModel;
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
