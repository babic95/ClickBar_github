using ClickBar.ViewModels.AppMain.Statistic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClickBar.Commands.AppMain.Statistic.ViewCalculation
{
    public class CancelDateCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ViewCalculationViewModel _currentViewModel;

        public CancelDateCommand(ViewCalculationViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if(parameter is string)
            {
                string par = parameter.ToString().ToLower();

                switch (par)
                {
                    case "to":
                        _currentViewModel.SearchToCalculationDate = null;
                        break;
                    case "from":
                        _currentViewModel.SearchFromCalculationDate = null;
                        break;
                }
            }
        }
    }
}