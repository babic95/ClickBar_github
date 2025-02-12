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
    public class ImportCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private SettingsViewModel _currentViewModel;
        public ImportCommand(SettingsViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            try
            {
                switch (parameter.ToString())
                {
                    case "Supergrups":
                        var supergrups = InputOutputExcelFilesManager.Instance.ImportSupergroups(_currentViewModel.DbContext).Result;
                        if (supergrups is not null)
                        {
                            MessageBox.Show("Uspešno ste uvezli nadgrupe!", "Uspešno!", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Greška prilikom uvoza nadgrupa!", "Greška!", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        break;
                    case "Groups":
                        var groups = InputOutputExcelFilesManager.Instance.ImportGroups(_currentViewModel.DbContext).Result;
                        if (groups is not null)
                        {
                            MessageBox.Show("Uspešno ste uvezli grupe!", "Uspešno!", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Greška prilikom uvoza grupa!", "Greška!", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        break;
                    case "Items":
                        var items = InputOutputExcelFilesManager.Instance.ImportItems(_currentViewModel.DbContext).Result;
                        if (items is not null)
                        {
                            MessageBox.Show("Uspešno ste uvezli artikle!", "Uspešno!", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Greška prilikom uvoza artikala!", "Greška!", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        break;
                    case "Cashirs":
                        var cashiers = InputOutputExcelFilesManager.Instance.ImportCashiers(_currentViewModel.DbContext).Result;

                        if (cashiers is not null)
                        {
                            MessageBox.Show("Uspešno ste uvezli kasire!", "Uspešno!", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Greška prilikom uvoza kasira!", "Greška!", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        break;
                }
            }
            catch
            {
                MessageBox.Show("Zatvorite fajl koji želite da uvezete!", "Greška pri uvozu", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}