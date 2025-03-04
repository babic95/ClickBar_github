using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar.Views.AppMain.AuxiliaryWindows.Statistic;
using ClickBar_DatabaseSQLManager.Models;
using ClickBar_DatabaseSQLManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using ClickBar_Logging;
using SQLitePCL;
using ClickBar.Models.Sale;
using ClickBar_eFaktura.Models;

namespace ClickBar.Commands.AppMain.Statistic.Norm
{
    public class FixNormCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private NormViewModel _currentViewModel;

        public FixNormCommand(NormViewModel currentViewModel)
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
                if (_currentViewModel.FromDate == null)
                {
                    _currentViewModel.FromDate = DateTime.Now;
                }
                if (_currentViewModel.ToDate == null)
                {
                    _currentViewModel.ToDate = DateTime.Now;
                }

                if (_currentViewModel.FromDate > _currentViewModel.ToDate)
                {
                    MessageBox.Show("Početni datum mora biti stariji od krajnjeg!", "Greška",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);

                    return;
                }

                SupergroupDB? supergroupDB = null;

                if (_currentViewModel.CurrentSupergroupSearch != null)
                {
                    supergroupDB = _currentViewModel.DbContext.Supergroups.Find(_currentViewModel.CurrentSupergroupSearch.Id);
                }

                var invoices = _currentViewModel.DbContext.Invoices.Where(invoice => invoice.SdcDateTime >= _currentViewModel.FromDate.Value &&
                    invoice.SdcDateTime <= _currentViewModel.ToDate.Value &&
                    invoice.InvoiceType == 0).ToList();

