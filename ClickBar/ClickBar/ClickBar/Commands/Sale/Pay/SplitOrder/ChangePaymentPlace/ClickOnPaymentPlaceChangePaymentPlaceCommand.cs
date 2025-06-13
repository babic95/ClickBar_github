using ClickBar.ViewModels.Sale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using ClickBar_Logging;
using ClickBar_DatabaseSQLManager;
using ClickBar_DatabaseSQLManager.Models;
using Microsoft.EntityFrameworkCore;
using ClickBar.Enums.Kuhinja;

namespace ClickBar.Commands.Sale.Pay.SplitOrder.ChangePaymentPlace
{
    public class ClickOnPaymentPlaceChangePaymentPlaceCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ChangePaymentPlaceViewModel _viewModel;

        public ClickOnPaymentPlaceChangePaymentPlaceCommand(ChangePaymentPlaceViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            try
            {
                if (parameter is int paymentPlaceId)
                {
                    if (paymentPlaceId > 0)
                    {
                        using (var dbContext = _viewModel.DbContextFactory.CreateDbContext())
                        using (var transaction = dbContext.Database.BeginTransaction())
                        {
                            var newUnprocessedOrdersDB = dbContext.UnprocessedOrders.Include(u => u.ItemsInUnprocessedOrder).FirstOrDefault(u => u.PaymentPlaceId == paymentPlaceId);
                            var oldUnprocessedOrdersDB = dbContext.UnprocessedOrders.Include(u => u.ItemsInUnprocessedOrder).FirstOrDefault(u => u.PaymentPlaceId == _viewModel.SplitOrderViewModel.PaySaleViewModel.SaleViewModel.TableId);

                            if (oldUnprocessedOrdersDB != null)
                            {
                                var oldOrdersTodayDB = dbContext.OrdersToday
                                    .Include(o => o.OrderTodayItems)
                                    .Where(o => o.UnprocessedOrderId == oldUnprocessedOrdersDB.Id &&
                                    o.Faza != (int)FazaKuhinjeEnumeration.Naplacena &&
                                    o.Faza != (int)FazaKuhinjeEnumeration.Obrisana)
                                    .ToList();

                                decimal totalAmountForPay = _viewModel.SplitOrderViewModel.ItemsInvoiceForPay.Sum(i => i.TotalAmout);

                                if (newUnprocessedOrdersDB == null)
                                {
                                    newUnprocessedOrdersDB = new UnprocessedOrderDB()
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        CashierId = oldUnprocessedOrdersDB.CashierId,
                                        PaymentPlaceId = paymentPlaceId,
                                        //TotalAmount = totalAmountForPay,
                                    };
                                    dbContext.UnprocessedOrders.Add(newUnprocessedOrdersDB);
                                    dbContext.SaveChanges();
                                }
                                //else
                                //{
                                //    //newUnprocessedOrdersDB.TotalAmount += totalAmountForPay;
                                //    //dbContext.UnprocessedOrders.Update(newUnprocessedOrdersDB);
                                //}

                                //oldUnprocessedOrdersDB.TotalAmount -= totalAmountForPay;
                                //dbContext.UnprocessedOrders.Update(oldUnprocessedOrdersDB);


                                foreach (var item in _viewModel.SplitOrderViewModel.ItemsInvoiceForPay)
                                {
                                    var itemsOrderToday = oldOrdersTodayDB.Where(o => o.OrderTodayItems.Any(oi => oi.ItemId == item.Item.Id &&
                                    oi.Quantity - oi.NaplacenoQuantity - oi.StornoQuantity > 0)).ToList();

                                    if (itemsOrderToday.Any())
                                    {
                                        decimal prebacivanjeQuantity = item.Quantity;

                                        foreach (var oldOrderToday in itemsOrderToday)
                                        {
                                            if (prebacivanjeQuantity == 0)
                                            {
                                                break;
                                            }

                                            var newOrderToday = dbContext.OrdersToday.Include(o => o.OrderTodayItems).
                                                FirstOrDefault(o => o.UnprocessedOrderId == newUnprocessedOrdersDB.Id &&
                                                o.OrderDateTime == oldOrderToday.OrderDateTime);

                                            if (newOrderToday == null)
                                            {
                                                newOrderToday = new OrderTodayDB()
                                                {
                                                    Id = Guid.NewGuid().ToString(),
                                                    CashierId = oldOrderToday.CashierId,
                                                    Counter = oldOrderToday.Counter,
                                                    CounterType = oldOrderToday.CounterType,
                                                    Name = oldOrderToday.Name,
                                                    OrderDateTime = oldOrderToday.OrderDateTime,
                                                    TableId = newUnprocessedOrdersDB.PaymentPlaceId,
                                                    Faza = oldOrderToday.Faza,
                                                    TotalPrice = 0,
                                                    UnprocessedOrderId = newUnprocessedOrdersDB.Id,
                                                    OrderTodayItems = new List<OrderTodayItemDB>()
                                                };
                                                dbContext.OrdersToday.Add(newOrderToday);
                                                dbContext.SaveChanges();
                                            }

                                            foreach (var oldOrderTodayItem in oldOrderToday.OrderTodayItems)
                                            {
                                                if (prebacivanjeQuantity == 0)
                                                {
                                                    break;
                                                }

                                                if (oldOrderTodayItem.ItemId == item.Item.Id)
                                                {
                                                    var newOrderTodayItem = dbContext.OrderTodayItems.FirstOrDefault(oi => oi.OrderTodayId == newOrderToday.Id &&
                                                    oi.ItemId == item.Item.Id);

                                                    if (newOrderTodayItem == null)
                                                    {
                                                        newOrderTodayItem = new OrderTodayItemDB()
                                                        {
                                                            Id = Guid.NewGuid().ToString(),
                                                            ItemId = item.Item.Id,
                                                            OrderTodayId = newOrderToday.Id,
                                                            Quantity = 0,
                                                            TotalPrice = 0,
                                                            Zelja = oldOrderTodayItem.Zelja,
                                                        };
                                                        dbContext.OrderTodayItems.Add(newOrderTodayItem);
                                                        dbContext.SaveChanges();
                                                    }

                                                    decimal preostaKolicina = oldOrderTodayItem.Quantity - oldOrderTodayItem.StornoQuantity - oldOrderTodayItem.NaplacenoQuantity;

                                                    if (preostaKolicina > 0)
                                                    {
                                                        if (preostaKolicina >= prebacivanjeQuantity)
                                                        {
                                                            decimal totalSum = Decimal.Round((oldOrderTodayItem.TotalPrice / oldOrderTodayItem.Quantity)
                                                                * prebacivanjeQuantity, 2);

                                                            oldOrderToday.TotalPrice -= totalSum;
                                                            newOrderToday.TotalPrice += totalSum;
                                                            newOrderTodayItem.Quantity += prebacivanjeQuantity;
                                                            newOrderTodayItem.TotalPrice += totalSum;
                                                            oldOrderTodayItem.Quantity -= prebacivanjeQuantity;
                                                            oldOrderTodayItem.TotalPrice -= totalSum;
                                                            prebacivanjeQuantity = 0;

                                                            if (oldOrderTodayItem.Quantity == 0)
                                                            {
                                                                dbContext.OrderTodayItems.Remove(oldOrderTodayItem);
                                                            }
                                                            else
                                                            {
                                                                dbContext.OrderTodayItems.Update(oldOrderTodayItem);
                                                            }
                                                            if (oldOrderToday.TotalPrice == 0)
                                                            {
                                                                dbContext.OrdersToday.Remove(oldOrderToday);
                                                            }
                                                            else
                                                            {
                                                                dbContext.OrdersToday.Update(oldOrderToday);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            decimal totalSum = Decimal.Round((oldOrderTodayItem.TotalPrice / oldOrderTodayItem.Quantity)
                                                                * preostaKolicina, 2);

                                                            oldOrderToday.TotalPrice -= totalSum;
                                                            newOrderToday.TotalPrice += totalSum;
                                                            newOrderTodayItem.Quantity += preostaKolicina;
                                                            newOrderTodayItem.TotalPrice += totalSum;
                                                            prebacivanjeQuantity -= preostaKolicina;

                                                            dbContext.OrderTodayItems.Remove(oldOrderTodayItem);

                                                            if (oldOrderToday.TotalPrice == 0)
                                                            {
                                                                dbContext.OrdersToday.Remove(oldOrderToday);
                                                            }
                                                            else
                                                            {
                                                                dbContext.OrdersToday.Update(oldOrderToday);
                                                            }

                                                        }
                                                        dbContext.OrdersToday.Update(newOrderToday);
                                                        dbContext.OrderTodayItems.Update(newOrderTodayItem);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                dbContext.SaveChanges();

                                var oldU = dbContext.OrdersToday.Where(o => o.UnprocessedOrderId == oldUnprocessedOrdersDB.Id).ToList();

                                if (!oldU.Any() ||
                                    oldU.Sum(o => o.OrderTodayItems.Sum(oti => oti.Quantity - oti.StornoQuantity - oti.NaplacenoQuantity)) == 0)
                                {
                                    dbContext.UnprocessedOrders.Remove(oldUnprocessedOrdersDB);
                                    dbContext.SaveChanges();
                                }

                                transaction.Commit();

                                _viewModel.TableId = paymentPlaceId;

                                _viewModel.SplitOrderViewModel.ChangePaymentPlaceWindow.Close();
                                _viewModel.SplitOrderViewModel.PaySaleViewModel.SplitOrderWindow.Close();
                                if (_viewModel.SplitOrderViewModel.PaySaleViewModel.SaleViewModel.PayWindow != null)
                                {
                                    _viewModel.SplitOrderViewModel.PaySaleViewModel.SaleViewModel.PayWindow.Close();
                                }
                                _viewModel.SplitOrderViewModel.PaySaleViewModel.SaleViewModel.Reset();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Desila se greška!",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Log.Error("ClickOnPaymentPlaceChangePaymentPlaceCommand -> Greška u biranju platnog mesta", ex);
            }
        }
    }
}