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

        public async void Execute(object parameter)
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
                    invoice.InvoiceType == 0);

                if (invoices != null &&
                    invoices.Any())
                {
                    foreach(var invoice in invoices)
                    {
                        List<ItemInNormForChange> itemInNormForChanges = new List<ItemInNormForChange>();

                        var itemsInInvoiceSirovine = _currentViewModel.DbContext.ItemInvoices.Where(itemInvoice => itemInvoice.InvoiceId == invoice.Id &&
                        itemInvoice.IsSirovina == 1);

                        if (itemsInInvoiceSirovine != null &&
                        itemsInInvoiceSirovine.Any())
                        {
                            foreach (var itemInInvoice in itemsInInvoiceSirovine)
                            {
                                var itemDB = _currentViewModel.DbContext.Items.Find(itemInInvoice.ItemCode);

                                if (itemDB != null)
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
                                        i.G.I.Id == itemDB.Id);

                                        if (itemSuperGroup == null)
                                        {
                                            continue;
                                        }
                                    }

                                    ReduceNormHasNorm(invoice,
                                        itemDB,
                                        itemInInvoice,
                                        itemInNormForChanges);
                                }
                            }
                        }
                        else
                        {
                            var itemsInInvoiceNotSirovina = _currentViewModel.DbContext.ItemInvoices.Where(itemInvoice => itemInvoice.InvoiceId == invoice.Id);

                            if (itemsInInvoiceNotSirovina != null &&
                                itemsInInvoiceNotSirovina.Any())
                            {
                                foreach(var itemInvoice in itemsInInvoiceNotSirovina)
                                {
                                    var itemDB = _currentViewModel.DbContext.Items.Find(itemInvoice.ItemCode);

                                    if (itemDB != null)
                                    {
                                        if (itemDB.IdNorm != null)
                                        {
                                            var norms2 = _currentViewModel.DbContext.ItemsInNorm.Where(itemInNorm => itemInNorm.IdNorm == itemDB.IdNorm);

                                            if (norms2 != null &&
                                            norms2.Any())
                                            {
                                                foreach (var itemInNorm2 in norms2)
                                                {
                                                    var itemDB2 = _currentViewModel.DbContext.Items.Find(itemInNorm2.IdItem);

                                                    if (itemDB2 != null)
                                                    {
                                                        if (itemDB2.IdNorm != null)
                                                        {
                                                            var norms3 = _currentViewModel.DbContext.ItemsInNorm.Where(itemInNorm =>
                                                            itemInNorm.IdNorm == itemDB2.IdNorm);

                                                            if (norms3 != null &&
                                                            norms3.Any())
                                                            {
                                                                foreach (var itemInNorm3 in norms3)
                                                                {
                                                                    var itemDB3 = _currentViewModel.DbContext.Items.Find(itemInNorm3.IdItem);

                                                                    if (itemDB3 != null)
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

                                                                        ReduceNormNoNorm(invoice,
                                                                            itemDB3,
                                                                            itemInNorm3.Quantity,
                                                                            itemInNormForChanges);
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
                                                            ReduceNormNoNorm(invoice,
                                                                itemDB2,
                                                                itemInNorm2.Quantity,
                                                                itemInNormForChanges);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        //else
                                        //{
                                        //    ReduceNormNoNorm(sqliteDbContext,
                                        //                    invoice,
                                        //                    itemDB,
                                        //                    itemInvoice.Quantity.Value,
                                        //                    itemInNormForChanges);
                                        //}
                                    }
                                }
                            }
                        }
                        _currentViewModel.DbContext.SaveChanges();

                        var itemsInInvoice = _currentViewModel.DbContext.ItemInvoices.Where(itemInvoice => itemInvoice.InvoiceId == invoice.Id &&
                        (itemInvoice.IsSirovina == null || itemInvoice.IsSirovina == 0));

                        if (itemsInInvoice != null &&
                        itemsInInvoice.Any())
                        {
                            foreach (var itemInvoice in itemsInInvoice)
                            {
                                var itemDB = _currentViewModel.DbContext.Items.Find(itemInvoice.ItemCode);

                                if (itemDB != null)
                                {
                                    decimal quantity = 0;
                                    if (itemInvoice.Quantity.HasValue)
                                    {
                                        quantity = itemInvoice.Quantity.Value;
                                    }
                                    if (itemDB.IdNorm != null)
                                    {
                                        var norms1 = _currentViewModel.DbContext.ItemsInNorm.Where(itemInNorm => itemInNorm.IdNorm == itemDB.IdNorm);

                                        if (norms1 != null &&
                                        norms1.Any())
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
                                                        itemInNorm.IdNorm == itemDB2.IdNorm);

                                                        if (norms2 != null &&
                                                        norms2.Any())
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
                                                                        itemInNorm.IdNorm == itemDB3.IdNorm);

                                                                        if (norms3 != null &&
                                                                        norms3.Any())
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
                                                                                    IncreaseNorm(invoice,
                                                                                        itemDB4,
                                                                                        quantity3,
                                                                                        itemInNormForChanges);
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
                                                                        IncreaseNorm(invoice,
                                                                            itemDB3,
                                                                            quantity2,
                                                                            itemInNormForChanges);
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
                                                        IncreaseNorm(invoice,
                                                            itemDB2,
                                                            quantity1,
                                                            itemInNormForChanges);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    //else
                                    //{
                                    //    IncreaseNormNotNorm(sqliteDbContext,
                                    //                invoice,
                                    //                itemDB,
                                    //                quantity);
                                    //}
                                }
                            }
                        }
                    }
                }
                _currentViewModel.DbContext.SaveChanges();

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
        private void UpdateNorm(ItemInNormDB normDB,
            InvoiceDB invoiceDB,
            ItemDB itemDB,
            int lastSalsItemIndex,
            int currentSalsItemIndex,
            decimal quantity)
        {
            if (invoiceDB.Id == "9b8c0fad-d499-4705-bad3-0a0cc0f9a925")
            {
                int a = 2;
            }
            var itemInvoiceDB = _currentViewModel.DbContext.ItemInvoices.FirstOrDefault(itemInvoice =>
            itemInvoice.ItemCode == normDB.IdItem && itemInvoice.InvoiceId == invoiceDB.Id &&
            itemInvoice.Id > lastSalsItemIndex && itemInvoice.Id < currentSalsItemIndex);

            if (itemInvoiceDB != null)
            {
                decimal price = itemInvoiceDB.UnitPrice.HasValue ? itemInvoiceDB.UnitPrice.Value : 0;
                decimal oldQuantity = itemInvoiceDB.Quantity.HasValue ? itemInvoiceDB.Quantity.Value : 0;

                if (quantity != oldQuantity)
                {
                    if (invoiceDB.TransactionType == 0)
                    {
                        itemDB.TotalQuantity -= (quantity - oldQuantity);
                    }
                    else
                    {
                        itemDB.TotalQuantity += (quantity - oldQuantity);
                    }

                    itemInvoiceDB.Quantity = quantity;
                    itemInvoiceDB.TotalAmout = quantity * price;
                    _currentViewModel.DbContext.Items.Update(itemDB);
                    _currentViewModel.DbContext.ItemInvoices.Update(itemInvoiceDB);
                }
            }
            else
            {
                var itemsInvoice = _currentViewModel.DbContext.ItemInvoices.Where(itemInvoice =>
                itemInvoice.InvoiceId == invoiceDB.Id &&
                itemInvoice.Id >= currentSalsItemIndex);

                if (itemsInvoice != null &&
                itemsInvoice.Any())
                {
                    itemsInvoice = itemsInvoice.OrderByDescending(i => i.Id);

                    foreach(var i in itemsInvoice)
                    {
                        var itemInvoice = new ItemInvoiceDB()
                        {
                            Id = i.Id + 1,
                            InvoiceId = i.InvoiceId,
                            IsSirovina = i.IsSirovina,
                            ItemCode = i.ItemCode,
                            Label = i.Label,
                            Name = i.Name,
                            OriginalUnitPrice = i.OriginalUnitPrice,
                            Quantity = i.Quantity,
                            TotalAmout = i.TotalAmout,
                            UnitPrice = i.UnitPrice,
                        };

                        _currentViewModel.DbContext.ItemInvoices.Remove(i);
                        _currentViewModel.DbContext.ItemInvoices.Add(itemInvoice);
                        _currentViewModel.DbContext.SaveChanges();
                    }
                }

                itemInvoiceDB = new ItemInvoiceDB()
                {
                    Id = currentSalsItemIndex,
                    InvoiceId = invoiceDB.Id,
                    IsSirovina = 1,
                    ItemCode = itemDB.Id,
                    Label = itemDB.Label,
                    Name = itemDB.Name,
                    Quantity = quantity,
                    OriginalUnitPrice = itemDB.SellingUnitPrice,
                    UnitPrice = itemDB.SellingUnitPrice,
                    TotalAmout = itemDB.SellingUnitPrice * quantity
                };

                _currentViewModel.DbContext.ItemInvoices.Add(itemInvoiceDB);
            }
            _currentViewModel.DbContext.SaveChanges();
        }
        private void ReduceNormHasNorm(InvoiceDB invoice,
            ItemDB itemDB,
            ItemInvoiceDB itemInInvoice,
            List<ItemInNormForChange> itemInNormForChanges)
        {

            if (itemInInvoice.UnitPrice != null &&
                itemInInvoice.UnitPrice.HasValue &&
                itemInInvoice.Quantity != null &&
                itemInInvoice.Quantity.HasValue)
            {
                if (invoice.TransactionType == 0)
                {
                    itemDB.TotalQuantity += itemInInvoice.Quantity.Value;
                }
                else
                {
                    itemDB.TotalQuantity -= itemInInvoice.Quantity.Value;
                }
                _currentViewModel.DbContext.Items.Update(itemDB);

                if (itemInNormForChanges.FirstOrDefault(i => i.ItemId == itemInInvoice.ItemCode &&
                i.InvoiceId == invoice.Id) == null)
                {
                    itemInNormForChanges.Add(new ItemInNormForChange()
                    {
                        ItemId = itemDB.Id,
                        UnitPrice = itemInInvoice.UnitPrice.Value,
                        InvoiceId = invoice.Id
                    });
                }
                _currentViewModel.DbContext.Remove(itemInInvoice);
                _currentViewModel.DbContext.SaveChanges();
            }
        }
        private void ReduceNormNoNorm(InvoiceDB invoice,
            ItemDB itemDB,
            decimal quantity,
            List<ItemInNormForChange> itemInNormForChanges)
        {
            if (invoice.TransactionType == 0)
            {
                itemDB.TotalQuantity += quantity;
            }
            else
            {
                itemDB.TotalQuantity -= quantity;
            }
            _currentViewModel.DbContext.Items.Update(itemDB);

            if (itemInNormForChanges.FirstOrDefault(i => i.ItemId == itemDB.Id &&
            i.InvoiceId == invoice.Id) == null)
            {
                itemInNormForChanges.Add(new ItemInNormForChange()
                {
                    ItemId = itemDB.Id,
                    UnitPrice = itemDB.SellingUnitPrice,
                    InvoiceId = invoice.Id
                });
            }
            _currentViewModel.DbContext.SaveChanges();
        }
        private void IncreaseNorm(InvoiceDB invoice,
        ItemDB itemDB,
        decimal quantity,
        List<ItemInNormForChange> itemInNormForChanges)
        {
            if (invoice.TransactionType == 0)
            {
                itemDB.TotalQuantity -= quantity;
            }
            else
            {
                itemDB.TotalQuantity += quantity;
            }
            _currentViewModel.DbContext.Items.Update(itemDB);

            var itemForChange = itemInNormForChanges.FirstOrDefault(i => i.ItemId == itemDB.Id &&
            i.InvoiceId == invoice.Id);

            int index = _currentViewModel.DbContext.ItemInvoices.Where(itemInvoice => itemInvoice.InvoiceId == invoice.Id).Max(i => i.Id) + 1;

            ItemInvoiceDB? itemInvoiceDB = null;
            if (itemForChange != null)
            {
                itemInvoiceDB = new ItemInvoiceDB()
                {
                    Id = index,
                    InvoiceId = invoice.Id,
                    IsSirovina = 1,
                    ItemCode = itemDB.Id,
                    Label = itemDB.Label,
                    Name = itemDB.Name,
                    Quantity = quantity,
                    OriginalUnitPrice = itemForChange.UnitPrice,
                    UnitPrice = itemForChange.UnitPrice,
                    TotalAmout = itemForChange.UnitPrice * quantity
                };
            }
            else
            {
                itemInvoiceDB = new ItemInvoiceDB()
                {
                    Id = index,
                    InvoiceId = invoice.Id,
                    IsSirovina = 1,
                    ItemCode = itemDB.Id,
                    Label = itemDB.Label,
                    Name = itemDB.Name,
                    Quantity = quantity,
                    OriginalUnitPrice = itemDB.SellingUnitPrice,
                    UnitPrice = itemDB.SellingUnitPrice,
                    TotalAmout = itemDB.SellingUnitPrice * quantity
                };
            }
            _currentViewModel.DbContext.ItemInvoices.Add(itemInvoiceDB);
            _currentViewModel.DbContext.SaveChanges();
        }
        private void IncreaseNormNotNorm(SqlServerDbContext sqliteDbContext,
        InvoiceDB invoice,
        ItemDB itemDB,
        decimal quantity)
        {
            if (invoice.TransactionType == 0)
            {
                itemDB.TotalQuantity -= quantity;
            }
            else
            {
                itemDB.TotalQuantity += quantity;
            }
            sqliteDbContext.Items.Update(itemDB);

            sqliteDbContext.Items.Update(itemDB);
            sqliteDbContext.SaveChanges();
        }

    }
    internal class ItemInNormForChange
    {
        public string? InvoiceId { get; set; }
        public string? ItemId { get; set; }
        public decimal? UnitPrice { get; set; }
    }
}