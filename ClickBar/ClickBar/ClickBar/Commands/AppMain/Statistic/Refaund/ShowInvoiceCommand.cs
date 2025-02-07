using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar.ViewModels;
using ClickBar_Common.Models.Invoice.Helpers;
using ClickBar_Common.Models.Invoice.Tax;
using ClickBar_Common.Models.Invoice;
using ClickBar_Database.Models;
using ClickBar_Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace ClickBar.Commands.AppMain.Statistic.Refaund
{
    public class ShowInvoiceCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ViewModelBase _currentViewModel;

        public ShowInvoiceCommand(ViewModelBase currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            //if (_currentViewModel is CopyInvoiceTypeViewModel)
            //{
            //    CopyInvoiceTypeViewModel copyInvoiceTypeViewModel = (CopyInvoiceTypeViewModel)_currentViewModel;

            //    if (string.IsNullOrEmpty(copyInvoiceTypeViewModel.Ref1) ||
            //    string.IsNullOrEmpty(copyInvoiceTypeViewModel.Ref2) ||
            //    string.IsNullOrEmpty(copyInvoiceTypeViewModel.RefNumber) ||
            //    string.IsNullOrEmpty(copyInvoiceTypeViewModel.RefDateDay) ||
            //    string.IsNullOrEmpty(copyInvoiceTypeViewModel.RefDateMonth) ||
            //    string.IsNullOrEmpty(copyInvoiceTypeViewModel.RefDateYear) ||
            //    string.IsNullOrEmpty(copyInvoiceTypeViewModel.RefDateHour) ||
            //    string.IsNullOrEmpty(copyInvoiceTypeViewModel.RefDateMinute) ||
            //    string.IsNullOrEmpty(copyInvoiceTypeViewModel.RefDateSecond) ||
            //    copyInvoiceTypeViewModel.CurrentInvoice is null)
            //    {
            //        MessageBox.Show("Selektujte račun u tabeli!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            //    }
            //    else
            //    {
            //        SqliteDbContext sqliteDbContext = new SqliteDbContext();

            //        InvoiceDB invoice = await sqliteDbContext.GetInvoice(copyInvoiceTypeViewModel.CurrentInvoice.Id);

            //        if (invoice is not null)
            //        {
            //            InvoiceRequest invoiceRequest = new InvoiceRequest()
            //            {
            //                Cashier = invoice.Cashier,
            //                DateAndTimeOfIssue = invoice.DateAndTimeOfIssue,
            //                BuyerId = invoice.BuyerId,
            //                BuyerName = invoice.BuyerName,
            //                BuyerAddress = invoice.BuyerAddress,
            //                BuyerCostCenterId = invoice.BuyerCostCenterId,
            //                InvoiceNumber = invoice.InvoiceNumber,
            //                InvoiceType = invoice.InvoiceType,
            //                ReferentDocumentDT = invoice.ReferentDocumentDT,
            //                ReferentDocumentNumber = invoice.ReferentDocumentNumber,
            //                TransactionType = invoice.TransactionType,
            //                Options = new InlineModel()
            //                {
            //                    OmitQRCodeGen = "1",
            //                    OmitTextualRepresentation = "1",
            //                }
            //            };
            //            List<Item> items = new List<Item>();
            //            List<ItemInvoiceDB> itemsInvoice = await sqliteDbContext.GetAllItemsFromInvoice(invoice.Id);

            //            itemsInvoice.ForEach(item =>
            //            {
            //                var itemDB = sqliteDbContext.Items.FirstOrDefault(i => i.Id == item.ItemCode);

            //                if (itemDB != null)
            //                {
            //                    items.Add(new Item()
            //                    {
            //                        Name = item.Name,
            //                        Jm = string.IsNullOrEmpty(itemDB.Group) ? "kom" : itemDB.Group,
            //                        Quantity = item.Quantity,
            //                        TotalAmount = item.TotalAmout,
            //                        UnitPrice = item.UnitPrice,
            //                        Gtin = string.IsNullOrEmpty(itemDB.BarCode) ? null :
            //                        itemDB.BarCode.Count() < 13 ||
            //                        itemDB.BarCode.Contains("-") ||
            //                        itemDB.BarCode.Contains(".") ||
            //                        itemDB.BarCode.Contains(":") ||
            //                        itemDB.BarCode.Contains("/") ||
            //                        itemDB.BarCode.Contains(" ") ||
            //                        itemDB.BarCode.Count() > 14 ? null : itemDB.BarCode,
            //                        Labels = new List<string>()
            //                        {
            //                            item.Label,
            //                        },
            //                        ItemCode = item.ItemCode
            //                    });
            //                }
            //            });
            //            invoiceRequest.Items = items;

            //            List<Payment> payments = new List<Payment>();

            //            var paymentsIvoice = await sqliteDbContext.GetAllPaymentFromInvoice(invoice.Id);
            //            paymentsIvoice.ForEach(payment =>
            //            {
            //                payments.Add(new Payment()
            //                {
            //                    Amount = payment.Amout,
            //                    PaymentType = payment.PaymentType
            //                });
            //            });
            //            invoiceRequest.Payment = payments;

            //            InvoiceResult invoiceResult = new InvoiceResult()
            //            {
            //                RequestedBy = invoice.RequestedBy,
            //                SignedBy = invoice.SignedBy,
            //                SdcDateTime = invoice.SdcDateTime.Value,
            //                InvoiceCounter = invoice.InvoiceCounter,
            //                InvoiceCounterExtension = invoice.InvoiceCounterExtension,
            //                InvoiceNumber = invoice.InvoiceNumberResult,
            //                TotalCounter = invoice.TotalCounter,
            //                TransactionTypeCounter = invoice.TransactionTypeCounter,
            //                TotalAmount = invoice.TotalAmount,
            //                EncryptedInternalData = invoice.EncryptedInternalData,
            //                Signature = invoice.Signature,
            //                BusinessName = invoice.BusinessName,
            //                LocationName = invoice.LocationName,
            //                Address = invoice.Address,
            //                Tin = invoice.Tin,
            //                District = invoice.District,
            //                TaxGroupRevision = invoice.TaxGroupRevision,
            //                Mrc = invoice.Mrc
            //            };

            //            List<TaxItem> taxItems = new List<TaxItem>();

            //            var taxItemInvoice = await sqliteDbContext.GetAllTaxFromInvoice(invoice.Id);

            //            taxItemInvoice.ForEach(taxItem =>
            //            {
            //                taxItems.Add(new TaxItem()
            //                {
            //                    Amount = taxItem.Amount,
            //                    CategoryName = taxItem.CategoryName,
            //                    CategoryType = taxItem.CategoryType,
            //                    Label = taxItem.Label,
            //                    Rate = taxItem.Rate
            //                });
            //            });

            //            invoiceResult.TaxItems = taxItems;

            //            copyInvoiceTypeViewModel.CurrentInvoiceRequest = invoiceRequest;
            //            copyInvoiceTypeViewModel.CurrentInvoiceResult = invoiceResult;
            //            copyInvoiceTypeViewModel.Journal = JournalHelper.CreateJournal(copyInvoiceTypeViewModel.CurrentInvoiceRequest, copyInvoiceTypeViewModel.CurrentInvoiceResult);

            //            copyInvoiceTypeViewModel.CreateCopyInvoiceWindow = new CreateCopyInvoice(copyInvoiceTypeViewModel);
            //            copyInvoiceTypeViewModel.CreateCopyInvoiceWindow.ShowDialog();
            //        }
            //    }
            //}
            //else
            //{
            //    if (parameter != null && parameter is TransactionEnumeration)
            //    {
            //        TransactionEnumeration transactionEnumeration = (TransactionEnumeration)parameter;

            //        CashierDB cashier = null;
            //        if (_currentViewModel is SaleViewModel)
            //        {
            //            SaleViewModel saleViewModel = (SaleViewModel)_currentViewModel;
            //            cashier = saleViewModel.LoggedCashier;
            //        }
            //        else if (_currentViewModel is RefaundViewModel)
            //        {
            //            RefaundViewModel refaundViewModel = (RefaundViewModel)_currentViewModel;
            //            cashier = refaundViewModel.LoggedCashier;
            //        }

            //        if (cashier != null)
            //        {
            //            CopyInvoiceTypeViewModel copyInvoiceTypeViewModel = new CopyInvoiceTypeViewModel(cashier, transactionEnumeration);

            //            CopyInvoiceTypeWindow copyInvoiceTypeWindow = new CopyInvoiceTypeWindow(copyInvoiceTypeViewModel);
            //            copyInvoiceTypeWindow.ShowDialog();
            //        }
            //    }

            //}
        }
    }
}