using ClickBar.ViewModels.AppMain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClickBar.Commands.AppMain.Report
{
    public class SetDateCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ReportViewModel _currentViewModel;

        public SetDateCommand(ReportViewModel currentViewModel)
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
                _currentViewModel.AuxiliaryWindow.Close();
            }
            catch { }
        }
    }
}
