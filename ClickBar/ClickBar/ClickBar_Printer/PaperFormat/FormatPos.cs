using ClickBar_Common.Enums;
using ClickBar_Common.Models.Invoice;
using ClickBar_Common.Models.Invoice.FileSystemWatcher;
using ClickBar_Common.Models.Order;
using ClickBar_Common.Models.Statistic;
using ClickBar_Logging;
using ClickBar_Printer.Enums;
using ClickBar_Report;
using ClickBar_Report.Models;
using ClickBar_Report.Models.Kuhinja;
using ClickBar_Settings;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_Printer.PaperFormat
{
    public class FormatPos
    {
        private static decimal _totalAmount;
        private static string _journal;
        private static string _reportString;
        private static string _inventoryStatus;
        private static string _kuhinjaReport;
        private static string _verificationQRCode;
        private static int _width;
        private static float _fontSizeInMM;
        private static readonly int _sizeQRmm = 45;
        //private static readonly float _fontSize80mm = 3.90f;
        //private static readonly float _fontSize58mm = 2.90f;
        private static readonly float _fontSize80mm = 3.06f;
        private static readonly float _fontSize58mm = 2.16f;

        public static void PrintKuhinjaReport(PrinterFormatEnumeration printerFormat, 
            KuhinjaReport kuhinjaReport)
        {
            _kuhinjaReport = "====================================\r\n";
            _kuhinjaReport += CenterString(kuhinjaReport.Name, 36);

            _kuhinjaReport += "   \r\n";
            _kuhinjaReport += "   \r\n";
            _kuhinjaReport += "   \r\n";
            _kuhinjaReport += "Artikli:\r\n";
            _kuhinjaReport += "------------------------------------\r\n";
            _kuhinjaReport += $"{"Naziv".PadRight(15)}{"Količina".PadLeft(9)}{"Ukupno".PadLeft(12)}\r\n";
            kuhinjaReport.Items.ForEach(item =>
            {
                _kuhinjaReport += SplitInParts($"{item.Id} - {item.Name}", "", 36, 1);
                string quantity = $"{string.Format("{0:#,##0.000}", item.Quantity).Replace(',', '#').Replace('.', ',').Replace('#', '.')}".PadLeft(9);
                string total = $"{string.Format("{0:#,##0.00}", item.Total).Replace(',', '#').Replace('.', ',').Replace('#', '.')}".PadLeft(12);

                _kuhinjaReport += $"{string.Empty.PadRight(15)}{quantity}{total}\r\n";

                _kuhinjaReport += "------------------------------------\r\n";
            });

            string total = $"{string.Format("{0:#,##0.00}", kuhinjaReport.Total).Replace(',', '#').Replace('.', ',').Replace('#', '.')}".PadLeft(28);
            _kuhinjaReport += $"UKUPNO: {total}\r\n";

            string? prName = SettingsManager.Instance.GetPrinterName();

            if (!string.IsNullOrEmpty(prName))
            {
                var pdoc = new PrintDocument();
                PrinterSettings ps = new PrinterSettings();
                pdoc.PrinterSettings.PrinterName = prName;

                int width = Convert.ToInt32(pdoc.PrinterSettings.DefaultPageSettings.PaperSize.Width / 100 * 25.4);
                switch (printerFormat)
                {
                    case PrinterFormatEnumeration.Pos58mm:
                        _fontSizeInMM = _fontSize58mm;
                        if (width > 52)
                        {
                            width = 52;
                        }
                        break;
                    case PrinterFormatEnumeration.Pos80mm:
                        _fontSizeInMM = _fontSize80mm;
                        if (width > 72)
                        {
                            width = 72;
                        }
                        break;
                }

                _width = width;

                pdoc.PrintPage += new PrintPageEventHandler(dailyKuhinja);
                pdoc.Print();
                pdoc.PrintPage -= new PrintPageEventHandler(dailyKuhinja);
            }
        }
        public static void PrintKuhinjaKonobariReport(PrinterFormatEnumeration printerFormat,
            List<KuhinjaReport> konobari, string name)
        {
            _kuhinjaReport = "====================================\r\n";
            _kuhinjaReport += CenterString(name, 36);

            _kuhinjaReport += "   \r\n";
            _kuhinjaReport += "   \r\n";
            _kuhinjaReport += "   \r\n";
            _kuhinjaReport += "Konobari:\r\n";
            _kuhinjaReport += "------------------------------------\r\n";
            _kuhinjaReport += $"{"Naziv".PadRight(20)}{"Ukupno".PadLeft(16)}\r\n";

            decimal total = 0;
            konobari.ForEach(konobar =>
            {
                string nameKonobar = konobar.Name.PadRight(20);
                string total = $"{string.Format("{0:#,##0.00}", konobar.Total).Replace(',', '#').Replace('.', ',').Replace('#', '.')}".PadLeft(16);

                _kuhinjaReport += $"{nameKonobar}{total}\r\n";

                _kuhinjaReport += "------------------------------------\r\n";

                total += konobar.Total;
            });

            string totalString = $"{string.Format("{0:#,##0.00}", total).Replace(',', '#').Replace('.', ',').Replace('#', '.')}".PadLeft(28);
            _kuhinjaReport += $"UKUPNO: {totalString}\r\n";

            string? prName = SettingsManager.Instance.GetPrinterName();

            if (!string.IsNullOrEmpty(prName))
            {
                var pdoc = new PrintDocument();
                PrinterSettings ps = new PrinterSettings();
                pdoc.PrinterSettings.PrinterName = prName;

                int width = Convert.ToInt32(pdoc.PrinterSettings.DefaultPageSettings.PaperSize.Width / 100 * 25.4);
                switch (printerFormat)
                {
                    case PrinterFormatEnumeration.Pos58mm:
                        _fontSizeInMM = _fontSize58mm;
                        if (width > 52)
                        {
                            width = 52;
                        }
                        break;
                    case PrinterFormatEnumeration.Pos80mm:
                        _fontSizeInMM = _fontSize80mm;
                        if (width > 72)
                        {
                            width = 72;
                        }
                        break;
                }

                _width = width;

                pdoc.PrintPage += new PrintPageEventHandler(dailyKuhinja);
                pdoc.Print();
                pdoc.PrintPage -= new PrintPageEventHandler(dailyKuhinja);
            }
        }
        public static void PrintInventoryStatus(PrinterFormatEnumeration printerFormat, 
            List<InvertoryGlobal> inventoryStatusAll, 
            string title,
            DateTime dateTime,
            SupplierGlobal? supplierGlobal = null)
        {
            decimal totalInputPriceCal = 0;
            decimal totalSellingPriceCal = 0;

            _inventoryStatus = "====================================\r\n";
            _inventoryStatus += CenterString(title, 36);
            _inventoryStatus += CenterString($"Datum: {dateTime.ToString("dd.MM.yyyy")}", 36);

            if (supplierGlobal != null)
            {

                if (!string.IsNullOrEmpty(supplierGlobal.InvoiceNumber))
                {
                    _inventoryStatus += "====================================\r\n";
                    _inventoryStatus += CenterString(supplierGlobal.InvoiceNumber, 36);
                    _inventoryStatus += "====================================\r\n";
                }
                else
                {
                    _inventoryStatus += "====================================\r\n";
                }

                _inventoryStatus += string.IsNullOrEmpty(supplierGlobal.Name) ? string.Empty : SplitInParts($"{supplierGlobal.Name}", "Naziv dobavljača:", 36);
                _inventoryStatus += string.IsNullOrEmpty(supplierGlobal.Pib) ? string.Empty : SplitInParts($"{supplierGlobal.Pib}", "PIB:", 36);
                _inventoryStatus += string.IsNullOrEmpty(supplierGlobal.Mb) ? string.Empty : SplitInParts($"{supplierGlobal.Mb}", "MB:", 36);
                _inventoryStatus += string.IsNullOrEmpty(supplierGlobal.City) ? string.Empty : SplitInParts($"{supplierGlobal.City}", "Grad:", 36);
                _inventoryStatus += string.IsNullOrEmpty(supplierGlobal.Address) ? string.Empty : SplitInParts($"{supplierGlobal.Address}", "Adresa:", 36);
                _inventoryStatus += string.IsNullOrEmpty(supplierGlobal.Email) ? string.Empty : SplitInParts($"{supplierGlobal.Email}", "E-mail:", 36);

                _inventoryStatus += "====================================\r\n";
            }

            _inventoryStatus += "                                        \r\n";
            inventoryStatusAll.ForEach(inventory =>
            {
                _inventoryStatus += SplitInParts($"{inventory.Id}", "Šifra:", 36);
                _inventoryStatus += SplitInParts($"{inventory.Name}", "Naziv:", 36);
                _inventoryStatus += SplitInParts($"{inventory.Jm}", "JM:", 36);
                _inventoryStatus += SplitInParts($"{string.Format("{0:#,##0.00}", inventory.Quantity).Replace(',', '#').Replace('.', ',').Replace('#', '.')}", "Količina:", 36);
                _inventoryStatus += SplitInParts($"{string.Format("{0:#,##0.00}", inventory.InputUnitPrice).Replace(',', '#').Replace('.', ',').Replace('#', '.')}", "Jed. ulazna cena:", 36);
                _inventoryStatus += SplitInParts($"{string.Format("{0:#,##0.00}", inventory.SellingUnitPrice).Replace(',', '#').Replace('.', ',').Replace('#', '.')}", "Jed. prodajna cena:", 36);
                _inventoryStatus += SplitInParts($"{string.Format("{0:#,##0.00}", inventory.TotalAmout).Replace(',', '#').Replace('.', ',').Replace('#', '.')}", "Prodajna vrednost:", 36);

                _inventoryStatus += "                                        \r\n";

                totalInputPriceCal += Decimal.Round(inventory.InputUnitPrice * inventory.Quantity, 2);
                totalSellingPriceCal += Decimal.Round(inventory.SellingUnitPrice * inventory.Quantity, 2);
            });

            _inventoryStatus += "====================================\r\n";

            _inventoryStatus += SplitInParts($"{string.Format("{0:#,##0.00}", totalInputPriceCal).Replace(',', '#').Replace('.', ',').Replace('#', '.')}", "Ukupan ulaz:", 36);
            _inventoryStatus += SplitInParts($"{string.Format("{0:#,##0.00}", totalSellingPriceCal).Replace(',', '#').Replace('.', ',').Replace('#', '.')}", "Ukupan izlaz:", 36);

            _inventoryStatus += "====================================\r\n";

            string? prName = SettingsManager.Instance.GetPrinterName();

            if (!string.IsNullOrEmpty(prName))
            {
                var pdoc = new PrintDocument();
                PrinterSettings ps = new PrinterSettings();
                pdoc.PrinterSettings.PrinterName = prName;

                int width = Convert.ToInt32(pdoc.PrinterSettings.DefaultPageSettings.PaperSize.Width / 100 * 25.4);
                switch (printerFormat)
                {
                    case PrinterFormatEnumeration.Pos58mm:
                        _fontSizeInMM = _fontSize58mm;
                        if (width > 52)
                        {
                            width = 52;
                        }
                        break;
                    case PrinterFormatEnumeration.Pos80mm:
                        _fontSizeInMM = _fontSize80mm;
                        if (width > 72)
                        {
                            width = 72;
                        }
                        break;
                }

                _width = width;

                pdoc.PrintPage += new PrintPageEventHandler(dailyDepInventory);
                pdoc.Print();
                pdoc.PrintPage -= new PrintPageEventHandler(dailyDepInventory);
            }
        }

        public static void PrintOrder(Order order, PosTypeEnumeration posTypeEnumeration, OrderTypeEnumeration orderTypeEnumeration)
        {
            string? prName = null;

            if(orderTypeEnumeration == OrderTypeEnumeration.Sank)
            {
                var name = SettingsManager.Instance.GetPrinterNameSank1();

                if (!string.IsNullOrEmpty(name))
                {
                    prName = name;
                }
            }
            else
            {
                var name = SettingsManager.Instance.GetPrinterNameKuhinja();

                if (!string.IsNullOrEmpty(name))
                {
                    prName = name;
                }
            }

            if (!string.IsNullOrEmpty(prName))
            {
                _journal = CreateOrder(order);

                var pdoc = new PrintDocument();
                PrinterSettings ps = new PrinterSettings();
                pdoc.PrinterSettings.PrinterName = prName;

                int width = Convert.ToInt32(pdoc.PrinterSettings.DefaultPageSettings.PaperSize.Width / 100 * 25.4);
                switch (posTypeEnumeration)
                {
                    case PosTypeEnumeration.Pos80mm:
                        _fontSizeInMM = _fontSize80mm;

                        if (width > 70)
                        {
                            width = 70;
                        }

                        _width = width;
                        break;
                    case PosTypeEnumeration.Pos58mm:
                        _fontSizeInMM = _fontSize58mm;

                        if (width > 50)
                        {
                            width = 50;
                        }

                        _width = width;
                        break;
                }

                pdoc.PrintPage += new PrintPageEventHandler(dailyDep);

                pdoc.Print();

                pdoc.PrintPage -= new PrintPageEventHandler(dailyDep);
            }
        }

        public static void PrintJournalBlack(InvoceRequestFileSystemWatcher invoiceRequest, PosTypeEnumeration posTypeEnumeration)
        {
            _totalAmount = 0;
            try
            {
                _journal = string.Format("Касир:{0}\r\n", invoiceRequest.Cashier.PadLeft(36 - "Касир:".Length));
                _journal += string.Format("Време:{0}\r\n", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss").PadLeft(36 - "Време:".Length));
                _journal += GetItemsForJournal(invoiceRequest);

                _journal += "------------------------------------\r\n";

                _journal += string.Format("Укупан износ:{0}\r\n", string.Format("{0:#,##0.00}", _totalAmount).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(36 - "Укупан износ:".Length));

                _journal += string.Format("{0}{1}\r\n", "Готовина:".PadRight(26),
                    string.Format("{0:#,##0.00}", _totalAmount).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(10));

                _journal += "====================================\r\n";


                string? prName = SettingsManager.Instance.GetPrinterName();

                if (!string.IsNullOrEmpty(prName))
                {
                    var pdoc = new PrintDocument();
                    PrinterSettings ps = new PrinterSettings();
                    pdoc.PrinterSettings.PrinterName = prName;

                    int width = Convert.ToInt32(pdoc.PrinterSettings.DefaultPageSettings.PaperSize.Width / 100 * 25.4);
                    switch (posTypeEnumeration)
                    {
                        case PosTypeEnumeration.Pos80mm:
                            _fontSizeInMM = _fontSize80mm;

                            if (width > 72)
                            {
                                width = 72;
                            }

                            _width = width;
                            break;
                        case PosTypeEnumeration.Pos58mm:
                            _fontSizeInMM = _fontSize58mm;

                            if (width > 50)
                            {
                                width = 50;
                            }

                            _width = width;
                            break;
                    }

                    pdoc.PrintPage += new PrintPageEventHandler(dailyDep);

                    for(int i = 0; i < SettingsManager.Instance.GetNumberCopy(); i++)
                    {
                        pdoc.Print();
                    }

                    pdoc.PrintPage -= new PrintPageEventHandler(dailyDep);
                }
            }
            catch(Exception ex) 
            {
                Log.Error("GRESKA KOD CRNOG RACUNA: ", ex);
            }
        }

        //public static void PrintJournal(InvoiceResult invoiceResult, InvoiceRequest invoiceRequest, PosTypeEnumeration posTypeEnumeration)
        //{
        //    try
        //    {
        //        invoiceResult.CreateVerificationQRCode();
        //        _journal = JournalHelper.CreateJournal(invoiceRequest, invoiceResult);
        //        _verificationQRCode = invoiceResult.VerificationQRCode;

        //        string? prName = SettingsManager.Instance.GetPrinterName();

        //        if (!string.IsNullOrEmpty(prName))
        //        {
        //            var pdoc = new PrintDocument();
        //            PrinterSettings ps = new PrinterSettings();
        //            pdoc.PrinterSettings.PrinterName = prName;

        //            int width = Convert.ToInt32(pdoc.PrinterSettings.DefaultPageSettings.PaperSize.Width / 100 * 25.4);
        //            switch (posTypeEnumeration)
        //            {
        //                case PosTypeEnumeration.Pos80mm:
        //                    _fontSizeInMM = _fontSize80mm;

        //                    if (width > 72)
        //                    {
        //                        width = 72;
        //                    }

        //                    _width = width;
        //                    break;
        //                case PosTypeEnumeration.Pos58mm:
        //                    _fontSizeInMM = _fontSize58mm;

        //                    if (width > 50)
        //                    {
        //                        width = 50;
        //                    }

        //                    _width = width;
        //                    break;
        //            }

        //            pdoc.PrintPage += new PrintPageEventHandler(dailyDep);
        //            pdoc.Print();
        //            pdoc.PrintPage -= new PrintPageEventHandler(dailyDep);
        //        }
        //    }
        //    catch { }
        //}
        public static void PrintReport(string report, PosTypeEnumeration posTypeEnumeration)
        {
            try
            {
                _reportString = report;

                string? prName = SettingsManager.Instance.GetPrinterName();

                if (!string.IsNullOrEmpty(prName))
                {
                    var pdoc = new PrintDocument();
                    PrinterSettings ps = new PrinterSettings();
                    pdoc.PrinterSettings.PrinterName = prName;

                    int width = Convert.ToInt32(pdoc.PrinterSettings.DefaultPageSettings.PaperSize.Width / 100 * 25.4);
                    switch (posTypeEnumeration)
                    {
                        case PosTypeEnumeration.Pos80mm:
                            _fontSizeInMM = _fontSize80mm;

                            if (width > 70)
                            {
                                width = 70;
                            }

                            _width = width;
                            break;
                        case PosTypeEnumeration.Pos58mm:
                            _fontSizeInMM = _fontSize58mm;

                            if (width > 50)
                            {
                                width = 50;
                            }

                            _width = width;
                            break;
                    }

                    pdoc.PrintPage += new PrintPageEventHandler(dailyDepReport);
                    pdoc.Print();
                    pdoc.PrintPage -= new PrintPageEventHandler(dailyDepReport);
                }
            }
            catch { }
        }
        public static string CreateReportBlack(Report report)
        {
            string reportText = "====================================\r\n";

            if (report.StartReport.Day == report.EndReport.Day)
            {
                if (report.StartReport.Month == report.EndReport.Month)
                {
                    DateTime dateTime = new DateTime(report.EndReport.Year, report.EndReport.Month, report.EndReport.Day, 23, 59, 59);

                    if (report.EndReport < dateTime)
                    {
                        reportText += CenterString("Presek stanja", 36);
                    }
                    else
                    {
                        reportText += CenterString("Dnevni izveštaj", 36);
                    }
                }
                else
                {
                    reportText += CenterString("Periodični izveštaj", 36);
                }
            }
            else
            {
                if (report.EndReport.Subtract(report.StartReport) < new TimeSpan(29, 0, 0))
                {
                    reportText += CenterString("Dnevni izveštaj", 36);
                }
                else
                {
                    reportText += CenterString("Periodični izveštaj", 36);
                }
            }


            reportText += SplitInParts($"{report.StartReport.ToString("dd.MM.yyyy. HH:mm")}", "Početak:", 36);
            reportText += SplitInParts($"{report.EndReport.ToString("dd.MM.yyyy. HH:mm")}", "Kraj:", 36);

            reportText += "====================================\r\n";
            reportText += "                            \r\n";
            reportText += "                            \r\n";

            //reportText += "=================TAKSE==================\r\n";
            //reportText += ReportReportTaxes(report.ReportTaxes);
            //reportText += "========================================\r\n";

            //reportText += "============NAČINI PLAĆANJA=============\r\n";
            //reportText += ReportPayments(report.Payments);
            //reportText += "========================================\r\n";

            if (report.ReportItems.Any())
            {
                reportText += "==============ARTIKLI===============\r\n";
                reportText += ReportReportItems(report.ReportItems);
                reportText += "====================================\r\n";
            }

            reportText += "===============KASIRI===============\r\n";
            reportText += ReportCashiers(report.ReportCashiers);
            reportText += "====================================\r\n";

            //if (report.InvoiceTypes.Any())
            //{
            //    reportText += "=========PROMET PO VRSTI RAČUNA=========\r\n";
            //    reportText += ReportInvoiceTypes(report.InvoiceTypes);
            //    reportText += "========================================\r\n";
            //}

            //reportText += "=======Gotovina u kasi======\r\n";
            //reportText += SplitInParts("Bruto", "Valuta", 28);
            //reportText += SplitInParts($"{string.Format("{0:#,##0.00}", report.CashInHand).Replace(',', '#').Replace('.', ',').Replace('#', '.')} din", "RSD", 28);
            //reportText += "========================================\r\n";

            //reportText += SplitInParts($"{report.TotalTraffic.ToString("00.00")} din", "Promet:", 40);
            reportText += "                            \r\n";
            reportText += "====================================\r\n";
            if (report.StartReport.Day == report.EndReport.Day)
            {
                if (report.StartReport.Month == report.EndReport.Month)
                {
                    DateTime dateTime = new DateTime(report.EndReport.Year, report.EndReport.Month, report.EndReport.Day, 23, 59, 59);

                    if (report.EndReport < dateTime)
                    {
                        reportText += CenterString("Kraj presek stanja", 36);
                    }
                    else
                    {
                        reportText += CenterString("Kraj dnevnog izveštaja", 36);
                    }
                }
                else
                {
                    reportText += CenterString("Kraj periodičnog izveštaja", 36);
                }
            }
            else
            {
                if (report.EndReport.Subtract(report.StartReport) < new TimeSpan(29, 0, 0))
                {
                    reportText += CenterString("Kraj dnevnog izveštaja", 36);
                }
                else
                {
                    reportText += CenterString("Kraj periodičnog izveštaja", 36);
                }
            }
            reportText += "====================================\r\n";

            return reportText;
        }
        public static string CreateReport(Report report, bool withTax = true)
        {
            string reportText = "====================================\r\n";
            if (report.StartReport.Day == report.EndReport.Day)
            {
                if (report.StartReport.Month == report.EndReport.Month)
                {
                    DateTime dateTime = new DateTime(report.EndReport.Year, report.EndReport.Month, report.EndReport.Day, 23, 59, 59);

                    if (report.EndReport < dateTime)
                    {
                        reportText += CenterString("Presek stanja", 36);
                    }
                    else
                    {
                        reportText += CenterString("Dnevni izveštaj", 36);
                    }
                }
                else
                {
                    reportText += CenterString("Periodični izveštaj", 36);
                }
            }
            else
            {
                if (report.EndReport.Subtract(report.StartReport) < new TimeSpan(29, 0, 0))
                {
                    reportText += CenterString("Dnevni izveštaj", 36);
                }
                else
                {
                    reportText += CenterString("Periodični izveštaj", 36);
                }
            }


            reportText += SplitInParts($"{report.StartReport.ToString("dd.MM.yyyy. HH:mm")}", "Početak:", 36);
            reportText += SplitInParts($"{report.EndReport.ToString("dd.MM.yyyy. HH:mm")}", "Kraj:", 36);
            reportText += "====================================\r\n";
            reportText += "                            \r\n";
            reportText += "                            \r\n";

            if (withTax)
            {
                reportText += "===============TAKSE================\r\n";
                reportText += ReportReportTaxes(report.ReportTaxes);
                reportText += "====================================\r\n";
            }

#if CRNO
#else
            reportText += "==========NAČINI PLAĆANJA===========\r\n";
            reportText += ReportPayments(report.Payments);
            reportText += "====================================\r\n";
#endif

            reportText += "===============KASIRI===============\r\n";
            reportText += ReportCashiers(report.ReportCashiers);
            reportText += "====================================\r\n";

            if (withTax)
            {
                reportText += "==========REKAPITULACIJA============\r\n";
                reportText += SplitInParts($"{string.Format("{0:#,##0.00}", report.NormalSale).Replace(',', '#').Replace('.', ',').Replace('#', '.')} din", "PROMET PRODAJA:", 36);
                reportText += SplitInParts($"{string.Format("{0:#,##0.00}", report.NormalRefund).Replace(',', '#').Replace('.', ',').Replace('#', '.')} din", "PROMET REFUNDACIJA:", 36);
                reportText += SplitInParts($"{string.Format("{0:#,##0.00}", (report.NormalSale + report.NormalRefund)).Replace(',', '#').Replace('.', ',').Replace('#', '.')} din", "UKUPAN PROMET:", 36);

                reportText += SplitInParts($"{string.Format("{0:#,##0.00}", report.NormalSalePDV).Replace(',', '#').Replace('.', ',').Replace('#', '.')} din", "PDV PROMET PRODAJA:", 36);
                reportText += SplitInParts($"{string.Format("{0:#,##0.00}", report.NormalRefundPDV).Replace(',', '#').Replace('.', ',').Replace('#', '.')} din", "PDV PROMET REFUNDACIJA:", 36);
                reportText += SplitInParts($"{string.Format("{0:#,##0.00}", (report.NormalSalePDV + report.NormalRefundPDV)).Replace(',', '#').Replace('.', ',').Replace('#', '.')} din", "UKUPAN PDV PROMET:", 36);
                reportText += "====================================\r\n";
            }

            if (report.ReportItems.Any())
            {
                reportText += "==============ARTIKLI===============\r\n";
                reportText += ReportReportItems(report.ReportItems);
                reportText += "====================================\r\n";
            }

            //if (report.InvoiceTypes.Any())
            //{
            //    reportText += "=========PROMET PO VRSTI RAČUNA=========\r\n";
            //    reportText += ReportInvoiceTypes(report.InvoiceTypes);
            //    reportText += "========================================\r\n";
            //}
#if CRNO
#else
            reportText += "===========Gotovina u kasi==========\r\n";
            reportText += SplitInParts("Bruto", "Valuta", 36);
            reportText += SplitInParts($"{string.Format("{0:#,##0.00}", report.CashInHand).Replace(',', '#').Replace('.', ',').Replace('#', '.')} din", "RSD", 36);
            //reportText += "========================================\r\n";
#endif
            //reportText += SplitInParts($"{report.TotalTraffic.ToString("00.00")} din", "Promet:", 40);
            reportText += "                            \r\n";
            reportText += "====================================\r\n";
            if (report.StartReport.Day == report.EndReport.Day)
            {
                if (report.StartReport.Month == report.EndReport.Month)
                {
                    DateTime dateTime = new DateTime(report.EndReport.Year, report.EndReport.Month, report.EndReport.Day, 23, 59, 59);

                    if (report.EndReport < dateTime)
                    {
                        reportText += CenterString("Kraj presek stanja", 36);
                    }
                    else
                    {
                        reportText += CenterString("Kraj dnevnog izveštaja", 36);
                    }
                }
                else
                {
                    reportText += CenterString("Kraj periodičnog izveštaja", 36);
                }
            }
            else
            {
                if (report.EndReport.Subtract(report.StartReport) < new TimeSpan(29, 0, 0))
                {
                    reportText += CenterString("Kraj dnevnog izveštaja", 36);
                }
                else
                {
                    reportText += CenterString("Kraj periodičnog izveštaja", 36);
                }
            }
            reportText += "====================================\r\n";

            return reportText;
        }
        private static void dailyKuhinja(object sender, PrintPageEventArgs e)
        {
            Graphics graphics = e.Graphics;
            graphics.PageUnit = GraphicsUnit.Point;
            Font drawFontRegular = new Font("Cascadia Code",
                _fontSizeInMM,
                System.Drawing.FontStyle.Regular, GraphicsUnit.Millimeter);
            SolidBrush drawBrush = new SolidBrush(System.Drawing.Color.Black);
            string[] splitForPrint = _kuhinjaReport.Split("\r\n");
            float x = 0;
            float y = 0;
            float width = 0; // max width I found through trial and error
            float height = 0F;
            int length = splitForPrint.Length;
            if (splitForPrint[length - 1].Length == 0)
            {
                length--;
            }
            for (int i = 0; i < length; i++)
            {
                graphics.DrawString(splitForPrint[i], drawFontRegular, drawBrush, x, y);
                y += graphics.MeasureString(splitForPrint[i], drawFontRegular).Height;
            }
        }
        private static void dailyDepInventory(object sender, PrintPageEventArgs e)
        {
            Graphics graphics = e.Graphics;
            graphics.PageUnit = GraphicsUnit.Point;
            Font drawFontRegular = new Font("Cascadia Code",
                _fontSizeInMM,
                System.Drawing.FontStyle.Bold, GraphicsUnit.Millimeter);
            SolidBrush drawBrush = new SolidBrush(System.Drawing.Color.Black);

            string[] splitForPrint = _inventoryStatus.Split("\r\n");

            float x = 0;
            float y = 0;
            float width = 0; // max width I found through trial and error
            float height = 0F;
            int length = splitForPrint.Length;

            if (splitForPrint[length - 1].Length == 0)
            {
                length--;
            }

            for (int i = 0; i < length; i++)
            {
                graphics.DrawString(splitForPrint[i], drawFontRegular, drawBrush, x, y);
                y += graphics.MeasureString(splitForPrint[i], drawFontRegular).Height;
            }
        }

        private static void dailyDepReport(object sender, PrintPageEventArgs e)
        {
            Graphics graphics = e.Graphics;
            graphics.PageUnit = GraphicsUnit.Point;
            Font drawFontRegular = new Font("Cascadia Code",
                _fontSizeInMM,
                System.Drawing.FontStyle.Regular, GraphicsUnit.Millimeter);
            SolidBrush drawBrush = new SolidBrush(System.Drawing.Color.Black);

            string[] splitForPrint = _reportString.Split("\r\n");

            float x = 0;
            float y = 0;
            float width = 0; // max width I found through trial and error
            float height = 0F;
            int length = splitForPrint.Length;

            if (splitForPrint[length - 1].Length == 0)
            {
                length--;
            }

            bool first = true;

            for (int i = 0; i < length; i++)
            {
                if (first)
                {
                    first = false;

                    width = graphics.MeasureString(splitForPrint[i], drawFontRegular).Width;
                    height = graphics.MeasureString(splitForPrint[i], drawFontRegular).Height;

                    float v = _width / 100f;
                    x = (v * 72 - width) / 2;
                    if (x < 5)
                    {
                        x = 5;
                    }

                    string strBLOBFilePath = SettingsManager.Instance.GetPathToLogo();

                    if (File.Exists(strBLOBFilePath))
                    {
                        FileStream fsBLOBFile = new FileStream(strBLOBFilePath, FileMode.Open, FileAccess.Read);
                        Byte[] bytBLOBData = new Byte[fsBLOBFile.Length];
                        fsBLOBFile.Read(bytBLOBData, 0, bytBLOBData.Length);
                        fsBLOBFile.Close();
                        using (MemoryStream ms = new MemoryStream(bytBLOBData))
                        {
                            var img = System.Drawing.Image.FromStream(ms);

                            var size = width * 0.30F;
                            var xx = x + width * 0.35F;
                            graphics.DrawImage(img, new RectangleF(xx, y + height, size, size));
                            y += 2 * height + size;
                        }
                    }
                }

                graphics.DrawString(splitForPrint[i], drawFontRegular, drawBrush, x, y);
                y += graphics.MeasureString(splitForPrint[i], drawFontRegular).Height;
            }
        }

        private static void dailyDep(object sender, PrintPageEventArgs e)
        {
            try
            {
                Graphics graphics = e.Graphics;
                graphics.PageUnit = GraphicsUnit.Point;
                Font drawFontRegular = new Font("Cascadia Code",
                    _fontSizeInMM,
                    System.Drawing.FontStyle.Regular, GraphicsUnit.Millimeter);
                Font drawFontUpperBold = new Font("Cascadia Code",
                    drawFontRegular.SizeInPoints * 1.5f,
                    System.Drawing.FontStyle.Bold, GraphicsUnit.Point);

                Font dostavaFont = new Font("Cascadia Code",
                    drawFontRegular.SizeInPoints * 4f,
                    System.Drawing.FontStyle.Bold, GraphicsUnit.Point);

                SolidBrush drawBrush = new SolidBrush(System.Drawing.Color.Black);

                string? dostavaSto = _journal.ToLower().Contains("dostava") ? CenterString("98", 10) : null;
                string[] splitForPrint = _journal.Split("\r\n");

                float x = 0;
                float y = 0;
                float width = 0; // max width I found through trial and error
                float height = 0F;
                int length = splitForPrint.Length;

                if (splitForPrint[length - 1].Length == 0)
                {
                    length--;
                }

                if(dostavaSto != null)
                {
                    graphics.DrawString(dostavaSto, dostavaFont, drawBrush, x, y);
                    y += graphics.MeasureString(dostavaSto, dostavaFont).Height;
                }

                for (int i = 0; i < length - 1; i++)
                {
                    if (width == 0)
                    {
                        width = graphics.MeasureString(splitForPrint[i], drawFontRegular).Width;
                        height = graphics.MeasureString(splitForPrint[i], drawFontRegular).Height;

                        float v = _width / 100f;
                        x = (v * 72 - width) / 2;
                        if (x < 5)
                        {
                            x = 5;
                        }

                        string strBLOBFilePath = SettingsManager.Instance.GetPathToLogo();

                        if (File.Exists(strBLOBFilePath))
                        {
                            FileStream fsBLOBFile = new FileStream(strBLOBFilePath, FileMode.Open, FileAccess.Read);
                            Byte[] bytBLOBData = new Byte[fsBLOBFile.Length];
                            fsBLOBFile.Read(bytBLOBData, 0, bytBLOBData.Length);
                            fsBLOBFile.Close();
                            using (MemoryStream ms = new MemoryStream(bytBLOBData))
                            {
                                var img = System.Drawing.Image.FromStream(ms);

                                var size = width * 0.30F;
                                var xx = x + width * 0.35F;
                                graphics.DrawImage(img, new RectangleF(xx, y + height, size, size));
                                y += 2 * height + size;
                            }
                        }
                    }
                    if (splitForPrint[i].Contains("ОВО НИЈЕ ФИСКАЛНИ РАЧУН") &&
                        !splitForPrint[i].Contains("="))
                    {
                        float xLeft = (width - graphics.MeasureString(splitForPrint[i], drawFontUpperBold).Width) / 2f;

                        graphics.DrawString(splitForPrint[i], drawFontUpperBold, drawBrush, xLeft, y);
                        y += graphics.MeasureString(splitForPrint[i], drawFontUpperBold).Height;
                    }
                    else
                    {
                        graphics.DrawString(splitForPrint[i], drawFontRegular, drawBrush, x, y);
                        y += graphics.MeasureString(splitForPrint[i], drawFontRegular).Height;
                    }
                }

                byte[] byteBuffer = Convert.FromBase64String(_verificationQRCode);
                using (MemoryStream ms = new MemoryStream(byteBuffer))
                {
                    var img = System.Drawing.Image.FromStream(ms);

                    var size = _sizeQRmm * 2.8346456693F;
                    var xx = x + (width - size) / 2F;
                    graphics.DrawImage(img, new RectangleF(xx, y + height, size, size));
                    y += 2 * height + size;
                }
                graphics.DrawString(splitForPrint[length - 1], drawFontRegular, drawBrush, x, y);
            }
            catch (Exception ex)
            {

            }
        }
        private static string ReportReportTaxes(Dictionary<string, ReportTax> reportTaxes)
        {
            string result = string.Empty;

            decimal totalGross = 0;
            decimal totalPdv = 0;
            decimal totalNet = 0;

            foreach (KeyValuePair<string, ReportTax> item in reportTaxes)
            {
                result += SplitInParts($"{item.Key} ({item.Value.Rate}%)", "PDV grupa:", 36);
                result += SplitInParts($"{item.Value.Gross.ToString("00.00")} din", "Bruto:", 36);
                result += SplitInParts($"{item.Value.Pdv.ToString("00.00")} din", "PDV:", 36);
                result += SplitInParts($"{item.Value.Net.ToString("00.00")} din", "Neto:", 36);

                result += "                                        \r\n";

                totalGross += item.Value.Gross;
                totalPdv += item.Value.Pdv;
                totalNet += item.Value.Net;
            }

            result += "-------------- Ukupno --------------\r\n";
            result += SplitInParts($"{totalGross.ToString("00.00")} din", "Bruto:", 36);
            result += SplitInParts($"{totalPdv.ToString("00.00")} din", "PDV:", 36);
            result += SplitInParts($"{totalNet.ToString("00.00")} din", "Neto:", 36);

            return result;
        }
        private static string ReportPayments(List<Payment> payments)
        {
            string result = string.Empty;

            decimal total = 0;

            payments.ForEach(x =>
            {
                string paymentType = string.Empty;

                switch (x.PaymentType)
                {
                    case PaymentTypeEnumeration.Cash:
                        paymentType = "Gotovina";
                        break;
                    case PaymentTypeEnumeration.Card:
                        paymentType = "Platna kartica";
                        break;
                    case PaymentTypeEnumeration.Check:
                        paymentType = "Ček";
                        break;
                    case PaymentTypeEnumeration.Voucher:
                        paymentType = "Vaučer";
                        break;
                    case PaymentTypeEnumeration.Other:
                        paymentType = "Drugo bezgotovinsko plaćanje";
                        break;
                    case PaymentTypeEnumeration.WireTransfer:
                        paymentType = "Prenos na račun";
                        break;
                    case PaymentTypeEnumeration.MobileMoney:
                        paymentType = "Instant plaćanje";
                        break;
                }

                result += SplitInParts($"{paymentType}", "Način plaćanja:", 36);
                result += SplitInParts($"{string.Format("{0:#,##0.00}", x.Amount).Replace(',', '#').Replace('.', ',').Replace('#', '.')} din", "Bruto:", 36);

                result += "                       \r\n";

                total += x.Amount;
            });

            result += "-------------- Ukupno --------------\r\n";
            result += SplitInParts($"{string.Format("{0:#,##0.00}", total).Replace(',', '#').Replace('.', ',').Replace('#', '.')} din", "Bruto:", 36);

            return result;
        }
        private static string ReportReportItems(Dictionary<string, Dictionary<string, ReportItem>> reportItems)
        {
            string result = string.Empty;

            decimal total = 0;

            foreach (KeyValuePair<string, Dictionary<string, ReportItem>> group in reportItems)
            {
                result += CenterString(group.Key, 28);

                foreach (KeyValuePair<string, ReportItem> item in group.Value)
                {
                    result += SplitInParts($"{item.Key}", "Šifra:", 36);
                    result += SplitInParts($"{item.Value.Name}", "Artikal:", 36);
                    result += SplitInParts($"{item.Value.Quantity}", "Količina:", 36);
                    result += SplitInParts($"{item.Value.Gross} din", "Bruto:", 36);

                    result += "                            \r\n";

                    total += item.Value.Gross;
                }
                result += "------------------------------------\r\n";
            }

            result += "-------------- Ukupno --------------\r\n";
            result += SplitInParts($"{string.Format("{0:#,##0.00}", total).Replace(',', '#').Replace('.', ',').Replace('#', '.')} din", "Bruto:", 36);

            return result;
        }
        private static string ReportCashiers(Dictionary<string, decimal> cashiers)
        {
            string result = string.Empty;

            decimal total = 0;

            foreach (KeyValuePair<string, decimal> item in cashiers)
            {
                result += SplitInParts($"{item.Key}", "Kasir:", 36);
                result += SplitInParts($"{string.Format("{0:#,##0.00}", item.Value).Replace(',', '#').Replace('.', ',').Replace('#', '.')} din", "Bruto:", 36);

                result += "                            \r\n";

                total += item.Value;
            }

            result += "-------------- Ukupno --------------\r\n";
            result += SplitInParts($"{string.Format("{0:#,##0.00}", total).Replace(',', '#').Replace('.', ',').Replace('#', '.')} din", "Bruto:", 36);

            return result;
        }

        private static string CreateOrder(Order order)
        {
            string orderPrint = CenterString($"Porudzbina {order.OrderName}", 36);

            if (order.PartHall == "storno")
            {
                orderPrint = CenterString($"STORNO {order.OrderName}", 36);
            }

            orderPrint += "====================================\r\n";
            orderPrint += SplitInParts($"{order.CashierName}", "Konobar:", 36);
            orderPrint += SplitInParts($"{order.OrderTime.ToString("dd.MM.yyyy HH:mm:ss")}", "Vreme:", 36);
            if (order.TableId != 0)
            {
                orderPrint += SplitInParts($"{order.TableId}", "Sto:", 36);

                if (!string.IsNullOrEmpty(order.PartHall))
                {
                    orderPrint += SplitInParts($"{order.PartHall}", "Deo sale:", 36);
                }
            }
            else
            {
                orderPrint += CenterString($"{order.PartHall}", 36);
            }
            orderPrint += "====================================\r\n";
            orderPrint += CenterString("Artikli", 36);
            orderPrint += string.Format("{0}{1}\r\n", "Naziv".PadRight(26), "Kol.".PadLeft(10));
            orderPrint += "------------------------------------\r\n";

            int counter = 1;
            foreach (var item in order.Items)
            {
                string i = string.Format("{0}{1}", item.Name, !string.IsNullOrEmpty(item.Zelja) ? $" - {item.Zelja}" : string.Empty);

                orderPrint += SplitInParts(i, "", 31, 1);
                orderPrint += string.Format("{0}{1}\r\n", string.Empty.PadRight(26),
                    string.Format("{0:#,##0.000}", item.Quantity).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(10));

                if (order.Items.Count != counter)
                {
                    orderPrint += "------------------------------------\r\n";
                    counter++;
                }
            }
            orderPrint += "====================================\r\n";
            orderPrint += CenterString(order.OrderTime.ToString("dd.MM.yyyy HH:mm:ss"), 36);
            orderPrint += "====================================\r\n\r\n";

            return orderPrint;
        }

        private static string GetItemsForJournal(InvoceRequestFileSystemWatcher invoiceRequest)
        {
            string items = "Артикли\r\n";
            items += "====================================\r\n";
            items += string.Format("{0}{1}{2}{3}\r\n", "Назив".PadRight(13), "Цена".PadRight(8), "Кол.".PadRight(5), "Укупно".PadLeft(10));

            foreach (ItemFileSystemWatcher item in invoiceRequest.Items)
            {
                string i = string.Format("{0}", item.Name);

                decimal price = item.TotalAmount / item.Quantity;

                items += SplitInParts(i, "", 36, 1);
                items += string.Format("{0}{1}{2}{3}\r\n", string.Empty.PadRight(13),
                    string.Format("{0:#,##0.00}", price).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadRight(8),
                    item.Quantity.ToString().PadRight(5),
                    string.Format("{0:#,##0.00}", item.TotalAmount).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(10));

                _totalAmount += item.TotalAmount;
            }

            return items;
        }
        //private static string ReportInvoiceTypes(Dictionary<InvoiceTypeEenumeration, List<ReportInvoiceType>> invoiceTypes)
        //{
        //    string result = string.Empty;

        //    decimal total = 0;

        //    foreach (KeyValuePair<InvoiceTypeEenumeration, List<ReportInvoiceType>> item in invoiceTypes)
        //    {
        //        string invoiceType = string.Empty;

        //        switch (item.Key)
        //        {
        //            case InvoiceTypeEenumeration.Normal:
        //                invoiceType = "Promet";
        //                break;
        //            case InvoiceTypeEenumeration.Proforma:
        //                invoiceType = "Predračun";
        //                break;
        //            case InvoiceTypeEenumeration.Copy:
        //                invoiceType = "Kopija";
        //                break;
        //            case InvoiceTypeEenumeration.Training:
        //                invoiceType = "Obuka";
        //                break;
        //            case InvoiceTypeEenumeration.Advance:
        //                invoiceType = "Avans";
        //                break;
        //        }
        //        result += SplitInParts("Bruto", "Kasir", 40);

        //        result += CenterString(invoiceType, 40);
        //        item.Value.ForEach(type =>
        //        {
        //            result += SplitInParts($"{type.Gross.ToString("00.00")} din", $"{type.Cashier}", 40);

        //            total += type.Gross;
        //        });

        //        result += "                                        \r\n";
        //    }

        //    result += "---------------- Ukupno ----------------\r\n";
        //    result += SplitInParts($"{total.ToString("00.00")} din", "Bruto:", 40);

        //    return result;
        //}

        private static string CenterString(string value, int length)
        {
            string journal = string.Empty;
            if (string.IsNullOrEmpty(value))
            {
                value = string.Empty;
            }

            if (value.Length < length)
            {
                int spaces = length - value.Length;
                int padLeft = spaces / 2 + value.Length;

                return $"{value.PadLeft(padLeft).PadRight(length)}\r\n";
            }

            string str = value;
            int journalLength = value.Length;

            int counter = 0;

            while (journalLength > 0)
            {
                int len = 0;
                if (journalLength > length)
                {
                    len = length;
                }
                else
                {
                    len = journalLength;
                }
                string s = str.Substring(counter * length, len);

                int spaces = length - s.Length;
                int padLeft = spaces / 2 + s.Length;

                journal += $"{s.PadLeft(padLeft).PadRight(length)}\r\n";

                journalLength -= s.Length;
                counter++;
            }

            return journal;
        }
        private static string SplitInParts(string value, string fixedPart, int length, int pad = 0)
        {
            string journal = string.Empty;

            if (string.IsNullOrEmpty(value))
            {
                value = string.Empty;
            }

            if (fixedPart.Length + value.Length <= length)
            {
                if (pad == 0)
                {
                    journal = string.Format("{0}{1}\r\n", fixedPart, value.PadLeft(length - fixedPart.Length));
                }
                else
                {
                    journal = string.Format("{0}{1}\r\n", fixedPart, value.PadRight(length));
                }
                return journal;
            }

            string str = fixedPart + value;

            int journalLength = str.Length;

            int counter = 0;

            while (journalLength > 0)
            {
                int len = 0;
                if (journalLength > length)
                {
                    len = length;
                }
                else
                {
                    len = journalLength;
                }
                string s = str.Substring(counter * length, len);

                journal += string.Format("{0}\r\n", s.PadRight(length));

                journalLength -= s.Length;
                counter++;
            }

            return journal;
        }
    }
}
