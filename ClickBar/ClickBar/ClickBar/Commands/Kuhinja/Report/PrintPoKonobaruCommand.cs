using ClickBar.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using ClickBar_Logging;
using ClickBar_Report.Models.Kuhinja;
using Microsoft.EntityFrameworkCore;
using ClickBar_Printer;

namespace ClickBar.Commands.Kuhinja.Report
{
    public class PrintPoKonobaruCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private KuhinjaViewModel _currentViewModel;

        public PrintPoKonobaruCommand(KuhinjaViewModel currentViewModel)
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
                List<KuhinjaReport> reportKonobari = new List<KuhinjaReport>();
                string name = _currentViewModel.FromDate.Date == _currentViewModel.ToDate.Date ?
                    $"Po konobarima za {_currentViewModel.FromDate.ToString("dd.MM.yyyy")}" :
                     $"Po konobarima za {_currentViewModel.FromDate.ToString("dd.MM.yyyy")} - {_currentViewModel.ToDate.ToString("dd.MM.yyyy")}";

                using (var dbContext = _currentViewModel.DbContext.CreateDbContext())
                {
                    var allKonobari = dbContext.OrdersToday.Include(o => o.OrderTodayItems).Include(o => o.Cashier)
                        .Where(o => o.OrderDateTime.Date >= _currentViewModel.FromDate.Date &&
                        o.OrderDateTime.Date <= _currentViewModel.ToDate.Date &&
                        !string.IsNullOrEmpty(o.Name) &&
                        (o.Name.ToLower().Contains("k") ||
                        o.Name.ToLower().Contains("h")))
                        .GroupBy(o => o.CashierId).ToList();

                    if (allKonobari.Any())
                    {
                        foreach (var konobar in allKonobari)
                        {
                            foreach (var orderToday in konobar)
                            {
                                var konobarReport = reportKonobari.FirstOrDefault(k => k.Name == $"{orderToday.Cashier.Name}");

                                if(konobarReport == null)
                                {
                                    konobarReport = new KuhinjaReport()
                                    {
                                        Name = $"{orderToday.Cashier.Name}",
                                        Total = 0,
                                        Items = new List<KuhinjaReportItem>()
                                    };
                                    reportKonobari.Add(konobarReport);
                                }

                                foreach (var orderTodayItem in orderToday.OrderTodayItems)
                                {
                                    var item = konobarReport.Items.FirstOrDefault(i => i.Id == orderTodayItem.ItemId);
                                    if (item == null)
                                    {
                                        var itemDB = dbContext.Items.FirstOrDefault(i => i.Id == orderTodayItem.ItemId);

                                        if (itemDB != null)
                                        {
                                            item = new KuhinjaReportItem()
                                            {
                                                Id = orderTodayItem.ItemId,
                                                Name = itemDB.Name,
                                                Quantity = orderTodayItem.Quantity - orderTodayItem.StornoQuantity,
                                                Price = orderTodayItem.TotalPrice == 0 ? 0 : Decimal.Round(orderTodayItem.TotalPrice / orderTodayItem.Quantity, 2),
                                                Total = Decimal.Round((orderTodayItem.TotalPrice / orderTodayItem.Quantity) * (orderTodayItem.Quantity - orderTodayItem.StornoQuantity), 2)
                                            };
                                            konobarReport.Items.Add(item);
                                        }
                                    }
                                    else
                                    {
                                        item.Quantity += orderTodayItem.Quantity - orderTodayItem.StornoQuantity;
                                        item.Total += Decimal.Round((orderTodayItem.TotalPrice / orderTodayItem.Quantity) * (orderTodayItem.Quantity - orderTodayItem.StornoQuantity), 2);
                                    }
                                    konobarReport.Total += Decimal.Round((orderTodayItem.TotalPrice / orderTodayItem.Quantity) * (orderTodayItem.Quantity - orderTodayItem.StornoQuantity), 2);
                                }
                            }
                        }
                    }
                }
                PrinterManager.Instance.PrintKuhinjaKonobariReport(reportKonobari, name);
            }
            catch (Exception ex)
            {
                Log.Error("PrintPoKonobaruCommand -> desila se greska prilikom stampe ukupno po konobarima: ", ex);
                MessageBox.Show("Desila se greška.\nObratite se serviseru.", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}