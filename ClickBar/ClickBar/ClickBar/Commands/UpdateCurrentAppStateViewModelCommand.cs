using ClickBar.Enums;
using ClickBar.State.Navigators;
using ClickBar.ViewModels;
using ClickBar.ViewModels.AppMain;
using ClickBar.ViewModels.Login;
using ClickBar.ViewModels.Sale;
using ClickBar_Settings;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows.Input;

namespace ClickBar.Commands
{
    public class UpdateCurrentAppStateViewModelCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private readonly INavigator _navigator;
        private readonly IServiceProvider _serviceProvider;

        public UpdateCurrentAppStateViewModelCommand(INavigator navigator, IServiceProvider serviceProvider)
        {
            _navigator = navigator;
            _serviceProvider = serviceProvider;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is AppStateParameter appState)
            {
                switch (appState.State)
                {
                    case AppStateEnumerable.Login:
                        if (SettingsManager.Instance.EnableSmartCard())
                        {
                            _navigator.CurrentViewModel = _serviceProvider.GetRequiredService<LoginCardViewModel>();
                        }
                        else
                        {
                            _navigator.CurrentViewModel = _serviceProvider.GetRequiredService<LoginViewModel>();
                        }
                        break;
                    case AppStateEnumerable.Main:
                        var appMainViewModel = _serviceProvider.GetRequiredService<AppMainViewModel>();
                        // Set additional properties if needed
                        _navigator.CurrentViewModel = appMainViewModel;
                        break;
                    case AppStateEnumerable.Sale:
                        if (appState.ViewModel is SaleViewModel saleViewModel)
                        {
                            _navigator.CurrentViewModel = saleViewModel;
                        }
                        else
                        {
                            var newSaleViewModel = _serviceProvider.GetRequiredService<SaleViewModel>();
                            // Set additional properties if needed
                            _navigator.CurrentViewModel = newSaleViewModel;
                        }
                        break;
                    case AppStateEnumerable.TableOverview:
                        if (appState.ViewModel is SaleViewModel tableOverviewSaleViewModel)
                        {
                            _navigator.CurrentViewModel = tableOverviewSaleViewModel.TableOverviewViewModel;
                        }
                        else
                        {
                            var newTableOverviewSaleViewModel = _serviceProvider.GetRequiredService<SaleViewModel>();
                            _navigator.CurrentViewModel = newTableOverviewSaleViewModel.TableOverviewViewModel;
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
}