using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar.Views.AppMain.AuxiliaryWindows.Statistic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClickBar.Commands.AppMain.Statistic.Parner
{
    public class OpenAddEditPartnerWindow : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private PartnerViewModel _currentViewModel;

        public OpenAddEditPartnerWindow(PartnerViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            AddEditPartnerWindow addEditPartnerWindow = new AddEditPartnerWindow(_currentViewModel);
            addEditPartnerWindow.ShowDialog();
        }
    }
}