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
using ClickBar_DatabaseSQLManager;
using ClickBar_DatabaseSQLManager.Models;
using ClickBar_Logging;
using ClickBar_Printer;
using ClickBar_Printer.PaperFormat;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ClickBar.Commands.AppMain.Statistic.Calculation
{
    public class SaveCalculationCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ViewModelBase _currentViewModel;
        private SqlServerDbContext _dbContext;

        public SaveCalculationCommand(ViewModelBase currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter) => true;

        public async void Execute(object parameter)
        {
            if (_currentViewModel is CalculationViewModel ||
                _currentViewModel is ViewCalculationViewModel)
            {
                if (_currentViewModel is CalculationViewModel calculationViewModel)
                {
                    _dbContext = calculationViewModel.DbContext;
                }
                else if (_currentViewModel is ViewCalculationViewModel viewCalculationViewModel)
                {
                    _dbContext = viewCalculationViewModel.DbContext;
                }

                await Calculate();
            }
        }

        private async Task Calculate()
        {
            if (_currentViewModel is CalculationViewModel calculationViewModel)
            {
                var result = MessageBox.Show("Da li ste sigurni da želite da uradite kalkulaciju?", "",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                    return;

                try
                {
                    var allNivelacijeInYear = calculationViewModel.DbContext.Calculations
                        .Where(cal => cal.CalculationDate.Year == calculationViewModel.CalculationDate.Year);

                    int counterCalculation = allNivelacijeInYear.Any()
                        ? allNivelacijeInYear.Max(nivelacija => nivelacija.Counter) + 1
                        : 1;

                    var nivelacija = new Models.AppMain.Statistic.Nivelacija(_dbContext,
                        NivelacijaStateEnumeration.Kalkulacija,
                        calculationViewModel.CalculationDate.AddSeconds(-1));

                    CalculationDB calculationDB = new CalculationDB()
                    {
                        Id = Guid.NewGuid().ToString(),
                        CashierId = calculationViewModel.LoggedCashier.Id,
                        SupplierId = calculationViewModel.SelectedSupplier.Id,
                        CalculationDate = calculationViewModel.CalculationDate,
                        InputTotalPrice = 0,
                        OutputTotalPrice = 0,
                        InvoiceNumber = calculationViewModel.InvoiceNumber,
                        Counter = counterCalculation
                    };
                    calculationViewModel.DbContext.Calculations.Add(calculationDB);
                    calculationViewModel.DbContext.SaveChanges();

                    List<InvertoryGlobal> invertoryGlobals = new List<InvertoryGlobal>();
                    bool nivelacijaIsAdded = false;
                    NivelacijaDB? nivelacijaDB = null;
                    decimal nivelacijaTotal = 0;

                    foreach (var item in calculationViewModel.Calculations.ToList())
                    {
                        var itemDB = calculationViewModel.DbContext.Items.Find(item.Item.Id);
                        if (itemDB == null) continue;

                        var group = calculationViewModel.DbContext.ItemGroups.Find(itemDB.IdItemGroup);
                        if (group == null)
                        {
                            MessageBox.Show("ARTIKAL MORA DA PRIPADA NEKOJ GRUPI!",
                                "Greška",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                            return;
                        }

                        bool isSirovina = group.Name.ToLower().Contains("sirovine") || group.Name.ToLower().Contains("sirovina");

                        // 1. Digni stanje
                        itemDB.TotalQuantity += item.Quantity;

                        // 2. Running prosečna cena i lager OD datuma kalkulacije pa do poslednjeg računa
                        FixInputPriceFromCalculation(calculationViewModel.CalculationDate, 
                            itemDB, 
                            isSirovina, 
                            calculationViewModel.DbContext,
                            item.Quantity,
                            item.InputPrice);

                        // 3. Nivelacija (ako je menjana prodajna cena)
                        if (!isSirovina && itemDB.SellingUnitPrice != item.Item.SellingUnitPrice)
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
                                calculationViewModel.DbContext.Nivelacijas.Add(nivelacijaDB);
                                calculationViewModel.DbContext.SaveChanges();
                            }
                            AddNivelacijaItem(item.Item, itemDB, nivelacija, ref nivelacijaTotal);
                        }

                        // 4. Upis kalkulacione stavke
                        CalculationItemDB calculationItemDB = new CalculationItemDB()
                        {
                            CalculationId = calculationDB.Id,
                            ItemId = itemDB.Id,
                            InputPrice = Decimal.Round(item.InputPrice / item.Quantity, 2),
                            OutputPrice = item.Item.SellingUnitPrice,
                            Quantity = item.Quantity
                        };
                        calculationViewModel.DbContext.CalculationItems.Add(calculationItemDB);

                        calculationDB.InputTotalPrice += item.InputPrice;
                        calculationDB.OutputTotalPrice += item.Item.SellingUnitPrice * item.Quantity;
                        calculationViewModel.DbContext.Calculations.Update(calculationDB);

                        calculationViewModel.DbContext.Items.Update(itemDB);

                        invertoryGlobals.Add(new InvertoryGlobal()
                        {
                            Id = item.Item.Id,
                            Name = item.Item.Name,
                            Jm = item.Item.Jm,
                            InputUnitPrice = Decimal.Round(item.InputPrice / item.Quantity, 2),
                            Quantity = item.Quantity,
                            TotalAmout = item.InputPrice,
                            SellingUnitPrice = item.Item.SellingUnitPrice
                        });
                    }

                    calculationViewModel.DbContext.SaveChanges();

                    // KEP upis za nivelaciju
                    if (nivelacijaIsAdded && nivelacijaDB != null)
                    {
                        var kepNivelacijaDB = new KepDB
                        {
                            Id = Guid.NewGuid().ToString(),
                            KepDate = nivelacijaDB.DateNivelacije,
                            Type = (int)KepStateEnumeration.Nivelacija,
                            Razduzenje = 0,
                            Zaduzenje = nivelacijaTotal,
                            Description = $"Nivelacija 'Nivelacija_{nivelacijaDB.Counter}-{nivelacijaDB.DateNivelacije.Year}' po kalkulaciji 'Kalkulacija_{calculationDB.Counter}-{calculationDB.CalculationDate.Year}'"
                        };
                        calculationViewModel.DbContext.Kep.Add(kepNivelacijaDB);
                    }

                    // KEP upis za kalkulaciju
                    var kepCalculationDB = new KepDB
                    {
                        Id = Guid.NewGuid().ToString(),
                        KepDate = calculationDB.CalculationDate,
                        Type = (int)KepStateEnumeration.Kalkulacija,
                        Razduzenje = 0,
                        Zaduzenje = calculationDB.OutputTotalPrice,
                        Description = $"Ručna kalkulacija 'Kalkulacija_{calculationDB.Counter}-{calculationDB.CalculationDate.Year}'"
                    };
                    calculationViewModel.DbContext.Kep.Add(kepCalculationDB);

                    calculationViewModel.DbContext.SaveChanges();

                    // Štampa
                    var supplierGlobal = new SupplierGlobal
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
                    PrinterManager.Instance.PrintInventoryStatus(invertoryGlobals, $"KALKULACIJA_{calculationDB.Counter}-{calculationDB.CalculationDate.Year}", calculationDB.CalculationDate, false, supplierGlobal);

                    // Reset VM
                    calculationViewModel.SuppliersAll = calculationViewModel.DbContext.Suppliers.Select(x => new Supplier(x)).ToList();
                    var items = calculationViewModel.DbContext.Items.ToList();

                    calculationViewModel.InventoryStatusAll = items
                        .Select(x =>
                        {
                            var group = calculationViewModel.DbContext.ItemGroups.Find(x.IdItemGroup);
                            bool isSirovina = group != null && (group.Name.ToLower().Contains("sirovina") || group.Name.ToLower().Contains("sirovine"));
                            return new Invertory(new Item(x), x.IdItemGroup, x.TotalQuantity, 0, x.AlarmQuantity, isSirovina);
                        }).ToList();

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
                catch (Exception ex)
                {
                    Log.Error("SaveCalculationCommand -> Greska prilikom kreiranja nove kalkulacije ->", ex);
                    MessageBox.Show("Greška prilikom kreiranja kalkulacije!", "Greška - kalkulacija", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Running prosečna cena i lager, ali SAMO od datuma kalkulacije pa do poslednjeg računa.
        /// Popravljeno - koristi identičnu logiku kao FixInputPriceCommand ali u užem intervalu!
        /// </summary>
        private void FixInputPriceFromCalculation(
            DateTime kalkulacijaDatum,
            ItemDB itemDB,
            bool isSirovina,
            SqlServerDbContext dbContext,
            decimal novaKolicina,
            decimal novaCena)
        {
            // 1. Nađi poslednju prosečnu cenu i količinu pre kalkulacije
            // Pronađi poslednju kalkulaciju PRE datuma kalkulacije
            var lastCalcBefore = dbContext.CalculationItems
                .Include(ci => ci.Calculation)
                .Where(ci => ci.ItemId == itemDB.Id && ci.Calculation.CalculationDate < kalkulacijaDatum)
                .OrderByDescending(ci => ci.Calculation.CalculationDate)
                .FirstOrDefault();

            // Pronađi poslednji izlaz (račun) PRE datuma kalkulacije
            var lastInvoiceBefore = dbContext.ItemInvoices
                .Include(ii => ii.Invoice)
                .Where(ii => ii.ItemCode == itemDB.Id && ii.Invoice.SdcDateTime != null && ii.Invoice.SdcDateTime.Value < kalkulacijaDatum)
                .OrderByDescending(ii => ii.Invoice.SdcDateTime)
                .FirstOrDefault();

            // Prosečna cena PRE kalkulacije
            decimal prosecnaCena = lastCalcBefore != null
            ? lastCalcBefore.InputPrice
            : (lastInvoiceBefore?.InputUnitPrice ?? (itemDB.InputUnitPrice is decimal d ? d : 0m));

            // Količina PRE kalkulacije
            // Saberi sve ulaze i izlaze pre tog datuma
            decimal totalUlaz = dbContext.CalculationItems
                .Include(ci => ci.Calculation)
                .Where(ci => ci.ItemId == itemDB.Id && ci.Calculation.CalculationDate < kalkulacijaDatum)
                .Sum(ci => ci.Quantity);

            decimal totalIzlaz = dbContext.ItemInvoices
                .Include(ii => ii.Invoice)
                .Where(ii => ii.ItemCode == itemDB.Id && ii.Invoice.SdcDateTime != null && ii.Invoice.SdcDateTime.Value < kalkulacijaDatum)
                .Sum(ii => ii.Quantity ?? 0);

            decimal pocetnoStanje = 0;
            var ps = dbContext.PocetnaStanja
                .OrderByDescending(x => x.PopisDate)
                .FirstOrDefault(x => x.PopisDate.Year == kalkulacijaDatum.Year);

            if (ps != null)
            {
                var psi = dbContext.PocetnaStanjaItems
                    .FirstOrDefault(x => x.IdPocetnoStanje == ps.Id && x.IdItem == itemDB.Id);
                if (psi != null)
                {
                    pocetnoStanje = psi.NewQuantity;
                }
            }
            // Trenutna količina pre kalkulacije
            decimal runningKolicina = pocetnoStanje + totalUlaz - totalIzlaz;

            // 2. Skupi sve kalkulacije i izlaze OD datuma kalkulacije pa nadalje
            var kalkulacije = dbContext.CalculationItems
                .Include(ci => ci.Calculation)
                .Where(ci => ci.ItemId == itemDB.Id && ci.Calculation.CalculationDate >= kalkulacijaDatum)
                .OrderBy(ci => ci.Calculation.CalculationDate)
                .ToList();

            var sviIzlazi = dbContext.ItemInvoices
                .Include(ii => ii.Invoice)
                .Where(ii => ii.ItemCode == itemDB.Id && ii.Invoice.SdcDateTime != null && ii.Invoice.SdcDateTime.Value >= kalkulacijaDatum)
                .OrderBy(ii => ii.Invoice.SdcDateTime)
                .ToList();

            // 3. Sastavi listu događaja, uključujući i trenutni ulaz
            var dogadjaji = kalkulacije
                .Select(k => new
                {
                    datum = k.Calculation.CalculationDate,
                    tip = "ulaz",
                    kolicina = k.Quantity,
                    cena = k.InputPrice,
                    invoiceId = (string)null,
                    id = (int?)null
                })
                .Concat(
                    sviIzlazi.Select(e => new
                    {
                        datum = e.Invoice.SdcDateTime ?? kalkulacijaDatum,
                        tip = "izlaz",
                        kolicina = e.Quantity ?? 0,
                        cena = prosecnaCena,
                        invoiceId = string.IsNullOrEmpty(e.InvoiceId) ? null : e.InvoiceId,
                        id = (int?)e.Id
                    })
                )
                .ToList();

            // Ručno dodaj trenutnu kalkulaciju kao dogadjaj
            dogadjaji.Add(new
            {
                datum = kalkulacijaDatum,
                tip = "ulaz",
                kolicina = novaKolicina,
                cena = novaCena / novaKolicina,
                invoiceId = (string)null,
                id = (int?)null
            });

            // Sortiraj celu listu po datumu
            dogadjaji = dogadjaji.OrderBy(d => d.datum).ToList();

            // 3. Running cena i update ItemInvoice
            foreach (var dogadjaj in dogadjaji)
            {
                if (dogadjaj.tip == "izlaz")
                {
                    runningKolicina -= dogadjaj.kolicina;
                    var itemInvoice = sviIzlazi.FirstOrDefault(ii => ii.Id == dogadjaj.id && ii.InvoiceId == dogadjaj.invoiceId);
                    if (itemInvoice != null)
                    {
                        itemInvoice.InputUnitPrice = prosecnaCena;
                        if (isSirovina)
                        {
                            itemInvoice.UnitPrice = prosecnaCena;
                            itemInvoice.OriginalUnitPrice = prosecnaCena;
                            if (itemInvoice.Quantity.HasValue)
                                itemInvoice.TotalAmout = Decimal.Round(prosecnaCena * itemInvoice.Quantity.Value, 2);
                        }
                        dbContext.ItemInvoices.Update(itemInvoice);
                    }
                }
                else if (dogadjaj.tip == "ulaz")
                {
                    decimal kolicinaPreUlaza = runningKolicina;
                    decimal brojilac, imenilac;
                    if (kolicinaPreUlaza <= 0)
                    {
                        brojilac = dogadjaj.cena * dogadjaj.kolicina;
                        imenilac = dogadjaj.kolicina;
                    }
                    else
                    {
                        brojilac = prosecnaCena * kolicinaPreUlaza + dogadjaj.cena * dogadjaj.kolicina;
                        imenilac = kolicinaPreUlaza + dogadjaj.kolicina;
                    }
                    prosecnaCena = SafeProsecnaCena(brojilac, imenilac, itemDB.Id, itemDB.Name, dogadjaj.datum);
                    runningKolicina = kolicinaPreUlaza + dogadjaj.kolicina;
                }
            }

            itemDB.InputUnitPrice = prosecnaCena;
            dbContext.Items.Update(itemDB);
            dbContext.SaveChanges();
        }

        private void AddNivelacijaItem(Item item, ItemDB itemDB, Models.AppMain.Statistic.Nivelacija nivelacija, ref decimal nivelacijaTotal)
        {
            var nivelacijaItem = new NivelacijaItem(_dbContext, item);
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
            _dbContext.ItemsNivelacija.Add(itemNivelacijaDB);

            itemDB.SellingUnitPrice = nivelacijaItem.NewPrice;
            _dbContext.Items.Update(itemDB);

            _dbContext.SaveChanges();

            nivelacijaTotal += (itemNivelacijaDB.NewUnitPrice - itemNivelacijaDB.OldUnitPrice) * itemNivelacijaDB.TotalQuantity;
            Log.Debug($"SaveCommand - AddNivelacija - Uspesno sacuvan artikal {item.Id} za nivelaciju {nivelacija.Id}");
        }

        private decimal SafeProsecnaCena(decimal deljenik, decimal delilac, string itemId = "", string itemName = "", DateTime? datum = null)
        {
            if (delilac <= 0)
            {
                Log.Error($"SaveCommand - SafeProsecnaCena - Negativno stanje za artikal {itemId} ({itemName}) na datumu {(datum.HasValue ? datum.Value.ToString("yyyy-MM-dd") : "")}. Prosečna cena postavljena na 0.");
                return 0;
            }
            var cena = Decimal.Round(deljenik / delilac, 2);
            return cena < 0 ? 0 : cena;
        }
    }
}