using ClickBar.ViewModels.AppMain;
using ClickBar_Common.Enums;
using ClickBar_Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ClickBar.Commands.AppMain.Settings
{
    public class SaveSettingsCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private SettingsViewModel _currentViewModel;

        public SaveSettingsCommand(SettingsViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            MessageBoxResult result = MessageBox.Show("Sigurni ste da želite da sačuvate podešavanje?", 
                "Čuvanje podešavanja", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                SettingsFile settingsFile = new SettingsFile()
                {
                    EnableTableOverview = _currentViewModel.Settings.EnableTableOverview,
                    EnableSmartCard = _currentViewModel.Settings.EnableSmartCard,
                    CancelOrderFromTable = _currentViewModel.Settings.CancelOrderFromTable,
                    EnableSuperGroup = _currentViewModel.Settings.EnableSuperGroup,
                    //EnableFileSystemWatcher = _currentViewModel.Settings.EnableFileSystemWatcher,
                    InDirectory = _currentViewModel.Settings.InDirectory,
                    OutDirectory = _currentViewModel.Settings.OutDirectory,
                    PrinterName = _currentViewModel.Settings.PrinterName,
                    PrinterNameSank1 = _currentViewModel.Settings.PrinterNameSank1,
                    PrinterNameKuhinja = _currentViewModel.Settings.PrinterNameKuhinja,
                    EfakturaDirectory = _currentViewModel.Settings.EfakturaDirectory,
                    PathToDrljaKuhinjaDB = _currentViewModel.Settings.PathToDrljaKuhinjaDB,
                    PathToMainDB = _currentViewModel.Settings.PathToMainDB,
                    HostPC_IP = _currentViewModel.Settings.HostPC_IP,
                    RunPorudzbineServis = _currentViewModel.Settings.RunPorudzbineServis,
                    //UrlToLPFR = _currentViewModel.Settings.UrlToLPFR,
                    TypeApp = _currentViewModel.Settings.TypeApp
                };

                if (_currentViewModel.Settings.Pos80mmFormat)
                {
                    settingsFile.PrinterFormat = PrinterFormatEnumeration.Pos80mm;
                }
                else
                {
                    settingsFile.PrinterFormat = PrinterFormatEnumeration.Pos58mm;
                }

                SettingsManager.Instance.SetSettingsFile(settingsFile);

                //Task.Run(() =>
                //{
                //    if (settingsFile.EnableFileSystemWatcher)
                //    {
                //        _ = FileSystemWatcherManager.Instance.RunFileSystemWatcher();
                //    }
                //    else
                //    {
                //        _ = FileSystemWatcherManager.Instance.CloseFileSystemWatcher();
                //    }
                //});

                MessageBox.Show("Uspešno ste sačuvali podešavanja!", "Uspešno!", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}