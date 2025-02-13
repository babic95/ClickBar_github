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
using Microsoft.EntityFrameworkCore;

namespace ClickBar.Commands.AppMain.Statistic.InventoryStatus
{
    public class FixQuantityCommand : ICommand
    {
        private InventoryStatusViewModel _viewModel;

        public event EventHandler CanExecuteChanged;

        public FixQuantityCommand(InventoryStatusViewModel inventoryStatusViewModel)
        {
            _viewModel = inventoryStatusViewModel;
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
                    pocetnoStanjeDB = _viewModel.DbContext.PocetnaStanja.OrderByDescending(p => p.PopisDate).FirstOrDefault();
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

                if (items != null && items.Any())
                {
                    foreach(var itemDB in items)
                    {
                        decimal totalCalculationQuantity = 0;
                        decimal totalOtpisQuantity = 0;
                        decimal totalPazarQuantity = 0;

                        decimal totalQuantityPocetnoStanje = 0;

                        DateTime pocetnoStanjeDate = new DateTime(DateTime.Now.Year, 1, 1, 0, 0, 0);

                        if (pocetnoStanjeDB != null)
                        {
                            var pocetnoStanjeItemDB = _viewModel.DbContext.PocetnaStanjaItems.FirstOrDefault(p => p.IdPocetnoStanje == pocetnoStanjeDB.Id &&
                            p.IdItem == itemDB.Id);

                            if (pocetnoStanjeItemDB != null)
                            {
                                totalQuantityPocetnoStanje = pocetnoStanjeItemDB.NewQuantity;
                            }

                            if (pocetnoStanjeDB.PopisDate.Date > pocetnoStanjeDate.Date)
                            {
                                pocetnoStanjeDate = pocetnoStanjeDB.PopisDate.Date;
                            }
                        }

                        var itemInCal = _viewModel.DbContext.Calculations.Join(_viewModel.DbContext.CalculationItems,
                            cal => cal.Id,
                            calItem => calItem.CalculationId,
                            (cal, calItem) => new { Cal = cal, CalItem = calItem })
                        .Where(cal => cal.CalItem.ItemId == itemDB.Id &&
                        cal.Cal.CalculationDate.Date >= pocetnoStanjeDate.Date);

                        if (itemInCal != null &&
                        itemInCal.Any())
                        {
                            totalCalculationQuantity = itemInCal.Select(i => i.CalItem.Quantity).ToList().Sum();
                        }

                        var itemInOtpis = _viewModel.DbContext.Otpisi.Join(_viewModel.DbContext.OtpisItems,
                            otpis => otpis.Id,
                            otpisItem => otpisItem.OtpisId,
                            (otpis, otpisItem) => new { Otpis = otpis, OtpisItem = otpisItem })
                        .Where(otp => otp.OtpisItem.ItemId == itemDB.Id &&
                        otp.Otpis.OtpisDate.Date >= pocetnoStanjeDate.Date);

                        if (itemInOtpis != null &&
                        itemInOtpis.Any())
                        {
                            totalOtpisQuantity = itemInOtpis.Select(i => i.OtpisItem.Quantity).ToList().Sum();
                        }

                        var pazari = _viewModel.DbContext.Invoices.Join(_viewModel.DbContext.ItemInvoices,
                            invoice => invoice.Id,
                            invoiceItem => invoiceItem.InvoiceId,
                            (invoice, invoiceItem) => new { Inv = invoice, InvItem = invoiceItem })
                            .Where(pazar => pazar.Inv.SdcDateTime != null &&
                            pazar.Inv.SdcDateTime.HasValue &&
                            pazar.InvItem.ItemCode == itemDB.Id &&
                            pazar.Inv.SdcDateTime.Value > pocetnoStanjeDate)
                            .OrderByDescending(item => item.Inv.SdcDateTime);

                        if (itemDB.IdNorm == null &&
                            pazari != null &&
                            pazari.Any())
                        {
                            //var listProdaja = pazari.Select(i => i.Inv.TransactionType != null && i.Inv.TransactionType == 0 &&
                            //i.InvItem.Quantity != null && i.InvItem.Quantity.HasValue ?
                            //i.InvItem.IsSirovina == 1 ? i.InvItem.Quantity.Value :
                            //itemDB.IdNorm == null ? i.InvItem.Quantity.Value : 0 : 0).ToList();

                            //var listRefundacija = pazari.Select(i => i.Inv.TransactionType != null && i.Inv.TransactionType == 1 &&
                            //i.InvItem.Quantity != null && i.InvItem.Quantity.HasValue ?
                            //i.InvItem.IsSirovina == 1 ? i.InvItem.Quantity.Value :
                            //itemDB.IdNorm == null ? i.InvItem.Quantity.Value : 0 : 0).ToList();

                            var listProdaja = pazari.Select(i => i.Inv.TransactionType != null && i.Inv.TransactionType == 0 &&
                            i.InvItem.Quantity != null && i.InvItem.Quantity.HasValue ?
                            i.InvItem.Quantity.Value : 0).ToList();

                            var listRefundacija = pazari.Select(i => i.Inv.TransactionType != null && i.Inv.TransactionType == 1 &&
                            i.InvItem.Quantity != null && i.InvItem.Quantity.HasValue ?
                            i.InvItem.Quantity.Value : 0).ToList();

                            decimal prodaja = listProdaja.Sum();
                            decimal refundacija = listRefundacija.Sum();

                            totalPazarQuantity += prodaja - refundacija;
                        }

                        itemDB.TotalQuantity = (totalQuantityPocetnoStanje + totalCalculationQuantity) - totalOtpisQuantity - totalPazarQuantity;
                        _viewModel.DbContext.Items.Update(itemDB);

                        _viewModel.DbContext.SaveChanges();

                    }
                }

                MessageBox.Show("Uspešno sređivanje količina artikala!",
                    "Uspešno",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch
            {
                MessageBox.Show("Greška prilikom sređivanja količina artikala!", "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}