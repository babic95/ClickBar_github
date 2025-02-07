using ClickBar.Models.AppMain.Statistic;
using ClickBar.ViewModels;
using ClickBar.ViewModels.AppMain.Statistic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClickBar.Commands.AppMain.Statistic.Calculation
{
    public class SearchGroupsCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ViewModelBase _currentViewModel;

        public SearchGroupsCommand(ViewModelBase currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (_currentViewModel is CalculationViewModel)
            {
                CalculationViewModel calculationViewModel = (CalculationViewModel)_currentViewModel;
                if (calculationViewModel.CurrentGroup != null)
                {
                    if (calculationViewModel.CurrentGroup.Id == -1)
                    {
                        calculationViewModel.SearchItems = new List<Invertory>(calculationViewModel.InventoryStatusAll);
                    }
                    else
                    {
                        calculationViewModel.SearchItems = new List<Invertory>(calculationViewModel.InventoryStatusAll.Where(item =>
                        item.IdGroupItems == calculationViewModel.CurrentGroup.Id));
                    }

                    calculationViewModel.SearchText = string.Empty;
                }
            }
            else if(_currentViewModel is ViewCalculationViewModel)
            {
                ViewCalculationViewModel viewCalculationViewModel = (ViewCalculationViewModel)_currentViewModel;
                if (viewCalculationViewModel.CurrentGroup != null)
                {
                    if (viewCalculationViewModel.CurrentGroup.Id == -1)
                    {
                        viewCalculationViewModel.SearchItems = new List<Invertory>(viewCalculationViewModel.InventoryStatusAll);
                    }
                    else
                    {
                        viewCalculationViewModel.SearchItems = new List<Invertory>(viewCalculationViewModel.InventoryStatusAll.Where(item =>
                        item.IdGroupItems == viewCalculationViewModel.CurrentGroup.Id));
                    }

                    viewCalculationViewModel.SearchText = string.Empty;
                }
            }
        }
    }
}