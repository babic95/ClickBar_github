﻿using ClickBar.ViewModels;
using ClickBar_DatabaseSQLManager;
using ClickBar_Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using ClickBar_Logging;
using ClickBar_DatabaseSQLManager.Models;
using System.Windows.Navigation;
using ClickBar_Database_Drlja;
using ClickBar.Enums;
using ClickBar.ViewModels.Sale;

namespace ClickBar.Commands.Sale
{
    public class RemoveOrderCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private SaleViewModel _viewModel;

        public RemoveOrderCommand(SaleViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            var result = MessageBox.Show("Da li ste sigurni da želite da obrišete trenutnu porudžbinu?", "Brisanje porudžbine",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    if (_viewModel.TableId > 0 &&
                        SettingsManager.Instance.CancelOrderFromTable())
                    {
                        if (_viewModel.OldOrders.Count == 0)
                        {
                            MessageBox.Show("Nema ništa za brisanje!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        var order = _viewModel.DbContext.UnprocessedOrders.FirstOrDefault(order => order.PaymentPlaceId == _viewModel.TableId);
                        decimal totalForDelete = 0;

                        if (order != null)
                        {
                            var itemsInOrder = _viewModel.DbContext.ItemsInUnprocessedOrder.Where(item => item.UnprocessedOrderId == order.Id);

                            if (itemsInOrder != null &&
                                itemsInOrder.Any())
                            {
                                //if (itemsInOrder.Count() != _viewModel.OldOrders.Count)
                                //{
                                //    Log.Error("RemoveOrderCommand -> Execute -> Greška prilikom brisanja porudžbine! Broj stavki u bazi i broj stavki u listi se ne poklapaju!");
                                //    MessageBox.Show("Greška prilikom brisanja porudžbine!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                                //    return;
                                //}

                                int orderBrisanjeCounter = 1;
                                int orderTotalCounter = 1;

                                var ordersTodayBrisanjeDB = _viewModel.DbContext.OrdersToday.Where(o => o.OrderDateTime.Date == DateTime.Now.Date &&
                                !string.IsNullOrEmpty(o.Name) &&
                                o.Name.ToLower().Contains("brisanje"));

                                var ordersTodayDB = _viewModel.DbContext.OrdersToday.Where(o => o.OrderDateTime.Date == DateTime.Now.Date);

                                if (ordersTodayBrisanjeDB != null &&
                                    ordersTodayBrisanjeDB.Any())
                                {
                                    orderBrisanjeCounter = ordersTodayBrisanjeDB.Max(o => o.CounterType);
                                    orderBrisanjeCounter++;
                                }

                                if (ordersTodayDB != null &&
                                    ordersTodayDB.Any())
                                {
                                    orderTotalCounter = ordersTodayDB.Max(o => o.Counter);
                                    orderTotalCounter++;
                                }

                                OrderTodayDB orderTodayDB = new OrderTodayDB()
                                {
                                    CashierId = _viewModel.LoggedCashier.Id,
                                    UnprocessedOrderId = order.Id,
                                    Id = Guid.NewGuid().ToString(),
                                    OrderDateTime = DateTime.Now,
                                    TableId = order.PaymentPlaceId,
                                    TotalPrice = 0,
                                    CounterType = orderBrisanjeCounter,
                                    Counter = orderTotalCounter,
                                    Name = $"BRISANJE{orderBrisanjeCounter}__{orderTotalCounter}",
                                    OrderTodayItems = new List<OrderTodayItemDB>()
                                };

                                List<OrderTodayItemDB> itemsForDelete = new List<OrderTodayItemDB>();

                                foreach(var oldOrder in _viewModel.OldOrders)
                                {
                                    foreach (var item in oldOrder.Items)
                                    {
                                        OrderTodayItemDB orderTodayItemDB = new OrderTodayItemDB()
                                        {
                                            ItemId = item.Item.Id,
                                            OrderTodayId = orderTodayDB.Id,
                                            Quantity = -1 * item.Quantity,
                                            TotalPrice = Decimal.Round(item.Quantity * item.Item.SellingUnitPrice, 2)
                                        };

                                        orderTodayDB.TotalPrice += orderTodayItemDB.TotalPrice;

                                        itemsForDelete.Add(orderTodayItemDB);
                                    }
                                }

                                _viewModel.DbContext.ItemsInUnprocessedOrder.RemoveRange(itemsInOrder);
                                _viewModel.DbContext.SaveChanges();

                                if (itemsForDelete.Any())
                                {
                                    itemsForDelete.ForEach(i =>
                                    {
                                        var itemFD = orderTodayDB.OrderTodayItems.FirstOrDefault(o => o.ItemId == i.ItemId);

                                        if (itemFD == null)
                                        {
                                            orderTodayDB.OrderTodayItems.Add(i);
                                        }
                                        else
                                        {
                                            itemFD.Quantity += i.Quantity;
                                            itemFD.TotalPrice += i.TotalPrice;
                                        }

                                        totalForDelete += i.TotalPrice;
                                    });
                                    _viewModel.DbContext.OrdersToday.Add(orderTodayDB);
                                    _viewModel.DbContext.SaveChanges();
                                }


                                //itemsInOrder.ToList().ForEach(item =>
                                //{
                                //    sqliteDbContext.ItemsInUnprocessedOrder.Remove(item);
                                //});
                            }

                            itemsInOrder = _viewModel.DbContext.ItemsInUnprocessedOrder.Where(item => item.UnprocessedOrderId == order.Id);
                            if (itemsInOrder == null ||
                                !itemsInOrder.Any())
                            {
                                _viewModel.DbContext.UnprocessedOrders.Remove(order);

                                var pathToDrljaDB = SettingsManager.Instance.GetPathToDrljaKuhinjaDB();

                                if (_viewModel.DrljaDbContext != null &&
                                    !string.IsNullOrEmpty(pathToDrljaDB))
                                {
                                    var narudzbineDrlja = _viewModel.DrljaDbContext.Narudzbine.Where(nar => nar.TR_STO.Contains(order.PaymentPlaceId.ToString())
                                    && nar.TR_FAZA != 4);
                                    if (narudzbineDrlja != null &&
                                        narudzbineDrlja.Any())
                                    {
                                        narudzbineDrlja.ToList().ForEach(narudzbinaDrlja =>
                                        {
                                            narudzbinaDrlja.TR_FAZA = 4;
                                            _viewModel.DrljaDbContext.Narudzbine.Update(narudzbinaDrlja);
                                            RetryHelperDrlja.ExecuteWithRetry(() => { _viewModel.DrljaDbContext.SaveChanges(); });
                                        });
                                    }
                                }
                            }
                            else
                            {
                                order.TotalAmount += totalForDelete;
                                _viewModel.DbContext.UnprocessedOrders.Update(order);
                            }
                            _viewModel.DbContext.SaveChanges();
                        }

                        _viewModel.Reset();

                        AppStateParameter appStateParameter = new AppStateParameter(AppStateEnumerable.TableOverview,
                            _viewModel.LoggedCashier,
                            _viewModel);
                        _viewModel.UpdateAppViewModelCommand.Execute(appStateParameter);
                    }
                }
                catch
                {
                    MessageBox.Show("Greška prilikom brisanja porudžbine!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
