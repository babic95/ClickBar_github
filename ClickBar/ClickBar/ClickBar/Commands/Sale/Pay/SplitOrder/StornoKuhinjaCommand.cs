using ClickBar.Models.Sale;
using ClickBar.ViewModels.Sale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public void Execute(object parameter)
        {
            if(_isExecuted)
            {
                return;
            }
            try
            {
                if (_viewModel.ItemsInvoiceForPay != null &&
                    _viewModel.ItemsInvoiceForPay.Any())
                {
                    if (_viewModel.PaySaleViewModel.SaleViewModel.TableId != 0)
                    {
                        var unprocessedOrderDB = _viewModel.DbContext.UnprocessedOrders.FirstOrDefault(x => x.PaymentPlaceId == _viewModel.PaySaleViewModel.SaleViewModel.TableId);

                        if (unprocessedOrderDB != null)
                        {
                            if (_viewModel.ItemsInvoiceForPay != null &&
                                _viewModel.ItemsInvoiceForPay.Any())
                            {
                                PorudzbinaDrlja? porudzbinaDrlja = null;

                                if (!string.IsNullOrEmpty(SettingsManager.Instance.GetPathToDrljaKuhinjaDB()))
                                {
                                    porudzbinaDrlja = new PorudzbinaDrlja()
                                    {
                                        PorudzbinaId = unprocessedOrderDB.Id,
                                        Items = new List<PorudzbinaItemDrlja>(),
                                        RadnikId = unprocessedOrderDB.CashierId,
                                        StoBr = unprocessedOrderDB.PaymentPlaceId.ToString()
                                    };
                                }

                                int orderStornoCounter = 1;
                                int orderTotalCounter = 1;

                                var ordersTodayBrisanjeDB = _viewModel.DbContext.OrdersToday.Where(o => o.OrderDateTime.Date == DateTime.Now.Date &&
                                !string.IsNullOrEmpty(o.Name) &&
                                o.Name.ToLower().Contains("storno"));

                                var ordersTodayDB = _viewModel.DbContext.OrdersToday.Where(o => o.OrderDateTime.Date == DateTime.Now.Date);

                                if (ordersTodayBrisanjeDB != null &&
                                    ordersTodayBrisanjeDB.Any())
                                {
                                    orderStornoCounter = ordersTodayBrisanjeDB.Max(o => o.CounterType);
                                    orderStornoCounter++;
                                }

                                if (ordersTodayDB != null &&
                                    ordersTodayDB.Any())
                                {
                                    orderTotalCounter = ordersTodayDB.Max(o => o.Counter);
                                    orderTotalCounter++;
                                }

                                OrderTodayDB orderTodayDB = new OrderTodayDB()
                                {
                                    CashierId = unprocessedOrderDB.CashierId,
                                    UnprocessedOrderId = unprocessedOrderDB.Id,
                                    Id = Guid.NewGuid().ToString(),
                                    Name = $"STORNO{orderStornoCounter}__{orderTotalCounter}",
                                    OrderDateTime = DateTime.Now,
                                    TableId = unprocessedOrderDB.PaymentPlaceId,
                                    TotalPrice = 0,
                                    Counter = orderTotalCounter,
                                    CounterType = orderStornoCounter,
                                    OrderTodayItems = new List<OrderTodayItemDB>(),
                                };

                                _viewModel.ItemsInvoiceForPay.ToList().ForEach(item =>
                                {
                                    var u = _viewModel.DbContext.ItemsInUnprocessedOrder.FirstOrDefault(x => x.ItemId == item.Item.Id &&
                                    x.UnprocessedOrderId == unprocessedOrderDB.Id);

                                    if (u != null)
                                    {
                                        unprocessedOrderDB.TotalAmount -= item.TotalAmout;
                                        _viewModel.DbContext.UnprocessedOrders.Update(unprocessedOrderDB);

                                        if (u.Quantity == item.Quantity)
                                        {
                                            _viewModel.DbContext.ItemsInUnprocessedOrder.Remove(u);
                                        }
                                        else
                                        {
                                            u.Quantity -= item.Quantity;
                                            _viewModel.DbContext.ItemsInUnprocessedOrder.Update(u);
                                        }

                                        _viewModel.DbContext.SaveChanges();
                                    }

                                    if (porudzbinaDrlja != null)
                                    {
                                        porudzbinaDrlja.Items.Add(new PorudzbinaItemDrlja()
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

                                    orderTodayDB.OrderTodayItems.Add(new OrderTodayItemDB()
                                    {
                                        ItemId = item.Item.Id,
                                        OrderTodayId = orderTodayDB.Id,
                                        Quantity = -1 * item.Quantity,
                                        TotalPrice = item.TotalAmout
                                    });
                                });

                                _viewModel.DbContext.OrdersToday.Add(orderTodayDB);
                                _viewModel.DbContext.SaveChanges();

                                if (porudzbinaDrlja != null)
                                {
                                    var result = PostStornoPorudzbinaAsync(porudzbinaDrlja).Result;

                                    if (result != 200)
                                    {
                                        MessageBox.Show("Desila se greška prilikom storniranja porudžbine!\nObratite se serviseru.",
                                            "Greška",
                                            MessageBoxButton.OK,
                                            MessageBoxImage.Error);

                                        Log.Error($"StornoKuhinjaCommand -> greška prilikom storinaranja kuhinje: Code={result}");
                                    }
                                }
                            }
                        }
                        MessageBox.Show("Uspešno!", "Uspešno", MessageBoxButton.OK, MessageBoxImage.Information);
                        _viewModel.PaySaleViewModel.SplitOrderWindow.Close();

                        if(_viewModel.PaySaleViewModel.SaleViewModel.PayWindow != null)
                        {
                            _viewModel.PaySaleViewModel.SaleViewModel.PayWindow.Close();
                        }

                        _viewModel.PaySaleViewModel.SaleViewModel.Reset();

                        AppStateParameter appStateParameter = new AppStateParameter(AppStateEnumerable.TableOverview,
                            _viewModel.PaySaleViewModel.SaleViewModel.LoggedCashier,
                            _viewModel.PaySaleViewModel.SaleViewModel);
                        _viewModel.PaySaleViewModel.SaleViewModel.UpdateAppViewModelCommand.Execute(appStateParameter);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Desila se greška prilikom storinaranja kuhinje!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                Log.Error("StornoKuhinjaCommand - Execute -> greška prilikom storinaranja kuhinje: ", ex);
            }
        }
        private async Task<int> PostStornoPorudzbinaAsync(PorudzbinaDrlja porudzbina)
        {
            try
            {
                var handler = new HttpClientHandler(); handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
                HttpClient client = new HttpClient(handler);

                var ip = SettingsManager.Instance.GetHOstPC_IP();

                string requestUrl = string.Empty;
                if (string.IsNullOrEmpty(ip))
                {
                    requestUrl = "http://localhost:5000/api/porudzbina/stornoKuhinja";
                }
                else
                {
                    requestUrl = $"https://{ip}:44323/api/porudzbina/stornoKuhinja";
                }

                var json = JsonConvert.SerializeObject(porudzbina);
                var data = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(requestUrl, data);

                if (response.IsSuccessStatusCode)
                {
                    return (int)response.StatusCode;
                }
                else
                {
                    var message = await response.Content.ReadAsStringAsync();
                    Log.Error($"StornoKuhinjaCommand -> PostStornoPorudzbinaAsync -> Status je: {(int)response.StatusCode} -> {response.StatusCode.ToString()}: {message}");
                    return (int)response.StatusCode;
                }
            }
            catch (Exception ex)
            {
                Log.Error("StornoKuhinjaCommand -> PostStornoPorudzbinaAsync -> Greska prilikom storniranja porudzbine: ", ex);
                return -1;
            }
        }
    }
}