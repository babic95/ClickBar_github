using ClickBar.Models.AppMain.Statistic;
using ClickBar.Models.Sale;
using ClickBar.ViewModels;
using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar.Views.AppMain.AuxiliaryWindows.Statistic;
using ClickBar_Database;
using ClickBar_Database.Models;
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
    public class EditCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ViewModelBase _currentViewModel;

        public EditCommand(ViewModelBase currentViewModel)
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

                try
                {
                    var supplier = addEditSupplierViewModel.Suppliers.Where(supplier => supplier.Id == Convert.ToInt32(parameter)).FirstOrDefault();

                    if(supplier != null)
                    {
                        addEditSupplierViewModel.CurrentSupplier = supplier;

                        AddEditSupplierWindow addEditSupplierWindow = new AddEditSupplierWindow(addEditSupplierViewModel);
                        addEditSupplierWindow.ShowDialog();
                    }
                    else
                    {
                        MessageBox.Show("Ne postoji dobavljač!", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch
                {
                    MessageBox.Show("Greška prilikom učitavanja dobavljača!", "", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (_currentViewModel is InventoryStatusViewModel)
            {
                InventoryStatusViewModel inventoryStatusViewModel = (InventoryStatusViewModel)_currentViewModel;
                inventoryStatusViewModel.Norma = new ObservableCollection<Invertory>();
                inventoryStatusViewModel.Zelje = new ObservableCollection<ItemZelja>();
                inventoryStatusViewModel.CurrentNorm = -1;
                try
                {
                    var item = inventoryStatusViewModel.InventoryStatus.Where(item => item.Item.Id == parameter.ToString()).FirstOrDefault();

                    if(item != null)
                    {
                        var group = inventoryStatusViewModel.AllGroupItems.FirstOrDefault(group => group.Id == item.IdGroupItems);

                        if (group != null)
                        {
                            bool isSirovina = group.Name.ToLower().Contains("sirovina") || group.Name.ToLower().Contains("sirovine") ? true : false;

                            inventoryStatusViewModel.CurrentInventoryStatus = item;
                            inventoryStatusViewModel.CurrentGroupItems = group;

#if CRNO
                            inventoryStatusViewModel.EditItemIsReadOnly = false;
#else
                            inventoryStatusViewModel.EditItemIsReadOnly = true;
#endif
                            AddEditItemWindow addEditItemWindow = new AddEditItemWindow(inventoryStatusViewModel);

                            using (SqliteDbContext sqliteDbContext = new SqliteDbContext())
                            {

                                ItemDB? itemDB = sqliteDbContext.Items.Find(item.Item.Id);

                                if (itemDB != null)
                                {
                                    //var itemZelje = sqliteDbContext.Zelje.Where(x => x.ItemId == itemDB.Id);

                                    //if (itemZelje != null &&
                                    //    itemZelje.Any())
                                    //{
                                    //    itemZelje.ToList().ForEach(zelja =>
                                    //    {
                                    //        inventoryStatusViewModel.Zelje.Add(new ItemZelja()
                                    //        {
                                    //            Id = zelja.Id,
                                    //            ItemId = zelja.ItemId,
                                    //            Zelja = zelja.Zelja
                                    //        });
                                    //    });
                                    //}

                                    var itemInNorm = sqliteDbContext.ItemsInNorm.Where(x => x.IdNorm == itemDB.IdNorm);

                                    if (itemInNorm.Any())
                                    {
                                        itemInNorm.ToList().ForEach(item =>
                                        {
                                            ItemDB? itemDB = sqliteDbContext.Items.Find(item.IdItem);

                                            if (itemDB != null)
                                            {
                                                Item it = new Item(itemDB);
                                                Invertory invertory = new Invertory(it, itemDB.IdItemGroup, item.Quantity, 0, 0, isSirovina);
                                                inventoryStatusViewModel.Norma.Add(invertory);
                                            }
                                        });
                                    }
                                }
                            }

                            addEditItemWindow.ShowDialog();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Ne postoji artikal!", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch
                {
                    MessageBox.Show("Greška prilikom učitavanja artikla!", "", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}