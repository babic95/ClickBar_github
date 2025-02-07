using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar.Views.AppMain.AuxiliaryWindows.Statistic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClickBar.Commands.AppMain.Statistic.Radnici
{
    public class OpenAddEditRadnikWindow : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private RadniciViewModel _currentViewModel;

        public OpenAddEditRadnikWindow(RadniciViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            AddEditRadnikWindow addEditRadnikWindow = new AddEditRadnikWindow(_currentViewModel);
            addEditRadnikWindow.ShowDialog();
        }
    }
}