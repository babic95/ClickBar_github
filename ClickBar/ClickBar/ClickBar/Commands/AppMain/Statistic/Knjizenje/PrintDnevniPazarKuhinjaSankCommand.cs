using ClickBar.Models.Sale;
using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar.ViewModels;
using ClickBar_Database.Models;
using ClickBar_Database;
using ClickBar_Printer;
using ClickBar_Report.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ClickBar_Logging;
using Microsoft.EntityFrameworkCore;
using DocumentFormat.OpenXml.Math;
using System.Windows;

namespace ClickBar.Commands.AppMain.Statistic.Knjizenje
{
    public class PrintDnevniPazarKuhinjaSankCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ViewModelBase _currentViewModel;

        public PrintDnevniPazarKuhinjaSankCommand(ViewModelBase currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            var result = MessageBox.Show("Da li želite da štampate i SIROVINE?",
                "Štampanje sirovina",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            bool enableSirovine = false;

            if(result == MessageBoxResult.Yes)
            {
                enableSirovine = true;
            }

            bool isKuhinja = false;
            string p = string.Empty;
            if (parameter is string)
            {
                p = (string)parameter;
                if (!string.IsNullOrEmpty(p))
                {
                    isKuhinja = p.Contains("K") ? true : false;
                }
            }
            SupergroupDB? supergroupDB = null;
            using (SqliteDbContext sqliteDbContext = new SqliteDbContext())
            {
                if(p == "K")
                {
                    supergroupDB = sqliteDbContext.Supergroups.FirstOrDefault(s => s.Name.ToLower().Equals("kuhinja") ||
                     s.Name.ToLower().Equals("hrana"));
                }
                else if(p == "S")
                {
                    sqliteDbContext.Supergroups.ForEachAsync(s =>
                    {
                        var ss = s.Name.ToLower();

                        if (s.Name.ToLower().Equals("piće") ||
                        s.Name.ToLower().Equals("pice") ||
                        s.Name.ToLower().Equals("šank") ||
                        s.Name.ToLower().Equals("sank"))
                        {
                            supergroupDB = s;
                        }
                    });
                }
            }

            if (supergroupDB != null)
            {
                ObservableCollection<Invoice> invoices;
                if (_currentViewModel is KnjizenjeViewModel)
                {
                    KnjizenjeViewModel knjizenjeViewModel = (KnjizenjeViewModel)_currentViewModel;
                    invoices = new ObservableCollection<Invoice>(knjizenjeViewModel.Invoices.OrderBy(i => i.SdcDateTime));
                }
                else if (_currentViewModel is PregledPazaraViewModel)
                {
                    PregledPazaraViewModel pregledPazaraViewModel = (PregledPazaraViewModel)_currentViewModel;
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
                        using (SqliteDbContext sqliteDbContext = new SqliteDbContext())
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

                            invoices.ToList().ForEach(invoice =>
                            {
                                try
                                {
                                    var invoiceDB = sqliteDbContext.Invoices.FirstOrDefault(inv => inv.Id == invoice.Id);

                                    if (invoiceDB != null)
                                    {
                                        var itemsDB = sqliteDbContext.ItemInvoices.Join(sqliteDbContext.Items,
                                            itemInvoice => itemInvoice.ItemCode,
                                            item => item.Id,
                                            (itemInvoice, item) => new { ItemInvoice = itemInvoice, Item = item })
                                        .Join(sqliteDbContext.ItemGroups,
                                        item => item.Item.IdItemGroup,
                                        group => group.Id,
                                        (item, group) => new { Item = item, Group = group })
                                        .Join(sqliteDbContext.Supergroups,
                                        item => item.Group.IdSupergroup,
                                        supergroup => supergroup.Id,
                                        (item, supergroup) => new { Item = item, Supergroup = supergroup })
                                        .Where(inv => inv.Supergroup.Id == supergroupDB.Id &&
                                        inv.Item.Item.ItemInvoice.InvoiceId == invoiceDB.Id)
                                        .Select(inv => inv.Item.Item.ItemInvoice);

                                        if (itemsDB != null &&
                                            itemsDB.Any())
                                        {
                                            itemsDB.ToList().ForEach(itemInvoiceDB =>
                                            {
                                                if (!string.IsNullOrEmpty(itemInvoiceDB.ItemCode))
                                                {
                                                    ItemDB? itemDB = null;
                                                    if (itemInvoiceDB.IsSirovina.HasValue &&
                                                    itemInvoiceDB.IsSirovina.Value == 1)
                                                    {
                                                        itemDB = sqliteDbContext.Items.FirstOrDefault(item => item.Id == itemInvoiceDB.ItemCode &&
                                                        item.InputUnitPrice != null &&
                                                        item.InputUnitPrice.HasValue);
                                                    }
                                                    else
                                                    {
                                                        itemDB = sqliteDbContext.Items.FirstOrDefault(item => item.Id == itemInvoiceDB.ItemCode);
                                                    }

                                                    if (itemDB != null &&
                                                    itemInvoiceDB.Quantity.HasValue)
                                                    {
                                                        var groupDB = sqliteDbContext.ItemGroups.Find(itemDB.IdItemGroup);

                                                        if (groupDB != null &&
                                                        groupDB.IdSupergroup == supergroupDB.Id)
                                                        {
                                                            Item item = new Item(itemDB);
                                                            ItemInvoice itemInvoice = new ItemInvoice(item, itemInvoiceDB);

                                                            bool isRefund = invoice.TransactionType == Enums.Sale.TransactionTypeEnumeration.Refundacija ? true : false;

                                                            switch (itemDB.Label)
                                                            {
                                                                case "Ђ":
                                                                    if (!itemInvoice.IsSirovina)
                                                                    {
                                                                        SetItemsPDV(allItems20PDV, itemInvoice, nivelacija, isRefund, itemDB, isKuhinja, groupDB);
                                                                    }
                                                                    else
                                                                    {
                                                                        if (enableSirovine)
                                                                        {
                                                                            SetItemsPDV(allItemsSirovina20PDV, itemInvoice, nivelacija, isRefund, itemDB, isKuhinja, groupDB);
                                                                        }
                                                                    }
                                                                    break;
                                                                case "6":
                                                                    if (!itemInvoice.IsSirovina)
                                                                    {
                                                                        SetItemsPDV(allItems20PDV, itemInvoice, nivelacija, isRefund, itemDB, isKuhinja, groupDB);
                                                                    }
                                                                    else
                                                                    {
                                                                        if (enableSirovine)
                                                                        {
                                                                            SetItemsPDV(allItemsSirovina20PDV, itemInvoice, nivelacija, isRefund, itemDB, isKuhinja, groupDB);
                                                                        }
                                                                    }
                                                                    break;
                                                                case "Е":
                                                                    if (!itemInvoice.IsSirovina)
                                                                    {
                                                                        SetItemsPDV(allItems10PDV, itemInvoice, nivelacija, isRefund, itemDB, isKuhinja, groupDB);
                                                                    }
                                                                    else
                                                                    {
                                                                        if (enableSirovine)
                                                                        {
                                                                            SetItemsPDV(allItemsSirovina10PDV, itemInvoice, nivelacija, isRefund, itemDB, isKuhinja, groupDB);
                                                                        }
                                                                    }
                                                                    break;
                                                                case "7":
                                                                    if (!itemInvoice.IsSirovina)
                                                                    {
                                                                        SetItemsPDV(allItems10PDV, itemInvoice, nivelacija, isRefund, itemDB, isKuhinja, groupDB);
                                                                    }
                                                                    else
                                                                    {
                                                                        if (enableSirovine)
                                                                        {
                                                                            SetItemsPDV(allItemsSirovina10PDV, itemInvoice, nivelacija, isRefund, itemDB, isKuhinja, groupDB);
                                                                        }
                                                                    }
                                                                    break;
                                                                case "Г":
                                                                    if (!itemInvoice.IsSirovina)
                                                                    {
                                                                        SetItemsPDV(allItems0PDV, itemInvoice, nivelacija, isRefund, itemDB, isKuhinja, groupDB);
                                                                    }
                                                                    else
                                                                    {
                                                                        if (enableSirovine)
                                                                        {
                                                                            SetItemsPDV(allItemsSirovina0PDV, itemInvoice, nivelacija, isRefund, itemDB, isKuhinja, groupDB);
                                                                        }
                                                                    }
                                                                    break;
                                                                case "4":
                                                                    if (!itemInvoice.IsSirovina)
                                                                    {
                                                                        SetItemsPDV(allItems0PDV, itemInvoice, nivelacija, isRefund, itemDB, isKuhinja, groupDB);
                                                                    }
                                                                    else
                                                                    {
                                                                        if (enableSirovine)
                                                                        {
                                                                            SetItemsPDV(allItemsSirovina0PDV, itemInvoice, nivelacija, isRefund, itemDB, isKuhinja, groupDB);
                                                                        }
                                                                    }
                                                                    break;
                                                                case "А":
                                                                    if (!itemInvoice.IsSirovina)
                                                                    {
                                                                        SetItemsPDV(allItemsNoPDV, itemInvoice, nivelacija, isRefund, itemDB, isKuhinja, groupDB);
                                                                    }
                                                                    else
                                                                    {
                                                                        if (enableSirovine)
                                                                        {
                                                                            SetItemsPDV(allItemsSirovinaNoPDV, itemInvoice, nivelacija, isRefund, itemDB, isKuhinja, groupDB);
                                                                        }
                                                                    }
                                                                    break;
                                                                case "1":
                                                                    if (!itemInvoice.IsSirovina)
                                                                    {
                                                                        SetItemsPDV(allItemsNoPDV, itemInvoice, nivelacija, isRefund, itemDB, isKuhinja, groupDB);
                                                                    }
                                                                    else
                                                                    {
                                                                        if (enableSirovine)
                                                                        {
                                                                            SetItemsPDV(allItemsSirovinaNoPDV, itemInvoice, nivelacija, isRefund, itemDB, isKuhinja, groupDB);
                                                                        }
                                                                    }
                                                                    break;
                                                                case "Ж":
                                                                    if (!itemInvoice.IsSirovina)
                                                                    {
                                                                        SetItemsPDV(allItems20PDV, itemInvoice, nivelacija, isRefund, itemDB, isKuhinja, groupDB);
                                                                    }
                                                                    else
                                                                    {
                                                                        if (enableSirovine)
                                                                        {
                                                                            SetItemsPDV(allItemsSirovina20PDV, itemInvoice, nivelacija, isRefund, itemDB, isKuhinja, groupDB);
                                                                        }
                                                                    }
                                                                    break;
                                                                case "8":
                                                                    if (!itemInvoice.IsSirovina)
                                                                    {
                                                                        SetItemsPDV(allItems20PDV, itemInvoice, nivelacija, isRefund, itemDB, isKuhinja, groupDB);
                                                                    }
                                                                    else
                                                                    {
                                                                        if (enableSirovine)
                                                                        {
                                                                            SetItemsPDV(allItemsSirovina20PDV, itemInvoice, nivelacija, isRefund, itemDB, isKuhinja, groupDB);
                                                                        }
                                                                    }
                                                                    break;
                                                                case "A":
                                                                    if (!itemInvoice.IsSirovina)
                                                                    {
                                                                        SetItemsPDV(allItems10PDV, itemInvoice, nivelacija, isRefund, itemDB, isKuhinja, groupDB);
                                                                    }
                                                                    else
                                                                    {
                                                                        if (enableSirovine)
                                                                        {
                                                                            SetItemsPDV(allItemsSirovina10PDV, itemInvoice, nivelacija, isRefund, itemDB, isKuhinja, groupDB);
                                                                        }
                                                                    }
                                                                    break;
                                                                case "31":
                                                                    if (!itemInvoice.IsSirovina)
                                                                    {
                                                                        SetItemsPDV(allItems10PDV, itemInvoice, nivelacija, isRefund, itemDB, isKuhinja, groupDB);
                                                                    }
                                                                    else
                                                                    {
                                                                        if (enableSirovine)
                                                                        {
                                                                            SetItemsPDV(allItemsSirovina10PDV, itemInvoice, nivelacija, isRefund, itemDB, isKuhinja, groupDB);
                                                                        }
                                                                    }
                                                                    break;
                                                                case "N":
                                                                    if (!itemInvoice.IsSirovina)
                                                                    {
                                                                        SetItemsPDV(allItemsNoPDV, itemInvoice, nivelacija, isRefund, itemDB, isKuhinja, groupDB);
                                                                    }
                                                                    else
                                                                    {
                                                                        if (enableSirovine)
                                                                        {
                                                                            SetItemsPDV(allItemsSirovinaNoPDV, itemInvoice, nivelacija, isRefund, itemDB, isKuhinja, groupDB);
                                                                        }
                                                                    }
                                                                    break;
                                                                case "47":
                                                                    if (!itemInvoice.IsSirovina)
                                                                    {
                                                                        SetItemsPDV(allItemsNoPDV, itemInvoice, nivelacija, isRefund, itemDB, isKuhinja, groupDB);
                                                                    }
                                                                    else
                                                                    {
                                                                        if (enableSirovine)
                                                                        {
                                                                            SetItemsPDV(allItemsSirovinaNoPDV, itemInvoice, nivelacija, isRefund, itemDB, isKuhinja, groupDB);
                                                                        }
                                                                    }
                                                                    break;
                                                                case "P":
                                                                    if (!itemInvoice.IsSirovina)
                                                                    {
                                                                        SetItemsPDV(allItems0PDV, itemInvoice, nivelacija, isRefund, itemDB, isKuhinja, groupDB);
                                                                    }
                                                                    else
                                                                    {
                                                                        if (enableSirovine)
                                                                        {
                                                                            SetItemsPDV(allItemsSirovina0PDV, itemInvoice, nivelacija, isRefund, itemDB, isKuhinja, groupDB);
                                                                        }
                                                                    }
                                                                    break;
                                                                case "49":
                                                                    if (!itemInvoice.IsSirovina)
                                                                    {
                                                                        SetItemsPDV(allItems0PDV, itemInvoice, nivelacija, isRefund, itemDB, isKuhinja, groupDB);
                                                                    }
                                                                    else
                                                                    {
                                                                        if (enableSirovine)
                                                                        {
                                                                            SetItemsPDV(allItemsSirovina0PDV, itemInvoice, nivelacija, isRefund, itemDB, isKuhinja, groupDB);
                                                                        }
                                                                    }
                                                                    break;
                                                            }
                                                        }
                                                    }
                                                }
                                            });
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    int aaa = 2;
                                }
                            });

                            PregledPazaraViewModel pregledPazaraViewModel = (PregledPazaraViewModel)_currentViewModel;

                            if (isKuhinja)
                            {
                                PrinterManager.Instance.PrintKuhinja(pregledPazaraViewModel.FromDate, pregledPazaraViewModel.ToDate,
                                    allItems20PDV,
                                    allItems10PDV,
                                    allItems0PDV,
                                    allItemsNoPDV,
                                    allItemsSirovina20PDV,
                                    allItemsSirovina10PDV,
                                    allItemsSirovina0PDV,
                                    allItemsSirovinaNoPDV,
                                    enableSirovine);
                            }
                            else
                            {
                                PrinterManager.Instance.PrintSank(pregledPazaraViewModel.FromDate, pregledPazaraViewModel.ToDate,
                                    allItems20PDV,
                                    allItems10PDV,
                                    allItems0PDV,
                                    allItemsNoPDV,
                                    allItemsSirovina20PDV,
                                    allItemsSirovina10PDV,
                                    allItemsSirovina0PDV,
                                    allItemsSirovinaNoPDV,
                                    enableSirovine);
                            }

                            pregledPazaraViewModel.SearchInvoicesCommand.Execute(null);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error("PrintDnevniPazarKuhinjaSankCommand - Greska prilikom stampe pazara -> ", ex);
                    }
                }
            }
        }
        private void SetItemsPDV(
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsPDV,
            ItemInvoice itemInvoice,
            decimal nivelacija,
            bool isRefund,
            ItemDB itemDB,
            bool isKuhinja,
            ItemGroupDB itemGroupDB)
        {
            if(itemDB.Id == "000002")
            {
                int a = 2;
            }

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
                                isKuhinja)
                            {
                                it.TotalAmount += itemInvoice.TotalAmout; //Decimal.Round(itemInvoice.InputUnitPrice.Value * itemInvoice.Quantity, 2);
                                it.MPC_Average = Decimal.Round(it.TotalAmount / it.Quantity, 2);
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
                                isKuhinja)
                            {
                                it.TotalAmount -= itemInvoice.TotalAmout; //Decimal.Round(itemInvoice.InputUnitPrice.Value * itemInvoice.Quantity, 2);
                                it.MPC_Average = Decimal.Round(it.TotalAmount / it.Quantity, 2);
                            }
                            else
                            {
                                it.TotalAmount -= itemInvoice.TotalAmout;
                                it.MPC_Average = it.TotalAmount > 0 && it.Quantity > 0 ? Decimal.Round(it.TotalAmount / it.Quantity, 2) : 0;
                                it.MPC_Original = it.MPC_Average;
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
                            isKuhinja)
                    {
                        reportPerItems.TotalAmount = isRefund ? (-1 * itemInvoice.TotalAmout) ://Decimal.Round(-1 * itemInvoice.InputUnitPrice.Value * itemInvoice.Quantity) :
                            itemInvoice.TotalAmout; //Decimal.Round(itemInvoice.InputUnitPrice.Value * itemInvoice.Quantity, 2);
                        reportPerItems.MPC_Average = itemInvoice.InputUnitPrice.Value;
                    }
                    else
                    {
                        reportPerItems.TotalAmount = isRefund ? -1 * itemInvoice.TotalAmout : itemInvoice.TotalAmout;
                        reportPerItems.MPC_Average = itemInvoice.Item.SellingUnitPrice;
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

                            it.TotalAmount += itemInvoice.TotalAmout;
                            it.MPC_Average = it.TotalAmount > 0 && it.Quantity > 0 ? Decimal.Round(it.TotalAmount / it.Quantity, 2) : 0;
                            //if (itemInvoice.InputUnitPrice != null &&
                            //    itemInvoice.InputUnitPrice.HasValue &&
                            //    isKuhinja)
                            //{
                            //    it.TotalAmount += itemInvoice.InputUnitPrice.Value * itemInvoice.Quantity;
                            //    it.MPC_Average = Decimal.Round(it.TotalAmount / it.Quantity, 2);
                            //}
                            //else
                            //{
                            //    it.TotalAmount += itemInvoice.TotalAmout;
                            //    it.MPC_Average = it.TotalAmount > 0 && it.Quantity > 0 ? Decimal.Round(it.TotalAmount / it.Quantity, 2) : 0;
                            //}

                            nivelacija -= it.Nivelacija;

                            decimal niv = -1 * Decimal.Round((it.Quantity * it.MPC_Original) - it.TotalAmount, 2);

                            it.Nivelacija = niv;
                            nivelacija += it.Nivelacija;
                        }
                        else
                        {
                            it.Quantity -= itemInvoice.Quantity;
                            it.TotalAmount -= itemInvoice.TotalAmout;

                            //if (itemInvoice.InputUnitPrice != null &&
                            //    itemInvoice.InputUnitPrice.HasValue &&
                            //    is1010)
                            //{
                            //    it.TotalAmount -= itemInvoice.InputUnitPrice.Value * itemInvoice.Quantity;
                            //    it.MPC_Average = Decimal.Round(it.TotalAmount / it.Quantity, 2);
                            //}
                            //else
                            //{
                            //    it.TotalAmount -= itemInvoice.TotalAmout;
                            //}
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
                            Nivelacija = isRefund ? 0 : -1 * Decimal.Round((itemInvoice.Item.OriginalUnitPrice * itemInvoice.Quantity) - itemInvoice.TotalAmout, 2),
                        };

                        reportPerItems.TotalAmount = isRefund ? -1 * itemInvoice.TotalAmout : itemInvoice.TotalAmout;
                        reportPerItems.MPC_Average = itemInvoice.Item.SellingUnitPrice;
                        //if (itemInvoice.InputUnitPrice != null &&
                        //        itemInvoice.InputUnitPrice.HasValue &&
                        //        is1010)
                        //{
                        //    reportPerItems.TotalAmount = isRefund ? -1 * itemInvoice.InputUnitPrice.Value * itemInvoice.Quantity :
                        //        itemInvoice.InputUnitPrice.Value * itemInvoice.Quantity;
                        //    reportPerItems.MPC_Average = itemInvoice.InputUnitPrice.Value;
                        //}
                        //else
                        //{
                        //    reportPerItems.TotalAmount = isRefund ? -1 * itemInvoice.TotalAmout : itemInvoice.TotalAmout;
                        //    reportPerItems.MPC_Average = itemInvoice.Item.SellingUnitPrice;
                        //}

                        //decimal niv = -1 * Decimal.Round((itemInvoice.Item.OriginalUnitPrice * itemInvoice.Quantity) - itemInvoice.TotalAmout, 2);

                        //reportPerItems.Nivelacija = niv > -1 && niv < 1 ? 0 : niv;
                        allItemsPDV[itemGroupDB.Name][itemInvoice.Item.Id].Add(reportPerItems);

                        nivelacija += reportPerItems.Nivelacija;
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
                        Nivelacija = isRefund ? 0 : -1 * Decimal.Round((itemInvoice.Item.OriginalUnitPrice * itemInvoice.Quantity) - itemInvoice.TotalAmout, 2),
                    };
                    reportPerItems.TotalAmount = isRefund ? -1 * itemInvoice.TotalAmout : itemInvoice.TotalAmout;
                    reportPerItems.MPC_Average = itemInvoice.Item.SellingUnitPrice;

                    //if (itemInvoice.InputUnitPrice != null &&
                    //        itemInvoice.InputUnitPrice.HasValue &&
                    //        is1010)
                    //{
                    //    reportPerItems.TotalAmount = isRefund ? -1 * itemInvoice.InputUnitPrice.Value * itemInvoice.Quantity :
                    //        itemInvoice.InputUnitPrice.Value * itemInvoice.Quantity;
                    //    reportPerItems.MPC_Average = itemInvoice.InputUnitPrice.Value;
                    //}
                    //else
                    //{
                    //    reportPerItems.TotalAmount = isRefund ? -1 * itemInvoice.TotalAmout : itemInvoice.TotalAmout;
                    //    reportPerItems.MPC_Average = itemInvoice.Item.SellingUnitPrice;
                    //}

                    //decimal niv = -1 * Decimal.Round((itemInvoice.Item.OriginalUnitPrice * itemInvoice.Quantity) - itemInvoice.TotalAmout, 2);

                    //reportPerItems.Nivelacija = niv > -1 && niv < 1 ? 0 : niv;

                    allItemsPDV[itemGroupDB.Name].Add(itemInvoice.Item.Id, new List<ReportPerItems>() { reportPerItems });

                    nivelacija += reportPerItems.Nivelacija;
                }
            }
        }
    }
}