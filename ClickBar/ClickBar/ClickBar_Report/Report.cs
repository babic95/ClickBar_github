using ClickBar_Common.Enums;
using ClickBar_Common.Models.Invoice;
using ClickBar_Database;
using ClickBar_Database.Models;
using ClickBar_Report.Models;
using Microsoft.EntityFrameworkCore;

namespace ClickBar_Report
{
    public class Report
    {
        #region Fields
        private List<InvoiceDB> _invoices;
        private bool _includeItems;
        #endregion Fields

        #region Constructors
        public Report(DateTime startReport, DateTime endReport, bool includeItems)
        {
            StartReport = startReport;
            EndReport = endReport;

            _includeItems = includeItems;
            using (SqliteDbContext sqliteDbContext = new SqliteDbContext())
            {
                _invoices = sqliteDbContext.GetInvoiceForReport(startReport, endReport);
            }

            SetReport();
        }
        public Report(DateTime startReport, DateTime endReport, bool includeItems, string smartCard)
        {
            StartReport = startReport;
            EndReport = endReport;

            _includeItems = includeItems;
            using (SqliteDbContext sqliteDbContext = new SqliteDbContext())
            {

                _invoices = sqliteDbContext.GetInvoiceForReport(startReport, endReport, smartCard);
            }
            SetReport();
        }
        public Report(DateTime startReport, DateTime endReport, CashierDB cashier)
        {
            StartReport = startReport;
            EndReport = endReport;
            Cashier = cashier;

            _includeItems = false;
            using (SqliteDbContext sqliteDbContext = new SqliteDbContext())
            {
                _invoices = sqliteDbContext.GetInvoiceForReport(startReport, endReport, cashier);

                var refundInvoices = sqliteDbContext.Invoices.Where(invoice => invoice.TransactionType == 1 &&
                invoice.InvoiceType == 0 &&
                invoice.SdcDateTime >= startReport &&
                invoice.SdcDateTime <= endReport);

                if (refundInvoices != null &&
                    refundInvoices.Any())
                {
                    refundInvoices.ForEachAsync(refundInvoiceDB =>
                    {
                        var invoice = _invoices.FirstOrDefault(i => i.InvoiceNumberResult == refundInvoiceDB.ReferentDocumentNumber);

                        if (invoice != null)
                        {
                            _invoices.Remove(invoice);
                        }
                    });
                }
            }
            SetReport();
        }
        #endregion Constructors

        #region Properties
        public DateTime StartReport { get; set; }
        public DateTime EndReport { get; set; }
        public CashierDB Cashier { get; set; }
        //public Dictionary<string, ReportTax> ReportTaxes { get; set; }
        public List<Payment> Payments { get; set; }
        public Dictionary<string, Dictionary<string, ReportItem>> ReportItems { get; set; }
        public Dictionary<string, decimal> ReportCashiers { get; set; }
        //public Dictionary<InvoiceTypeEenumeration, List<ReportInvoiceType>> InvoiceTypes { get; set; }
        public decimal CashInHand { get; set; }
        public decimal TotalTraffic { get; set; }
        #endregion Properties

        #region Private method
        private async void SetReport()
        {
            //ReportTaxes = new Dictionary<string, ReportTax>();
            Payments = new List<Payment>();
            ReportItems = new Dictionary<string, Dictionary<string, ReportItem>>();
            ReportCashiers = new Dictionary<string, decimal>();
            //InvoiceTypes = new Dictionary<InvoiceTypeEenumeration, List<ReportInvoiceType>>();
            CashInHand = 0;
            TotalTraffic = 0;

            _invoices.ForEach(async invoice =>
            {
                if (invoice.SdcDateTime.HasValue)
                {
                    //await SetReportTaxes(invoice);
                    await SetPayments(invoice);
                    if (_includeItems)
                    {
                        await SetReportItems(invoice);

                        ReportItems.ToList().ForEach(item =>
                        {
                            ReportItems[item.Key] = item.Value.OrderBy(it => it.Key).ToDictionary(x => x.Key, x => x.Value);
                        });

                        //await SetInvoiceTypes(invoice);
                    }
                    await SetReportCashiers(invoice);
                }
            });
        }
        private async Task SetReportCashiers(InvoiceDB invoice)
        {
            if (invoice.TotalAmount.HasValue &&
                invoice.TransactionType.HasValue)
            {
                decimal total = invoice.TotalAmount.Value;

                if ((TransactionTypeEnumeration)invoice.TransactionType == TransactionTypeEnumeration.Refund)
                {
                    total *= -1;
                }
                SqliteDbContext db = new SqliteDbContext();
                var cashier = db.Cashiers.Find(invoice.Cashier);
                if (cashier != null)
                {
                    if (ReportCashiers.ContainsKey(cashier.Name))
                    {
                        ReportCashiers[cashier.Name] += total;
                    }
                    else
                    {
                        ReportCashiers.Add(cashier.Name, total);
                    }
                }
            }
        }
        private decimal CalculateGross(decimal rate, decimal amountRate)
        {
            decimal gross = ((100 + rate) * amountRate) / rate;
            return Decimal.Round(gross, 2);
        }
        //private async Task SetReportTaxes(InvoiceDB invoice)
        //{
        //    if (invoice.TotalAmount.HasValue)
        //    {
        //        SqliteDbContext sqliteDbContext = new SqliteDbContext();

