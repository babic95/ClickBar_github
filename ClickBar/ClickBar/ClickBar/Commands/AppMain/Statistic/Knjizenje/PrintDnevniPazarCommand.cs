using ClickBar.Enums.AppMain.Statistic;
using ClickBar.Models.AppMain.Statistic;
using ClickBar.Models.Sale;
using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar_DatabaseSQLManager.Models;
using ClickBar_DatabaseSQLManager;
using ClickBar_Report.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using ClickBar_Logging;
using ClickBar.ViewModels;
using System.Collections.ObjectModel;
using ClickBar_Printer;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;

namespace ClickBar.Commands.AppMain.Statistic.Knjizenje
{
    public class PrintDnevniPazarCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ViewModelBase _currentViewModel;
        private SqlServerDbContext _dbContext;

        public PrintDnevniPazarCommand(ViewModelBase currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            bool is1010 = parameter != null ? true : false;
            ObservableCollection<Invoice> invoices;
            if(_currentViewModel is KnjizenjeViewModel)
            {
                KnjizenjeViewModel knjizenjeViewModel = (KnjizenjeViewModel)_currentViewModel;

                _dbContext = knjizenjeViewModel.DbContext;

                invoices = new ObservableCollection<Invoice>(knjizenjeViewModel.Invoices.OrderBy(i => i.SdcDateTime));
            }
            else if(_currentViewModel is PregledPazaraViewModel)
            {
                PregledPazaraViewModel pregledPazaraViewModel = (PregledPazaraViewModel)_currentViewModel;

                _dbContext = pregledPazaraViewModel.DbContext;

                invoices = new ObservableCollection<Invoice>(pregledPazaraViewModel.Invoices.OrderBy(i => i.SdcDateTime));
            }
            else
            {
                return;
            }

            if (invoices.Any())
            {
                try
                {
                    Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItems10PDV = new Dictionary<string, Dictionary<string, List<ReportPerItems>>>();
                    Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItems20PDV = new Dictionary<string, Dictionary<string, List<ReportPerItems>>>();
                    Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItems0PDV = new Dictionary<string, Dictionary<string, List<ReportPerItems>>>();
                    Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsNoPDV = new Dictionary<string, Dictionary<string, List<ReportPerItems>>>();

                    Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsSirovina10PDV = new Dictionary<string, Dictionary<string, List<ReportPerItems>>>();
                    Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsSirovina20PDV = new Dictionary<string, Dictionary<string, List<ReportPerItems>>>();
                    Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsSirovina0PDV = new Dictionary<string, Dictionary<string, List<ReportPerItems>>>();
                    Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsSirovinaNoPDV = new Dictionary<string, Dictionary<string, List<ReportPerItems>>>();

                    decimal nivelacija = 0;

                    // Učitaj sve potrebne invoice id-jeve
                    var invoiceIds = invoices.Select(x => x.Id).ToList();

                    // Učitaj Invoice iz baze sa svim ItemInvoice
                    var invoiceDBs = _dbContext.Invoices
                        .Where(inv => invoiceIds.Contains(inv.Id))
                        .ToList();

                    var itemInvoices = _dbContext.ItemInvoices
                        .Where(ii => invoiceIds.Contains(ii.InvoiceId))
                        .ToList();

                    // Učitaj sve ItemCode iz itemInvoices gde nije null ili empty
                    var itemCodes = itemInvoices
                        .Where(ii => !string.IsNullOrEmpty(ii.ItemCode))
                        .Select(ii => ii.ItemCode)
                        .Distinct()
                        .ToList();

                    // Učitaj sve ItemDB objekte u batch-u
                    List<ItemDB> itemsDB;
                    if (is1010)
                    {
                        itemsDB = _dbContext.Items
                            .Where(item => itemCodes.Contains(item.Id)
                                && item.InputUnitPrice != null
                                && item.InputUnitPrice.HasValue)
                            .ToList();
                    }
                    else
                    {
                        itemsDB = _dbContext.Items
                            .Where(item => itemCodes.Contains(item.Id))
                            .ToList();
                    }

                    // Učitaj ItemGroups
                    var groupIds = itemsDB.Select(i => i.IdItemGroup).Distinct().ToList();
                    var groupsDB = _dbContext.ItemGroups
                        .Where(g => groupIds.Contains(g.Id))
                        .ToDictionary(g => g.Id);

                    // Dictionary za brzi pristup
                    var itemsDBDict = itemsDB.ToDictionary(i => i.Id);

                    foreach (var invoice in invoices)
                    {
                        var invoiceDB = invoiceDBs.FirstOrDefault(inv => inv.Id == invoice.Id);
                        if (invoiceDB == null) continue;

                        var relatedItems = itemInvoices.Where(ii => ii.InvoiceId == invoiceDB.Id);
                        foreach (var itemInvoiceDB in relatedItems)
                        {
                            if (string.IsNullOrEmpty(itemInvoiceDB.ItemCode)) continue;

                            if (itemsDBDict.TryGetValue(itemInvoiceDB.ItemCode, out var itemDB) && itemInvoiceDB.Quantity.HasValue)
                            {
                                if (!groupsDB.TryGetValue(itemDB.IdItemGroup, out var groupDB)) continue;

                                var item = new Item(itemDB);
                                var itemInvoice = new ItemInvoice(item, itemInvoiceDB);

                                bool isRefund = invoice.TransactionType == Enums.Sale.TransactionTypeEnumeration.Refundacija;

                                Action<ItemInvoice, ItemDB, ItemGroupDB> setPDV = null;

                                // Možeš napraviti mapu labela u akcije za još brži kod, ali ovako je jasnije:
                                switch (itemDB.Label)
                                {
                                    case "Ђ":
                                    case "6":
                                    case "Ж":
                                    case "8":
                                        setPDV = itemInvoice.IsSirovina
                                            ? (ii, idb, gdb) => SetItemsPDV(allItemsSirovina20PDV, ii, nivelacija, isRefund, idb, is1010, gdb)
                                            : (ii, idb, gdb) => SetItemsPDV(allItems20PDV, ii, nivelacija, isRefund, idb, is1010, gdb);
                                        break;
                                    case "Е":
                                    case "7":
                                    case "A":
                                    case "31":
                                        setPDV = itemInvoice.IsSirovina
                                            ? (ii, idb, gdb) => SetItemsPDV(allItemsSirovina10PDV, ii, nivelacija, isRefund, idb, is1010, gdb)
                                            : (ii, idb, gdb) => SetItemsPDV(allItems10PDV, ii, nivelacija, isRefund, idb, is1010, gdb);
                                        break;
                                    case "Г":
                                    case "4":
                                    case "P":
                                    case "49":
                                        setPDV = itemInvoice.IsSirovina
                                            ? (ii, idb, gdb) => SetItemsPDV(allItemsSirovina0PDV, ii, nivelacija, isRefund, idb, is1010, gdb)
                                            : (ii, idb, gdb) => SetItemsPDV(allItems0PDV, ii, nivelacija, isRefund, idb, is1010, gdb);
                                        break;
                                    case "А":
                                    case "1":
                                    case "N":
                                    case "47":
                                        setPDV = itemInvoice.IsSirovina
                                            ? (ii, idb, gdb) => SetItemsPDV(allItemsSirovinaNoPDV, ii, nivelacija, isRefund, idb, is1010, gdb)
                                            : (ii, idb, gdb) => SetItemsPDV(allItemsNoPDV, ii, nivelacija, isRefund, idb, is1010, gdb);
                                        break;
                                }

                                setPDV?.Invoke(itemInvoice, itemDB, groupDB);
                            }
                        }
                    }

                    if (_currentViewModel is KnjizenjeViewModel)
                    {
                        KnjizenjeViewModel knjizenjeViewModel = (KnjizenjeViewModel)_currentViewModel;

                        if (is1010)
                        {
                            PrinterManager.Instance.PrintIzlaz1010(knjizenjeViewModel.DbContext, knjizenjeViewModel.CurrentDate, null,
                                allItems20PDV,
                                allItems10PDV,
                                allItems0PDV,
                                allItemsNoPDV,
                                allItemsSirovina20PDV,
                                allItemsSirovina10PDV,
                                allItemsSirovina0PDV,
                                allItemsSirovinaNoPDV);
                        }
                        else
                        {
                            PrinterManager.Instance.PrintDnevniPazar(knjizenjeViewModel.DbContext, knjizenjeViewModel.CurrentDate, null,
                                allItems20PDV,
                                allItems10PDV,
                                allItems0PDV,
                                allItemsNoPDV,
                                allItemsSirovina20PDV,
                                allItemsSirovina10PDV,
                                allItemsSirovina0PDV,
                                allItemsSirovinaNoPDV);
                        }

                        knjizenjeViewModel.SearchInvoicesCommand.Execute(null);
                    }
                    else if (_currentViewModel is PregledPazaraViewModel)
                    {
                        PregledPazaraViewModel pregledPazaraViewModel = (PregledPazaraViewModel)_currentViewModel;

                        if (is1010)
                        {
                            PrinterManager.Instance.PrintIzlaz1010(pregledPazaraViewModel.DbContext, 
                                pregledPazaraViewModel.FromDate, pregledPazaraViewModel.ToDate,
                                allItems20PDV,
                                allItems10PDV,
                                allItems0PDV,
                                allItemsNoPDV,
                                allItemsSirovina20PDV,
                                allItemsSirovina10PDV,
                                allItemsSirovina0PDV,
                                allItemsSirovinaNoPDV);
                        }
                        else
                        {
                            PrinterManager.Instance.PrintDnevniPazar(pregledPazaraViewModel.DbContext,
                                pregledPazaraViewModel.FromDate, pregledPazaraViewModel.ToDate,
                                allItems20PDV,
                                allItems10PDV,
                                allItems0PDV,
                                allItemsNoPDV,
                                allItemsSirovina20PDV,
                                allItemsSirovina10PDV,
                                allItemsSirovina0PDV,
                                allItemsSirovinaNoPDV);
                        }

                        pregledPazaraViewModel.SearchInvoicesCommand.Execute(null);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("PrintDnevniPazarCommand - Greska prilikom stampe pazara -> ", ex);
                }
            }
        }
        private void SetItemsPDV(
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsPDV,
            ItemInvoice itemInvoice,
            decimal nivelacija,
            bool isRefund,
            ItemDB itemDB,
            bool is1010,
            ItemGroupDB itemGroupDB)
        {
                if (!allItemsPDV.ContainsKey(itemGroupDB.Name))
                {
                    allItemsPDV.Add(itemGroupDB.Name, new Dictionary<string, List<ReportPerItems>>());
                }

            if (itemInvoice.IsSirovina)
            {
                if (allItemsPDV[itemGroupDB.Name].ContainsKey(itemInvoice.Item.Id))
                {
                    var it = allItemsPDV[itemGroupDB.Name][itemInvoice.Item.Id].FirstOrDefault();

                    if (it != null)
                    {
                        if (!isRefund)
                        {
                            it.Quantity += itemInvoice.Quantity;

                            if (itemInvoice.InputUnitPrice != null &&
                                itemInvoice.InputUnitPrice.HasValue &&
                                is1010)
                            {
                                it.TotalAmount += itemInvoice.InputUnitPrice.Value * itemInvoice.Quantity;
                                it.MPC_Average = it.TotalAmount == 0 || it.Quantity == 0 ? 0 : Decimal.Round(it.TotalAmount / it.Quantity, 2);
                            }
                            else
                            {
                                it.TotalAmount += itemInvoice.TotalAmout;
                                it.MPC_Average = it.TotalAmount > 0 && it.Quantity > 0 ? Decimal.Round(it.TotalAmount / it.Quantity, 2) : 0;
                                it.MPC_Original = it.MPC_Average;
                            }
                        }
                        else
                        {
                            it.Quantity -= itemInvoice.Quantity;

                            if (itemInvoice.InputUnitPrice != null &&
                                itemInvoice.InputUnitPrice.HasValue &&
                                is1010)
                            {
                                it.TotalAmount -= itemInvoice.InputUnitPrice.Value * itemInvoice.Quantity;
                                it.MPC_Average = it.TotalAmount == 0 || it.Quantity == 0 ? 0 : Decimal.Round(it.TotalAmount / it.Quantity, 2);
                            }
                            else
                            {
                                it.TotalAmount -= itemInvoice.TotalAmout;
                                it.MPC_Average = it.TotalAmount > 0 && it.Quantity > 0 ? Decimal.Round(it.TotalAmount / it.Quantity, 2) : 0;
                                it.MPC_Original = it.MPC_Average;
                            }
                        }

                        if (itemDB.IdNorm == null)
                        {
                            if (itemInvoice.InputUnitPrice.HasValue)
                            {
                                it.TotalInputPrice += !isRefund ? decimal.Round(itemInvoice.Quantity * itemInvoice.InputUnitPrice.Value, 2) : decimal.Round(itemInvoice.Quantity * -1 * itemInvoice.InputUnitPrice.Value, 2);
                            }
                        }
                        else
                        {
                            decimal inputPrice = 0;

                            var itemsNorm1 = _dbContext.ItemsInNorm.Include(i => i.Item).Where(n => n.IdNorm == itemDB.IdNorm);

                            if (itemsNorm1 != null && itemsNorm1.Any())
                            {
                                foreach (var itemNorm1 in itemsNorm1)
                                {
                                    var itemsNorm2 = _dbContext.ItemsInNorm.Include(i => i.Item).Where(n => itemNorm1.Item.IdNorm != null &&
                                    n.IdNorm == itemNorm1.Item.IdNorm);

                                    if (itemsNorm2 != null && itemsNorm2.Any())
                                    {
                                        foreach (var itemNorm2 in itemsNorm2)
                                        {
                                            var itemsNorm3 = _dbContext.ItemsInNorm.Include(i => i.Item).Where(n => itemNorm2.Item.IdNorm != null &&
                                            n.IdNorm == itemNorm2.Item.IdNorm);

                                            if (itemsNorm3 != null && itemsNorm3.Any())
                                            {
                                                foreach (var itemNorm3 in itemsNorm3)
                                                {
                                                    var normInInvoice = _dbContext.ItemInvoices.FirstOrDefault(ii => ii.ItemCode == itemNorm3.IdItem &&
                                                    ii.InvoiceId == itemInvoice.InvoiceId);

                                                    if (normInInvoice != null && normInInvoice.InputUnitPrice.HasValue)
                                                    {
                                                        inputPrice += decimal.Round(itemInvoice.Quantity *
                                                            itemNorm3.Quantity *
                                                            itemNorm2.Quantity *
                                                            itemNorm1.Quantity *
                                                            normInInvoice.InputUnitPrice.Value, 2);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                var normInInvoice = _dbContext.ItemInvoices.FirstOrDefault(ii => ii.ItemCode == itemNorm2.IdItem &&
                                                ii.InvoiceId == itemInvoice.InvoiceId);

                                                if (normInInvoice != null && normInInvoice.InputUnitPrice.HasValue)
                                                {
                                                    inputPrice += decimal.Round(itemInvoice.Quantity *
                                                        itemNorm2.Quantity *
                                                        itemNorm1.Quantity *
                                                        normInInvoice.InputUnitPrice.Value, 2);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var normInInvoice = _dbContext.ItemInvoices.FirstOrDefault(ii => ii.ItemCode == itemNorm1.IdItem &&
                                        ii.InvoiceId == itemInvoice.InvoiceId);

                                        if (normInInvoice != null && normInInvoice.InputUnitPrice.HasValue)
                                        {
                                            inputPrice += decimal.Round(itemInvoice.Quantity *
                                                itemNorm1.Quantity *
                                                normInInvoice.InputUnitPrice.Value, 2);
                                        }
                                    }
                                }

                                if (inputPrice > 0)
                                {
                                    it.TotalInputPrice += !isRefund ? inputPrice : -1 * inputPrice;
                                }
                            }
                            else
                            {
                                if (itemInvoice.InputUnitPrice.HasValue)
                                {
                                    it.TotalInputPrice += !isRefund ? decimal.Round(itemInvoice.Quantity * itemInvoice.InputUnitPrice.Value, 2) : decimal.Round(itemInvoice.Quantity * -1 * itemInvoice.InputUnitPrice.Value, 2);
                                }
                            }
                        }
                    }
                }
                else
                {
                    ReportPerItems reportPerItems = new ReportPerItems()
                    {
                        ItemId = itemInvoice.Item.Id,
                        JM = itemInvoice.Item.Jm,
                        Name = itemInvoice.Item.Name,
                        Quantity = isRefund ? -1 * itemInvoice.Quantity : itemInvoice.Quantity,
                        //TotalAmount = isRefund ? -1 * itemInvoice.TotalAmout : itemInvoice.TotalAmout,
                        //MPC_Average = itemInvoice.Item.SellingUnitPrice,
                        MPC_Original = itemInvoice.Item.OriginalUnitPrice,
                        IsSirovina = itemInvoice.IsSirovina,
                        Nivelacija = 0,
                    };

                    if (itemInvoice.InputUnitPrice != null &&
                            itemInvoice.InputUnitPrice.HasValue &&
                            is1010)
                    {
                        reportPerItems.TotalAmount = isRefund ? -1 * itemInvoice.InputUnitPrice.Value * itemInvoice.Quantity :
                            itemInvoice.InputUnitPrice.Value * itemInvoice.Quantity;
                        reportPerItems.MPC_Average = itemInvoice.InputUnitPrice.Value;
                    }
                    else
                    {
                        reportPerItems.TotalAmount = isRefund ? -1 * itemInvoice.TotalAmout : itemInvoice.TotalAmout;
                        reportPerItems.MPC_Average = itemInvoice.Item.SellingUnitPrice;

                        if (itemDB.IdNorm == null)
                        {
                            if (itemInvoice.InputUnitPrice.HasValue)
                            {
                                reportPerItems.TotalInputPrice += !isRefund ? decimal.Round(itemInvoice.Quantity * itemInvoice.InputUnitPrice.Value, 2) : decimal.Round(itemInvoice.Quantity * -1 * itemInvoice.InputUnitPrice.Value, 2);
                            }
                        }
                        else
                        {
                            decimal inputPrice = 0;

                            var itemsNorm1 = _dbContext.ItemsInNorm.Include(i => i.Item).Where(n => n.IdNorm == itemDB.IdNorm);

                            if (itemsNorm1 != null && itemsNorm1.Any())
                            {
                                foreach (var itemNorm1 in itemsNorm1)
                                {
                                    var itemsNorm2 = _dbContext.ItemsInNorm.Include(i => i.Item).Where(n => itemNorm1.Item.IdNorm != null &&
                                    n.IdNorm == itemNorm1.Item.IdNorm);

                                    if (itemsNorm2 != null && itemsNorm2.Any())
                                    {
                                        foreach (var itemNorm2 in itemsNorm2)
                                        {
                                            var itemsNorm3 = _dbContext.ItemsInNorm.Include(i => i.Item).Where(n => itemNorm2.Item.IdNorm != null &&
                                            n.IdNorm == itemNorm2.Item.IdNorm);

                                            if (itemsNorm3 != null && itemsNorm3.Any())
                                            {
                                                foreach (var itemNorm3 in itemsNorm3)
                                                {
                                                    var normInInvoice = _dbContext.ItemInvoices.FirstOrDefault(ii => ii.ItemCode == itemNorm3.IdItem &&
                                                    ii.InvoiceId == itemInvoice.InvoiceId);

                                                    if (normInInvoice != null && normInInvoice.InputUnitPrice.HasValue)
                                                    {
                                                        inputPrice += decimal.Round(itemInvoice.Quantity *
                                                            itemNorm3.Quantity *
                                                            itemNorm2.Quantity *
                                                            itemNorm1.Quantity *
                                                            normInInvoice.InputUnitPrice.Value, 2);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                var normInInvoice = _dbContext.ItemInvoices.FirstOrDefault(ii => ii.ItemCode == itemNorm2.IdItem &&
                                                ii.InvoiceId == itemInvoice.InvoiceId);

                                                if (normInInvoice != null && normInInvoice.InputUnitPrice.HasValue)
                                                {
                                                    inputPrice += decimal.Round(itemInvoice.Quantity *
                                                        itemNorm2.Quantity *
                                                        itemNorm1.Quantity *
                                                        normInInvoice.InputUnitPrice.Value, 2);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var normInInvoice = _dbContext.ItemInvoices.FirstOrDefault(ii => ii.ItemCode == itemNorm1.IdItem &&
                                        ii.InvoiceId == itemInvoice.InvoiceId);

                                        if (normInInvoice != null && normInInvoice.InputUnitPrice.HasValue)
                                        {
                                            inputPrice += decimal.Round(itemInvoice.Quantity *
                                                itemNorm1.Quantity *
                                                normInInvoice.InputUnitPrice.Value, 2);
                                        }
                                    }
                                }

                                if (inputPrice > 0)
                                {
                                    reportPerItems.TotalInputPrice += !isRefund ? inputPrice : -1 * inputPrice;
                                }
                            }
                            else
                            {
                                if (itemInvoice.InputUnitPrice.HasValue)
                                {
                                    reportPerItems.TotalInputPrice += !isRefund ? decimal.Round(itemInvoice.Quantity * itemInvoice.InputUnitPrice.Value, 2) : decimal.Round(itemInvoice.Quantity * -1 * itemInvoice.InputUnitPrice.Value, 2);
                                }
                            }
                        }
                    }

                    allItemsPDV[itemGroupDB.Name].Add(itemInvoice.Item.Id, new List<ReportPerItems>() { reportPerItems });
                }
            }
            else
            {
                if (allItemsPDV[itemGroupDB.Name].ContainsKey(itemInvoice.Item.Id))
                {
                    var it = allItemsPDV[itemGroupDB.Name][itemInvoice.Item.Id].FirstOrDefault(i => i.MPC_Original == itemInvoice.Item.OriginalUnitPrice);

                    if (it != null)
                    {
                        if (!isRefund)
                        {
                            it.Quantity += itemInvoice.Quantity;

                            if (itemInvoice.InputUnitPrice != null &&
                                itemInvoice.InputUnitPrice.HasValue &&
                                is1010)
                            {
                                it.TotalAmount += itemInvoice.InputUnitPrice.Value * itemInvoice.Quantity;
                                it.MPC_Average = it.TotalAmount == 0 || it.Quantity == 0 ? 0 : Decimal.Round(it.TotalAmount / it.Quantity, 2);
                            }
                            else
                            {
                                it.TotalAmount += itemInvoice.TotalAmout;
                                it.MPC_Average = it.TotalAmount > 0 && it.Quantity > 0 ? Decimal.Round(it.TotalAmount / it.Quantity, 2) : 0;
                            }

                            nivelacija -= it.Nivelacija;

                            decimal niv = -1 * Decimal.Round((it.Quantity * it.MPC_Original) - it.TotalAmount, 2);

                            it.Nivelacija = niv;
                            nivelacija += it.Nivelacija;
                        }
                        else
                        {
                            it.Quantity -= itemInvoice.Quantity;

                            if (itemInvoice.InputUnitPrice != null &&
                                itemInvoice.InputUnitPrice.HasValue &&
                                is1010)
                            {
                                it.TotalAmount -= itemInvoice.InputUnitPrice.Value * itemInvoice.Quantity;
                                it.MPC_Average = it.TotalAmount == 0 || it.Quantity == 0 ? 0 : Decimal.Round(it.TotalAmount / it.Quantity, 2);
                            }
                            else
                            {
                                it.TotalAmount -= itemInvoice.TotalAmout;
                            }
                        }

                        if (itemDB.IdNorm == null)
                        {
                            if (itemInvoice.InputUnitPrice.HasValue)
                            {
                                it.TotalInputPrice += !isRefund ? decimal.Round(itemInvoice.Quantity * itemInvoice.InputUnitPrice.Value, 2) : decimal.Round(itemInvoice.Quantity * -1 * itemInvoice.InputUnitPrice.Value, 2);
                            }
                        }
                        else
                        {
                            decimal inputPrice = 0;

                            var itemsNorm1 = _dbContext.ItemsInNorm.Include(i => i.Item).Where(n => n.IdNorm == itemDB.IdNorm);

                            if (itemsNorm1 != null && itemsNorm1.Any())
                            {
                                foreach (var itemNorm1 in itemsNorm1)
                                {
                                    var itemsNorm2 = _dbContext.ItemsInNorm.Include(i => i.Item).Where(n => itemNorm1.Item.IdNorm != null &&
                                    n.IdNorm == itemNorm1.Item.IdNorm);

                                    if (itemsNorm2 != null && itemsNorm2.Any())
                                    {
                                        foreach (var itemNorm2 in itemsNorm2)
                                        {
                                            var itemsNorm3 = _dbContext.ItemsInNorm.Include(i => i.Item).Where(n => itemNorm2.Item.IdNorm != null &&
                                            n.IdNorm == itemNorm2.Item.IdNorm);

                                            if (itemsNorm3 != null && itemsNorm3.Any())
                                            {
                                                foreach (var itemNorm3 in itemsNorm3)
                                                {
                                                    var normInInvoice = _dbContext.ItemInvoices.FirstOrDefault(ii => ii.ItemCode == itemNorm3.IdItem &&
                                                    ii.InvoiceId == itemInvoice.InvoiceId);

                                                    if (normInInvoice != null && normInInvoice.InputUnitPrice.HasValue)
                                                    {
                                                        inputPrice += decimal.Round(itemInvoice.Quantity *
                                                            itemNorm3.Quantity *
                                                            itemNorm2.Quantity *
                                                            itemNorm1.Quantity *
                                                            normInInvoice.InputUnitPrice.Value, 2);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                var normInInvoice = _dbContext.ItemInvoices.FirstOrDefault(ii => ii.ItemCode == itemNorm2.IdItem &&
                                                ii.InvoiceId == itemInvoice.InvoiceId);

                                                if (normInInvoice != null && normInInvoice.InputUnitPrice.HasValue)
                                                {
                                                    inputPrice += decimal.Round(itemInvoice.Quantity *
                                                        itemNorm2.Quantity *
                                                        itemNorm1.Quantity *
                                                        normInInvoice.InputUnitPrice.Value, 2);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var normInInvoice = _dbContext.ItemInvoices.FirstOrDefault(ii => ii.ItemCode == itemNorm1.IdItem &&
                                        ii.InvoiceId == itemInvoice.InvoiceId);

                                        if (normInInvoice != null && normInInvoice.InputUnitPrice.HasValue)
                                        {
                                            inputPrice += decimal.Round(itemInvoice.Quantity *
                                                itemNorm1.Quantity *
                                                normInInvoice.InputUnitPrice.Value, 2);
                                        }
                                    }
                                }

                                if (inputPrice > 0)
                                {
                                    it.TotalInputPrice += !isRefund ? inputPrice : -1 * inputPrice;
                                }
                            }
                            else
                            {
                                if (itemInvoice.InputUnitPrice.HasValue)
                                {
                                    it.TotalInputPrice += !isRefund ? decimal.Round(itemInvoice.Quantity * itemInvoice.InputUnitPrice.Value, 2) : decimal.Round(itemInvoice.Quantity * -1 * itemInvoice.InputUnitPrice.Value, 2);
                                }
                            }
                        }
                    }
                    else
                    {
                        ReportPerItems reportPerItems = new ReportPerItems()
                        {
                            ItemId = itemInvoice.Item.Id,
                            JM = itemInvoice.Item.Jm,
                            Name = itemInvoice.Item.Name,
                            Quantity = isRefund ? -1 * itemInvoice.Quantity : itemInvoice.Quantity,
                            //TotalAmount = isRefund ? -1 * itemInvoice.TotalAmout : itemInvoice.TotalAmout,
                            //MPC_Average = itemInvoice.Item.SellingUnitPrice,
                            //TotalInputPrice = 0,
                            MPC_Original = itemInvoice.Item.OriginalUnitPrice,
                            IsSirovina = itemInvoice.IsSirovina,
                            Nivelacija = isRefund ? 0 : -1 * Decimal.Round((itemInvoice.Item.OriginalUnitPrice * itemInvoice.Quantity) - itemInvoice.TotalAmout, 2),
                        };

                        if (itemInvoice.InputUnitPrice != null &&
                                itemInvoice.InputUnitPrice.HasValue &&
                                is1010)
                        {
                            reportPerItems.TotalAmount = isRefund ? -1 * itemInvoice.InputUnitPrice.Value * itemInvoice.Quantity :
                                itemInvoice.InputUnitPrice.Value * itemInvoice.Quantity;
                            reportPerItems.MPC_Average = itemInvoice.InputUnitPrice.Value;
                        }
                        else
                        {
                            reportPerItems.TotalAmount = isRefund ? -1 * itemInvoice.TotalAmout : itemInvoice.TotalAmout;
                            reportPerItems.MPC_Average = itemInvoice.Item.SellingUnitPrice;
                        }

                        //decimal niv = -1 * Decimal.Round((itemInvoice.Item.OriginalUnitPrice * itemInvoice.Quantity) - itemInvoice.TotalAmout, 2);

                        //reportPerItems.Nivelacija = niv > -1 && niv < 1 ? 0 : niv;
                        allItemsPDV[itemGroupDB.Name][itemInvoice.Item.Id].Add(reportPerItems);

                        nivelacija += reportPerItems.Nivelacija;

                        if (itemDB.IdNorm == null)
                        {
                            if (itemInvoice.InputUnitPrice.HasValue)
                            {
                                reportPerItems.TotalInputPrice += !isRefund ? decimal.Round(itemInvoice.Quantity * itemInvoice.InputUnitPrice.Value, 2) : decimal.Round(itemInvoice.Quantity * -1 * itemInvoice.InputUnitPrice.Value, 2);
                            }
                        }
                        else
                        {
                            decimal inputPrice = 0;

                            var itemsNorm1 = _dbContext.ItemsInNorm.Include(i => i.Item).Where(n => n.IdNorm == itemDB.IdNorm);

                            if (itemsNorm1 != null && itemsNorm1.Any())
                            {
                                foreach (var itemNorm1 in itemsNorm1)
                                {
                                    var itemsNorm2 = _dbContext.ItemsInNorm.Include(i => i.Item).Where(n => itemNorm1.Item.IdNorm != null &&
                                    n.IdNorm == itemNorm1.Item.IdNorm);

                                    if (itemsNorm2 != null && itemsNorm2.Any())
                                    {
                                        foreach (var itemNorm2 in itemsNorm2)
                                        {
                                            var itemsNorm3 = _dbContext.ItemsInNorm.Include(i => i.Item).Where(n => itemNorm2.Item.IdNorm != null &&
                                            n.IdNorm == itemNorm2.Item.IdNorm);

                                            if (itemsNorm3 != null && itemsNorm3.Any())
                                            {
                                                foreach (var itemNorm3 in itemsNorm3)
                                                {
                                                    var normInInvoice = _dbContext.ItemInvoices.FirstOrDefault(ii => ii.ItemCode == itemNorm3.IdItem &&
                                                    ii.InvoiceId == itemInvoice.InvoiceId);

                                                    if (normInInvoice != null && normInInvoice.InputUnitPrice.HasValue)
                                                    {
                                                        inputPrice += decimal.Round(itemInvoice.Quantity *
                                                            itemNorm3.Quantity *
                                                            itemNorm2.Quantity *
                                                            itemNorm1.Quantity *
                                                            normInInvoice.InputUnitPrice.Value, 2);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                var normInInvoice = _dbContext.ItemInvoices.FirstOrDefault(ii => ii.ItemCode == itemNorm2.IdItem &&
                                                ii.InvoiceId == itemInvoice.InvoiceId);

                                                if (normInInvoice != null && normInInvoice.InputUnitPrice.HasValue)
                                                {
                                                    inputPrice += decimal.Round(itemInvoice.Quantity *
                                                        itemNorm2.Quantity *
                                                        itemNorm1.Quantity *
                                                        normInInvoice.InputUnitPrice.Value, 2);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var normInInvoice = _dbContext.ItemInvoices.FirstOrDefault(ii => ii.ItemCode == itemNorm1.IdItem &&
                                        ii.InvoiceId == itemInvoice.InvoiceId);

                                        if (normInInvoice != null && normInInvoice.InputUnitPrice.HasValue)
                                        {
                                            inputPrice += decimal.Round(itemInvoice.Quantity *
                                                itemNorm1.Quantity *
                                                normInInvoice.InputUnitPrice.Value, 2);
                                        }
                                    }
                                }

                                if (inputPrice > 0)
                                {
                                    reportPerItems.TotalInputPrice += !isRefund ? inputPrice : -1 * inputPrice;
                                }
                            }
                            else
                            {
                                if (itemInvoice.InputUnitPrice.HasValue)
                                {
                                    reportPerItems.TotalInputPrice += !isRefund ? decimal.Round(itemInvoice.Quantity * itemInvoice.InputUnitPrice.Value, 2) : decimal.Round(itemInvoice.Quantity * -1 * itemInvoice.InputUnitPrice.Value, 2);
                                }
                            }
                        }
                    }
                }
                else
                {
                    ReportPerItems reportPerItems = new ReportPerItems()
                    {
                        ItemId = itemInvoice.Item.Id,
                        JM = itemInvoice.Item.Jm,
                        Name = itemInvoice.Item.Name,
                        Quantity = isRefund ? -1 * itemInvoice.Quantity : itemInvoice.Quantity,
                        //TotalAmount = isRefund ? -1 * itemInvoice.TotalAmout : itemInvoice.TotalAmout,
                        //MPC_Average = itemInvoice.Item.SellingUnitPrice,
                        //TotalInputPrice = 0,
                        MPC_Original = itemInvoice.Item.OriginalUnitPrice,
                        IsSirovina = itemInvoice.IsSirovina,
                        Nivelacija = isRefund ? 0 : -1 * Decimal.Round((itemInvoice.Item.OriginalUnitPrice * itemInvoice.Quantity) - itemInvoice.TotalAmout, 2),
                    };

                    if (itemInvoice.InputUnitPrice != null &&
                            itemInvoice.InputUnitPrice.HasValue &&
                            is1010)
                    {
                        reportPerItems.TotalAmount = isRefund ? -1 * itemInvoice.InputUnitPrice.Value * itemInvoice.Quantity :
                            itemInvoice.InputUnitPrice.Value * itemInvoice.Quantity;
                        reportPerItems.MPC_Average = itemInvoice.InputUnitPrice.Value;
                    }
                    else
                    {
                        reportPerItems.TotalAmount = isRefund ? -1 * itemInvoice.TotalAmout : itemInvoice.TotalAmout;
                        reportPerItems.MPC_Average = itemInvoice.Item.SellingUnitPrice;
                    }

                    //decimal niv = -1 * Decimal.Round((itemInvoice.Item.OriginalUnitPrice * itemInvoice.Quantity) - itemInvoice.TotalAmout, 2);

                    //reportPerItems.Nivelacija = niv > -1 && niv < 1 ? 0 : niv;

                    allItemsPDV[itemGroupDB.Name].Add(itemInvoice.Item.Id, new List<ReportPerItems>() { reportPerItems });

                    nivelacija += reportPerItems.Nivelacija;

                    if (itemDB.IdNorm == null)
                    {
                        if (itemInvoice.InputUnitPrice.HasValue)
                        {
                            reportPerItems.TotalInputPrice += !isRefund ? decimal.Round(itemInvoice.Quantity * itemInvoice.InputUnitPrice.Value, 2) : decimal.Round(itemInvoice.Quantity * -1 * itemInvoice.InputUnitPrice.Value, 2);
                        }
                    }
                    else
                    {
                        decimal inputPrice = 0;

                        var itemsNorm1 = _dbContext.ItemsInNorm.Include(i => i.Item).Where(n => n.IdNorm == itemDB.IdNorm);

                        if (itemsNorm1 != null && itemsNorm1.Any())
                        {
                            foreach (var itemNorm1 in itemsNorm1)
                            {
                                var itemsNorm2 = _dbContext.ItemsInNorm.Include(i => i.Item).Where(n => itemNorm1.Item.IdNorm != null &&
                                n.IdNorm == itemNorm1.Item.IdNorm);

                                if (itemsNorm2 != null && itemsNorm2.Any())
                                {
                                    foreach (var itemNorm2 in itemsNorm2)
                                    {
                                        var itemsNorm3 = _dbContext.ItemsInNorm.Include(i => i.Item).Where(n => itemNorm2.Item.IdNorm != null &&
                                        n.IdNorm == itemNorm2.Item.IdNorm);

                                        if (itemsNorm3 != null && itemsNorm3.Any())
                                        {
                                            foreach (var itemNorm3 in itemsNorm3)
                                            {
                                                var normInInvoice = _dbContext.ItemInvoices.FirstOrDefault(ii => ii.ItemCode == itemNorm3.IdItem &&
                                                ii.InvoiceId == itemInvoice.InvoiceId);

                                                if (normInInvoice != null && normInInvoice.InputUnitPrice.HasValue)
                                                {
                                                    inputPrice += decimal.Round(itemInvoice.Quantity *
                                                        itemNorm3.Quantity *
                                                        itemNorm2.Quantity *
                                                        itemNorm1.Quantity *
                                                        normInInvoice.InputUnitPrice.Value, 2);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            var normInInvoice = _dbContext.ItemInvoices.FirstOrDefault(ii => ii.ItemCode == itemNorm2.IdItem &&
                                            ii.InvoiceId == itemInvoice.InvoiceId);

                                            if (normInInvoice != null && normInInvoice.InputUnitPrice.HasValue)
                                            {
                                                inputPrice += decimal.Round(itemInvoice.Quantity *
                                                    itemNorm2.Quantity *
                                                    itemNorm1.Quantity *
                                                    normInInvoice.InputUnitPrice.Value, 2);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    var normInInvoice = _dbContext.ItemInvoices.FirstOrDefault(ii => ii.ItemCode == itemNorm1.IdItem &&
                                    ii.InvoiceId == itemInvoice.InvoiceId);

                                    if (normInInvoice != null && normInInvoice.InputUnitPrice.HasValue)
                                    {
                                        inputPrice += decimal.Round(itemInvoice.Quantity *
                                            itemNorm1.Quantity *
                                            normInInvoice.InputUnitPrice.Value, 2);
                                    }
                                }
                            }

                            if (inputPrice > 0)
                            {
                                reportPerItems.TotalInputPrice += !isRefund ? inputPrice : -1 * inputPrice;
                            }
                        }
                        else
                        {
                            if (itemInvoice.InputUnitPrice.HasValue)
                            {
                                reportPerItems.TotalInputPrice += !isRefund ? decimal.Round(itemInvoice.Quantity * itemInvoice.InputUnitPrice.Value, 2) : decimal.Round(itemInvoice.Quantity * -1 * itemInvoice.InputUnitPrice.Value, 2);
                            }
                        }
                    }
                }
            }
        }
    }
}