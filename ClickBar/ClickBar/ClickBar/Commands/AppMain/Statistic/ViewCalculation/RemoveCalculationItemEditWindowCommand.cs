using ClickBar.Models.AppMain.Statistic;
using ClickBar.ViewModels.AppMain.Statistic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ClickBar.Commands.AppMain.Statistic.ViewCalculation
{
    public class RemoveCalculationItemEditWindowCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ViewCalculationViewModel _currentViewModel;

        public RemoveCalculationItemEditWindowCommand(ViewCalculationViewModel currentViewModel)
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
                string itemId = (string)parameter;
                var result = MessageBox.Show("Da li ste sigurni da želite da obrišete artikal iz kalkulacije?", "Brisanje artikla iz kalkulacije", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    if (_currentViewModel.CurrentCalculation != null &&
                        _currentViewModel.CurrentCalculation.CalculationItems != null &&
                        _currentViewModel.CurrentCalculation.CalculationItems.Any())
                    {
                        var calculationItem = _currentViewModel.CurrentCalculation.CalculationItems.FirstOrDefault(calculation => calculation.Item.Id == itemId);

                        if (calculationItem != null)
                        {
                            _currentViewModel.CurrentCalculation.InputTotalPrice -= calculationItem.InputPrice;
                            _currentViewModel.CurrentCalculation.CalculationItems.Remove(calculationItem);

                            MessageBox.Show("Uspešno ste obrisali artikal iz kalkulacije!",
                                "Uspešno",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                        }
                    }
                }
            }
        }
    }
}