using ClickBar.Models.Sale;
using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar.ViewModels;
using ClickBar.Views.AppMain.AuxiliaryWindows.Statistic.Knjizenje;
using ClickBar_DatabaseSQLManager;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ClickBar_Printer.Models.Otpremnice;
using ClickBar_Printer;
using System.Windows;

namespace ClickBar.Commands.AppMain.Statistic.Knjizenje
{
    public class PrintInvoiceCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ViewModelBase _currentViewModel;
        private SqlServerDbContext _dbContext;

        public PrintInvoiceCommand(ViewModelBase currentViewModel)
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
                KnjizenjePazara(parameter);
            }
            else if (_currentViewModel is PregledPazaraViewModel pregledPazaraViewModel)
            {
                _dbContext = pregledPazaraViewModel.DbContext;
                PregledPazara(parameter);
            }
            else
            {
                return;
            }
        }
        private void KnjizenjePazara(object parameter)
        {
            KnjizenjeViewModel knjizenjeViewModel = (KnjizenjeViewModel)_currentViewModel;

            if (parameter != null &&
                parameter is string)
            {
                string invoiceId = (string)parameter;

                var invoice = knjizenjeViewModel.Invoices.FirstOrDefault(inv => inv.Id == invoiceId);

                if (invoice != null)
                {
                    OtpremnicaPrint otpremnicaPrint = new OtpremnicaPrint()
                    {
                        BuyerId = invoice.BuyerId,
                        BuyerName = invoice.BuyerName,
                        InvoiceNumber = invoice.InvoiceNumber,
                        SdcDateTime = invoice.SdcDateTime,
                        TotalAmount = invoice.TotalAmount,
                        Items = new List<OtpremnicaItemPrint>()
                    };

                    var itemsDB = _dbContext.ItemInvoices.Where(inv => inv.InvoiceId == invoiceId &&
                                (inv.IsSirovina == null || inv.IsSirovina == 0));

                    if (itemsDB != null &&
                        itemsDB.Any())
                    {
                        foreach(var itemInvoiceDB in itemsDB)
                        {
                            if (itemInvoiceDB.Quantity.HasValue &&
                                itemInvoiceDB.TotalAmout.HasValue &&
                                itemInvoiceDB.UnitPrice.HasValue &&
                                !string.IsNullOrEmpty(itemInvoiceDB.Name))
                            {
                                OtpremnicaItemPrint otpremnicaItemPrint = new OtpremnicaItemPrint()
                                {
                                    ItemName = itemInvoiceDB.Name,
                                    Quantity = itemInvoiceDB.Quantity.Value,
                                    TotalPrice = itemInvoiceDB.TotalAmout.Value,
                                    UnitPrice = itemInvoiceDB.UnitPrice.Value
                                };
                                otpremnicaPrint.Items.Add(otpremnicaItemPrint);
                            }
                        }

                        var firmaDB = _dbContext.Firmas.FirstOrDefault();
                        if (firmaDB != null)
                        {
                            PrinterManager.Instance.PrintOtpremnica(otpremnicaPrint, firmaDB);
                        }
                        else
                        {
                            MessageBox.Show("Firma nije postavljena!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }
        private void PregledPazara(object parameter)
        {
            PregledPazaraViewModel pregledPazaraViewModel = (PregledPazaraViewModel)_currentViewModel;

            if (parameter != null &&
                 parameter is string)
            {
                string invoiceId = (string)parameter;

                var invoice = pregledPazaraViewModel.Invoices.FirstOrDefault(inv => inv.Id == invoiceId);

                if (invoice != null)
                {
                    OtpremnicaPrint otpremnicaPrint = new OtpremnicaPrint()
                    {
                        BuyerId = invoice.BuyerId,
                        BuyerName = invoice.BuyerName,
                        InvoiceNumber = invoice.InvoiceNumber,
                        SdcDateTime = invoice.SdcDateTime,
                        TotalAmount = invoice.TotalAmount,
                        Items = new List<OtpremnicaItemPrint>()
                    };

                    var itemsDB = _dbContext.ItemInvoices.Where(inv => inv.InvoiceId == invoiceId &&
                                (inv.IsSirovina == null || inv.IsSirovina == 0));

                    if (itemsDB != null &&
                        itemsDB.Any())
                    {
                        foreach (var itemInvoiceDB in itemsDB)
                        {
                            if (itemInvoiceDB.Quantity.HasValue &&
                                itemInvoiceDB.TotalAmout.HasValue &&
                                itemInvoiceDB.UnitPrice.HasValue &&
                                !string.IsNullOrEmpty(itemInvoiceDB.Name))
                            {
                                OtpremnicaItemPrint otpremnicaItemPrint = new OtpremnicaItemPrint()
                                {
                                    ItemName = itemInvoiceDB.Name,
                                    Quantity = itemInvoiceDB.Quantity.Value,
                                    TotalPrice = itemInvoiceDB.TotalAmout.Value,
                                    UnitPrice = itemInvoiceDB.UnitPrice.Value
                                };
                                otpremnicaPrint.Items.Add(otpremnicaItemPrint);
                            }
                        }

                        var firmaDB = _dbContext.Firmas.FirstOrDefault();
                        if (firmaDB != null)
                        {
                            PrinterManager.Instance.PrintOtpremnica(otpremnicaPrint, firmaDB);
                        }
                        else
                        {
                            MessageBox.Show("Firma nije postavljena!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }
    }
}