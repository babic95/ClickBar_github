using ClickBar.Models.AppMain.Statistic;
using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar.ViewModels;
using ClickBar.Views.AppMain.AuxiliaryWindows.Statistic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ClickBar_Logging;
using System.Windows;
using ClickBar.Models.AppMain.Statistic.DPU;
using Microsoft.EntityFrameworkCore;
using DocumentFormat.OpenXml.Spreadsheet;
using ClickBar_DatabaseSQLManager.Models;

namespace ClickBar.Commands.AppMain.Statistic.DPU
{
    public class SearchDPUItemsCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private DPU_PeriodicniViewModel _currentViewModel;

        public SearchDPUItemsCommand(DPU_PeriodicniViewModel currentViewModel)
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
                if (_currentViewModel.FromDate.Date > _currentViewModel.ToDate.Date)
                {
                    MessageBox.Show("Datum od ne može biti veći od datuma do!", "Greška", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    return;
                }

                var itemsInvoice = _currentViewModel.DbContext.ItemInvoices.Include(i => i.Invoice)
                    .Where(x => x.Invoice.SdcDateTime.HasValue &&
                    x.Invoice.SdcDateTime.Value.Date >= _currentViewModel.FromDate.Date &&
                    x.Invoice.SdcDateTime.Value.Date <= _currentViewModel.ToDate.Date)
                    .GroupBy(x => new { x.ItemCode, x.Name })
                    .Select(g => new DPU_Item
                    {
                        Id = g.Key.ItemCode,
                        Name = g.Key.Name,
                        OutputQuantity = g.Sum(x => x.Quantity.Value),
                        InputQuantity = 0,
                        EndQuantity = 0,
                        StartQuantity = 0
                    })
                    .ToList();

                var itemsCalculation = _currentViewModel.DbContext.CalculationItems.Include(i => i.Calculation)
                    .Join(_currentViewModel.DbContext.Items,
                    ci => ci.ItemId, i => i.Id, (ci, i) => new { Calculation = ci, Item = i })
                    .Where(x => x.Calculation.Calculation.CalculationDate.Date >= _currentViewModel.FromDate.Date &&
                    x.Calculation.Calculation.CalculationDate.Date <= _currentViewModel.ToDate.Date)
                    .GroupBy(x => new { x.Calculation.ItemId, x.Item.Name })
                    .Select(g => new DPU_Item
                    {
                        Id = g.Key.ItemId,
                        Name = g.Key.Name,
                        InputQuantity = g.Sum(x => x.Calculation.Quantity),
                        EndQuantity = 0,
                        StartQuantity = 0,
                        OutputQuantity = 0
                    })
                    .ToList();

                var started = GetStartQuantity();

                if (started == null)
                {
                    return;
                }

                // Kombinovanje svih stavki i izračunavanje EndQuantity
                var combinedItems = started.Select(s =>
                {
                    var matchingInvoice = itemsInvoice.FirstOrDefault(i => i.Id == s.Id);
                    var matchingCalculation = itemsCalculation.FirstOrDefault(c => c.Id == s.Id);

                    return new DPU_Item
                    {
                        Id = s.Id,
                        Name = s.Name,
                        StartQuantity = s.StartQuantity,
                        InputQuantity = matchingCalculation?.InputQuantity ?? 0,
                        OutputQuantity = matchingInvoice?.OutputQuantity ?? 0,
                        EndQuantity = s.StartQuantity + (matchingCalculation?.InputQuantity ?? 0) - (matchingInvoice?.OutputQuantity ?? 0)
                    };
                }).OrderBy(x => x.Id).ToList();

                _currentViewModel.AllItems = new List<DPU_Item>(combinedItems);
                _currentViewModel.Items = new ObservableCollection<DPU_Item>(_currentViewModel.AllItems);
            }
            catch (Exception ex)
            {
                Log.Error("SearchDPUItemsCommand -> Desila se greska prilikom pretrage DPU: ", ex);
                MessageBox.Show("Greška prilikom pretrage!", "Greška", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private List<DPU_Item>? GetStartQuantity()
        {
            List<DPU_Item> items = new List<DPU_Item>();

            try
            {
                PocetnoStanjeDB? pocetnoStanjeDB = null;

                if (_currentViewModel.DbContext.PocetnaStanja != null &&
                    _currentViewModel.DbContext.PocetnaStanja.Any())
                {
                    pocetnoStanjeDB = _currentViewModel.DbContext.PocetnaStanja.Where(p => p.PopisDate.Date < _currentViewModel.FromDate.Date).
                        OrderByDescending(p => p.PopisDate).FirstOrDefault();
                }

                DateTime pocetnoStanjeDate = new DateTime(DateTime.Now.Year, 1, 1, 0, 0, 0);

                if (pocetnoStanjeDB != null)
                {
                    if (pocetnoStanjeDB.PopisDate.Date > pocetnoStanjeDate.Date)
                    {
                        pocetnoStanjeDate = pocetnoStanjeDB.PopisDate.Date;
                    }
                }

                var allCalculations = _currentViewModel.DbContext.Calculations.Join(_currentViewModel.DbContext.CalculationItems,
                    calculation => calculation.Id,
                    calculationItem => calculationItem.CalculationId,
                    (calculation, calculationItem) => new { Calculation = calculation, CalculationItem = calculationItem })
                    .Where(cal => cal.Calculation.CalculationDate.Date >= pocetnoStanjeDate.Date &&
                    cal.Calculation.CalculationDate.Date < _currentViewModel.FromDate.Date);

                var pazar = _currentViewModel.DbContext.Invoices.Join(_currentViewModel.DbContext.ItemInvoices,
                    invoice => invoice.Id,
                    invoiceItem => invoiceItem.InvoiceId,
                    (invoice, invoiceItem) => new { Invoice = invoice, InvoiceItem = invoiceItem })
                    .Where(inv => inv.Invoice.SdcDateTime != null &&
                    inv.Invoice.SdcDateTime.Value.Date >= pocetnoStanjeDate.Date &&
                    inv.Invoice.SdcDateTime.Value.Date < _currentViewModel.FromDate.Date);

                if (_currentViewModel.DbContext.Items != null &&
                    _currentViewModel.DbContext.Items.Any())
                {
                    foreach (var x in _currentViewModel.DbContext.Items.Where(i => i.IdNorm == null))
                    {
                        decimal totalQuantityPocetnoStanje = 0;

                        if (pocetnoStanjeDB != null)
                        {
                            var pocetnoStanjeItemDB = _currentViewModel.DbContext.PocetnaStanjaItems.FirstOrDefault(p => p.IdPocetnoStanje == pocetnoStanjeDB.Id &&
                            p.IdItem == x.Id);

                            if (pocetnoStanjeItemDB != null)
                            {
                                totalQuantityPocetnoStanje = pocetnoStanjeItemDB.NewQuantity;
                            }
                        }

                            decimal quantity = totalQuantityPocetnoStanje;

                        if (x.IdNorm == null)
                        {
                            if (allCalculations != null &&
                            allCalculations.Any())
                            {
                                var itemsInCal = allCalculations.Where(cal => cal.CalculationItem.ItemId == x.Id);

                                if (itemsInCal != null &&
                                itemsInCal.Any())
                                {
                                    foreach (var i in itemsInCal)
                                    {
                                        quantity += i.CalculationItem.Quantity;
                                    }
                                }
                            }

                            if (pazar != null &&
                            pazar.Any())
                            {
                                var itemsInPazar = pazar.Where(paz => paz.InvoiceItem.ItemCode == x.Id);

                                if (itemsInPazar != null &&
                                itemsInPazar.Any())
                                {
                                    foreach (var i in itemsInPazar)
                                    {
                                        if (i.InvoiceItem.Quantity != null)
                                        {
                                            if (i.Invoice.TransactionType == 0)
                                            {
                                                quantity -= i.InvoiceItem.Quantity.Value;
                                            }
                                            else
                                            {
                                                quantity += i.InvoiceItem.Quantity.Value;
                                            }
                                        }

                                    }
                                }
                            }

                            DPU_Item dPU_Item = new DPU_Item()
                            {
                                Id = x.Id,
                                Name = x.Name,
                                StartQuantity = quantity,
                                EndQuantity = 0,
                                InputQuantity = 0,
                                OutputQuantity = 0
                            };

                            items.Add(dPU_Item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("SearchDPUItemsCommand -> GetStartQuantity -> Desila se greska prilikom pretrage DPU: ", ex);
                MessageBox.Show("Greška prilikom pretrage!", "Greška", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return null;
            }

            return items;
        }
    }
}