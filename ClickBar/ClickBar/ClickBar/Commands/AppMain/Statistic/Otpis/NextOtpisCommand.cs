using ClickBar.ViewModels.AppMain.Statistic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using ClickBar_Logging;
using ClickBar.Models.AppMain.Statistic.Otpis;
using ClickBar.Views.AppMain.AuxiliaryWindows.Statistic.Otpis;

namespace ClickBar.Commands.AppMain.Statistic.Otpis
{
    public class NextOtpisCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private OtpisViewModel _currentViewModel;

        public NextOtpisCommand(OtpisViewModel currentViewModel)
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
                if (_currentViewModel.SelectedItem != null)
                {
                    _currentViewModel.CurrentItem = new OtpisItem()
                    {
                        ItemInOtpis = _currentViewModel.SelectedItem.Item,
                        Quantity = 0
                    };

                    _currentViewModel.AllItemsWindow.Close();

                    _currentViewModel.QuantityWindow = new AddQuantityForOtpisWindow(_currentViewModel);
                    _currentViewModel.QuantityWindow.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                Log.Error("NextOtpisCommand -> Execute -> Greska prilikom next komande", ex);
                MessageBox.Show("Greška prilikom prelaska na sledeći korak!",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}