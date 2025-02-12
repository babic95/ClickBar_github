using ClickBar.ViewModels.AppMain.Statistic;
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
    public class DeleteNormCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private InventoryStatusViewModel _currentViewModel;

        public DeleteNormCommand(InventoryStatusViewModel currentViewModel)
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

                    var result = MessageBox.Show("Da li zaista želite da obrišete normativ artikala?",
                        "Brisanje normativa artikala",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        if (string.IsNullOrEmpty(_currentViewModel.CurrentInventoryStatus.Item.Id))
                        {
                            var norm = _currentViewModel.Norma.FirstOrDefault(norm => norm.Item.Id == idItem);
                            if (norm != null)
                            {
                                _currentViewModel.Norma.Remove(norm);
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
                                    _currentViewModel.DbContext.ItemsInNorm.Remove(itemInNorm);
                                    _currentViewModel.DbContext.SaveChanges();

                                    var norm = _currentViewModel.Norma.FirstOrDefault(norm => norm.Item.Id == idItem);

                                    if (norm != null)
                                    {
                                        _currentViewModel.Norma.Remove(norm);
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