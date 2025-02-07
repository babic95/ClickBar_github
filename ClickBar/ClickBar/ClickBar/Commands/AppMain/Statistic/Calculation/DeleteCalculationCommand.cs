using ClickBar.ViewModels.AppMain.Statistic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ClickBar.Commands.AppMain.Statistic.Calculation
{
    public class DeleteCalculationItemCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private CalculationViewModel _currentViewModel;

        public DeleteCalculationItemCommand(CalculationViewModel currentViewModel)
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
                if (parameter is string)
                {
                    var result = MessageBox.Show("Da li ste sigurni da želite da obrišete artikal iz kalkulacije?",
                        "Brisanje artikla iz kalkulacije",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        string idItem = (string)parameter;
                        if (_currentViewModel.Calculations != null &&
                            _currentViewModel.Calculations.Any())
                        {
                            var calculation = _currentViewModel.Calculations.FirstOrDefault(calculation => calculation.Item.Id == idItem);

                            if (calculation != null)
                            {
                                _currentViewModel.TotalCalculation -= calculation.InputPrice;
                                _currentViewModel.Calculations.Remove(calculation);

                                MessageBox.Show("Uspešno ste obrisali kalkulaciju!",
                                    "Uspešno",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                            }
                        }
                    }
                }
            }
            catch
            {
                MessageBox.Show("Greška prilikom brisanja kalkulacije!",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}