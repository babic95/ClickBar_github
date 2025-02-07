using ClickBar.Models.Sale;
using ClickBar.ViewModels.Sale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using ClickBar_Database;
using Microsoft.EntityFrameworkCore;
using ClickBar_Logging;
using ClickBar_Common.Models.Order.Drlja;
using ClickBar_Settings;
using Newtonsoft.Json;
using System.Net.Http;
using ClickBar.Enums;

namespace ClickBar.Commands.Sale.Pay.SplitOrder
{
    public class StornoKuhinjaCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private SplitOrderViewModel _viewModel;

        public StornoKuhinjaCommand(SplitOrderViewModel viewModel)
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
                if (_viewModel.ItemsInvoiceForPay != null &&
                    _viewModel.ItemsInvoiceForPay.Any())
                {
                    if (_viewModel.PaySaleViewModel.SaleViewModel.TableId != 0)
                    {
                        using (SqliteDbContext sqliteDbContext = new SqliteDbContext())
                        {
                            var unprocessedOrderDB = sqliteDbContext.UnprocessedOrders.FirstOrDefault(x => x.PaymentPlaceId == _viewModel.PaySaleViewModel.SaleViewModel.TableId);

                            if (unprocessedOrderDB != null)
                            {
                                if (_viewModel.ItemsInvoiceForPay != null &&
                                    _viewModel.ItemsInvoiceForPay.Any())
                                {
                                    PorudzbinaDrlja porudzbinaDrlja = new PorudzbinaDrlja()
                                    {
                                        PorudzbinaId = unprocessedOrderDB.Id,
                                        Items = new List<PorudzbinaItemDrlja>(),
                                        RadnikId = unprocessedOrderDB.CashierId,
                                        StoBr = unprocessedOrderDB.PaymentPlaceId.ToString()
                                    };

                                    _viewModel.ItemsInvoiceForPay.ToList().ForEach(item =>
                                    {
                                        var u = sqliteDbContext.ItemsInUnprocessedOrder.FirstOrDefault(x => x.ItemId == item.Item.Id &&
                                        x.UnprocessedOrderId == unprocessedOrderDB.Id);

                                        if (u != null)
                                        {
                                            unprocessedOrderDB.TotalAmount -= item.TotalAmout;
                                            sqliteDbContext.UnprocessedOrders.Update(unprocessedOrderDB);

                                            if (u.Quantity == item.Quantity)
                                            {
                                                sqliteDbContext.ItemsInUnprocessedOrder.Remove(u);
                                            }
                                            else
                                            {
                                                u.Quantity -= item.Quantity;
                                                sqliteDbContext.ItemsInUnprocessedOrder.Update(u);
                                            }

                                            RetryHelper.ExecuteWithRetry(() => { sqliteDbContext.SaveChanges(); });
                                        }

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
                                    });
                                    var result = await PostStornoPorudzbinaAsync(porudzbinaDrlja);

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
                        MessageBox.Show("Uspešno ste stornirali kuhinju!", "Uspešno", MessageBoxButton.OK, MessageBoxImage.Information);
                        _viewModel.PaySaleViewModel.SplitOrderWindow.Close();
                        _viewModel.PaySaleViewModel.Window.Close();

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