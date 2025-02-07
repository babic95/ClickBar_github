using ClickBar.Models.AppMain.Statistic.Knjizenje;
using ClickBar.Models.Sale;
using ClickBar.ViewModels;
using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar_Common.Enums;
using ClickBar_Database;
using ClickBar_Database.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ClickBar.Commands.AppMain.Statistic.Knjizenje
{
    public class SearchInvoicesCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ViewModelBase _currentViewModel;

        public SearchInvoicesCommand(ViewModelBase currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (_currentViewModel is KnjizenjeViewModel)
            {
                KnjizenjePazara();
            }
            else if (_currentViewModel is PregledPazaraViewModel)
            {
                PregledPazara();
            }
            else
            {
                return;
            }
        }
        private void KnjizenjePazara()
        {
            KnjizenjeViewModel knjizenjeViewModel = (KnjizenjeViewModel)_currentViewModel;

            if (knjizenjeViewModel.CurrentDate.Date > DateTime.Now.Date)
            {
                MessageBox.Show("Datum mora biti u sadašnjisti ili prošlosti!", "Datum je u budućnosti", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            knjizenjeViewModel.Invoices = new ObservableCollection<Invoice>();

            using (SqliteDbContext sqliteDbContext = new SqliteDbContext())
            {
                knjizenjeViewModel.CurrentKnjizenjePazara = new KnjizenjePazara(knjizenjeViewModel.CurrentDate);

                DateTime fromDateTime = new DateTime(knjizenjeViewModel.CurrentDate.Year,
                    knjizenjeViewModel.CurrentDate.Month, 
                    knjizenjeViewModel.CurrentDate.Day,
                    5, 0, 0);
                DateTime toDateTime = new DateTime(knjizenjeViewModel.CurrentDate.Year,
                    knjizenjeViewModel.CurrentDate.Month,
                    knjizenjeViewModel.CurrentDate.Day,
                    4, 59, 59).AddDays(1);

                var invoices = sqliteDbContext.Invoices.Where(invoice => invoice.SdcDateTime != null && invoice.SdcDateTime.HasValue &&
                invoice.SdcDateTime.Value >= fromDateTime && invoice.SdcDateTime.Value <= toDateTime &&
                string.IsNullOrEmpty(invoice.KnjizenjePazaraId));

                if (invoices != null &&
                    invoices.Any())
                {
                    invoices.ForEachAsync(invoice =>
                    {
                        var cashier = sqliteDbContext.Cashiers.Find(invoice.Cashier);

                        if (cashier != null)
                        {
                            var inv = new Invoice(invoice, knjizenjeViewModel.Invoices.Count + 1);

                            inv.Cashier = cashier.Name;
                            if (invoice.InvoiceType != null && invoice.InvoiceType.HasValue &&
                            invoice.InvoiceType.Value == (int)InvoiceTypeEenumeration.Normal)
                            {
                                knjizenjeViewModel.Invoices.Add(inv);
                            }
                        }
                    });
                    UpdatePaymentType(sqliteDbContext, knjizenjeViewModel.CurrentKnjizenjePazara, invoices);
                    knjizenjeViewModel.Invoices = new ObservableCollection<Invoice>(knjizenjeViewModel.Invoices.OrderBy(i => i.SdcDateTime));
                }
            }
        }
        private void PregledPazara()
        {
            PregledPazaraViewModel pregledPazaraViewModel = (PregledPazaraViewModel)_currentViewModel;

            DateTime fromDate = new DateTime(pregledPazaraViewModel.FromDate.Year, pregledPazaraViewModel.FromDate.Month, pregledPazaraViewModel.FromDate.Day, 5, 0, 0);
            DateTime date = pregledPazaraViewModel.ToDate.AddDays(1);
            DateTime toDate = new DateTime(date.Year, date.Month, date.Day, 4, 59, 59);

            if(fromDate > toDate)
            {
                MessageBox.Show("Početni datum ne sme biti mlađi od krajnjeg!", "Greška u datumu", MessageBoxButton.OK, MessageBoxImage.Error);

                pregledPazaraViewModel.FromDate = DateTime.Now;
                pregledPazaraViewModel.ToDate = DateTime.Now;

                return;
            }

            if (fromDate.Date > DateTime.Now.Date)
            {
                MessageBox.Show("Početni datum mora biti u sadašnjisti ili prošlosti!", "Datum je u budućnosti", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            pregledPazaraViewModel.Invoices = new ObservableCollection<Invoice>();

            using (SqliteDbContext sqliteDbContext = new SqliteDbContext())
            {

                pregledPazaraViewModel.CurrentKnjizenjePazara = new KnjizenjePazara(fromDate);

                var invoices = sqliteDbContext.Invoices.Where(invoice => invoice.SdcDateTime != null && invoice.SdcDateTime.HasValue &&
                invoice.SdcDateTime.Value >= fromDate && invoice.SdcDateTime.Value <= toDate &&
                !string.IsNullOrEmpty(invoice.KnjizenjePazaraId));

                if (invoices != null &&
                    invoices.Any())
                {
                    invoices.ForEachAsync(invoice =>
                    {
                        var cashier = sqliteDbContext.Cashiers.Find(invoice.Cashier);

                        if (cashier != null)
                        {
                            var inv = new Invoice(invoice, pregledPazaraViewModel.Invoices.Count + 1);

                            inv.Cashier = cashier.Name;
                            if (invoice.InvoiceType != null && invoice.InvoiceType.HasValue &&
                            invoice.InvoiceType.Value == (int)InvoiceTypeEenumeration.Normal)
                            {
                                pregledPazaraViewModel.Invoices.Add(inv);
                            }
                        }
                    });
                    UpdatePaymentType(sqliteDbContext, pregledPazaraViewModel.CurrentKnjizenjePazara, invoices);

                    pregledPazaraViewModel.Invoices = new ObservableCollection<Invoice>(pregledPazaraViewModel.Invoices.OrderBy(i => i.SdcDateTime));
                }
            }
        }
        private void UpdatePaymentType(SqliteDbContext sqliteDbContext, 
            KnjizenjePazara knjizenjePazara,
            IQueryable<InvoiceDB> invoicesDB)
        {
            knjizenjePazara.NormalSaleCash = invoicesDB.Join(sqliteDbContext.PaymentInvoices,
                invoice => invoice.Id,
                payment => payment.InvoiceId,
                (invoice, payment) => new { I = invoice, P = payment }).Where(p => p.I.TotalAmount.HasValue &&
                p.P.PaymentType == PaymentTypeEnumeration.Cash &&
                p.I.TransactionType.HasValue &&
                p.I.TransactionType.Value == (int)ClickBar_Common.Enums.TransactionTypeEnumeration.Sale)
                .Sum(p => p.I.TotalAmount.Value);
            knjizenjePazara.NormalSaleCard = invoicesDB.Join(sqliteDbContext.PaymentInvoices,
                invoice => invoice.Id,
                payment => payment.InvoiceId,
                (invoice, payment) => new { I = invoice, P = payment }).Where(p => p.I.TotalAmount.HasValue &&
                p.P.PaymentType == PaymentTypeEnumeration.Card &&
                p.I.TransactionType.HasValue &&
                p.I.TransactionType.Value == (int)ClickBar_Common.Enums.TransactionTypeEnumeration.Sale)
                .Sum(p => p.I.TotalAmount.Value);
            knjizenjePazara.NormalSaleWireTransfer = invoicesDB.Join(sqliteDbContext.PaymentInvoices,
                invoice => invoice.Id,
                payment => payment.InvoiceId,
                (invoice, payment) => new { I = invoice, P = payment }).Where(p => p.I.TotalAmount.HasValue &&
                p.P.PaymentType == PaymentTypeEnumeration.WireTransfer &&
                p.I.TransactionType.HasValue &&
                p.I.TransactionType.Value == (int)ClickBar_Common.Enums.TransactionTypeEnumeration.Sale)
                .Sum(p => p.I.TotalAmount.Value);

            knjizenjePazara.NormalRefundCash = invoicesDB.Join(sqliteDbContext.PaymentInvoices,
                invoice => invoice.Id,
                payment => payment.InvoiceId,
                (invoice, payment) => new { I = invoice, P = payment }).Where(p => p.I.TotalAmount.HasValue &&
                p.P.PaymentType == PaymentTypeEnumeration.Cash &&
                p.I.TransactionType.HasValue &&
                p.I.TransactionType.Value == (int)ClickBar_Common.Enums.TransactionTypeEnumeration.Refund)
                .Sum(p => p.I.TotalAmount.Value);
            knjizenjePazara.NormalRefundCard = invoicesDB.Join(sqliteDbContext.PaymentInvoices,
                invoice => invoice.Id,
                payment => payment.InvoiceId,
                (invoice, payment) => new { I = invoice, P = payment }).Where(p => p.I.TotalAmount.HasValue &&
                p.P.PaymentType == PaymentTypeEnumeration.Card &&
                p.I.TransactionType.HasValue &&
                p.I.TransactionType.Value == (int)ClickBar_Common.Enums.TransactionTypeEnumeration.Refund)
                .Sum(p => p.I.TotalAmount.Value);
            knjizenjePazara.NormalRefundWireTransfer = invoicesDB.Join(sqliteDbContext.PaymentInvoices,
                invoice => invoice.Id,
                payment => payment.InvoiceId,
                (invoice, payment) => new { I = invoice, P = payment }).Where(p => p.I.TotalAmount.HasValue &&
                p.P.PaymentType == PaymentTypeEnumeration.WireTransfer &&
                p.I.TransactionType.HasValue &&
                p.I.TransactionType.Value == (int)ClickBar_Common.Enums.TransactionTypeEnumeration.Refund)
                .Sum(p => p.I.TotalAmount.Value);

            knjizenjePazara.Total = knjizenjePazara.NormalSaleCash + knjizenjePazara.NormalSaleCard + knjizenjePazara.NormalSaleWireTransfer -
                knjizenjePazara.NormalRefundCash - knjizenjePazara.NormalRefundCard - knjizenjePazara.NormalRefundWireTransfer;

            //var payments = sqliteDbContext.PaymentInvoices.Where(pay => pay.InvoiceId == invoiceDB.Id);

            //if(payments != null && payments.Any())
            //{
            //    payments.ToList().ForEach(payment =>
            //    {
            //        if (payment.Amout.HasValue)
            //        {
            //            if (invoiceDB.TransactionType != null &&
            //            invoiceDB.TransactionType.HasValue)
            //            {
            //                if (invoiceDB.TransactionType.Value == (int)ClickBar_Common.Enums.TransactionTypeEnumeration.Refund)
            //                {
            //                    switch (payment.PaymentType)
            //                    {
            //                        case PaymentTypeEnumeration.Cash:
            //                            knjizenjePazara.NormalRefundCash -= payment.Amout.Value;
            //                            break;
            //                        case PaymentTypeEnumeration.Card:
            //                            knjizenjePazara.NormalRefundCard -= payment.Amout.Value;
            //                            break;
            //                        case PaymentTypeEnumeration.WireTransfer:
            //                            knjizenjePazara.NormalRefundWireTransfer -= payment.Amout.Value;
            //                            break;
            //                    }
            //                    knjizenjePazara.Total -= payment.Amout.Value;
            //                }
            //                else 
            //                {
            //                    switch (payment.PaymentType)
            //                    {
            //                        case PaymentTypeEnumeration.Cash:
            //                            knjizenjePazara.NormalSaleCash += payment.Amout.Value;
            //                            break;
            //                        case PaymentTypeEnumeration.Card:
            //                            knjizenjePazara.NormalSaleCard += payment.Amout.Value;
            //                            break;
            //                        case PaymentTypeEnumeration.WireTransfer:
            //                            knjizenjePazara.NormalSaleWireTransfer += payment.Amout.Value;
            //                            break;
            //                    }
            //                    knjizenjePazara.Total += payment.Amout.Value;
            //                }
            //            }
            //        }
            //    });
            //}
        }
    }
}