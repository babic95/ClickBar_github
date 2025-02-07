using ClickBar_Common.Enums;
using ClickBar_Common.Models.Invoice;
using ClickBar_Common.Models.Invoice.FileSystemWatcher;
using ClickBar_Common.Models.Statistic;
using ClickBar_Common.Models.Statistic.Nivelacija;
using ClickBar_Common.Models.Statistic.Norm;
using ClickBar_Database.Models;
using ClickBar_Printer.Documents;
using ClickBar_Printer.Enums;
using ClickBar_Printer.Models.DPU;
using ClickBar_Printer.Models.DrljaKuhinja;
using ClickBar_Printer.PaperFormat;
using ClickBar_Report;
using ClickBar_Report.Models;
using ClickBar_Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static log4net.Appender.RollingFileAppender;

namespace ClickBar_Printer
{
    public sealed class PrinterManager
    {
        #region Fields Singleton
        private static readonly object lockObject = new object();
        private static PrinterManager instance = null;
        #endregion Fields Singleton

        #region Fields
        #endregion Fields

        #region Constructors
        private PrinterManager() { }
        public static PrinterManager Instance
        {
            get
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = new PrinterManager();
                    }
                    return instance;
                }
            }
        }
        #endregion Constructors

        #region Public methods
        public MemoryStream PrintDPU(List<DPU_Print> podaci)
        {
            return DPU_Document.PrintDPU(podaci);
        }
        public void PrintPorudzbina(PorudzbinaPrint porudzbinaPrint)
        {
            var kuhinjaItems = porudzbinaPrint.Items.Where(i => i.Type == OrderTypeEnumeration.Kuhinja);
            var sankItems = porudzbinaPrint.Items.Where(i => i.Type == OrderTypeEnumeration.Sank);

            if (kuhinjaItems != null &&
                kuhinjaItems.Any())
            {
                PorudzbinaPrint porudzbinaKuhinja = new PorudzbinaPrint()
                {
                    Worker = porudzbinaPrint.Worker,
                    Sto = porudzbinaPrint.Sto,
                    PorudzbinaDateTime = porudzbinaPrint.PorudzbinaDateTime,
                    PorudzbinaNumber = porudzbinaPrint.PorudzbinaNumber,
                    Items = kuhinjaItems.ToList()
                };

                PorudzbinaDocument.PrintPorudzbina(porudzbinaKuhinja, OrderTypeEnumeration.Kuhinja);
            }

            if (sankItems != null &&
                sankItems.Any())
            {
                PorudzbinaPrint porudzbinaSank = new PorudzbinaPrint()
                {
                    Worker = porudzbinaPrint.Worker,
                    Sto = porudzbinaPrint.Sto,
                    PorudzbinaDateTime = porudzbinaPrint.PorudzbinaDateTime,
                    PorudzbinaNumber = porudzbinaPrint.PorudzbinaNumber,
                    Items = sankItems.ToList()
                };

                PorudzbinaDocument.PrintPorudzbina(porudzbinaSank, OrderTypeEnumeration.Sank);
            }

        }
        public void PrintJournal(InvoceRequestFileSystemWatcher invoiceRequest)
        {
            PrinterFormatEnumeration? printerFormatEnumeration = SettingsManager.Instance.GetPrinterFormat();

            if (printerFormatEnumeration != null)
            {
                switch (printerFormatEnumeration.Value)
                {
                    case PrinterFormatEnumeration.Pos58mm:
                        PrintPos58mm(invoiceRequest);
                        break;
                    case PrinterFormatEnumeration.Pos80mm:
                        PrintPos80mm(invoiceRequest);
                        break;
                }
            }
        }
        public void PrintA4InventoryStatus(List<InvertoryGlobal> inventoryStatusAll,
            string title,
            DateTime dateTime,
            SupplierGlobal? supplierGlobal = null)
        {
            FormatA4.PrintA4InventoryStatus(inventoryStatusAll, title, dateTime, supplierGlobal);
        }
        public void PrintInventoryStatus(List<InvertoryGlobal> inventoryStatusAll, 
            string title, 
            DateTime dateTime,
            SupplierGlobal? supplierGlobal = null)
        {
            PrinterFormatEnumeration? printerFormatEnumeration = SettingsManager.Instance.GetPrinterFormat();

            if (printerFormatEnumeration != null)
            {
                switch (printerFormatEnumeration.Value)
                {
                    case PrinterFormatEnumeration.Pos58mm:
                        FormatPos.PrintInventoryStatus(PrinterFormatEnumeration.Pos58mm, inventoryStatusAll, title, dateTime, supplierGlobal);
                        break;
                    case PrinterFormatEnumeration.Pos80mm:
                        FormatPos.PrintInventoryStatus(PrinterFormatEnumeration.Pos80mm, inventoryStatusAll, title, dateTime, supplierGlobal);
                        break;
                }
            }
        }
        public void PrintReport(string report)
        {
            PrinterFormatEnumeration? printerFormatEnumeration = SettingsManager.Instance.GetPrinterFormat();

            if (printerFormatEnumeration != null)
            {
                switch (printerFormatEnumeration.Value)
                {
                    case PrinterFormatEnumeration.Pos58mm:
                        PrintReportPos58mm(report);
                        break;
                    case PrinterFormatEnumeration.Pos80mm:
                        PrintReportPos80mm(report);
                        break;
                }
            }
        }
        public void PrintNorms(Dictionary<string, Dictionary<string, List<NormGlobal>>> norms)
        {
            FormatA4.PrintNorms(norms);
        }
        public void PrintNivelacija(NivelacijaGlobal nivelacija)
        {
            FormatA4.PrintNivelacija(nivelacija);
        }
        public void PrintDnevniPazar(DateTime fromDateTime, DateTime? toDateTime,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItems20PDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItems10PDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItems0PDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsNoPDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsSirovina20PDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsSirovina10PDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsSirovina0PDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsSirovinaNoPDV)
        {
            FormatA4.PrintDnevniPazar(fromDateTime, toDateTime,
                allItems20PDV,
                allItems10PDV,
                allItems0PDV,
                allItemsNoPDV,
                allItemsSirovina20PDV,
                allItemsSirovina10PDV,
                allItemsSirovina0PDV,
                allItemsSirovinaNoPDV);
        }
        public void LagerListaNaDan(DateTime dateTime,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItems20PDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItems10PDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItems0PDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsNoPDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsSirovina20PDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsSirovina10PDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsSirovina0PDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsSirovinaNoPDV)
        {
            FormatA4.LagerListaNaDan(dateTime,
                allItems20PDV,
                allItems10PDV,
                allItems0PDV,
                allItemsNoPDV,
                allItemsSirovina20PDV,
                allItemsSirovina10PDV,
                allItemsSirovina0PDV,
                allItemsSirovinaNoPDV);
        }
        public void PrintIzlaz1010(DateTime fromDateTime, DateTime? toDateTime,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItems20PDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItems10PDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItems0PDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsNoPDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsSirovina20PDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsSirovina10PDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsSirovina0PDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsSirovinaNoPDV)
        {
            FormatA4.PrintIzlaz1010(fromDateTime, toDateTime,
                allItems20PDV,
                allItems10PDV,
                allItems0PDV,
                allItemsNoPDV,
                allItemsSirovina20PDV,
                allItemsSirovina10PDV,
                allItemsSirovina0PDV,
                allItemsSirovinaNoPDV);
        }
        public void PrintSank(DateTime fromDateTime, DateTime? toDateTime,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItems20PDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItems10PDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItems0PDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsNoPDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsSirovina20PDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsSirovina10PDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsSirovina0PDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsSirovinaNoPDV,
            bool enableSirovine)
        {
            FormatA4.PrintSank(fromDateTime, toDateTime,
                allItems20PDV,
                allItems10PDV,
                allItems0PDV,
                allItemsNoPDV,
                allItemsSirovina20PDV,
                allItemsSirovina10PDV,
                allItemsSirovina0PDV,
                allItemsSirovinaNoPDV,
                enableSirovine);
        }
        public void PrintKuhinja(DateTime fromDateTime, DateTime? toDateTime,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItems20PDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItems10PDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItems0PDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsNoPDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsSirovina20PDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsSirovina10PDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsSirovina0PDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsSirovinaNoPDV,
            bool enableSirovine)
        {
            FormatA4.PrintKuhinja(fromDateTime, toDateTime,
                allItems20PDV,
                allItems10PDV,
                allItems0PDV,
                allItemsNoPDV,
                allItemsSirovina20PDV,
                allItemsSirovina10PDV,
                allItemsSirovina0PDV,
                allItemsSirovinaNoPDV,
                enableSirovine);
        }
        public void PrintKEP(DateTime fromDate, DateTime toDate, List<ItemKEP> kep)
        {
            FormatA4.PrintKEP(fromDate, toDate, kep);
        }
        public void Print1010(DateTime fromDate, DateTime toDate, List<ItemKEP> kep)
        {
            FormatA4.Print1010(fromDate, toDate, kep);
        }
        #endregion Public methods

        #region Private methods
        private void PrintPos80mm(InvoceRequestFileSystemWatcher invoiceRequest)
        {
            FormatPos.PrintJournalBlack(invoiceRequest, PosTypeEnumeration.Pos80mm);
        }
        private void PrintPos58mm(InvoceRequestFileSystemWatcher invoiceRequest)
        {
            FormatPos.PrintJournalBlack(invoiceRequest, PosTypeEnumeration.Pos58mm);
        }
        //private void PrintPos58mm(InvoiceResult invoiceResult, InvoiceRequest invoiceRequest)
        //{
        //    FormatPos.PrintJournal(invoiceResult, invoiceRequest, PosTypeEnumeration.Pos58mm);
        //}
        //private void PrintPosA4(InvoiceResult invoiceResult, InvoiceRequest invoiceRequest)
        //{
        //    FormatA4.PrintJournal(invoiceResult, invoiceRequest);
        //}
        private void PrintReportPos80mm(string report)
        {
            FormatPos.PrintReport(report, PosTypeEnumeration.Pos80mm);
        }
        private void PrintReportPos58mm(string report)
        {
            FormatPos.PrintReport(report, PosTypeEnumeration.Pos58mm);
        }
        //private void PrintReportPosA4(Report report)
        //{
        //    FormatA4.PrintReport(report);
        //}
        #endregion Private methods
    }
}
