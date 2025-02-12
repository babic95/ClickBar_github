using ClickBar.Models.AppMain.Statistic;
using ClickBar.Models.Sale;
using ClickBar.ViewModels;
using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar_DatabaseSQLManager;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ClickBar.Commands.AppMain.Statistic
{
    public class DeleteCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ViewModelBase _currentViewModel;

        public DeleteCommand(ViewModelBase currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (_currentViewModel is AddEditSupplierViewModel)
            {
                AddEditSupplierViewModel addEditSupplierViewModel = (AddEditSupplierViewModel)_currentViewModel;

                var result = MessageBox.Show("Da li ste sigurni da želite da obrišete dobavljača?", "", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var supplier = addEditSupplierViewModel.DbContext.Suppliers.Find(Convert.ToInt32(parameter));

                        if (supplier != null)
                        {
                            addEditSupplierViewModel.DbContext.Suppliers.Remove(supplier);
                            addEditSupplierViewModel.DbContext.SaveChanges();

                            addEditSupplierViewModel.SuppliersAll = new List<Supplier>();
                            addEditSupplierViewModel.DbContext.Suppliers.ToList().ForEach(x =>
                            {
                                addEditSupplierViewModel.SuppliersAll.Add(new Supplier(x));
                            });

                            addEditSupplierViewModel.Suppliers = new ObservableCollection<Supplier>(addEditSupplierViewModel.SuppliersAll);
                            addEditSupplierViewModel.CurrentSupplier = new Supplier();

                            MessageBox.Show("Uspešno ste obrisali dobavljača!", "", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Ne postoji dobavljač!", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Greška prilikom brisanja dobavljača!", "", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else if (_currentViewModel is InventoryStatusViewModel)
            {
                InventoryStatusViewModel inventoryStatusViewModel = (InventoryStatusViewModel)_currentViewModel;

                var result = MessageBox.Show("Da li ste sigurni da želite da obrišete Artikal?", "", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var item = inventoryStatusViewModel.DbContext.Items.Find(parameter.ToString());

                        if (item != null)
                        {
                            inventoryStatusViewModel.DbContext.Items.Remove(item);
                            inventoryStatusViewModel.DbContext.SaveChanges();

                            inventoryStatusViewModel.InventoryStatusAll = new List<Invertory>();
                            inventoryStatusViewModel.DbContext.Items.ToList().ForEach(x =>
                            {
                                Item item = new Item(x);
                                var group = inventoryStatusViewModel.DbContext.ItemGroups.Find(x.IdItemGroup);

                                if (group != null)
                                {
                                    bool isSirovina = group.Name.ToLower().Contains("sirovina") || group.Name.ToLower().Contains("sirovine") ? true : false;
                                    inventoryStatusViewModel.InventoryStatusAll.Add(new Invertory(item, x.IdItemGroup, x.TotalQuantity, 0, x.AlarmQuantity, isSirovina));
                                }
                            });
                            inventoryStatusViewModel.InventoryStatus = new ObservableCollection<Invertory>(inventoryStatusViewModel.InventoryStatusAll);
                            inventoryStatusViewModel.InventoryStatusNorm = new ObservableCollection<Invertory>(inventoryStatusViewModel.InventoryStatusAll);
                            inventoryStatusViewModel.Norma = new ObservableCollection<Invertory>();
                            inventoryStatusViewModel.NormQuantity = 0;
                            inventoryStatusViewModel.VisibilityNext = Visibility.Hidden;
                            inventoryStatusViewModel.SearchItems = string.Empty;
                            inventoryStatusViewModel.CurrentInventoryStatusNorm = null;
                            if (inventoryStatusViewModel.WindowHelper != null)
                            {
                                inventoryStatusViewModel.WindowHelper.Close();
                            }

                            MessageBox.Show("Uspešno ste obrisali artikal!", "", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Ne postoji artikal!", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Greška prilikom brisanja artikla!", "", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}