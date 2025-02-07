using ClickBar.ViewModels.AppMain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace ClickBar.Commands.AppMain.Settings
{
    public class SetInOrOutDirectoryCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private SettingsViewModel _currentViewModel;

        public SetInOrOutDirectoryCommand(SettingsViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is string)
            {
                FolderBrowserDialog openFileDlg = new FolderBrowserDialog();
                var result = openFileDlg.ShowDialog();
                if (result.ToString() == string.Empty)
                {
                    System.Windows.MessageBox.Show("Morate da izaberete direktorijum!", "Greška!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                switch (parameter.ToString())
                {
                    case "in":
                        _currentViewModel.Settings.InDirectory = openFileDlg.SelectedPath;
                        break;
                    case "out":
                        _currentViewModel.Settings.OutDirectory = openFileDlg.SelectedPath;
                        break;
                    case "eFaktura":
                        _currentViewModel.Settings.EfakturaDirectory = openFileDlg.SelectedPath;
                        break;
                }
            }
        }
    }
}