using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar.Views.AppMain.AuxiliaryWindows.Statistic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ClickBar.Commands.AppMain.Statistic.Calculation
{
    public class EditCalculationItemCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private CalculationViewModel _currentViewModel;

        public EditCalculationItemCommand(CalculationViewModel currentViewModel)
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
                    var result = MessageBox.Show("Da li ste sigurni da želite da izmenite kalkulaciju?",
                        "Izmena kalkulacije",
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
                                _currentViewModel.QuantityCommandParameter = "QuantityEdit";

                                _currentViewModel.TotalCalculation -= calculation.InputPrice;
                                _currentViewModel.CalculationQuantityString = calculation.Quantity.ToString();
                                _currentViewModel.CalculationPriceString = calculation.InputPrice.ToString();

                                _currentViewModel.Window = new AddQuantityToCalculationWindow(_currentViewModel);
                                _currentViewModel.Window.ShowDialog();

                                if (_currentViewModel.CalculationQuantity > 0 &&
                                    _currentViewModel.CalculationPrice > 0)
                                {
                                    calculation.Quantity = _currentViewModel.CalculationQuantity;
                                    calculation.InputPrice = _currentViewModel.CalculationPrice;

                                    _currentViewModel.TotalCalculation += calculation.InputPrice;
                                    _currentViewModel.CalculationQuantityString = "0";
                                    _currentViewModel.CalculationPriceString = "0";

                                    MessageBox.Show("Uspešno ste izmenili artikal iz kalkulacije!",
                                        "Uspešno",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Information);
                                }
                                else
                                {
                                    MessageBox.Show("KOLIČINA I ULAZNA CENA MORAJU BITI VEĆI OD 0!",
                                        "Greška",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                MessageBox.Show("Greška prilikom izmene kalkulacije!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}