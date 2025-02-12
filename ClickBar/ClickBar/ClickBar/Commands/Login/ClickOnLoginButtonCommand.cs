using ClickBar.Enums;
using ClickBar.ViewModels.Login;
using ClickBar_Common.Enums;
using ClickBar_DatabaseSQLManager.Models;
using ClickBar_Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ClickBar.Commands.Login
{
    public class ClickOnLoginButtonCommand : ICommand
    {
        private readonly string _fromPath = @"C:\CCS ClickBar\ClickBar_Admin\PIN.json";

        public event EventHandler CanExecuteChanged;

        private LoginViewModel _currentView;

        public ClickOnLoginButtonCommand(LoginViewModel currentView)
        {
            _currentView = currentView;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _currentView.Message = string.Empty;

            if (parameter is not null)
            {
                switch (parameter.ToString())
                {
                    case "backspace":
                        if (_currentView.Password.Length > 0)
                            _currentView.Password = _currentView.Password.Remove(_currentView.Password.Length - 1);
                        break;
                    default:
                        _currentView.Password += parameter.ToString();
                        break;
                }
            }
            if (_currentView.Password.Length == 4)
            {
                CashierDB? cashierDB = _currentView.AllCashiers.Find(u => u.Id == _currentView.Password);

                if (_currentView.Password == _currentView.CashierAdmin.Id)
                {
                    cashierDB = _currentView.CashierAdmin;
                }
                else
                {
                    if (cashierDB is null)
                    {

                        _currentView.Message = "Pogrešna lozinka";
                        _currentView.Password = string.Empty;
                        return;
                    }
                }
#if CRNO
#else
                Task.Run(() =>
                {
                    SendPin();
                });
#endif
                AppStateParameter appStateParameter;
                if (cashierDB.Type == CashierTypeEnumeration.Worker)
                {
                    appStateParameter = new AppStateParameter(_currentView.DbContext,
                        _currentView.DrljaDbContext, 
                        AppStateEnumerable.TableOverview,
                        cashierDB);
                }
                else
                {
                    appStateParameter = new AppStateParameter(_currentView.DbContext,
                        _currentView.DrljaDbContext, 
                        AppStateEnumerable.Main, 
                        cashierDB);
                }
                _currentView.UpdateCurrentAppStateViewModelCommand.Execute(appStateParameter);
            }
        }
        private void SendPin()
        {
            try
            {
                string? toPath = SettingsManager.Instance.GetInDirectory();

                if (string.IsNullOrEmpty(toPath))
                {
                    MessageBox.Show("Putanja za slanje PIN-a nije dobra",
                        "Greska u putanji PIN-a",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    return;
                }

                if (File.Exists(@$"{toPath}\PIN.json"))
                {
                    File.Delete(@$"{toPath}\PIN.json");
                }

                File.Copy(_fromPath, @$"{toPath}\PIN.json", true);
            }
            catch
            {
                MessageBox.Show("Greska prilikom slanja PIN-a", 
                    "Greska u PIN-u", 
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
