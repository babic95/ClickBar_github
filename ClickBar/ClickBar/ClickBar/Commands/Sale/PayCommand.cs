using ClickBar.Converters;
using ClickBar.Enums;
using ClickBar.Enums.AppMain.Statistic;
using ClickBar.Enums.Kuhinja;
using ClickBar.Models.AppMain.Statistic;
using ClickBar.Models.Sale;
using ClickBar.ViewModels;
using ClickBar.ViewModels.Sale;
using ClickBar.Views.Sale.PaySale;
using ClickBar_Common.Enums;
using ClickBar_Common.Models.Invoice;
using ClickBar_Common.Models.Invoice.FileSystemWatcher;
using ClickBar_DatabaseSQLManager;
using ClickBar_DatabaseSQLManager.Models;
using ClickBar_Logging;
using ClickBar_Printer;
using ClickBar_Printer.PaperFormat;
using ClickBar_Settings;
using DocumentFormat.OpenXml.Vml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using MessageBox = System.Windows.MessageBox;

namespace ClickBar.Commands.Sale
{
    public class PayCommand<TViewModel> : ICommand where TViewModel : ViewModelBase
    {
        public event EventHandler CanExecuteChanged;

        private TViewModel _viewModel;
        private IServiceProvider _serviceProvider;

        private const int ERROR_SHARING_VIOLATION = 32;
        private const int ERROR_LOCK_VIOLATION = 33;

        private DateTime _timer;
        private List<Payment> _payment;

