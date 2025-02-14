using ClickBar.Models.Sale;
using ClickBar.Models.TableOverview;
using ClickBar.ViewModels;
using ClickBar_Common.Enums;
using ClickBar_Common.Models.Order.Drlja;
using ClickBar_DatabaseSQLManager;
using ClickBar_DatabaseSQLManager.Models;
using ClickBar_Database_Drlja;
using ClickBar_Logging;
using ClickBar_Printer.Enums;
using ClickBar_Printer.Models.DrljaKuhinja;
using ClickBar_Printer.PaperFormat;
using ClickBar_Settings;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms.Design;
using System.Windows.Input;
using System.Windows.Media;

namespace ClickBar.Commands.TableOverview
{
    public class ClickOnPaymentPlaceCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private TableOverviewViewModel _viewModel;

        public ClickOnPaymentPlaceCommand(TableOverviewViewModel viewModel)
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
                if (parameter is Order)
                {
                    Order order = (Order)parameter;

                    if (_viewModel.SaleViewModel.CurrentOrder is null ||
                        _viewModel.SaleViewModel.CurrentOrder.TableId != order.TableId ||
                        _viewModel.SaleViewModel.TableId == 0 ||
                        _viewModel.SaleViewModel.ItemsInvoice == null ||
                        !_viewModel.SaleViewModel.ItemsInvoice.Any())
                    {
                        Log.Debug($"ClickOnPaymentPlaceCommand - Execute - Naplati / pregledaj porudzbinu sa stola {order.TableId}");

                        ChargeOrder(order);
                    }
                    else
                    {
                        Log.Debug($"ClickOnPaymentPlaceCommand - Execute - Zakaci porudzbinu na sto {order.TableId}");

                        var unprocessedOrderDB = _viewModel.DbContext.UnprocessedOrders.FirstOrDefault(table => table.PaymentPlaceId == order.TableId);

                        PaymentPlace? paymentPlace = _viewModel.NormalPaymentPlaces.FirstOrDefault(pp => pp.Order.TableId == order.TableId);

                        if (paymentPlace == null)
                        {
                            paymentPlace = _viewModel.RoundPaymentPlaces.FirstOrDefault(pp => pp.Order.TableId == order.TableId);
                        }

                        if (paymentPlace != null)
                        {
                            if (paymentPlace.Popust > 0 &&
                                paymentPlace.Popust < 100)
                            {
                                var itemWithPopust = _viewModel.SaleViewModel.ItemsInvoice.FirstOrDefault(i => i.Item.IsCheckedZabraniPopust);

                                if (itemWithPopust != null)
                                {
                                    MessageBox.Show("Nije moguće dodati artikal sa zabranom popusta na sto sa popustom!",
                                        "Greška",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Error);

                                    return;
                                }
                            }

                            if (unprocessedOrderDB != null)
                            {
                                Log.Debug($"ClickOnPaymentPlaceCommand - Execute - Kreiranje nove porudzbine na vec postojecu porudzbinu na stolu {order.TableId}!");
                                AddToOldOrder(order, paymentPlace, unprocessedOrderDB);
                            }
                            else
                            {
                                unprocessedOrderDB = new UnprocessedOrderDB()
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    CashierId = _viewModel.SaleViewModel.CurrentOrder.Cashier.Id,
                                    PaymentPlaceId = paymentPlace.Id,
                                    TotalAmount = 0//paymentPlace.Total
                                };
                                Log.Debug($"ClickOnPaymentPlaceCommand - Execute - Kreiranje nove porudzbine na stolu {order.TableId}!");
                                AddNewOrder(order, paymentPlace, unprocessedOrderDB);
                            }

                            Task.Run(() =>
                            {
                                PrintOrder(_viewModel.SaleViewModel.CurrentOrder.Cashier,
                                    order.TableId,
                                    _viewModel.SaleViewModel.CurrentOrder.Items.ToList(),
                                    unprocessedOrderDB);
                            });
                        }
                    }
                    Log.Debug($"ClickOnPaymentPlaceCommand - Execute - Uspesno kreirana porudzbina na stolu {order.TableId}!");
                }
                
            }
            catch (Exception ex)
            {
                Log.Error($"ClickOnPaymentPlaceCommand - Execute - Greska na stolu {parameter.ToString()}: ", ex);
                MessageBox.Show("Desila se greška, pokušajte ponovo.",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        private async void ChargeOrder(Order order)
        {
            try
            {
                PaymentPlace? paymentPlace = _viewModel.NormalPaymentPlaces.FirstOrDefault(pp => pp.Id == order.TableId);

                if (paymentPlace == null)
                {
                    paymentPlace = _viewModel.RoundPaymentPlaces.FirstOrDefault(pp => pp.Id == order.TableId);
                }

                if (paymentPlace != null)
                {
                    if (_viewModel.DrljaDbContextFactory != null &&
                        paymentPlace.Background == Brushes.Blue)
                    {
                        using (var DrljaDbContext = _viewModel.DrljaDbContextFactory.CreateDbContext())
                        {
                            var nrudzbine = DrljaDbContext.Narudzbine.Where(n => n.TR_STO == $"S{paymentPlace.Id}" &&
                        n.TR_FAZA == 2);

                            if (nrudzbine != null &&
                                nrudzbine.Any())
                            {
                                foreach (var n in nrudzbine)
                                {
                                    n.TR_FAZA = 3;
                                    DrljaDbContext.Narudzbine.Update(n);
                                }

                                RetryHelperDrlja.ExecuteWithRetry(() => { DrljaDbContext.SaveChanges(); });
                            }
                        }
                    }

                    _viewModel.SaleViewModel.TableId = order.TableId;

                    if (paymentPlace.Order.Items != null &&
                        paymentPlace.Order.Items.Any())
                    {
                        //_viewModel.SaleViewModel.ItemsInvoice = new ObservableCollection<ItemInvoice>(paymentPlace.Order.Items);
                        _viewModel.SaleViewModel.OldItemsInvoice = new ObservableCollection<ItemInvoice>(paymentPlace.Order.Items);
                        _viewModel.SaleViewModel.TotalAmount = paymentPlace.Total;
                        _viewModel.SaleViewModel.CurrentOrder = new Order(paymentPlace.Id, paymentPlace.PartHallId)
                        {
                            Items = new ObservableCollection<ItemInvoice>(paymentPlace.Order.Items),
                            Cashier = order.Cashier
                        };
                        //paymentPlace.Background = Brushes.Green;
                        //paymentPlace.Order = null;
                        //paymentPlace.Total = 0;
                    }
                    else
                    {
                        _viewModel.SaleViewModel.OldItemsInvoice = new ObservableCollection<ItemInvoice>();
                        _viewModel.SaleViewModel.ItemsInvoice = new ObservableCollection<ItemInvoice>();
                        _viewModel.SaleViewModel.TotalAmount = 0;

                        _viewModel.SaleViewModel.CurrentOrder = new Order(paymentPlace.Id, paymentPlace.PartHallId)
                        {
                            Items = new ObservableCollection<ItemInvoice>(),
                            Cashier = _viewModel.SaleViewModel.LoggedCashier
                        };
                    }
                    _viewModel.CancelCommand.Execute(null);
                }
            }
            catch (Exception ex)
            {
                Log.Error("ClickOnPaymentPlaceCommand -> ChargeOrder -> Greska prilikom naplate: ", ex);
            }
        }
        private void AddToOldOrder(Order order,
            PaymentPlace paymentPlace,
            UnprocessedOrderDB unprocessedOrderDB)
        {
            if (_viewModel.SaleViewModel.CurrentOrder.Items is not null)
            {
                if (paymentPlace.Background == Brushes.Green)
                {
                    paymentPlace.Order.Items = new ObservableCollection<ItemInvoice>();
                    paymentPlace.Order.Cashier = _viewModel.SaleViewModel.LoggedCashier;
                    paymentPlace.Background = Brushes.Red;
                }

                _viewModel.SaleViewModel.ItemsInvoice.ToList().ForEach(item =>
                {
                    var itemInUnprocessedOrderDB = _viewModel.DbContext.ItemsInUnprocessedOrder.FirstOrDefault(i => i.ItemId == item.Item.Id &&
                    i.UnprocessedOrderId == unprocessedOrderDB.Id);

                    if (itemInUnprocessedOrderDB == null)
                    {
                        itemInUnprocessedOrderDB = new ItemInUnprocessedOrderDB()
                        {
                            ItemId = item.Item.Id,
                            UnprocessedOrderId = unprocessedOrderDB.Id,
                            Quantity = item.Quantity
                        };
                        _viewModel.DbContext.ItemsInUnprocessedOrder.Add(itemInUnprocessedOrderDB);
                    }
                    else
                    {
                        itemInUnprocessedOrderDB.Quantity += item.Quantity;
                        _viewModel.DbContext.ItemsInUnprocessedOrder.Update(itemInUnprocessedOrderDB);
                    }
                    unprocessedOrderDB.TotalAmount += item.TotalAmout;
                    unprocessedOrderDB.CashierId = _viewModel.SaleViewModel.LoggedCashier.Id;
                    _viewModel.DbContext.UnprocessedOrders.Update(unprocessedOrderDB);
                    //sqliteDbContext.SaveChanges();

                    if (paymentPlace.Order.Items != null &&
                        paymentPlace.Order.Items.Any())
                    {
                        var i = paymentPlace.Order.Items.FirstOrDefault(it => it.Item.Id == item.Item.Id);

                        if (i != null)
                        {
                            i.TotalAmout += item.TotalAmout;
                            i.Quantity += item.Quantity;
                        }
                        else
                        {
                            paymentPlace.Order.Items.Add(item);
                        }
                    }
                    else
                    {
                        if (paymentPlace.Order.Items == null)
                        {
                            paymentPlace.Order.Items = new ObservableCollection<ItemInvoice>();
                        }

                        paymentPlace.Order.Items.Add(item);
                    }

                    paymentPlace.Total += item.TotalAmout;
                    //sqliteDbContext.SaveChanges();
                });

                _viewModel.DbContext.SaveChanges();

#if CRNO
                _viewModel.SaleViewModel.LogoutCommand.Execute(true);
#else
                if (SettingsManager.Instance.EnableSmartCard())
                {
                    _viewModel.SaleViewModel.LogoutCommand.Execute(true);
                }
                else
                {
                    _viewModel.CancelCommand.Execute(null);
                    _viewModel.SaleViewModel.Reset();
                }
#endif

            }
            
        }
        private void AddNewOrder(Order order, 
            PaymentPlace paymentPlace,
            UnprocessedOrderDB unprocessedOrderDB)
        {
            if (_viewModel.SaleViewModel.CurrentOrder.Items is not null)
            {
                if (paymentPlace.Background == Brushes.Green)
                {
                    paymentPlace.Order.Items = new ObservableCollection<ItemInvoice>();
                    paymentPlace.Order.Cashier = _viewModel.SaleViewModel.LoggedCashier;
                    paymentPlace.Background = Brushes.Red;
                }

                _viewModel.DbContext.UnprocessedOrders.Add(unprocessedOrderDB);
                _viewModel.DbContext.SaveChanges();

                _viewModel.SaleViewModel.CurrentOrder.Items.ToList().ForEach(item =>
                {
                    ItemInUnprocessedOrderDB itemInUnprocessedOrderDB = new ItemInUnprocessedOrderDB()
                    {
                        ItemId = item.Item.Id,
                        UnprocessedOrderId = unprocessedOrderDB.Id,
                        Quantity = item.Quantity
                    };
                    _viewModel.DbContext.ItemsInUnprocessedOrder.Add(itemInUnprocessedOrderDB);

                    if (paymentPlace.Order.Items != null &&
                        paymentPlace.Order.Items.Any())
                    {
                        var i = paymentPlace.Order.Items.Where(it => it.Item.Id == item.Item.Id).ToList().FirstOrDefault();

                        if (i != null)
                        {
                            i.TotalAmout += item.TotalAmout;
                            i.Quantity += item.Quantity;
                        }
                        else
                        {
                            paymentPlace.Order.Items.Add(item);
                        }
                    }
                    else
                    {
                        if (paymentPlace.Order.Items == null)
                        {
                            paymentPlace.Order.Items = new ObservableCollection<ItemInvoice>();
                        }

                        paymentPlace.Order.Items.Add(item);
                    }

                    paymentPlace.Total += item.TotalAmout;
                    //sqliteDbContext.SaveChanges();
                });

                unprocessedOrderDB.TotalAmount = paymentPlace.Total;
                _viewModel.DbContext.UnprocessedOrders.Update(unprocessedOrderDB);
                _viewModel.DbContext.SaveChanges();

#if CRNO
                _viewModel.SaleViewModel.LogoutCommand.Execute(true);
#else
                if (SettingsManager.Instance.EnableSmartCard())
                {
                    _viewModel.SaleViewModel.LogoutCommand.Execute(true);
                }
                else
                {
                    _viewModel.CancelCommand.Execute(null);
                    _viewModel.SaleViewModel.Reset();
                }
#endif

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
                        OrderTodayItemDB orderTodayItemDB = new OrderTodayItemDB()
                        {
                            ItemId = item.Id,
                            OrderTodayId = orderTodayDB.Id,
                            Quantity = item.Quantity,
                            TotalPrice = item.TotalAmount,
                        };
                        orderTodayDB.OrderTodayItems.Add(orderTodayItemDB);
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
                        var orderTodayItemDB = _viewModel.DbContext.OrderTodayItems.FirstOrDefault(o => o.ItemId == item.Id &&
                        o.OrderTodayId == orderTodayDB.Id);

                        if (orderTodayItemDB != null)
                        {
                            orderTodayItemDB.Quantity += item.Quantity;
                            orderTodayItemDB.TotalPrice += item.TotalAmount;
                            _viewModel.DbContext.OrderTodayItems.Update(orderTodayItemDB);
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
                        OrderTodayItemDB orderTodayItemDB = new OrderTodayItemDB()
                        {
                            ItemId = item.Id,
                            OrderTodayId = orderTodayDB.Id,
                            Quantity = item.Quantity,
                            TotalPrice = item.TotalAmount,
                        };
                        orderTodayDB.OrderTodayItems.Add(orderTodayItemDB);
                    });

                    _viewModel.DbContext.OrdersToday.Add(orderTodayDB);
                    _viewModel.DbContext.SaveChanges();

                    FormatPos.PrintOrder(orderDrugo, posType, ClickBar_Printer.Enums.OrderTypeEnumeration.Sank);
                }

            }
            catch (Exception ex)
            {
                Log.Error("ClickOnPaymentPlaceCommand -> PrintOrder -> Greska prilikom printanja porudzbine: ", ex);
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
                    if (paymetPlaceDB.Name.ToLower().Contains("1"))
                    {
                        radnikId = "3333";
                        radnikName = "Dostava 1";
                    }
                    else
                    {
                        radnikId = "4444";
                        radnikName = "Dostava 2";
                    }
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
                Log.Error("ClickOnPaymentPlaceCommand -> AddDrljaNarudzbina -> Greska prilikom slanja porudzbine: ", ex);
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
                if(string.IsNullOrEmpty(ip))
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
                    Log.Error($"ClickOnPaymentPlaceCommand -> PostPorudzbinaAsync -> Status je: {(int)response.StatusCode} -> {response.StatusCode.ToString()} ");
                    return (int)response.StatusCode;
                }
            }
            catch (Exception ex)
            {
                Log.Error("ClickOnPaymentPlaceCommand -> PostPorudzbinaAsync -> Greska prilikom slanja porudzbine: ", ex);
                return -1;
            }
        }
    }
}
