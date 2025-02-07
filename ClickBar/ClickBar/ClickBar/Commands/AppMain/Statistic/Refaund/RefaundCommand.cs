using ClickBar.Enums.Sale;
using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar.ViewModels;
using ClickBar_Common.Enums;
using ClickBar_Common.Models.Invoice.Tax;
using ClickBar_Common.Models.Invoice;
using ClickBar_Database.Models;
using ClickBar_Database;
using ClickBar_Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using ClickBar.ViewModels.Sale;
using ClickBar_Common.Models.Invoice.FileSystemWatcher;
using ClickBar.Views.AppMain.AuxiliaryWindows.Statistic.Refaund;
using System.Threading;

namespace ClickBar.Commands.AppMain.Statistic.Refaund
{
    public class RefaundCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ViewModelBase _currentViewModel;

        public RefaundCommand(ViewModelBase currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            if (_currentViewModel is RefaundViewModel)
            {
                if (parameter is string)
                {
                    bool isEfaktura = parameter.ToString().Contains("eFaktura");

                    if (isEfaktura)
                    {
                        Refaund(isEfaktura);
                    }
                }
                else
                {
                    Refaund();
                }
            }
            else if (_currentViewModel is PayRefaundViewModel)
            {
                PayRefaund();
            }
        }
        private async void Refaund(bool isEfaktura = false)
        {
            RefaundViewModel refaundViewModel = (RefaundViewModel)_currentViewModel;

            if (string.IsNullOrEmpty(refaundViewModel.RefNumber) ||
                string.IsNullOrEmpty(refaundViewModel.RefDateDay) ||
                string.IsNullOrEmpty(refaundViewModel.RefDateMonth) ||
                string.IsNullOrEmpty(refaundViewModel.RefDateYear) ||
                string.IsNullOrEmpty(refaundViewModel.RefDateHour) ||
                string.IsNullOrEmpty(refaundViewModel.RefDateMinute) ||
                string.IsNullOrEmpty(refaundViewModel.RefDateSecond) ||
                refaundViewModel.CurrentInvoice is null)
            {
                MessageBox.Show("Selektujte račun u tabeli!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                using (SqliteDbContext sqliteDbContext = new SqliteDbContext())
                {

                    var invoiceDB = await sqliteDbContext.Invoices.FindAsync(refaundViewModel.CurrentInvoice.Id);

                    if (invoiceDB is not null)
                    {
                        InvoceRequestFileSystemWatcher invoiceRequest = new InvoceRequestFileSystemWatcher()
                        {
                            Cashier = refaundViewModel.LoggedCashier.Name,
                            InvoiceType = invoiceDB.InvoiceType != null && invoiceDB.InvoiceType.HasValue ? (InvoiceTypeEenumeration)invoiceDB.InvoiceType.Value :
                            InvoiceTypeEenumeration.Normal,
                        };

                        //InvoiceRequest invoiceRequest = new InvoiceRequest()
                        //{
                        //    Cashier = invoiceDB.Cashier,
                        //    DateAndTimeOfIssue = invoice.DateAndTimeOfIssue,
                        //    BuyerId = invoice.BuyerId,
                        //    BuyerName = invoice.BuyerName,
                        //    BuyerAddress = invoice.BuyerAddress,
                        //    BuyerCostCenterId = invoice.BuyerCostCenterId,
                        //    InvoiceNumber = invoice.InvoiceNumber,
                        //    InvoiceType = invoice.InvoiceType,
                        //    ReferentDocumentDT = invoice.ReferentDocumentDT,
                        //    ReferentDocumentNumber = invoice.ReferentDocumentNumber,
                        //    TransactionType = invoice.TransactionType,
                        //    Options = new InlineModel()
                        //    {
                        //        OmitQRCodeGen = "1",
                        //        OmitTextualRepresentation = "1",
                        //    }
                        //};
                        List<ItemFileSystemWatcher> items = new List<ItemFileSystemWatcher>();
                        List<ItemInvoiceDB> itemsInvoice = await sqliteDbContext.GetAllItemsFromInvoice(invoiceDB.Id);

                        itemsInvoice.ForEach(item =>
                        {
                            if (item.TotalAmout > 0)
                            {
                                var itemDB = sqliteDbContext.Items.FirstOrDefault(i => i.Id == item.ItemCode);

                                if (itemDB != null &&
                                item.Quantity.HasValue &&
                                item.TotalAmout.HasValue &&
                                item.UnitPrice.HasValue)
                                {
                                    items.Add(new ItemFileSystemWatcher()
                                    {
                                        Name = item.Name,
                                        Jm = string.IsNullOrEmpty(itemDB.Jm) ? "kom" : itemDB.Jm,
                                        Quantity = item.Quantity.Value,
                                        TotalAmount = item.TotalAmout.Value,
                                        UnitPrice = item.UnitPrice.Value,
                                        Label = item.Label,
                                        Id = item.ItemCode,
                                    });
                                }
                            }
                        });
                        invoiceRequest.Items = items;

                        List<Payment> payments = new List<Payment>();

                        var paymentsIvoice = await sqliteDbContext.GetAllPaymentFromInvoice(invoiceDB.Id);
                        paymentsIvoice.ForEach(payment =>
                        {
                            if (payment.Amout.HasValue)
                            {
                                payments.Add(new Payment()
                                {
                                    Amount = payment.Amout.Value,
                                    PaymentType = payment.PaymentType
                                });
                            }
                        });
                        invoiceRequest.Payment = payments;

                        InvoiceResult invoiceResult = new InvoiceResult()
                        {
                            RequestedBy = invoiceDB.RequestedBy,
                            SignedBy = invoiceDB.SignedBy,
                            SdcDateTime = invoiceDB.SdcDateTime.Value,
                            InvoiceCounter = invoiceDB.InvoiceCounter,
                            InvoiceCounterExtension = invoiceDB.InvoiceCounterExtension,
                            InvoiceNumber = invoiceDB.InvoiceNumberResult,
                            TotalCounter = invoiceDB.TotalCounter,
                            TransactionTypeCounter = invoiceDB.TransactionTypeCounter,
                            TotalAmount = invoiceDB.TotalAmount,
                            EncryptedInternalData = invoiceDB.EncryptedInternalData,
                            Signature = invoiceDB.Signature,
                            BusinessName = invoiceDB.BusinessName,
                            LocationName = invoiceDB.LocationName,
                            Address = invoiceDB.Address,
                            Tin = invoiceDB.Tin,
                            District = invoiceDB.District,
                            TaxGroupRevision = invoiceDB.TaxGroupRevision,
                            Mrc = invoiceDB.Mrc
                        };

                        List<TaxItem> taxItems = new List<TaxItem>();

                        var taxItemInvoice = await sqliteDbContext.GetAllTaxFromInvoice(invoiceDB.Id);

                        taxItemInvoice.ForEach(taxItem =>
                        {
                            if (taxItem.Amount.HasValue &&
                            taxItem.CategoryType.HasValue &&
                            taxItem.Rate.HasValue)
                            {
                                taxItems.Add(new TaxItem()
                                {
                                    Amount = taxItem.Amount.Value,
                                    CategoryName = taxItem.CategoryName,
                                    CategoryType = (CategoryTypeEnumeration)taxItem.CategoryType.Value,
                                    Label = taxItem.Label,
                                    Rate = taxItem.Rate.Value
                                });
                            }
                        });

                        invoiceResult.TaxItems = taxItems;

                        if (!isEfaktura)
                        {

                            refaundViewModel.CurrentInvoiceRequest = invoiceRequest;
                            refaundViewModel.CurrentInvoiceResult = invoiceResult;

                            var firma = sqliteDbContext.Firmas.FirstOrDefault();

                            if (firma != null &&
                                !string.IsNullOrEmpty(firma.Pib))
                            {
                                refaundViewModel.CurrentInvoiceRequest.BuyerId = firma.Pib;
                            }

                            PayRefaundWindow payRefaundWindow = new PayRefaundWindow(refaundViewModel);
                            payRefaundWindow.Show();
                        }
                        else
                        {
                            var firma = sqliteDbContext.Firmas.FirstOrDefault();

                            if (firma != null &&
                                !string.IsNullOrEmpty(firma.Pib))
                            {
                                invoiceRequest.TransactionType = ClickBar_Common.Enums.TransactionTypeEnumeration.Refund;
                                invoiceRequest.ReferentDocumentNumber = invoiceResult.InvoiceNumber;
                                invoiceRequest.ReferentDocumentDT = Convert.ToDateTime(invoiceResult.SdcDateTime);
                                invoiceRequest.BuyerId = $"10:{firma.Pib}";

                                refaundViewModel.CurrentInvoiceRequest = invoiceRequest;

                                refaundViewModel.FinisedRefaund(true);
                            }
                        }
                    }
                }
            }
        }
        private void PayRefaund()
        {
            PayRefaundViewModel payRefaundViewModel = (PayRefaundViewModel)_currentViewModel;

            AddPayment(payRefaundViewModel);

            //payRefaundViewModel.RefaundViewModel.CurrentInvoiceRequest.InvoiceNumber = SettingsManager.Instance.GetPosNumber();
            payRefaundViewModel.RefaundViewModel.CurrentInvoiceRequest.TransactionType = ClickBar_Common.Enums.TransactionTypeEnumeration.Refund;
            payRefaundViewModel.RefaundViewModel.CurrentInvoiceRequest.ReferentDocumentNumber = payRefaundViewModel.RefaundViewModel.CurrentInvoice.InvoiceNumber;
            payRefaundViewModel.RefaundViewModel.CurrentInvoiceRequest.ReferentDocumentDT = Convert.ToDateTime(payRefaundViewModel.RefaundViewModel.CurrentInvoice.SdcDateTime);

            if (!string.IsNullOrEmpty(payRefaundViewModel.BuyerId))
            {
                payRefaundViewModel.RefaundViewModel.CurrentInvoiceRequest.BuyerId = $"{payRefaundViewModel.CurrentBuyerIdElement.Id}:{payRefaundViewModel.BuyerId}";
            }
            else
            {
                payRefaundViewModel.RefaundViewModel.CurrentInvoiceRequest.BuyerId = null;
            }

            payRefaundViewModel.RefaundViewModel.CurrentInvoiceRequest.Payment = payRefaundViewModel.Payment;

            payRefaundViewModel.RefaundViewModel.FinisedRefaund(false);
            payRefaundViewModel.Window.Close();
            //payRefaundViewModel.RefaundViewModel.SearchRefaundInvoiceCommand.Execute(null);
        }
        private void AddPayment(PayRefaundViewModel payRefaundViewModel)
        {
            if (payRefaundViewModel.RefaundViewModel.InvoiceType == InvoiceTypeEnumeration.Predračun)
            {
                payRefaundViewModel.Payment.Add(new Payment()
                {
                    Amount = 0,
                    PaymentType = PaymentTypeEnumeration.Cash,
                });
                payRefaundViewModel.Payment.Add(new Payment()
                {
                    Amount = 0,
                    PaymentType = PaymentTypeEnumeration.Cash,
                });
                return;
            }

            decimal Cash = Convert.ToDecimal(payRefaundViewModel.Cash);
            decimal Card = Convert.ToDecimal(payRefaundViewModel.Card);
            decimal WireTransfer = Convert.ToDecimal(payRefaundViewModel.WireTransfer);

            if (Cash == 0 &&
                Card == 0 &&
                WireTransfer == 0)
            {
                payRefaundViewModel.Payment.Add(new Payment()
                {
                    Amount = Cash,
                    PaymentType = PaymentTypeEnumeration.Cash,
                });
                return;
            }

            if (Cash > 0)
            {
                payRefaundViewModel.Payment.Add(new Payment()
                {
                    Amount = Cash,
                    PaymentType = PaymentTypeEnumeration.Cash,
                });
            }
            if (Card > 0)
            {
                var payCash = payRefaundViewModel.Payment.FirstOrDefault(p => p.PaymentType == PaymentTypeEnumeration.Cash);

                if (payCash != null)
                {
                    payCash.Amount += Card;
                }
                else
                {
                    payRefaundViewModel.Payment.Add(new Payment()
                    {
                        Amount = Card,
                        PaymentType = PaymentTypeEnumeration.Cash,
                    });
                }
            }
            if (WireTransfer > 0)
            {
                payRefaundViewModel.Payment.Add(new Payment()
                {
                    Amount = WireTransfer,
                    PaymentType = PaymentTypeEnumeration.WireTransfer,
                });
            }
        }
    }
}