using ClickBar.Converters;
using ClickBar.Enums;
using ClickBar.Enums.AppMain.Statistic;
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
using ClickBar_Database_Drlja;
using ClickBar_Logging;
using ClickBar_Printer;
using ClickBar_Printer.PaperFormat;
using ClickBar_Settings;
using DocumentFormat.OpenXml.Vml;
using Microsoft.EntityFrameworkCore;
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
using System.Windows.Input;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using MessageBox = System.Windows.MessageBox;

namespace ClickBar.Commands.Sale
{
    public class PayCommand<TViewModel> : ICommand where TViewModel : ViewModelBase
    {
        public event EventHandler CanExecuteChanged;

        private TViewModel _viewModel;

        private const int ERROR_SHARING_VIOLATION = 32;
        private const int ERROR_LOCK_VIOLATION = 33;

        private DateTime _timer;
        private List<Payment> _payment;

        public PayCommand(TViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }
        public async void Execute(object parameter)
        {
            if (_viewModel is SaleViewModel saleViewModel)
            {
                if (saleViewModel.OldItemsInvoice.Any() ||
                    saleViewModel.ItemsInvoice.Any())
                {
                    if (saleViewModel.OldItemsInvoice.Any())
                    {
                        saleViewModel.OldItemsInvoice.ToList().ForEach(item =>
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
                        });
                    }

                    PaySaleWindow paySaleWindow = new PaySaleWindow(saleViewModel);// _serviceProvider.GetRequiredService<PaySaleWindow>();
                    paySaleWindow.ShowDialog();

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

                    var partnerDB = paySaleViewModel.DbContext.Partners.AsNoTracking().FirstOrDefault(partner => partner.Pib == paySaleViewModel.BuyerId);
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
                            paySaleViewModel.DbContext.Partners.Add(partnerDB);
                            paySaleViewModel.DbContext.SaveChanges();
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

                await FinisedSale(paySaleViewModel, popust);

                paySaleViewModel.Window.Close();
            }
            else if (_viewModel is SplitOrderViewModel splitOrderViewModel)
            {
                splitOrderViewModel.PaySaleViewModel.SplitOrderWindow.Close();

                if (splitOrderViewModel.ItemsInvoiceForPay.Any())
                {
                    splitOrderViewModel.PaySaleViewModel.ItemsInvoice = splitOrderViewModel.ItemsInvoiceForPay;
                    splitOrderViewModel.PaySaleViewModel.TotalAmount = splitOrderViewModel.TotalAmountForPay;
                }

                if (splitOrderViewModel.ItemsInvoice.Any())
                {
                    splitOrderViewModel.PaySaleViewModel.SaleViewModel.ItemsInvoice = splitOrderViewModel.ItemsInvoice;
                    splitOrderViewModel.PaySaleViewModel.SaleViewModel.TotalAmount = splitOrderViewModel.TotalAmount;
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
        private async Task FinisedSale(PaySaleViewModel paySaleViewModel, decimal popust)
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

#if CRNO
            paySaleViewModel.ItemsInvoice.ToList().ForEach(item =>
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
            });
            invoiceRequset.Items = items;
            Black(invoiceRequset, paySaleViewModel, total, items);
#else
            paySaleViewModel.ItemsInvoice.ToList().ForEach(item =>
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
            });
            invoiceRequset.Items = items;
            Normal(invoiceRequset, paySaleViewModel, total, items, popust, paySaleViewModel.SaleViewModel.LoggedCashier);
#endif
        }
        private async Task TakingDownOrder(InvoiceDB invoice,
            PaySaleViewModel paySaleViewModel,
            int tableId,
            ObservableCollection<ItemInvoice> items)
        {
            try
            {
                var unprocessedOrders = paySaleViewModel.DbContext.UnprocessedOrders.FirstOrDefault(order => order.PaymentPlaceId == tableId);

                if (unprocessedOrders != null)
                {
                    OrderDB orderDB = new OrderDB()
                    {
                        InvoiceId = invoice.Id,
                        PaymentPlaceId = tableId,
                        CashierId = paySaleViewModel.SaleViewModel.LoggedCashier.Id
                    };
                    paySaleViewModel.DbContext.Orders.Add(orderDB);

                    var itemsInUnprocessedOrder = paySaleViewModel.DbContext.ItemsInUnprocessedOrder.AsNoTracking().Where(item => item.UnprocessedOrderId == unprocessedOrders.Id);

                    //decimal totalAmount = unprocessedOrders.TotalAmount;
                    if (itemsInUnprocessedOrder != null &&
                        itemsInUnprocessedOrder.Any())
                    {
                        if (itemsInUnprocessedOrder.Sum(a => a.Quantity) == invoice.ItemInvoices.Where(a => a.IsSirovina == null ||
                        a.IsSirovina == 0).Sum(b => b.Quantity))
                        {
                            paySaleViewModel.DbContext.ItemsInUnprocessedOrder.RemoveRange(itemsInUnprocessedOrder);
                            paySaleViewModel.DbContext.SaveChanges();
                        }
                        else
                        {
                            await itemsInUnprocessedOrder.ForEachAsync(item =>
                            {
                                var invoiceItem = invoice.ItemInvoices.FirstOrDefault(itemInvoice => itemInvoice.ItemCode == item.ItemId);

                                if (invoiceItem != null)
                                {
                                    if (invoiceItem.Quantity.HasValue &&
                                    invoiceItem.TotalAmout.HasValue &&
                                    invoiceItem.UnitPrice.HasValue)
                                    {
                                        decimal ta = 0;
                                        if (item.Quantity <= invoiceItem.Quantity.Value)
                                        {
                                            ta = item.Quantity * invoiceItem.UnitPrice.Value;
                                            paySaleViewModel.DbContext.ItemsInUnprocessedOrder.Remove(item);

                                            Log.Debug($"PayCommand -> TakingDownOrder -> obrisan je artikal sa stola_{unprocessedOrders.PaymentPlaceId} - {item.ItemId}: {item.Quantity}  {ta} din");
                                        }
                                        else if (item.Quantity > invoiceItem.Quantity.Value)
                                        {
                                            Log.Debug($"PayCommand -> TakingDownOrder -> update artikla {item.ItemId}: stara vrednost: {item.Quantity} -> nova vrednost: {invoiceItem.Quantity.Value}");
                                            item.Quantity -= invoiceItem.Quantity.Value;
                                            ta = invoiceItem.TotalAmout.Value;
                                            paySaleViewModel.DbContext.ItemsInUnprocessedOrder.Update(item);

                                            Log.Debug($"PayCommand -> TakingDownOrder -> update je artikal sa stola_{unprocessedOrders.PaymentPlaceId} - {item.ItemId}: {item.Quantity}  {ta} din");
                                        }
                                        unprocessedOrders.TotalAmount -= ta;

                                        paySaleViewModel.DbContext.UnprocessedOrders.Update(unprocessedOrders);
                                        paySaleViewModel.DbContext.SaveChanges();
                                    }
                                }
                            });
                        }
                    }

                    itemsInUnprocessedOrder = paySaleViewModel.DbContext.ItemsInUnprocessedOrder.AsNoTracking().Where(item => item.UnprocessedOrderId == unprocessedOrders.Id);

                    if (itemsInUnprocessedOrder != null &&
                        itemsInUnprocessedOrder.Any())
                    {
                        Log.Debug($"PayCommand -> TakingDownOrder -> Preostali iznos na stolu {unprocessedOrders.PaymentPlaceId} je: {unprocessedOrders.TotalAmount}");
                    }
                    else
                    {
                        paySaleViewModel.DbContext.UnprocessedOrders.Remove(unprocessedOrders);

                        var pathToDrljaDB = SettingsManager.Instance.GetPathToDrljaKuhinjaDB();

                        if (!string.IsNullOrEmpty(pathToDrljaDB))
                        {
                            var narudzbineDrlja = paySaleViewModel.DrljaDbContext.Narudzbine.AsNoTracking().Where(nar => nar.TR_STO.Contains(tableId.ToString())
                                    && nar.TR_FAZA != 4);
                            if (narudzbineDrlja != null &&
                                narudzbineDrlja.Any())
                            {
                                narudzbineDrlja.ToList().ForEach(narudzbinaDrlja =>
                                {
                                    narudzbinaDrlja.TR_FAZA = 4;
                                    paySaleViewModel.DrljaDbContext.Narudzbine.Update(narudzbinaDrlja);
                                    RetryHelperDrlja.ExecuteWithRetry(() => { paySaleViewModel.DrljaDbContext.SaveChanges(); });
                                });
                            }
                        }
                    }
                    paySaleViewModel.DbContext.SaveChanges();
                }
                else
                {
                    OrderDB orderDB = new OrderDB()
                    {
                        InvoiceId = invoice.Id,
                        PaymentPlaceId = tableId,
                        CashierId = paySaleViewModel.SaleViewModel.LoggedCashier.Id
                    };
                    paySaleViewModel.DbContext.Orders.Add(orderDB);
                    paySaleViewModel.DbContext.SaveChanges();
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

                paySaleViewModel.DbContext.Invoices.Update(invoiceDB);
                paySaleViewModel.DbContext.SaveChanges();

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

                        paySaleViewModel.DbContext.TaxItemInvoices.Add(taxItemInvoiceDB);
                    });
                }
                paySaleViewModel.DbContext.SaveChanges();
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

