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
    public class PrintGazdeCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private KuhinjaViewModel _currentViewModel;

        public PrintGazdeCommand(KuhinjaViewModel currentViewModel)
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
                    $"Gazde za {_currentViewModel.FromDate.ToString("dd.MM.yyyy")}" :
                     $"Gazde za {_currentViewModel.FromDate.ToString("dd.MM.yyyy")} - {_currentViewModel.ToDate.ToString("dd.MM.yyyy")}",
                    Total = 0,
                    Items = new List<KuhinjaReportItem>()
                };

                using (var dbContext = _currentViewModel.DbContext.CreateDbContext())
                {
                    var allOrdersToday = dbContext.OrdersToday.Include(o => o.OrderTodayItems)
                        .Join(dbContext.PaymentPlaces,
                        o => o.TableId,
                        p => p.Id,
                        (o, p) => new { OrderToday = o, PaymentPlace = p })
                        .Where(o => !string.IsNullOrEmpty(o.PaymentPlace.Name) &&
                        (o.PaymentPlace.Name.ToLower().Contains("gazda") || o.PaymentPlace.Name.ToLower().Contains("gazde")) &&
                        o.OrderToday.OrderDateTime.Date >= _currentViewModel.FromDate.Date &&
                        o.OrderToday.OrderDateTime.Date <= _currentViewModel.ToDate.Date &&
                        !string.IsNullOrEmpty(o.OrderToday.Name) &&
                        (o.OrderToday.Name.ToLower().Contains("k") ||
                        o.OrderToday.Name.ToLower().Contains("h")))
                        .Select(o => o.OrderToday).ToList();

                    if (allOrdersToday.Any())
                    {
                        foreach (var orderToday in allOrdersToday)
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
                Log.Error("PrintGazdeCommand -> desila se greska prilikom stampe izvestaja za gazde: ", ex);
                MessageBox.Show("Desila se greška.\nObratite se serviseru.", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}