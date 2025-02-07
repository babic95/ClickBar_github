using ClickBar.Converters;
using ClickBar.Enums.AppMain.Statistic;
using ClickBar.Models.AppMain.Statistic;
using ClickBar.Models.AppMain.Statistic.Knjizenje;
using ClickBar.Models.Sale;
using ClickBar.ViewModels;
using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar.Views.AppMain.AuxiliaryWindows.Statistic.Nivelacija;
using ClickBar_Common.Enums;
using ClickBar_Common.Models.Statistic;
using ClickBar_Database;
using ClickBar_Database.Models;
using ClickBar_Logging;
using ClickBar_Printer;
using ClickBar_Printer.PaperFormat;
using DocumentFormat.OpenXml.Drawing;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ClickBar.Commands.AppMain.Statistic.Calculation
{
    public class SaveCalculationCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ViewModelBase _currentViewModel;

        public SaveCalculationCommand(ViewModelBase currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            if (_currentViewModel is CalculationViewModel ||
                _currentViewModel is ViewCalculationViewModel)
            {
                Calculate();
            }
        }
        private async void Calculate()
        {
            if (_currentViewModel is CalculationViewModel)
            {
                CalculationViewModel calculationViewModel = (CalculationViewModel)_currentViewModel;

                var result = MessageBox.Show("Da li ste sigurni da želite da uradite kalkulaciju?", "",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        bool onlySirovine = true;
                        using (SqliteDbContext sqliteDbContext = new SqliteDbContext())
                        {

                            var cas = sqliteDbContext.Cashiers.Find(calculationViewModel.LoggedCashier.Id);
                            var sup = sqliteDbContext.Suppliers.Find(calculationViewModel.SelectedSupplier.Id);

                            var allNivelacijeInYear = sqliteDbContext.Calculations.Where(cal => cal.CalculationDate.Year == calculationViewModel.CalculationDate.Year);

                            int counterCalculation = 1;

                            if (allNivelacijeInYear != null &&
                                allNivelacijeInYear.Any())
                            {
                                var maxNivelacija = allNivelacijeInYear.Max(nivelacija => nivelacija.Counter);

                                if (maxNivelacija > 0)
                                {
                                    counterCalculation = maxNivelacija + 1;
                                }
                            }
                            var nivelacija = new Models.AppMain.Statistic.Nivelacija(sqliteDbContext, NivelacijaStateEnumeration.Kalkulacija, calculationViewModel.CalculationDate.AddSeconds(-1));

                            CalculationDB calculationDB = new CalculationDB()
                            {
                                Id = Guid.NewGuid().ToString(),
                                CashierId = calculationViewModel.LoggedCashier.Id,
                                SupplierId = calculationViewModel.SelectedSupplier.Id,
                                CalculationDate = calculationViewModel.CalculationDate,
                                InputTotalPrice = 0,
                                OutputTotalPrice = 0,
                                InvoiceNumber = calculationViewModel.InvoiceNumber,
                                Counter = counterCalculation,
                                //Cashier = cas,
                                //Supplier = sup
                            };
                            sqliteDbContext.Calculations.Add(calculationDB);
                            RetryHelper.ExecuteWithRetry(() => { sqliteDbContext.SaveChanges(); });

                            List<InvertoryGlobal> invertoryGlobals = new List<InvertoryGlobal>();

                            bool nivelacijaIsAdded = false;

                            NivelacijaDB? nivelacijaDB = null;
                            decimal nivelacijaTotal = 0;

                            calculationViewModel.Calculations.ToList().ForEach(item =>
                            {
                                var itemDB = sqliteDbContext.Items.Find(item.Item.Id);

                                if (itemDB != null)
                                {
                                    var group = sqliteDbContext.ItemGroups.Find(itemDB.IdItemGroup);

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
                                        if (calculationViewModel.CalculationDate.Date < DateTime.Now.Date)
                                        {
                                            StarijaKalkulacija(sqliteDbContext,
                                                calculationViewModel.CalculationDate,
                                                item,
                                                itemDB,
                                                true,
                                                null);
                                        }
                                        else
                                        {
                                            Kalkulacija(sqliteDbContext, item, itemDB);
                                        }
                                        item.Item.InputUnitPrice = itemDB.InputUnitPrice;

                                        invertoryGlobals.Add(new InvertoryGlobal()
                                        {
                                            Id = item.Item.Id,
                                            Name = item.Item.Name,
                                            Jm = item.Item.Jm,
                                            InputUnitPrice = Decimal.Round(item.InputPrice / item.Quantity, 2),
                                            Quantity = item.Quantity,
                                            TotalAmout = item.InputPrice
                                        });

                                        CalculationItemDB calculationItemDB = new CalculationItemDB()
                                        {
                                            CalculationId = calculationDB.Id,
                                            ItemId = itemDB.Id,
                                            InputPrice = Decimal.Round(item.InputPrice / item.Quantity, 2),
                                            OutputPrice = Decimal.Round(item.InputPrice / item.Quantity, 2),
                                            Quantity = item.Quantity
                                        };
                                        sqliteDbContext.CalculationItems.Add(calculationItemDB);

                                        calculationDB.InputTotalPrice += item.InputPrice;
                                        sqliteDbContext.Calculations.Update(calculationDB);
                                    }
                                    else
                                    {
                                        onlySirovine = false;

                                        if (calculationViewModel.CalculationDate.Date < DateTime.Now.Date)
                                        {
                                            StarijaKalkulacija(sqliteDbContext,
                                                calculationViewModel.CalculationDate,
                                                item,
                                                itemDB,
                                                false,
                                                null);
                                        }
                                        else
                                        {
                                            Kalkulacija(sqliteDbContext, item, itemDB);
                                        }

                                        if (itemDB.SellingUnitPrice != item.Item.SellingUnitPrice)
                                        {
                                            if (!nivelacijaIsAdded)
                                            {
                                                nivelacijaIsAdded = true;

                                                nivelacijaDB = new NivelacijaDB()
                                                {
                                                    Id = nivelacija.Id,
                                                    Counter = nivelacija.CounterNivelacije,
                                                    DateNivelacije = nivelacija.NivelacijaDate,
                                                    Description = nivelacija.Description,
                                                    Type = (int)nivelacija.Type
                                                };
                                                sqliteDbContext.Nivelacijas.Add(nivelacijaDB);
                                                RetryHelper.ExecuteWithRetry(() => { sqliteDbContext.SaveChanges(); });

                                                Log.Debug($"SaveCommand - Kalkulacija - Uspesno sacuvana nivelacija {nivelacija.Id}");
                                            }

                                            AddNivelacijaItem(sqliteDbContext, item.Item, itemDB, nivelacija, ref nivelacijaTotal);
                                        }

                                        item.Item.InputUnitPrice = itemDB.InputUnitPrice;

                                        invertoryGlobals.Add(new InvertoryGlobal()
                                        {
                                            Id = item.Item.Id,
                                            Name = item.Item.Name,
                                            Jm = item.Item.Jm,
                                            SellingUnitPrice = item.Item.SellingUnitPrice,

                                            InputUnitPrice = Decimal.Round(item.InputPrice / item.Quantity, 2),

                                            Quantity = item.Quantity,
                                            TotalAmout = item.Item.SellingUnitPrice * item.Quantity
                                        });

                                        CalculationItemDB calculationItemDB = new CalculationItemDB()
                                        {
                                            CalculationId = calculationDB.Id,
                                            ItemId = itemDB.Id,
                                            InputPrice = Decimal.Round(item.InputPrice / item.Quantity, 2),
                                            OutputPrice = item.Item.SellingUnitPrice,
                                            Quantity = item.Quantity
                                        };
                                        sqliteDbContext.CalculationItems.Add(calculationItemDB);

                                        calculationDB.OutputTotalPrice += item.Item.SellingUnitPrice * item.Quantity;
                                        calculationDB.InputTotalPrice += item.InputPrice;
                                        sqliteDbContext.Calculations.Update(calculationDB);
                                    }

                                    itemDB.TotalQuantity += item.Quantity;
                                    sqliteDbContext.Update(itemDB);
                                    RetryHelper.ExecuteWithRetry(() => { sqliteDbContext.SaveChanges(); });
                                }
                            });


                            await KnjizenjePazaraKEP(sqliteDbContext, calculationDB);

                            SupplierGlobal supplierGlobal = new SupplierGlobal()
                            {
                                Name = calculationViewModel.SelectedSupplier.Name,
                                Pib = calculationViewModel.SelectedSupplier.Pib,
                                Address = calculationViewModel.SelectedSupplier.Address,
                                City = calculationViewModel.SelectedSupplier.City,
                                ContractNumber = calculationViewModel.SelectedSupplier.ContractNumber,
                                Email = calculationViewModel.SelectedSupplier.Email,
                                Mb = calculationViewModel.SelectedSupplier.MB,
                                InvoiceNumber = calculationViewModel.InvoiceNumber
                            };

                            if (!onlySirovine)
                            {
                                if (nivelacijaDB != null)
                                {
                                    KepDB kepNivelacijaDB = new KepDB()
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        KepDate = nivelacijaDB.DateNivelacije,
                                        Type = (int)KepStateEnumeration.Nivelacija,
                                        Razduzenje = 0,
                                        Zaduzenje = nivelacijaTotal,
                                        Description = $"Nivelacija 'Nivelacija_{nivelacijaDB.Counter}-{nivelacijaDB.DateNivelacije.Year}' po kalkulaciji 'Kalkulacija_{calculationDB.Counter}-{calculationDB.CalculationDate.Year}'"
                                    };
                                    sqliteDbContext.Kep.Add(kepNivelacijaDB);
                                    RetryHelper.ExecuteWithRetry(() => { sqliteDbContext.SaveChanges(); });
                                }

                                KepDB kepCalculationDB = new KepDB()
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    KepDate = calculationDB.CalculationDate,
                                    Type = (int)KepStateEnumeration.Kalkulacija,
                                    Razduzenje = 0,
                                    Zaduzenje = calculationDB.OutputTotalPrice,
                                    Description = $"Ručna kalkulacija 'Kalkulacija_{calculationDB.Counter}-{calculationDB.CalculationDate.Year}'"
                                };
                                sqliteDbContext.Kep.Add(kepCalculationDB);

                            }

                            RetryHelper.ExecuteWithRetry(() => { sqliteDbContext.SaveChanges(); });
                            PrinterManager.Instance.PrintInventoryStatus(invertoryGlobals, $"KALKULACIJA_{calculationDB.Counter}-{calculationDB.CalculationDate.Year}", calculationDB.CalculationDate, supplierGlobal);
                            calculationViewModel.SuppliersAll = new List<Supplier>();
                            calculationViewModel.InventoryStatusAll = new List<Invertory>();
                            sqliteDbContext.Suppliers.ToList().ForEach(x =>
                            {
                                calculationViewModel.SuppliersAll.Add(new Supplier(x));
                            });
                            sqliteDbContext.Items.ToList().ForEach(x =>
                            {
                                Item item = new Item(x);

                                var group = sqliteDbContext.ItemGroups.Find(x.IdItemGroup);

                                if (group != null)
                                {
                                    bool isSirovina = group.Name.ToLower().Contains("sirovina") || group.Name.ToLower().Contains("sirovine") ? true : false;
                                    calculationViewModel.InventoryStatusAll.Add(new Invertory(item, x.IdItemGroup, x.TotalQuantity, 0, x.AlarmQuantity, isSirovina));
                                }
                                else
                                {
                                    MessageBox.Show("ARTIKAL MORA DA PRIPADA NEKOJ GRUPI!",
                                        "Greška",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Error);

                                    return;
                                }
                            });

                            calculationViewModel.Suppliers = new ObservableCollection<Supplier>(calculationViewModel.SuppliersAll);
                            calculationViewModel.InventoryStatusCalculation = new ObservableCollection<Invertory>(calculationViewModel.InventoryStatusAll);
                            calculationViewModel.Calculations = new ObservableCollection<Invertory>();
                            calculationViewModel.CalculationQuantityString = "0";
                            calculationViewModel.CalculationPriceString = "0";
                            calculationViewModel.TotalCalculation = 0;
                            calculationViewModel.InvoiceNumber = string.Empty;
                            calculationViewModel.VisibilityNext = Visibility.Hidden;
                            calculationViewModel.SearchText = string.Empty;
                            calculationViewModel.CurrentGroup = calculationViewModel.AllGroups.FirstOrDefault();
                            calculationViewModel.CurrentInventoryStatusCalculation = null;

                            MessageBox.Show("Uspešno ste izvrsili kalkulaciju!", "", MessageBoxButton.OK, MessageBoxImage.Information);

                            calculationViewModel.Window.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error("SaveCalculationCommand -> Greska prilikom kreiranja nove kalkulacije ->", ex);
                        MessageBox.Show("Greška prilikom kreiranja kalkulacije!", "Greška - kalkulacija", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else if (_currentViewModel is ViewCalculationViewModel)
            {
                ViewCalculationViewModel viewCalculationViewModel = (ViewCalculationViewModel)_currentViewModel;

                var result = MessageBox.Show("Da li ste sigurni da želite da izmenite kalkulaciju?", "Izmena kalkulacije",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        if(viewCalculationViewModel.CurrentCalculation.Supplier == null ||
                            viewCalculationViewModel.CurrentCalculation.Supplier.Id < 0)
                        {
                            MessageBox.Show("Morate izabrati dobavljača!",
                                "Greška",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                            return;
                        }

                        if (viewCalculationViewModel.CurrentCalculation != null)
                        {
                            using (SqliteDbContext sqliteDbContext = new SqliteDbContext())
                            {
                                var calculationDB = sqliteDbContext.Calculations.Find(viewCalculationViewModel.CurrentCalculation.Id);

                                if (calculationDB != null)
                                {
                                    calculationDB.CalculationDate = viewCalculationViewModel.CurrentCalculation.CalculationDate;
                                    calculationDB.InvoiceNumber = viewCalculationViewModel.CurrentCalculation.InvoiceNumber;
                                    calculationDB.SupplierId = viewCalculationViewModel.CurrentCalculation.Supplier.Id;
                                    calculationDB.InputTotalPrice = 0;
                                    calculationDB.OutputTotalPrice = 0;
                                    bool onlySirovine = true;
                                    List<InvertoryGlobal> invertoryGlobals = new List<InvertoryGlobal>();

                                    var itemsInCalculation = sqliteDbContext.CalculationItems.Where(item => item.CalculationId == calculationDB.Id);

                                    if (itemsInCalculation != null &&
                                        itemsInCalculation.Any())
                                    {
                                        await itemsInCalculation.ForEachAsync(i =>
                                        {
                                            var item = viewCalculationViewModel.CurrentCalculation.CalculationItems.FirstOrDefault(it => it.Item.Id == i.ItemId);

                                            if (item == null)
                                            {
                                                sqliteDbContext.CalculationItems.Remove(i);
                                            }
                                        });
                                        RetryHelper.ExecuteWithRetry(() => { sqliteDbContext.SaveChanges(); });
                                    }

                                    viewCalculationViewModel.CurrentCalculation.CalculationItems.ToList().ForEach(item =>
                                    {
                                        var itemDB = sqliteDbContext.Items.Find(item.Item.Id);

                                        if (itemDB != null)
                                        {
                                            var group = sqliteDbContext.ItemGroups.Find(itemDB.IdItemGroup);

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

                                                if (viewCalculationViewModel.CurrentCalculation.CalculationDate.Date < DateTime.Now.Date)
                                                {
                                                    StarijaKalkulacija(sqliteDbContext,
                                                        viewCalculationViewModel.CurrentCalculation.CalculationDate,
                                                        item,
                                                        itemDB,
                                                        true,
                                                        calculationDB);
                                                }
                                                else
                                                {
                                                    Kalkulacija(sqliteDbContext, item, itemDB);
                                                }

                                                if (itemsInCalculation != null &&
                                                itemsInCalculation.Any())
                                                {
                                                    var itemInCal = itemsInCalculation.FirstOrDefault(i => i.ItemId == itemDB.Id);

                                                    if (itemInCal != null)
                                                    {
                                                        itemDB.TotalQuantity -= itemInCal.Quantity;
                                                        sqliteDbContext.Items.Update(itemDB);
                                                        sqliteDbContext.CalculationItems.Remove(itemInCal);
                                                        itemsInCalculation.ToList().Remove(itemInCal);
                                                    }
                                                }

                                                item.Item.InputUnitPrice = itemDB.InputUnitPrice;
                                                invertoryGlobals.Add(new InvertoryGlobal()
                                                {
                                                    Id = item.Item.Id,
                                                    Name = item.Item.Name,
                                                    Jm = item.Item.Jm,
                                                    InputUnitPrice = Decimal.Round(item.InputPrice / item.Quantity, 2),
                                                    Quantity = item.Quantity,
                                                    TotalAmout = item.InputPrice,
                                                    SellingUnitPrice = 0,
                                                });

                                                CalculationItemDB calculationItemDB = new CalculationItemDB()
                                                {
                                                    CalculationId = calculationDB.Id,
                                                    ItemId = itemDB.Id,
                                                    InputPrice = Decimal.Round(item.InputPrice / item.Quantity, 2),
                                                    OutputPrice = Decimal.Round(item.InputPrice / item.Quantity, 2),
                                                    Quantity = item.Quantity,
                                                };
                                                sqliteDbContext.CalculationItems.Add(calculationItemDB);

                                                calculationDB.InputTotalPrice += item.InputPrice;
                                                sqliteDbContext.Calculations.Update(calculationDB);
                                            }
                                            else
                                            {
                                                onlySirovine = false;

                                                if (viewCalculationViewModel.CurrentCalculation.CalculationDate.Date < DateTime.Now.Date)
                                                {
                                                    StarijaKalkulacija(sqliteDbContext,
                                                        viewCalculationViewModel.CurrentCalculation.CalculationDate,
                                                        item,
                                                        itemDB,
                                                        false,
                                                        calculationDB);
                                                }
                                                else
                                                {
                                                    Kalkulacija(sqliteDbContext, item, itemDB);
                                                }

                                                if (itemsInCalculation != null &&
                                                itemsInCalculation.Any())
                                                {
                                                    var itemInCal = itemsInCalculation.FirstOrDefault(i => i.ItemId == itemDB.Id);

                                                    if (itemInCal != null)
                                                    {
                                                        itemDB.TotalQuantity -= itemInCal.Quantity;
                                                        sqliteDbContext.Items.Update(itemDB);
                                                        sqliteDbContext.CalculationItems.Remove(itemInCal);
                                                        itemsInCalculation.ToList().Remove(itemInCal);
                                                    }
                                                }
                                                item.Item.InputUnitPrice = itemDB.InputUnitPrice;

                                                invertoryGlobals.Add(new InvertoryGlobal()
                                                {
                                                    Id = item.Item.Id,
                                                    Name = item.Item.Name,
                                                    Jm = item.Item.Jm,
                                                    SellingUnitPrice = item.Item.SellingUnitPrice,

                                                    InputUnitPrice = Decimal.Round(item.InputPrice / item.Quantity, 2),

                                                    Quantity = item.Quantity,
                                                    TotalAmout = Decimal.Round(item.Item.SellingUnitPrice * item.Quantity, 2)
                                                });

                                                CalculationItemDB calculationItemDB = new CalculationItemDB()
                                                {
                                                    CalculationId = calculationDB.Id,
                                                    ItemId = itemDB.Id,
                                                    InputPrice = Decimal.Round(item.InputPrice / item.Quantity, 2),
                                                    OutputPrice = item.Item.SellingUnitPrice,
                                                    Quantity = item.Quantity
                                                };
                                                sqliteDbContext.CalculationItems.Add(calculationItemDB);

                                                calculationDB.OutputTotalPrice += item.Item.SellingUnitPrice * item.Quantity;
                                                calculationDB.InputTotalPrice += item.InputPrice;
                                                sqliteDbContext.Calculations.Update(calculationDB);
                                            }

                                            itemDB.TotalQuantity += item.Quantity;
                                            sqliteDbContext.Update(itemDB);
                                            RetryHelper.ExecuteWithRetry(() => { sqliteDbContext.SaveChanges(); });
                                        }
                                    });

                                    await KnjizenjePazaraKEP(sqliteDbContext, calculationDB);

                                    SupplierGlobal supplierGlobal = new SupplierGlobal()
                                    {
                                        Name = viewCalculationViewModel.CurrentCalculation.Supplier.Name,
                                        Pib = viewCalculationViewModel.CurrentCalculation.Supplier.Pib,
                                        Address = viewCalculationViewModel.CurrentCalculation.Supplier.Address,
                                        City = viewCalculationViewModel.CurrentCalculation.Supplier.City,
                                        ContractNumber = viewCalculationViewModel.CurrentCalculation.Supplier.ContractNumber,
                                        Email = viewCalculationViewModel.CurrentCalculation.Supplier.Email,
                                        Mb = viewCalculationViewModel.CurrentCalculation.Supplier.MB,
                                        InvoiceNumber = viewCalculationViewModel.CurrentCalculation.InvoiceNumber
                                    };

                                    KepDB? kepCalculationDB = sqliteDbContext.Kep.FirstOrDefault(kep =>
                                    kep.Description.ToLower().Contains($"kalkulacija_{viewCalculationViewModel.CurrentCalculation.Counter}-{viewCalculationViewModel.CurrentCalculation.CalculationDate.Year}"));

                                    if (kepCalculationDB != null)
                                    {
                                        if (!onlySirovine)
                                        {
                                            kepCalculationDB.Zaduzenje = calculationDB.OutputTotalPrice;
                                            kepCalculationDB.KepDate = calculationDB.CalculationDate;
                                            sqliteDbContext.Kep.Update(kepCalculationDB);
                                        }
                                        else
                                        {
                                            sqliteDbContext.Kep.Remove(kepCalculationDB);
                                        }
                                    }
                                    else
                                    {
                                        if (!onlySirovine)
                                        {
                                            kepCalculationDB = new KepDB()
                                            {
                                                Id = Guid.NewGuid().ToString(),
                                                KepDate = calculationDB.CalculationDate,
                                                Type = (int)KepStateEnumeration.Kalkulacija,
                                                Razduzenje = 0,
                                                Zaduzenje = calculationDB.OutputTotalPrice,
                                                Description = $"Ručna kalkulacija 'Kalkulacija_{calculationDB.Counter}-{calculationDB.CalculationDate.Year}'"
                                            };
                                            sqliteDbContext.Kep.Add(kepCalculationDB);
                                        }
                                    }
                                    RetryHelper.ExecuteWithRetry(() => { sqliteDbContext.SaveChanges(); });

                                    PrinterManager.Instance.PrintInventoryStatus(invertoryGlobals, $"KALKULACIJA_{calculationDB.Counter}-{calculationDB.CalculationDate.Year}", calculationDB.CalculationDate, supplierGlobal);

                                    viewCalculationViewModel.CalculationsAll = new ObservableCollection<Models.AppMain.Statistic.Calculation>();
                                    viewCalculationViewModel.Calculations = new ObservableCollection<Models.AppMain.Statistic.Calculation>();

                                    var calculations = sqliteDbContext.Calculations.Where(cal => viewCalculationViewModel.SearchFromCalculationDate.HasValue &&
                                    cal.CalculationDate.Date >= viewCalculationViewModel.SearchFromCalculationDate.Value.Date &&
                                    viewCalculationViewModel.SearchToCalculationDate.HasValue &&
                                    cal.CalculationDate.Date <= viewCalculationViewModel.SearchToCalculationDate.Value.Date)
                                    .Join(sqliteDbContext.Suppliers,
                                    cal => cal.SupplierId,
                                    supp => supp.Id,
                                    (cal, supp) => new { Cal = cal, Supp = supp })
                                    .Join(sqliteDbContext.Cashiers,
                                    cal => cal.Cal.CashierId,
                                    cash => cash.Id,
                                    (cal, cash) => new { Cal = cal, Cash = cash });

                                    if (calculations != null &&
                                        calculations.Any())
                                    {
                                        await calculations.ForEachAsync(cal =>
                                        {
                                            Models.AppMain.Statistic.Calculation calculation = new Models.AppMain.Statistic.Calculation()
                                            {
                                                Id = cal.Cal.Cal.Id,
                                                CalculationDate = cal.Cal.Cal.CalculationDate,
                                                InputTotalPrice = cal.Cal.Cal.InputTotalPrice,
                                                InvoiceNumber = cal.Cal.Cal.InvoiceNumber,
                                                OutputTotalPrice = cal.Cal.Cal.OutputTotalPrice,
                                                Counter = cal.Cal.Cal.Counter,
                                                Name = $"Kalkulacija_{cal.Cal.Cal.Counter}-{cal.Cal.Cal.CalculationDate.Year}",
                                                Supplier = cal.Cal.Supp == null ? null : new Supplier(cal.Cal.Supp),
                                                Cashier = cal.Cash
                                            };

                                            viewCalculationViewModel.CalculationsAll.Add(calculation);
                                        });
                                    }
                                    viewCalculationViewModel.Calculations = new ObservableCollection<Models.AppMain.Statistic.Calculation>(viewCalculationViewModel.CalculationsAll.OrderBy(cal =>
                                    cal.CalculationDate));

                                    viewCalculationViewModel.InventoryStatusAll = new List<Invertory>();
                                    sqliteDbContext.Items.ToList().ForEach(x =>
                                    {
                                        Item item = new Item(x);

                                        var group = sqliteDbContext.ItemGroups.Find(x.IdItemGroup);

                                        if (group != null)
                                        {
                                            bool isSirovina = group.Name.ToLower().Contains("sirovina") || group.Name.ToLower().Contains("sirovine") ? true : false;
                                            viewCalculationViewModel.InventoryStatusAll.Add(new Invertory(item, x.IdItemGroup, x.TotalQuantity, 0, x.AlarmQuantity, isSirovina));
                                        }
                                        else
                                        {
                                            MessageBox.Show("ARTIKAL MORA DA PRIPADA NEKOJ GRUPI!",
                                                "Greška",
                                                MessageBoxButton.OK,
                                                MessageBoxImage.Error);

                                            return;
                                        }
                                    });

                                    viewCalculationViewModel.InventoryStatusCalculation = new ObservableCollection<Invertory>(viewCalculationViewModel.InventoryStatusAll);
                                    viewCalculationViewModel.CalculationQuantityString = "0";
                                    viewCalculationViewModel.CalculationPriceString = "0";
                                    //viewCalculationViewModel.VisibilityNext = Visibility.Hidden;
                                    viewCalculationViewModel.SearchText = string.Empty;
                                    viewCalculationViewModel.CurrentGroup = viewCalculationViewModel.AllGroups.FirstOrDefault();
                                    viewCalculationViewModel.CurrentInventoryStatusCalculation = null;
                                    MessageBox.Show("Uspešno ste izmenili kalkulaciju!", "Uspešna kalkulacija", MessageBoxButton.OK, MessageBoxImage.Information);

                                    if (viewCalculationViewModel.EditWindow != null)
                                    {
                                        viewCalculationViewModel.EditWindow.Close();
                                    }

                                    if (viewCalculationViewModel.CurrentWindow != null)
                                    {
                                        viewCalculationViewModel.CurrentWindow.Close();
                                    }

                                    if (viewCalculationViewModel.EditQuantityWindow != null)
                                    {
                                        viewCalculationViewModel.EditQuantityWindow.Close();
                                    }
                                }
                                else
                                {
                                    Log.Error("SaveCalculationCommand -> Greska, kalkulacija ne postoji u bazi ->");
                                    MessageBox.Show("Greška prilikom izmene kalkulacije!", "Greška - kalkulacija", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error("SaveCalculationCommand -> Greska prilikom kreiranja izmene kalkulacije ->", ex);
                        MessageBox.Show("Greška prilikom izmene kalkulacije!", "Greška - kalkulacija", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        private async Task KnjizenjePazaraKEP(SqliteDbContext sqliteDbContext,
            CalculationDB calculationDB)
        {
            var proknjizeniPazariDB = sqliteDbContext.KnjizenjePazara.Where(kp => kp.IssueDateTime.Date >= calculationDB.CalculationDate.Date);

            if (proknjizeniPazariDB != null &&
                proknjizeniPazariDB.Any())
            {
                await proknjizeniPazariDB.ForEachAsync(kp =>
                {
                    var kep = sqliteDbContext.Kep.Where(kep => kep.KepDate.Date == kp.IssueDateTime.Date &&
                    kep.Type >= (int)KepStateEnumeration.Dnevni_Pazar_Prodaja_Gotovina && kep.Type <= (int)KepStateEnumeration.Dnevni_Pazar_Refundacija_Virman);

                    if(kep != null &&
                    kep.Any())
                    {
                        kep.ForEachAsync(k =>
                        {
                            PaymentTypeEnumeration? paymentType = null;

                            switch (k.Type)
                            {
                                case (int)KepStateEnumeration.Dnevni_Pazar_Prodaja_Gotovina:
                                    kp.NormalSaleCash = 0;
                                    paymentType = PaymentTypeEnumeration.Cash;
                                    break;
                                case (int)KepStateEnumeration.Dnevni_Pazar_Prodaja_Kartica:
                                    kp.NormalSaleCard = 0;
                                    paymentType = PaymentTypeEnumeration.Card;
                                    break;
                                case (int)KepStateEnumeration.Dnevni_Pazar_Prodaja_Virman:
                                    kp.NormalSaleWireTransfer = 0;
                                    paymentType = PaymentTypeEnumeration.WireTransfer;
                                    break;
                                case (int)KepStateEnumeration.Dnevni_Pazar_Refundacija_Gotovina:
                                    kp.NormalRefundCash = 0;
                                    paymentType = PaymentTypeEnumeration.Cash;
                                    break;
                                case (int)KepStateEnumeration.Dnevni_Pazar_Refundacija_Kartica:
                                    kp.NormalRefundCard = 0;
                                    paymentType = PaymentTypeEnumeration.Card;
                                    break;
                                case (int)KepStateEnumeration.Dnevni_Pazar_Refundacija_Virman:
                                    kp.NormalRefundWireTransfer = 0;
                                    paymentType = PaymentTypeEnumeration.WireTransfer;
                                    break;
                            }

                            if (paymentType != null)
                            {
                                var invoices = sqliteDbContext.Invoices.Where(invoice => invoice.KnjizenjePazaraId == kp.Id)
                                .Join(sqliteDbContext.PaymentInvoices,
                                invoice => invoice.Id,
                                payment => payment.InvoiceId,
                                (invoice, payment) => new { Invoice = invoice, Payment = payment })
                                .Where(invoice => invoice.Payment.PaymentType == paymentType)
                                .Select(invoice => invoice.Invoice);

                                if (invoices != null &&
                                invoices.Any())
                                {
                                    k.Razduzenje = 0;
                                    k.Zaduzenje = 0;
                                    invoices.ForEachAsync(invoice =>
                                    {
                                        var itemsInInvoice = sqliteDbContext.ItemInvoices.Where(itemInvoice => itemInvoice.InvoiceId == invoice.Id &&
                                        (itemInvoice.IsSirovina == null || itemInvoice.IsSirovina == 0));

                                        if(itemsInInvoice != null &&
                                        itemsInInvoice.Any())
                                        {
                                            itemsInInvoice.ForEachAsync(item =>
                                            {
                                                var itemDB = sqliteDbContext.Items.Find(item.ItemCode);

                                                if (itemDB != null)
                                                {
                                                    if (itemDB.IdNorm != null)
                                                    {
                                                        switch (paymentType)
                                                        {
                                                            case PaymentTypeEnumeration.Cash:
                                                                if (invoice.TransactionType == 0)
                                                                {
                                                                    k.Zaduzenje += item.TotalAmout != null && item.TotalAmout.HasValue ? item.TotalAmout.Value : 0;
                                                                }
                                                                else
                                                                {
                                                                    k.Zaduzenje -= item.TotalAmout != null && item.TotalAmout.HasValue ? item.TotalAmout.Value : 0;
                                                                }
                                                                break;
                                                            case PaymentTypeEnumeration.Card:
                                                                if (invoice.TransactionType == 0)
                                                                {
                                                                    k.Zaduzenje += item.TotalAmout != null && item.TotalAmout.HasValue ? item.TotalAmout.Value : 0;
                                                                }
                                                                else
                                                                {
                                                                    k.Zaduzenje -= item.TotalAmout != null && item.TotalAmout.HasValue ? item.TotalAmout.Value : 0;
                                                                }
                                                                break;
                                                            case PaymentTypeEnumeration.WireTransfer:
                                                                if (invoice.TransactionType == 0)
                                                                {
                                                                    k.Zaduzenje += item.TotalAmout != null && item.TotalAmout.HasValue ? item.TotalAmout.Value : 0;
                                                                }
                                                                else
                                                                {
                                                                    k.Zaduzenje -= item.TotalAmout != null && item.TotalAmout.HasValue ? item.TotalAmout.Value : 0;
                                                                }
                                                                break;
                                                        }
                                                    }
                                                }
                                            });
                                        }

                                        switch (paymentType)
                                        {
                                            case PaymentTypeEnumeration.Cash:
                                                if (invoice.TransactionType == 0)
                                                {
                                                    kp.NormalSaleCash += invoice.TotalAmount != null && invoice.TotalAmount.HasValue ? invoice.TotalAmount.Value : 0;
                                                    k.Razduzenje += invoice.TotalAmount != null && invoice.TotalAmount.HasValue ? invoice.TotalAmount.Value : 0;
                                                }
                                                else
                                                {
                                                    kp.NormalRefundCash -= invoice.TotalAmount != null && invoice.TotalAmount.HasValue ? invoice.TotalAmount.Value : 0;
                                                    k.Razduzenje -= invoice.TotalAmount != null && invoice.TotalAmount.HasValue ? invoice.TotalAmount.Value : 0;
                                                }
                                                break;
                                            case PaymentTypeEnumeration.Card:
                                                if (invoice.TransactionType == 0)
                                                {
                                                    kp.NormalSaleCard += invoice.TotalAmount != null && invoice.TotalAmount.HasValue ? invoice.TotalAmount.Value : 0;
                                                    k.Razduzenje += invoice.TotalAmount != null && invoice.TotalAmount.HasValue ? invoice.TotalAmount.Value : 0;
                                                }
                                                else
                                                {
                                                    kp.NormalRefundCard -= invoice.TotalAmount != null && invoice.TotalAmount.HasValue ? invoice.TotalAmount.Value : 0;
                                                    k.Razduzenje -= invoice.TotalAmount != null && invoice.TotalAmount.HasValue ? invoice.TotalAmount.Value : 0;
                                                }
                                                break;
                                            case PaymentTypeEnumeration.WireTransfer:
                                                if (invoice.TransactionType == 0)
                                                {
                                                    kp.NormalSaleWireTransfer += invoice.TotalAmount != null && invoice.TotalAmount.HasValue ? invoice.TotalAmount.Value : 0;
                                                    k.Razduzenje -= invoice.TotalAmount != null && invoice.TotalAmount.HasValue ? invoice.TotalAmount.Value : 0;
                                                }
                                                else
                                                {
                                                    kp.NormalRefundWireTransfer -= invoice.TotalAmount != null && invoice.TotalAmount.HasValue ? invoice.TotalAmount.Value : 0;
                                                    k.Razduzenje -= invoice.TotalAmount != null && invoice.TotalAmount.HasValue ? invoice.TotalAmount.Value : 0;
                                                }
                                                break;
                                        }
                                    });
                                }
                            }
                            sqliteDbContext.Kep.Update(k);
                        });
                    }
                    sqliteDbContext.KnjizenjePazara.Update(kp);
                });
                RetryHelper.ExecuteWithRetry(() => { sqliteDbContext.SaveChanges(); });
            }
        }
        private async Task<decimal> SrediProsecnuCenu(SqliteDbContext sqliteDbContext, 
            ItemDB itemDB,
            bool isSirovina)
        {
            decimal prosecnaCena = 0;
            decimal quantity = 0;

            var proknjizeniPazari = sqliteDbContext.KnjizenjePazara.Join(sqliteDbContext.Invoices,
            knjizenje => knjizenje.Id,
                invoice => invoice.KnjizenjePazaraId,
                (knjizenje, invoice) => new { Knji = knjizenje, Invoice = invoice })
                .Join(sqliteDbContext.ItemInvoices,
                invoice => invoice.Invoice.Id,
                invoiceItem => invoiceItem.InvoiceId,
                (invoice, invoiceItem) => new { KnjizenjeInvoice = invoice, InvItem = invoiceItem })
                .Where(pazar => pazar.KnjizenjeInvoice.Invoice.SdcDateTime != null &&
                pazar.KnjizenjeInvoice.Invoice.SdcDateTime.HasValue &&
                pazar.InvItem.ItemCode == itemDB.Id)
                .OrderBy(item => item.KnjizenjeInvoice.Knji.IssueDateTime);

            var neproknjizeniPazari = sqliteDbContext.Invoices.Join(sqliteDbContext.ItemInvoices,
            invoice => invoice.Id,
                invoiceItem => invoiceItem.InvoiceId,
                (invoice, invoiceItem) => new { Inv = invoice, InvItem = invoiceItem })
                .Where(pazar => pazar.Inv.SdcDateTime != null && pazar.Inv.SdcDateTime.HasValue &&
                string.IsNullOrEmpty(pazar.Inv.KnjizenjePazaraId) &&
                pazar.InvItem.ItemCode == itemDB.Id)
                .OrderBy(item => item.Inv.SdcDateTime);

            var kalkulacije = sqliteDbContext.Calculations.Join(sqliteDbContext.CalculationItems,
            calculacion => calculacion.Id,
                calculationItem => calculationItem.CalculationId,
                (calculacion, calculationItem) => new { Cal = calculacion, CalItem = calculationItem })
                .Where(kal => kal.CalItem.ItemId == itemDB.Id)
                .OrderBy(item => item.Cal.CalculationDate);

            List<string> neproknjizeni = new List<string>();
            List<string> kalkulacijeOdradjene = new List<string>();

            if (proknjizeniPazari != null &&
               proknjizeniPazari.Any())
            {
                proknjizeniPazari = proknjizeniPazari.OrderBy(paz => paz.KnjizenjeInvoice.Invoice.SdcDateTime);

                await proknjizeniPazari.ForEachAsync(prPazar =>
                {
                    if (kalkulacije != null &&
                        kalkulacije.Any())
                    {
                        var kalkulacijeForDate = kalkulacije.Where(kal => prPazar.KnjizenjeInvoice.Invoice.SdcDateTime.HasValue &&
                        kal.Cal.CalculationDate.Date <= prPazar.KnjizenjeInvoice.Invoice.SdcDateTime.Value.Date);

                        if (kalkulacijeForDate != null &&
                        kalkulacijeForDate.Any())
                        {
                            kalkulacijeForDate = kalkulacijeForDate.OrderBy(kal => kal.Cal.CalculationDate);

                            kalkulacijeForDate.ForEachAsync(kal =>
                            {
                                if (!kalkulacijeOdradjene.Any() ||
                                !kalkulacijeOdradjene.Contains(kal.Cal.Id))
                                {
                                    if(prosecnaCena == 0)
                                    {
                                        prosecnaCena = kal.CalItem.InputPrice;
                                    }
                                    else
                                    {
                                        decimal delilac = Math.Abs(quantity) * prosecnaCena + kal.CalItem.Quantity * kal.CalItem.InputPrice;
                                        decimal deljenik = Math.Abs(quantity) + kal.CalItem.Quantity;

                                        if (delilac == 0 || deljenik == 0)
                                        {
                                            prosecnaCena = 0;
                                        }
                                        else 
                                        {
                                            prosecnaCena = Decimal.Round(delilac / deljenik, 2); 
                                        }
                                    }
                                    quantity += kal.CalItem.Quantity;

                                    kalkulacijeOdradjene.Add(kal.Cal.Id);
                                }
                            });
                        }
                    }

                    if (neproknjizeniPazari != null &&
                    neproknjizeniPazari.Any())
                    {
                        var neproknjizeniPazariForDate = neproknjizeniPazari.Where(paz => paz.Inv.SdcDateTime.HasValue &&
                        prPazar.KnjizenjeInvoice.Invoice.SdcDateTime.HasValue &&
                        paz.Inv.SdcDateTime.Value.Date <= prPazar.KnjizenjeInvoice.Invoice.SdcDateTime.Value.Date);

                        if (neproknjizeniPazariForDate != null &&
                        neproknjizeniPazariForDate.Any())
                        {
                            neproknjizeniPazariForDate = neproknjizeniPazariForDate.OrderBy(paz => paz.Inv.SdcDateTime);

                            neproknjizeniPazariForDate.ForEachAsync(paz =>
                            {
                                if (!neproknjizeni.Any() ||
                                !neproknjizeni.Contains(paz.Inv.Id))
                                {
                                    if (paz.InvItem.Quantity.HasValue)
                                    {
                                        quantity -= paz.InvItem.Quantity.Value;

                                        if (isSirovina)
                                        {
                                            paz.InvItem.UnitPrice = prosecnaCena;
                                        }
                                        paz.InvItem.InputUnitPrice = prosecnaCena;
                                        sqliteDbContext.ItemInvoices.Update(paz.InvItem);
                                        neproknjizeni.Add(paz.Inv.Id);
                                    }
                                }
                            });
                        }


                    }

                    if (prPazar.InvItem.Quantity.HasValue)
                    {
                        quantity -= prPazar.InvItem.Quantity.Value;
                        if (isSirovina)
                        {
                            prPazar.InvItem.UnitPrice = prosecnaCena;
                        }
                        prPazar.InvItem.InputUnitPrice = prosecnaCena;
                        sqliteDbContext.ItemInvoices.Update(prPazar.InvItem);
                    }
                });
            }
            else
            {
                if (neproknjizeniPazari != null &&
                    neproknjizeniPazari.Any())
                {
                    neproknjizeniPazari = neproknjizeniPazari.OrderBy(paz => paz.Inv.SdcDateTime);

                    await neproknjizeniPazari.ForEachAsync(pazar =>
                    {
                        if (kalkulacije != null &&
                            kalkulacije.Any())
                        {
                            var kalkulacijeForDate = kalkulacije.Where(kal => pazar.Inv.SdcDateTime.HasValue &&
                            kal.Cal.CalculationDate.Date <= pazar.Inv.SdcDateTime.Value.Date);

                            if (kalkulacijeForDate != null &&
                            kalkulacijeForDate.Any())
                            {
                                kalkulacijeForDate = kalkulacijeForDate.OrderBy(kal => kal.Cal.CalculationDate);

                                kalkulacijeForDate.ForEachAsync(kal =>
                                {
                                    if (!kalkulacijeOdradjene.Any() ||
                                    !kalkulacijeOdradjene.Contains(kal.Cal.Id))
                                    {
                                        if (prosecnaCena == 0)
                                        {
                                            prosecnaCena = kal.CalItem.InputPrice;
                                        }
                                        else
                                        {
                                            decimal delilac = Math.Abs(quantity) * prosecnaCena + kal.CalItem.Quantity * kal.CalItem.InputPrice;
                                            decimal deljenik = Math.Abs(quantity) + kal.CalItem.Quantity;

                                            if (delilac == 0 || deljenik == 0)
                                            {
                                                prosecnaCena = 0;
                                            }
                                            else
                                            {
                                                prosecnaCena = Decimal.Round(delilac / deljenik, 2);
                                            }
                                        }
                                        quantity += kal.CalItem.Quantity;

                                        kalkulacijeOdradjene.Add(kal.Cal.Id);
                                    }
                                });
                            }
                        }
                        if (pazar.InvItem.Quantity.HasValue)
                        {
                            quantity -= pazar.InvItem.Quantity.Value;
                            if (isSirovina)
                            {
                                pazar.InvItem.UnitPrice = prosecnaCena;
                            }
                            pazar.InvItem.InputUnitPrice = prosecnaCena;
                            sqliteDbContext.ItemInvoices.Update(pazar.InvItem);
                        }
                    });
                }
                else
                {
                    if (kalkulacije != null &&
                        kalkulacije.Any())
                    {
                        kalkulacije = kalkulacije.OrderBy(kal => kal.Cal.CalculationDate);

                        await kalkulacije.ForEachAsync(kal =>
                        {
                            if(prosecnaCena == 0)
                            {
                                quantity += kal.CalItem.Quantity;
                            }
                            else
                            {
                                decimal delilac = Math.Abs(quantity) * prosecnaCena + kal.CalItem.InputPrice * kal.CalItem.Quantity;
                                decimal deljenik = Math.Abs(quantity) + kal.CalItem.Quantity;

                                if (delilac == 0 || deljenik == 0)
                                {
                                    prosecnaCena = 0;
                                }
                                else
                                {
                                    prosecnaCena = Decimal.Round(delilac / deljenik, 2);
                                }
                            }

                            quantity += kal.CalItem.Quantity;
                        });
                    }
                }
            }

            return Decimal.Round(prosecnaCena, 2);
        }
        private async void StarijaKalkulacija(SqliteDbContext sqliteDbContext,
            DateTime calculationDate,
            Invertory calculationItem,
            ItemDB itemDB,
            bool isSirovina,
            CalculationDB? calculationDB,
            decimal popisQuantity = 0)
        {
            decimal qunatityPazari = 0;
            decimal qunatityNivelacija = 0;
            decimal qunatityKalkulacija = 0;
            decimal unitPrice = itemDB.InputUnitPrice != null && itemDB.InputUnitPrice.HasValue ? itemDB.InputUnitPrice.Value : 0;

            if (itemDB.TotalQuantity != 0 &&
                unitPrice == 0)
            {
                unitPrice = await SrediProsecnuCenu(sqliteDbContext, itemDB, isSirovina);
            }

            var proknjizeniPazari = sqliteDbContext.KnjizenjePazara.Join(sqliteDbContext.Invoices,
                knjizenje => knjizenje.Id,
                invoice => invoice.KnjizenjePazaraId,
                (knjizenje, invoice) => new { Knji = knjizenje, Invoice = invoice })
                .Where(pazar => pazar.Knji.IssueDateTime.Date >= calculationDate.Date)
                .Join(sqliteDbContext.ItemInvoices,
                invoice => invoice.Invoice.Id,
                invoiceItem => invoiceItem.InvoiceId,
                (invoice, invoiceItem) => new { KnjizenjeInvoice = invoice, InvItem = invoiceItem })
                .Where(pazar => pazar.KnjizenjeInvoice.Invoice.SdcDateTime != null &&
                pazar.KnjizenjeInvoice.Invoice.SdcDateTime.HasValue &&
                pazar.KnjizenjeInvoice.Invoice.SdcDateTime.Value.Date >= calculationDate.Date &&
                pazar.InvItem.ItemCode == itemDB.Id)
                .OrderByDescending(item => item.KnjizenjeInvoice.Knji.IssueDateTime);

            var neproknjizeniPazari = sqliteDbContext.Invoices.Join(sqliteDbContext.ItemInvoices,
                invoice => invoice.Id,
                invoiceItem => invoiceItem.InvoiceId,
                (invoice, invoiceItem) => new { Inv = invoice, InvItem = invoiceItem })
                .Where(pazar => pazar.Inv.SdcDateTime != null && pazar.Inv.SdcDateTime.HasValue &&
                pazar.Inv.SdcDateTime.Value.Date >= calculationDate.Date && string.IsNullOrEmpty(pazar.Inv.KnjizenjePazaraId) &&
                pazar.InvItem.ItemCode == itemDB.Id)
                .OrderByDescending(item => item.Inv.SdcDateTime);

            var kalkulacije  = sqliteDbContext.Calculations.Join(sqliteDbContext.CalculationItems,
                calculacion => calculacion.Id,
                calculationItem => calculationItem.CalculationId,
                (calculacion, calculationItem) => new { Cal = calculacion, CalItem = calculationItem })
                .Where(kal => kal.Cal.CalculationDate >= calculationDate.Date &&
                kal.CalItem.ItemId == itemDB.Id)
                .OrderByDescending(item => item.Cal.CalculationDate);

            //if (calculationDB != null)
            //{
            //    kalkulacije = sqliteDbContext.Calculations.Join(sqliteDbContext.CalculationItems,
            //    calculacion => calculacion.Id,
            //    calculationItem => calculationItem.CalculationId,
            //    (calculacion, calculationItem) => new { Cal = calculacion, CalItem = calculationItem })
            //    .Where(kal => kal.Cal.CalculationDate >= calculationDate.Date &&
            //    kal.CalItem.ItemId == itemDB.Id &&
            //    ((calculationDB != null && kal.Cal.Counter != calculationDB.Counter) || calculationDB == null))
            //    .OrderByDescending(item => item.Cal.CalculationDate);
            //}

            List<string> neproknjizeni = new List<string>();
            List<string> kalkulacijeOdradjene = new List<string>();

            if (proknjizeniPazari != null &&
                proknjizeniPazari.Any())
            {
                await proknjizeniPazari.ForEachAsync(prPazar =>
                {
                    var neproknjizeniPazariForDate = neproknjizeniPazari.Where(paz => paz.Inv.SdcDateTime.HasValue &&
                    prPazar.KnjizenjeInvoice.Invoice.SdcDateTime.HasValue &&
                    paz.Inv.SdcDateTime.Value.Date <= prPazar.KnjizenjeInvoice.Invoice.SdcDateTime.Value.Date);

                    if (neproknjizeniPazariForDate != null &&
                    neproknjizeniPazariForDate.Any())
                    {
                        neproknjizeniPazariForDate.ForEachAsync(paz =>
                        {
                            if (!neproknjizeni.Any() ||
                            !neproknjizeni.Contains(paz.Inv.Id))
                            {
                                if (paz.InvItem.Quantity.HasValue)
                                {
                                    qunatityPazari += paz.InvItem.Quantity.Value;

                                    neproknjizeni.Add(paz.Inv.Id);
                                }
                            }
                        });
                    }

                    if (prPazar.InvItem.Quantity.HasValue)
                    {
                        qunatityPazari += prPazar.InvItem.Quantity.Value;
                    }

                    if (kalkulacije != null &&
                    kalkulacije.Any())
                    {
                        var kalkulacijeForDate = kalkulacije.Where(kal => prPazar.KnjizenjeInvoice.Invoice.SdcDateTime.HasValue &&
                        kal.Cal.CalculationDate.Date <= prPazar.KnjizenjeInvoice.Invoice.SdcDateTime.Value.Date);

                        if (kalkulacijeForDate != null &&
                        kalkulacijeForDate.Any())
                        {
                            kalkulacijeForDate.ForEachAsync(kal =>
                            {
                                if (!kalkulacijeOdradjene.Any() ||
                                !kalkulacijeOdradjene.Contains(kal.Cal.Id))
                                {
                                    if (unitPrice == 0)
                                    {
                                        unitPrice = kal.CalItem.InputPrice;
                                    }
                                    else
                                    {
                                        decimal quantityTrenutno = Math.Abs(itemDB.TotalQuantity)  + qunatityPazari - qunatityKalkulacija;
                                        decimal quantityUkupno = Math.Abs(itemDB.TotalQuantity) + qunatityPazari - qunatityKalkulacija - kal.CalItem.Quantity;

                                        decimal delilac = (quantityTrenutno * unitPrice) - kal.CalItem.InputPrice * kal.CalItem.Quantity;

                                        if (delilac == 0 || quantityUkupno == 0)
                                        {
                                            unitPrice = 0;
                                        }
                                        else
                                        {
                                            unitPrice = Decimal.Round(delilac / quantityUkupno, 2);
                                        }
                                    }
                                    qunatityKalkulacija += kal.CalItem.Quantity;

                                    kalkulacijeOdradjene.Add(kal.Cal.Id);
                                }
                            });
                        }
                    }
                });
            }
            if (neproknjizeniPazari != null &&
                neproknjizeniPazari.Any())
            {
                await neproknjizeniPazari.ForEachAsync(pazar =>
                {
                    if (pazar.InvItem.Quantity.HasValue)
                    {
                        qunatityPazari += pazar.InvItem.Quantity.Value;
                    }
                    if (kalkulacije != null &&
                        kalkulacije.Any())
                    {

                        var kalkulacijeForDate = kalkulacije.Where(kal => pazar.Inv.SdcDateTime.HasValue &&
                        kal.Cal.CalculationDate.Date <= pazar.Inv.SdcDateTime.Value.Date);

                        if (kalkulacijeForDate != null &&
                            kalkulacijeForDate.Any())
                        {
                            kalkulacijeForDate.ForEachAsync(kal =>
                            {
                                if (!kalkulacijeOdradjene.Any() ||
                                    !kalkulacijeOdradjene.Contains(kal.Cal.Id))
                                {
                                    decimal quantityTrenutno = Math.Abs(itemDB.TotalQuantity) + qunatityPazari - qunatityKalkulacija;
                                    decimal quantityPreKalkulacije = Math.Abs(itemDB.TotalQuantity) + qunatityPazari - qunatityKalkulacija - kal.CalItem.Quantity;

                                    if (unitPrice == 0)
                                    {
                                        unitPrice = kal.CalItem.InputPrice;
                                    }
                                    else
                                    {
                                        decimal delilac = (quantityTrenutno * unitPrice) - kal.CalItem.InputPrice * kal.CalItem.Quantity;

                                        if (delilac == 0 || quantityPreKalkulacije == 0)
                                        {
                                            unitPrice = 0;
                                        }
                                        else
                                        {
                                            unitPrice = Decimal.Round(delilac / quantityPreKalkulacije, 2);
                                        }

                                        //unitPrice = Decimal.Round(((quantityTrenutno * unitPrice) - kal.CalItem.InputPrice * kal.CalItem.Quantity) / quantityPreKalkulacije, 2);

                                    }
                                    qunatityKalkulacija += kal.CalItem.Quantity;

                                    kalkulacijeOdradjene.Add(kal.Cal.Id);
                                }
                            });

                        }
                    }
                });
            }
            if (kalkulacije != null &&
                kalkulacije.Any())
            {
                await kalkulacije.ForEachAsync(kal =>
                {
                    if (!kalkulacijeOdradjene.Any() ||
                        !kalkulacijeOdradjene.Contains(kal.Cal.Id))
                    {
                        if (unitPrice == 0)
                        {
                            unitPrice = kal.CalItem.InputPrice;
                        }
                        else
                        {
                            decimal delilac = (Math.Abs(itemDB.TotalQuantity) + qunatityPazari) * unitPrice - kal.CalItem.InputPrice * kal.CalItem.Quantity;
                            decimal deljenik = Math.Abs(itemDB.TotalQuantity) + qunatityPazari - kal.CalItem.Quantity - qunatityKalkulacija;

                            if (delilac == 0 || deljenik == 0)
                            {
                                unitPrice = 0;
                            }
                            else
                            {
                                unitPrice = Decimal.Round(delilac / deljenik, 2);
                            }
                        }
                        qunatityKalkulacija += kal.CalItem.Quantity;
                    }
                });
            }

            decimal quantityTotal = itemDB.TotalQuantity + qunatityPazari - qunatityKalkulacija;
            decimal deljenik = Math.Abs(quantityTotal)  * unitPrice + (calculationItem.InputPrice);
            decimal delilac = Math.Abs(quantityTotal) + calculationItem.Quantity;

            if (delilac == 0 || deljenik == 0)
            {
                itemDB.InputUnitPrice = unitPrice = 0;
            }
            else
            {
                itemDB.InputUnitPrice = unitPrice = Decimal.Round(deljenik / delilac, 2);
            }

            quantityTotal += calculationItem.Quantity;

            neproknjizeni = new List<string>();
            kalkulacijeOdradjene = new List<string>();

            if (proknjizeniPazari != null &&
                proknjizeniPazari.Any())
            {
                proknjizeniPazari = proknjizeniPazari.OrderBy(paz => paz.KnjizenjeInvoice.Invoice.SdcDateTime);

                await proknjizeniPazari.ForEachAsync(prPazar =>
                {
                    if (kalkulacije != null &&
                        kalkulacije.Any())
                    {
                        var kalkulacijeForDate = kalkulacije.Where(kal => prPazar.KnjizenjeInvoice.Invoice.SdcDateTime.HasValue &&
                        kal.Cal.CalculationDate.Date <= prPazar.KnjizenjeInvoice.Invoice.SdcDateTime.Value.Date);

                        if (kalkulacijeForDate != null &&
                        kalkulacijeForDate.Any())
                        {
                            kalkulacijeForDate = kalkulacijeForDate.OrderBy(kal => kal.Cal.CalculationDate);

                            kalkulacijeForDate.ForEachAsync(kal =>
                            {
                                if (!kalkulacijeOdradjene.Any() ||
                                !kalkulacijeOdradjene.Contains(kal.Cal.Id))
                                {
                                    if (calculationDB == null ||
                                    calculationDB.Counter != kal.Cal.Counter)
                                    {
                                        decimal delilac = Math.Abs(quantityTotal) * itemDB.InputUnitPrice.Value + kal.CalItem.Quantity * kal.CalItem.InputPrice;
                                        decimal deljenik = Math.Abs(quantityTotal) + kal.CalItem.Quantity;

                                        if (delilac == 0 || deljenik == 0)
                                        {
                                            itemDB.InputUnitPrice = unitPrice = 0;
                                        }
                                        else {
                                            itemDB.InputUnitPrice = unitPrice = Decimal.Round(delilac / deljenik, 2); 
                                        }

                                        qunatityKalkulacija -= kal.CalItem.Quantity;
                                        quantityTotal += kal.CalItem.Quantity;

                                        kalkulacijeOdradjene.Add(kal.Cal.Id);
                                    }
                                }
                            });
                        }
                    }

                    if (neproknjizeniPazari != null &&
                    neproknjizeniPazari.Any())
                    {
                        var neproknjizeniPazariForDate = neproknjizeniPazari.Where(paz => paz.Inv.SdcDateTime.HasValue &&
                        prPazar.KnjizenjeInvoice.Invoice.SdcDateTime.HasValue &&
                        paz.Inv.SdcDateTime.Value.Date <= prPazar.KnjizenjeInvoice.Invoice.SdcDateTime.Value.Date);

                        if (neproknjizeniPazariForDate != null &&
                        neproknjizeniPazariForDate.Any())
                        {
                            neproknjizeniPazariForDate = neproknjizeniPazariForDate.OrderBy(paz => paz.Inv.SdcDateTime);

                            neproknjizeniPazariForDate.ForEachAsync(paz =>
                            {
                                if (!neproknjizeni.Any() ||
                                !neproknjizeni.Contains(paz.Inv.Id))
                                {
                                    if (paz.InvItem.Quantity.HasValue)
                                    {
                                        quantityTotal -= paz.InvItem.Quantity.Value;
                                        qunatityPazari -= paz.InvItem.Quantity.Value;

                                        if (isSirovina)
                                        {
                                            paz.InvItem.UnitPrice = itemDB.InputUnitPrice;
                                            paz.InvItem.OriginalUnitPrice = itemDB.InputUnitPrice;
                                            paz.InvItem.TotalAmout = Decimal.Round(itemDB.InputUnitPrice.Value * paz.InvItem.Quantity.Value, 2);
                                        }
                                        paz.InvItem.InputUnitPrice = itemDB.InputUnitPrice;
                                        sqliteDbContext.ItemInvoices.Update(paz.InvItem);

                                        neproknjizeni.Add(paz.Inv.Id);
                                    }
                                }
                            });
                        }


                    }

                    if (prPazar.InvItem.Quantity.HasValue)
                    {
                        quantityTotal -= prPazar.InvItem.Quantity.Value;
                        qunatityPazari -= prPazar.InvItem.Quantity.Value;
                        if (isSirovina)
                        {
                            prPazar.InvItem.UnitPrice = itemDB.InputUnitPrice;
                            prPazar.InvItem.OriginalUnitPrice = itemDB.InputUnitPrice;
                            prPazar.InvItem.TotalAmout = Decimal.Round(itemDB.InputUnitPrice.Value * prPazar.InvItem.Quantity.Value);
                        }
                        prPazar.InvItem.InputUnitPrice = itemDB.InputUnitPrice;
                        sqliteDbContext.ItemInvoices.Update(prPazar.InvItem);
                    }
                });
            }
            if (neproknjizeniPazari != null &&
                neproknjizeniPazari.Any())
            {
                neproknjizeniPazari = neproknjizeniPazari.OrderBy(paz => paz.Inv.SdcDateTime);

                await neproknjizeniPazari.ForEachAsync(pazar =>
                {
                    if (kalkulacije != null &&
                        kalkulacije.Any())
                    {
                        var kalkulacijeForDate = kalkulacije.Where(kal => pazar.Inv.SdcDateTime.HasValue &&
                        kal.Cal.CalculationDate.Date <= pazar.Inv.SdcDateTime.Value.Date);

                        if (kalkulacijeForDate != null &&
                        kalkulacijeForDate.Any())
                        {
                            kalkulacijeForDate = kalkulacijeForDate.OrderBy(kal => kal.Cal.CalculationDate);

                            kalkulacijeForDate.ForEachAsync(kal =>
                            {
                                if (!kalkulacijeOdradjene.Any() ||
                                !kalkulacijeOdradjene.Contains(kal.Cal.Id))
                                {
                                    if (calculationDB == null ||
                                    calculationDB.Counter != kal.Cal.Counter)
                                    {
                                        decimal delilac = Math.Abs(quantityTotal) * unitPrice + kal.CalItem.Quantity * kal.CalItem.InputPrice;
                                        decimal deljenik = Math.Abs(quantityTotal) + kal.CalItem.Quantity;

                                        if (delilac == 0 || deljenik == 0)
                                        {
                                            itemDB.InputUnitPrice = unitPrice = 0;
                                        }
                                        else
                                        {
                                            itemDB.InputUnitPrice = unitPrice = Decimal.Round(delilac / deljenik, 2);
                                        }

                                        qunatityKalkulacija -= kal.CalItem.Quantity;
                                        quantityTotal += kal.CalItem.Quantity;

                                        kalkulacijeOdradjene.Add(kal.Cal.Id);
                                    }
                                }
                            });
                        }
                    }
                    if (pazar.InvItem.Quantity.HasValue)
                    {
                        quantityTotal -= pazar.InvItem.Quantity.Value;
                        qunatityPazari -= pazar.InvItem.Quantity.Value;

                        pazar.InvItem.InputUnitPrice = itemDB.InputUnitPrice;
                        if (isSirovina)
                        {
                            pazar.InvItem.UnitPrice = itemDB.InputUnitPrice;
                            pazar.InvItem.OriginalUnitPrice = itemDB.InputUnitPrice;
                            pazar.InvItem.TotalAmout = Decimal.Round(itemDB.InputUnitPrice.Value * pazar.InvItem.Quantity.Value, 2);
                        }
                        sqliteDbContext.ItemInvoices.Update(pazar.InvItem);
                    }
                });
            }
            if (kalkulacije != null &&
                kalkulacije.Any())
            {
                kalkulacije = kalkulacije.OrderBy(kal => kal.Cal.CalculationDate);

                await kalkulacije.ForEachAsync(kal =>
                {
                    if (!kalkulacijeOdradjene.Any() ||
                    !kalkulacijeOdradjene.Contains(kal.Cal.Id))
                    {
                        if (calculationDB == null ||
                        calculationDB.Counter != kal.Cal.Counter)
                        {
                            decimal delilac = Math.Abs(quantityTotal) * unitPrice + kal.CalItem.InputPrice * kal.CalItem.Quantity;
                            decimal deljenik = Math.Abs(quantityTotal) + kal.CalItem.Quantity;

                            if (delilac == 0 || deljenik == 0)
                            {
                                itemDB.InputUnitPrice = unitPrice = 0;
                            }
                            else
                            {
                                itemDB.InputUnitPrice = unitPrice = Decimal.Round(delilac / deljenik, 2);
                            }

                            qunatityKalkulacija -= kal.CalItem.Quantity;
                            quantityTotal += kal.CalItem.Quantity;
                        }
                    }
                });

                sqliteDbContext.Items.Update(itemDB);
                RetryHelper.ExecuteWithRetry(() => { sqliteDbContext.SaveChanges(); });
            }
        }
        private void Kalkulacija(SqliteDbContext sqliteDbContext,
            Invertory calculationItem,
            ItemDB itemDB,
            CalculationDB? calculationDB = null)
        {
            decimal quantity = itemDB.TotalQuantity > 0 ? itemDB.TotalQuantity : 0;
            decimal inputUnitPrice = itemDB.InputUnitPrice != null && itemDB.InputUnitPrice.HasValue ? itemDB.InputUnitPrice.Value : 0;

            itemDB.InputUnitPrice = Decimal.Round(((inputUnitPrice * quantity) + (calculationItem.InputPrice)) / (quantity + calculationItem.Quantity), 2);
            
            sqliteDbContext.Items.Update(itemDB);
            RetryHelper.ExecuteWithRetry(() => { sqliteDbContext.SaveChanges(); });
        }
        private void AddNivelacijaItem(SqliteDbContext sqliteDbContext,
            Item item,
            ItemDB itemDB,
            Models.AppMain.Statistic.Nivelacija nivelacija,
            ref decimal nivelacijaTotal)
        {

            var nivelacijaItem = new NivelacijaItem(item);
            nivelacijaItem.OldPrice = itemDB.SellingUnitPrice;
            nivelacijaItem.NewPrice = item.SellingUnitPrice;

            ItemNivelacijaDB itemNivelacijaDB = new ItemNivelacijaDB()
            {
                IdNivelacija = nivelacija.Id,
                IdItem = nivelacijaItem.IdItem,
                NewUnitPrice = nivelacijaItem.NewPrice,
                OldUnitPrice = nivelacijaItem.OldPrice,
                StopaPDV = nivelacijaItem.StopaPDV,
                TotalQuantity = itemDB.TotalQuantity,
            };
            sqliteDbContext.ItemsNivelacija.Add(itemNivelacijaDB);

            itemDB.SellingUnitPrice = nivelacijaItem.NewPrice;
            sqliteDbContext.Items.Update(itemDB);

            RetryHelper.ExecuteWithRetry(() => { sqliteDbContext.SaveChanges(); });

            nivelacijaTotal += (itemNivelacijaDB.NewUnitPrice - itemNivelacijaDB.OldUnitPrice) * itemNivelacijaDB.TotalQuantity;

            Log.Debug($"SaveCommand - AddNivelacija - Uspesno sacuvan artikal {item.Id} za nivelaciju {nivelacija.Id}");
        }
    }
}