            paySaleViewModel.DbContext.Add(invoiceDB);
            paySaleViewModel.DbContext.SaveChanges();

            int itemInvoiceId = 0;
            paySaleViewModel.ItemsInvoice.ToList().ForEach(async item =>
            {
                ItemDB? itemDB = paySaleViewModel.DbContext.Items.AsNoTracking().FirstOrDefault(i => i.Id == item.Item.Id);
                if (itemDB != null)
                {
                    if (itemDB.IdNorm != null)
                    {
                        var norms = paySaleViewModel.DbContext.ItemsInNorm.AsNoTracking().Where(norm => norm.IdNorm == itemDB.IdNorm);

                        if (norms != null &&
                            norms.Any())
                        {
                            await norms.ForEachAsync(async norm =>
                            {
                                var normItem = paySaleViewModel.DbContext.Items.AsNoTracking().FirstOrDefault(i => i.Id == norm.IdItem);
                                if (normItem != null)
                                {
                                    if (normItem.IdNorm != null)
                                    {
                                        var norms2 = paySaleViewModel.DbContext.ItemsInNorm.AsNoTracking().Where(norm => norm.IdNorm == normItem.IdNorm);

                                        if (norms2 != null &&
                                            norms2.Any())
                                        {
                                            await norms2.ForEachAsync(async norm2 =>
                                            {
                                                var normItem2 = paySaleViewModel.DbContext.Items.AsNoTracking().FirstOrDefault(i => i.Id == norm2.IdItem);

                                                if (normItem2 != null)
                                                {
                                                    if (normItem2.IdNorm != null)
                                                    {
                                                        var norms3 = paySaleViewModel.DbContext.ItemsInNorm.AsNoTracking().Where(norm => norm.IdNorm == normItem2.IdNorm);
                                                        if (norms3 != null &&
                                                            norms3.Any())
                                                        {
                                                            await norms3.ForEachAsync(norm3 =>
                                                            {
                                                                var normItem3 = paySaleViewModel.DbContext.Items.AsNoTracking().FirstOrDefault(i => i.Id == norm3.IdItem);
                                                                if (normItem3 != null)
                                                                {
                                                                    decimal unitPrice = normItem3.InputUnitPrice != null && normItem3.InputUnitPrice.HasValue ?
                                                                    normItem3.InputUnitPrice.Value : 0;

                                                                    var itemInvoice = new ItemInvoiceDB()
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
                                                                    paySaleViewModel.DbContext.Add(itemInvoice);
                                                                }
                                                            });
                                                        }
                                                    }
                                                    else
                                                    {
                                                        decimal unitPrice = normItem2.InputUnitPrice != null && normItem2.InputUnitPrice.HasValue ?
                                                        normItem2.InputUnitPrice.Value : 0;

                                                        var itemInvoice = new ItemInvoiceDB()
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
                                                        paySaleViewModel.DbContext.Add(itemInvoice);
                                                    }
                                                }
                                            });
                                        }
                                        else
                                        {
                                            decimal unitPrice = normItem.InputUnitPrice != null && normItem.InputUnitPrice.HasValue ?
                                            normItem.InputUnitPrice.Value : 0;

                                            var itemInvoice = new ItemInvoiceDB()
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
                                            paySaleViewModel.DbContext.Add(itemInvoice);
                                        }
                                    }
                                    else
                                    {
                                        decimal unitPrice = normItem.InputUnitPrice != null && normItem.InputUnitPrice.HasValue ?
                                        normItem.InputUnitPrice.Value : 0;

                                        var itemInvoice = new ItemInvoiceDB()
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
                                        paySaleViewModel.DbContext.Add(itemInvoice);
                                    }
                                }
                            });
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
                    paySaleViewModel.DbContext.Add(itemInvoice);
                }
            });

            _payment.ForEach(payment =>
            {
                PaymentInvoiceDB paymentInvoice = new PaymentInvoiceDB()
                {
                    InvoiceId = invoiceDB.Id,
                    Amout = payment.Amount,
                    PaymentType = payment.PaymentType
                };

                paySaleViewModel.DbContext.PaymentInvoices.Add(paymentInvoice);
            });
            paySaleViewModel.DbContext.SaveChanges();

            return invoiceDB;
        }
        private async Task TakingDownNorm(InvoiceDB invoice,
            PaySaleViewModel paySaleViewModel)
        {
            try
            {
                List<ItemDB> itemsForCondition = new List<ItemDB>();

                var itemsInInvoice = paySaleViewModel.DbContext.ItemInvoices.AsNoTracking().Where(item => item.IsSirovina == 0);

                if (itemsInInvoice != null &&
                    itemsInInvoice.Any())
                {
                    itemsInInvoice.ToList().ForEach(async item =>
                    {
                        var it = paySaleViewModel.DbContext.Items.AsNoTracking().FirstOrDefault(i => i.Id == item.ItemCode);
                        if (it != null &&
                        item.Quantity.HasValue)
                        {
                            var itemInNorm = paySaleViewModel.DbContext.ItemsInNorm.AsNoTracking().Where(norm => it.IdNorm == norm.IdNorm);

                            if (itemInNorm != null &&
                            itemInNorm.Any())
                            {
                                await itemInNorm.ForEachAsync(norm =>
                                {
                                    var itm = paySaleViewModel.DbContext.Items.AsNoTracking().FirstOrDefault(i => i.Id == norm.IdItem);

                                    if (itm != null)
                                    {
                                        if (itm.IdNorm == null)
                                        {
                                            itm.TotalQuantity -= item.Quantity.Value * norm.Quantity;
                                            paySaleViewModel.DbContext.Items.Update(itm);
                                        }
                                        else
                                        {
                                            var itemInNorm2 = paySaleViewModel.DbContext.ItemsInNorm.AsNoTracking().Where(norm => itm.IdNorm == norm.IdNorm);
                                            if (itemInNorm2.Any())
                                            {
                                                itemInNorm2.ToList().ForEach(norm2 =>
                                                {
                                                    var itm2 = paySaleViewModel.DbContext.Items.AsNoTracking().FirstOrDefault(i => i.Id == norm2.IdItem);

                                                    if (itm2 != null)
                                                    {
                                                        if (itm2.IdNorm == null)
                                                        {
                                                            itm2.TotalQuantity -= item.Quantity.Value * norm.Quantity * norm2.Quantity;
                                                            paySaleViewModel.DbContext.Items.Update(itm2);
                                                        }
                                                        else
                                                        {
                                                            var itemInNorm3 = paySaleViewModel.DbContext.ItemsInNorm.AsNoTracking().Where(norm => itm2.IdNorm == norm2.IdNorm);
                                                            if (itemInNorm3.Any())
                                                            {
                                                                itemInNorm3.ToList().ForEach(norm3 =>
                                                                {
                                                                    var itm3 = paySaleViewModel.DbContext.Items.AsNoTracking().FirstOrDefault(i => i.Id == norm3.IdItem);

                                                                    if (itm3 != null)
                                                                    {
                                                                        if (itm3.IdNorm == null)
                                                                        {
                                                                            itm3.TotalQuantity -= item.Quantity.Value * norm.Quantity * norm2.Quantity * norm3.Quantity;
                                                                            paySaleViewModel.DbContext.Items.Update(itm3);
                                                                        }
                                                                    }
                                                                });
                                                            }
                                                            else
                                                            {
                                                                itm2.TotalQuantity -= item.Quantity.Value * norm.Quantity * norm2.Quantity;
                                                                paySaleViewModel.DbContext.Items.Update(itm2);
                                                            }
                                                        }
                                                    }
                                                });
                                            }
                                            else
                                            {
                                                itm.TotalQuantity -= item.Quantity.Value * norm.Quantity;
                                                paySaleViewModel.DbContext.Items.Update(itm);
                                            }
                                        }
                                    }
                                });
                            }
                            else
                            {
                                it.TotalQuantity -= item.Quantity.Value;
                                paySaleViewModel.DbContext.Items.Update(it);
                            }
                        }
                    });

                    paySaleViewModel.DbContext.SaveChanges();
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
            InvoiceDB invoiceDB)
        {
            await UpdateInvode(invoiceDB, total, responseJson, paySaleViewModel);
            await TakingDownNorm(invoiceDB, paySaleViewModel);
            await TakingDownOrder(invoiceDB, paySaleViewModel, tableId, itemsInvoice);
        }
        private void Black(InvoceRequestFileSystemWatcher invoiceRequset,
            PaySaleViewModel paySaleViewModel,
            decimal total,
            List<ItemFileSystemWatcher> items)
        {
            Task.Run(() =>
            {
                try
                {
                    PrinterManager.Instance.PrintJournal(invoiceRequset);
                }
                catch (Exception ex)
                {
                    Log.Error("GRESKA PRILIKOM STAMPE CRNOG RACUNA");
                }
            });

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
            paySaleViewModel.DbContext.Add(invoiceDB);

            paySaleViewModel.DbContext.SaveChanges();

            int itemInvoiceId = 0;
            List<ItemInvoiceDB> itemsInvoiceDB = new List<ItemInvoiceDB>();
            items.ForEach(item =>
            {
                ItemDB? itemDB = paySaleViewModel.DbContext.Items.Find(item.Id);
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

                paySaleViewModel.DbContext.Add(itemInvoice);
            });
            paySaleViewModel.DbContext.SaveChanges();

            TakingDownOrder(invoiceDB, paySaleViewModel, paySaleViewModel.SaleViewModel.TableId, itemsInvoice);

            if (SettingsManager.Instance.EnableSmartCard())
            {
                paySaleViewModel.SaleViewModel.LogoutCommand.Execute(true);
            }
            else
            {
                paySaleViewModel.SaleViewModel.Reset();

                AppStateParameter appStateParameter = new AppStateParameter(paySaleViewModel.DbContext, 
                    paySaleViewModel.DrljaDbContext,
                    AppStateEnumerable.TableOverview,
                    paySaleViewModel.SaleViewModel.LoggedCashier,
                    paySaleViewModel.SaleViewModel);
                paySaleViewModel.SaleViewModel.UpdateAppViewModelCommand.Execute(appStateParameter);
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
                ObservableCollection<ItemInvoice> itemsInvoice = new ObservableCollection<ItemInvoice>(paySaleViewModel.SaleViewModel.ItemsInvoice);

                InvoiceDB invoiceDB = await InsertInvoiceInDB(invoiceRequset,
                    items,
                    paySaleViewModel,
                    dateTimeOfIssue);

                _ = Task.Run(() =>
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
                                                invoiceDB);
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

                paySaleViewModel.SaleViewModel.SendToDisplay("* * * HVALA * * *");
                if (SettingsManager.Instance.EnableSmartCard())
                {
                    paySaleViewModel.SaleViewModel.LogoutCommand.Execute(true);
                }
                else
                {
                    paySaleViewModel.SaleViewModel.Reset();

                    AppStateParameter appStateParameter = new AppStateParameter(paySaleViewModel.DbContext,
                        paySaleViewModel.DrljaDbContext,
                        AppStateEnumerable.TableOverview,
                        paySaleViewModel.SaleViewModel.LoggedCashier,
                        paySaleViewModel.SaleViewModel);
                    paySaleViewModel.SaleViewModel.UpdateAppViewModelCommand.Execute(appStateParameter);
                }
            }
        }
        private void PrintOrder(CashierDB cashierDB, 
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

            items.ForEach(item =>
            {
                var itemNadgroup = paySaleViewModel.DbContext.Items.Join(paySaleViewModel.DbContext.ItemGroups,
                item => item.IdItemGroup,
                itemGroup => itemGroup.Id,
                (item, itemGroup) => new { Item = item, ItemGroup = itemGroup })
                .Join(paySaleViewModel.DbContext.Supergroups,
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

            var ordersTodayTotalDB = paySaleViewModel.DbContext.OrdersToday.Where(o => o.OrderDateTime.Date == DateTime.Now.Date);

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

                var ordersTodayTypeDB = paySaleViewModel.DbContext.OrdersToday.Where(o => o.OrderDateTime.Date == DateTime.Now.Date &&
                !string.IsNullOrEmpty(o.Name) &&
                o.Name.ToLower().Contains(orderSank.OrderName.ToLower()));

                if (ordersTodayTypeDB != null &&
                    ordersTodayTypeDB.Any())
                {
                    orderCounterType = ordersTodayTypeDB.Max(o => o.CounterType);
                    orderCounterType++;
                }

                orderSank.OrderName += orderCounterType.ToString() + "_" + orderCounterTotal.ToString();

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

                paySaleViewModel.DbContext.OrdersToday.Add(orderTodayDB);
                paySaleViewModel.DbContext.SaveChanges();

                orderSank.Items.ForEach(item =>
                {
                    OrderTodayItemDB orderTodayItemDB = new OrderTodayItemDB()
                    {
                        ItemId = item.Id,
                        OrderTodayId = orderTodayDB.Id,
                        Quantity = item.Quantity,
                        TotalPrice = item.TotalAmount,
                    };
                    paySaleViewModel.DbContext.OrderTodayItems.Add(orderTodayItemDB);
                });
                paySaleViewModel.DbContext.SaveChanges();

                FormatPos.PrintOrder(orderSank, posType, ClickBar_Printer.Enums.OrderTypeEnumeration.Sank);
            }
            if (orderKuhinja.Items.Any())
            {
                int orderCounter = 1;

                var ordersTodayDB = paySaleViewModel.DbContext.OrdersToday.Where(o => o.OrderDateTime.Date == DateTime.Now.Date &&
                !string.IsNullOrEmpty(o.Name) &&
                o.Name.ToLower().Contains(orderKuhinja.OrderName.ToLower()));

                if (ordersTodayDB != null &&
                    ordersTodayDB.Any())
                {
                    orderCounter = ordersTodayDB.Max(o => o.CounterType);
                    orderCounter++;
                }

                orderKuhinja.OrderName += orderCounter.ToString() + "_" + orderCounterTotal.ToString();

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

                paySaleViewModel.DbContext.OrdersToday.Add(orderTodayDB);
                paySaleViewModel.DbContext.SaveChanges();

                orderKuhinja.Items.ForEach(item =>
                {
                    var orderTodayItemDB = paySaleViewModel.DbContext.OrderTodayItems.FirstOrDefault(o => o.ItemId == item.Id &&
                    o.OrderTodayId == orderTodayDB.Id);

                    if (orderTodayItemDB != null)
                    {
                        orderTodayItemDB.Quantity += item.Quantity;
                        orderTodayItemDB.TotalPrice += item.TotalAmount;
                        paySaleViewModel.DbContext.OrderTodayItems.Update(orderTodayItemDB);
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
                        paySaleViewModel.DbContext.OrderTodayItems.Add(orderTodayItemDB);
                    }
                    paySaleViewModel.DbContext.SaveChanges();
                });

                FormatPos.PrintOrder(orderKuhinja, posType, ClickBar_Printer.Enums.OrderTypeEnumeration.Kuhinja);
            }
            if (orderDrugo.Items.Any())
            {
                int orderCounter = 1;

                var ordersTodayDB = paySaleViewModel.DbContext.OrdersToday.Where(o => o.OrderDateTime.Date == DateTime.Now.Date &&
                !string.IsNullOrEmpty(o.Name) &&
                o.Name.ToLower().Contains(orderDrugo.OrderName.ToLower()));

                if (ordersTodayDB != null &&
                    ordersTodayDB.Any())
                {
                    orderCounter = ordersTodayDB.Max(o => o.Counter);
                    orderCounter++;
                }

                orderDrugo.OrderName += orderCounter.ToString() + "_" + orderCounterTotal.ToString();

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

                paySaleViewModel.DbContext.OrdersToday.Add(orderTodayDB);
                paySaleViewModel.DbContext.SaveChanges();

                orderDrugo.Items.ForEach(item =>
                {
                    OrderTodayItemDB orderTodayItemDB = new OrderTodayItemDB()
                    {
                        ItemId = item.Id,
                        OrderTodayId = orderTodayDB.Id,
                        Quantity = item.Quantity,
                        TotalPrice = item.TotalAmount,
                    };
                    paySaleViewModel.DbContext.OrderTodayItems.Add(orderTodayItemDB);
                });
                paySaleViewModel.DbContext.SaveChanges();

                FormatPos.PrintOrder(orderDrugo, posType, ClickBar_Printer.Enums.OrderTypeEnumeration.Sank);
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
