using ClickBar.Enums.AppMain.Statistic;
using ClickBar.Models.AppMain.Statistic;
using ClickBar.Models.AppMain.Statistic.Knjizenje;
using ClickBar.Models.Sale;
using ClickBar.ViewModels;
using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar.Views.AppMain.AuxiliaryWindows.Statistic.Nivelacija;
using ClickBar_Common.Models.Statistic;
using ClickBar_DatabaseSQLManager;
using ClickBar_DatabaseSQLManager.Models;
using ClickBar_Logging;
using ClickBar_Printer.PaperFormat;
using DocumentFormat.OpenXml.Drawing;
using Microsoft.EntityFrameworkCore;
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
    public class SaveCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ViewModelBase _currentViewModel;

        public SaveCommand(ViewModelBase currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if(_currentViewModel is AddEditSupplierViewModel)
            {
                AddEditSupplierViewModel addEditSupplierViewModel = (AddEditSupplierViewModel)_currentViewModel;
                if (addEditSupplierViewModel.CurrentSupplier.Id == null ||
                    addEditSupplierViewModel.CurrentSupplier.Id < 1)
                {
                    AddEditSupplier();
                }
                else
                {
                    AddEditSupplier(addEditSupplierViewModel.CurrentSupplier.Id);
                }
            }
            else if(_currentViewModel is InventoryStatusViewModel)
            {
                InventoryStatusViewModel inventoryStatusViewModel = (InventoryStatusViewModel)_currentViewModel;

                if (inventoryStatusViewModel.CurrentInventoryStatus == null ||
                    string.IsNullOrEmpty(inventoryStatusViewModel.CurrentInventoryStatus.Item.Id))
                {
                    AddEditItem();
                }
                else
                {
                    AddEditItem(inventoryStatusViewModel.CurrentInventoryStatus.Item.Id);
                }
            }
        }


        private void AddEditSupplier(int? id = null)
        {
            AddEditSupplierViewModel addEditSupplierViewModel = (AddEditSupplierViewModel)_currentViewModel;

            if (id == null)
            {
                SupplierDB supplier = new SupplierDB();
                try
                {
                    supplier.Name = addEditSupplierViewModel.CurrentSupplier.Name;
                    supplier.Pib = addEditSupplierViewModel.CurrentSupplier.Pib;
                    supplier.Mb = addEditSupplierViewModel.CurrentSupplier.MB;
                    supplier.Address = addEditSupplierViewModel.CurrentSupplier.Address;
                    supplier.City = addEditSupplierViewModel.CurrentSupplier.City;
                    supplier.ContractNumber = addEditSupplierViewModel.CurrentSupplier.ContractNumber;
                    supplier.Email = addEditSupplierViewModel.CurrentSupplier.Email;

                    addEditSupplierViewModel.DbContext.Add(supplier);
                    addEditSupplierViewModel.DbContext.SaveChanges();

                    MessageBox.Show("Uspešno ste dodali dobavljača!", "", MessageBoxButton.OK, MessageBoxImage.Information);

                    addEditSupplierViewModel.Window.Close();
                }
                catch
                {
                    MessageBox.Show("Greška prilikom dodavanja dobavljača!", "", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                var result = MessageBox.Show("Da li ste sigurni da želite da izmenite dobavljača?", "",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var supplier = addEditSupplierViewModel.DbContext.Suppliers.Find(id);

                        if (supplier != null)
                        {
                            supplier.Name = addEditSupplierViewModel.CurrentSupplier.Name;
                            supplier.Pib = addEditSupplierViewModel.CurrentSupplier.Pib;
                            supplier.Mb = addEditSupplierViewModel.CurrentSupplier.MB;
                            supplier.Address = addEditSupplierViewModel.CurrentSupplier.Address;
                            supplier.City = addEditSupplierViewModel.CurrentSupplier.City;
                            supplier.ContractNumber = addEditSupplierViewModel.CurrentSupplier.ContractNumber;
                            supplier.Email = addEditSupplierViewModel.CurrentSupplier.Email;

                            addEditSupplierViewModel.DbContext.Suppliers.Update(supplier);
                            addEditSupplierViewModel.DbContext.SaveChanges();

                            MessageBox.Show("Uspešno ste izmenili dobavljača!", "", MessageBoxButton.OK, MessageBoxImage.Information);

                            addEditSupplierViewModel.Window.Close();
                        }
                        else
                        {
                            MessageBox.Show("Ne postoji dobavljač!", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Greška prilikom izmene dobavljača!", "", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

            addEditSupplierViewModel.SuppliersAll = new List<Supplier>();
            addEditSupplierViewModel.DbContext.Suppliers.ToList().ForEach(x =>
            {
                addEditSupplierViewModel.SuppliersAll.Add(new Supplier(x));
            });

            addEditSupplierViewModel.Suppliers = new ObservableCollection<Supplier>(addEditSupplierViewModel.SuppliersAll);
            addEditSupplierViewModel.CurrentSupplier = new Supplier();
        }
    
        private async void AddEditItem(string? id = null)
        {
            InventoryStatusViewModel inventoryStatusViewModel = (InventoryStatusViewModel)_currentViewModel;

            if (inventoryStatusViewModel.CurrentGroupItems == null)
            {
                MessageBox.Show("Artikal mora da pripada nekoj grupi!", "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }

            if (string.IsNullOrEmpty(id))
            {
                var result = MessageBox.Show("Da li ste sigurni da želite da dodate artikal?", "Dodavanje artikla",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        int maxItemId = 1;
                        if (inventoryStatusViewModel.DbContext.Items != null && inventoryStatusViewModel.DbContext.Items.Any())
                        {
                            maxItemId = inventoryStatusViewModel.DbContext.Items.Select(x => Convert.ToInt32(x.Id)).ToList().Max();
                            maxItemId++;

                            while (inventoryStatusViewModel.DbContext.Items.Find(maxItemId.ToString("000000")) != null)
                            {
                                maxItemId++;
                            }
                        }

                        ItemDB itemDB = new ItemDB()
                        {
                            Id = maxItemId.ToString("000000"),
                            Name = inventoryStatusViewModel.CurrentInventoryStatus.Item.Name,
                            InputUnitPrice = 0,
                            SellingUnitPrice = inventoryStatusViewModel.CurrentInventoryStatus.Item.SellingUnitPrice,
                            SellingNocnaUnitPrice = inventoryStatusViewModel.CurrentInventoryStatus.Item.SellingNocnaUnitPrice,
                            SellingDnevnaUnitPrice = inventoryStatusViewModel.CurrentInventoryStatus.Item.SellingDnevnaUnitPrice,
                            Label = inventoryStatusViewModel.CurrentLabel.Id,
                            Jm = inventoryStatusViewModel.CurrentInventoryStatus.Item.Jm,
                            AlarmQuantity = inventoryStatusViewModel.CurrentInventoryStatus.Alarm,
                            TotalQuantity = 0,
                            IdItemGroup = inventoryStatusViewModel.CurrentGroupItems.Id,
                            IdNorm = inventoryStatusViewModel.CurrentNorm > 0 ? inventoryStatusViewModel.CurrentNorm : null,
                            DisableItem = inventoryStatusViewModel.CurrentInventoryStatus.Item.IsCheckedDesableItem ? 1 : 0,
                            IsCheckedZabraniPopust = inventoryStatusViewModel.CurrentInventoryStatus.Item.IsCheckedZabraniPopust ? 1 : 0,
                        };

                        if (inventoryStatusViewModel.DbContext.Items != null)
                        {
                            inventoryStatusViewModel.DbContext.Items.Add(itemDB);
                            inventoryStatusViewModel.DbContext.SaveChanges();

                            if (inventoryStatusViewModel.Zelje != null &&
                                inventoryStatusViewModel.Zelje.Any())
                            {
                                inventoryStatusViewModel.Zelje.ToList().ForEach(item =>
                                {
                                    ItemZeljaDB itemZeljaDB = new ItemZeljaDB()
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        ItemId = itemDB.Id,
                                        Zelja = item.Zelja,
                                    };
                                    inventoryStatusViewModel.DbContext.Zelje.Add(itemZeljaDB);
                                });
                                inventoryStatusViewModel.DbContext.SaveChanges();
                            }

                            if (inventoryStatusViewModel.Norma != null && inventoryStatusViewModel.Norma.Any())
                            {
                                inventoryStatusViewModel.Norma.ToList().ForEach(item =>
                                {
                                    var norms = inventoryStatusViewModel.DbContext.ItemsInNorm.Where(it => it.IdNorm == itemDB.IdNorm && it.IdItem == item.Item.Id);

                                    if (norms.Any())
                                    {
                                        ItemInNormDB? itemInNormDB = norms.FirstOrDefault();

                                        if (itemInNormDB != null)
                                        {
                                            itemInNormDB.Quantity = item.Quantity;

                                            inventoryStatusViewModel.DbContext.Update(itemInNormDB);
                                            inventoryStatusViewModel.DbContext.SaveChanges();
                                        }
                                    }
                                    else
                                    {
                                        ItemInNormDB itemInNormDB = new ItemInNormDB()
                                        {
                                            IdItem = item.Item.Id,
                                            IdNorm = itemDB.IdNorm.Value,
                                            Quantity = item.Quantity,
                                        };

                                        inventoryStatusViewModel.DbContext.ItemsInNorm.AddAsync(itemInNormDB);
                                        inventoryStatusViewModel.DbContext.SaveChanges();
                                    }
                                });

                                inventoryStatusViewModel.DbContext.Items.Update(itemDB);
                                inventoryStatusViewModel.DbContext.SaveChanges();
                            }

                            if (inventoryStatusViewModel.DbContext.Items != null &&
                            inventoryStatusViewModel.DbContext.Items.Any())
                            {
                                await inventoryStatusViewModel.DbContext.Items.Where(i => i.IdNorm != null)
                                    .ForEachAsync(itemDB =>
                                    {
                                        var itemInNorm = inventoryStatusViewModel.DbContext.ItemsInNorm.Where(i => i.IdNorm == itemDB.IdNorm);

                                        if (itemInNorm == null ||
                                        !itemInNorm.Any())
                                        {
                                            itemDB.IdNorm = null;
                                            inventoryStatusViewModel.DbContext.Items.Update(itemDB);
                                        }
                                    });
                                inventoryStatusViewModel.DbContext.SaveChanges();
                            }

                            inventoryStatusViewModel.InventoryStatusAll = new List<Invertory>();

                            await inventoryStatusViewModel.DbContext.Items.ForEachAsync(x =>
                            {
                                Item item = new Item(x);

                                var group = inventoryStatusViewModel.DbContext.ItemGroups.Find(x.IdItemGroup);

                                if (group != null)
                                {
                                    bool isSirovina = group.Name.ToLower().Contains("sirovina") || group.Name.ToLower().Contains("sirovine") ? true : false;

                                    inventoryStatusViewModel.InventoryStatusAll.Add(new Invertory(item, x.IdItemGroup, x.TotalQuantity, 0, x.AlarmQuantity, isSirovina));
                                }
                                else
                                {
                                    MessageBox.Show("Artikal ne pripada ni jednoj grupi!!!",
                                        "Greška",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Error);

                                    return;
                                }
                            });
                            if (inventoryStatusViewModel.CurrentGroup.Id == -1)
                            {
                                inventoryStatusViewModel.InventoryStatus = new ObservableCollection<Invertory>(inventoryStatusViewModel.InventoryStatusAll);
                            }
                            else
                            {
                                inventoryStatusViewModel.InventoryStatus = new ObservableCollection<Invertory>(inventoryStatusViewModel.InventoryStatusAll.Where(inventory =>
                                inventory.IdGroupItems == inventoryStatusViewModel.CurrentGroup.Id));
                            }
                            inventoryStatusViewModel.InventoryStatusNorm = new ObservableCollection<Invertory>(inventoryStatusViewModel.InventoryStatusAll);
                            inventoryStatusViewModel.Norma = new ObservableCollection<Invertory>();
                            inventoryStatusViewModel.Zelje = new ObservableCollection<ItemZelja>();
                            inventoryStatusViewModel.NormQuantity = 0;
                            inventoryStatusViewModel.VisibilityNext = Visibility.Hidden;
                            inventoryStatusViewModel.SearchItems = string.Empty;
                            inventoryStatusViewModel.CurrentInventoryStatusNorm = null;
                            if (inventoryStatusViewModel.WindowHelper != null)
                            {
                                inventoryStatusViewModel.WindowHelper.Close();
                            }

                            MessageBox.Show("Uspešno ste dodali artikal!", "", MessageBoxButton.OK, MessageBoxImage.Information);

                            inventoryStatusViewModel.Window.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Greška prilikom dodavanja novog artikla!", "Greška",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                } 
            }
            else
            {
                var result = MessageBox.Show("Da li ste sigurni da želite da izmenite artikal?", "Izmena artikla",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var itemDB = inventoryStatusViewModel.DbContext.Items.Find(inventoryStatusViewModel.CurrentInventoryStatus.Item.Id);

                        if (itemDB == null)
                        {
                            MessageBox.Show("GREŠKA U IZMENI ARTIKLA!", "", MessageBoxButton.OK, MessageBoxImage.Error);
                        }

                        if (inventoryStatusViewModel.CurrentNorm >= 0)
                        {
                            itemDB.IdNorm = inventoryStatusViewModel.CurrentNorm;
                        }

                        itemDB.Name = inventoryStatusViewModel.CurrentInventoryStatus.Item.Name;
                        itemDB.SellingUnitPrice = inventoryStatusViewModel.CurrentInventoryStatus.Item.SellingUnitPrice;
                        itemDB.SellingNocnaUnitPrice = inventoryStatusViewModel.CurrentInventoryStatus.Item.SellingNocnaUnitPrice;
                        itemDB.SellingDnevnaUnitPrice = inventoryStatusViewModel.CurrentInventoryStatus.Item.SellingDnevnaUnitPrice;
                        itemDB.Label = inventoryStatusViewModel.CurrentLabel.Id;
                        itemDB.Jm = inventoryStatusViewModel.CurrentInventoryStatus.Item.Jm;
                        itemDB.AlarmQuantity = inventoryStatusViewModel.CurrentInventoryStatus.Alarm;
                        itemDB.IdItemGroup = inventoryStatusViewModel.CurrentGroupItems.Id;
                        itemDB.DisableItem = inventoryStatusViewModel.CurrentInventoryStatus.Item.IsCheckedDesableItem ? 1 : 0;
                        itemDB.IsCheckedZabraniPopust = inventoryStatusViewModel.CurrentInventoryStatus.Item.IsCheckedZabraniPopust ? 1 : 0;

                        inventoryStatusViewModel.Zelje.ToList().ForEach(zelja =>
                        {
                            var zeljaDB = inventoryStatusViewModel.DbContext.Zelje.FirstOrDefault(z => z.Id == zelja.Id);

                            if (zeljaDB != null)
                            {
                                zeljaDB.Zelja = zelja.Zelja;
                                inventoryStatusViewModel.DbContext.Zelje.Update(zeljaDB);
                            }
                            else
                            {
                                ItemZeljaDB itemZeljaDB = new ItemZeljaDB()
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    ItemId = itemDB.Id,
                                    Zelja = zelja.Zelja,
                                };
                                inventoryStatusViewModel.DbContext.Zelje.Add(itemZeljaDB);
                            }
                        });
                        inventoryStatusViewModel.DbContext.SaveChanges();

                        inventoryStatusViewModel.Norma.ToList().ForEach(item =>
                        {
                            var norms = inventoryStatusViewModel.DbContext.ItemsInNorm.Where(it => it.IdNorm == itemDB.IdNorm && it.IdItem == item.Item.Id);

                            if (norms.Any())
                            {
                                ItemInNormDB? itemInNormDB = norms.FirstOrDefault();

                                if (itemInNormDB != null)
                                {
                                    itemInNormDB.Quantity = item.Quantity;

                                    inventoryStatusViewModel.DbContext.Update(itemInNormDB);
                                    inventoryStatusViewModel.DbContext.SaveChanges();
                                }
                            }
                            else
                            {
                                ItemInNormDB itemInNormDB = new ItemInNormDB()
                                {
                                    IdItem = item.Item.Id,
                                    IdNorm = itemDB.IdNorm.Value,
                                    Quantity = item.Quantity,
                                };

                                inventoryStatusViewModel.DbContext.ItemsInNorm.AddAsync(itemInNormDB);
                                inventoryStatusViewModel.DbContext.SaveChanges();
                            }
                        });

                        if (inventoryStatusViewModel.DbContext.Items != null &&
                            inventoryStatusViewModel.DbContext.Items.Any())
                        {
                            await inventoryStatusViewModel.DbContext.Items.Where(i => i.IdNorm != null)
                                .ForEachAsync(itemDB =>
                                {
                                    var itemInNorm = inventoryStatusViewModel.DbContext.ItemsInNorm.Where(i => i.IdNorm == itemDB.IdNorm);

                                    if (itemInNorm == null ||
                                    !itemInNorm.Any())
                                    {
                                        itemDB.IdNorm = null;
                                        inventoryStatusViewModel.DbContext.Items.Update(itemDB);
                                    }
                                });
                            inventoryStatusViewModel.DbContext.SaveChanges();
                        }

                        inventoryStatusViewModel.InventoryStatusAll = new List<Invertory>();
                        await inventoryStatusViewModel.DbContext.Items.ForEachAsync(x =>
                        {
                            Item item = new Item(x);

                            var group = inventoryStatusViewModel.DbContext.ItemGroups.Find(x.IdItemGroup);

                            if (group != null)
                            {
                                bool isSirovina = group.Name.ToLower().Contains("sirovina") || group.Name.ToLower().Contains("sirovine") ? true : false;
                                inventoryStatusViewModel.InventoryStatusAll.Add(new Invertory(item, x.IdItemGroup, x.TotalQuantity, 0, x.AlarmQuantity, isSirovina));
                            }
                            else
                            {
                                MessageBox.Show("Artikal ne pripada ni jednoj grupi!!!",
                                    "Greška",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);

                                return;
                            }
                        });
                        if (inventoryStatusViewModel.CurrentGroup.Id == -1)
                        {
                            inventoryStatusViewModel.InventoryStatus = new ObservableCollection<Invertory>(inventoryStatusViewModel.InventoryStatusAll);
                        }
                        else
                        {
                            inventoryStatusViewModel.InventoryStatus = new ObservableCollection<Invertory>(inventoryStatusViewModel.InventoryStatusAll.Where(inventory =>
                            inventory.IdGroupItems == inventoryStatusViewModel.CurrentGroup.Id));
                        }
                        inventoryStatusViewModel.InventoryStatusNorm = new ObservableCollection<Invertory>(inventoryStatusViewModel.InventoryStatusAll);
                        inventoryStatusViewModel.Norma = new ObservableCollection<Invertory>();
                        inventoryStatusViewModel.Zelje = new ObservableCollection<ItemZelja>();
                        inventoryStatusViewModel.NormQuantity = 0;
                        inventoryStatusViewModel.VisibilityNext = Visibility.Hidden;
                        inventoryStatusViewModel.SearchItems = string.Empty;
                        inventoryStatusViewModel.CurrentInventoryStatusNorm = null;
                        if (inventoryStatusViewModel.WindowHelper != null)
                        {
                            inventoryStatusViewModel.WindowHelper.Close();
                        }

                        inventoryStatusViewModel.DbContext.Items.Update(itemDB);
                        inventoryStatusViewModel.DbContext.SaveChanges();

                        MessageBox.Show("Uspešno ste izmenili artikal!", "", MessageBoxButton.OK, MessageBoxImage.Information);

                        inventoryStatusViewModel.Window.Close();
                    }
                    catch
                    {
                        MessageBox.Show("Greška prilikom izmene artikla!", "", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}