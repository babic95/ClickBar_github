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
using ClickBar_Common.Models.Order.Drlja;
using ClickBar_Settings;
using Newtonsoft.Json;
using System.Net.Http;
using Microsoft.EntityFrameworkCore;

namespace ClickBar.Commands.Sale.Pay.SplitOrder.ChangePaymentPlace
{
    public class ClickOnPaymentPlaceChangePaymentPlaceCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ChangePaymentPlaceViewModel _viewModel;
        private bool _isFirstTime;

        public ClickOnPaymentPlaceChangePaymentPlaceCommand(ChangePaymentPlaceViewModel viewModel)
        {
            _viewModel = viewModel;
            _isFirstTime = true;
        }

        public bool CanExecute(object? parameter)
        {
            return _isFirstTime;
        }

        public async void Execute(object parameter)
        {
            if (!_isFirstTime)
            {
                return;
            }
            _isFirstTime = false;
            try
            {
                if (parameter is int paymentPlaceId)
                {
                    if (paymentPlaceId > 0)
                    {
                        var newUnprocessedOrdersDB = _viewModel.DbContext.UnprocessedOrders.Include(u => u.ItemsInUnprocessedOrder).FirstOrDefault(u => u.PaymentPlaceId == paymentPlaceId);
                        var oldUnprocessedOrdersDB = _viewModel.DbContext.UnprocessedOrders.Include(u => u.ItemsInUnprocessedOrder).FirstOrDefault(u => u.PaymentPlaceId == _viewModel.SplitOrderViewModel.PaySaleViewModel.SaleViewModel.TableId);

                        if (oldUnprocessedOrdersDB != null)
                        {
                            MovePorudzbinaClickBarDrlja? movePorudzbinaClickBarDrlja = null;

                            if (!string.IsNullOrEmpty(SettingsManager.Instance.GetPathToDrljaKuhinjaDB()))
                            {
                                movePorudzbinaClickBarDrlja = new MovePorudzbinaClickBarDrlja()
                                {
                                    OldUnprocessedOrderId = oldUnprocessedOrdersDB.Id,
                                    Items = new List<PorudzbinaItemDrlja>(),
                                };
                            }

                            var oldOrdersTodayDB = _viewModel.DbContext.OrdersToday
                                .Where(o => o.UnprocessedOrderId == oldUnprocessedOrdersDB.Id)
                                .Include(o => o.OrderTodayItems)
                                .ToList();

                            if (newUnprocessedOrdersDB == null)
                            {
                                newUnprocessedOrdersDB = new UnprocessedOrderDB()
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    CashierId = oldUnprocessedOrdersDB.CashierId,
                                    PaymentPlaceId = paymentPlaceId,
                                    TotalAmount = 0,
                                };
                                _viewModel.DbContext.UnprocessedOrders.Add(newUnprocessedOrdersDB);
                                await _viewModel.DbContext.SaveChangesAsync(); // Save changes to ensure UnprocessedOrderId exists
                            }

                            if (movePorudzbinaClickBarDrlja != null)
                            {
                                movePorudzbinaClickBarDrlja.NewUnprocessedOrderId = newUnprocessedOrdersDB.Id;
                                movePorudzbinaClickBarDrlja.NewSto = newUnprocessedOrdersDB.PaymentPlaceId;
                            }

                            foreach (var item in _viewModel.SplitOrderViewModel.ItemsInvoiceForPay)
                            {
                                var itemInUnprocessedOrder = _viewModel.DbContext.ItemsInUnprocessedOrder.FirstOrDefault(i => i.ItemId == item.Item.Id &&
                                i.UnprocessedOrderId == newUnprocessedOrdersDB.Id);

                                if (itemInUnprocessedOrder != null)
                                {
                                    itemInUnprocessedOrder.Quantity += item.Quantity;
                                }
                                else
                                {
                                    itemInUnprocessedOrder = new ItemInUnprocessedOrderDB
                                    {
                                        ItemId = item.Item.Id,
                                        Quantity = item.Quantity,
                                        UnprocessedOrderId = newUnprocessedOrdersDB.Id
                                    };
                                    _viewModel.DbContext.ItemsInUnprocessedOrder.Add(itemInUnprocessedOrder);
                                }

                                await _viewModel.DbContext.SaveChangesAsync(); // Save changes for each item

                                var itemInOldUnprocessedOrder = _viewModel.DbContext.ItemsInUnprocessedOrder.FirstOrDefault(i => i.ItemId == item.Item.Id &&
                                i.UnprocessedOrderId == oldUnprocessedOrdersDB.Id);

                                if (itemInOldUnprocessedOrder != null)
                                {
                                    if (itemInOldUnprocessedOrder.Quantity == item.Quantity)
                                    {
                                        _viewModel.DbContext.ItemsInUnprocessedOrder.Remove(itemInOldUnprocessedOrder);
                                    }
                                    else
                                    {
                                        itemInOldUnprocessedOrder.Quantity -= item.Quantity;
                                        _viewModel.DbContext.ItemsInUnprocessedOrder.Update(itemInOldUnprocessedOrder);
                                    }
                                }

                                await _viewModel.DbContext.SaveChangesAsync(); // Save changes for each item

                                newUnprocessedOrdersDB.TotalAmount += item.TotalAmout;
                                oldUnprocessedOrdersDB.TotalAmount -= item.TotalAmout;

                                var orders = oldOrdersTodayDB.Where(o => o.OrderTodayItems.Any(oi => oi.ItemId == item.Item.Id)).
                                    OrderBy(o => o.OrderDateTime).ToList();

                                if (orders != null && orders.Any())
                                {
                                    decimal quantityItem = item.Quantity;
                                    foreach (var order in orders)
                                    {
                                        if(quantityItem == 0)
                                        {
                                            break;
                                        }

                                        var orderTodayDB = _viewModel.DbContext.OrdersToday.Include(o => o.OrderTodayItems).
                                            FirstOrDefault(o => o.UnprocessedOrderId == newUnprocessedOrdersDB.Id &&
                                            o.OrderDateTime == order.OrderDateTime);

                                        if (orderTodayDB == null)
                                        {
                                            orderTodayDB = new OrderTodayDB()
                                            {
                                                Id = Guid.NewGuid().ToString(),
                                                CashierId = order.CashierId,
                                                Counter = order.Counter,
                                                CounterType = order.CounterType,
                                                Name = order.Name,
                                                OrderDateTime = order.OrderDateTime,
                                                TableId = order.TableId,
                                                TotalPrice = 0,
                                                UnprocessedOrderId = newUnprocessedOrdersDB.Id,
                                                OrderTodayItems = new List<OrderTodayItemDB>()
                                            };

                                            _viewModel.DbContext.OrdersToday.Add(orderTodayDB);
                                        }

                                        var orderTodayItem = order.OrderTodayItems.FirstOrDefault(oi => oi.ItemId == item.Item.Id);
                                        if (orderTodayItem != null)
                                        {
                                            var orderTodayItemDB = orderTodayDB.OrderTodayItems.FirstOrDefault(oi => oi.OrderTodayId == orderTodayDB.Id &&
                                            oi.ItemId == item.Item.Id);

                                            if (orderTodayItemDB == null)
                                            {
                                                orderTodayItemDB = new OrderTodayItemDB()
                                                {
                                                    ItemId = item.Item.Id,
                                                    OrderTodayId = orderTodayDB.Id,
                                                    Quantity = 0,
                                                    TotalPrice = 0
                                                };

                                                orderTodayDB.OrderTodayItems.Add(orderTodayItemDB);
                                            }

                                            decimal unitPrice = Decimal.Round(orderTodayItem.TotalPrice / orderTodayItem.Quantity, 2);

                                            if (orderTodayItem.Quantity > quantityItem)
                                            {
                                                order.TotalPrice -= Decimal.Round(unitPrice * quantityItem);
                                                orderTodayDB.TotalPrice += Decimal.Round(unitPrice * quantityItem);
                                                orderTodayItem.Quantity -= quantityItem;

                                                orderTodayItemDB.Quantity += quantityItem;
                                                orderTodayItemDB.TotalPrice += Decimal.Round(unitPrice * quantityItem, 2);

                                                quantityItem = 0;
                                            }
                                            else
                                            {
                                                orderTodayDB.TotalPrice += orderTodayItem.TotalPrice;
                                                order.TotalPrice -= orderTodayItem.TotalPrice;

                                                orderTodayItemDB.Quantity += orderTodayItem.Quantity;
                                                orderTodayItemDB.TotalPrice += orderTodayItem.TotalPrice;

                                                _viewModel.DbContext.OrderTodayItems.Remove(orderTodayItem);

                                                quantityItem -= orderTodayItem.Quantity;
                                            }

                                            await _viewModel.DbContext.SaveChangesAsync(); // Save changes for each orderTodayItem

                                            if (order.TotalPrice == 0)
                                            {
                                                _viewModel.DbContext.OrdersToday.Remove(order);
                                            }

                                            await _viewModel.DbContext.SaveChangesAsync(); // Save changes for each order

                                            if (quantityItem == 0)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                }

                                if (movePorudzbinaClickBarDrlja != null)
                                {
                                    movePorudzbinaClickBarDrlja.Items.Add(new PorudzbinaItemDrlja()
                                    {
                                        Kolicina = item.Quantity,
                                        MPC = item.Item.SellingUnitPrice,
                                        ItemIdString = item.Item.Id,
                                        Naziv = item.Item.Name,
                                        RBS = 0,
                                        BrojNarudzbe = 0,
                                        Jm = item.Item.Jm
                                    });
                                }
                            }

                            if (oldUnprocessedOrdersDB.TotalAmount == 0)
                            {
                                _viewModel.DbContext.UnprocessedOrders.Remove(oldUnprocessedOrdersDB);
                            }

                            await _viewModel.DbContext.SaveChangesAsync(); // Save final changes

                            if (movePorudzbinaClickBarDrlja != null)
                            {
                                var result = await PostChangePaymentPlaceAsync(movePorudzbinaClickBarDrlja);

                                if (result != 200)
                                {
                                    MessageBox.Show("Desila se greška prilikom prebacivanja porudžbine!\nObratite se serviseru.",
                                        "Greška",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Error);

                                    Log.Error($"ClickOnPaymentPlaceChangePaymentPlaceCommand -> greška prilikom prebacivanja porudzbine na drugi sto: Code={result}");
                                }
                            }

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
            catch (Exception ex)
            {
                MessageBox.Show("Desila se greška!",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Log.Error("ClickOnPaymentPlaceChangePaymentPlaceCommand -> Greška u biranju platnog mesta", ex);
            }
        }

        private async Task<int> PostChangePaymentPlaceAsync(MovePorudzbinaClickBarDrlja movePorudzbinaClickBarDrlja)
        {
            try
            {
                var handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
                HttpClient client = new HttpClient(handler);

                var ip = SettingsManager.Instance.GetHOstPC_IP();

                string requestUrl = string.Empty;
                if (string.IsNullOrEmpty(ip))
                {
                    requestUrl = "http://localhost:5000/api/porudzbina/movePorudzbina";
                }
                else
                {
                    requestUrl = $"https://{ip}:44323/api/porudzbina/movePorudzbina";
                }

                var json = JsonConvert.SerializeObject(movePorudzbinaClickBarDrlja);
                var data = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(requestUrl, data);

                if (response.IsSuccessStatusCode)
                {
                    return (int)response.StatusCode;
                }
                else
                {
                    Log.Error($"ClickOnPaymentPlaceChangePaymentPlaceCommand -> PostChangePaymentPlaceAsync -> Status je: {(int)response.StatusCode} -> {response.StatusCode.ToString()} ");
                    return (int)response.StatusCode;
                }
            }
            catch (Exception ex)
            {
                Log.Error("ClickOnPaymentPlaceChangePaymentPlaceCommand -> PostChangePaymentPlaceAsync -> Greska prilikom prebacivanja porudzbine: ", ex);
                return -1;
            }
        }
    }
}