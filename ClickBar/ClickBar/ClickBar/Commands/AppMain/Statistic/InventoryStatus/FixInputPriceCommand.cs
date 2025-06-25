using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar_DatabaseSQLManager.Models;
using ClickBar_DatabaseSQLManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows;
using ClickBar_Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace ClickBar.Commands.AppMain.Statistic.InventoryStatus
{
    public class FixInputPriceCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private InventoryStatusViewModel _viewModel;

        public FixInputPriceCommand(InventoryStatusViewModel currentViewModel)
        {
            _viewModel = currentViewModel;
        }

        public bool CanExecute(object parameter) => true;

        public async void Execute(object parameter)
        {
            try
            {
                var pocetnoStanjeDB = _viewModel.DbContext.PocetnaStanja?
                    .Where(p => p.PopisDate.Year == DateTime.Now.Year)
                    .OrderByDescending(p => p.PopisDate)
                    .FirstOrDefault();

                IQueryable<ItemDB> items = _viewModel.DbContext.Items;

                if (_viewModel.CurrentSupergroupSearch != null &&
                    _viewModel.CurrentSupergroupSearch.Id != -1)
                {
                    items = items.Join(_viewModel.DbContext.ItemGroups,
                        item => item.IdItemGroup,
                        itemGroup => itemGroup.Id,
                        (item, itemGroup) => new { Item = item, ItemGroup = itemGroup })
                        .Where(x => x.ItemGroup.IdSupergroup == _viewModel.CurrentSupergroupSearch.Id)
                        .Select(x => x.Item);
                }

                var itemList = items.ToList();
                var pocetnaStanjaItems = _viewModel.DbContext.PocetnaStanjaItems.ToList();
                var pocetnoStanjeId = pocetnoStanjeDB?.Id;
                var pocetnoStanjeDate = pocetnoStanjeDB?.PopisDate ?? new DateTime(DateTime.Now.Year, 1, 1);

                Parallel.ForEach(itemList, itemDB =>
                {
                    try
                    {
                        using (var dbContext = _viewModel.ServiceProvider.GetRequiredService<IDbContextFactory<SqlServerDbContext>>().CreateDbContext())
                        {
                            decimal quantityPocetnoStanje = 0;
                            decimal inputPricePocetnoStanje = 0;
                            DateTime psDate = pocetnoStanjeDate;

                            if (pocetnoStanjeId != null)
                            {
                                var psi = pocetnaStanjaItems
                                    .FirstOrDefault(p => p.IdItem == itemDB.Id && p.IdPocetnoStanje == pocetnoStanjeId);
                                if (psi != null)
                                {
                                    quantityPocetnoStanje = psi.NewQuantity;
                                    inputPricePocetnoStanje = psi.InputPrice;
                                }
                            }
                            FixInputPriceThreaded(psDate, itemDB.Id, quantityPocetnoStanje, inputPricePocetnoStanje, dbContext);
                        }
                    }
                    catch (Exception exItem)
                    {
                        Log.Error($"FixInputPriceCommand -> Item: {itemDB.Id} - {itemDB.Name}", exItem);
                    }
                });

                MessageBox.Show("Uspešno sređivanje prosečnih ulaznih cena!",
                    "Uspešno",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Log.Error("FixInputPriceCommand -> Execute -> Greška prilikom popravke ulaznih cena: ", ex);
                MessageBox.Show("Greška prilikom popravke ulaznih cena!", "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Helper metoda koja NIKAD ne vraća negativnu cenu. Ako je količina nula ili negativna, vraća 0 i loguje alarm.
        /// </summary>
        private static decimal SafeProsecnaCena(decimal deljenik, decimal delilac, string itemId = "", string itemName = "", DateTime? datum = null)
        {
            if (delilac <= 0)
            {
                Log.Error($"Negativno stanje za artikal {itemId} ({itemName}) na datumu {(datum.HasValue ? datum.Value.ToString("yyyy-MM-dd") : "")}. Prosečna cena postavljena na 0.");
                return 0;
            }
            var cena = Decimal.Round(deljenik / delilac, 2);
            return cena < 0 ? 0 : cena;
        }

        private static void FixInputPriceThreaded(DateTime pocetniDatum, string itemId, decimal popisQuantity, decimal popisInputPrice, SqlServerDbContext dbContext)
        {
            var itemDB = dbContext.Items.Find(itemId);
            if (itemDB == null)
                return;

            var proknjizeniPazari = dbContext.KnjizenjePazara
                .Join(dbContext.Invoices,
                    knjizenje => knjizenje.Id,
                    invoice => invoice.KnjizenjePazaraId,
                    (knjizenje, invoice) => new { Knji = knjizenje, Invoice = invoice })
                .Where(pazar => pazar.Knji.IssueDateTime.Date > pocetniDatum.Date)
                .Join(dbContext.ItemInvoices,
                    invoice => invoice.Invoice.Id,
                    invoiceItem => invoiceItem.InvoiceId,
                    (invoice, invoiceItem) => new { KnjizenjeInvoice = invoice, InvItem = invoiceItem })
                .Where(pazar => pazar.KnjizenjeInvoice.Invoice.SdcDateTime != null &&
                                pazar.KnjizenjeInvoice.Invoice.SdcDateTime.HasValue &&
                                pazar.KnjizenjeInvoice.Invoice.SdcDateTime.Value.Date > pocetniDatum.Date &&
                                pazar.InvItem.ItemCode == itemDB.Id)
                .OrderBy(item => item.KnjizenjeInvoice.Knji.IssueDateTime)
                .ToList();

            var neproknjizeniPazari = dbContext.Invoices
                .Join(dbContext.ItemInvoices,
                    invoice => invoice.Id,
                    invoiceItem => invoiceItem.InvoiceId,
                    (invoice, invoiceItem) => new { Inv = invoice, InvItem = invoiceItem })
                .Where(pazar => pazar.Inv.SdcDateTime != null &&
                                pazar.Inv.SdcDateTime.HasValue &&
                                pazar.Inv.SdcDateTime.Value.Date > pocetniDatum.Date &&
                                string.IsNullOrEmpty(pazar.Inv.KnjizenjePazaraId) &&
                                pazar.InvItem.ItemCode == itemDB.Id)
                .OrderBy(item => item.Inv.SdcDateTime)
                .ToList();

            var kalkulacije = dbContext.Calculations
                .Join(dbContext.CalculationItems,
                    calculacion => calculacion.Id,
                    calculationItem => calculationItem.CalculationId,
                    (calculacion, calculationItem) => new { Cal = calculacion, CalItem = calculationItem })
                .Where(kal => kal.Cal.CalculationDate >= pocetniDatum.Date &&
                              kal.CalItem.ItemId == itemDB.Id)
                .OrderBy(item => item.Cal.CalculationDate)
                .ToList();

            decimal prosecnaCena = popisInputPrice;
            decimal trenutnaKolicina = popisQuantity;
            DateTime pocetak = pocetniDatum;

            foreach (var kal in kalkulacije)
            {
                // Saberemo sve prodaje/izlaze od 'pocetak' do trenutne kalkulacije
                decimal izlazUKalkulaciji = 0;

                izlazUKalkulaciji += proknjizeniPazari
                    .Where(pazar => pazar.KnjizenjeInvoice.Invoice.SdcDateTime.HasValue &&
                                    pazar.KnjizenjeInvoice.Invoice.SdcDateTime.Value.Date >= pocetak.Date &&
                                    pazar.KnjizenjeInvoice.Invoice.SdcDateTime.Value.Date < kal.Cal.CalculationDate.Date)
                    .Sum(p =>
                        (p.KnjizenjeInvoice.Invoice.TransactionType.HasValue &&
                         p.KnjizenjeInvoice.Invoice.TransactionType.Value == 0 &&
                         p.InvItem.Quantity.HasValue ? p.InvItem.Quantity.Value : 0)
                        -
                        (p.KnjizenjeInvoice.Invoice.TransactionType.HasValue &&
                         p.KnjizenjeInvoice.Invoice.TransactionType.Value == 1 &&
                         p.InvItem.Quantity.HasValue ? p.InvItem.Quantity.Value : 0)
                    );

                izlazUKalkulaciji += neproknjizeniPazari
                    .Where(pazar => pazar.Inv.SdcDateTime.HasValue &&
                                    pazar.Inv.SdcDateTime.Value.Date >= pocetak.Date &&
                                    pazar.Inv.SdcDateTime.Value.Date < kal.Cal.CalculationDate.Date)
                    .Sum(p =>
                        (p.Inv.TransactionType.HasValue &&
                         p.Inv.TransactionType.Value == 0 &&
                         p.InvItem.Quantity.HasValue ? p.InvItem.Quantity.Value : 0)
                        -
                        (p.Inv.TransactionType.HasValue &&
                         p.Inv.TransactionType.Value == 1 &&
                         p.InvItem.Quantity.HasValue ? p.InvItem.Quantity.Value : 0)
                    );

                decimal kolicinaPreUlaza = trenutnaKolicina - izlazUKalkulaciji;

                decimal brojilac, imenilac;

                if (kolicinaPreUlaza <= 0)
                {
                    brojilac = kal.CalItem.InputPrice * kal.CalItem.Quantity;
                    imenilac = kal.CalItem.Quantity;
                }
                else
                {
                    brojilac = prosecnaCena * kolicinaPreUlaza + kal.CalItem.InputPrice * kal.CalItem.Quantity;
                    imenilac = kolicinaPreUlaza + kal.CalItem.Quantity;
                }

                prosecnaCena = SafeProsecnaCena(brojilac, imenilac, itemDB.Id, itemDB.Name, kal.Cal.CalculationDate);

                // Sada ažuriraj stanje za sledeću iteraciju
                trenutnaKolicina = kolicinaPreUlaza + kal.CalItem.Quantity;

                // Ažuriraj sve item-invoice u ovom periodu
                proknjizeniPazari
                    .Where(pazar => pazar.KnjizenjeInvoice.Invoice.SdcDateTime.HasValue &&
                                    pazar.KnjizenjeInvoice.Invoice.SdcDateTime.Value.Date >= pocetak.Date &&
                                    pazar.KnjizenjeInvoice.Invoice.SdcDateTime.Value.Date < kal.Cal.CalculationDate.Date)
                    .ToList()
                    .ForEach(i =>
                    {
                        if (i.InvItem.IsSirovina == 1 && i.InvItem.Quantity.HasValue)
                        {
                            i.InvItem.UnitPrice = prosecnaCena;
                            i.InvItem.OriginalUnitPrice = prosecnaCena;
                            i.InvItem.TotalAmout = Decimal.Round(prosecnaCena * i.InvItem.Quantity.Value, 2);
                        }
                        i.InvItem.InputUnitPrice = prosecnaCena;
                        dbContext.ItemInvoices.Update(i.InvItem);
                        dbContext.SaveChanges();
                    });

                neproknjizeniPazari
                    .Where(pazar => pazar.Inv.SdcDateTime.HasValue &&
                                    pazar.Inv.SdcDateTime.Value.Date >= pocetak.Date &&
                                    pazar.Inv.SdcDateTime.Value.Date < kal.Cal.CalculationDate.Date)
                    .ToList()
                    .ForEach(i =>
                    {
                        if (i.InvItem.IsSirovina == 1 && i.InvItem.Quantity.HasValue)
                        {
                            i.InvItem.UnitPrice = prosecnaCena;
                            i.InvItem.OriginalUnitPrice = prosecnaCena;
                            i.InvItem.TotalAmout = Decimal.Round(prosecnaCena * i.InvItem.Quantity.Value, 2);
                        }
                        i.InvItem.InputUnitPrice = prosecnaCena;
                        dbContext.ItemInvoices.Update(i.InvItem);
                        dbContext.SaveChanges();
                    });

                pocetak = kal.Cal.CalculationDate;
            }

            // Poslednji interval nakon svih kalkulacija
            proknjizeniPazari
                .Where(pazar => pazar.KnjizenjeInvoice.Invoice.SdcDateTime.HasValue &&
                                pazar.KnjizenjeInvoice.Invoice.SdcDateTime.Value.Date >= pocetak.Date)
                .ToList()
                .ForEach(i =>
                {
                    if (i.InvItem.IsSirovina == 1 && i.InvItem.Quantity.HasValue)
                    {
                        i.InvItem.UnitPrice = prosecnaCena;
                        i.InvItem.OriginalUnitPrice = prosecnaCena;
                        i.InvItem.TotalAmout = Decimal.Round(prosecnaCena * i.InvItem.Quantity.Value, 2);
                    }
                    i.InvItem.InputUnitPrice = prosecnaCena;
                    dbContext.ItemInvoices.Update(i.InvItem);
                    dbContext.SaveChanges();
                });

            neproknjizeniPazari
                .Where(pazar => pazar.Inv.SdcDateTime.HasValue &&
                                pazar.Inv.SdcDateTime.Value.Date >= pocetak.Date)
                .ToList()
                .ForEach(i =>
                {
                    if (i.InvItem.IsSirovina == 1 && i.InvItem.Quantity.HasValue)
                    {
                        i.InvItem.UnitPrice = prosecnaCena;
                        i.InvItem.OriginalUnitPrice = prosecnaCena;
                        i.InvItem.TotalAmout = Decimal.Round(prosecnaCena * i.InvItem.Quantity.Value, 2);
                    }
                    i.InvItem.InputUnitPrice = prosecnaCena;
                    dbContext.ItemInvoices.Update(i.InvItem);
                    dbContext.SaveChanges();
                });

            itemDB.InputUnitPrice = prosecnaCena;
            dbContext.Items.Update(itemDB);
            dbContext.SaveChanges();
        }
    }
}