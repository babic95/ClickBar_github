using ClickBar.Enums;
using ClickBar.Models.Sale;
using ClickBar.Models.TableOverview;
using ClickBar.ViewModels;
using ClickBar_Common.Enums;
using ClickBar_Common.Models.Invoice;
using ClickBar_Common.Models.Order.Drlja;
using ClickBar_Database_Drlja;
using ClickBar_DatabaseSQLManager;
using ClickBar_DatabaseSQLManager.Models;
using ClickBar_Logging;
using ClickBar_Printer.PaperFormat;
using ClickBar_Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ClickBar.Commands.Sale
{
    public class HookOrderOnTableCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private SaleViewModel _viewModel;
        private readonly IServiceProvider _serviceProvider;

        public HookOrderOnTableCommand(SaleViewModel viewModel, IServiceProvider serviceProvider)
        {
            _viewModel = viewModel;
            _serviceProvider = serviceProvider;
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            if (_viewModel.TableId == 0)
            {
                var dbContextFactory = _serviceProvider.GetRequiredService<IDbContextFactory<SqlServerDbContext>>();
                var drljaDbContextFactory = _serviceProvider.GetRequiredService<IDbContextFactory<SqliteDrljaDbContext>>();
                _viewModel.TableOverviewViewModel = new TableOverviewViewModel(_serviceProvider, dbContextFactory, drljaDbContextFactory, _viewModel);

                _viewModel.CurrentOrder = new Order(_viewModel.LoggedCashier, _viewModel.ItemsInvoice)
                {
                    TableId = _viewModel.TableId,
                };

                AppStateParameter appStateParameter = new AppStateParameter(AppStateEnumerable.TableOverview,
                    _viewModel.LoggedCashier,
                    _viewModel);
                _viewModel.UpdateAppViewModelCommand.Execute(appStateParameter);
            }
            else
            {
                try
                {
                    var tableDB = _viewModel.DbContext.PaymentPlaces.Find(_viewModel.TableId);

                    if (tableDB != null)
                    {
                        if (tableDB.Popust > 0 &&
                            tableDB.Popust < 100)
                        {
                            var itemBezPopusta = _viewModel.ItemsInvoice.FirstOrDefault(i => i.Item.IsCheckedZabraniPopust);

                            if (itemBezPopusta != null)
                            {
                                MessageBox.Show("Na ovaj sto ne možete staviti artikle koji su označeni kao ZABRANJEN popust!",
                                    "Zabranjen popust",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);

                                _viewModel.Reset();
                                return;
                            }
                        }
                    }

                    var unprocessedOrderDB = _viewModel.DbContext.UnprocessedOrders.FirstOrDefault(table => table.PaymentPlaceId == _viewModel.TableId);

                    if (unprocessedOrderDB != null)
                    {
                        _viewModel.ItemsInvoice.ToList().ForEach(item =>
                        {
                            var itemInUnprocessedOrderDB = _viewModel.DbContext.ItemsInUnprocessedOrder.FirstOrDefault(i => i.UnprocessedOrderId == unprocessedOrderDB.Id &&
                            i.ItemId == item.Item.Id);

                            if (itemInUnprocessedOrderDB == null)
                            {
                                itemInUnprocessedOrderDB = new ItemInUnprocessedOrderDB()
                                {
                                    ItemId = item.Item.Id,
                                    Quantity = item.Quantity,
                                    UnprocessedOrderId = unprocessedOrderDB.Id,
                                };

                                _viewModel.DbContext.ItemsInUnprocessedOrder.Add(itemInUnprocessedOrderDB);
                            }
                            else
                            {
                                itemInUnprocessedOrderDB.Quantity += item.Quantity;
                                _viewModel.DbContext.ItemsInUnprocessedOrder.Update(itemInUnprocessedOrderDB);
                            }

                            unprocessedOrderDB.TotalAmount += item.TotalAmout;
                            unprocessedOrderDB.CashierId = _viewModel.LoggedCashier.Id;
                            _viewModel.DbContext.UnprocessedOrders.Update(unprocessedOrderDB);
                        });
                    }
                    else
                    {
                        unprocessedOrderDB = new UnprocessedOrderDB()
                        {
                            Id = Guid.NewGuid().ToString(),
                            CashierId = _viewModel.LoggedCashier.Id,
                            PaymentPlaceId = _viewModel.CurrentOrder.TableId,
                            TotalAmount = 0,
                            ItemsInUnprocessedOrder = new List<ItemInUnprocessedOrderDB>(),
                        };

                        _viewModel.ItemsInvoice.ToList().ForEach(item =>
                        {
                            var itemInUnprocessedOrderDB = new ItemInUnprocessedOrderDB()
                            {
                                ItemId = item.Item.Id,
                                Quantity = item.Quantity,
                                UnprocessedOrderId = unprocessedOrderDB.Id,
                            };

                            unprocessedOrderDB.ItemsInUnprocessedOrder.Add(itemInUnprocessedOrderDB);

                            unprocessedOrderDB.TotalAmount += item.TotalAmout;
                        });
                        _viewModel.DbContext.UnprocessedOrders.Add(unprocessedOrderDB);
                    }

                    _viewModel.DbContext.SaveChanges();

                    var loggedCashier = _viewModel.LoggedCashier;
                    var tableId = _viewModel.TableId;
                    var itemsInvoice = _viewModel.ItemsInvoice.ToList();
                    var unprocessedOrder = unprocessedOrderDB;

                    Task.Run(() =>
                    {
                        PrintOrder(loggedCashier,
                            tableId,
                            itemsInvoice,
                            unprocessedOrder);
                    });

                    _viewModel.Reset();

                    AppStateParameter appStateParameter = new AppStateParameter(AppStateEnumerable.TableOverview,
                        _viewModel.LoggedCashier,
                        _viewModel);
                    _viewModel.UpdateAppViewModelCommand.Execute(appStateParameter);
                }
                catch (Exception ex)
                {
                    Log.Error("HookOrderOnTableCommand -> Execute -> Greska prilikom kreiranja porudzbine na vec postojecu: ", ex);
                    MessageBox.Show("Desila se greška prilikom kreiranja porudžbine!\nObratite se serviseru.",
                        "Greška",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }
        private void PrintOrder(CashierDB cashierDB,
            int tableId,
            List<ItemInvoice> items,
            UnprocessedOrderDB unprocessedOrderDB)
        {
            try
            {
                DateTime orderTime = DateTime.Now;

                ClickBar_Common.Models.Order.Order orderKuhinja = new ClickBar_Common.Models.Order.Order()
                {
                    CashierName = cashierDB.Name,
                    TableId = tableId,
                    Items = new List<ClickBar_Common.Models.Order.ItemOrder>(),
                    OrderTime = orderTime,
                    OrderName = "K"
                };
                ClickBar_Common.Models.Order.Order orderSank = new ClickBar_Common.Models.Order.Order()
                {
                    CashierName = cashierDB.Name,
                    TableId = tableId,
                    Items = new List<ClickBar_Common.Models.Order.ItemOrder>(),
                    OrderTime = orderTime,
                    OrderName = "S"
                };
                ClickBar_Common.Models.Order.Order orderDrugo = new ClickBar_Common.Models.Order.Order()
                {
                    CashierName = cashierDB.Name,
                    TableId = tableId,
                    Items = new List<ClickBar_Common.Models.Order.ItemOrder>(),
                    OrderTime = orderTime
                };
                var partHall = _viewModel.DbContext.PartHalls.Join(_viewModel.DbContext.PaymentPlaces,
                    partHall => partHall.Id,
                    table => table.PartHallId,
                    (partHall, table) => new { PartHall = partHall, Table = table })
                    .FirstOrDefault(t => t.Table.Id == tableId);

                if (partHall != null)
                {
                    orderKuhinja.PartHall = partHall.PartHall.Name;
                    orderSank.PartHall = partHall.PartHall.Name;
                    orderDrugo.PartHall = partHall.PartHall.Name;
                }

                items.ForEach(item =>
                {
                    var itemNadgroup = _viewModel.DbContext.Items.Join(_viewModel.DbContext.ItemGroups,
                    item => item.IdItemGroup,
                    itemGroup => itemGroup.Id,
                    (item, itemGroup) => new { Item = item, ItemGroup = itemGroup })
                    .Join(_viewModel.DbContext.Supergroups,
                    group => group.ItemGroup.IdSupergroup,
                    supergroup => supergroup.Id,
                    (group, supergroup) => new { Group = group, Supergroup = supergroup })
                    .FirstOrDefault(it => it.Group.Item.Id == item.Item.Id);

                    if (itemNadgroup != null)
                    {
                        if (itemNadgroup.Supergroup.Name.ToLower().Contains("hrana") ||
                        itemNadgroup.Supergroup.Name.ToLower().Contains("kuhinja"))
                        {
                            if (!string.IsNullOrEmpty(item.GlobalZelja))
                            {
                                orderKuhinja.Items.Add(new ClickBar_Common.Models.Order.ItemOrder()
                                {
                                    Name = item.Item.Name,
                                    Quantity = item.Quantity,
                                    Id = item.Item.Id,
                                    TotalAmount = item.TotalAmout,
                                    Zelja = item.GlobalZelja
                                });
                            }
                            else if (item.Zelje.FirstOrDefault(f => !string.IsNullOrEmpty(f.Description)) != null)
                            {
                                var saZeljama = item.Zelje.Where(z => !string.IsNullOrEmpty(z.Description));

                                if (saZeljama.Any())
                                {
                                    saZeljama.ToList().ForEach(z =>
                                    {
                                        decimal quantity = item.Quantity;
                                        orderKuhinja.Items.Add(new ClickBar_Common.Models.Order.ItemOrder()
                                        {
                                            Name = item.Item.Name,
                                            Quantity = 1,
                                            Id = item.Item.Id,
                                            TotalAmount = item.Item.SellingUnitPrice,
                                            Zelja = z.Description
                                        });
                                    });
                                }

                                decimal quantity = item.Quantity - (saZeljama != null ? saZeljama.Count() : 0);

                                if (quantity > 0)
                                {
                                    orderKuhinja.Items.Add(new ClickBar_Common.Models.Order.ItemOrder()
                                    {
                                        Name = item.Item.Name,
                                        Quantity = quantity,
                                        Id = item.Item.Id,
                                        TotalAmount = decimal.Round(item.Item.SellingUnitPrice * quantity, 2)
                                    });
                                }
                            }
                            else
                            {
                                orderKuhinja.Items.Add(new ClickBar_Common.Models.Order.ItemOrder()
                                {
                                    Name = item.Item.Name,
                                    Quantity = item.Quantity,
                                    Id = item.Item.Id,
                                    TotalAmount = item.TotalAmout
                                });
                            }
                        }
                        else
                        {
                            if (itemNadgroup.Supergroup.Name.ToLower().Contains("pice") ||
                            itemNadgroup.Supergroup.Name.ToLower().Contains("piće") ||
                            itemNadgroup.Supergroup.Name.ToLower().Contains("sank") ||
                            itemNadgroup.Supergroup.Name.ToLower().Contains("šank"))
                            {
                                orderSank.Items.Add(new ClickBar_Common.Models.Order.ItemOrder()
                                {
                                    Name = item.Item.Name,
                                    Quantity = item.Quantity,
                                    Id = item.Item.Id,
                                    TotalAmount = item.TotalAmout
                                });
                            }
                            else
                            {
                                orderDrugo.OrderName = $"{itemNadgroup.Supergroup.Name[0]}";
                                orderDrugo.Items.Add(new ClickBar_Common.Models.Order.ItemOrder()
                                {
                                    Name = item.Item.Name,
                                    Quantity = item.Quantity,
                                    Id = item.Item.Id,
                                    TotalAmount = item.TotalAmout
                                });
                            }
                        }
                    }
                    else
                    {
                        orderSank.Items.Add(new ClickBar_Common.Models.Order.ItemOrder()
                        {
                            Name = item.Item.Name,
                            Quantity = item.Quantity,
                            Id = item.Item.Id,
                            TotalAmount = item.TotalAmout
                        });
                    }
                });

                int orderCounter = 1;

                var ordersTodayDB = _viewModel.DbContext.OrdersToday.Where(o => o.OrderDateTime.Date == DateTime.Now.Date);

                if (ordersTodayDB != null &&
                    ordersTodayDB.Any())
                {
                    orderCounter = ordersTodayDB.Max(o => o.Counter);
                    orderCounter++;
                }

                var posType = SettingsManager.Instance.GetPrinterFormat() == PrinterFormatEnumeration.Pos80mm ?
                ClickBar_Printer.Enums.PosTypeEnumeration.Pos80mm : ClickBar_Printer.Enums.PosTypeEnumeration.Pos58mm;
                if (orderSank.Items.Any())
                {
                    int orderCounterType = 1;

                    var ordersTodayTypeDB = _viewModel.DbContext.OrdersToday.Where(o => o.OrderDateTime.Date == DateTime.Now.Date &&
                    !string.IsNullOrEmpty(o.Name) &&
                    o.Name.ToLower().Contains(orderSank.OrderName.ToLower()));

                    if (ordersTodayTypeDB != null &&
                        ordersTodayTypeDB.Any())
                    {
                        orderCounterType = ordersTodayTypeDB.Max(o => o.CounterType);
                        orderCounterType++;
                    }

                    orderSank.OrderName += orderCounterType.ToString() + "_" + orderCounter.ToString();

                    decimal totalAmount = orderSank.Items.Sum(i => i.TotalAmount);

                    OrderTodayDB orderTodayDB = new OrderTodayDB()
                    {
                        Id = Guid.NewGuid().ToString(),
                        CashierId = cashierDB.Id,
                        Counter = orderCounter,
                        CounterType = orderCounterType,
                        OrderDateTime = DateTime.Now,
                        TotalPrice = totalAmount,
                        Name = orderSank.OrderName,
                        TableId = tableId,
                        OrderTodayItems = new List<OrderTodayItemDB>()
                    };

                    //sqliteDbContext.OrdersToday.Add(orderTodayDB);
                    //sqliteDbContext.SaveChanges();

                    orderSank.Items.ForEach(item =>
                    {
                        var orderTodayItemDB = orderTodayDB.OrderTodayItems.FirstOrDefault(o => o.ItemId == item.Id);

                        if (orderTodayItemDB != null)
                        {
                            orderTodayItemDB.Quantity += item.Quantity;
                            orderTodayItemDB.TotalPrice += item.TotalAmount;
                            //sqliteDbContext.OrderTodayItems.Update(orderTodayItemDB);
                        }
                        else
                        {
                            orderTodayItemDB = new OrderTodayItemDB()
                            {
                                ItemId = item.Id,
                                OrderTodayId = orderTodayDB.Id,
                                Quantity = item.Quantity,
                                TotalPrice = item.TotalAmount,
                            };
                            orderTodayDB.OrderTodayItems.Add(orderTodayItemDB);
                        }
                    });
                    _viewModel.DbContext.OrdersToday.Add(orderTodayDB);
                    _viewModel.DbContext.SaveChanges();

                    FormatPos.PrintOrder(orderSank, posType, ClickBar_Printer.Enums.OrderTypeEnumeration.Sank);
                }
                if (orderKuhinja.Items.Any())
                {
                    int orderCounterType = 1;

                    var ordersTodayTypeDB = _viewModel.DbContext.OrdersToday.Where(o => o.OrderDateTime.Date == DateTime.Now.Date &&
                    !string.IsNullOrEmpty(o.Name) &&
                    o.Name.ToLower().Contains(orderKuhinja.OrderName.ToLower()));

                    if (ordersTodayTypeDB != null &&
                        ordersTodayTypeDB.Any())
                    {
                        orderCounterType = ordersTodayTypeDB.Max(o => o.CounterType);
                        orderCounterType++;
                    }

                    orderKuhinja.OrderName += orderCounterType.ToString() + "_" + orderCounter.ToString();

                    decimal totalAmount = orderKuhinja.Items.Sum(i => i.TotalAmount);

                    OrderTodayDB orderTodayDB = new OrderTodayDB()
                    {
                        Id = Guid.NewGuid().ToString(),
                        CashierId = cashierDB.Id,
                        Counter = orderCounter,
                        CounterType = orderCounterType,
                        OrderDateTime = DateTime.Now,
                        TotalPrice = totalAmount,
                        Name = orderKuhinja.OrderName,
                        TableId = tableId,
                        OrderTodayItems = new List<OrderTodayItemDB>()
                    };

                    //sqliteDbContext.OrdersToday.Add(orderTodayDB);
                    //sqliteDbContext.SaveChanges();

                    orderKuhinja.Items.ForEach(item =>
                    {
                        var orderTodayItemDB = orderTodayDB.OrderTodayItems.FirstOrDefault(o => o.ItemId == item.Id);

                        if (orderTodayItemDB != null)
                        {
                            orderTodayItemDB.Quantity += item.Quantity;
                            orderTodayItemDB.TotalPrice += item.TotalAmount;
                            //sqliteDbContext.OrderTodayItems.Update(orderTodayItemDB);
                        }
                        else
                        {
                            orderTodayItemDB = new OrderTodayItemDB()
                            {
                                ItemId = item.Id,
                                OrderTodayId = orderTodayDB.Id,
                                Quantity = item.Quantity,
                                TotalPrice = item.TotalAmount,
                            };
                            orderTodayDB.OrderTodayItems.Add(orderTodayItemDB);
                        }
                        //sqliteDbContext.SaveChanges();
                    });
                    _viewModel.DbContext.OrdersToday.Add(orderTodayDB);
                    _viewModel.DbContext.SaveChanges();


                    if (!string.IsNullOrEmpty(SettingsManager.Instance.GetPathToDrljaKuhinjaDB()))
                    {
                        var isSucceed = AddDrljaNarudzbina(orderTodayDB,
                            orderKuhinja.Items,
                            cashierDB,
                            unprocessedOrderDB).Result;

                        //if(!isSucceed)
                        //{
                        //    FormatPos.PrintOrder(orderKuhinja, posType, ClickBar_Printer.Enums.OrderTypeEnumeration.Kuhinja);
                        //}
                    }
                    else
                    {
                        FormatPos.PrintOrder(orderKuhinja, posType, ClickBar_Printer.Enums.OrderTypeEnumeration.Kuhinja);
                    }
                }
                if (orderDrugo.Items.Any())
                {
                    int orderCounterType = 1;

                    var ordersTodayTypeDB = _viewModel.DbContext.OrdersToday.Where(o => o.OrderDateTime.Date == DateTime.Now.Date &&
                    !string.IsNullOrEmpty(o.Name) &&
                    o.Name.ToLower().Contains(orderDrugo.OrderName.ToLower()));

                    if (ordersTodayTypeDB != null &&
                        ordersTodayTypeDB.Any())
                    {
                        orderCounterType = ordersTodayTypeDB.Max(o => o.CounterType);
                        orderCounterType++;
                    }

                    orderDrugo.OrderName += orderCounterType.ToString() + "_" + orderCounter.ToString();

                    decimal totalAmount = orderDrugo.Items.Sum(i => i.TotalAmount);

                    OrderTodayDB orderTodayDB = new OrderTodayDB()
                    {
                        Id = Guid.NewGuid().ToString(),
                        CashierId = cashierDB.Id,
                        Counter = orderCounter,
                        CounterType = orderCounterType,
                        OrderDateTime = DateTime.Now,
                        TotalPrice = totalAmount,
                        Name = orderDrugo.OrderName,
                        TableId = tableId,
                        OrderTodayItems = new List<OrderTodayItemDB>()
                    };

                    //sqliteDbContext.OrdersToday.Add(orderTodayDB);
                    //sqliteDbContext.SaveChanges();

                    orderDrugo.Items.ForEach(item =>
                    {
                        var orderTodayItemDB = orderTodayDB.OrderTodayItems.FirstOrDefault(o => o.ItemId == item.Id);

                        if (orderTodayItemDB != null)
                        {
                            orderTodayItemDB.Quantity += item.Quantity;
                            orderTodayItemDB.TotalPrice += item.TotalAmount;
                            //sqliteDbContext.OrderTodayItems.Update(orderTodayItemDB);
                        }
                        else
                        {
                            orderTodayItemDB = new OrderTodayItemDB()
                            {
                                ItemId = item.Id,
                                OrderTodayId = orderTodayDB.Id,
                                Quantity = item.Quantity,
                                TotalPrice = item.TotalAmount,
                            };
                            orderTodayDB.OrderTodayItems.Add(orderTodayItemDB);
                        }
                    });

                    _viewModel.DbContext.OrdersToday.Add(orderTodayDB);
                    _viewModel.DbContext.SaveChanges();

                    FormatPos.PrintOrder(orderDrugo, posType, ClickBar_Printer.Enums.OrderTypeEnumeration.Sank);
                }
            }
            catch (Exception ex)
            {
                Log.Error("HookOrderOnTableCommand -> PrintOrder -> Greska prilikom printanja porudzbine: ", ex);
                MessageBox.Show("Desila se greška prilikom printanja porudžbine!\nObratite se serviseru.",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        private async Task<bool> AddDrljaNarudzbina(OrderTodayDB orderTodayDB,
            List<ClickBar_Common.Models.Order.ItemOrder> items,
            CashierDB cashierDB,
            UnprocessedOrderDB unprocessedOrderDB)
        {
            try
            {
                var paymetPlaceDB = _viewModel.DbContext.PaymentPlaces.Find(orderTodayDB.TableId);

                if (paymetPlaceDB == null)
                {
                    MessageBox.Show("Desila se greška prilikom slanja porudžbine!\nObratite se serviseru.",
                        "Greška",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return false;
                }

                string radnikId = cashierDB.Id;
                string radnikName = cashierDB.Name;

                if (!string.IsNullOrEmpty(paymetPlaceDB.Name) &&
                    paymetPlaceDB.Name.ToLower().Contains("dostava"))
                {
                    radnikId = cashierDB.Id == "1111" ? "3333" : cashierDB.Id == "2222" ? "4444" : cashierDB.Id;
                    radnikName = "Dostava";
                }

                PorudzbinaDrlja porudzbinaDrlja = new PorudzbinaDrlja()
                {
                    Items = new List<PorudzbinaItemDrlja>(),
                    RadnikName = radnikName,
                    RadnikId = radnikId,
                    StoBr = orderTodayDB.TableId.HasValue ? orderTodayDB.TableId.Value.ToString() : "1",
                    InsertInDB = false,
                    PorudzbinaId = unprocessedOrderDB.Id
                };

                items.ForEach(item =>
                {
                    decimal mpc = decimal.Round(item.TotalAmount / item.Quantity, 2);

                    if (paymetPlaceDB.Popust > 0)
                    {
                        mpc = decimal.Round(mpc - (mpc * paymetPlaceDB.Popust / 100), 2);
                    }

                    porudzbinaDrlja.Items.Add(new PorudzbinaItemDrlja()
                    {
                        ItemIdString = item.Id,
                        Kolicina = item.Quantity,
                        Naziv = item.Name,
                        MPC = mpc,
                        Zelje = item.Zelja,
                        RBS = 0,
                        BrojNarudzbe = 0,
                        Jm = "kom",
                    });
                });

                var result = await PostPorudzbinaAsync(porudzbinaDrlja);

                if (result != 200)
                {
                    MessageBox.Show("Desila se greška prilikom slanja porudžbine!\nObratite se serviseru.",
                        "Greška",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("HookOrderOnTableCommand -> AddDrljaNarudzbina -> Greska prilikom slanja porudzbine: ", ex);
                MessageBox.Show("Desila se nepoznata greška prilikom slanja porudžbine!\nObratite se serviseru.",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
        }

        private async Task<int> PostPorudzbinaAsync(PorudzbinaDrlja porudzbina)
        {
            try
            {
                var handler = new HttpClientHandler(); handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
                HttpClient client = new HttpClient(handler);

                var ip = SettingsManager.Instance.GetHOstPC_IP();

                string requestUrl = string.Empty;
                if (string.IsNullOrEmpty(ip))
                {
                    requestUrl = "http://localhost:5000/api/porudzbina/create";
                }
                else
                {
                    requestUrl = $"https://{ip}:44323/api/porudzbina/create";
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
                    Log.Error($"HookOrderOnTableCommand -> PostPorudzbinaAsync -> Status je: {(int)response.StatusCode} -> {response.StatusCode.ToString()} ");
                    return (int)response.StatusCode;
                }
            }
            catch (Exception ex)
            {
                Log.Error("HookOrderOnTableCommand -> PostPorudzbinaAsync -> Greska prilikom slanja porudzbine: ", ex);
                return -1;
            }
        }
    }
}
