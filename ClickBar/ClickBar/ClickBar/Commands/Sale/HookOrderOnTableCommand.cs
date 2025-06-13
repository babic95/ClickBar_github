using ClickBar.Enums;
using ClickBar.Enums.Kuhinja;
using ClickBar.Models.Sale;
using ClickBar.Models.TableOverview;
using ClickBar.ViewModels;
using ClickBar_Common.Enums;
using ClickBar_Common.Models.Invoice;
using ClickBar_DatabaseSQLManager;
using ClickBar_DatabaseSQLManager.Models;
using ClickBar_Logging;
using ClickBar_Printer.Enums;
using ClickBar_Printer.PaperFormat;
using ClickBar_Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
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
        private static bool _isPrinted = false;

        public HookOrderOnTableCommand(SaleViewModel viewModel, IServiceProvider serviceProvider)
        {
            _viewModel = viewModel;
            _serviceProvider = serviceProvider;
        }

        public bool CanExecute(object? parameter)
        {
            return !_isPrinted;
        }
        public async void Execute(object parameter)
        {
            var datetime = DateTime.Now;
            _isPrinted = true;
            RaiseCanExecuteChanged(); // Obavesti UI da se stanje promenilo

            try
            {
                if (_viewModel.TableId == 0)
                {
                    //var dbContextFactory = _serviceProvider.GetRequiredService<IDbContextFactory<SqlServerDbContext>>();
                    //_viewModel.TableOverviewViewModel = _serviceProvider.GetRequiredService<TableOverviewViewModel>();

                    _viewModel.CurrentOrder = new Order(_viewModel.LoggedCashier)
                    {
                        TableId = _viewModel.TableId,
                    };

                    var typeApp = SettingsManager.Instance.GetTypeApp();

                    var appStateParameter = new AppStateParameter(
                        typeApp == TypeAppEnumeration.Sale ? AppStateEnumerable.Sale : AppStateEnumerable.TableOverview,
                        _viewModel.LoggedCashier,
                        -1,
                        _viewModel);

                    _viewModel.UpdateAppViewModelCommand.Execute(appStateParameter);
                }
                else
                {
                    try
                    {
                        using (var context = _viewModel.DbContextFactory.CreateDbContext())
                        {
                            var tableDB = await context.PaymentPlaces.FindAsync(_viewModel.TableId);

                            if (tableDB != null && tableDB.Popust > 0 && tableDB.Popust < 100)
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

                            var unprocessedOrderDB = await context.UnprocessedOrders
                                .FirstOrDefaultAsync(table => table.PaymentPlaceId == _viewModel.TableId);

                            if (unprocessedOrderDB != null)
                            {
                                //decimal totalSum = _viewModel.ItemsInvoice.Sum(i => i.TotalAmout);

                                //unprocessedOrderDB.TotalAmount += Decimal.Round(totalSum * ((100 - tableDB.Popust) / 100), 2);

                                unprocessedOrderDB.CashierId = _viewModel.LoggedCashier.Id;
                                context.UnprocessedOrders.Update(unprocessedOrderDB);
                            }
                            else
                            {
                                unprocessedOrderDB = new UnprocessedOrderDB()
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    CashierId = _viewModel.LoggedCashier.Id,
                                    PaymentPlaceId = _viewModel.TableId,
                                    //TotalAmount = _viewModel.ItemsInvoice.Sum(i => i.TotalAmout),
                                    ItemsInUnprocessedOrder = new List<ItemInUnprocessedOrderDB>(),
                                };

                                await context.UnprocessedOrders.AddAsync(unprocessedOrderDB);
                            }

                            bool saveFailed;
                            int retryCount = 0;
                            do
                            {
                                saveFailed = false;
                                try
                                {
                                    await context.SaveChangesAsync();
                                }
                                catch (System.Data.Entity.Infrastructure.DbUpdateConcurrencyException ex)
                                {
                                    saveFailed = true;
                                    retryCount++;

                                    // Update the values of the entity that failed to save from the store
                                    ex.Entries.Single().Reload();

                                    if (retryCount >= 3)
                                    {
                                        throw;
                                    }
                                }
                            } while (saveFailed);

                            var loggedCashier = _viewModel.LoggedCashier;
                            var tableId = _viewModel.TableId;
                            var paymentPlace = await context.PaymentPlaces.Include(p => p.PartHall).FirstOrDefaultAsync(p => p.Id == tableId);
                            var itemsInvoice = _viewModel.ItemsInvoice.ToList();
                            var unprocessedOrder = unprocessedOrderDB;

                            await PrintOrder(loggedCashier, paymentPlace, itemsInvoice, unprocessedOrder, context);

                            _viewModel.Reset();

                            var appStateParameter = new AppStateParameter(
                                SettingsManager.Instance.GetTypeApp() == TypeAppEnumeration.Sale ? AppStateEnumerable.Sale : AppStateEnumerable.TableOverview,
                                _viewModel.LoggedCashier,
                                tableId,
                                _viewModel);

                            _viewModel.UpdateAppViewModelCommand.Execute(appStateParameter);
                        }
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
            catch (Exception ex)
            {
                Log.Error("HookOrderOnTableCommand -> Execute -> dogodila se greska prilikom kacenja porudzbine: ", ex);
                MessageBox.Show("Desila se greška prilikom kreiranja porudžbine!\nObratite se serviseru.",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                _isPrinted = false;
                RaiseCanExecuteChanged(); // Obavesti UI da se stanje promenilo
            }


            Log.Debug($"PROVERA -> VREME IZVRSENJA: {DateTime.Now.Subtract(datetime).TotalMilliseconds} ms");
        }
        private async Task PrintOrder(CashierDB cashierDB,
            PaymentPlaceDB paymentPlaceDB,
            List<ItemInvoice> items,
            UnprocessedOrderDB unprocessedOrderDB,
            SqlServerDbContext context)
        {
            try
            {
                DateTime orderTime = DateTime.Now;

                ClickBar_Common.Models.Order.Order orderKuhinja = new ClickBar_Common.Models.Order.Order()
                {
                    CashierName = cashierDB.Name,
                    TableId = paymentPlaceDB.Id,
                    Items = new List<ClickBar_Common.Models.Order.ItemOrder>(),
                    OrderTime = orderTime,
                    OrderName = "K"
                };
                ClickBar_Common.Models.Order.Order orderSank = new ClickBar_Common.Models.Order.Order()
                {
                    CashierName = cashierDB.Name,
                    TableId = paymentPlaceDB.Id,
                    Items = new List<ClickBar_Common.Models.Order.ItemOrder>(),
                    OrderTime = orderTime,
                    OrderName = "S"
                };
                ClickBar_Common.Models.Order.Order orderDrugo = new ClickBar_Common.Models.Order.Order()
                {
                    CashierName = cashierDB.Name,
                    TableId = paymentPlaceDB.Id,
                    Items = new List<ClickBar_Common.Models.Order.ItemOrder>(),
                    OrderTime = orderTime
                };

                orderKuhinja.PartHall = paymentPlaceDB.PartHall.Name;
                orderSank.PartHall = paymentPlaceDB.PartHall.Name;
                orderDrugo.PartHall = paymentPlaceDB.PartHall.Name;

                var allItemsDB = await context.Items.Include(i => i.ItemGroupNavigation)
                    .ThenInclude(ig => ig.IdSupergroupNavigation).ToListAsync();

                foreach (var item in items)
                {
                    var itemDB = allItemsDB.FirstOrDefault(i => i.Id == item.Item.Id);

                    decimal popust = (100 - paymentPlaceDB.Popust) / 100;
                    //var itemNadgroup = context.Items.Join(context.ItemGroups,
                    //item => item.IdItemGroup,
                    //itemGroup => itemGroup.Id,
                    //(item, itemGroup) => new { Item = item, ItemGroup = itemGroup })
                    //.Join(context.Supergroups,
                    //group => group.ItemGroup.IdSupergroup,
                    //supergroup => supergroup.Id,
                    //(group, supergroup) => new { Group = group, Supergroup = supergroup })
                    //.FirstOrDefault(it => it.Group.Item.Id == item.Item.Id);

                    if (itemDB != null)
                    {

                        if (itemDB.ItemGroupNavigation.IdSupergroupNavigation.Name.ToLower().Contains("hrana") ||
                        itemDB.ItemGroupNavigation.IdSupergroupNavigation.Name.ToLower().Contains("kuhinja"))
                        {
                            if (!string.IsNullOrEmpty(item.GlobalZelja))
                            {
                                orderKuhinja.Items.Add(new ClickBar_Common.Models.Order.ItemOrder()
                                {
                                    Name = item.Item.Name,
                                    Quantity = item.Quantity,
                                    Id = item.Item.Id,
                                    TotalAmount = item.TotalAmout * popust,
                                    Zelja = item.GlobalZelja
                                });
                            }
                            else if (item.Zelje.FirstOrDefault(f => !string.IsNullOrEmpty(f.Description)) != null)
                            {
                                var saZeljama = item.Zelje.Where(z => !string.IsNullOrEmpty(z.Description));

                                if (saZeljama.Any())
                                {
                                    foreach (var z in saZeljama)
                                    {
                                        decimal quantityz = item.Quantity;
                                        orderKuhinja.Items.Add(new ClickBar_Common.Models.Order.ItemOrder()
                                        {
                                            Name = item.Item.Name,
                                            Quantity = 1,
                                            Id = item.Item.Id,
                                            TotalAmount = item.Item.SellingUnitPrice * popust,
                                            Zelja = z.Description
                                        });
                                    }
                                }

                                decimal quantity = item.Quantity - (saZeljama != null ? saZeljama.Count() : 0);

                                if (quantity > 0)
                                {
                                    orderKuhinja.Items.Add(new ClickBar_Common.Models.Order.ItemOrder()
                                    {
                                        Name = item.Item.Name,
                                        Quantity = quantity,
                                        Id = item.Item.Id,
                                        TotalAmount = decimal.Round(item.Item.SellingUnitPrice * quantity, 2) * popust,
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
                                    TotalAmount = item.TotalAmout * popust,
                                });
                            }
                        }
                        else
                        {
                            if (itemDB.ItemGroupNavigation.IdSupergroupNavigation.Name.ToLower().Contains("pice") ||
                            itemDB.ItemGroupNavigation.IdSupergroupNavigation.Name.ToLower().Contains("piće") ||
                            itemDB.ItemGroupNavigation.IdSupergroupNavigation.Name.ToLower().Contains("sank") ||
                            itemDB.ItemGroupNavigation.IdSupergroupNavigation.Name.ToLower().Contains("šank"))
                            {
                                orderSank.Items.Add(new ClickBar_Common.Models.Order.ItemOrder()
                                {
                                    Name = item.Item.Name,
                                    Quantity = item.Quantity,
                                    Id = item.Item.Id,
                                    TotalAmount = item.TotalAmout * popust,
                                });
                            }
                            else
                            {
                                orderDrugo.OrderName = $"{itemDB.ItemGroupNavigation.IdSupergroupNavigation.Name[0]}";
                                orderDrugo.Items.Add(new ClickBar_Common.Models.Order.ItemOrder()
                                {
                                    Name = item.Item.Name,
                                    Quantity = item.Quantity,
                                    Id = item.Item.Id,
                                    TotalAmount = item.TotalAmout * popust,
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
                            TotalAmount = item.TotalAmout * popust,
                        });
                    }
                }

                int orderCounter = 1;

                var ordersTodayDB = context.OrdersToday.Where(o => o.OrderDateTime.Date == DateTime.Now.Date);

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

                    var ordersTodayTypeDB = context.OrdersToday.Where(o => o.OrderDateTime.Date == DateTime.Now.Date &&
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
                        CashierId = !string.IsNullOrEmpty(paymentPlaceDB.Name) && paymentPlaceDB.Name.ToLower().Contains("dostava 1") ? "3333" :
                        !string.IsNullOrEmpty(paymentPlaceDB.Name) && paymentPlaceDB.Name.ToLower().Contains("dostava 2") ? "4444" : cashierDB.Id,
                        Counter = orderCounter,
                        CounterType = orderCounterType,
                        OrderDateTime = DateTime.Now,
                        TotalPrice = totalAmount,
                        Name = orderSank.OrderName,
                        TableId = paymentPlaceDB.Id,
                        OrderTodayItems = new List<OrderTodayItemDB>(),
                        Faza = (int)FazaKuhinjeEnumeration.Nova
                    };

                    //sqliteDbContext.OrdersToday.Add(orderTodayDB);
                    //sqliteDbContext.SaveChanges();

                    foreach(var item in orderSank.Items)
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
                                Id = Guid.NewGuid().ToString(),
                                ItemId = item.Id,
                                OrderTodayId = orderTodayDB.Id,
                                Quantity = item.Quantity,
                                TotalPrice = item.TotalAmount,
                            };
                            orderTodayDB.OrderTodayItems.Add(orderTodayItemDB);
                        }
                    }
                    await context.OrdersToday.AddAsync(orderTodayDB);
                    await context.SaveChangesAsync();

                    FormatPos.PrintOrder(orderSank, posType, ClickBar_Printer.Enums.OrderTypeEnumeration.Sank);
                }
                if (orderKuhinja.Items.Any())
                {
                    int orderCounterType = 1;

                    var ordersTodayTypeDB = context.OrdersToday.Where(o => o.OrderDateTime.Date == DateTime.Now.Date &&
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
                        CashierId = !string.IsNullOrEmpty(paymentPlaceDB.Name) && paymentPlaceDB.Name.ToLower().Contains("dostava 1") ? "3333" :
                        !string.IsNullOrEmpty(paymentPlaceDB.Name) && paymentPlaceDB.Name.ToLower().Contains("dostava 2") ? "4444" : cashierDB.Id,
                        Counter = orderCounter,
                        CounterType = orderCounterType,
                        OrderDateTime = DateTime.Now,
                        TotalPrice = totalAmount,
                        Name = orderKuhinja.OrderName,
                        TableId = paymentPlaceDB.Id,
                        OrderTodayItems = new List<OrderTodayItemDB>(),
                        Faza = (int)FazaKuhinjeEnumeration.Nova
                    };

                    //sqliteDbContext.OrdersToday.Add(orderTodayDB);
                    //sqliteDbContext.SaveChanges();

                    foreach (var item in orderKuhinja.Items)
                    {
                        var orderTodayItemDB = orderTodayDB.OrderTodayItems.FirstOrDefault(o => o.ItemId == item.Id);

                        //if (orderTodayItemDB != null)
                        //{
                        //    orderTodayItemDB.Quantity += item.Quantity;
                        //    orderTodayItemDB.TotalPrice += item.TotalAmount;
                        //    //sqliteDbContext.OrderTodayItems.Update(orderTodayItemDB);
                        //}
                        //else
                        //{
                        orderTodayItemDB = new OrderTodayItemDB()
                        {
                            Id = Guid.NewGuid().ToString(),
                            ItemId = item.Id,
                            OrderTodayId = orderTodayDB.Id,
                            Quantity = item.Quantity,
                            TotalPrice = item.TotalAmount,
                            Zelja = item.Zelja
                        };
                        orderTodayDB.OrderTodayItems.Add(orderTodayItemDB);
                        //}
                        //sqliteDbContext.SaveChanges();
                    }
                    await context.OrdersToday.AddAsync(orderTodayDB);
                    await context.SaveChangesAsync();


                    if (string.IsNullOrEmpty(SettingsManager.Instance.GetPathToDrljaKuhinjaDB()))
                    {
                        FormatPos.PrintOrder(orderKuhinja, posType, ClickBar_Printer.Enums.OrderTypeEnumeration.Kuhinja);
                    }
                }
                if (orderDrugo.Items.Any())
                {
                    int orderCounterType = 1;

                    var ordersTodayTypeDB = context.OrdersToday.Where(o => o.OrderDateTime.Date == DateTime.Now.Date &&
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
                        CashierId = !string.IsNullOrEmpty(paymentPlaceDB.Name) && paymentPlaceDB.Name.ToLower().Contains("dostava 1") ? "3333" :
                        !string.IsNullOrEmpty(paymentPlaceDB.Name) && paymentPlaceDB.Name.ToLower().Contains("dostava 2") ? "4444" : cashierDB.Id,
                        Counter = orderCounter,
                        CounterType = orderCounterType,
                        OrderDateTime = DateTime.Now,
                        TotalPrice = totalAmount,
                        Name = orderDrugo.OrderName,
                        TableId = paymentPlaceDB.Id,
                        OrderTodayItems = new List<OrderTodayItemDB>(),
                        Faza = (int)FazaKuhinjeEnumeration.Nova
                    };

                    foreach (var item in orderDrugo.Items)
                    {
                        var orderTodayItemDB = orderTodayDB.OrderTodayItems.FirstOrDefault(o => o.ItemId == item.Id);

                        if (orderTodayItemDB != null)
                        {
                            orderTodayItemDB.Quantity += item.Quantity;
                            orderTodayItemDB.TotalPrice += item.TotalAmount;
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
                    }

                    await context.OrdersToday.AddAsync(orderTodayDB);
                    await context.SaveChangesAsync();

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

        private async Task ProcessOrder(SqlServerDbContext context, ClickBar_Common.Models.Order.Order order, PaymentPlaceDB paymentPlaceDB, PosTypeEnumeration posType, OrderTypeEnumeration orderType)
        {
            // Logika za obradu narudžbine i štampanje
            var totalAmount = order.Items.Sum(i => i.TotalAmount);
            var orderTodayDB = new OrderTodayDB
            {
                // Inicijalizacija OrderTodayDB
            };

            foreach (var item in order.Items)
            {
                var orderTodayItemDB = new OrderTodayItemDB
                {
                    // Inicijalizacija OrderTodayItemDB
                };
                orderTodayDB.OrderTodayItems.Add(orderTodayItemDB);
            }

            await context.OrdersToday.AddAsync(orderTodayDB);
            await context.SaveChangesAsync();

            FormatPos.PrintOrder(order, posType, orderType);
        }
        // Metoda za obaveštavanje UI-a o promenama
        private void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}