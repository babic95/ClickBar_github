using ClickBar.ViewModels;
using ClickBar.Views.AppMain.AuxiliaryWindows;
using ClickBar_Common.Enums;
using ClickBar_Database;
using ClickBar_DatabaseSQLManager.Models;
using ClickBar_Logging;
using ClickBar_Settings;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ClickBar.Commands.AppMain.AuxiliaryWindows
{
    public class CopyInvoicesFromESIRCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private AppMainViewModel _viewModel;
        public CopyInvoicesFromESIRCommand(AppMainViewModel appMainViewModel)
        {
            _viewModel = appMainViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            try
            {
                // Prva baza
                string firstDatabasePath = SettingsManager.Instance.GetESIRBaza();
                string firstConnectionString = $"Data Source={firstDatabasePath};Version=3;";

                using (var dbContext = _viewModel.DbContextFactory.CreateDbContext())
                {
                    using (SQLiteConnection firstConnection = new SQLiteConnection(firstConnectionString))
                    {
                        firstConnection.Open();

                        var invoicesDB = dbContext.Invoices.Where(i => i.InvoiceNumberResult == null &&
                        (i.InvoiceType == (int)InvoiceTypeEenumeration.Normal || i.InvoiceType == (int)InvoiceTypeEenumeration.Advance)).ToList();

                        if (invoicesDB.Any())
                        {
                            foreach (var invoice in invoicesDB)
                            {
                                // Pronađi prethodni račun sa InvoiceNumberResult != null koristeći DateAndTimeOfIssue
                                var previousInvoice = dbContext.Invoices
                                    .Where(i => i.InvoiceNumberResult != null && i.DateAndTimeOfIssue < invoice.DateAndTimeOfIssue)
                                    .OrderByDescending(i => i.DateAndTimeOfIssue)
                                    .FirstOrDefault();

                                // Pronađi sledeći račun sa InvoiceNumberResult != null koristeći DateAndTimeOfIssue
                                var nextInvoice = dbContext.Invoices
                                    .Where(i => i.InvoiceNumberResult != null && i.DateAndTimeOfIssue > invoice.DateAndTimeOfIssue)
                                    .OrderBy(i => i.DateAndTimeOfIssue)
                                    .FirstOrDefault();

                                // Logika za popunjavanje podataka
                                if (previousInvoice != null && nextInvoice != null)
                                {
                                    int previousInvoiceTotalCounter = Convert.ToInt32(previousInvoice.InvoiceNumberResult.Split('-')[2]);
                                    int nextInvoiceTotalCounter = Convert.ToInt32(nextInvoice.InvoiceNumberResult.Split('-')[2]);

                                    var razlika = nextInvoiceTotalCounter - previousInvoiceTotalCounter;

                                    if (razlika > 1)
                                    {
                                        for (int i = 1; i < razlika; i++)
                                        {
                                            // Pronađi podatke iz InvoiceResult u prvoj bazi
                                            string invoiceResultQuery = "SELECT * FROM Invoice WHERE InvoiceNumberResult = @invoiceResultId";

                                            string invoiceResultNumber = $"{previousInvoice.InvoiceNumberResult.Split('-')[0]}-{previousInvoice.InvoiceNumberResult.Split('-')[1]}-{previousInvoiceTotalCounter + i}";

                                            string invoiceId = string.Empty;

                                            using (SQLiteCommand command = new SQLiteCommand(invoiceResultQuery, firstConnection))
                                            {
                                                command.Parameters.AddWithValue("@invoiceResultId", invoiceResultNumber);

                                                using (SQLiteDataReader reader = command.ExecuteReader())
                                                {
                                                    // Definišite vremensku zonu Beograda
                                                    //TimeZoneInfo beogradTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");

                                                    if (reader.Read())
                                                    {
                                                        decimal totalAmount = Convert.ToDecimal(reader["TotalAmount"]);

                                                        decimal itemTotalAmount = dbContext.ItemInvoices
                                                            .Where(i => i.InvoiceId == invoice.Id &&
                                                            i.IsSirovina == 0 &&
                                                            i.TotalAmout.HasValue)
                                                            .AsEnumerable()
                                                            .Sum(i => i.TotalAmout.Value);

                                                        if (totalAmount != itemTotalAmount)
                                                        {
                                                            continue;
                                                        }

                                                        invoiceId = reader["Id"].ToString();
                                                        invoice.InvoiceNumberResult = reader["InvoiceNumber"].ToString();
                                                        invoice.SdcDateTime = Convert.ToDateTime(reader["SdcDateTime"].ToString());
                                                        invoice.InvoiceCounter = reader["InvoiceCounter"].ToString();
                                                        invoice.TotalAmount = totalAmount;
                                                    }
                                                }
                                            }

                                            // Pronađi podatke iz TaxItem u prvoj bazi
                                            string taxItemQuery = "SELECT * FROM TaxItemInvoice WHERE InvoiceId = @invoiceId";

                                            using (SQLiteCommand taxCommand = new SQLiteCommand(taxItemQuery, firstConnection))
                                            {
                                                taxCommand.Parameters.AddWithValue("@invoiceId", invoiceId);

                                                using (SQLiteDataReader taxReader = taxCommand.ExecuteReader())
                                                {
                                                    while (taxReader.Read())
                                                    {
                                                        var taxItemInvoice = new TaxItemInvoiceDB
                                                        {
                                                            InvoiceId = invoice.Id,
                                                            Label = taxReader["Label"].ToString(),
                                                            CategoryName = taxReader["CategoryName"].ToString(),
                                                            CategoryType = Convert.ToInt32(taxReader["CategoryType"]),
                                                            Rate = Convert.ToDecimal(taxReader["Rate"]),
                                                            Amount = Convert.ToDecimal(taxReader["Amount"])
                                                        };

                                                        // Upis u drugu bazu
                                                        dbContext.TaxItemInvoices.Add(taxItemInvoice);
                                                    }
                                                }
                                            }

                                            dbContext.Invoices.Update(invoice);

                                            break;
                                        }
                                    }
                                }
                            }

                            dbContext.SaveChanges();
                        }
                    }
                }
                MessageBox.Show("Uspešno ste prepisali sve podatke iz prve baze u drugu bazu.", "", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Log.Error("GRESKA U REFUNDACIJI", ex);
                MessageBox.Show("Greška prilikom prepisivanja", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
