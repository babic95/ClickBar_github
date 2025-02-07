using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar.Views.AppMain.AuxiliaryWindows.Statistic.ViewCalculation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClickBar.Commands.AppMain.Statistic.ViewCalculation
{
    public class OpenAllItemsWindowCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ViewCalculationViewModel _currentViewModel;

        public OpenAllItemsWindowCommand(ViewCalculationViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (_currentViewModel.AllItemsWindow != null &&
                _currentViewModel.AllItemsWindow.IsActive)
            {
                _currentViewModel.AllItemsWindow.Close();
            }

            _currentViewModel.AllItemsWindow = new AllItemsForCalculationWindow(_currentViewModel);
            _currentViewModel.AllItemsWindow.ShowDialog();
        }
    }
}