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
                            if (!(_navigator.CurrentViewModel is LoginCardViewModel))
                            {
                                _navigator.CurrentViewModel = _serviceProvider.GetRequiredService<LoginCardViewModel>();
                            }
                        }
                        else
                        {
                            if (!(_navigator.CurrentViewModel is LoginViewModel))
                            {
                                _navigator.CurrentViewModel = _serviceProvider.GetRequiredService<LoginViewModel>();
                            }
                        }
                        break;
                    case AppStateEnumerable.Main:
                        if (!(_navigator.CurrentViewModel is AppMainViewModel))
                        {
                            var appMainViewModel = _serviceProvider.GetRequiredService<AppMainViewModel>();
                            // Set additional properties if needed
                            _navigator.CurrentViewModel = appMainViewModel;
                        }
                        break;
                    case AppStateEnumerable.Sale:
                        if (appState.ViewModel is SaleViewModel saleViewModel)
                        {
                            if (!(_navigator.CurrentViewModel is SaleViewModel))
                            {
                                _navigator.CurrentViewModel = saleViewModel;
                            }
                        }
                        else
                        {
                            if (!(_navigator.CurrentViewModel is SaleViewModel))
                            {
                                var newSaleViewModel = _serviceProvider.GetRequiredService<SaleViewModel>();
                                // Set additional properties if needed
                                _navigator.CurrentViewModel = newSaleViewModel;
                            }
                        }
                        break;
                    case AppStateEnumerable.TableOverview:
                        if (appState.ViewModel is SaleViewModel tableOverviewSaleViewModel)
                        {
                            if (!(_navigator.CurrentViewModel is TableOverviewViewModel))
                            {
                                _navigator.CurrentViewModel = tableOverviewSaleViewModel.TableOverviewViewModel;
                            }
                        }
                        else
                        {
                            if (!(_navigator.CurrentViewModel is TableOverviewViewModel))
                            {
                                var newTableOverviewSaleViewModel = _serviceProvider.GetRequiredService<SaleViewModel>();
                                _navigator.CurrentViewModel = newTableOverviewSaleViewModel.TableOverviewViewModel;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
}