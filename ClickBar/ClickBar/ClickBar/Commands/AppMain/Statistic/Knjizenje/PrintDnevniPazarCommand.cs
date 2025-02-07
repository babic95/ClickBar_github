using ClickBar.Enums.AppMain.Statistic;
using ClickBar.Models.AppMain.Statistic;
using ClickBar.Models.Sale;
using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar_Database.Models;
using ClickBar_Database;
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

namespace ClickBar.Commands.AppMain.Statistic.Knjizenje
{
    public class PrintDnevniPazarCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ViewModelBase _currentViewModel;

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
                invoices = new ObservableCollection<Invoice>(knjizenjeViewModel.Invoices.OrderBy(i => i.SdcDateTime));
            }
            else if(_currentViewModel is PregledPazaraViewModel)
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
                                    var itemsDB = sqliteDbContext.ItemInvoices.Where(inv => inv.InvoiceId == invoiceDB.Id);

                                    if (itemsDB != null &&
                                        itemsDB.Any())
                                    {
                                        itemsDB.ToList().ForEach(itemInvoiceDB =>
                                        {
                                            if (!string.IsNullOrEmpty(itemInvoiceDB.ItemCode))
                                            {
                                                ItemDB? itemDB = null;
                                                if (is1010)
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

                                                    if (groupDB != null)
                                                    {
                                                        Item item = new Item(itemDB);
                                                        ItemInvoice itemInvoice = new ItemInvoice(item, itemInvoiceDB);

                                                        bool isRefund = invoice.TransactionType == Enums.Sale.TransactionTypeEnumeration.Refundacija ? true : false;

                                                        switch (itemDB.Label)
                                                        {
                                                            case "Ђ":
                                                                if (!itemInvoice.IsSirovina)
                                                                {
                                                                    SetItemsPDV(allItems20PDV, itemInvoice, nivelacija, isRefund, itemDB, is1010, groupDB);
                                                                }
                                                                else
                                                                {
                                                                    SetItemsPDV(allItemsSirovina20PDV, itemInvoice, nivelacija, isRefund, itemDB, is1010, groupDB);
                                                                }
                                                                break;
                                                            case "6":
                                                                if (!itemInvoice.IsSirovina)
                                                                {
                                                                    SetItemsPDV(allItems20PDV, itemInvoice, nivelacija, isRefund, itemDB, is1010, groupDB);
                                                                }
                                                                else
                                                                {
                                                                    SetItemsPDV(allItemsSirovina20PDV, itemInvoice, nivelacija, isRefund, itemDB, is1010, groupDB);
                                                                }
                                                                break;
                                                            case "Е":
                                                                if (!itemInvoice.IsSirovina)
                                                                {
                                                                    SetItemsPDV(allItems10PDV, itemInvoice, nivelacija, isRefund, itemDB, is1010, groupDB);
                                                                }
                                                                else
                                                                {
                                                                    SetItemsPDV(allItemsSirovina10PDV, itemInvoice, nivelacija, isRefund, itemDB, is1010, groupDB);
                                                                }
                                                                break;
                                                            case "7":
                                                                if (!itemInvoice.IsSirovina)
                                                                {
                                                                    SetItemsPDV(allItems10PDV, itemInvoice, nivelacija, isRefund, itemDB, is1010, groupDB);
                                                                }
                                                                else
                                                                {
                                                                    SetItemsPDV(allItemsSirovina10PDV, itemInvoice, nivelacija, isRefund, itemDB, is1010, groupDB);
                                                                }
                                                                break;
                                                            case "Г":
                                                                if (!itemInvoice.IsSirovina)
                                                                {
                                                                    SetItemsPDV(allItems0PDV, itemInvoice, nivelacija, isRefund, itemDB, is1010, groupDB);
                                                                }
                                                                else
                                                                {
                                                                    SetItemsPDV(allItemsSirovina0PDV, itemInvoice, nivelacija, isRefund, itemDB, is1010, groupDB);
                                                                }
                                                                break;
                                                            case "4":
                                                                if (!itemInvoice.IsSirovina)
                                                                {
                                                                    SetItemsPDV(allItems0PDV, itemInvoice, nivelacija, isRefund, itemDB, is1010, groupDB);
                                                                }
                                                                else
                                                                {
                                                                    SetItemsPDV(allItemsSirovina0PDV, itemInvoice, nivelacija, isRefund, itemDB, is1010, groupDB);
                                                                }
                                                                break;
                                                            case "А":
                                                                if (!itemInvoice.IsSirovina)
                                                                {
                                                                    SetItemsPDV(allItemsNoPDV, itemInvoice, nivelacija, isRefund, itemDB, is1010, groupDB);
                                                                }
                                                                else
                                                                {
                                                                    SetItemsPDV(allItemsSirovinaNoPDV, itemInvoice, nivelacija, isRefund, itemDB, is1010, groupDB);
                                                                }
                                                                break;
                                                            case "1":
                                                                if (!itemInvoice.IsSirovina)
                                                                {
                                                                    SetItemsPDV(allItemsNoPDV, itemInvoice, nivelacija, isRefund, itemDB, is1010, groupDB);
                                                                }
                                                                else
                                                                {
                                                                    SetItemsPDV(allItemsSirovinaNoPDV, itemInvoice, nivelacija, isRefund, itemDB, is1010, groupDB);
                                                                }
                                                                break;
                                                            case "Ж":
                                                                if (!itemInvoice.IsSirovina)
                                                                {
                                                                    SetItemsPDV(allItems20PDV, itemInvoice, nivelacija, isRefund, itemDB, is1010, groupDB);
                                                                }
                                                                else
                                                                {
                                                                    SetItemsPDV(allItemsSirovina20PDV, itemInvoice, nivelacija, isRefund, itemDB, is1010, groupDB);
                                                                }
                                                                break;
                                                            case "8":
                                                                if (!itemInvoice.IsSirovina)
                                                                {
                                                                    SetItemsPDV(allItems20PDV, itemInvoice, nivelacija, isRefund, itemDB, is1010, groupDB);
                                                                }
                                                                else
                                                                {
                                                                    SetItemsPDV(allItemsSirovina20PDV, itemInvoice, nivelacija, isRefund, itemDB, is1010, groupDB);
                                                                }
                                                                break;
                                                            case "A":
                                                                if (!itemInvoice.IsSirovina)
                                                                {
                                                                    SetItemsPDV(allItems10PDV, itemInvoice, nivelacija, isRefund, itemDB, is1010, groupDB);
                                                                }
                                                                else
                                                                {
                                                                    SetItemsPDV(allItemsSirovina10PDV, itemInvoice, nivelacija, isRefund, itemDB, is1010, groupDB);
                                                                }
                                                                break;
                                                            case "31":
                                                                if (!itemInvoice.IsSirovina)
                                                                {
                                                                    SetItemsPDV(allItems10PDV, itemInvoice, nivelacija, isRefund, itemDB, is1010, groupDB);
                                                                }
                                                                else
                                                                {
                                                                    SetItemsPDV(allItemsSirovina10PDV, itemInvoice, nivelacija, isRefund, itemDB, is1010, groupDB);
                                                                }
                                                                break;
                                                            case "N":
                                                                if (!itemInvoice.IsSirovina)
                                                                {
                                                                    SetItemsPDV(allItemsNoPDV, itemInvoice, nivelacija, isRefund, itemDB, is1010, groupDB);
                                                                }
                                                                else
                                                                {
                                                                    SetItemsPDV(allItemsSirovinaNoPDV, itemInvoice, nivelacija, isRefund, itemDB, is1010, groupDB);
                                                                }
                                                                break;
                                                            case "47":
                                                                if (!itemInvoice.IsSirovina)
                                                                {
                                                                    SetItemsPDV(allItemsNoPDV, itemInvoice, nivelacija, isRefund, itemDB, is1010, groupDB);
                                                                }
                                                                else
                                                                {
                                                                    SetItemsPDV(allItemsSirovinaNoPDV, itemInvoice, nivelacija, isRefund, itemDB, is1010, groupDB);
                                                                }
                                                                break;
                                                            case "P":
                                                                if (!itemInvoice.IsSirovina)
                                                                {
                                                                    SetItemsPDV(allItems0PDV, itemInvoice, nivelacija, isRefund, itemDB, is1010, groupDB);
                                                                }
                                                                else
                                                                {
                                                                    SetItemsPDV(allItemsSirovina0PDV, itemInvoice, nivelacija, isRefund, itemDB, is1010, groupDB);
                                                                }
                                                                break;
                                                            case "49":
                                                                if (!itemInvoice.IsSirovina)
                                                                {
                                                                    SetItemsPDV(allItems0PDV, itemInvoice, nivelacija, isRefund, itemDB, is1010, groupDB);
                                                                }
                                                                else
                                                                {
                                                                    SetItemsPDV(allItemsSirovina0PDV, itemInvoice, nivelacija, isRefund, itemDB, is1010, groupDB);
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

                        if (_currentViewModel is KnjizenjeViewModel)
                        {
                            KnjizenjeViewModel knjizenjeViewModel = (KnjizenjeViewModel)_currentViewModel;

                            if (is1010)
                            {
                                PrinterManager.Instance.PrintIzlaz1010(knjizenjeViewModel.CurrentDate, null,
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
                                PrinterManager.Instance.PrintDnevniPazar(knjizenjeViewModel.CurrentDate, null,
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
                                PrinterManager.Instance.PrintIzlaz1010(pregledPazaraViewModel.FromDate, pregledPazaraViewModel.ToDate,
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
                                PrinterManager.Instance.PrintDnevniPazar(pregledPazaraViewModel.FromDate, pregledPazaraViewModel.ToDate,
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
                                it.MPC_Average = Decimal.Round(it.TotalAmount / it.Quantity, 2);
                            }
                            else
                            {
                                it.MPC_Average = it.TotalAmount > 0 && it.Quantity > 0 ? Decimal.Round(it.TotalAmount / it.Quantity, 2) : 0;
                                it.MPC_Original = it.MPC_Average;
                                it.TotalAmount += itemInvoice.TotalAmout;
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
                                it.MPC_Average = Decimal.Round(it.TotalAmount / it.Quantity, 2);
                            }
                            else
                            {
                                it.MPC_Average = it.TotalAmount > 0 && it.Quantity > 0 ? Decimal.Round(it.TotalAmount / it.Quantity, 2) : 0;
                                it.MPC_Original = it.MPC_Average;
                                it.TotalAmount -= itemInvoice.TotalAmout;
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
                                it.MPC_Average = Decimal.Round(it.TotalAmount / it.Quantity, 2);
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
                                it.MPC_Average = Decimal.Round(it.TotalAmount / it.Quantity, 2);
                            }
                            else
                            {
                                it.TotalAmount -= itemInvoice.TotalAmout;
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
                            Nivelacija = isRefund ? 0 : - 1 * Decimal.Round((itemInvoice.Item.OriginalUnitPrice * itemInvoice.Quantity) - itemInvoice.TotalAmout, 2),
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
                }
            }
        }
    }
}