using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar.Views.AppMain.AuxiliaryWindows.Statistic.Otpis;
using ClickBar_Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ClickBar.Commands.AppMain.Statistic.Otpis
{
    public class OpenAllItemsCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private OtpisViewModel _currentViewModel;

        public OpenAllItemsCommand(OtpisViewModel currentViewModel)
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
                _currentViewModel.AllItemsWindow = new AllItemsForOtpisWindow(_currentViewModel);
                _currentViewModel.AllItemsWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                Log.Error("OpenAllItemsCommand -> Execute -> Greska prilikom otvaranja prozora za sve artikle", ex);
                MessageBox.Show("Greška prilikom otvaranja prozora!", 
                    "Greška", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
        }
    }
}