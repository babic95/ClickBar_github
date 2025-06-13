using ClickBar.ViewModels;
using ClickBar.Views.Kuhinja;
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
    public class PrintPoArtikluCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private KuhinjaViewModel _currentViewModel;

        public PrintPoArtikluCommand(KuhinjaViewModel currentViewModel)
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
                KuhinjaReport kuhinjaReport = new KuhinjaReport()
                {
                    Name = _currentViewModel.FromDate.Date == _currentViewModel.ToDate.Date ?
                    $"Po artiklima za {_currentViewModel.FromDate.ToString("dd.MM.yyyy")}" :
                     $"Po artiklima za {_currentViewModel.FromDate.ToString("dd.MM.yyyy")} - {_currentViewModel.ToDate.ToString("dd.MM.yyyy")}",
                    Total = 0,
                    Items = new List<KuhinjaReportItem>()
                };

                using (var dbContext = _currentViewModel.DbContext.CreateDbContext())
                {
                    var allOrdersToday = dbContext.OrdersToday.Include(o => o.OrderTodayItems)
                        .Where(o => o.OrderDateTime.Date >= _currentViewModel.FromDate.Date &&
                        o.OrderDateTime.Date <= _currentViewModel.ToDate.Date &&
                        !string.IsNullOrEmpty(o.Name) &&
                        (o.Name.ToLower().Contains("k") ||
                        o.Name.ToLower().Contains("h")))
                        .ToList();

                    if (allOrdersToday.Any())
                    {
                        foreach(var orderToday in allOrdersToday)
                        {
                            foreach (var orderTodayItem in orderToday.OrderTodayItems)
                            {
                                var item = kuhinjaReport.Items.FirstOrDefault(i => i.Id == orderTodayItem.ItemId);
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
                                        kuhinjaReport.Items.Add(item);
                                    }
                                }
                                else
                                {
                                    item.Quantity += orderTodayItem.Quantity - orderTodayItem.StornoQuantity;
                                    item.Total += Decimal.Round((orderTodayItem.TotalPrice / orderTodayItem.Quantity) * (orderTodayItem.Quantity - orderTodayItem.StornoQuantity), 2);
                                }
                                kuhinjaReport.Total += Decimal.Round((orderTodayItem.TotalPrice / orderTodayItem.Quantity) * (orderTodayItem.Quantity - orderTodayItem.StornoQuantity), 2);
                            }
                        }
                    }
                }
                PrinterManager.Instance.PrintKuhinjaReport(kuhinjaReport);
            }
            catch (Exception ex)
            {
                Log.Error("PrintPoArtikluCommand -> desila se greska prilikom stampe arikala u izvestaju: ", ex);
                MessageBox.Show("Desila se greška.\nObratite se serviseru.", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
