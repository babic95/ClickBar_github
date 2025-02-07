using ClickBar.Models.AppMain.Statistic;
using ClickBar.ViewModels.AppMain.Statistic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using ClickBar.Views.AppMain.AuxiliaryWindows.Statistic.ViewCalculation;
using ClickBar_Database.Models;
using ClickBar_Database;

namespace ClickBar.Commands.AppMain.Statistic.ViewCalculation
{
    public class OpenEditCalculationItemEditWindowCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ViewCalculationViewModel _currentViewModel;

        public OpenEditCalculationItemEditWindowCommand(ViewCalculationViewModel currentViewModel)
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

                var result = MessageBox.Show("Da li ste sigurni da želite da izmenite artikal iz kalkulacije?", "Izmena artikla iz kalkulacije", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    if (_currentViewModel.CurrentCalculation != null &&
                        _currentViewModel.CurrentCalculation.CalculationItems != null &&
                        _currentViewModel.CurrentCalculation.CalculationItems.Any())
                    {
                        var calculationItem = _currentViewModel.CurrentCalculation.CalculationItems.FirstOrDefault(calculation => calculation.Item.Id == itemId);

                        if (calculationItem != null)
                        {
                            using (SqliteDbContext sqliteDbContext = new SqliteDbContext())
                            {
                                var group = sqliteDbContext.ItemGroups.Find(calculationItem.IdGroupItems);

                                if (group == null)
                                {
                                    MessageBox.Show("ARTIKAL MORA DA PRIPADA NEKOJ GRUPI!",
                                        "Greška",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Error);

                                    return;
                                }

                                if (group.Name.ToLower().Contains("sirovine") ||
                                    group.Name.ToLower().Contains("sirovina"))
                                {
                                    _currentViewModel.VisibilityProsecnaPrice = Visibility.Visible;

                                    _currentViewModel.ProsecnaPrice = calculationItem.Item.InputUnitPrice != null &&
                                        calculationItem.Item.InputUnitPrice.HasValue ? calculationItem.Item.InputUnitPrice.Value : 0;
                                }
                                else
                                {
                                    _currentViewModel.VisibilityProsecnaPrice = Visibility.Hidden;

                                    _currentViewModel.OldPrice = calculationItem.Item.SellingUnitPrice;
                                }

                                decimal totalItem = calculationItem.InputPrice;
                                _currentViewModel.CalculationQuantityString = calculationItem.Quantity.ToString();
                                _currentViewModel.CalculationPriceString = calculationItem.InputPrice.ToString();
                                _currentViewModel.JM = calculationItem.Item.Jm;

                                _currentViewModel.EditQuantityWindow = new EditQuantityCalculationWindow(_currentViewModel);
                                _currentViewModel.EditQuantityWindow.ShowDialog();

                                if (_currentViewModel.CalculationQuantity > 0 &&
                                        _currentViewModel.CalculationPrice > 0)
                                {
                                    calculationItem.Quantity = _currentViewModel.CalculationQuantity;
                                    calculationItem.InputPrice = _currentViewModel.CalculationPrice;

                                    _currentViewModel.CurrentCalculation.InputTotalPrice += calculationItem.InputPrice - totalItem;
                                    _currentViewModel.CalculationQuantityString = "0";
                                    _currentViewModel.JM = string.Empty;

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
        }
    }
}