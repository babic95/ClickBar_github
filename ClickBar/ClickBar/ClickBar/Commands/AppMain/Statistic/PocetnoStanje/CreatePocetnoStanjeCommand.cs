using ClickBar.Models.Sale;
using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar_Common.Models.Statistic.Norm;
using ClickBar_Database;
using ClickBar_Database.Models;
using ClickBar_Logging;
using ClickBar_Printer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ClickBar.Commands.AppMain.Statistic.PocetnoStanje
{
    public class CreatePocetnoStanjeCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private PocetnoStanjeViewModel _currentViewModel;

        public CreatePocetnoStanjeCommand(PocetnoStanjeViewModel currentViewModel)
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
                var result = MessageBox.Show("Da li ste sigurni da želite da unesete novo početno stanje?",
                    "Početno stanje",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    using (SqliteDbContext sqliteDbContext = new SqliteDbContext())
                    {
                        var pocetnoStanjeDB = sqliteDbContext.PocetnaStanja.FirstOrDefault(p => 
                        p.PopisDate.Date == _currentViewModel.CurrentPocetnoStanje.PopisDate.Date);

                        if(pocetnoStanjeDB == null)
                        {
                            pocetnoStanjeDB = new PocetnoStanjeDB()
                            {
                                Id = _currentViewModel.CurrentPocetnoStanje.Id,
                                PopisDate = _currentViewModel.CurrentPocetnoStanje.PopisDate
                            };
                            sqliteDbContext.PocetnaStanja.Add(pocetnoStanjeDB);
                            RetryHelper.ExecuteWithRetry(() => { sqliteDbContext.SaveChanges(); });
                        }
                        else
                        {
                            result = MessageBox.Show("Početno stanje sa zadatim datumom je već kreirano, da li želite da ga izmenite?",
                            "Početno stanje već postoji",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

                            if (result == MessageBoxResult.No)
                            {
                                return;
                            }
                        }

                        _currentViewModel.CurrentPocetnoStanje.Items.ToList().ForEach(item =>
                        {
                            var itemDB = sqliteDbContext.Items.Find(item.Item.Item.Id);

                            if (itemDB != null)
                            {
                                var itemInInvoices = sqliteDbContext.Invoices.Join(sqliteDbContext.ItemInvoices,
                                    invoice => invoice.Id,
                                    itemsInvoice => itemsInvoice.InvoiceId,
                                    (invoice, itemsInvoice) => new { Invoice = invoice, ItemsInvoice = itemsInvoice })
                                .Where(i => i.ItemsInvoice.ItemCode == itemDB.Id &&
                                i.Invoice.SdcDateTime.HasValue &&
                                i.Invoice.SdcDateTime.Value.Date > _currentViewModel.CurrentPocetnoStanje.PopisDate.Date &&
                                i.Invoice.InvoiceType == 0);

                                var calculationItem = sqliteDbContext.CalculationItems.Join(sqliteDbContext.Calculations,
                                    itemC => itemC.CalculationId,
                                    c => c.Id,
                                    (itemC, c) => new { ItemC = itemC, C = c })
                                .Where(c => c.C.CalculationDate.Date >= _currentViewModel.CurrentPocetnoStanje.PopisDate.Date &&
                                c.ItemC.ItemId == itemDB.Id);

                                decimal calculationQuantity = 0;

                                if(calculationItem.Any())
                                {
                                    calculationQuantity = calculationItem.Select(i => i.ItemC.Quantity).ToList().Sum();
                                }

                                var pocetnoStanjeItemDB = sqliteDbContext.PocetnaStanjaItems.FirstOrDefault(p => p.IdPocetnoStanje == pocetnoStanjeDB.Id &&
                                p.IdItem == item.Item.Item.Id);

                                if (pocetnoStanjeItemDB == null)
                                {
                                    pocetnoStanjeItemDB = new PocetnoStanjeItemDB()
                                    {
                                        IdPocetnoStanje = pocetnoStanjeDB.Id,
                                        IdItem = item.Item.Item.Id,
                                        OldQuantity = decimal.Round(item.Item.Quantity, 3),
                                        NewQuantity = decimal.Round(item.NewQuantity, 3),
                                        InputPrice = item.NewInputPrice, // item.Item.Item.InputUnitPrice == null ? 0 : item.Item.Item.InputUnitPrice.Value,
                                        OutputPrice = item.Item.Item.SellingUnitPrice,
                                    };

                                    sqliteDbContext.PocetnaStanjaItems.Add(pocetnoStanjeItemDB);

                                    if (itemInInvoices.Any())
                                    {
                                        if (itemDB.IdNorm == null)
                                        {
                                            itemInInvoices.ForEachAsync(i =>
                                            {
                                                if (i.ItemsInvoice.Quantity != null)
                                                {
                                                    if (i.Invoice.TransactionType == 0)
                                                    {
                                                        item.NewQuantity -= i.ItemsInvoice.Quantity.Value;
                                                    }
                                                    else
                                                    {
                                                        if (i.Invoice.SendEfaktura == 0)
                                                        {
                                                            item.NewQuantity += i.ItemsInvoice.Quantity.Value;
                                                        }
                                                    }
                                                }
                                            });
                                        }
                                    }

                                    item.NewQuantity += calculationQuantity;

                                    if (item.NewQuantity != itemDB.TotalQuantity)
                                    {
                                        itemDB.TotalQuantity = item.NewQuantity;

                                        sqliteDbContext.Items.Update(itemDB);
                                    }

                                    if (item.NewInputPrice != itemDB.InputUnitPrice)
                                    {
                                        itemDB.InputUnitPrice = item.NewInputPrice;
                                        sqliteDbContext.Items.Update(itemDB);
                                    }
                                }
                                else
                                {
                                    if (decimal.Round(item.NewQuantity, 3) != decimal.Round(pocetnoStanjeItemDB.NewQuantity, 3))
                                    {
                                        pocetnoStanjeItemDB.NewQuantity = decimal.Round(item.NewQuantity, 3);
                                        pocetnoStanjeItemDB.InputPrice = item.NewInputPrice;

                                        sqliteDbContext.PocetnaStanjaItems.Update(pocetnoStanjeItemDB);

                                        if (itemInInvoices.Any())
                                        {
                                            if (itemDB.IdNorm == null)
                                            {
                                                itemInInvoices.ForEachAsync(i =>
                                                {
                                                    if (i.ItemsInvoice.Quantity != null)
                                                    {
                                                        if (i.Invoice.TransactionType == 0)
                                                        {
                                                            item.NewQuantity -= i.ItemsInvoice.Quantity.Value;
                                                        }
                                                        else
                                                        {
                                                            if (i.Invoice.SendEfaktura == 0)
                                                            {
                                                                item.NewQuantity += i.ItemsInvoice.Quantity.Value;
                                                            }
                                                        }
                                                    }
                                                });
                                            }
                                        }

                                        item.NewQuantity += calculationQuantity;

                                        if (item.NewQuantity != itemDB.TotalQuantity)
                                        {
                                            itemDB.TotalQuantity = item.NewQuantity;

                                            sqliteDbContext.Items.Update(itemDB);
                                        }

                                        if (item.NewInputPrice != itemDB.InputUnitPrice)
                                        {
                                            itemDB.InputUnitPrice = item.NewInputPrice;
                                            sqliteDbContext.Items.Update(itemDB);
                                        }
                                    }
                                }
                            }
                        });

                        RetryHelper.ExecuteWithRetry(() => { sqliteDbContext.SaveChanges(); });

                        MessageBox.Show("Uspešno ste uneli novo početno stanje.",
                            "Uspešno",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                }
            }
            catch(Exception ex)
            {
                Log.Error("CreatePocetnoStanjeCommand -> Execute -> Greska prilikom kreiranja pocetnog stanja: ", ex);
                MessageBox.Show("Desila se greška prilikom kreiranja početnog stanja.\nObratite se serviseru.",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}