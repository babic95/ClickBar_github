using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar.Views.AppMain.AuxiliaryWindows.Statistic;
using ClickBar_DatabaseSQLManager;
using ClickBar_DatabaseSQLManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ClickBar.Commands.AppMain.Statistic.InventoryStatus
{
    public class EditNormCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private InventoryStatusViewModel _currentViewModel;

        public EditNormCommand(InventoryStatusViewModel currentViewModel)
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
                if (parameter is string &&
                    _currentViewModel.CurrentInventoryStatus != null &&
                    _currentViewModel.CurrentInventoryStatus.Item != null)
                {
                    string idItem = (string)parameter;

                    var result = MessageBox.Show("Da li zaista želite da izmenite normativ artikala?",
                        "Izmena normativa artikala",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        if (string.IsNullOrEmpty(_currentViewModel.CurrentInventoryStatus.Item.Id))
                        {
                            var norm = _currentViewModel.Norma.FirstOrDefault(norm => norm.Item.Id == idItem);

                            if (norm != null)
                            {
                                _currentViewModel.NormQuantity = norm.Quantity;
                                _currentViewModel.QuantityCommandParameter = "QuantityEdit";

                                _currentViewModel.WindowHelper = new AddQuantityToNormWindow(_currentViewModel);
                                _currentViewModel.WindowHelper.ShowDialog();

                                if (_currentViewModel.NormQuantity > 0)
                                {
                                    norm.Quantity = _currentViewModel.NormQuantity;
                                    _currentViewModel.NormQuantity = 0;
                                }
                                else
                                {
                                    MessageBox.Show("KOLIČINA MORA BITI BROJ!", "", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                        }
                        else
                        {
                            ItemDB? currentItemDB = _currentViewModel.DbContext.Items.Find(_currentViewModel.CurrentInventoryStatus.Item.Id);

                            if (currentItemDB != null)
                            {
                                var itemInNorm = _currentViewModel.DbContext.ItemsInNorm.FirstOrDefault(x => x.IdNorm == currentItemDB.IdNorm && x.IdItem == idItem);

                                if (itemInNorm != null)
                                {
                                    _currentViewModel.NormQuantity = itemInNorm.Quantity;
                                    _currentViewModel.QuantityCommandParameter = "QuantityEdit";

                                    _currentViewModel.WindowHelper = new AddQuantityToNormWindow(_currentViewModel);
                                    _currentViewModel.WindowHelper.ShowDialog();

                                    var norm = _currentViewModel.Norma.FirstOrDefault(norm => norm.Item.Id == idItem);

                                    if (norm != null)
                                    {
                                        if (_currentViewModel.NormQuantity > 0)
                                        {
                                            norm.Quantity = _currentViewModel.NormQuantity;
                                            itemInNorm.Quantity = _currentViewModel.NormQuantity;

                                            _currentViewModel.DbContext.ItemsInNorm.Update(itemInNorm);
                                            _currentViewModel.DbContext.SaveChanges();
                                            _currentViewModel.NormQuantity = 0;
                                        }
                                        else
                                        {
                                            MessageBox.Show("KOLIČINA MORA BITI BROJ!", "", MessageBoxButton.OK, MessageBoxImage.Error);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                MessageBox.Show("Greška prilikom brisanja normativa iz artikla!", "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}