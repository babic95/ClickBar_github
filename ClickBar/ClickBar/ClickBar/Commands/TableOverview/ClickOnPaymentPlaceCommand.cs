using ClickBar.Enums.Kuhinja;
using ClickBar.Models.Sale;
using ClickBar.Models.TableOverview;
using ClickBar.ViewModels;
using ClickBar_Common.Enums;
using ClickBar_Common.Models.Order.Drlja;
using ClickBar_DatabaseSQLManager;
using ClickBar_DatabaseSQLManager.Models;
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

                        using (var dbContext = _viewModel.DbContextFactory.CreateDbContext())
                        {
                            var unprocessedOrderDB = dbContext.UnprocessedOrders.FirstOrDefault(table => table.PaymentPlaceId == order.TableId);

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
                                        //TotalAmount = _viewModel.SaleViewModel.ItemsInvoice.Sum(i => i.TotalAmout)//paymentPlace.Total
                                    };
                                    Log.Debug($"ClickOnPaymentPlaceCommand - Execute - Kreiranje nove porudzbine na stolu {order.TableId}!");
                                    AddNewOrder(order, paymentPlace, unprocessedOrderDB);
                                }

                                Task.Run(() =>
                                {
                                    PrintOrder(_viewModel.SaleViewModel.CurrentOrder.Cashier,
                                        paymentPlace,
                                        _viewModel.SaleViewModel.ItemsInvoice.ToList(),
                                        unprocessedOrderDB);
                                });
                            }
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
                    _viewModel.SaleViewModel.TableId = order.TableId;

                    using (var dbContext = _viewModel.DbContextFactory.CreateDbContext())
                    {
                        var unprocessedOrderDB = dbContext.UnprocessedOrders.Include(u => u.Cashier)
                            .FirstOrDefault(table => table.PaymentPlaceId == order.TableId);

                        if (unprocessedOrderDB != null)
                        {
                            //if (unprocessedOrderDB.TotalAmount >= 0)
                            //{
                                var ordersToday = dbContext.OrdersToday.Include(o => o.OrderTodayItems)
                                    .Where(ord => ord.UnprocessedOrderId == unprocessedOrderDB.Id &&
                                    ord.OrderTodayItems.Any() &&
                                    ord.Faza != (int)FazaKuhinjeEnumeration.Naplacena &&
                                    ord.Faza != (int)FazaKuhinjeEnumeration.Obrisana);

                            if (ordersToday != null && ordersToday.Any())
                            {
                                var someoneElsePayment = ordersToday.Any(o => o.CashierId != _viewModel.SaleViewModel.LoggedCashier.Id);

                                if (SettingsManager.Instance.GetDisableSomeoneElsePayment() &&
                                    someoneElsePayment)
                                {
                                    _viewModel.SaleViewModel.TableId = 0;
                                   MessageBox.Show("Nije moguće naplatiti sto od drugog konobara!",
                                        "Greška",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                                    return;
                                }

                                List<OldOrder> oldOrders = new List<OldOrder>();
                                foreach (var orderToday in ordersToday)
                                {
                                    var items = new ObservableCollection<ItemInvoice>(orderToday.OrderTodayItems
                                        .Where(o => o.Quantity - o.StornoQuantity - o.NaplacenoQuantity > 0).Select(oti => new ItemInvoice(
                                            new Item(dbContext.Items.FirstOrDefault(i => i.Id == oti.ItemId)),
                                            oti.Quantity - oti.StornoQuantity - oti.NaplacenoQuantity, paymentPlace.Popust)
                                    ));

                                    if (items.Any())
                                    {
                                        OldOrder oldOrder = new OldOrder(orderToday.OrderDateTime,
                                            unprocessedOrderDB.Cashier.Name,
                                            unprocessedOrderDB.Cashier.Id,
                                            orderToday.Name,
                                            items);

                                        oldOrders.Add(oldOrder);
                                    }
                                }

                                _viewModel.SaleViewModel.OldOrders = oldOrders.Any() ? new ObservableCollection<OldOrder>(oldOrders.OrderBy(o => o.OrderDateTime)) :
                                    new ObservableCollection<OldOrder>();

                                decimal total = ordersToday.Sum(o => o.OrderTodayItems.Sum(oti => Decimal.Round((oti.Quantity - oti.StornoQuantity - oti.NaplacenoQuantity) * (oti.TotalPrice / oti.Quantity), 2)));
                                _viewModel.SaleViewModel.TotalAmount = total;
                                _viewModel.SaleViewModel.CurrentOrder = new Order(paymentPlace.Id, paymentPlace.PartHallId)
                                {
                                    Cashier = order.Cashier
                                };
                                //paymentPlace.Background = Brushes.Green;
                                //paymentPlace.Order = null;
                                //paymentPlace.Total = 0;
                            }
                            //}
                            //else
                            //{
                            //    _viewModel.SaleViewModel.OldOrders = new ObservableCollection<OldOrder>();
                            //    _viewModel.SaleViewModel.ItemsInvoice = new ObservableCollection<ItemInvoice>();
                            //    _viewModel.SaleViewModel.TotalAmount = 0;

                            //    _viewModel.SaleViewModel.CurrentOrder = new Order(paymentPlace.Id, paymentPlace.PartHallId)
                            //    {
                            //        Cashier = _viewModel.SaleViewModel.LoggedCashier
                            //    };
                            //}
                        }

                        if (paymentPlace.Background == Brushes.Blue)
                        {
                            var ordersZavrsene = dbContext.OrdersToday.Where(o => o.Faza == (int)FazaKuhinjeEnumeration.Uradjena &&
                            o.UnprocessedOrderId == unprocessedOrderDB.Id);

                            if (ordersZavrsene != null &&
                                ordersZavrsene.Any())
                            {
                                foreach (var zavrseno in ordersZavrsene)
                                {
                                    zavrseno.Faza = (int)FazaKuhinjeEnumeration.Isporucena;
                                    dbContext.OrdersToday.Update(zavrseno);
                                }

                                dbContext.SaveChanges();
                            }
                        }
                        _viewModel.CancelCommand.Execute(null);
                    }
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
            if (paymentPlace.Background == Brushes.Green)
            {
                paymentPlace.Order.Cashier = _viewModel.SaleViewModel.LoggedCashier;
                paymentPlace.Background = Brushes.Red;
            }

            using (var dbContext = _viewModel.DbContextFactory.CreateDbContext())
            {
                //unprocessedOrderDB.TotalAmount += _viewModel.SaleViewModel.ItemsInvoice.Sum(i => i.TotalAmout);
                unprocessedOrderDB.CashierId = _viewModel.SaleViewModel.LoggedCashier.Id;
                dbContext.UnprocessedOrders.Update(unprocessedOrderDB);

                dbContext.SaveChanges();

                //paymentPlace.Total = unprocessedOrderDB.TotalAmount;
            }

//#if CRNO
//                _viewModel.SaleViewModel.LogoutCommand.Execute(true);
//#else
            if (SettingsManager.Instance.EnableSmartCard())
            {
                _viewModel.SaleViewModel.LogoutCommand.Execute(true);
            }
            else
            {
                _viewModel.CancelCommand.Execute(null);
                _viewModel.SaleViewModel.Reset();
            }
//#endif

        }
        private void AddNewOrder(Order order,
            PaymentPlace paymentPlace,
            UnprocessedOrderDB unprocessedOrderDB)
        {
            if (paymentPlace.Background == Brushes.Green)
            {
                paymentPlace.Order.Cashier = _viewModel.SaleViewModel.LoggedCashier;
                paymentPlace.Background = Brushes.Red;
            }
            using (var dbContext = _viewModel.DbContextFactory.CreateDbContext())
            {
                dbContext.UnprocessedOrders.Add(unprocessedOrderDB);
                dbContext.SaveChanges();

//#if CRNO
//                _viewModel.SaleViewModel.LogoutCommand.Execute(true);
//#else
                if (SettingsManager.Instance.EnableSmartCard())
                {
                    _viewModel.SaleViewModel.LogoutCommand.Execute(true);
                }
                else
                {
                    _viewModel.CancelCommand.Execute(null);
                    _viewModel.SaleViewModel.Reset();
                }
//#endif
            }
        }

        private void PrintOrder(CashierDB cashierDB,
           PaymentPlace paymentPlace,
           List<ItemInvoice> items,
           UnprocessedOrderDB unprocessedOrderDB)
        {
            try
            {
                DateTime orderTime = DateTime.Now;

                ClickBar_Common.Models.Order.Order orderKuhinja = new ClickBar_Common.Models.Order.Order()
                {
                    CashierName = cashierDB.Name,
                    TableId = paymentPlace.Id,
                    Items = new List<ClickBar_Common.Models.Order.ItemOrder>(),
                    OrderTime = orderTime,
                    OrderName = "K"
                };
                ClickBar_Common.Models.Order.Order orderSank = new ClickBar_Common.Models.Order.Order()
                {
                    CashierName = cashierDB.Name,
                    TableId = paymentPlace.Id,
                    Items = new List<ClickBar_Common.Models.Order.ItemOrder>(),
                    OrderTime = orderTime,
                    OrderName = "S"
                };
                ClickBar_Common.Models.Order.Order orderDrugo = new ClickBar_Common.Models.Order.Order()
                {
                    CashierName = cashierDB.Name,
                    TableId = paymentPlace.Id,
                    Items = new List<ClickBar_Common.Models.Order.ItemOrder>(),
                    OrderTime = orderTime
                };

                using (var dbContext = _viewModel.DbContextFactory.CreateDbContext())
                {
                    var partHall = dbContext.PartHalls.Join(dbContext.PaymentPlaces,
                    partHall => partHall.Id,
                    table => table.PartHallId,
                    (partHall, table) => new { PartHall = partHall, Table = table })
                    .FirstOrDefault(t => t.Table.Id == paymentPlace.Id);

                    if (partHall != null)
                    {
                        orderKuhinja.PartHall = partHall.PartHall.Name;
                        orderSank.PartHall = partHall.PartHall.Name;
                        orderDrugo.PartHall = partHall.PartHall.Name;
                    }

                    foreach (var item in items)
                    {
                        var itemNadgroup = dbContext.Items.Join(dbContext.ItemGroups,
                        item => item.IdItemGroup,
                        itemGroup => itemGroup.Id,
                        (item, itemGroup) => new { Item = item, ItemGroup = itemGroup })
                        .Join(dbContext.Supergroups,
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
                    }

                    int orderCounter = 1;

                    var ordersTodayDB = dbContext.OrdersToday.Where(o => o.OrderDateTime.Date == DateTime.Now.Date);

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

                        var ordersTodayTypeDB = dbContext.OrdersToday.Where(o => o.OrderDateTime.Date == DateTime.Now.Date &&
                        !string.IsNullOrEmpty(o.Name) &&
                        o.Name.ToLower().Contains(orderSank.OrderName.ToLower()));

                        if (ordersTodayTypeDB != null &&
                            ordersTodayTypeDB.Any())
                        {
                            orderCounterType = ordersTodayTypeDB.Max(o => o.CounterType);
                            orderCounterType++;
                        }

                        orderSank.OrderName += orderCounterType.ToString() + "__" + orderCounter.ToString();

                        decimal totalAmount = orderSank.Items.Sum(i => i.TotalAmount);

                        OrderTodayDB orderTodayDB = new OrderTodayDB()
                        {
                            Id = Guid.NewGuid().ToString(),
                            UnprocessedOrderId = unprocessedOrderDB.Id,
                            CashierId = !string.IsNullOrEmpty(paymentPlace.Name) && paymentPlace.Name.ToLower().Contains("dostava 1") ? "3333" :
                            !string.IsNullOrEmpty(paymentPlace.Name) && paymentPlace.Name.ToLower().Contains("dostava 2") ? "4444" : cashierDB.Id,
                            Counter = orderCounter,
                            CounterType = orderCounterType,
                            OrderDateTime = DateTime.Now,
                            TotalPrice = totalAmount,
                            Name = orderSank.OrderName,
                            TableId = paymentPlace.Id,
                            OrderTodayItems = new List<OrderTodayItemDB>(),
                            Faza = (int)FazaKuhinjeEnumeration.Nova
                        };

                        //sqliteDbContext.OrdersToday.Add(orderTodayDB);
                        //sqliteDbContext.SaveChanges();

                        orderSank.Items.ForEach(item =>
                        {
                            OrderTodayItemDB orderTodayItemDB = new OrderTodayItemDB()
                            {
                                Id = Guid.NewGuid().ToString(),
                                ItemId = item.Id,
                                OrderTodayId = orderTodayDB.Id,
                                Quantity = item.Quantity,
                                TotalPrice = item.TotalAmount,
                            };
                            orderTodayDB.OrderTodayItems.Add(orderTodayItemDB);
                        });
                        dbContext.OrdersToday.Add(orderTodayDB);
                        dbContext.SaveChanges();

                        FormatPos.PrintOrder(orderSank, posType, ClickBar_Printer.Enums.OrderTypeEnumeration.Sank);
                    }
                    if (orderKuhinja.Items.Any())
                    {
                        int orderCounterType = 1;

                        var ordersTodayTypeDB = dbContext.OrdersToday.Where(o => o.OrderDateTime.Date == DateTime.Now.Date &&
                        !string.IsNullOrEmpty(o.Name) &&
                        o.Name.ToLower().Contains(orderKuhinja.OrderName.ToLower()));

                        if (ordersTodayTypeDB != null &&
                            ordersTodayTypeDB.Any())
                        {
                            orderCounterType = ordersTodayTypeDB.Max(o => o.CounterType);
                            orderCounterType++;
                        }

                        orderKuhinja.OrderName += orderCounterType.ToString() + "__" + orderCounter.ToString();

                        decimal totalAmount = orderKuhinja.Items.Sum(i => i.TotalAmount);

                        OrderTodayDB orderTodayDB = new OrderTodayDB()
                        {
                            Id = Guid.NewGuid().ToString(),
                            UnprocessedOrderId = unprocessedOrderDB.Id,
                            CashierId = !string.IsNullOrEmpty(paymentPlace.Name) && paymentPlace.Name.ToLower().Contains("dostava 1") ? "3333" :
                            !string.IsNullOrEmpty(paymentPlace.Name) && paymentPlace.Name.ToLower().Contains("dostava 2") ? "4444" : cashierDB.Id,
                            Counter = orderCounter,
                            CounterType = orderCounterType,
                            OrderDateTime = DateTime.Now,
                            TotalPrice = totalAmount,
                            Name = orderKuhinja.OrderName,
                            TableId = paymentPlace.Id,
                            OrderTodayItems = new List<OrderTodayItemDB>(),
                            Faza = (int)FazaKuhinjeEnumeration.Nova
                        };

                        //sqliteDbContext.OrdersToday.Add(orderTodayDB);
                        //sqliteDbContext.SaveChanges();

                        orderKuhinja.Items.ForEach(item =>
                        {
                            var orderTodayItemDB = dbContext.OrderTodayItems.FirstOrDefault(o => o.ItemId == item.Id &&
                            o.OrderTodayId == orderTodayDB.Id);

                            if (orderTodayItemDB != null)
                            {
                                orderTodayItemDB.Quantity += item.Quantity;
                                orderTodayItemDB.TotalPrice += item.TotalAmount;
                                dbContext.OrderTodayItems.Update(orderTodayItemDB);
                            }
                            else
                            {
                                orderTodayItemDB = new OrderTodayItemDB()
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    ItemId = item.Id,
                                    OrderTodayId = orderTodayDB.Id,
                                    Quantity = item.Quantity,
                                    TotalPrice = item.TotalAmount,
                                };
                                orderTodayDB.OrderTodayItems.Add(orderTodayItemDB);
                            }
                            //sqliteDbContext.SaveChanges();
                        });
                        dbContext.OrdersToday.Add(orderTodayDB);
                        dbContext.SaveChanges();

                        if (string.IsNullOrEmpty(SettingsManager.Instance.GetPathToDrljaKuhinjaDB()))
                        {
                            FormatPos.PrintOrder(orderKuhinja, posType, ClickBar_Printer.Enums.OrderTypeEnumeration.Kuhinja);
                        }
                    }
                    if (orderDrugo.Items.Any())
                    {
                        int orderCounterType = 1;

                        var ordersTodayTypeDB = dbContext.OrdersToday.Where(o => o.OrderDateTime.Date == DateTime.Now.Date &&
                        !string.IsNullOrEmpty(o.Name) &&
                        o.Name.ToLower().Contains(orderDrugo.OrderName.ToLower()));

                        if (ordersTodayTypeDB != null &&
                            ordersTodayTypeDB.Any())
                        {
                            orderCounterType = ordersTodayTypeDB.Max(o => o.CounterType);
                            orderCounterType++;
                        }

                        orderDrugo.OrderName += orderCounterType.ToString() + "__" + orderCounter.ToString();

                        decimal totalAmount = orderDrugo.Items.Sum(i => i.TotalAmount);

                        OrderTodayDB orderTodayDB = new OrderTodayDB()
                        {
                            Id = Guid.NewGuid().ToString(),
                            UnprocessedOrderId = unprocessedOrderDB.Id,
                            CashierId = !string.IsNullOrEmpty(paymentPlace.Name) && paymentPlace.Name.ToLower().Contains("dostava 1") ? "3333" :
                            !string.IsNullOrEmpty(paymentPlace.Name) && paymentPlace.Name.ToLower().Contains("dostava 2") ? "4444" : cashierDB.Id,
                            Counter = orderCounter,
                            CounterType = orderCounterType,
                            OrderDateTime = DateTime.Now,
                            TotalPrice = totalAmount,
                            Name = orderDrugo.OrderName,
                            TableId = paymentPlace.Id,
                            OrderTodayItems = new List<OrderTodayItemDB>(),
                            Faza = (int)FazaKuhinjeEnumeration.Nova
                        };

                        //sqliteDbContext.OrdersToday.Add(orderTodayDB);
                        //sqliteDbContext.SaveChanges();

                        orderDrugo.Items.ForEach(item =>
                        {
                            OrderTodayItemDB orderTodayItemDB = new OrderTodayItemDB()
                            {
                                Id = Guid.NewGuid().ToString(),
                                ItemId = item.Id,
                                OrderTodayId = orderTodayDB.Id,
                                Quantity = item.Quantity,
                                TotalPrice = item.TotalAmount,
                            };
                            orderTodayDB.OrderTodayItems.Add(orderTodayItemDB);
                        });

                        dbContext.OrdersToday.Add(orderTodayDB);
                        dbContext.SaveChanges();

                        FormatPos.PrintOrder(orderDrugo, posType, ClickBar_Printer.Enums.OrderTypeEnumeration.Sank);
                    }
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
    }
}
