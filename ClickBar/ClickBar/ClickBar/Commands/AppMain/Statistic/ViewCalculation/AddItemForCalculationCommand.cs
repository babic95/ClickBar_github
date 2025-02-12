using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar.Views.AppMain.AuxiliaryWindows.Statistic.ViewCalculation;
using ClickBar_DatabaseSQLManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace ClickBar.Commands.AppMain.Statistic.ViewCalculation
{
    public class AddItemForCalculationCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ViewCalculationViewModel _currentViewModel;

        public AddItemForCalculationCommand(ViewCalculationViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (_currentViewModel.CurrentInventoryStatusCalculation != null)
            {
                var group = _currentViewModel.DbContext.ItemGroups.Find(_currentViewModel.CurrentInventoryStatusCalculation.IdGroupItems);

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

                    _currentViewModel.ProsecnaPrice = _currentViewModel.CurrentInventoryStatusCalculation.Item.InputUnitPrice != null &&
                        _currentViewModel.CurrentInventoryStatusCalculation.Item.InputUnitPrice.HasValue ? _currentViewModel.CurrentInventoryStatusCalculation.Item.InputUnitPrice.Value : 0;
                }
                else
                {
                    _currentViewModel.VisibilityProsecnaPrice = Visibility.Hidden;

                    _currentViewModel.OldPrice = _currentViewModel.CurrentInventoryStatusCalculation.Item.SellingUnitPrice;
                }

                decimal totalItem = 0;
                _currentViewModel.CalculationQuantityString = "0";
                _currentViewModel.CalculationPriceString = "0";
                _currentViewModel.JM = _currentViewModel.CurrentInventoryStatusCalculation.Item.Jm;

                _currentViewModel.EditQuantityWindow = new EditQuantityCalculationWindow(_currentViewModel);
                _currentViewModel.EditQuantityWindow.ShowDialog();

                if (_currentViewModel.CalculationQuantity > 0 &&
                    _currentViewModel.CalculationPrice > 0)
                {
                    var calculationItem = _currentViewModel.CurrentCalculation.CalculationItems.FirstOrDefault(calculation =>
                    calculation.Item.Id == _currentViewModel.CurrentInventoryStatusCalculation.Item.Id);

                    if (calculationItem != null)
                    {
                        totalItem = calculationItem.InputPrice;
                        calculationItem.Quantity += _currentViewModel.CalculationQuantity;
                        calculationItem.InputPrice += _currentViewModel.CalculationPrice;
                    }
                    else
                    {
                        calculationItem = new Models.AppMain.Statistic.Invertory()
                        {
                            Quantity = _currentViewModel.CalculationQuantity,
                            InputPrice = _currentViewModel.CalculationPrice,
                            Item = _currentViewModel.CurrentInventoryStatusCalculation.Item,
                            IdGroupItems = group.Id,
                        };

                        _currentViewModel.CurrentCalculation.CalculationItems.Add(calculationItem);
                    }

                    _currentViewModel.CurrentCalculation.InputTotalPrice += calculationItem.InputPrice - totalItem;
                    _currentViewModel.CalculationQuantityString = "0";
                    _currentViewModel.JM = string.Empty;

                    MessageBox.Show("Uspešno ste dodali artikal u kalkulaciju!",
                        "Uspešno",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    _currentViewModel.AllItemsWindow.Close();
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