        //        var taxes = await sqliteDbContext.GetAllTaxFromInvoice(invoice.Id);

        //        decimal totalGross = 0;
        //        List<string> zeroTaxes = new List<string>();

        //        if (taxes.Any())
        //        {
        //            taxes.ForEach(tax =>
        //            {
        //                if (tax.Rate > 0)
        //                {
        //                    ReportTax reportTax = new ReportTax()
        //                    {
        //                        Pdv = tax.Amount,
        //                        Gross = CalculateGross(tax.Rate, tax.Amount),
        //                        Rate = tax.Rate
        //                    };
        //                    reportTax.Net = reportTax.Gross - reportTax.Pdv;

        //                    totalGross += reportTax.Gross;

        //                    if (ReportTaxes.ContainsKey(tax.Label))
        //                    {
        //                        if (invoice.TransactionType == TransactionTypeEnumeration.Refund)
        //                        {
        //                            reportTax.Net *= -1;
        //                            reportTax.Pdv *= -1;
        //                            reportTax.Gross *= -1;
        //                        }

        //                        ReportTaxes[tax.Label].Net += reportTax.Net;
        //                        ReportTaxes[tax.Label].Pdv += reportTax.Pdv;
        //                        ReportTaxes[tax.Label].Gross += reportTax.Gross;
        //                    }
        //                    else
        //                    {
        //                        if (invoice.TransactionType == TransactionTypeEnumeration.Refund)
        //                        {
        //                            reportTax.Net *= -1;
        //                            reportTax.Pdv *= -1;
        //                            reportTax.Gross *= -1;
        //                        }

        //                        ReportTaxes.Add(tax.Label, reportTax);
        //                    }
        //                }
        //                else
        //                {
        //                    if (!zeroTaxes.Contains(tax.Label))
        //                    {
        //                        zeroTaxes.Add(tax.Label);
        //                    }
        //                }
        //            });

        //            if (zeroTaxes.Any())
        //            {
        //                decimal gross = (invoice.TotalAmount.Value - totalGross) / zeroTaxes.Count;

        //                zeroTaxes.ForEach(tax =>
        //                {
        //                    ReportTax reportTax = new ReportTax()
        //                    {
        //                        Pdv = 0,
        //                        Gross = gross,
        //                        Rate = 0,
        //                        Net = gross
        //                    };

        //                    if (ReportTaxes.ContainsKey(tax))
        //                    {
        //                        if (invoice.TransactionType == TransactionTypeEnumeration.Refund)
        //                        {
        //                            reportTax.Net *= -1;
        //                            reportTax.Pdv *= -1;
        //                            reportTax.Gross *= -1;
        //                        }

        //                        ReportTaxes[tax].Net += reportTax.Net;
        //                        ReportTaxes[tax].Pdv += reportTax.Pdv;
        //                        ReportTaxes[tax].Gross += reportTax.Gross;
        //                    }
        //                    else
        //                    {
        //                        if (invoice.TransactionType == TransactionTypeEnumeration.Refund)
        //                        {
        //                            reportTax.Net *= -1;
        //                            reportTax.Pdv *= -1;
        //                            reportTax.Gross *= -1;
        //                        }

