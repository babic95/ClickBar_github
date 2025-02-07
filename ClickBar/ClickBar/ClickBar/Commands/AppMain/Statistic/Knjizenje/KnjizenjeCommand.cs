using ClickBar.Enums.AppMain.Statistic;
using ClickBar.Enums.Sale;
using ClickBar.Models.AppMain.Statistic;
using ClickBar.Models.AppMain.Statistic.Knjizenje;
using ClickBar.Models.Sale;
using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar_Common.Enums;
using ClickBar_Common.Models.Invoice;
using ClickBar_Database;
using ClickBar_Database.Models;
using ClickBar_Logging;
using ClickBar_Report.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows;
using System.Windows.Input;

namespace ClickBar.Commands.AppMain.Statistic.Knjizenje
{
    public class KnjizenjeCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private KnjizenjeViewModel _currentViewModel;

        public KnjizenjeCommand(KnjizenjeViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }
        private class Zaduzenje
        {
            public Zaduzenje()
            {
                NormalSaleCash = 0;
                NormalSaleCard = 0;
                NormalSaleWireTransfer = 0;
                NormalRefundCash = 0;
                NormalRefundCard = 0;
                NormalRefundWireTransfer = 0;
            }

            public decimal NormalSaleCash { get; set; }
            public decimal NormalSaleCard { get; set; }
            public decimal NormalSaleWireTransfer { get; set; }
            public decimal NormalRefundCash { get; set; }
            public decimal NormalRefundCard { get; set; }
            public decimal NormalRefundWireTransfer { get; set; }

        }
        public void Execute(object parameter)
        {
            Zaduzenje zaduzenje = new Zaduzenje();

            if (_currentViewModel.Invoices.Any())
            {
                MessageBoxResult result = MessageBox.Show("Da li ste sigurni da želite da proknjižite trenutan pazar?", 
                    "Knjiženje neobrađenog pazara",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (SqliteDbContext sqliteDbContext = new SqliteDbContext())
                        {

                            KnjizenjePazaraDB? knjizenjePazaraDB = sqliteDbContext.KnjizenjePazara.FirstOrDefault(kp =>
                            kp.IssueDateTime.Date == _currentViewModel.CurrentDate.Date);

                            if (knjizenjePazaraDB != null)
                            {
                                knjizenjePazaraDB.NormalSaleCash += _currentViewModel.CurrentKnjizenjePazara.NormalSaleCash;
                                knjizenjePazaraDB.NormalSaleCard += _currentViewModel.CurrentKnjizenjePazara.NormalSaleCard;
                                knjizenjePazaraDB.NormalSaleWireTransfer += _currentViewModel.CurrentKnjizenjePazara.NormalSaleWireTransfer;
                                knjizenjePazaraDB.NormalRefundCash += _currentViewModel.CurrentKnjizenjePazara.NormalRefundCash;
                                knjizenjePazaraDB.NormalRefundCard += _currentViewModel.CurrentKnjizenjePazara.NormalRefundCard;
                                knjizenjePazaraDB.NormalRefundWireTransfer += _currentViewModel.CurrentKnjizenjePazara.NormalRefundWireTransfer;

                                sqliteDbContext.KnjizenjePazara.Update(knjizenjePazaraDB);
                            }
                            else
                            {
                                knjizenjePazaraDB = new KnjizenjePazaraDB()
                                {
                                    Id = _currentViewModel.CurrentKnjizenjePazara.Id,
                                    Description = _currentViewModel.CurrentKnjizenjePazara.Description,
                                    IssueDateTime = _currentViewModel.CurrentKnjizenjePazara.IssueDateTime,
                                    NormalSaleCash = _currentViewModel.CurrentKnjizenjePazara.NormalSaleCash,
                                    NormalSaleCard = _currentViewModel.CurrentKnjizenjePazara.NormalSaleCard,
                                    NormalSaleWireTransfer = _currentViewModel.CurrentKnjizenjePazara.NormalSaleWireTransfer,
                                    NormalRefundCash = _currentViewModel.CurrentKnjizenjePazara.NormalRefundCash,
                                    NormalRefundCard = _currentViewModel.CurrentKnjizenjePazara.NormalRefundCard,
                                    NormalRefundWireTransfer = _currentViewModel.CurrentKnjizenjePazara.NormalRefundWireTransfer
                                };
                                sqliteDbContext.KnjizenjePazara.Add(knjizenjePazaraDB);
                            }
                            RetryHelper.ExecuteWithRetry(() => { sqliteDbContext.SaveChanges(); });

                            decimal nivelacija = 0;

                            _currentViewModel.Invoices.ToList().ForEach(invoice =>
                            {
                                var invoiceDB = sqliteDbContext.Invoices.FirstOrDefault(inv => inv.Id == invoice.Id);

                                if (invoiceDB != null)
                                {
                                    invoiceDB.KnjizenjePazaraId = knjizenjePazaraDB.Id;
                                    sqliteDbContext.Invoices.Update(invoiceDB);

                                    var invoiceItems = sqliteDbContext.ItemInvoices.Where(itemInvoice => itemInvoice.InvoiceId == invoiceDB.Id &&
                                    (itemInvoice.IsSirovina == null || itemInvoice.IsSirovina == 0));

                                    if (invoiceItems != null &&
                                    invoiceItems.Any())
                                    {
                                        invoiceItems.ToList().ForEach(item =>
                                        {
                                            var itemDB = sqliteDbContext.Items.Find(item.ItemCode);

                                            if (itemDB != null)
                                            {
                                                if (itemDB.IdNorm != null)
                                                {
                                                    var payment = sqliteDbContext.PaymentInvoices.FirstOrDefault(pay => pay.InvoiceId == invoiceDB.Id);

                                                    if (invoiceDB.TransactionType == (int)Enums.Sale.TransactionTypeEnumeration.Prodaja &&
                                                    item.TotalAmout != null &&
                                                    item.TotalAmout.HasValue)
                                                    {
                                                        switch (payment.PaymentType)
                                                        {
                                                            case PaymentTypeEnumeration.Cash:
                                                                zaduzenje.NormalSaleCash += item.TotalAmout.Value;
                                                                break;
                                                            case PaymentTypeEnumeration.Card:
                                                                zaduzenje.NormalSaleCard += item.TotalAmout.Value;
                                                                break;
                                                            case PaymentTypeEnumeration.WireTransfer:
                                                                zaduzenje.NormalSaleWireTransfer += item.TotalAmout.Value;
                                                                break;
                                                        }
                                                    }
                                                    else if (invoiceDB.TransactionType == (int)Enums.Sale.TransactionTypeEnumeration.Refundacija &&
                                                    item.TotalAmout != null &&
                                                    item.TotalAmout.HasValue)
                                                    {
                                                        switch (payment.PaymentType)
                                                        {
                                                            case PaymentTypeEnumeration.Cash:
                                                                zaduzenje.NormalRefundCash -= item.TotalAmout.Value;
                                                                break;
                                                            case PaymentTypeEnumeration.Card:
                                                                zaduzenje.NormalRefundCard -= item.TotalAmout.Value;
                                                                break;
                                                            case PaymentTypeEnumeration.WireTransfer:
                                                                zaduzenje.NormalRefundWireTransfer -= item.TotalAmout.Value;
                                                                break;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (item.OriginalUnitPrice.HasValue &&
                                                    item.UnitPrice.HasValue && item.Quantity.HasValue)
                                                    {
                                                        if (invoiceDB.TransactionType == (int)Enums.Sale.TransactionTypeEnumeration.Prodaja &&
                                                        item.OriginalUnitPrice != item.UnitPrice)
                                                        {
                                                            nivelacija += (item.UnitPrice.Value - item.OriginalUnitPrice.Value) * item.Quantity.Value;
                                                        }
                                                    }
                                                }
                                            }
                                        });
                                    }
                                }
                            });

                            RetryHelper.ExecuteWithRetry(() => { sqliteDbContext.SaveChanges(); });

                            Knjizenje(knjizenjePazaraDB, nivelacija, zaduzenje, sqliteDbContext);

                            _currentViewModel.SearchInvoicesCommand.Execute(null);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error("KnjizenjeCommand - Greska prilikom knjizenja pazara -> ", ex);
                    }
                }
            }
        }
        private void Knjizenje(KnjizenjePazaraDB knjizenjePazaraDB, 
            decimal nivelacija,
            Zaduzenje zaduzenje,
            SqliteDbContext sqliteDbContext)
        {
            if (knjizenjePazaraDB.NormalSaleCash != 0)
            {
                var kepPazar = sqliteDbContext.Kep.FirstOrDefault(kep => kep.KepDate.Date == knjizenjePazaraDB.IssueDateTime.Date &&
                kep.Type == (int)KepStateEnumeration.Dnevni_Pazar_Prodaja_Gotovina);

                if(kepPazar != null)
                {
                    kepPazar.Razduzenje = knjizenjePazaraDB.NormalSaleCash;
                    kepPazar.Zaduzenje += zaduzenje.NormalSaleCash;
                    sqliteDbContext.Kep.Update(kepPazar);
                }
                else
                {
                    KepDB kepDB = new KepDB()
                    {
                        Id = Guid.NewGuid().ToString(),
                        KepDate = knjizenjePazaraDB.IssueDateTime,
                        Type = (int)KepStateEnumeration.Dnevni_Pazar_Prodaja_Gotovina,
                        Razduzenje = knjizenjePazaraDB.NormalSaleCash,
                        Zaduzenje = zaduzenje.NormalSaleCash,
                        Description = $"Pazar na dan {knjizenjePazaraDB.IssueDateTime.ToString("dd.MM.yyyy")} PROMET PRODAJA - GOTOVINA"
                    };
                    sqliteDbContext.Kep.Add(kepDB);
                }
                RetryHelper.ExecuteWithRetry(() => { sqliteDbContext.SaveChanges(); });
            }
            if (knjizenjePazaraDB.NormalSaleCard != 0)
            {
                var kepPazar = sqliteDbContext.Kep.FirstOrDefault(kep => kep.KepDate.Date == knjizenjePazaraDB.IssueDateTime.Date &&
                kep.Type == (int)KepStateEnumeration.Dnevni_Pazar_Prodaja_Kartica);

                if (kepPazar != null)
                {
                    kepPazar.Razduzenje = knjizenjePazaraDB.NormalSaleCard;
                    kepPazar.Zaduzenje += zaduzenje.NormalSaleCard;
                    sqliteDbContext.Kep.Update(kepPazar);
                }
                else
                {
                    KepDB kepDB = new KepDB()
                    {
                        Id = Guid.NewGuid().ToString(),
                        KepDate = knjizenjePazaraDB.IssueDateTime,
                        Type = (int)KepStateEnumeration.Dnevni_Pazar_Prodaja_Kartica,
                        Razduzenje = knjizenjePazaraDB.NormalSaleCard,
                        Zaduzenje = zaduzenje.NormalSaleCard,
                        Description = $"Pazar na dan {knjizenjePazaraDB.IssueDateTime.ToString("dd.MM.yyyy")} PROMET PRODAJA - PLATNA KARTICA"
                    };
                    sqliteDbContext.Kep.Add(kepDB);
                }
                RetryHelper.ExecuteWithRetry(() => { sqliteDbContext.SaveChanges(); });
            }
            if (knjizenjePazaraDB.NormalSaleWireTransfer != 0)
            {
                var kepPazar = sqliteDbContext.Kep.FirstOrDefault(kep => kep.KepDate.Date == knjizenjePazaraDB.IssueDateTime.Date &&
                kep.Type == (int)KepStateEnumeration.Dnevni_Pazar_Prodaja_Virman);

                if (kepPazar != null)
                {
                    kepPazar.Razduzenje = knjizenjePazaraDB.NormalSaleWireTransfer;
                    kepPazar.Zaduzenje += zaduzenje.NormalSaleWireTransfer;
                    sqliteDbContext.Kep.Update(kepPazar);
                }
                else
                {
                    KepDB kepDB = new KepDB()
                    {
                        Id = Guid.NewGuid().ToString(),
                        KepDate = knjizenjePazaraDB.IssueDateTime,
                        Type = (int)KepStateEnumeration.Dnevni_Pazar_Prodaja_Virman,
                        Razduzenje = knjizenjePazaraDB.NormalSaleWireTransfer,
                        Zaduzenje = zaduzenje.NormalSaleWireTransfer,
                        Description = $"Pazar na dan {knjizenjePazaraDB.IssueDateTime.ToString("dd.MM.yyyy")} PROMET PRODAJA - PRENOS NA RAČUN"
                    };
                    sqliteDbContext.Kep.Add(kepDB);
                }
                RetryHelper.ExecuteWithRetry(() => { sqliteDbContext.SaveChanges(); });
            }
            if (knjizenjePazaraDB.NormalRefundCash != 0)
            {
                var kepPazar = sqliteDbContext.Kep.FirstOrDefault(kep => kep.KepDate.Date == knjizenjePazaraDB.IssueDateTime.Date &&
                kep.Type == (int)KepStateEnumeration.Dnevni_Pazar_Refundacija_Gotovina);

                if (kepPazar != null)
                {
                    kepPazar.Razduzenje = knjizenjePazaraDB.NormalRefundCash;
                    kepPazar.Zaduzenje += zaduzenje.NormalRefundCash;
                    sqliteDbContext.Kep.Update(kepPazar);
                }
                else
                {
                    KepDB kepDB = new KepDB()
                    {
                        Id = Guid.NewGuid().ToString(),
                        KepDate = knjizenjePazaraDB.IssueDateTime,
                        Type = (int)KepStateEnumeration.Dnevni_Pazar_Refundacija_Gotovina,
                        Razduzenje = knjizenjePazaraDB.NormalRefundCash,
                        Zaduzenje = zaduzenje.NormalRefundCash,
                        Description = $"Pazar na dan {knjizenjePazaraDB.IssueDateTime.ToString("dd.MM.yyyy")} PROMET REFUNDACIJA - GOTOVINA"
                    };
                    sqliteDbContext.Kep.Add(kepDB);
                }
                RetryHelper.ExecuteWithRetry(() => { sqliteDbContext.SaveChanges(); });
            }
            if (knjizenjePazaraDB.NormalRefundCard != 0)
            {
                var kepPazar = sqliteDbContext.Kep.FirstOrDefault(kep => kep.KepDate.Date == knjizenjePazaraDB.IssueDateTime.Date &&
                kep.Type == (int)KepStateEnumeration.Dnevni_Pazar_Refundacija_Kartica);

                if (kepPazar != null)
                {
                    kepPazar.Razduzenje = knjizenjePazaraDB.NormalRefundCard;
                    kepPazar.Zaduzenje += zaduzenje.NormalRefundCard;
                    sqliteDbContext.Kep.Update(kepPazar);
                }
                else
                {
                    KepDB kepDB = new KepDB()
                    {
                        Id = Guid.NewGuid().ToString(),
                        KepDate = knjizenjePazaraDB.IssueDateTime,
                        Type = (int)KepStateEnumeration.Dnevni_Pazar_Refundacija_Kartica,
                        Razduzenje = knjizenjePazaraDB.NormalRefundCard,
                        Zaduzenje = zaduzenje.NormalRefundCard,
                        Description = $"Pazar na dan {knjizenjePazaraDB.IssueDateTime.ToString("dd.MM.yyyy")} PROMET REFUNDACIJA - PLATNA KARTICA"
                    };
                    sqliteDbContext.Kep.Add(kepDB);
                }
                RetryHelper.ExecuteWithRetry(() => { sqliteDbContext.SaveChanges(); });
            }
            if (knjizenjePazaraDB.NormalRefundWireTransfer != 0)
            {
                var kepPazar = sqliteDbContext.Kep.FirstOrDefault(kep => kep.KepDate.Date == knjizenjePazaraDB.IssueDateTime.Date &&
                kep.Type == (int)KepStateEnumeration.Dnevni_Pazar_Refundacija_Virman);

                if (kepPazar != null)
                {
                    kepPazar.Razduzenje = knjizenjePazaraDB.NormalRefundWireTransfer;
                    kepPazar.Zaduzenje += zaduzenje.NormalRefundWireTransfer;
                    sqliteDbContext.Kep.Update(kepPazar);
                }
                else
                {
                    KepDB kepDB = new KepDB()
                    {
                        Id = Guid.NewGuid().ToString(),
                        KepDate = knjizenjePazaraDB.IssueDateTime,
                        Type = (int)KepStateEnumeration.Dnevni_Pazar_Refundacija_Virman,
                        Razduzenje = knjizenjePazaraDB.NormalRefundWireTransfer,
                        Zaduzenje = zaduzenje.NormalRefundWireTransfer,
                        Description = $"Pazar na dan {knjizenjePazaraDB.IssueDateTime.ToString("dd.MM.yyyy")} PROMET REFUNDACIJA - PRENOS NA RAČUN"
                    };
                    sqliteDbContext.Kep.Add(kepDB);
                }
                RetryHelper.ExecuteWithRetry(() => { sqliteDbContext.SaveChanges(); });
            }

            if (nivelacija != 0)
            {
                var kep = sqliteDbContext.Kep.FirstOrDefault(kep => kep.KepDate.Date == knjizenjePazaraDB.IssueDateTime.Date &&
                kep.Type == (int)KepStateEnumeration.Nivelacija &&
                kep.Description.Contains("Nivelacija po pazaru"));

                if (kep != null)
                {
                    kep.Zaduzenje += nivelacija;
                    sqliteDbContext.Kep.Update(kep);
                }
                else
                {
                    KepDB kepDB = new KepDB()
                    {
                        Id = Guid.NewGuid().ToString(),
                        KepDate = knjizenjePazaraDB.IssueDateTime,
                        Type = (int)KepStateEnumeration.Nivelacija,
                        Razduzenje = 0,
                        Zaduzenje = nivelacija,
                        Description = $"Nivelacija po pazaru {knjizenjePazaraDB.IssueDateTime.ToString("dd.MM.yyyy")}"
                    };
                    sqliteDbContext.Kep.Add(kepDB);
                }
                RetryHelper.ExecuteWithRetry(() => { sqliteDbContext.SaveChanges(); });
            }
        }
    }
}