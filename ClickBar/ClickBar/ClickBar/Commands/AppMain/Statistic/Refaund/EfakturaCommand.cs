using ClickBar.Enums.Sale;
using ClickBar.Models.Sale;
using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar_Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using ClickBar_Settings;
using ClickBar_eFaktura;
using DocumentFormat.OpenXml.Office2010.Excel;
using ClickBar_eFaktura.Models;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using ClickBar_Database.Models;
using ClickBar.Models.AppMain.Statistic;
using System.Xml.Serialization;
using System.Xml;
using System.IO;

namespace ClickBar.Commands.AppMain.Statistic.Refaund
{
    public class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
    }
    public class EfakturaCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private RefaundViewModel _currentViewModel;
        private bool _sendToCirBool;
        private string _apyKey;

        public EfakturaCommand(RefaundViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            if (string.IsNullOrEmpty(_currentViewModel.RefNumber) ||
                string.IsNullOrEmpty(_currentViewModel.RefDateDay) ||
                string.IsNullOrEmpty(_currentViewModel.RefDateMonth) ||
                string.IsNullOrEmpty(_currentViewModel.RefDateYear) ||
                string.IsNullOrEmpty(_currentViewModel.RefDateHour) ||
                string.IsNullOrEmpty(_currentViewModel.RefDateMinute) ||
                string.IsNullOrEmpty(_currentViewModel.RefDateSecond) ||
                _currentViewModel.CurrentInvoice is null)
            {
                MessageBox.Show("Selektujte račun u tabeli!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {
                var eFakturaDirectory = SettingsManager.Instance.GetEfakturaDirectory();

                if(string.IsNullOrEmpty(eFakturaDirectory) )
                {
                    MessageBox.Show("Putanja do eFakture nije uneta!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var result = MessageBox.Show("Da li ste sigurni da želite da kreirate eFakturu na osnovu selektovanog računa?", "", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _sendToCirBool = false;

                    var invoice = await CreateInvoice();

                    if(invoice == null)
                    {
                        return;
                    }


                    string path = Path.Combine(eFakturaDirectory, $"{invoice.ID}.xml");

                    string invoiceXmlString = string.Empty;

                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Indent = true;
                    settings.Encoding = Encoding.UTF8;

                    using (XmlWriter writer = XmlWriter.Create(path, settings))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(ClickBar_eFaktura.Models.Invoice));

                        XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
                        //namespaces.Add(string.Empty, "urn:carrier:names:specification:ubl:schema:xsd:CarrierDealerContractorInvoice-1.0");
                        namespaces.Add("cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
                        namespaces.Add("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
                        namespaces.Add("cec", "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");
                        //namespaces.Add("ibc", "urn:carrier:names:specification:ubl:schema:xsd:CarrierDealerContractorInvoiceBasicComponents-1.0");
                        //namespaces.Add("iac", "urn:carrier:names:specification:ubl:schema:xsd:CarrierDealerContractorInvoiceAggregateComponents-1.0");
                        //namespaces.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");

                        serializer.Serialize(writer, invoice, namespaces);
                        //serializer.Serialize(writer, invoice);
                        writer.Close();
                    }
                    string idRequest = "000001";
                    try
                    {
                        idRequest = Convert.ToInt32(invoice.ID.Split("-")[2]).ToString("000000");
                    }
                    catch
                    {

                    }

                    bool isSend = await eFakturaManager.Instance.ImportSalesInvoiceXmlFile(path, _apyKey, idRequest, _sendToCirBool);
                    //bool isSend = await eFakturaManager.Instance.SendSalesInvoice(invoice.ID, invoiceXmlString, _apyKey, idRequest, _sendToCirBool);

                    if (isSend)
                    {
                        _currentViewModel.RefaundCommand.Execute("eFaktura");

                        MessageBox.Show("Uspešno poslata eFaktura!", "Uspešno", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Greška prilikom izdavanja eFakture!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        private async Task<ClickBar_eFaktura.Models.Invoice?> CreateInvoice()
        {
            if (string.IsNullOrEmpty(_currentViewModel.CurrentInvoice.BuyerId))
            {
                MessageBox.Show("eFaktura može da se pošalje samo pravnim licima ili budžetskim korisnicima!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
            using (SqliteDbContext sqliteDbContext = new SqliteDbContext())
            {
                var sender = sqliteDbContext.Firmas.FirstOrDefault();

                if (sender == null ||
                    string.IsNullOrEmpty(sender.Pib) ||
                    string.IsNullOrEmpty(sender.BankAcc) ||
                    string.IsNullOrEmpty(sender.AuthenticationKey))
                {
                    MessageBox.Show("Morate unite podatke Vaše firme!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }

                _apyKey = sender.AuthenticationKey;
                DateTime date = DateTime.Now;
                ClickBar_eFaktura.Models.Invoice? invoice = new ClickBar_eFaktura.Models.Invoice()
                {
                    CustomizationID = "urn:cen.eu:en16931:2017#compliant#urn:mfin.gov.rs:srbdt:2021",
                    ProfileID = "CCS eFaktura",
                    ID = _currentViewModel.CurrentInvoice.InvoiceNumber,
                    IssueDate = date.ToString("yyyy-MM-dd"),
                    DueDate = date.AddDays(15).ToString("yyyy-MM-dd"),
                    InvoiceTypeCode = 380,
                    DocumentCurrencyCode = "RSD",
                    InvoicePeriod = new InvoicePeriod()
                    {
                        DescriptionCode = 35
                    },
                    Delivery = new Delivery()
                    {
                        ActualDeliveryDate = _currentViewModel.CurrentInvoice.SdcDateTime.Date.ToString("yyyy-MM-dd"),
                    },
                    PaymentMeans = new PaymentMeans()
                    {
                        PaymentMeansCode = "30",
                        InstructionNote = "Plaćanje po računu",
                        PaymentID = $"{_currentViewModel.CurrentInvoice.InvoiceNumber}",
                        PayeeFinancialAccount = new PaymentMeansPayeeFinancialAccount()
                        {
                            ID = sender.BankAcc
                        }
                    },
                };

                ClickBar_eFaktura.Models.Request.Envelope requestSender = new ClickBar_eFaktura.Models.Request.Envelope()
                {
                    Header = new ClickBar_eFaktura.Models.Request.EnvelopeHeader()
                    {
                        AuthenticationHeader = new ClickBar_eFaktura.Models.Request.AuthenticationHeader()
                        {
                            LicenceID = "ef42df23-7658-418d-991e-3f9f6fdc37f4",
                            Password = "U3XKag3b",
                            UserName = "cleancodesirmium"
                        }
                    },
                    Body = new ClickBar_eFaktura.Models.Request.EnvelopeBody()
                    {
                        GetCompany = new ClickBar_eFaktura.Models.Request.GetCompany()
                        {
                            taxIdentificationNumber = $"{sender.Pib}"
                        }
                    }
                };

                var responseSender = await eFakturaManager.Instance.GetPravnaLica(requestSender);

                if (responseSender == null)
                {
                    MessageBox.Show("Greška u podacima Vaše firme!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }

                invoice.AccountingSupplierParty = new ClickBar_eFaktura.Models.AccountingSupplierParty()
                {
                    Party = new ClickBar_eFaktura.Models.AccountingSupplierPartyParty()
                    {
                        EndpointID = new ClickBar_eFaktura.Models.EndpointID()
                        {
                            schemeID = 9948,
                            Value = responseSender.pib
                        },
                        PartyName = new ClickBar_eFaktura.Models.AccountingSupplierPartyPartyPartyName()
                        {
                            Name = responseSender.naziv
                        },
                        PartyTaxScheme = new ClickBar_eFaktura.Models.AccountingSupplierPartyPartyPartyTaxScheme()
                        {
                            CompanyID = $"RS{responseSender.pib}",
                            TaxScheme = new ClickBar_eFaktura.Models.AccountingSupplierPartyPartyPartyTaxSchemeTaxScheme()
                            {
                                ID = "VAT"
                            }
                        },
                        PartyLegalEntity = new ClickBar_eFaktura.Models.AccountingSupplierPartyPartyPartyLegalEntity()
                        {
                            RegistrationName = responseSender.naziv,
                            CompanyID = responseSender.mb,
                        },
                        PostalAddress = new ClickBar_eFaktura.Models.AccountingSupplierPartyPartyPostalAddress()
                        {
                            CityName = responseSender.mesto,
                            StreetName = responseSender.adresa,
                            Country = new ClickBar_eFaktura.Models.AccountingSupplierPartyPartyPostalAddressCountry()
                            {
                                IdentificationCode = "RS"
                            }
                        }
                    },
                };

                if (_currentViewModel.CurrentInvoice.BuyerId.Contains("12:"))
                {
                    invoice.ContractDocumentReference = new ContractDocumentReference()
                    {
                        ID = invoice.ID
                    };
                    invoice = await CreateInvoiceBudzetskiKorisnik(invoice);
                }
                else
                {
                    invoice = await CreateInvoicePravniKorisnik(invoice);
                }

                if (invoice == null)
                {
                    return null;
                }

                invoice = await SetTax(sqliteDbContext, invoice);

                if (invoice != null)
                {
                    invoice = await SetItems(sqliteDbContext, invoice);
                }

                return invoice;
            }
        }
        private string GetTaxID(string label)
        {
            switch (label)
            {
                case "Ђ":
                    return "S";
                case "6":
                    return "S";
                case "Е":
                    return "S";
                case "7":
                    return "S";
                case "Г":
                    return "S";
                case "4":
                    return "S";
                case "А":
                    return "SS";
                case "1":
                    return "SS";
                case "Ж":
                    return "S";
                case "8":
                    return "S";
                case "A":
                    return "S";
                case "31":
                    return "S";
                case "N":
                    return "SS";
                case "47":
                    return "SS";
                case "F":
                    return "S";
                case "39":
                    return "S";
            }

            return string.Empty;
        }
        private async Task<ClickBar_eFaktura.Models.Invoice?> SetTax(SqliteDbContext sqliteDbContext, 
            ClickBar_eFaktura.Models.Invoice? invoice)
        {
            if (invoice != null)
            {
                var taxDB = sqliteDbContext.TaxItemInvoices.Where(tax => tax.InvoiceId == _currentViewModel.CurrentInvoice.Id);

                if (taxDB == null ||
                    !taxDB.Any())
                {
                    MessageBox.Show($"Greška u poreskim stopama za račun {_currentViewModel.CurrentInvoice.InvoiceNumber} (nema ih)!",
                        "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }

                invoice.TaxTotal = new TaxTotal()
                {
                    TaxAmount = new TaxAmount()
                    {
                        currencyID = "RSD",
                        Value = 0
                    },
                    TaxSubtotal = new List<TaxTotalTaxSubtotal>(),
                };

                await taxDB.ForEachAsync(tax =>
                {
                    if (tax.Amount.HasValue &&
                    tax.Rate.HasValue)
                    {
                        string taxID = GetTaxID(tax.Label);
                        if (string.IsNullOrEmpty(taxID))
                        {
                            MessageBox.Show($"Greška u poreskim stopama za račun {_currentViewModel.CurrentInvoice.InvoiceNumber} (NEISPRAVNE)!",
                                "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                            invoice = null;
                            return;
                        }
                        TaxTotalTaxSubtotal taxTotalTaxSubtotal = new TaxTotalTaxSubtotal()
                        {
                            TaxableAmount = new TaxableAmount()
                            {
                                currencyID = "RSD",
                                Value = tax.Rate.Value != 0 ? Decimal.Round((tax.Amount.Value * 100) / tax.Rate.Value, 2) : Decimal.Round(tax.Amount.Value, 2)
                            },
                            TaxAmount = new TaxAmount()
                            {
                                currencyID = "RSD",
                                Value = Decimal.Round(tax.Amount.Value, 2)
                            },
                            TaxCategory = new TaxTotalTaxSubtotalTaxCategory()
                            {
                                Percent = tax.Rate.Value.ToString(),
                                TaxScheme = new TaxTotalTaxSubtotalTaxCategoryTaxScheme()
                                {
                                    ID = $"VAT"
                                },
                                ID = $"{taxID}"
                            }
                        };
                        if(taxID == "SS")
                        {
                            taxTotalTaxSubtotal.TaxCategory.TaxExemptionReasonCode = "PDV-RS-33";
                        }

                        invoice.TaxTotal.TaxSubtotal.Add(taxTotalTaxSubtotal);
                        invoice.TaxTotal.TaxAmount.Value += Decimal.Round(tax.Amount.Value, 2);
                    }
                });
            }

            return invoice;
        }
        private async Task<ClickBar_eFaktura.Models.Invoice?> SetItems(SqliteDbContext sqliteDbContext, 
            ClickBar_eFaktura.Models.Invoice invoice)
        {
            var itemInInvoiceDB = sqliteDbContext.ItemInvoices.Where(item => 
            item.InvoiceId == _currentViewModel.CurrentInvoice.Id &&
            (item.IsSirovina == null || item.IsSirovina == 0));

            if (itemInInvoiceDB == null ||
                !itemInInvoiceDB.Any())
            {
                MessageBox.Show($"Greška u artiklima za račun {_currentViewModel.CurrentInvoice.InvoiceNumber} (nema ih)!",
                    "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            decimal netoIznos = 0; //neto iznos iz stavki racuna
            decimal brutoIznos = 0; //ukupan iznos sa PDV
            decimal allowanceTotalAmount = 0; // zbir popusta na nivou dokumenta
            decimal prepaidAmount = 0; // placeni iznos
            decimal payableRoundingAmount = 0; // iznos zaokruzivanja

            invoice.InvoiceLine = new List<InvoiceLine>();

            await itemInInvoiceDB.ForEachAsync(item =>
            {
                if (item.TotalAmout.HasValue &&
                !string.IsNullOrEmpty(item.Label))
                {
                    string taxID = GetTaxID(item.Label);
                    if (string.IsNullOrEmpty(taxID))
                    {
                        MessageBox.Show($"Greška u poreskim stopama za račun {_currentViewModel.CurrentInvoice.InvoiceNumber} (NEISPRAVNE)!",
                            "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                        invoice = null;
                        return;
                    }

                    brutoIznos += item.TotalAmout.Value;

                    decimal taxAmount = 0;
                    switch (item.Label)
                    {
                        case "Ђ":
                            taxAmount = 20;
                            netoIznos += Decimal.Round((item.TotalAmout.Value * 100) / (100 + taxAmount), 2);
                            break;
                        case "6":
                            taxAmount = 20;
                            netoIznos += Decimal.Round((item.TotalAmout.Value * 100) / (100 + taxAmount), 2);
                            break;
                        case "Е":
                            taxAmount = 10;
                            netoIznos += Decimal.Round((item.TotalAmout.Value * 100) / (100 + taxAmount), 2);
                            break;
                        case "7":
                            taxAmount = 10;
                            netoIznos += Decimal.Round((item.TotalAmout.Value * 100) / (100 + taxAmount), 2);
                            break;
                        case "Г":
                            taxAmount = 0;
                            netoIznos += Decimal.Round(item.TotalAmout.Value, 2);
                            break;
                        case "4":
                            taxAmount = 0;
                            netoIznos += Decimal.Round(item.TotalAmout.Value, 2);
                            break;
                        case "А":
                            taxAmount = 0;
                            netoIznos += Decimal.Round(item.TotalAmout.Value, 2);
                            break;
                        case "1":
                            taxAmount = 0;
                            netoIznos += Decimal.Round(item.TotalAmout.Value, 2);
                            break;
                        case "Ж":
                            taxAmount = 19;
                            netoIznos += Decimal.Round((item.TotalAmout.Value * 100) / (100 + taxAmount), 2);
                            break;
                        case "8":
                            taxAmount = 19;
                            netoIznos += Decimal.Round((item.TotalAmout.Value * 100) / (100 + taxAmount), 2);
                            break;
                        case "A":
                            taxAmount = 9;
                            netoIznos += Decimal.Round((item.TotalAmout.Value * 100) / (100 + taxAmount), 2);
                            break;
                        case "31":
                            taxAmount = 9;
                            netoIznos += Decimal.Round((item.TotalAmout.Value * 100) / (100 + taxAmount), 2);
                            break;
                        case "N":
                            taxAmount = 0;
                            netoIznos += Decimal.Round(item.TotalAmout.Value, 2);
                            break;
                        case "47":
                            taxAmount = 0;
                            netoIznos += Decimal.Round(item.TotalAmout.Value, 2);
                            break;
                        case "F":
                            taxAmount = 11;
                            netoIznos += Decimal.Round(item.TotalAmout.Value,2 );
                            break;
                        case "39":
                            taxAmount = 11;
                            netoIznos += Decimal.Round((item.TotalAmout.Value * 100) / (100 + taxAmount), 2);
                            break;
                    }

                    string jm = SetJM(sqliteDbContext, item);

                    if (string.IsNullOrEmpty(jm))
                    {
                        MessageBox.Show($"Greška u jedinici mere artikla {item.ItemCode} - {item.Name} u racunu {_currentViewModel.CurrentInvoice.InvoiceNumber}!",
                            "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                        invoice = null;
                        return;
                    }

                    InvoiceLine invoiceLine = new InvoiceLine()
                    {
                        ID = item.ItemCode,
                        InvoicedQuantity = new InvoicedQuantity
                        {
                            unitCode = jm,
                            Value = item.Quantity.Value
                        },
                        LineExtensionAmount = new LineExtensionAmount()
                        {
                            currencyID = "RSD",
                            Value = item.TotalAmout.Value
                        },
                        Item = new InvoiceLineItem()
                        {
                            Name = item.Name,
                            SellersItemIdentification = new InvoiceLineItemSellersItemIdentification()
                            {
                                ID = item.ItemCode
                            },
                            ClassifiedTaxCategory = new InvoiceLineItemClassifiedTaxCategory()
                            {
                                ID = $"{taxID}",
                                Percent = Decimal.Round(taxAmount, 2).ToString(),
                                TaxScheme = new InvoiceLineItemClassifiedTaxCategoryTaxScheme()
                                {
                                    ID = "VAT"
                                }
                            }
                        },
                        Price = new InvoiceLinePrice()
                        {
                            PriceAmount = new PriceAmount()
                            {
                                currencyID = "RSD",
                                Value = Decimal.Round(item.TotalAmout.Value / item.Quantity.Value, 2)
                            }
                        }
                    };

                    invoice.InvoiceLine.Add(invoiceLine);
                }
            });

            invoice.LegalMonetaryTotal = new LegalMonetaryTotal()
            {
                LineExtensionAmount = new LineExtensionAmount()
                {
                    currencyID = "RSD",
                    Value = Decimal.Round(netoIznos, 2)
                },
                TaxExclusiveAmount = new TaxExclusiveAmount()
                {
                    currencyID = "RSD",
                    Value = Decimal.Round(netoIznos, 2)
                },
                TaxInclusiveAmount = new TaxInclusiveAmount()
                {
                    currencyID = "RSD",
                    Value = Decimal.Round(brutoIznos, 2)
                },
                AllowanceTotalAmount = new AllowanceTotalAmount()
                {
                    currencyID = "RSD",
                    Value = Decimal.Round(allowanceTotalAmount, 2).ToString()
                },
                PrepaidAmount = new PrepaidAmount()
                {
                    currencyID = "RSD",
                    Value = Decimal.Round(prepaidAmount, 2).ToString()
                },
                PayableAmount = new PayableAmount()
                {
                    currencyID = "RSD",
                    Value = Decimal.Round(brutoIznos, 2)
                },
                PayableRoundingAmount = new PrepaidAmount()
                {
                    currencyID = "RSD",
                    Value = Decimal.Round(payableRoundingAmount, 2).ToString()
                }
            };
            return invoice;
        }
        private string SetJM(SqliteDbContext sqliteDbContext, ItemInvoiceDB itemInvoice)
        {
            var itemDB = sqliteDbContext.Items.Find(itemInvoice.ItemCode);

            if(itemDB != null )
            {
                switch (itemDB.Jm.ToLower())
                {
                    case "kom":
                        return "H87";
                    case "kg":
                        return "KGM";
                    case "km":
                        return "KMT";
                    case "g":
                        return "GRM";
                    case "m":
                        return "MTR";
                    case "l":
                        return "LTR";
                    case "t":
                        return "TNE";
                    case "m2":
                        return "MTK";
                    case "m3":
                        return "MTQ";
                    case "min":
                        return "MIN";
                    case "h":
                        return "HUR";
                    case "day":
                        return "DAY";
                    case "mon":
                        return "MON";
                    case "god":
                        return "ANN";
                    case "kwh":
                        return "KWH";
                }
            }
            return string.Empty;
        }
        private async Task<ClickBar_eFaktura.Models.Invoice?> CreateInvoiceBudzetskiKorisnik(ClickBar_eFaktura.Models.Invoice invoice)
        {
            try
            {
                var splitPib = _currentViewModel.CurrentInvoice.BuyerId.Split(":");

                if (splitPib.Length != 3)
                {
                    MessageBox.Show("Greška u podacima budzetskog korisnika!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);

                    return null;
                }

                var response = await eFakturaManager.Instance.GetBudzetskiKorisnik(splitPib[2]);

                if (response == null)
                {
                    MessageBox.Show("Greška u podacima budzetskog korisnika!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }

                invoice.AccountingCustomerParty = new AccountingCustomerParty()
                {
                    Party = new AccountingCustomerPartyParty()
                    {
                        EndpointID = new ClickBar_eFaktura.Models.EndpointID()
                        {
                            schemeID = 9948,
                            Value = response.pib
                        },
                        PartyIdentification = new AccountingCustomerPartyPartyPartyIdentification()
                        {
                            ID = $"JBKJS:{response.jbbk}"
                        },
                        PartyName = new AccountingCustomerPartyPartyPartyName
                        {
                            Name = response.naziv
                        },
                        PartyTaxScheme = new AccountingCustomerPartyPartyPartyTaxScheme
                        {
                            CompanyID = $"RS{response.pib}",
                            TaxScheme = new AccountingCustomerPartyPartyPartyTaxSchemeTaxScheme
                            {
                                ID = "VAT"
                            }
                        },
                        PartyLegalEntity = new AccountingCustomerPartyPartyPartyLegalEntity
                        {
                            RegistrationName = response.naziv,
                            CompanyID = response.mb,
                        },
                        PostalAddress = new AccountingCustomerPartyPartyPostalAddress
                        {
                            CityName = response.mesto,
                            StreetName = response.adresa,
                            Country = new AccountingCustomerPartyPartyPostalAddressCountry
                            {
                                IdentificationCode = "RS"
                            }
                        }
                    },
                };

                if (response.tip.Contains("0") ||
                   response.tip.Contains("1") ||
                   response.tip.Contains("2") ||
                   response.tip.Contains("4") ||
                   response.tip.Contains("5") ||
                   response.tip.Contains("6") ||
                   response.tip.Contains("9") ||
                   response.tip.Contains("10") ||
                   response.tip.Contains("11"))
                {
                    _sendToCirBool = true;
                }
            
                return invoice;
            }
            catch (Exception ex)
            {
                return null;
            }

            return null;
        }
        private async Task<ClickBar_eFaktura.Models.Invoice?> CreateInvoicePravniKorisnik(ClickBar_eFaktura.Models.Invoice invoice)
        {
            try
            {
                string pib = _currentViewModel.CurrentInvoice.BuyerId.Split(":")[1];

                ClickBar_eFaktura.Models.Request.Envelope request = new ClickBar_eFaktura.Models.Request.Envelope()
                {
                    Header = new ClickBar_eFaktura.Models.Request.EnvelopeHeader()
                    {
                        AuthenticationHeader = new ClickBar_eFaktura.Models.Request.AuthenticationHeader()
                        {
                            LicenceID = "ef42df23-7658-418d-991e-3f9f6fdc37f4",
                            Password = "U3XKag3b",
                            UserName = "cleancodesirmium"
                        }
                    },
                    Body = new ClickBar_eFaktura.Models.Request.EnvelopeBody()
                    {
                        GetCompany = new ClickBar_eFaktura.Models.Request.GetCompany()
                        {
                            taxIdentificationNumber = $"{pib}"
                        }
                    }
                };

                var response = await eFakturaManager.Instance.GetPravnaLica(request);

                if (response == null)
                {
                    MessageBox.Show("Greška u podacima pravnig lica!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }

                invoice.AccountingCustomerParty = new AccountingCustomerParty()
                {
                    Party = new AccountingCustomerPartyParty()
                    {
                        EndpointID = new ClickBar_eFaktura.Models.EndpointID()
                        {
                            schemeID = 9948,
                            Value = response.pib
                        },
                        PartyName = new AccountingCustomerPartyPartyPartyName
                        {
                            Name = response.naziv
                        },
                        PartyTaxScheme = new AccountingCustomerPartyPartyPartyTaxScheme
                        {
                            CompanyID = $"RS{response.pib}",
                            TaxScheme = new AccountingCustomerPartyPartyPartyTaxSchemeTaxScheme
                            {
                                ID = "VAT"
                            }
                        },
                        PartyLegalEntity = new AccountingCustomerPartyPartyPartyLegalEntity
                        {
                            RegistrationName = response.naziv,
                            CompanyID = response.mb,
                        },
                        PostalAddress = new AccountingCustomerPartyPartyPostalAddress
                        {
                            CityName = response.mesto,
                            StreetName = response.adresa,
                            Country = new AccountingCustomerPartyPartyPostalAddressCountry
                            {
                                IdentificationCode = "RS"
                            }
                        }
                    },
                };
                return invoice;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}