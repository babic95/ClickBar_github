using ClickBar.Enums;
using ClickBar.ViewModels;
using ClickBar.ViewModels.AppMain;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using ClickBar_DatabaseSQLManager.Models;

namespace ClickBar.Commands.Login
{
    public class LogoutCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private readonly ViewModelBase _currentView;
        private readonly IServiceProvider _serviceProvider;

        public LogoutCommand(ViewModelBase currentView, IServiceProvider serviceProvider)
        {
            _currentView = currentView;
            _serviceProvider = serviceProvider;
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
                    Logout();
                }
            }
            else
            {
                Logout();
            }
        }

        private void Logout()
        {
            // Resetovanje CashierDB
            var scopedCashierDB = _serviceProvider.GetRequiredService<CashierDB>();
            scopedCashierDB.Id = null;
            scopedCashierDB.Name = null;
            scopedCashierDB.Type = ClickBar_Common.Enums.CashierTypeEnumeration.Error;

            AppStateParameter appStateParameter;
            if (_currentView is AppMainViewModel appMainViewModel)
            {
                appStateParameter = new AppStateParameter(appMainViewModel.DbContext,
                    appMainViewModel.DrljaDbContext,
                    AppStateEnumerable.Login, null);
                appMainViewModel.UpdateAppViewModelCommand.Execute(appStateParameter);
            }
            else if (_currentView is SaleViewModel saleViewModel)
            {
                appStateParameter = new AppStateParameter(saleViewModel.DbContext,
                    saleViewModel.DrljaDbContext,
                    AppStateEnumerable.Login, null);
                saleViewModel.UpdateAppViewModelCommand.Execute(appStateParameter);
            }
        }
    }
}