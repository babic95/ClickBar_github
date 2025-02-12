using ClickBar.ViewModels.AppMain;
using ClickBar_InputOutputExcelFiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ClickBar.Commands.AppMain.Settings
{
    public class ExportCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private SettingsViewModel _currentViewModel;
        public ExportCommand(SettingsViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            switch (parameter.ToString())
            {
                case "Groups":
                    bool groups = InputOutputExcelFilesManager.Instance.ExportGroups(_currentViewModel.DbContext).Result;

                    if (groups)
                    {
                        MessageBox.Show("Uspešno ste izvezli grupe!", "Uspešno!", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Greška prilikom izvoza grupe!", "Greška!", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    break;
                case "Items":
                    bool items = InputOutputExcelFilesManager.Instance.ExportItems(_currentViewModel.DbContext).Result;

                    if (items)
                    {
                        MessageBox.Show("Uspešno ste izvezli artikle!", "Uspešno!", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Greška prilikom izvoza artikala!", "Greška!", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    break;
                case "Cashirs":
                    bool cashiers = InputOutputExcelFilesManager.Instance.ExportCashiers(_currentViewModel.DbContext).Result;

                    if (cashiers)
                    {
                        MessageBox.Show("Uspešno ste izvezli kasire!", "Uspešno!", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Greška prilikom izvoza kasira!", "Greška!", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    break;
            }
        }
    }
}