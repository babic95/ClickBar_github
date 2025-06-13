using ClickBar.Models.Sale;
using ClickBar.ViewModels.Sale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using ClickBar_DatabaseSQLManager;
using Microsoft.EntityFrameworkCore;
using ClickBar_Logging;
using ClickBar_Common.Models.Order.Drlja;
using ClickBar_Settings;
using Newtonsoft.Json;
using System.Net.Http;
using ClickBar.Enums;
using ClickBar_DatabaseSQLManager.Models;
using ClickBar_Common.Enums;
using ClickBar.Enums.Kuhinja;
using DocumentFormat.OpenXml.InkML;

namespace ClickBar.Commands.Sale.Pay.SplitOrder
{
    public class StornoKuhinjaCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private SplitOrderViewModel _viewModel;
        private bool _isExecuted;

        public StornoKuhinjaCommand(SplitOrderViewModel viewModel)
        {
            _viewModel = viewModel;
            _isExecuted = false;
        }

        public bool CanExecute(object? parameter)
        {
            return !_isExecuted;
        }

        public async void Execute(object parameter)
        {
            if (_isExecuted)
            {
                return;
            }

            try
            {
                if (_viewModel.ItemsInvoiceForPay != null && _viewModel.ItemsInvoiceForPay.Any())
                {
                    if (_viewModel.PaySaleViewModel.SaleViewModel.TableId != 0)
                    {
                        using (var context = _viewModel.DbContextFactory.CreateDbContext())
                        using (var transaction = await context.Database.BeginTransactionAsync())
                        {
                            try
                            {
                                var unprocessedOrderDB = await context.UnprocessedOrders
                                    .FirstOrDefaultAsync(x => x.PaymentPlaceId == _viewModel.PaySaleViewModel.SaleViewModel.TableId);

                                if (unprocessedOrderDB != null)
                                {
                                    int orderStornoCounter = 1;
                                    int orderTotalCounter = 1;

                                    var ordersTodayBrisanjeDB = await context.OrdersToday
                                        .Where(o => o.OrderDateTime.Date == DateTime.Now.Date && !string.IsNullOrEmpty(o.Name) && o.Name.ToLower().Contains("storno"))
                                        .ToListAsync();

                                    var ordersTodayDB = await context.OrdersToday
                                        .Where(o => o.OrderDateTime.Date == DateTime.Now.Date)
                                        .ToListAsync();

                                    if (ordersTodayBrisanjeDB.Any())
                                    {
                                        orderStornoCounter = ordersTodayBrisanjeDB.Max(o => o.CounterType) + 1;
                                    }

                                    if (ordersTodayDB.Any())
                                    {
                                        orderTotalCounter = ordersTodayDB.Max(o => o.Counter) + 1;
                                    }

                                    OrderTodayDB orderStornoTodayDB = new OrderTodayDB()
                                    {
                                        CashierId = unprocessedOrderDB.CashierId,
                                        Id = Guid.NewGuid().ToString(),
                                        Name = $"STORNO{orderStornoCounter}__{orderTotalCounter}",
                                        OrderDateTime = DateTime.Now,
                                        TableId = unprocessedOrderDB.PaymentPlaceId,
                                        TotalPrice = 0,
                                        Counter = orderTotalCounter,
                                        CounterType = orderStornoCounter,
                                        OrderTodayItems = new List<OrderTodayItemDB>(),
                                    };

                                    //unprocessedOrderDB.TotalAmount -= _viewModel.TotalAmountForPay;
                                    //context.UnprocessedOrders.Update(unprocessedOrderDB);

                                    foreach (var item in _viewModel.ItemsInvoiceForPay)
                                    {
                                        OrderTodayItemDB orderTodayItemDB = new OrderTodayItemDB()
                                        {
                                            Id = Guid.NewGuid().ToString(),
                                            ItemId = item.Item.Id,
                                            OrderTodayId = orderStornoTodayDB.Id,
                                            Quantity = -1 * item.Quantity,
                                            TotalPrice = item.TotalAmout,
                                            StornoQuantity = 0,
                                            NaplacenoQuantity = 0,
                                        };
                                        orderStornoTodayDB.OrderTodayItems.Add(orderTodayItemDB);

                                        var oldOrderItems = await context.OrderTodayItems
                                            .Include(i => i.OrderToday)
                                            .Where(x => x.ItemId == item.Item.Id &&
                                                        x.OrderToday.UnprocessedOrderId == unprocessedOrderDB.Id &&
                                                        x.OrderToday.Faza != (int)FazaKuhinjeEnumeration.Naplacena &&
                                                        x.OrderToday.Faza != (int)FazaKuhinjeEnumeration.Obrisana)
                                            .ToListAsync();

                                        if (oldOrderItems.Any())
                                        {
                                            decimal stornoQuantity = item.Quantity;
                                            foreach (var oldItem in oldOrderItems)
                                            {
                                                if (stornoQuantity == 0)
                                                {
                                                    break;
                                                }

                                                if (oldItem.Quantity - oldItem.StornoQuantity - oldItem.NaplacenoQuantity >= stornoQuantity)
                                                {
                                                    oldItem.StornoQuantity += stornoQuantity;
                                                    stornoQuantity = 0;
                                                }
                                                else
                                                {
                                                    stornoQuantity -= (oldItem.Quantity - oldItem.StornoQuantity - oldItem.NaplacenoQuantity);
                                                    oldItem.StornoQuantity = oldItem.Quantity;
                                                }
                                                context.OrderTodayItems.Update(oldItem);
                                            }

                                            await context.SaveChangesAsync();
                                        }
                                    }

                                    context.OrdersToday.Add(orderStornoTodayDB);
                                    await context.SaveChangesAsync();

                                    var oldU = context.OrdersToday.Include(a => a.OrderTodayItems).Where(o => o.UnprocessedOrderId == unprocessedOrderDB.Id).ToList();

                                    if (oldU.Any())
                                    {
                                        decimal sum = oldU.SelectMany(o => o.OrderTodayItems)
                                            .Sum(item => (item.TotalPrice / item.Quantity) * (item.Quantity - item.StornoQuantity - item.NaplacenoQuantity));

                                        if (sum == 0)
                                        {
                                            context.UnprocessedOrders.Remove(unprocessedOrderDB);
                                            context.SaveChanges();
                                        }
                                    }
                                    else
                                    {
                                        context.UnprocessedOrders.Remove(unprocessedOrderDB);
                                        context.SaveChanges();
                                    }

                                    await transaction.CommitAsync();

                                    MessageBox.Show("Uspešno!", "Uspešno", MessageBoxButton.OK, MessageBoxImage.Information);
                                    _viewModel.PaySaleViewModel.SplitOrderWindow.Close();

                                    if (_viewModel.PaySaleViewModel.SaleViewModel.PayWindow != null)
                                    {
                                        _viewModel.PaySaleViewModel.SaleViewModel.PayWindow.Close();
                                    }

                                    int tableId = _viewModel.PaySaleViewModel.SaleViewModel.TableId;
                                    _viewModel.PaySaleViewModel.SaleViewModel.Reset();

                                    var typeApp = SettingsManager.Instance.GetTypeApp();

                                    var appStateParameter = new AppStateParameter(
                                        typeApp == TypeAppEnumeration.Sale ? AppStateEnumerable.Sale : AppStateEnumerable.TableOverview,
                                        _viewModel.PaySaleViewModel.SaleViewModel.LoggedCashier,
                                        tableId,
                                        _viewModel.PaySaleViewModel.SaleViewModel);

                                    _viewModel.PaySaleViewModel.SaleViewModel.UpdateAppViewModelCommand.Execute(appStateParameter);
                                }
                            }
                            catch (Exception ex)
                            {
                                await transaction.RollbackAsync();
                                Log.Error("StornoKuhinjaCommand - Execute -> greška prilikom storinaranja kuhinje: ", ex);
                                MessageBox.Show("Desila se greška prilikom storinaranja kuhinje!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("StornoKuhinjaCommand - Execute -> greška prilikom storinaranja kuhinje: ", ex);
                MessageBox.Show("Desila se greška prilikom storinaranja kuhinje!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _isExecuted = false;
            }
        }
    }
}