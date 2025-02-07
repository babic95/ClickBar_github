using ClickBar.Enums;
using ClickBar.ViewModels;
using ClickBar.ViewModels.AppMain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ClickBar.Commands.Login
{
    public class LogoutCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ViewModelBase _currentView;

        public LogoutCommand(ViewModelBase currentView)
        {
            _currentView = currentView;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter == null)
            {
                MessageBoxResult result = MessageBox.Show("Da li ste sigurni da hoćete da se izlogujete?", "Izloguj se", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    AppStateParameter appStateParameter = new AppStateParameter(AppStateEnumerable.Login, null);
                    if (_currentView is AppMainViewModel)
                    {
                        AppMainViewModel appMainViewModel = (AppMainViewModel)_currentView;
                        appMainViewModel.UpdateAppViewModelCommand.Execute(appStateParameter);
                    }
                    else if (_currentView is SaleViewModel)
                    {
                        SaleViewModel saleViewModel = (SaleViewModel)_currentView;
                        saleViewModel.UpdateAppViewModelCommand.Execute(appStateParameter);
                    }
                }
            }
            else
            {
                AppStateParameter appStateParameter = new AppStateParameter(AppStateEnumerable.Login, null);
                if (_currentView is AppMainViewModel)
                {
                    AppMainViewModel appMainViewModel = (AppMainViewModel)_currentView;
                    appMainViewModel.UpdateAppViewModelCommand.Execute(appStateParameter);
                }
                else if (_currentView is SaleViewModel)
                {
                    SaleViewModel saleViewModel = (SaleViewModel)_currentView;
                    saleViewModel.UpdateAppViewModelCommand.Execute(appStateParameter);
                }
            }
        }
    }
}
