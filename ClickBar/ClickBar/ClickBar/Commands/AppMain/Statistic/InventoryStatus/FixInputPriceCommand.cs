using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar_DatabaseSQLManager.Models;
using ClickBar_DatabaseSQLManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using ClickBar_Logging;
using ClickBar.Models.AppMain.Statistic;
using Microsoft.EntityFrameworkCore;

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

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            try
            {
                PocetnoStanjeDB? pocetnoStanjeDB = null;

                if (_viewModel.DbContext.PocetnaStanja != null &&
                    _viewModel.DbContext.PocetnaStanja.Any())
                {
                    pocetnoStanjeDB = _viewModel.DbContext.PocetnaStanja
                        .Where(p => p.PopisDate.Year == DateTime.Now.Year)
                        .OrderByDescending(p => p.PopisDate).FirstOrDefault();
                }

                IQueryable<ItemDB> items = _viewModel.DbContext.Items;

                if (_viewModel.CurrentSupergroupSearch != null &&
                    _viewModel.CurrentSupergroupSearch.Id != -1)
                {
                    items = items.Join(_viewModel.DbContext.ItemGroups,
                        item => item.IdItemGroup,
                        itemGroup => itemGroup.Id,
                        (item, itemGroup) => new { Item = item, ItemGroup = itemGroup })
                        .Where(item => item.ItemGroup.IdSupergroup == _viewModel.CurrentSupergroupSearch.Id)
                        .Select(item => item.Item);
                }

                await items.ForEachAsync(itemDB =>
                {
                    if (itemDB.Id == "000024")
                    {
                        int a = 2;
                    }

                    decimal quantityPocetnoStanje = 0;
                    decimal inputPricePocetnoStanje = 0;

                    DateTime pocetnoStanjeDate = new DateTime(DateTime.Now.Year, 1, 1, 0, 0, 0);

                    if (pocetnoStanjeDB != null)
                    {
                        pocetnoStanjeDate = pocetnoStanjeDB.PopisDate;

                        quantityPocetnoStanje = _viewModel.DbContext.PocetnaStanjaItems
                            .Where(p => p.IdItem == itemDB.Id && p.IdPocetnoStanje == pocetnoStanjeDB.Id)
                            .Select(p => p.NewQuantity)
                            .FirstOrDefault();

                        inputPricePocetnoStanje = _viewModel.DbContext.PocetnaStanjaItems
                            .Where(p => p.IdItem == itemDB.Id && p.IdPocetnoStanje == pocetnoStanjeDB.Id)
                            .Select(p => p.InputPrice)
                            .FirstOrDefault();
                    }

                    FixInputPrice(pocetnoStanjeDate,
                        itemDB,
                        quantityPocetnoStanje,
                        inputPricePocetnoStanje);
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
        private async void FixInputPrice(DateTime pocetniDatum,
            ItemDB itemDB,
            decimal popisQuantity,
            decimal popisInputPrice)
        {
            var proknjizeniPazari = _viewModel.DbContext.KnjizenjePazara.Join(_viewModel.DbContext.Invoices,
            knjizenje => knjizenje.Id,
            invoice => invoice.KnjizenjePazaraId,
            (knjizenje, invoice) => new { Knji = knjizenje, Invoice = invoice })
            .Where(pazar => pazar.Knji.IssueDateTime.Date > pocetniDatum.Date)
            .Join(_viewModel.DbContext.ItemInvoices,
            invoice => invoice.Invoice.Id,
            invoiceItem => invoiceItem.InvoiceId,
            (invoice, invoiceItem) => new { KnjizenjeInvoice = invoice, InvItem = invoiceItem })
            .Where(pazar => pazar.KnjizenjeInvoice.Invoice.SdcDateTime != null &&
            pazar.KnjizenjeInvoice.Invoice.SdcDateTime.HasValue &&
            pazar.KnjizenjeInvoice.Invoice.SdcDateTime.Value.Date > pocetniDatum.Date &&
            pazar.InvItem.ItemCode == itemDB.Id)
            .OrderBy(item => item.KnjizenjeInvoice.Knji.IssueDateTime);

            var neproknjizeniPazari = _viewModel.DbContext.Invoices.Join(_viewModel.DbContext.ItemInvoices,
                invoice => invoice.Id,
                invoiceItem => invoiceItem.InvoiceId,
                (invoice, invoiceItem) => new { Inv = invoice, InvItem = invoiceItem })
                .Where(pazar => pazar.Inv.SdcDateTime != null &&
                pazar.Inv.SdcDateTime.HasValue &&
                pazar.Inv.SdcDateTime.Value.Date > pocetniDatum.Date &&
                string.IsNullOrEmpty(pazar.Inv.KnjizenjePazaraId) &&
                pazar.InvItem.ItemCode == itemDB.Id)
                .OrderBy(item => item.Inv.SdcDateTime);

            var kalkulacije = _viewModel.DbContext.Calculations.Join(_viewModel.DbContext.CalculationItems,
                calculacion => calculacion.Id,
                calculationItem => calculationItem.CalculationId,
                (calculacion, calculationItem) => new { Cal = calculacion, CalItem = calculationItem })
                .Where(kal => kal.Cal.CalculationDate >= pocetniDatum.Date &&
                kal.CalItem.ItemId == itemDB.Id)
                .OrderBy(item => item.Cal.CalculationDate);

            decimal prosecnaCena = popisInputPrice;
            decimal trenutnaKolicina = popisQuantity;
            DateTime pocetak = pocetniDatum;

            if (kalkulacije != null &&
                kalkulacije.Any())
            {
                foreach (var kal in kalkulacije)
                {
                    decimal proknjizenPazarDoKalkulacijeQuantity = 0;
                    decimal neproknjizenPazarDoKalkulacijeQuantity = 0;

                    var proknjizeniPazariDoKalkulacije = proknjizeniPazari.Where(pazar => pazar.KnjizenjeInvoice.Invoice.SdcDateTime.HasValue &&
                    pazar.KnjizenjeInvoice.Invoice.SdcDateTime.Value.Date >= pocetak.Date &&
                    pazar.KnjizenjeInvoice.Invoice.SdcDateTime.Value.Date < kal.Cal.CalculationDate.Date);

                    if (proknjizeniPazariDoKalkulacije != null &&
                    proknjizeniPazariDoKalkulacije.Any())
                    {
                        decimal prodaja = proknjizeniPazariDoKalkulacije.Select(p => p.KnjizenjeInvoice.Invoice.TransactionType.HasValue &&
                        p.KnjizenjeInvoice.Invoice.TransactionType.Value == 0 &&
                        p.InvItem.Quantity.HasValue ?
                        p.InvItem.Quantity.Value : 0).Sum();

                        decimal refundacija = proknjizeniPazariDoKalkulacije.Select(r => r.KnjizenjeInvoice.Invoice.TransactionType.HasValue &&
                        r.KnjizenjeInvoice.Invoice.TransactionType.Value == 1 &&
                        r.InvItem.Quantity.HasValue ?
                        r.InvItem.Quantity.Value : 0).Sum();

                        proknjizenPazarDoKalkulacijeQuantity = prodaja - refundacija;

                        foreach (var i in proknjizeniPazariDoKalkulacije)
                        {
                            if (i.InvItem.IsSirovina == 1 &&
                                i.InvItem.Quantity.HasValue)
                            {
                                i.InvItem.UnitPrice = prosecnaCena;
                                i.InvItem.OriginalUnitPrice = prosecnaCena;
                                i.InvItem.TotalAmout = Decimal.Round(prosecnaCena * i.InvItem.Quantity.Value, 2);
                            }

                            i.InvItem.InputUnitPrice = prosecnaCena;
                            _viewModel.DbContext.ItemInvoices.Update(i.InvItem);
                            _viewModel.DbContext.SaveChanges();
                        }
                    }

                    var neproknjizeniPazariDoKalkulacije = neproknjizeniPazari.Where(pazar => pazar.Inv.SdcDateTime.HasValue &&
                    pazar.Inv.SdcDateTime.Value.Date >= pocetak.Date &&
                    pazar.Inv.SdcDateTime.Value.Date < kal.Cal.CalculationDate.Date);

                    if (neproknjizeniPazariDoKalkulacije != null &&
                    neproknjizeniPazariDoKalkulacije.Any())
                    {
                        decimal prodaja = neproknjizeniPazariDoKalkulacije.Select(p => p.Inv.TransactionType.HasValue &&
                        p.Inv.TransactionType.Value == 0 &&
                        p.InvItem.Quantity.HasValue ?
                        p.InvItem.Quantity.Value : 0).Sum();

                        decimal refundacija = neproknjizeniPazariDoKalkulacije.Select(r => r.Inv.TransactionType.HasValue &&
                        r.Inv.TransactionType.Value == 1 &&
                        r.InvItem.Quantity.HasValue ?
                        r.InvItem.Quantity.Value : 0).Sum();

                        neproknjizenPazarDoKalkulacijeQuantity = prodaja - refundacija;

                        foreach (var i in neproknjizeniPazariDoKalkulacije)
                        {
                            if (i.InvItem.IsSirovina == 1 &&
                                i.InvItem.Quantity.HasValue)
                            {
                                i.InvItem.UnitPrice = prosecnaCena;
                                i.InvItem.OriginalUnitPrice = prosecnaCena;
                                i.InvItem.TotalAmout = Decimal.Round(prosecnaCena * i.InvItem.Quantity.Value, 2);
                            }

                            i.InvItem.InputUnitPrice = prosecnaCena;
                            _viewModel.DbContext.ItemInvoices.Update(i.InvItem);
                            _viewModel.DbContext.SaveChanges();
                        }
                    }

                    decimal pazarDoKalkulacijeQuantity = proknjizenPazarDoKalkulacijeQuantity + neproknjizenPazarDoKalkulacijeQuantity;

                    decimal deljenik = (kal.CalItem.InputPrice * kal.CalItem.Quantity) + (trenutnaKolicina - pazarDoKalkulacijeQuantity) * prosecnaCena;
                    decimal delilac = kal.CalItem.Quantity + (trenutnaKolicina - pazarDoKalkulacijeQuantity);

                    prosecnaCena = Decimal.Round(deljenik / delilac, 2);

                    pocetak = kal.Cal.CalculationDate;
                }
            }

            var proknjizeni = proknjizeniPazari.Where(pazar => pazar.KnjizenjeInvoice.Invoice.SdcDateTime.HasValue &&
            pazar.KnjizenjeInvoice.Invoice.SdcDateTime.Value.Date >= pocetak.Date);

            if (proknjizeni != null &&
                    proknjizeni.Any())
            {
                foreach (var i in proknjizeni)
                {
                    if (i.InvItem.IsSirovina == 1 &&
                        i.InvItem.Quantity.HasValue)
                    {
                        i.InvItem.UnitPrice = prosecnaCena;
                        i.InvItem.OriginalUnitPrice = prosecnaCena;
                        i.InvItem.TotalAmout = Decimal.Round(prosecnaCena * i.InvItem.Quantity.Value, 2);
                    }

                    i.InvItem.InputUnitPrice = prosecnaCena;
                    _viewModel.DbContext.ItemInvoices.Update(i.InvItem);
                    _viewModel.DbContext.SaveChanges();
                }
            }

            var neproknjizeni = neproknjizeniPazari.Where(pazar => pazar.Inv.SdcDateTime.HasValue &&
            pazar.Inv.SdcDateTime.Value.Date >= pocetak.Date);

            if (neproknjizeni != null &&
            neproknjizeni.Any())
            {
                foreach (var i in neproknjizeni)
                {
                    if (i.InvItem.IsSirovina == 1 &&
                        i.InvItem.Quantity.HasValue)
                    {
                        i.InvItem.UnitPrice = prosecnaCena;
                        i.InvItem.OriginalUnitPrice = prosecnaCena;
                        i.InvItem.TotalAmout = Decimal.Round(prosecnaCena * i.InvItem.Quantity.Value, 2);
                    }

                    i.InvItem.InputUnitPrice = prosecnaCena;
                    _viewModel.DbContext.ItemInvoices.Update(i.InvItem);
                    _viewModel.DbContext.SaveChanges();
                }
            }

            itemDB.InputUnitPrice = prosecnaCena;
            _viewModel.DbContext.Items.Update(itemDB);
            _viewModel.DbContext.SaveChanges();
        }
        private async void StarijaKalkulacija(DateTime pocetniDatum,
            Invertory calculationItem,
            ItemDB itemDB,
            bool isSirovina,
            decimal popisQuantity,
            decimal popisInputPrice)
        {
            decimal qunatityPazari = 0;
            decimal qunatityNivelacija = 0;
            decimal qunatityKalkulacija = 0;
            decimal unitPrice = itemDB.InputUnitPrice != null && itemDB.InputUnitPrice.HasValue ? itemDB.InputUnitPrice.Value : 0;

            if (itemDB.TotalQuantity != 0 &&
                unitPrice == 0)
            {
                unitPrice = await SrediProsecnuCenu(itemDB, isSirovina);
            }
            var proknjizeniPazari = _viewModel.DbContext.KnjizenjePazara.Join(_viewModel.DbContext.Invoices,
            knjizenje => knjizenje.Id,
            invoice => invoice.KnjizenjePazaraId,
            (knjizenje, invoice) => new { Knji = knjizenje, Invoice = invoice })
            .Where(pazar => pazar.Knji.IssueDateTime.Date > pocetniDatum.Date)
            .Join(_viewModel.DbContext.ItemInvoices,
            invoice => invoice.Invoice.Id,
            invoiceItem => invoiceItem.InvoiceId,
            (invoice, invoiceItem) => new { KnjizenjeInvoice = invoice, InvItem = invoiceItem })
            .Where(pazar => pazar.KnjizenjeInvoice.Invoice.SdcDateTime != null &&
            pazar.KnjizenjeInvoice.Invoice.SdcDateTime.HasValue &&
            pazar.KnjizenjeInvoice.Invoice.SdcDateTime.Value.Date > pocetniDatum.Date &&
            pazar.InvItem.ItemCode == itemDB.Id)
            .OrderByDescending(item => item.KnjizenjeInvoice.Knji.IssueDateTime);

            var neproknjizeniPazari = _viewModel.DbContext.Invoices.Join(_viewModel.DbContext.ItemInvoices,
                invoice => invoice.Id,
                invoiceItem => invoiceItem.InvoiceId,
                (invoice, invoiceItem) => new { Inv = invoice, InvItem = invoiceItem })
                .Where(pazar => pazar.Inv.SdcDateTime != null &&
                pazar.Inv.SdcDateTime.HasValue &&
                pazar.Inv.SdcDateTime.Value.Date > pocetniDatum.Date &&
                string.IsNullOrEmpty(pazar.Inv.KnjizenjePazaraId) &&
                pazar.InvItem.ItemCode == itemDB.Id)
                .OrderByDescending(item => item.Inv.SdcDateTime);

            var kalkulacije = _viewModel.DbContext.Calculations.Join(_viewModel.DbContext.CalculationItems,
                calculacion => calculacion.Id,
                calculationItem => calculationItem.CalculationId,
                (calculacion, calculationItem) => new { Cal = calculacion, CalItem = calculationItem })
                .Where(kal => kal.Cal.CalculationDate >= pocetniDatum.Date &&
                kal.CalItem.ItemId == itemDB.Id)
                .OrderByDescending(item => item.Cal.CalculationDate);

            List<string> neproknjizeni = new List<string>();
            List<string> kalkulacijeOdradjene = new List<string>();

            if (proknjizeniPazari != null &&
                proknjizeniPazari.Any())
            {
                await proknjizeniPazari.ForEachAsync(async prPazar =>
                {
                    var neproknjizeniPazariForDate = neproknjizeniPazari.Where(paz => paz.Inv.SdcDateTime.HasValue &&
                    prPazar.KnjizenjeInvoice.Invoice.SdcDateTime.HasValue &&
                    paz.Inv.SdcDateTime.Value.Date <= prPazar.KnjizenjeInvoice.Invoice.SdcDateTime.Value.Date);

                    if (neproknjizeniPazariForDate != null &&
                    neproknjizeniPazariForDate.Any())
                    {
                        await neproknjizeniPazariForDate.ForEachAsync(paz =>
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
                            await kalkulacijeForDate.ForEachAsync(kal =>
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
                                        decimal quantityTrenutno = Math.Abs(itemDB.TotalQuantity) + qunatityPazari - qunatityKalkulacija;
                                        decimal quantityUkupno = Math.Abs(itemDB.TotalQuantity) + qunatityPazari - qunatityKalkulacija - kal.CalItem.Quantity;

                                        decimal delilac = (quantityTrenutno * unitPrice) - kal.CalItem.InputPrice * kal.CalItem.Quantity;

                                        if (delilac == 0 ||
                                        quantityUkupno == 0)
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
                await neproknjizeniPazari.ForEachAsync(async pazar =>
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
                            await kalkulacijeForDate.ForEachAsync(kal =>
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

            decimal quantityTotal = itemDB.TotalQuantity + popisQuantity + qunatityPazari - qunatityKalkulacija;
            decimal deljenik = Math.Abs(quantityTotal) * unitPrice + (calculationItem.InputPrice);
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

                await proknjizeniPazari.ForEachAsync(async prPazar =>
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

                            await kalkulacijeForDate.ForEachAsync(kal =>
                            {
                                if (!kalkulacijeOdradjene.Any() ||
                                !kalkulacijeOdradjene.Contains(kal.Cal.Id))
                                {
                                    decimal delilac = Math.Abs(quantityTotal) * itemDB.InputUnitPrice.Value + kal.CalItem.Quantity * kal.CalItem.InputPrice;
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

                            await neproknjizeniPazariForDate.ForEachAsync(paz =>
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
                                        _viewModel.DbContext.ItemInvoices.Update(paz.InvItem);

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
                        _viewModel.DbContext.ItemInvoices.Update(prPazar.InvItem);
                    }
                });
            }
            if (neproknjizeniPazari != null &&
                neproknjizeniPazari.Any())
            {
                neproknjizeniPazari = neproknjizeniPazari.OrderBy(paz => paz.Inv.SdcDateTime);

                await neproknjizeniPazari.ForEachAsync(async pazar =>
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

                            await kalkulacijeForDate.ForEachAsync(kal =>
                            {
                                if (!kalkulacijeOdradjene.Any() ||
                                !kalkulacijeOdradjene.Contains(kal.Cal.Id))
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
                        _viewModel.DbContext.ItemInvoices.Update(pazar.InvItem);
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
                });

                _viewModel.DbContext.Items.Update(itemDB);
                _viewModel.DbContext.SaveChanges();
            }
        }
        private async Task<decimal> SrediProsecnuCenu(ItemDB itemDB,
            bool isSirovina)
        {
            decimal prosecnaCena = 0;
            decimal quantity = 0;
            var proknjizeniPazari = _viewModel.DbContext.KnjizenjePazara.Join(_viewModel.DbContext.Invoices,
            knjizenje => knjizenje.Id,
            invoice => invoice.KnjizenjePazaraId,
            (knjizenje, invoice) => new { Knji = knjizenje, Invoice = invoice })
            .Join(_viewModel.DbContext.ItemInvoices,
            invoice => invoice.Invoice.Id,
            invoiceItem => invoiceItem.InvoiceId,
            (invoice, invoiceItem) => new { KnjizenjeInvoice = invoice, InvItem = invoiceItem })
            .Where(pazar => pazar.KnjizenjeInvoice.Invoice.SdcDateTime != null &&
            pazar.KnjizenjeInvoice.Invoice.SdcDateTime.HasValue &&
            pazar.InvItem.ItemCode == itemDB.Id)
            .OrderBy(item => item.KnjizenjeInvoice.Knji.IssueDateTime);

            var neproknjizeniPazari = _viewModel.DbContext.Invoices.Join(_viewModel.DbContext.ItemInvoices,
            invoice => invoice.Id,
                invoiceItem => invoiceItem.InvoiceId,
                (invoice, invoiceItem) => new { Inv = invoice, InvItem = invoiceItem })
                .Where(pazar => pazar.Inv.SdcDateTime != null && pazar.Inv.SdcDateTime.HasValue &&
                string.IsNullOrEmpty(pazar.Inv.KnjizenjePazaraId) &&
                pazar.InvItem.ItemCode == itemDB.Id)
                .OrderBy(item => item.Inv.SdcDateTime);

            var kalkulacije = _viewModel.DbContext.Calculations.Join(_viewModel.DbContext.CalculationItems,
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

                await proknjizeniPazari.ForEachAsync(async prPazar =>
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

                            await kalkulacijeForDate.ForEachAsync(kal =>
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

                            await neproknjizeniPazariForDate.ForEachAsync(paz =>
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
                                        _viewModel.DbContext.ItemInvoices.Update(paz.InvItem);
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
                        _viewModel.DbContext.ItemInvoices.Update(prPazar.InvItem);
                    }
                });
            }
            else
            {
                if (neproknjizeniPazari != null &&
                    neproknjizeniPazari.Any())
                {
                    neproknjizeniPazari = neproknjizeniPazari.OrderBy(paz => paz.Inv.SdcDateTime);

                    await neproknjizeniPazari.ForEachAsync(async pazar =>
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

                                await kalkulacijeForDate.ForEachAsync(kal =>
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
                            _viewModel.DbContext.ItemInvoices.Update(pazar.InvItem);
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
                            if (prosecnaCena == 0)
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
    }
}