                if (invoices != null && invoices.Any())
                {
                    using (var transaction = _currentViewModel.DbContext.Database.BeginTransaction())
                    {
                        try
                        {
                            foreach (var invoice in invoices)
                            {
                                // Prvo uklonite sve normative iz InvoiceItems za dati invoice gde je IsSirovina == 1
                                var existingItems = _currentViewModel.DbContext.ItemInvoices.Where(itemInvoice =>
                                itemInvoice.InvoiceId == invoice.Id && itemInvoice.IsSirovina == 1).ToList();
                                if (supergroupDB == null)
                                {
                                    _currentViewModel.DbContext.ItemInvoices.RemoveRange(existingItems);
                                }
                                else
                                {
                                    foreach (var item in existingItems)
                                    {
                                        var itemDB = _currentViewModel.DbContext.Items.Find(item.ItemCode);
                                        if (itemDB != null)
                                        {
                                            var itemSuperGroup = _currentViewModel.DbContext.Items.Join(_currentViewModel.DbContext.ItemGroups,
                                                i => i.IdItemGroup,
                                                g => g.Id,
                                                (i, g) => new { I = i, G = g })
                                            .Join(_currentViewModel.DbContext.Supergroups,
                                            g => g.G.IdSupergroup,
                                            s => s.Id,
                                            (g, s) => new { G = g, S = s }).FirstOrDefault(i => i.S.Id == supergroupDB.Id &&
                                            i.G.I.Id == itemDB.Id);
                                            if (itemSuperGroup == null)
                                            {
                                                _currentViewModel.DbContext.ItemInvoices.Remove(item);
                                            }
                                        }
                                    }
                                }
                                _currentViewModel.DbContext.SaveChanges();

                                // Ponovo prolazimo kroz sve ItemInvoices gde je IsSirovina == 0 i dodajemo nove normative
                                var itemsInInvoice = _currentViewModel.DbContext.ItemInvoices.Where(itemInvoice =>
                                itemInvoice.InvoiceId == invoice.Id &&
                                (itemInvoice.IsSirovina == null || itemInvoice.IsSirovina == 0)).ToList();

                                foreach (var glavniItem in itemsInInvoice)
                                {
                                    var itemDB = _currentViewModel.DbContext.Items.Find(glavniItem.ItemCode);

                                    if (itemDB != null)
                                    {
                                        decimal quantity = 0;
                                        if (glavniItem.Quantity.HasValue)
                                        {
                                            quantity = glavniItem.Quantity.Value;
                                        }
                                        if (itemDB.IdNorm != null)
                                        {
                                            var norms1 = _currentViewModel.DbContext.ItemsInNorm.Where(itemInNorm => itemInNorm.IdNorm == itemDB.IdNorm).ToList();

                                            if (norms1 != null && norms1.Any())
                                            {
                                                foreach (var itemInNorm1 in norms1)
                                                {
                                                    decimal quantity1 = quantity * itemInNorm1.Quantity;
                                                    var itemDB2 = _currentViewModel.DbContext.Items.Find(itemInNorm1.IdItem);

                                                    if (itemDB2 != null)
                                                    {
                                                        if (itemDB2.IdNorm != null)
                                                        {
                                                            var norms2 = _currentViewModel.DbContext.ItemsInNorm.Where(itemInNorm =>
                                                            itemInNorm.IdNorm == itemDB2.IdNorm).ToList();

                                                            if (norms2 != null && norms2.Any())
                                                            {
                                                                foreach (var itemInNorm2 in norms2)
                                                                {
                                                                    decimal quantity2 = quantity1 * itemInNorm2.Quantity;
                                                                    var itemDB3 = _currentViewModel.DbContext.Items.Find(itemInNorm2.IdItem);

                                                                    if (itemDB3 != null)
                                                                    {
                                                                        if (itemDB3.IdNorm != null)
                                                                        {
                                                                            var norms3 = _currentViewModel.DbContext.ItemsInNorm.Where(itemInNorm =>
                                                                            itemInNorm.IdNorm == itemDB3.IdNorm).ToList();

                                                                            if (norms3 != null && norms3.Any())
                                                                            {
                                                                                foreach (var itemInNorm3 in norms3)
                                                                                {
                                                                                    decimal quantity3 = quantity2 * itemInNorm3.Quantity;
                                                                                    var itemDB4 = _currentViewModel.DbContext.Items.Find(itemInNorm3.IdItem);

                                                                                    if (itemDB4 != null)
                                                                                    {
                                                                                        if (supergroupDB != null)
                                                                                        {
                                                                                            var itemSuperGroup = _currentViewModel.DbContext.Items.Join(_currentViewModel.DbContext.ItemGroups,
                                                                                                i => i.IdItemGroup,
                                                                                                g => g.Id,
                                                                                                (i, g) => new { I = i, G = g })
                                                                                            .Join(_currentViewModel.DbContext.Supergroups,
                                                                                            g => g.G.IdSupergroup,
                                                                                            s => s.Id,
                                                                                            (g, s) => new { G = g, S = s }).FirstOrDefault(i => i.S.Id == supergroupDB.Id &&
                                                                                            i.G.I.Id == itemDB4.Id);

                                                                                            if (itemSuperGroup == null)
                                                                                            {
                                                                                                continue;
                                                                                            }
                                                                                        }
                                                                                        int index = _currentViewModel.DbContext.ItemInvoices.Where(itemInvoice => itemInvoice.InvoiceId == invoice.Id).Max(i => i.Id) + 1;

                                                                                        var itemInvoiceSirovina = _currentViewModel.DbContext.ItemInvoices.FirstOrDefault(i => i.InvoiceId == invoice.Id &&
                                                                                        i.ItemCode == itemDB4.Id &&
                                                                                        i.IsSirovina == 1 &&
                                                                                        i.Id == index);

                                                                                        if (itemInvoiceSirovina == null)
                                                                                        {
                                                                                            itemInvoiceSirovina = new ItemInvoiceDB()
                                                                                            {
                                                                                                Id = index,
                                                                                                IsSirovina = 1,
                                                                                                InvoiceId = invoice.Id,
                                                                                                ItemCode = itemDB4.Id,
                                                                                                Quantity = quantity3,
                                                                                                InputUnitPrice = itemDB4.InputUnitPrice.HasValue ? itemDB4.InputUnitPrice.Value : 0,
                                                                                                Label = itemDB4.Label,
                                                                                                Name = itemDB4.Name,
                                                                                                UnitPrice = itemDB4.InputUnitPrice.HasValue ? itemDB4.InputUnitPrice.Value : 0,
                                                                                                OriginalUnitPrice = itemDB4.InputUnitPrice.HasValue ? itemDB4.InputUnitPrice.Value : 0,
                                                                                                TotalAmout = Decimal.Round(
                                                                                                    quantity3 * (itemDB4.InputUnitPrice.HasValue ? itemDB4.InputUnitPrice.Value : 0), 2),
                                                                                            };

                                                                                            _currentViewModel.DbContext.ItemInvoices.Add(itemInvoiceSirovina);
                                                                                        }
                                                                                        else
                                                                                        {
                                                                                            itemInvoiceSirovina.Quantity += quantity3;
                                                                                            itemInvoiceSirovina.TotalAmout = Decimal.Round(
                                                                                                itemInvoiceSirovina.Quantity.Value * (itemDB4.InputUnitPrice.HasValue ? itemDB4.InputUnitPrice.Value : 0), 2);

                                                                                            _currentViewModel.DbContext.ItemInvoices.Update(itemInvoiceSirovina);
                                                                                        }
                                                                                        // Privremeno čuvanje u bazu
                                                                                        _currentViewModel.DbContext.SaveChanges();

                                                                                        // Odvojite entitet iz konteksta
                                                                                        _currentViewModel.DbContext.Entry(itemInvoiceSirovina).State = EntityState.Detached;
                                                                                        _currentViewModel.DbContext.Entry(itemDB4).State = EntityState.Detached;
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                        else
                                                                        {
                                                                            if (supergroupDB != null)
                                                                            {
                                                                                var itemSuperGroup = _currentViewModel.DbContext.Items.Join(_currentViewModel.DbContext.ItemGroups,
                                                                                    i => i.IdItemGroup,
                                                                                    g => g.Id,
                                                                                    (i, g) => new { I = i, G = g })
                                                                                .Join(_currentViewModel.DbContext.Supergroups,
                                                                                g => g.G.IdSupergroup,
                                                                                s => s.Id,
                                                                                (g, s) => new { G = g, S = s }).FirstOrDefault(i => i.S.Id == supergroupDB.Id &&
                                                                                i.G.I.Id == itemDB3.Id);

                                                                                if (itemSuperGroup == null)
                                                                                {
                                                                                    continue;
                                                                                }
                                                                            }
                                                                            int index = _currentViewModel.DbContext.ItemInvoices.Where(itemInvoice => itemInvoice.InvoiceId == invoice.Id).Max(i => i.Id) + 1;

                                                                            var itemInvoiceSirovina = _currentViewModel.DbContext.ItemInvoices.FirstOrDefault(i => i.InvoiceId == invoice.Id &&
                                                                                    i.ItemCode == itemDB3.Id &&
                                                                                    i.IsSirovina == 1 &&
                                                                                    i.Id == index);

                                                                            if (itemInvoiceSirovina == null)
                                                                            {
                                                                                itemInvoiceSirovina = new ItemInvoiceDB()
                                                                                {
                                                                                    Id = index,
                                                                                    IsSirovina = 1,
                                                                                    InvoiceId = invoice.Id,
                                                                                    ItemCode = itemDB3.Id,
                                                                                    Quantity = quantity2,
                                                                                    InputUnitPrice = itemDB3.InputUnitPrice.HasValue ? itemDB3.InputUnitPrice.Value : 0,
                                                                                    Label = itemDB3.Label,
                                                                                    Name = itemDB3.Name,
                                                                                    UnitPrice = itemDB3.InputUnitPrice.HasValue ? itemDB3.InputUnitPrice.Value : 0,
                                                                                    OriginalUnitPrice = itemDB3.InputUnitPrice.HasValue ? itemDB3.InputUnitPrice.Value : 0,
                                                                                    TotalAmout = Decimal.Round(
                                                                                    quantity2 * (itemDB3.InputUnitPrice.HasValue ? itemDB3.InputUnitPrice.Value : 0), 2),
                                                                                };

                                                                                _currentViewModel.DbContext.ItemInvoices.Add(itemInvoiceSirovina);
                                                                            }
                                                                            else
                                                                            {
                                                                                itemInvoiceSirovina.Quantity += quantity2;
                                                                                itemInvoiceSirovina.TotalAmout = Decimal.Round(
                                                                                itemInvoiceSirovina.Quantity.Value * (itemDB3.InputUnitPrice.HasValue ? itemDB3.InputUnitPrice.Value : 0), 2);

                                                                                _currentViewModel.DbContext.ItemInvoices.Update(itemInvoiceSirovina);
                                                                            }
                                                                            // Privremeno čuvanje u bazu
                                                                            _currentViewModel.DbContext.SaveChanges();

                                                                            // Odvojite entitet iz konteksta
                                                                            _currentViewModel.DbContext.Entry(itemInvoiceSirovina).State = EntityState.Detached;
                                                                            _currentViewModel.DbContext.Entry(itemDB3).State = EntityState.Detached;
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (supergroupDB != null)
                                                            {
                                                                var itemSuperGroup = _currentViewModel.DbContext.Items.Join(_currentViewModel.DbContext.ItemGroups,
                                                                    i => i.IdItemGroup,
                                                                    g => g.Id,
                                                                    (i, g) => new { I = i, G = g })
                                                                .Join(_currentViewModel.DbContext.Supergroups,
                                                                g => g.G.IdSupergroup,
                                                                s => s.Id,
                                                                (g, s) => new { G = g, S = s }).FirstOrDefault(i => i.S.Id == supergroupDB.Id &&
                                                                i.G.I.Id == itemDB2.Id);

                                                                if (itemSuperGroup == null)
                                                                {
                                                                    continue;
                                                                }
                                                            }
                                                            int index = _currentViewModel.DbContext.ItemInvoices.Where(itemInvoice => itemInvoice.InvoiceId == invoice.Id).Max(i => i.Id) + 1;

                                                            var itemInvoiceSirovina = _currentViewModel.DbContext.ItemInvoices.FirstOrDefault(i => i.InvoiceId == invoice.Id &&
                                                                                            i.ItemCode == itemDB2.Id &&
                                                                                            i.IsSirovina == 1 &&
                                                                                            i.Id == index);

                                                            if (itemInvoiceSirovina == null)
                                                            {
                                                                itemInvoiceSirovina = new ItemInvoiceDB()
                                                                {
                                                                    Id = index,
                                                                    IsSirovina = 1,
                                                                    InvoiceId = invoice.Id,
                                                                    ItemCode = itemDB2.Id,
                                                                    Quantity = quantity1,
                                                                    InputUnitPrice = itemDB2.InputUnitPrice.HasValue ? itemDB2.InputUnitPrice.Value : 0,
                                                                    Label = itemDB2.Label,
                                                                    Name = itemDB2.Name,
                                                                    UnitPrice = itemDB2.InputUnitPrice.HasValue ? itemDB2.InputUnitPrice.Value : 0,
                                                                    OriginalUnitPrice = itemDB2.InputUnitPrice.HasValue ? itemDB2.InputUnitPrice.Value : 0,
                                                                    TotalAmout = Decimal.Round(
                                                                        quantity1 * (itemDB2.InputUnitPrice.HasValue ? itemDB2.InputUnitPrice.Value : 0), 2),
                                                                };

                                                                _currentViewModel.DbContext.ItemInvoices.Add(itemInvoiceSirovina);
                                                            }
                                                            else
                                                            {
                                                                itemInvoiceSirovina.Quantity += quantity1;
                                                                itemInvoiceSirovina.TotalAmout = Decimal.Round(
                                                                    itemInvoiceSirovina.Quantity.Value * (itemDB2.InputUnitPrice.HasValue ? itemDB2.InputUnitPrice.Value : 0), 2);

                                                                _currentViewModel.DbContext.ItemInvoices.Update(itemInvoiceSirovina);
                                                            }
                                                            // Privremeno čuvanje u bazu
                                                            _currentViewModel.DbContext.SaveChanges();

                                                            // Odvojite entitet iz konteksta
                                                            _currentViewModel.DbContext.Entry(itemInvoiceSirovina).State = EntityState.Detached;
                                                            _currentViewModel.DbContext.Entry(itemDB2).State = EntityState.Detached;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            transaction.Commit();
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }

                MessageBox.Show("Uspešno ste sredili stanje sirovina za zadati period!", "Uspešno",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Greška prilikom sredjivanja normativa!", "Greška",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                Log.Error("FixNormCommand -> Greska prilikom sredjivanja normativa -> ", ex);
            }
        }
    }
}