        public PayCommand(IServiceProvider serviceProvider, TViewModel viewModel)
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
            if (_viewModel is SaleViewModel saleViewModel)
            {
                if (saleViewModel.OldOrders.Any() ||
                    saleViewModel.ItemsInvoice.Any())
                {
                    if (saleViewModel.OldOrders.Any())
                    {
                        var someoneElsePayment = saleViewModel.OldOrders.Any(o => o.CashierId != saleViewModel.LoggedCashier.Id);

                        if (SettingsManager.Instance.GetDisableSomeoneElsePayment() &&
                            someoneElsePayment)
                        {
                            MessageBox.Show("Nije moguće naplatiti sto od drugog konobara!",
                                "Greška",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                            return;
                        }

                        foreach (var oldOrder in saleViewModel.OldOrders)
                        {
                            if (oldOrder.Items.Any())
                            {
                                foreach (var item in oldOrder.Items)
                                {
                                    var it = saleViewModel.ItemsInvoice.FirstOrDefault(i => i.Item.Id == item.Item.Id);

                                    if (it == null)
                                    {
                                        it = new ItemInvoice(item.Item, item.Quantity);
                                        saleViewModel.ItemsInvoice.Add(it);
                                    }
                                    else
                                    {
                                        it.Quantity += item.Quantity;
                                        it.TotalAmout += item.TotalAmout;
                                    }
                                }
                            }
                        }
                    }

                    PaySaleViewModel paySaleViewModel = new PaySaleViewModel(_serviceProvider, saleViewModel);
                    saleViewModel.PayWindow = new PaySaleWindow(paySaleViewModel);// _serviceProvider.GetRequiredService<PaySaleWindow>();
                    saleViewModel.PayWindow.ShowDialog();

                    saleViewModel.Reset();
                }
                else
                {
                    MessageBox.Show("Nema artikala za prodaju!", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else if (_viewModel is PaySaleViewModel paySaleViewModel)
            {
                if (parameter is not string)
                {
                    return;
                }

                string paymentType = parameter as string;

                paySaleViewModel.ChangeFocusCommand.Execute("Pay");

                if (paySaleViewModel.Amount < paySaleViewModel.TotalAmount &&
                    paySaleViewModel.InvoiceType != Enums.Sale.InvoiceTypeEnumeration.Predračun)
                {
                    MessageBox.Show("Uplata nije dobra!", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                    paySaleViewModel.Focus = FocusEnumeration.Cash;
                    return;
                }


                if (paymentType == "WireTransfer" &&
                    string.IsNullOrEmpty(paySaleViewModel.BuyerId))
                {
                    MessageBox.Show("Morate uneti PIB kupca!", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                    paySaleViewModel.Focus = FocusEnumeration.BuyerId;
                    return;
                }


                if (!string.IsNullOrEmpty(paySaleViewModel.BuyerId))
                {
                    if (paySaleViewModel.CurrentBuyerIdElement.Id == 10)
                    {
                        if (paySaleViewModel.BuyerId.Length != 9)
                        {
                            MessageBox.Show("PIB mora da ima 9 cifara!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                    else if (paySaleViewModel.CurrentBuyerIdElement.Id == 11)
                    {
                        if (paySaleViewModel.BuyerId.Length != 13)
                        {
                            MessageBox.Show("JMBG mora da ima 13 cifara!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                    else if (paySaleViewModel.CurrentBuyerIdElement.Id == 12)
                    {
                        if (paySaleViewModel.BuyerId.Length != 15 ||
                            !paySaleViewModel.BuyerId.Contains(":"))
                        {
                            MessageBox.Show("Morate uneti PIB:JBKJS budžetskog korisnika!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                    else if (paySaleViewModel.CurrentBuyerIdElement.Id == 20)
                    {
                        if (paySaleViewModel.BuyerId.Length != 9)
                        {
                            MessageBox.Show("Broj lične karte mora da ima 9 cifara!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }

                    using (var dbContext = paySaleViewModel.DbContext.CreateDbContext())
                    {
                        var partnerDB = dbContext.Partners.AsNoTracking().FirstOrDefault(partner => partner.Pib == paySaleViewModel.BuyerId);
                        if (partnerDB == null)
                        {
                            if (string.IsNullOrEmpty(paySaleViewModel.BuyerName))
                            {
                                var result = MessageBox.Show("Ako želite da zavedete kupca u bazu, morate da unesete i naziv firme?",
                                    "Unos nove firme?",
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Question);

                                if (result == MessageBoxResult.Yes)
                                {
                                    return;
                                }
                            }
                            else
                            {
                                partnerDB = new PartnerDB()
                                {
                                    Pib = paySaleViewModel.BuyerId,
                                    Name = paySaleViewModel.BuyerName,
                                    Address = !string.IsNullOrEmpty(paySaleViewModel.BuyerAdress) ? paySaleViewModel.BuyerAdress : null
                                };
                                dbContext.Partners.Add(partnerDB);
                                dbContext.SaveChanges();
                            }
                        }
                    }
                }

                //if (paySaleViewModel.SaleViewModel.InvoiceType == Enums.InvoiceTypeEnumeration.Avans &&
                //    paySaleViewModel.Amount < 1m)
                //{
                //    MessageBox.Show("Vrednost uplate avansa mora biti minimum 1!", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                //    paySaleViewModel.Focus = ViewModels.AppMain.Sale.FocusEnumeration.Cash;
                //    return;
                //}

                //if (paySaleViewModel.SaleViewModel.InvoiceType != Enums.InvoiceTypeEnumeration.Avans &&
                //    paySaleViewModel.Amount < paySaleViewModel.SaleViewModel.TotalAmount)
                //{
                //    paySaleViewModel.Focus = ViewModels.AppMain.Sale.FocusEnumeration.Cash;
                //    return;
                //}

                decimal popust = 0;
                if (!string.IsNullOrEmpty(paySaleViewModel.Popust))
                {
                    try
                    {
                        popust = Convert.ToDecimal(paySaleViewModel.Popust);
                    }
                    catch { }
                }

                if (popust > 0)
                {
                    paySaleViewModel.Amount = paySaleViewModel.TotalAmount = paySaleViewModel.TotalAmount * ((100 - popust) / 100);
                }

                if (paySaleViewModel.Amount > paySaleViewModel.TotalAmount)
                {
                    decimal rest = paySaleViewModel.Amount - paySaleViewModel.TotalAmount;
                    MessageBox.Show($"KUSUR JE -> {rest}", "Kusur", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                AddPayment(paySaleViewModel, paymentType);

                FinisedSale(paySaleViewModel, popust);

                if (paySaleViewModel.SaleViewModel.PayWindow != null &&
                    paySaleViewModel.SaleViewModel.PayWindow.IsActive)
                {
                    paySaleViewModel.SaleViewModel.PayWindow.Close();
                }
            }
            else if (_viewModel is SplitOrderViewModel splitOrderViewModel)
            {
                splitOrderViewModel.PaySaleViewModel.SplitOrderWindow.Close();

                if (splitOrderViewModel.ItemsInvoiceForPay.Any())
                {
                    splitOrderViewModel.PaySaleViewModel.ItemsInvoice = splitOrderViewModel.ItemsInvoiceForPay;
                    splitOrderViewModel.PaySaleViewModel.TotalAmount = splitOrderViewModel.TotalAmountForPay;

                    splitOrderViewModel.PaySaleViewModel.SaleViewModel.ItemsInvoice = splitOrderViewModel.ItemsInvoiceForPay;
                    splitOrderViewModel.PaySaleViewModel.SaleViewModel.TotalAmount = splitOrderViewModel.TotalAmountForPay;
                }
            }
        }
        private void AddPayment(PaySaleViewModel paySaleViewModel, string paymentType)
        {
            _payment = new List<Payment>();

            if (!string.IsNullOrEmpty(paymentType))
            {
                switch (paymentType)
                {
                    case "Cash":
                        var paymentCash = paySaleViewModel.Payment.FirstOrDefault(pay => pay.PaymentType == PaymentTypeEnumeration.Cash);
                        if (paymentCash != null)
                        {
                            paymentCash.Amount += paySaleViewModel.TotalAmount;
                        }
                        else
                        {
                            paySaleViewModel.Payment.Add(new Payment()
                            {
                                Amount = paySaleViewModel.TotalAmount,
                                PaymentType = PaymentTypeEnumeration.Cash,
                            });
                        }
                        _payment.Add(new Payment()
                        {
                            Amount = paySaleViewModel.TotalAmount,
                            PaymentType = PaymentTypeEnumeration.Cash,
                        });
                        break;
                    case "Card":
                        var paymentCard = paySaleViewModel.Payment.FirstOrDefault(pay => pay.PaymentType == PaymentTypeEnumeration.Cash);
                        if (paymentCard != null)
                        {
                            paymentCard.Amount += paySaleViewModel.TotalAmount;
                        }
                        else
                        {
                            paySaleViewModel.Payment.Add(new Payment()
                            {
                                Amount = paySaleViewModel.TotalAmount,
                                PaymentType = PaymentTypeEnumeration.Cash,
                            });
                        }
                        _payment.Add(new Payment()
                        {
                            Amount = paySaleViewModel.TotalAmount,
                            PaymentType = PaymentTypeEnumeration.Card,
                        });
                        break;
                    case "WireTransfer":
                        paySaleViewModel.Payment.Add(new Payment()
                        {
                            Amount = paySaleViewModel.TotalAmount,
                            PaymentType = PaymentTypeEnumeration.WireTransfer,
                        });
                        _payment.Add(new Payment()
                        {
                            Amount = paySaleViewModel.TotalAmount,
                            PaymentType = PaymentTypeEnumeration.WireTransfer,
                        });
                        break;
                }
            }
            else
            {
                decimal Other = Convert.ToDecimal(paySaleViewModel.Other);
                decimal Cash = Convert.ToDecimal(paySaleViewModel.Cash);
                decimal Card = Convert.ToDecimal(paySaleViewModel.Card);
                decimal Check = Convert.ToDecimal(paySaleViewModel.Check);
                decimal WireTransfer = Convert.ToDecimal(paySaleViewModel.WireTransfer);
                decimal Voucher = Convert.ToDecimal(paySaleViewModel.Voucher);
                decimal MobileMoney = Convert.ToDecimal(paySaleViewModel.MobileMoney);
                if (Other > 0)
                {
                    paySaleViewModel.Payment.Add(new Payment()
                    {
                        Amount = Other,
                        PaymentType = PaymentTypeEnumeration.Other,
                    });
                }
                if (Cash > 0)
                {
                    var payment = paySaleViewModel.Payment.FirstOrDefault(pay => pay.PaymentType == PaymentTypeEnumeration.Cash);
                    if (payment != null)
                    {
                        payment.Amount += Cash;
                    }
                    else
                    {
                        paySaleViewModel.Payment.Add(new Payment()
                        {
                            Amount = Cash,
                            PaymentType = PaymentTypeEnumeration.Cash,
                        });
                    }
                    _payment.Add(new Payment()
                    {
                        Amount = Cash,
                        PaymentType = PaymentTypeEnumeration.Cash,
                    });
                }
                if (Card > 0)
                {
                    var payment = paySaleViewModel.Payment.FirstOrDefault(pay => pay.PaymentType == PaymentTypeEnumeration.Cash);
                    if (payment != null)
                    {
                        payment.Amount += Card;
                    }
                    else
                    {
                        paySaleViewModel.Payment.Add(new Payment()
                        {
                            Amount = Card,
                            PaymentType = PaymentTypeEnumeration.Cash,
                        });
                    }
                    _payment.Add(new Payment()
                    {
                        Amount = Card,
                        PaymentType = PaymentTypeEnumeration.Card,
                    });
                }
                if (Check > 0)
                {
                    paySaleViewModel.Payment.Add(new Payment()
                    {
                        Amount = Check,
                        PaymentType = PaymentTypeEnumeration.Cash,
                    });
                }
                if (WireTransfer > 0)
                {
                    paySaleViewModel.Payment.Add(new Payment()
                    {
                        Amount = WireTransfer,
                        PaymentType = PaymentTypeEnumeration.WireTransfer,
                    });
                    _payment.Add(new Payment()
                    {
                        Amount = WireTransfer,
                        PaymentType = PaymentTypeEnumeration.WireTransfer,
                    });
                }
                if (Voucher > 0)
                {
                    paySaleViewModel.Payment.Add(new Payment()
                    {
                        Amount = Voucher,
                        PaymentType = PaymentTypeEnumeration.Voucher,
                    });
                }
                if (MobileMoney > 0)
                {
                    paySaleViewModel.Payment.Add(new Payment()
                    {
                        Amount = MobileMoney,
                        PaymentType = PaymentTypeEnumeration.MobileMoney,
                    });
                }
            }
        }
        private void FinisedSale(PaySaleViewModel paySaleViewModel, decimal popust)
        {
            InvoceRequestFileSystemWatcher invoiceRequset = new InvoceRequestFileSystemWatcher()
            {
                Cashier = paySaleViewModel.SaleViewModel.LoggedCashier.Name,
                InvoiceType = (InvoiceTypeEenumeration)paySaleViewModel.InvoiceType,
                TransactionType = TransactionTypeEnumeration.Sale,
                BuyerId = string.IsNullOrEmpty(paySaleViewModel.BuyerId) == true ? null :
                $"{paySaleViewModel.CurrentBuyerIdElement.Id}:{paySaleViewModel.BuyerId}",
                BuyerAddress = string.IsNullOrEmpty(paySaleViewModel.BuyerAdress) == true ? null :
                paySaleViewModel.BuyerAdress,
                BuyerName = string.IsNullOrEmpty(paySaleViewModel.BuyerName) == true ? null :
                paySaleViewModel.BuyerName,
                Payment = paySaleViewModel.Payment
            };

            List<ItemFileSystemWatcher> items = new List<ItemFileSystemWatcher>();

            decimal total = 0;

            foreach (var item in paySaleViewModel.ItemsInvoice)
            {
                if (item.TotalAmout > 0)
                {
                    item.Item.SellingUnitPrice = Decimal.Round(item.Item.SellingUnitPrice * ((100 - popust) / 100), 2);
                    ItemFileSystemWatcher itemFileSystemWatcher = new ItemFileSystemWatcher()
                    {
                        Id = item.Item.Id,
                        Label = item.Item.Label,
                        Name = $"{item.Item.Name}",
                        UnitPrice = item.Item.SellingUnitPrice,
                        Quantity = item.Quantity,
                        TotalAmount = Decimal.Round(item.Item.SellingUnitPrice * item.Quantity, 2),
                        Jm = item.Item.Jm
                    };

                    items.Add(itemFileSystemWatcher);
                    total += itemFileSystemWatcher.TotalAmount;
                }
            }
            invoiceRequset.Items = items;
#if CRNO
            //paySaleViewModel.ItemsInvoice.ToList().ForEach(item =>
            //{
            //    item.Item.SellingUnitPrice = Decimal.Round(item.Item.SellingUnitPrice * ((100 - popust) / 100), 2);
            //    ItemFileSystemWatcher itemFileSystemWatcher = new ItemFileSystemWatcher()
            //    {
            //        Id = item.Item.Id,
            //        Label = item.Item.Label,
            //        Name = $"{item.Item.Name}",
            //        UnitPrice = item.Item.SellingUnitPrice,
            //        Quantity = item.Quantity,
            //        TotalAmount = Decimal.Round(item.Item.SellingUnitPrice * item.Quantity, 2),
            //        Jm = item.Item.Jm
            //    };

            //    items.Add(itemFileSystemWatcher);
            //    total += itemFileSystemWatcher.TotalAmount;
            //});
            //invoiceRequset.Items = items;
            Black(invoiceRequset, paySaleViewModel, total, items, paySaleViewModel.SaleViewModel.LoggedCashier.Id);
#else
            Normal(invoiceRequset, paySaleViewModel, total, items, popust, paySaleViewModel.SaleViewModel.LoggedCashier);
#endif
        }
        private async Task TakingDownOrder(InvoiceDB invoice, PaySaleViewModel paySaleViewModel, int tableId, ObservableCollection<ItemInvoice> items, string cashierId)
        {
            try
            {
                using (var dbContext = paySaleViewModel.DbContext.CreateDbContext())
                {
                    var unprocessedOrders = dbContext.UnprocessedOrders.FirstOrDefault(order => order.PaymentPlaceId == tableId);

                    if (unprocessedOrders != null)
                    {
                        OrderDB orderDB = new OrderDB()
                        {
                            InvoiceId = invoice.Id,
                            PaymentPlaceId = tableId,
                            CashierId = cashierId
                        };
                        dbContext.Orders.Add(orderDB);

                        var ordersForTable = dbContext.OrdersToday.Include(i => i.OrderTodayItems)
                            .Where(order => order.UnprocessedOrderId == unprocessedOrders.Id).ToList();

                        if (ordersForTable != null &&
                            ordersForTable.Any())
                        {
                            if (ordersForTable.Sum(o => o.OrderTodayItems.Sum(i => i.Quantity - i.StornoQuantity - i.NaplacenoQuantity)) == invoice.ItemInvoices.Sum(i => i.Quantity) &&
                                ordersForTable.Sum(o => o.OrderTodayItems.Sum(i => (i.TotalPrice / i.Quantity) * (i.Quantity - i.StornoQuantity - i.NaplacenoQuantity))) == invoice.TotalAmount)
                            {
                                ordersForTable.ForEach(o =>
                                {
                                    o.UnprocessedOrderId = null;
                                    o.Faza = (int)FazaKuhinjeEnumeration.Naplacena;
                                    dbContext.OrdersToday.Update(o);
                                });
                                await dbContext.SaveChangesAsync();

                                dbContext.UnprocessedOrders.Remove(unprocessedOrders);
                            }
                            else
                            {
                                foreach (var itemFoPay in invoice.ItemInvoices)
                                {
                                    if (itemFoPay.Quantity.HasValue)
                                    {
                                        decimal quantityForPay = itemFoPay.Quantity.Value;

                                        var orders = ordersForTable.Where(o => o.OrderTodayItems.Any(i => i.ItemId == itemFoPay.ItemCode &&
                                        (i.Quantity - i.StornoQuantity - i.NaplacenoQuantity) > 0)).ToList();

                                        foreach (var order in orders)
                                        {
                                            if(quantityForPay == 0)
                                            {
                                                break;
                                            }

                                            var orderTodayItem = order.OrderTodayItems.FirstOrDefault(i => i.ItemId == itemFoPay.ItemCode);
                                            if (orderTodayItem != null)
                                            {
                                                if((orderTodayItem.Quantity - orderTodayItem.StornoQuantity - orderTodayItem.NaplacenoQuantity) >= quantityForPay)
                                                {
                                                    orderTodayItem.NaplacenoQuantity = quantityForPay;
                                                    quantityForPay = 0;
                                                }
                                                else
                                                {
                                                    decimal q = orderTodayItem.Quantity - orderTodayItem.StornoQuantity - orderTodayItem.NaplacenoQuantity;
                                                    orderTodayItem.NaplacenoQuantity += q;
                                                    quantityForPay -= q;
                                                }
                                                dbContext.OrderTodayItems.Update(orderTodayItem);
                                            }
                                        }
                                    }
                                }
                                await dbContext.SaveChangesAsync();

                                var ordersNaplaceno = dbContext.OrdersToday.Include(o => o.OrderTodayItems).Where(
                                    o => o.UnprocessedOrderId == unprocessedOrders.Id &&
                                    o.Faza != (int)FazaKuhinjeEnumeration.Naplacena &&
                                    o.Faza != (int)FazaKuhinjeEnumeration.Obrisana &&
                                    o.OrderTodayItems.Sum(i => i.Quantity - i.StornoQuantity - i.NaplacenoQuantity) == 0).ToList();

                                if(ordersNaplaceno != null &&
                                    ordersNaplaceno.Any())
                                {
                                    foreach(var o in ordersNaplaceno)
                                    {
                                        o.UnprocessedOrderId = null;
                                        o.Faza = (int)FazaKuhinjeEnumeration.Naplacena;
                                        dbContext.OrdersToday.Update(o);
                                    }
                                    await dbContext.SaveChangesAsync();
                                }

                                //unprocessedOrders.TotalAmount -= invoice.TotalAmount.Value;
                                var porudzbina = dbContext.OrdersToday.Where(o => o.UnprocessedOrderId == unprocessedOrders.Id);

                                if(porudzbina == null ||
                                    !porudzbina.Any())
                                {
                                    dbContext.UnprocessedOrders.Remove(unprocessedOrders);
                                    await dbContext.SaveChangesAsync();
                                }

                            }
                            await dbContext.SaveChangesAsync();
                        }
                    }
                    else
                    {
                        OrderDB orderDB = new OrderDB()
                        {
                            InvoiceId = invoice.Id,
                            PaymentPlaceId = tableId,
                            CashierId = cashierId
                        };
                        dbContext.Orders.Add(orderDB);
                        await dbContext.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("PayCommand -> TakingDownOrder -> Greska prilikom skidanja porudzbine sa stola: ", ex);
            }
        }
        private async Task UpdateInvode(InvoiceDB invoiceDB,
            decimal total,
            ResponseJson responseJson,
            PaySaleViewModel paySaleViewModel)
        {
            try
            {
                invoiceDB.SdcDateTime = Convert.ToDateTime(responseJson.DateTime);
                invoiceDB.TotalAmount = total;
                invoiceDB.InvoiceCounter = responseJson.TotalInvoiceNumber;
                invoiceDB.InvoiceNumberResult = responseJson.InvoiceNumber;
                using (var dbContext = paySaleViewModel.DbContext.CreateDbContext())
                {
                    dbContext.Invoices.Update(invoiceDB);
                    await dbContext.SaveChangesAsync();

                    if (responseJson.TaxItems != null &&
                        responseJson.TaxItems.Any())
                    {
                        responseJson.TaxItems.ToList().ForEach(taxItem =>
                        {
                            TaxItemInvoiceDB taxItemInvoiceDB = new TaxItemInvoiceDB()
                            {
                                Amount = taxItem.Amount,
                                CategoryName = taxItem.CategoryName,
                                CategoryType = (int)taxItem.CategoryType.Value,
                                Label = taxItem.Label,
                                Rate = taxItem.Rate,
                                InvoiceId = invoiceDB.Id
                            };

                            dbContext.TaxItemInvoices.Add(taxItemInvoiceDB);
                        });
                    }

                    var paymentInvoiceTotal = dbContext.PaymentInvoices.FirstOrDefault(p => p.InvoiceId == invoiceDB.Id);

                    if (paymentInvoiceTotal != null)
                    {
                        if (invoiceDB.TotalAmount != paymentInvoiceTotal.Amout)
                        {
                            paymentInvoiceTotal.Amout = invoiceDB.TotalAmount;
                            dbContext.PaymentInvoices.Update(paymentInvoiceTotal);
                        }
                    }

                    await dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greška prilikom ažuriranja računa!",
                    "Greška",
                    MessageBoxButton.OK);
                Log.Error($"PayCommand - UpdateInvode - Greska prilikom update Invoice {invoiceDB.Id}", ex);
                throw;
            }
        }
        private async Task<InvoiceDB> InsertInvoiceInDB(InvoceRequestFileSystemWatcher invoiceRequset,
            List<ItemFileSystemWatcher> items,
            PaySaleViewModel paySaleViewModel,
            DateTime dateTimeOfIssue)
        {
            invoiceRequset.Items = items;

            InvoiceDB invoiceDB = new InvoiceDB()
            {
                Id = Guid.NewGuid().ToString(),
                DateAndTimeOfIssue = dateTimeOfIssue,
                Cashier = paySaleViewModel.SaleViewModel.LoggedCashier.Id,
                InvoiceType = (int)invoiceRequset.InvoiceType,
                TransactionType = (int)invoiceRequset.TransactionType,
                BuyerId = invoiceRequset.BuyerId,
                BuyerName = invoiceRequset.BuyerName,
                BuyerAddress = invoiceRequset.BuyerAddress,
            };
            using (var dbContext = paySaleViewModel.DbContext.CreateDbContext())
            {
                dbContext.Add(invoiceDB);
                await dbContext.SaveChangesAsync();

                int itemInvoiceId = 0;
                foreach(var item in paySaleViewModel.ItemsInvoice.ToList())
                {
                    ItemDB? itemDB = dbContext.Items.AsNoTracking().FirstOrDefault(i => i.Id == item.Item.Id);
                    if (itemDB != null)
                    {
                        if (itemDB.IdNorm != null)
                        {
                            var norms = dbContext.ItemsInNorm.AsNoTracking().Where(norm => norm.IdNorm == itemDB.IdNorm);

                            if (norms != null &&
                                norms.Any())
                            {
                                foreach(var norm in norms)
                                {
                                    var normItem = dbContext.Items.AsNoTracking().FirstOrDefault(i => i.Id == norm.IdItem);
                                    if (normItem != null)
                                    {
                                        if (normItem.IdNorm != null)
                                        {
                                            var norms2 = dbContext.ItemsInNorm.AsNoTracking().Where(norm => norm.IdNorm == normItem.IdNorm);

                                            if (norms2 != null &&
                                                norms2.Any())
                                            {
                                                foreach(var norm2 in norms2)
                                                {
                                                    var normItem2 = dbContext.Items.AsNoTracking().FirstOrDefault(i => i.Id == norm2.IdItem);

                                                    if (normItem2 != null)
                                                    {
                                                        if (normItem2.IdNorm != null)
                                                        {
                                                            var norms3 = dbContext.ItemsInNorm.AsNoTracking().Where(norm => norm.IdNorm == normItem2.IdNorm);
                                                            if (norms3 != null &&
                                                                norms3.Any())
                                                            {
                                                                foreach(var norm3 in norms3)
                                                                {
                                                                    var normItem3 = dbContext.Items.AsNoTracking().FirstOrDefault(i => i.Id == norm3.IdItem);
                                                                    if (normItem3 != null)
                                                                    {
                                                                        decimal unitPrice = normItem3.InputUnitPrice != null && normItem3.InputUnitPrice.HasValue ?
                                                                        normItem3.InputUnitPrice.Value : 0;

                                                                        var itemInvoice3 = new ItemInvoiceDB()
                                                                        {
                                                                            Id = itemInvoiceId++,
                                                                            Quantity = item.Quantity * norm.Quantity * norm2.Quantity * norm3.Quantity,
                                                                            TotalAmout = item.Quantity * norm.Quantity * norm2.Quantity * norm3.Quantity * unitPrice,
                                                                            Label = normItem3.Label,
                                                                            Name = normItem3.Name,
                                                                            UnitPrice = unitPrice,
                                                                            ItemCode = normItem3.Id,
                                                                            OriginalUnitPrice = unitPrice,
                                                                            InvoiceId = invoiceDB.Id,
                                                                            IsSirovina = 1,
                                                                            InputUnitPrice = unitPrice
                                                                        };
                                                                        dbContext.Add(itemInvoice3);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            decimal unitPrice = normItem2.InputUnitPrice != null && normItem2.InputUnitPrice.HasValue ?
                                                            normItem2.InputUnitPrice.Value : 0;

                                                            var itemInvoice2 = new ItemInvoiceDB()
                                                            {
                                                                Id = itemInvoiceId++,
                                                                Quantity = item.Quantity * norm.Quantity * norm2.Quantity,
                                                                TotalAmout = item.Quantity * norm.Quantity * norm2.Quantity * unitPrice,
                                                                Label = normItem2.Label,
                                                                Name = normItem2.Name,
                                                                UnitPrice = unitPrice,
                                                                ItemCode = normItem2.Id,
                                                                OriginalUnitPrice = unitPrice,
                                                                InvoiceId = invoiceDB.Id,
                                                                IsSirovina = 1,
                                                                InputUnitPrice = unitPrice
                                                            };
                                                            dbContext.Add(itemInvoice2);
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                decimal unitPrice = normItem.InputUnitPrice != null && normItem.InputUnitPrice.HasValue ?
                                                normItem.InputUnitPrice.Value : 0;

                                                var itemInvoice1 = new ItemInvoiceDB()
                                                {
                                                    Id = itemInvoiceId++,
                                                    Quantity = item.Quantity * norm.Quantity,
                                                    TotalAmout = item.Quantity * norm.Quantity * unitPrice,
                                                    Label = normItem.Label,
                                                    Name = normItem.Name,
                                                    UnitPrice = unitPrice,
                                                    ItemCode = normItem.Id,
                                                    OriginalUnitPrice = unitPrice,
                                                    InvoiceId = invoiceDB.Id,
                                                    IsSirovina = 1,
                                                    InputUnitPrice = unitPrice
                                                };
                                                dbContext.Add(itemInvoice1);
                                            }
                                        }
                                        else
                                        {
                                            decimal unitPrice = normItem.InputUnitPrice != null && normItem.InputUnitPrice.HasValue ?
                                            normItem.InputUnitPrice.Value : 0;

                                            var itemInvoice4 = new ItemInvoiceDB()
                                            {
                                                Id = itemInvoiceId++,
                                                Quantity = item.Quantity * norm.Quantity,
                                                TotalAmout = item.Quantity * norm.Quantity * unitPrice,
                                                Label = normItem.Label,
                                                Name = normItem.Name,
                                                UnitPrice = unitPrice,
                                                ItemCode = normItem.Id,
                                                OriginalUnitPrice = unitPrice,
                                                InvoiceId = invoiceDB.Id,
                                                IsSirovina = 1,
                                                InputUnitPrice = unitPrice
                                            };
                                            dbContext.Add(itemInvoice4);
                                        }
                                    }
                                }
                            }
                        }

                        var itemInvoice = new ItemInvoiceDB()
                        {
                            Id = itemInvoiceId++,
                            Quantity = item.Quantity,
                            TotalAmout = item.Quantity * item.Item.SellingUnitPrice,
                            Label = item.Item.Label,
                            Name = item.Item.Name,
                            UnitPrice = item.Item.SellingUnitPrice,
                            ItemCode = item.Item.Id,
                            OriginalUnitPrice = itemDB.SellingUnitPrice,
                            InvoiceId = invoiceDB.Id,
                            IsSirovina = 0,
                            InputUnitPrice = itemDB.InputUnitPrice
                        };
                        dbContext.Add(itemInvoice);
                    }
                }

                _payment.ForEach(payment =>
                {
                    PaymentInvoiceDB paymentInvoice = new PaymentInvoiceDB()
                    {
                        InvoiceId = invoiceDB.Id,
                        Amout = payment.Amount,
                        PaymentType = payment.PaymentType
                    };

                    dbContext.PaymentInvoices.Add(paymentInvoice);
                });
                await dbContext.SaveChangesAsync();
            }
            return invoiceDB;
        }
        private async Task TakingDownNorm(InvoiceDB invoice,
            PaySaleViewModel paySaleViewModel)
        {
            try
            {
                List<ItemDB> itemsForCondition = new List<ItemDB>();
                using (var dbContext = paySaleViewModel.DbContext.CreateDbContext())
                {
                    var itemsInInvoice = dbContext.ItemInvoices.Where(item => item.InvoiceId == invoice.Id &&
                    item.IsSirovina == 0);

                    if (itemsInInvoice != null && itemsInInvoice.Any())
                    {
                        foreach (var item in itemsInInvoice.ToList())
                        {
                            var it = dbContext.Items.FirstOrDefault(i => i.Id == item.ItemCode);
                            if (it != null && item.Quantity.HasValue)
                            {
                                var itemInNorm = dbContext.ItemsInNorm.Where(norm => it.IdNorm == norm.IdNorm);

                                if (itemInNorm != null && itemInNorm.Any())
                                {
                                    foreach (var norm in itemInNorm)
                                    {
                                        var itm = dbContext.Items.FirstOrDefault(i => i.Id == norm.IdItem);

                                        if (itm != null)
                                        {
                                            if (itm.IdNorm == null)
                                            {
                                                itm.TotalQuantity -= Decimal.Round(item.Quantity.Value * norm.Quantity, 3);
                                            }
                                            else
                                            {
                                                var itemInNorm2 = dbContext.ItemsInNorm.Where(norm => itm.IdNorm == norm.IdNorm);
                                                if (itemInNorm2.Any())
                                                {
                                                    foreach (var norm2 in itemInNorm2.ToList())
                                                    {
                                                        var itm2 = dbContext.Items.FirstOrDefault(i => i.Id == norm2.IdItem);

                                                        if (itm2 != null)
                                                        {
                                                            if (itm2.IdNorm == null)
                                                            {
                                                                itm2.TotalQuantity -= Decimal.Round(item.Quantity.Value * norm.Quantity * norm2.Quantity, 3);
                                                            }
                                                            else
                                                            {
                                                                var itemInNorm3 = dbContext.ItemsInNorm.Where(norm => itm2.IdNorm == norm.IdNorm);
                                                                if (itemInNorm3.Any())
                                                                {
                                                                    foreach (var norm3 in itemInNorm3)
                                                                    {
                                                                        var itm3 = dbContext.Items.FirstOrDefault(i => i.Id == norm3.IdItem);

                                                                        if (itm3 != null)
                                                                        {
                                                                            if (itm3.IdNorm == null)
                                                                            {
                                                                                itm3.TotalQuantity -= Decimal.Round(item.Quantity.Value * norm.Quantity * norm2.Quantity * norm3.Quantity, 3);
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    itm2.TotalQuantity -= Decimal.Round(item.Quantity.Value * norm.Quantity * norm2.Quantity, 3);
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    itm.TotalQuantity -= Decimal.Round(item.Quantity.Value * norm.Quantity, 3);
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    it.TotalQuantity -= item.Quantity.Value;
                                }
                                dbContext.Items.Update(it);
                            }
                        }
                        await dbContext.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"PayCommand -> TakingDownNorm -> Greska prilikom skidanja normativa za racun {invoice.Id} - {invoice.InvoiceNumberResult}: ", ex);
            }
        }
        private async void SaveToDB(InvoceRequestFileSystemWatcher invoiceRequset,
            List<ItemFileSystemWatcher> items,
            decimal total,
            PaySaleViewModel paySaleViewModel,
            int tableId,
            ObservableCollection<ItemInvoice> itemsInvoice,
            ResponseJson responseJson,
            InvoiceDB invoiceDB,
            string cashierId)
        {
            await UpdateInvode(invoiceDB, total, responseJson, paySaleViewModel);
            await TakingDownNorm(invoiceDB, paySaleViewModel);
            await TakingDownOrder(invoiceDB, paySaleViewModel, tableId, itemsInvoice, cashierId);
        }
        private async void Black(InvoceRequestFileSystemWatcher invoiceRequset,
            PaySaleViewModel paySaleViewModel,
            decimal total,
            List<ItemFileSystemWatcher> items, 
            string cashierId)
        {
            try
            {

                int tableId = paySaleViewModel.SaleViewModel.TableId;
                ObservableCollection<ItemInvoice> itemsInvoice = new ObservableCollection<ItemInvoice>(paySaleViewModel.SaleViewModel.ItemsInvoice);

                InvoiceDB invoiceDB = new InvoiceDB()
                {
                    Id = Guid.NewGuid().ToString(),
                    Cashier = paySaleViewModel.SaleViewModel.LoggedCashier.Id,
                    InvoiceType = 0,
                    TransactionType = 0,
                    SdcDateTime = DateTime.Now,
                    TotalAmount = total,
                };
                using (var dbContext = paySaleViewModel.DbContext.CreateDbContext())
                {
                    dbContext.Add(invoiceDB);

                    await dbContext.SaveChangesAsync();

                    int itemInvoiceId = 0;
                    List<ItemInvoiceDB> itemsInvoiceDB = new List<ItemInvoiceDB>();
                    items.ForEach(item =>
                    {
                        ItemDB? itemDB = dbContext.Items.Find(item.Id);
                        if (itemDB != null)
                        {
                            itemsInvoiceDB.Add(new ItemInvoiceDB()
                            {
                                Id = itemInvoiceId++,
                                Quantity = item.Quantity,
                                TotalAmout = item.TotalAmount,
                                Label = item.Label,
                                Name = item.Name,
                                UnitPrice = item.UnitPrice,
                                ItemCode = item.Id
                            });
                        }
                    });

                    itemsInvoiceDB.ForEach(itemInvoice =>
                    {
                        itemInvoice.InvoiceId = invoiceDB.Id;

                        dbContext.Add(itemInvoice);
                    });
                    await dbContext.SaveChangesAsync();

                    // await TakingDownNorm(invoiceDB, paySaleViewModel);
                    await TakingDownOrder(invoiceDB, paySaleViewModel, tableId, itemsInvoice, cashierId);

                    await Task.Run(() =>
                    {
                        try
                        {
                            PrinterManager.Instance.PrintJournal(invoiceRequset); //stampa crnog
                        }
                        catch (Exception ex)
                        {
                            Log.Error("GRESKA PRILIKOM STAMPE CRNOG RACUNA");
                        }
                    });

                    if (SettingsManager.Instance.EnableSmartCard())
                    {
                        paySaleViewModel.SaleViewModel.LogoutCommand.Execute(true);
                    }
                    else
                    {
                        paySaleViewModel.SaleViewModel.Reset();

                        var typeApp = SettingsManager.Instance.GetTypeApp();

                        if (typeApp == TypeAppEnumeration.Sale)
                        {
                            AppStateParameter appStateParameter = new AppStateParameter(AppStateEnumerable.Sale,
                            paySaleViewModel.SaleViewModel.LoggedCashier,
                            tableId,
                            paySaleViewModel.SaleViewModel);
                            paySaleViewModel.SaleViewModel.UpdateAppViewModelCommand.Execute(appStateParameter);
                        }
                        else
                        {
                            AppStateParameter appStateParameter = new AppStateParameter(AppStateEnumerable.TableOverview,
                            paySaleViewModel.SaleViewModel.LoggedCashier,
                            tableId,
                            paySaleViewModel.SaleViewModel);
                            paySaleViewModel.SaleViewModel.UpdateAppViewModelCommand.Execute(appStateParameter);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Log.Error("DESILA SE GRESKA KOD CRNOG RACUNA: ", ex);
                MessageBox.Show("GREŠKA PRILIKOM IZDAVANJA RAČUNA!", "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void Normal(InvoceRequestFileSystemWatcher invoiceRequset,
            PaySaleViewModel paySaleViewModel,
            decimal total,
            List<ItemFileSystemWatcher> items,
            decimal popust,
            CashierDB cashierDB)
        {
            string json = JsonConvert.SerializeObject(invoiceRequset);

            string? inDirectory = SettingsManager.Instance.GetInDirectory();

            if (string.IsNullOrEmpty(inDirectory))
            {
                MessageBox.Show("Putanja do ulaznog foldera nije setovana! Račun ne moze da se fiskalizuje.",
                    "Ulazni folder",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }
            else
            {
                DateTime dateTimeOfIssue = DateTime.Now;

                string invoiceName = $"Invoice_{dateTimeOfIssue.ToString("dd-MM-yyyy HH-mm-ss")}.json";
                string jsonPath = System.IO.Path.Combine(inDirectory, invoiceName);

                File.WriteAllText(jsonPath, json);

                int tableId = paySaleViewModel.SaleViewModel.TableId;
                string cashierId = cashierDB.Id;
                ObservableCollection<ItemInvoice> itemsInvoice = new ObservableCollection<ItemInvoice>(paySaleViewModel.SaleViewModel.ItemsInvoice);

                InvoiceDB invoiceDB = await InsertInvoiceInDB(invoiceRequset,
                    items,
                    paySaleViewModel,
                    dateTimeOfIssue);

                await Task.Run(() =>
                {
                    string? outDirectory = SettingsManager.Instance.GetOutDirectory();

                    if (!string.IsNullOrEmpty(outDirectory))
                    {
                        string jsonOutPath = System.IO.Path.Combine(outDirectory, invoiceName);

                        _timer = DateTime.Now;
                        while (IsFileLocked(jsonOutPath)) ;

                        try
                        {
                            using (StreamReader r = new StreamReader(jsonOutPath))
                            {
                                string response = r.ReadToEnd();

                                ResponseJson? responseJson = JsonConvert.DeserializeObject<ResponseJson>(response);

                                if (responseJson != null)
                                {
                                    if (responseJson.Message.Contains("Uspešna fiskalizacija"))
                                    {
                                        try
                                        {
                                            SaveToDB(invoiceRequset,
                                                items,
                                                total,
                                                paySaleViewModel,
                                                tableId,
                                                itemsInvoice,
                                                responseJson,
                                                invoiceDB,
                                                cashierId);
                                        }
                                        catch
                                        {
                                            MessageBox.Show("GREŠKA PRILIKOM UPISA U BAZU!", "", MessageBoxButton.OK, MessageBoxImage.Error);
                                        }
                                    }
                                    else
                                    {
                                        MessageBox.Show("PROVERITE ESIR I LPFR!", "", MessageBoxButton.OK, MessageBoxImage.Error);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error("PayCommand -> FinisedSale -> ", ex);
                            MessageBox.Show("GREŠKA U PROVERI IZLAZNOG FAJLA!\nPROVERITE DA LI JE ESIR POKRENUT", "", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                });

                //paySaleViewModel.SaleViewModel.TableOverviewViewModel.CheckDatabaseStatusStolova(this, null);

                paySaleViewModel.SaleViewModel.SendToDisplay("* * * HVALA * * *");
                if (SettingsManager.Instance.EnableSmartCard())
                {
                    paySaleViewModel.SaleViewModel.LogoutCommand.Execute(true);
                }
                else
                {
                    paySaleViewModel.SaleViewModel.Reset();

                    var typeApp = SettingsManager.Instance.GetTypeApp();

                    if (typeApp == TypeAppEnumeration.Sale)
                    {
                        AppStateParameter appStateParameter = new AppStateParameter(AppStateEnumerable.Sale,
                        paySaleViewModel.SaleViewModel.LoggedCashier,
                        tableId,
                        paySaleViewModel.SaleViewModel);
                        paySaleViewModel.SaleViewModel.UpdateAppViewModelCommand.Execute(appStateParameter);
                    }
                    else
                    {
                        AppStateParameter appStateParameter = new AppStateParameter(AppStateEnumerable.TableOverview,
                        paySaleViewModel.SaleViewModel.LoggedCashier,
                        tableId,
                        paySaleViewModel.SaleViewModel);
                        paySaleViewModel.SaleViewModel.UpdateAppViewModelCommand.Execute(appStateParameter);
                    }
                }
            }
        }
        private async void PrintOrder(CashierDB cashierDB,
            List<ItemInvoice> items,
            PaySaleViewModel paySaleViewModel)
        {
            DateTime orderTime = DateTime.Now;

            ClickBar_Common.Models.Order.Order orderKuhinja = new ClickBar_Common.Models.Order.Order()
            {
                CashierName = cashierDB.Name,
                TableId = 0,
                Items = new List<ClickBar_Common.Models.Order.ItemOrder>(),
                OrderTime = orderTime,
                PartHall = "Za poneti",
                OrderName = "K"
            };
            ClickBar_Common.Models.Order.Order orderSank = new ClickBar_Common.Models.Order.Order()
            {
                CashierName = cashierDB.Name,
                TableId = 0,
                Items = new List<ClickBar_Common.Models.Order.ItemOrder>(),
                OrderTime = orderTime,
                PartHall = "Za poneti",
                OrderName = "S"
            };
            ClickBar_Common.Models.Order.Order orderDrugo = new ClickBar_Common.Models.Order.Order()
            {
                CashierName = cashierDB.Name,
                TableId = 0,
                Items = new List<ClickBar_Common.Models.Order.ItemOrder>(),
                OrderTime = orderTime,
                PartHall = "Za poneti"
            };
            using (var dbContext = paySaleViewModel.DbContext.CreateDbContext())
            {
                items.ForEach(item =>
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
                        if (item.Zelje.FirstOrDefault(f => !string.IsNullOrEmpty(f.Description)) != null)
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

                            orderKuhinja.Items.Add(new ClickBar_Common.Models.Order.ItemOrder()
                            {
                                Name = item.Item.Name,
                                Quantity = quantity,
                                Id = item.Item.Id,
                                TotalAmount = decimal.Round(item.Item.SellingUnitPrice * quantity, 2)
                            });
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

                int orderCounterTotal = 1;

                var ordersTodayTotalDB = dbContext.OrdersToday.Where(o => o.OrderDateTime.Date == DateTime.Now.Date);

                if (ordersTodayTotalDB != null &&
                    ordersTodayTotalDB.Any())
                {
                    orderCounterTotal = ordersTodayTotalDB.Max(o => o.Counter);
                    orderCounterTotal++;
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

                    orderSank.OrderName += orderCounterType.ToString() + "__" + orderCounterTotal.ToString();

                    decimal totalAmount = orderSank.Items.Sum(i => i.TotalAmount);

                    OrderTodayDB orderTodayDB = new OrderTodayDB()
                    {
                        Id = Guid.NewGuid().ToString(),
                        CashierId = cashierDB.Id,
                        CounterType = orderCounterType,
                        Counter = orderCounterTotal,
                        OrderDateTime = DateTime.Now,
                        TotalPrice = totalAmount,
                        Name = orderSank.OrderName,
                        TableId = 0,
                    };

                    dbContext.OrdersToday.Add(orderTodayDB);
                    await dbContext.SaveChangesAsync();

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
                        dbContext.OrderTodayItems.Add(orderTodayItemDB);
                    });
                    await dbContext.SaveChangesAsync();

                    FormatPos.PrintOrder(orderSank, posType, ClickBar_Printer.Enums.OrderTypeEnumeration.Sank);
                }
                if (orderKuhinja.Items.Any())
                {
                    int orderCounter = 1;

                    var ordersTodayDB = dbContext.OrdersToday.Where(o => o.OrderDateTime.Date == DateTime.Now.Date &&
                    !string.IsNullOrEmpty(o.Name) &&
                    o.Name.ToLower().Contains(orderKuhinja.OrderName.ToLower()));

                    if (ordersTodayDB != null &&
                        ordersTodayDB.Any())
                    {
                        orderCounter = ordersTodayDB.Max(o => o.CounterType);
                        orderCounter++;
                    }

                    orderKuhinja.OrderName += orderCounter.ToString() + "__" + orderCounterTotal.ToString();

                    decimal totalAmount = orderKuhinja.Items.Sum(i => i.TotalAmount);

                    OrderTodayDB orderTodayDB = new OrderTodayDB()
                    {
                        Id = Guid.NewGuid().ToString(),
                        CashierId = cashierDB.Id,
                        CounterType = orderCounter,
                        Counter = orderCounterTotal,
                        OrderDateTime = DateTime.Now,
                        TotalPrice = totalAmount,
                        Name = orderKuhinja.OrderName,
                        TableId = 0
                    };

                    dbContext.OrdersToday.Add(orderTodayDB);
                    await dbContext.SaveChangesAsync();

                    orderKuhinja.Items.ForEach(async item =>
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
                            dbContext.OrderTodayItems.Add(orderTodayItemDB);
                        }
                        await dbContext.SaveChangesAsync();
                    });

                    FormatPos.PrintOrder(orderKuhinja, posType, ClickBar_Printer.Enums.OrderTypeEnumeration.Kuhinja);
                }
                if (orderDrugo.Items.Any())
                {
                    int orderCounter = 1;

                    var ordersTodayDB = dbContext.OrdersToday.Where(o => o.OrderDateTime.Date == DateTime.Now.Date &&
                    !string.IsNullOrEmpty(o.Name) &&
                    o.Name.ToLower().Contains(orderDrugo.OrderName.ToLower()));

                    if (ordersTodayDB != null &&
                        ordersTodayDB.Any())
                    {
                        orderCounter = ordersTodayDB.Max(o => o.Counter);
                        orderCounter++;
                    }

                    orderDrugo.OrderName += orderCounter.ToString() + "__" + orderCounterTotal.ToString();

                    decimal totalAmount = orderDrugo.Items.Sum(i => i.TotalAmount);

                    OrderTodayDB orderTodayDB = new OrderTodayDB()
                    {
                        Id = Guid.NewGuid().ToString(),
                        CashierId = cashierDB.Id,
                        CounterType = orderCounter,
                        Counter = orderCounterTotal,
                        OrderDateTime = DateTime.Now,
                        TotalPrice = totalAmount,
                        Name = orderDrugo.OrderName,
                        TableId = 0
                    };

                    dbContext.OrdersToday.Add(orderTodayDB);
                    await dbContext.SaveChangesAsync();

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
                        dbContext.OrderTodayItems.Add(orderTodayItemDB);
                    });
                    await dbContext.SaveChangesAsync();

                    FormatPos.PrintOrder(orderDrugo, posType, ClickBar_Printer.Enums.OrderTypeEnumeration.Sank);
                }
            }
        }
        private bool IsFileLocked(string file)
        {
            if (File.Exists(file))
            {
                FileStream stream = null;
                try
                {
                    stream = File.Open(file, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                }
                catch (Exception ex2)
                {
                    int errorCode = Marshal.GetHRForException(ex2) & ((1 << 16) - 1);
                    if ((ex2 is IOException) && (errorCode == ERROR_SHARING_VIOLATION || errorCode == ERROR_LOCK_VIOLATION))
                    {
                        return true;
                    }
                }
                finally
                {
                    if (stream != null)
                        stream.Close();
                }
            }
            else
            {
                if (DateTime.Now.Subtract(_timer).TotalSeconds > 15)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            return false;
        }
    }
}
