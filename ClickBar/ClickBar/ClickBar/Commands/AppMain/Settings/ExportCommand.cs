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

        public ExportCommand()
        {
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
                    bool groups = InputOutputExcelFilesManager.Instance.ExportGroups().Result;

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
                    bool items = InputOutputExcelFilesManager.Instance.ExportItems().Result;

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
                    bool cashiers = InputOutputExcelFilesManager.Instance.ExportCashiers().Result;

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