        //                        ReportTaxes.Add(tax, reportTax);
        //                    }
        //                });
        //            }
        //        }
        //    }
        //}
        private async Task SetPayments(InvoiceDB invoice)
        {
            using (SqliteDbContext sqliteDbContext = new SqliteDbContext())
            {

                var payments = await sqliteDbContext.GetAllPaymentFromInvoice(invoice.Id);

                if (payments.Any())
                {
                    decimal totalPayment = 0;
                    payments.ForEach(payment =>
                    {
                        if (invoice.TransactionType.HasValue &&
                        payment.Amout.HasValue)
                        {
                            var pays = Payments.Where(pay => pay.PaymentType == payment.PaymentType).ToList();

                            if (pays != null &&
                            pays.Any())
                            {
                                Payment? pay = pays.FirstOrDefault();

                                if (pay != null)
                                {
                                    if ((TransactionTypeEnumeration)invoice.TransactionType.Value == TransactionTypeEnumeration.Sale)
                                    {
                                        pay.Amount += payment.Amout.Value;
                                        totalPayment += payment.Amout.Value;
                                    }
                                    else
                                    {
                                        pay.Amount -= payment.Amout.Value;
                                        totalPayment -= payment.Amout.Value;
                                    }
                                }
                            }
                            else
                            {
                                Payment pay = new Payment()
                                {
                                    Amount = payment.Amout.Value,
                                    PaymentType = payment.PaymentType
                                };

                                if ((TransactionTypeEnumeration)invoice.TransactionType == TransactionTypeEnumeration.Refund)
                                {
                                    pay.Amount *= -1;
                                }

                                totalPayment += pay.Amount;
                                Payments.Add(pay);
                            }

                            if ((TransactionTypeEnumeration)invoice.TransactionType == TransactionTypeEnumeration.Sale)
                            {
                                if (payment.PaymentType == PaymentTypeEnumeration.Cash)
                                {
                                    CashInHand += payment.Amout.Value;
                                }

                                TotalTraffic += payment.Amout.Value;
                            }
                            else
                            {
                                if (payment.PaymentType == PaymentTypeEnumeration.Cash)
                                {
                                    CashInHand -= payment.Amout.Value;
                                }

                                TotalTraffic -= payment.Amout.Value;
                            }
                        }
                    });

                    if (totalPayment > invoice.TotalAmount)
                    {
                        decimal change = totalPayment - invoice.TotalAmount.Value;

                        var pays = Payments.Where(pay => pay.PaymentType == PaymentTypeEnumeration.Cash).ToList();

                        if (pays != null &&
                            pays.Any())
                        {
                            Payment? pay = pays.FirstOrDefault();
                            if (pay != null)
                            {
                                pay.Amount -= change;
                            }
                        }

                        CashInHand -= change;
                        TotalTraffic -= change;
                    }
                }
            }
        }
        private async Task SetReportItems(InvoiceDB invoice)
        {
            using (SqliteDbContext sqliteDbContext = new SqliteDbContext()) { 

                var items = await sqliteDbContext.GetAllItemsFromInvoice(invoice.Id);

                if (items.Any())
                {
                    items.ForEach(item =>
                    {
                        var itemDB = sqliteDbContext.Items.Find(item.ItemCode);

                        if (itemDB != null)
                        {
                            var groupDB = sqliteDbContext.ItemGroups.Find(itemDB.IdItemGroup);

                            IEnumerable<ItemInNormDB>? norms;
                            if (itemDB.IdNorm != null &&
                            itemDB.IdNorm.HasValue &&
                            itemDB.IdNorm.Value > 0)
                            {
                                norms = sqliteDbContext.ItemsInNorm.Where(it => it.IdNorm == itemDB.IdNorm);

                                if (norms != null &&
                                norms.Any())
                                {
                                    norms.ToList().ForEach(norm =>
                                    {
                                        var itemNormDB = sqliteDbContext.Items.Find(norm.IdItem);

                                        if (itemNormDB != null)
                                        {
                                            var itemNormGroupDB = sqliteDbContext.ItemGroups.Find(itemNormDB.IdItemGroup);

                                            if (itemNormGroupDB != null)
                                            {
                                                if (ReportItems.ContainsKey(itemNormGroupDB.Name))
                                                {
                                                    if (ReportItems[itemNormGroupDB.Name].ContainsKey(itemNormDB.Id))
                                                    {
                                                        if (invoice.TransactionType == (int)TransactionTypeEnumeration.Sale)
                                                        {
                                                            ReportItems[itemNormGroupDB.Name][itemNormDB.Id].Quantity += (decimal)norm.Quantity;
                                                            ReportItems[itemNormGroupDB.Name][itemNormDB.Id].Gross += 0;
                                                        }
                                                        else
                                                        {
                                                            ReportItems[itemNormGroupDB.Name][itemNormDB.Id].Quantity -= (decimal)norm.Quantity;
                                                            ReportItems[itemNormGroupDB.Name][itemNormDB.Id].Gross -= 0;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        ReportItem reportItem = new ReportItem()
                                                        {
                                                            Name = itemNormDB.Name,
                                                            Quantity = (decimal)norm.Quantity,
                                                            Gross = 0
                                                        };

                                                        if (invoice.TransactionType == (int)TransactionTypeEnumeration.Refund)
                                                        {
                                                            reportItem.Quantity *= -1;
                                                        }

                                                        ReportItems[itemNormGroupDB.Name].Add(itemNormDB.Id, reportItem);
                                                    }
                                                }
                                                else
                                                {
                                                    Dictionary<string, ReportItem> pairs = new Dictionary<string, ReportItem>();
                                                    ReportItem reportItem = new ReportItem()
                                                    {
                                                        Name = itemNormDB.Name,
                                                        Quantity = (decimal)norm.Quantity,
                                                        Gross = 0
                                                    };

                                                    if (invoice.TransactionType == (int)TransactionTypeEnumeration.Refund)
                                                    {
                                                        reportItem.Quantity *= -1;
                                                    }

                                                    pairs.Add(itemNormDB.Id, reportItem);
                                                    ReportItems.Add(itemNormGroupDB.Name, pairs);
                                                }
                                            }
                                        }
                                    });
                                }
                            }

                            if (ReportItems.ContainsKey(groupDB.Name))
                            {
                                if (ReportItems[groupDB.Name].ContainsKey(item.ItemCode))
                                {
                                    if (invoice.TransactionType == (int)TransactionTypeEnumeration.Sale)
                                    {
                                        ReportItems[groupDB.Name][item.ItemCode].Quantity += (decimal)item.Quantity;
                                        ReportItems[groupDB.Name][item.ItemCode].Gross += (decimal)item.TotalAmout;
                                    }
                                    else
                                    {
                                        ReportItems[groupDB.Name][item.ItemCode].Quantity -= (decimal)item.Quantity;
                                        ReportItems[groupDB.Name][item.ItemCode].Gross -= (decimal)item.TotalAmout;
                                    }
                                }
                                else
                                {
                                    ReportItem reportItem = new ReportItem()
                                    {
                                        Name = item.Name,
                                        Quantity = (decimal)item.Quantity,
                                        Gross = (decimal)item.TotalAmout
                                    };

                                    if (invoice.TransactionType == (int)TransactionTypeEnumeration.Refund)
                                    {
                                        reportItem.Gross *= -1;
                                        reportItem.Quantity *= -1;
                                    }

                                    ReportItems[groupDB.Name].Add(item.ItemCode, reportItem);
                                }
                            }
                            else
                            {
                                Dictionary<string, ReportItem> pairs = new Dictionary<string, ReportItem>();
                                ReportItem reportItem = new ReportItem()
                                {
                                    Name = item.Name,
                                    Quantity = (decimal)item.Quantity,
                                    Gross = (decimal)item.TotalAmout
                                };

                                if (invoice.TransactionType == (int)TransactionTypeEnumeration.Refund)
                                {
                                    reportItem.Gross *= -1;
                                    reportItem.Quantity *= -1;
                                }

                                pairs.Add(item.ItemCode, reportItem);
                                ReportItems.Add(groupDB.Name, pairs);
                            }
                        }
                    });
                }
            }
        }
        //private async Task SetInvoiceTypes(InvoiceDB invoice)
        //{
        //    if (invoice.TotalAmount.HasValue)
        //    {
        //        ReportInvoiceType reportInvoiceType = new ReportInvoiceType()
        //        {
        //            Cashier = invoice.Cashier,
        //            Gross = invoice.TotalAmount.Value
        //        };

        //        if (invoice.TransactionType == TransactionTypeEnumeration.Refund)
        //        {
        //            reportInvoiceType.Gross *= -1;
        //        }


        //        if (InvoiceTypes.ContainsKey(invoice.InvoiceType))
        //        {
        //            InvoiceTypes[invoice.InvoiceType].Add(reportInvoiceType);
        //        }
        //        else
        //        {
        //            InvoiceTypes.Add(invoice.InvoiceType, new List<ReportInvoiceType>() { reportInvoiceType });
        //        }
        //    }
        //}
        #endregion Private method
    }
}
