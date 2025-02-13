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
        public async void Execute(object parameter)
        {
            try
            {
                if (parameter is int paymentPlaceId)
                {
                    if (paymentPlaceId > 0)
                    {
                        var newUnprocessedOrdersDB = _viewModel.DbContext.UnprocessedOrders.FirstOrDefault(u => u.PaymentPlaceId == paymentPlaceId);
                        var oldUnprocessedOrdersDB = _viewModel.DbContext.UnprocessedOrders.FirstOrDefault(u => u.PaymentPlaceId == _viewModel.SplitOrderViewModel.PaySaleViewModel.SaleViewModel.TableId);

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

                            if (newUnprocessedOrdersDB != null)
                            {
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
                                        _viewModel.DbContext.ItemsInUnprocessedOrder.Update(itemInUnprocessedOrder);
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

                                    newUnprocessedOrdersDB.TotalAmount += item.TotalAmout;
                                    oldUnprocessedOrdersDB.TotalAmount -= item.TotalAmout;

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
                                _viewModel.DbContext.UnprocessedOrders.Update(newUnprocessedOrdersDB);
                                if (oldUnprocessedOrdersDB.TotalAmount == 0)
                                {
                                    _viewModel.DbContext.UnprocessedOrders.Remove(oldUnprocessedOrdersDB);
                                }
                                else
                                {
                                    _viewModel.DbContext.UnprocessedOrders.Update(oldUnprocessedOrdersDB);
                                }
                                _viewModel.DbContext.SaveChanges();
                            }
                            else
                            {
                                UnprocessedOrderDB unprocessedOrderDB = new UnprocessedOrderDB()
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    CashierId = oldUnprocessedOrdersDB.CashierId,
                                    PaymentPlaceId = paymentPlaceId,
                                    TotalAmount = 0,
                                };
                                _viewModel.DbContext.UnprocessedOrders.Add(unprocessedOrderDB);
                                _viewModel.DbContext.SaveChanges();

                                if (movePorudzbinaClickBarDrlja != null)
                                {
                                    movePorudzbinaClickBarDrlja.NewUnprocessedOrderId = unprocessedOrderDB.Id;
                                    movePorudzbinaClickBarDrlja.NewSto = paymentPlaceId;
                                }

                                foreach (var item in _viewModel.SplitOrderViewModel.ItemsInvoiceForPay)
                                {
                                    var itemInUnprocessedOrder = new ItemInUnprocessedOrderDB
                                    {
                                        ItemId = item.Item.Id,
                                        Quantity = item.Quantity,
                                        UnprocessedOrderId = unprocessedOrderDB.Id
                                    };
                                    _viewModel.DbContext.ItemsInUnprocessedOrder.Add(itemInUnprocessedOrder);


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

                                    unprocessedOrderDB.TotalAmount += item.TotalAmout;
                                    oldUnprocessedOrdersDB.TotalAmount -= item.TotalAmout;

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
                                _viewModel.DbContext.UnprocessedOrders.Update(unprocessedOrderDB);

                                if (oldUnprocessedOrdersDB.TotalAmount == 0)
                                {
                                    _viewModel.DbContext.UnprocessedOrders.Remove(oldUnprocessedOrdersDB);
                                }
                                else
                                {
                                    _viewModel.DbContext.UnprocessedOrders.Update(oldUnprocessedOrdersDB);
                                }
                                _viewModel.DbContext.SaveChanges();
                            }

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
                var handler = new HttpClientHandler(); handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
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