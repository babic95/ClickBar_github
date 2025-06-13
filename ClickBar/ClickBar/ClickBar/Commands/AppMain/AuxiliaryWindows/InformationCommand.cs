using ClickBar.ViewModels;
using ClickBar.Views.AppMain.AuxiliaryWindows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClickBar.Commands.AppMain.AuxiliaryWindows
{
    public class InformationCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private AppMainViewModel _viewModel;

        public InformationCommand(AppMainViewModel appMainViewModel)
        {
            _viewModel = appMainViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            InformationWindow informationWindow = new InformationWindow(_viewModel);
            informationWindow.Show();
        }
    }
}
