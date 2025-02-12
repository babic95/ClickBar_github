using ClickBar.Models.AppMain.Statistic.Knjizenje;
using ClickBar.Models.Sale;
using ClickBar.ViewModels;
using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar_Common.Enums;
using ClickBar_DatabaseSQLManager;
using ClickBar_DatabaseSQLManager.Models;
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
        private SqlServerDbContext _dbContext;

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
            if (_currentViewModel is KnjizenjeViewModel knjizenjeViewModel)
            {
                _dbContext = knjizenjeViewModel.DbContext;
                KnjizenjePazara();
            }
            else if (_currentViewModel is PregledPazaraViewModel pregledPazaraViewModel)
            {
                _dbContext = pregledPazaraViewModel.DbContext;
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

            knjizenjeViewModel.CurrentKnjizenjePazara = new KnjizenjePazara(knjizenjeViewModel.CurrentDate);

            DateTime fromDateTime = new DateTime(knjizenjeViewModel.CurrentDate.Year,
                knjizenjeViewModel.CurrentDate.Month,
                knjizenjeViewModel.CurrentDate.Day,
                5, 0, 0);
            DateTime toDateTime = new DateTime(knjizenjeViewModel.CurrentDate.Year,
                knjizenjeViewModel.CurrentDate.Month,
                knjizenjeViewModel.CurrentDate.Day,
                4, 59, 59).AddDays(1);

            var invoices = _dbContext.Invoices
                .Where(invoice => invoice.SdcDateTime != null && invoice.SdcDateTime.HasValue &&
                                  invoice.SdcDateTime.Value >= fromDateTime && invoice.SdcDateTime.Value <= toDateTime &&
                                  string.IsNullOrEmpty(invoice.KnjizenjePazaraId))
                .ToList();

            if (invoices != null && invoices.Any())
            {
                foreach (var invoice in invoices)
                {
                    var cashier = _dbContext.Cashiers.Find(invoice.Cashier);

                    if (cashier != null)
                    {
                        var inv = new Invoice(invoice, knjizenjeViewModel.Invoices.Count + 1)
                        {
                            Cashier = cashier.Name
                        };

                        if (invoice.InvoiceType != null && invoice.InvoiceType.HasValue &&
                            invoice.InvoiceType.Value == (int)InvoiceTypeEenumeration.Normal)
                        {
                            knjizenjeViewModel.Invoices.Add(inv);
                        }
                    }
                }
                UpdatePaymentType(knjizenjeViewModel.CurrentKnjizenjePazara, invoices);
                knjizenjeViewModel.Invoices = new ObservableCollection<Invoice>(knjizenjeViewModel.Invoices.OrderBy(i => i.SdcDateTime));
            }
        }
        private void PregledPazara()
        {
            PregledPazaraViewModel pregledPazaraViewModel = (PregledPazaraViewModel)_currentViewModel;

            DateTime fromDate = new DateTime(pregledPazaraViewModel.FromDate.Year, pregledPazaraViewModel.FromDate.Month, pregledPazaraViewModel.FromDate.Day, 5, 0, 0);
            DateTime date = pregledPazaraViewModel.ToDate.AddDays(1);
            DateTime toDate = new DateTime(date.Year, date.Month, date.Day, 4, 59, 59);

            if (fromDate > toDate)
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

            pregledPazaraViewModel.CurrentKnjizenjePazara = new KnjizenjePazara(fromDate);

            var invoices = _dbContext.Invoices
                .Where(invoice => invoice.SdcDateTime != null && invoice.SdcDateTime.HasValue &&
                                  invoice.SdcDateTime.Value >= fromDate && invoice.SdcDateTime.Value <= toDate &&
                                  !string.IsNullOrEmpty(invoice.KnjizenjePazaraId)).ToList();

            if (invoices != null && invoices.Any())
            {
                foreach (var invoice in invoices)
                {
                    var cashier = _dbContext.Cashiers.Find(invoice.Cashier);

                    if (cashier != null)
                    {
                        var inv = new Invoice(invoice, pregledPazaraViewModel.Invoices.Count + 1)
                        {
                            Cashier = cashier.Name
                        };

                        if (invoice.InvoiceType != null && invoice.InvoiceType.HasValue &&
                            invoice.InvoiceType.Value == (int)InvoiceTypeEenumeration.Normal)
                        {
                            pregledPazaraViewModel.Invoices.Add(inv);
                        }
                    }
                }
                UpdatePaymentType(pregledPazaraViewModel.CurrentKnjizenjePazara, invoices);

                pregledPazaraViewModel.Invoices = new ObservableCollection<Invoice>(pregledPazaraViewModel.Invoices.OrderBy(i => i.SdcDateTime));
            }
        }
        private void UpdatePaymentType(KnjizenjePazara knjizenjePazara,
            List<InvoiceDB> invoicesDB)
        {
            var paymentInvoices = _dbContext.PaymentInvoices.ToList();

            knjizenjePazara.NormalSaleCash = invoicesDB
                .Join(paymentInvoices,
                    invoice => invoice.Id,
                    payment => payment.InvoiceId,
                    (invoice, payment) => new { I = invoice, P = payment })
                .Where(p => p.I.TotalAmount.HasValue &&
                            p.P.PaymentType == PaymentTypeEnumeration.Cash &&
                            p.I.TransactionType.HasValue &&
                            p.I.TransactionType.Value == (int)ClickBar_Common.Enums.TransactionTypeEnumeration.Sale)
                .Sum(p => p.I.TotalAmount.Value);

            knjizenjePazara.NormalSaleCard = invoicesDB
                .Join(paymentInvoices,
                    invoice => invoice.Id,
                    payment => payment.InvoiceId,
                    (invoice, payment) => new { I = invoice, P = payment })
                .Where(p => p.I.TotalAmount.HasValue &&
                            p.P.PaymentType == PaymentTypeEnumeration.Card &&
                            p.I.TransactionType.HasValue &&
                            p.I.TransactionType.Value == (int)ClickBar_Common.Enums.TransactionTypeEnumeration.Sale)
                .Sum(p => p.I.TotalAmount.Value);

            knjizenjePazara.NormalSaleWireTransfer = invoicesDB
                .Join(paymentInvoices,
                    invoice => invoice.Id,
                    payment => payment.InvoiceId,
                    (invoice, payment) => new { I = invoice, P = payment })
                .Where(p => p.I.TotalAmount.HasValue &&
                            p.P.PaymentType == PaymentTypeEnumeration.WireTransfer &&
                            p.I.TransactionType.HasValue &&
                            p.I.TransactionType.Value == (int)ClickBar_Common.Enums.TransactionTypeEnumeration.Sale)
                .Sum(p => p.I.TotalAmount.Value);

            knjizenjePazara.NormalRefundCash = invoicesDB
                .Join(paymentInvoices,
                    invoice => invoice.Id,
                    payment => payment.InvoiceId,
                    (invoice, payment) => new { I = invoice, P = payment })
                .Where(p => p.I.TotalAmount.HasValue &&
                            p.P.PaymentType == PaymentTypeEnumeration.Cash &&
                            p.I.TransactionType.HasValue &&
                            p.I.TransactionType.Value == (int)ClickBar_Common.Enums.TransactionTypeEnumeration.Refund)
                .Sum(p => p.I.TotalAmount.Value);

            knjizenjePazara.NormalRefundCard = invoicesDB
                .Join(paymentInvoices,
                    invoice => invoice.Id,
                    payment => payment.InvoiceId,
                    (invoice, payment) => new { I = invoice, P = payment })
                .Where(p => p.I.TotalAmount.HasValue &&
                            p.P.PaymentType == PaymentTypeEnumeration.Card &&
                            p.I.TransactionType.HasValue &&
                            p.I.TransactionType.Value == (int)ClickBar_Common.Enums.TransactionTypeEnumeration.Refund)
                .Sum(p => p.I.TotalAmount.Value);

            knjizenjePazara.NormalRefundWireTransfer = invoicesDB
                .Join(paymentInvoices,
                    invoice => invoice.Id,
                    payment => payment.InvoiceId,
                    (invoice, payment) => new { I = invoice, P = payment })
                .Where(p => p.I.TotalAmount.HasValue &&
                            p.P.PaymentType == PaymentTypeEnumeration.WireTransfer &&
                            p.I.TransactionType.HasValue &&
                            p.I.TransactionType.Value == (int)ClickBar_Common.Enums.TransactionTypeEnumeration.Refund)
                .Sum(p => p.I.TotalAmount.Value);

            knjizenjePazara.Total = knjizenjePazara.NormalSaleCash + knjizenjePazara.NormalSaleCard + knjizenjePazara.NormalSaleWireTransfer -
                knjizenjePazara.NormalRefundCash - knjizenjePazara.NormalRefundCard - knjizenjePazara.NormalRefundWireTransfer;
        }
    }
}