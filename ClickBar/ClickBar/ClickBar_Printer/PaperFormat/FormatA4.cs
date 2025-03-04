using ClickBar_Printer.Models;
using ClickBar_Settings;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClickBar_DatabaseSQLManager.Models;
using ClickBar_Common.Models.Statistic.Nivelacija;
using ClickBar_Report.Models;
using System.Security.Cryptography.X509Certificates;
using ClickBar_Common.Models.Statistic.Norm;
using ClickBar_Common.Models.Invoice;
using ClickBar_DatabaseSQLManager;
using System.Diagnostics.Metrics;
using System.Reflection.Metadata.Ecma335;
using ClickBar_Common.Models.Statistic;

namespace ClickBar_Printer.PaperFormat
{
    internal class FormatA4
    {
        #region Fields
        private static MorePage? _morePage;

        private static string _start;
        private static string _end;
        private static string _firma;

        private static string _kepFix;
        private static string _kepItemsFix;
        private static string _kepItems;
        private static decimal _kepZaduzenje;
        private static decimal _kepRazduzenje;

        private static string _dnevniPazarFix;
        private static string _dnevniPazarItemsFix;
        private static string _dnevniPazarItemsFixSirovine;
        private static string _dnevniPazarItemsProdaja;
        private static string _dnevniPazarItemsSirovine;
        private static int _dnevniPazarCounter;

        private static decimal _dnevniPazarTotalAmountProdaja;
        private static decimal _dnevniPazarNivelacijaProdaja;
        private static decimal _dnevniPazarTotalAmountSirovine;

        private static string _nivelacijaFix;
        private static string _nivelacijaItemsFix;
        private static string _nivelacijaItems;

        private static string _calculationFix;
        private static string _calculationItemsFix;
        private static string _calculationItems;

        private static decimal _totalQuantity;
        private static decimal _totalOldValue;
        private static decimal _totalNewValue;
        private static decimal _totalNivelacija;
        private static decimal _totalRazlikaPDV;

        private static string _typeFiscal;

        private static string _norms;

        private static bool _enableSirovina;

        private static readonly float _fontSizeInMM = 3.12f;
        private static int _width;
        #endregion Fields

        #region Constructors
        #endregion Constructors

        #region Public methods
        public static void PrintA4InventoryStatus(SqlServerDbContext sqliteDbContext,
            List<InvertoryGlobal> inventoryStatusAll,
            string title,
            DateTime dateTime,
            SupplierGlobal? supplierGlobal = null)
        {
            try
            {
                var firma = sqliteDbContext.Firmas.FirstOrDefault();

                _firma = string.Empty;
                if (firma != null)
                {
                    _firma += " \r\n";
                    _firma += string.IsNullOrEmpty(firma.Name) ? "" : $"{"Naziv firme:".PadRight(27)}{firma.Name}\r\n";
                    _firma += string.IsNullOrEmpty(firma.Pib) ? "" : $"{"PIB:".PadRight(27)}{firma.Pib}\r\n";
                    _firma += string.IsNullOrEmpty(firma.MB) ? "" : $"{"MB:".PadRight(27)}{firma.MB}\r\n";
                    _firma += string.IsNullOrEmpty(firma.NamePP) ? "" : $"{"Naziv poslovnog prostora:".PadRight(27)}{firma.NamePP}\r\n";
                    _firma += string.IsNullOrEmpty(firma.AddressPP) ? "" : $"{"Adresa poslovnog prostora:".PadRight(27)}{firma.AddressPP}\r\n";
                    _firma += string.IsNullOrEmpty(firma.Number) ? "" : $"{"Broj telefona:".PadRight(27)}{firma.Number}\r\n";
                    _firma += string.IsNullOrEmpty(firma.Email) ? "" : $"{"Email:".PadRight(27)}{firma.Email}\r\n";
                    _firma += " \r\n";
                    _firma += " \r\n";
                }

                _start = $"Stanje magacina na dan: {dateTime.ToString("dd.MM.yyyy HH:mm")}";

                _dnevniPazarFix = "-----------------------------------------------------------------------------------------------------\r\n";

                _dnevniPazarFix += "                          \r\n";
                _dnevniPazarFix += "                          \r\n";
                _dnevniPazarFix += "                          \r\n";
                _dnevniPazarFix += $"Artikli - {title}:\r\n";

                _dnevniPazarItemsFix = "Br.-"; //CenterString("Br.", 4, false) + "-";
                _dnevniPazarItemsFix += $"{CenterString("Artikal", 36, false)}-".PadRight(50);
                _dnevniPazarItemsFix += $"{CenterString("Prosečna", 16)}".PadRight(28);
                _dnevniPazarItemsFix += $"{CenterString("ulazna cena", 16, false)}-".PadLeft(20);

                _dnevniPazarItemsFix += $"{CenterString("Količina", 20, false)}-".PadRight(25);
                _dnevniPazarItemsFix += $"JM-";

                _dnevniPazarItemsFix += $"{CenterString("Prodajna", 16)}";
                _dnevniPazarItemsFix += $"{CenterString("cena", 16, false)}-".PadLeft(10);

                _dnevniPazarItemsFix += $"{CenterString("Ukupna", 22)}";
                _dnevniPazarItemsFix += $"{CenterString("vrednost", 22, false)}-".PadLeft(20);

                _dnevniPazarCounter = 1;

                _dnevniPazarItemsProdaja = GetItemA4InventoryStatus(inventoryStatusAll);

                _end = "";

                string? prName = null;
                foreach (string printer in PrinterSettings.InstalledPrinters)
                {
                    if (printer.ToLower().Contains("pdf"))
                    {
                        prName = printer;
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(prName))
                {
                    var pdoc = new PrintDocument();
                    pdoc.PrinterSettings.PrinterName = prName;
                    _width = pdoc.PrinterSettings.DefaultPageSettings.PaperSize.Width;

                    _morePage = null;
                    //pdoc.PrinterSettings.PrintFileName = nivelacija.NameNivelacije.Replace('-', '_');
                    //pdoc.PrinterSettings.PrintToFile = true;
                    pdoc.PrintPage += new PrintPageEventHandler(printStanjeArtikla);
                    pdoc.Print();
                    pdoc.PrintPage -= new PrintPageEventHandler(printStanjeArtikla);
                    _morePage = null;
                }
            }
            catch (Exception ex)
            {
                ClickBar_Logging.Log.Error("FormatA4 - PrintA4InventoryStatus - Greska prilokom stampe stanja artikala: ", ex);
            }
        }
        public static void Print1010(SqlServerDbContext sqliteDbContext,
            DateTime fromDateTime,
            DateTime toDateTime,
            List<ItemKEP> kep)
        {
            try
            {
                var firma = sqliteDbContext.Firmas.FirstOrDefault();

                _firma = string.Empty;
                if (firma != null)
                {
                    _firma += " \r\n";
                    _firma += string.IsNullOrEmpty(firma.Name) ? "" : $"{"Naziv firme:".PadRight(27)}{firma.Name}\r\n";
                    _firma += string.IsNullOrEmpty(firma.Pib) ? "" : $"{"PIB:".PadRight(27)}{firma.Pib}\r\n";
                    _firma += string.IsNullOrEmpty(firma.MB) ? "" : $"{"MB:".PadRight(27)}{firma.MB}\r\n";
                    _firma += string.IsNullOrEmpty(firma.NamePP) ? "" : $"{"Naziv poslovnog prostora:".PadRight(27)}{firma.NamePP}\r\n";
                    _firma += string.IsNullOrEmpty(firma.AddressPP) ? "" : $"{"Adresa poslovnog prostora:".PadRight(27)}{firma.AddressPP}\r\n";
                    _firma += string.IsNullOrEmpty(firma.Number) ? "" : $"{"Broj telefona:".PadRight(27)}{firma.Number}\r\n";
                    _firma += string.IsNullOrEmpty(firma.Email) ? "" : $"{"Email:".PadRight(27)}{firma.Email}\r\n";
                    _firma += " \r\n";
                    _firma += " \r\n";
                }

                _start = $"KARTICA KONTA";
                _kepFix = "-----------------------------------------------------------------------------------------------------\r\n";
                _kepFix += $"Konto: 1010\r\n";
                _kepFix += $"Period: {fromDateTime.ToString("dd.MM.yyyy")} - {toDateTime.ToString("dd.MM.yyyy")}\r\n";
                _kepFix += $" \r\n";
                _kepFix += $"1010 Sirovine i osnovni materijal";

                _kepZaduzenje = 0;
                _kepRazduzenje = 0;

                _kepItemsFix = "Datum-";// $"{CenterString("Datum", 10, false)}-";
                _kepItemsFix += $"{CenterString("Opis", 42, false)}-".PadLeft(42);
                _kepItemsFix += $"{CenterString("Duguje", 16, false)}-".PadLeft(35);
                _kepItemsFix += $"{CenterString("Potražuje", 16, false)}-".PadLeft(24);
                _kepItemsFix += $"{CenterString("Saldo", 16, false)}-".PadLeft(27);

                _kepItems = GetKepItems(kep);

                _end = $"UKUPNO {string.Format("{0:#,##0.00}", Decimal.Round(_kepZaduzenje, 2)).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(53)}" +
                    $"{string.Format("{0:#,##0.00}", Decimal.Round(_kepRazduzenje, 2)).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(20)}" +
                    $"{string.Format("{0:#,##0.00}", (Decimal.Round(_kepZaduzenje - _kepRazduzenje, 2))).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(20)}";

                string? prName = null;
                foreach (string printer in PrinterSettings.InstalledPrinters)
                {
                    if (printer.ToLower().Contains("pdf"))
                    {
                        prName = printer;
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(prName))
                {
                    var pdoc = new PrintDocument();
                    pdoc.PrinterSettings.PrinterName = prName;
                    _width = pdoc.PrinterSettings.DefaultPageSettings.PaperSize.Width;

                    _morePage = null;
                    //pdoc.PrinterSettings.PrintFileName = nivelacija.NameNivelacije.Replace('-', '_');
                    //pdoc.PrinterSettings.PrintToFile = true;
                    pdoc.PrintPage += new PrintPageEventHandler(print1010);
                    pdoc.Print();
                    pdoc.PrintPage -= new PrintPageEventHandler(print1010);
                    _morePage = null;
                }
            }
            catch (Exception ex)
            {
                ClickBar_Logging.Log.Error("FormatA4 - Print1010 - Greska prilokom stampe salda 1010: ", ex);
            }
        }
        public static void PrintKEP(SqlServerDbContext sqliteDbContext,
            DateTime fromDateTime, 
            DateTime toDateTime, 
            List<ItemKEP> kep)
        {
            try
            {
                var firma = sqliteDbContext.Firmas.FirstOrDefault();

                _firma = string.Empty;
                if (firma != null)
                {
                    _firma += " \r\n";
                    _firma += string.IsNullOrEmpty(firma.Name) ? "" : $"{"Naziv firme:".PadRight(27)}{firma.Name}\r\n";
                    _firma += string.IsNullOrEmpty(firma.Pib) ? "" : $"{"PIB:".PadRight(27)}{firma.Pib}\r\n";
                    _firma += string.IsNullOrEmpty(firma.MB) ? "" : $"{"MB:".PadRight(27)}{firma.MB}\r\n";
                    _firma += string.IsNullOrEmpty(firma.NamePP) ? "" : $"{"Naziv poslovnog prostora:".PadRight(27)}{firma.NamePP}\r\n";
                    _firma += string.IsNullOrEmpty(firma.AddressPP) ? "" : $"{"Adresa poslovnog prostora:".PadRight(27)}{firma.AddressPP}\r\n";
                    _firma += string.IsNullOrEmpty(firma.Number) ? "" : $"{"Broj telefona:".PadRight(27)}{firma.Number}\r\n";
                    _firma += string.IsNullOrEmpty(firma.Email) ? "" : $"{"Email:".PadRight(27)}{firma.Email}\r\n";
                    _firma += " \r\n";
                    _firma += " \r\n";
                }

                _start = $"KEP KNJIGA";
                _kepFix = "-----------------------------------------------------------------------------------------------------\r\n";
                _kepFix += $"Kep knjiga za period: {fromDateTime.ToString("dd.MM.yyyy")} - {toDateTime.ToString("dd.MM.yyyy")}\r\n".PadLeft(103);

                _kepZaduzenje = 0;
                _kepRazduzenje = 0;

                _kepItemsFix = "Datum-";// $"{CenterString("Datum", 10, false)}-";
                _kepItemsFix += $"{CenterString("Opis", 42, false)}-".PadLeft(42);
                _kepItemsFix += $"{CenterString("Zaduženje", 16, false)}-".PadLeft(35);
                _kepItemsFix += $"{CenterString("Razduženje", 16, false)}-".PadLeft(24);
                _kepItemsFix += $"{CenterString("Saldo", 16, false)}-".PadLeft(27);

                _kepItems = GetKepItems(kep);

                _end = $"UKUPNO {string.Format("{0:#,##0.00}", _kepZaduzenje).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(53)}" +
                    $"{string.Format("{0:#,##0.00}", _kepRazduzenje).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(20)}" +
                    $"{string.Format("{0:#,##0.00}", (_kepZaduzenje - _kepRazduzenje)).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(20)}";

                string? prName = null;
                foreach (string printer in PrinterSettings.InstalledPrinters)
                {
                    if (printer.ToLower().Contains("pdf"))
                    {
                        prName = printer;
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(prName))
                {
                    var pdoc = new PrintDocument();
                    pdoc.PrinterSettings.PrinterName = prName;
                    _width = pdoc.PrinterSettings.DefaultPageSettings.PaperSize.Width;

                    _morePage = null;
                    //pdoc.PrinterSettings.PrintFileName = nivelacija.NameNivelacije.Replace('-', '_');
                    //pdoc.PrinterSettings.PrintToFile = true;
                    pdoc.PrintPage += new PrintPageEventHandler(printKep);
                    pdoc.Print();
                    pdoc.PrintPage -= new PrintPageEventHandler(printKep);
                    _morePage = null;
                }
            }
            catch (Exception ex)
            {
                ClickBar_Logging.Log.Error("FormatA4 - PrintKEP - Greska prilokom stampe KEP: ", ex);
            }
        }
        public static void PrintDnevniPazar(SqlServerDbContext sqliteDbContext,
            DateTime fromDateTime, 
            DateTime? toDateTime,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItems20PDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItems10PDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItems0PDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsNoPDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsSirovina20PDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsSirovina10PDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsSirovina0PDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsSirovinaNoPDV)
        {
            try
            {
                _enableSirovina = true;
                var firma = sqliteDbContext.Firmas.FirstOrDefault();

                _firma = string.Empty;
                if (firma != null)
                {
                    _firma += " \r\n";
                    _firma += string.IsNullOrEmpty(firma.Name) ? "" : $"{"Naziv firme:".PadRight(27)}{firma.Name}\r\n";
                    _firma += string.IsNullOrEmpty(firma.Pib) ? "" : $"{"PIB:".PadRight(27)}{firma.Pib}\r\n";
                    _firma += string.IsNullOrEmpty(firma.MB) ? "" : $"{"MB:".PadRight(27)}{firma.MB}\r\n";
                    _firma += string.IsNullOrEmpty(firma.NamePP) ? "" : $"{"Naziv poslovnog prostora:".PadRight(27)}{firma.NamePP}\r\n";
                    _firma += string.IsNullOrEmpty(firma.AddressPP) ? "" : $"{"Adresa poslovnog prostora:".PadRight(27)}{firma.AddressPP}\r\n";
                    _firma += string.IsNullOrEmpty(firma.Number) ? "" : $"{"Broj telefona:".PadRight(27)}{firma.Number}\r\n";
                    _firma += string.IsNullOrEmpty(firma.Email) ? "" : $"{"Email:".PadRight(27)}{firma.Email}\r\n";
                    _firma += " \r\n";
                    _firma += " \r\n";
                }
                _dnevniPazarCounter = 1;
                _dnevniPazarTotalAmountProdaja = 0;
                _dnevniPazarNivelacijaProdaja = 0;
                _dnevniPazarTotalAmountSirovine = 0;

                _start = $"Izveštaj po artiklima";

                _dnevniPazarFix = "-----------------------------------------------------------------------------------------------------\r\n";

                if (toDateTime == null)
                {
                    _dnevniPazarFix += $"Izveštaj po artiklima za: {fromDateTime.ToString("dd.MM.yyyy")}\r\n".PadLeft(103);
                }
                else
                {
                    _dnevniPazarFix += $"Izveštaj po artiklima za period: {fromDateTime.ToString("dd.MM.yyyy")} - {toDateTime.Value.ToString("dd.MM.yyyy")}\r\n".PadLeft(90);
                }

                _dnevniPazarFix += "                          \r\n";
                _dnevniPazarFix += "                          \r\n";
                _dnevniPazarFix += "                          \r\n";
                _dnevniPazarFix += "                          \r\n";
                _dnevniPazarFix += "                          \r\n";
                _dnevniPazarFix += "                          \r\n";
                _dnevniPazarFix += $"Artikli - Prodaja:\r\n";

                _dnevniPazarItemsFix = "Br.-"; //CenterString("Br.", 4, false) + "-";
                _dnevniPazarItemsFix += $"{CenterString("Artikal", 26, false)}-".PadRight(39);
                _dnevniPazarItemsFix += $"JM-".PadRight(9);
                _dnevniPazarItemsFix += $"{CenterString("MPC", 8)}".PadRight(10);
                _dnevniPazarItemsFix += $"{CenterString("artikla", 8, false)}-".PadLeft(15);

                _dnevniPazarItemsFix += $"{CenterString("Količina", 13, false)}-".PadRight(16);

                _dnevniPazarItemsFix += $"{CenterString("Prosečna", 10)}".PadRight(10);
                _dnevniPazarItemsFix += $"{CenterString("MPC", 10, false)}-".PadLeft(13);

                _dnevniPazarItemsFix += $"{CenterString("Iznos", 16)}".PadRight(21);
                _dnevniPazarItemsFix += $"{CenterString("pazara", 10, false)}-".PadLeft(10);

                _dnevniPazarItemsFix += $"{CenterString("Vrednost", 16)}".PadRight(16);
                _dnevniPazarItemsFix += $"{CenterString("niv.", 16, false)}-";

                _dnevniPazarItemsFix += $"{CenterString("Niv.", 8)}";
                _dnevniPazarItemsFix += $"{CenterString("u %", 8, false)}-";

                _dnevniPazarItemsProdaja = GetDnevniPazarItems(allItems20PDV,
                allItems10PDV,
                allItems0PDV,
                allItemsNoPDV,
                false);

                _dnevniPazarItemsSirovine = GetDnevniPazarItems(allItemsSirovina20PDV,
                allItemsSirovina10PDV,
                allItemsSirovina0PDV,
                allItemsSirovinaNoPDV,
                true);

                _end = $"UKUPNO PRODAJA:" +
                    $"{string.Format("{0:#,##0.00}", _dnevniPazarTotalAmountProdaja).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(75)}" +
                    $"{string.Format("{0:#,##0.00}", _dnevniPazarNivelacijaProdaja).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(12)}\r\n";

                _end += $"UKUPNO SIROVINE:" +
                    $"{string.Format("{0:#,##0.00}", _dnevniPazarTotalAmountSirovine).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(74)}\r\n";


                string? prName = null;
                foreach (string printer in PrinterSettings.InstalledPrinters)
                {
                    if (printer.ToLower().Contains("pdf"))
                    {
                        prName = printer;
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(prName))
                {
                    var pdoc = new PrintDocument();
                    pdoc.PrinterSettings.PrinterName = prName;
                    _width = pdoc.PrinterSettings.DefaultPageSettings.PaperSize.Width;

                    _morePage = null;
                    //pdoc.PrinterSettings.PrintFileName = nivelacija.NameNivelacije.Replace('-', '_');
                    //pdoc.PrinterSettings.PrintToFile = true;
                    pdoc.PrintPage += new PrintPageEventHandler(printDnevniPazar);
                    pdoc.Print();
                    pdoc.PrintPage -= new PrintPageEventHandler(printDnevniPazar);
                    _morePage = null;
                }
            }
            catch (Exception ex)
            {
                ClickBar_Logging.Log.Error("FormatA4 - PrintDnevniPazar - Greska prilokom stampe dnevnog pazara: ", ex);
            }
        }
        public static void LagerListaNaDan(SqlServerDbContext sqliteDbContext,
            DateTime dateTime,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItems20PDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItems10PDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItems0PDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsNoPDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsSirovina20PDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsSirovina10PDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsSirovina0PDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsSirovinaNoPDV)
        {
            try
            {
                _enableSirovina = true;
                var firma = sqliteDbContext.Firmas.FirstOrDefault();

                _firma = string.Empty;
                if (firma != null)
                {
                    _firma += " \r\n";
                    _firma += string.IsNullOrEmpty(firma.Name) ? "" : $"{"Naziv firme:".PadRight(27)}{firma.Name}\r\n";
                    _firma += string.IsNullOrEmpty(firma.Pib) ? "" : $"{"PIB:".PadRight(27)}{firma.Pib}\r\n";
                    _firma += string.IsNullOrEmpty(firma.MB) ? "" : $"{"MB:".PadRight(27)}{firma.MB}\r\n";
                    _firma += string.IsNullOrEmpty(firma.NamePP) ? "" : $"{"Naziv poslovnog prostora:".PadRight(27)}{firma.NamePP}\r\n";
                    _firma += string.IsNullOrEmpty(firma.AddressPP) ? "" : $"{"Adresa poslovnog prostora:".PadRight(27)}{firma.AddressPP}\r\n";
                    _firma += string.IsNullOrEmpty(firma.Number) ? "" : $"{"Broj telefona:".PadRight(27)}{firma.Number}\r\n";
                    _firma += string.IsNullOrEmpty(firma.Email) ? "" : $"{"Email:".PadRight(27)}{firma.Email}\r\n";
                    _firma += " \r\n";
                    _firma += " \r\n";
                }
                _dnevniPazarCounter = 1;
                _dnevniPazarTotalAmountProdaja = 0;
                _dnevniPazarNivelacijaProdaja = 0;
                _dnevniPazarTotalAmountSirovine = 0;

                _start = $"Lager lista na dan {dateTime.ToString("dd.MM.yyyy")}";

                _dnevniPazarFix = "-----------------------------------------------------------------------------------------------------\r\n";

                _dnevniPazarFix += "                          \r\n";
                _dnevniPazarFix += $"Artikli - prodaja:\r\n";

                _dnevniPazarItemsFix = "Br.-"; //CenterString("Br.", 4, false) + "-";
                _dnevniPazarItemsFix += $"{CenterString("Artikal", 46, false)}-".PadRight(65);
                _dnevniPazarItemsFix += $"JM-".PadRight(9);
                _dnevniPazarItemsFix += $"{CenterString("Prosečna", 16)}".PadRight(24);
                _dnevniPazarItemsFix += $"{CenterString("ulazna cena", 16, false)}-".PadLeft(17);

                _dnevniPazarItemsFix += $"{CenterString("Količina", 13, false)}-".PadLeft(18);

                _dnevniPazarItemsFix += $"{CenterString("Ukupan", 24)}";
                _dnevniPazarItemsFix += $"{CenterString("iznos", 24, false)}-".PadLeft(25);

                _dnevniPazarItemsProdaja = GetDnevniPazarItems1010(allItems20PDV,
                allItems10PDV,
                allItems0PDV,
                allItemsNoPDV,
                false);

                _dnevniPazarItemsSirovine = GetDnevniPazarItems1010(allItemsSirovina20PDV,
                allItemsSirovina10PDV,
                allItemsSirovina0PDV,
                allItemsSirovinaNoPDV,
                true);

                _end = $"UKUPNO PRODAJA:" +
                    $"{string.Format("{0:#,##0.00}", _dnevniPazarTotalAmountProdaja).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(97)}\r\n";

                _end += $"UKUPNO SIROVINE:" +
                    $"{string.Format("{0:#,##0.00}", _dnevniPazarTotalAmountSirovine).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(96)}\r\n";


                string? prName = null;
                foreach (string printer in PrinterSettings.InstalledPrinters)
                {
                    if (printer.ToLower().Contains("pdf"))
                    {
                        prName = printer;
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(prName))
                {
                    var pdoc = new PrintDocument();
                    pdoc.PrinterSettings.PrinterName = prName;
                    _width = pdoc.PrinterSettings.DefaultPageSettings.PaperSize.Width;

                    _morePage = null;
                    //pdoc.PrinterSettings.PrintFileName = nivelacija.NameNivelacije.Replace('-', '_');
                    //pdoc.PrinterSettings.PrintToFile = true;
                    pdoc.PrintPage += new PrintPageEventHandler(printDnevniPazar);
                    pdoc.Print();
                    pdoc.PrintPage -= new PrintPageEventHandler(printDnevniPazar);
                    _morePage = null;
                }
            }
            catch (Exception ex)
            {
                ClickBar_Logging.Log.Error("FormatA4 - LagerListaNaDan - Greska prilokom stampe Lager Liste Na Dan: ", ex);
            }
        }
        public static void PrintIzlaz1010(SqlServerDbContext sqliteDbContext,
            DateTime fromDateTime, 
            DateTime? toDateTime,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItems20PDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItems10PDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItems0PDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsNoPDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsSirovina20PDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsSirovina10PDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsSirovina0PDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsSirovinaNoPDV)
        {
            try
            {
                _enableSirovina = true;
                var firma = sqliteDbContext.Firmas.FirstOrDefault();

                _firma = string.Empty;
                if (firma != null)
                {
                    _firma += " \r\n";
                    _firma += string.IsNullOrEmpty(firma.Name) ? "" : $"{"Naziv firme:".PadRight(27)}{firma.Name}\r\n";
                    _firma += string.IsNullOrEmpty(firma.Pib) ? "" : $"{"PIB:".PadRight(27)}{firma.Pib}\r\n";
                    _firma += string.IsNullOrEmpty(firma.MB) ? "" : $"{"MB:".PadRight(27)}{firma.MB}\r\n";
                    _firma += string.IsNullOrEmpty(firma.NamePP) ? "" : $"{"Naziv poslovnog prostora:".PadRight(27)}{firma.NamePP}\r\n";
                    _firma += string.IsNullOrEmpty(firma.AddressPP) ? "" : $"{"Adresa poslovnog prostora:".PadRight(27)}{firma.AddressPP}\r\n";
                    _firma += string.IsNullOrEmpty(firma.Number) ? "" : $"{"Broj telefona:".PadRight(27)}{firma.Number}\r\n";
                    _firma += string.IsNullOrEmpty(firma.Email) ? "" : $"{"Email:".PadRight(27)}{firma.Email}\r\n";
                    _firma += " \r\n";
                    _firma += " \r\n";
                }
                _dnevniPazarCounter = 1;
                _dnevniPazarTotalAmountProdaja = 0;
                _dnevniPazarNivelacijaProdaja = 0;
                _dnevniPazarTotalAmountSirovine = 0;

                _start = $"Izveštaj po artiklima - izlaz 1010";

                _dnevniPazarFix = "-----------------------------------------------------------------------------------------------------\r\n";

                if (toDateTime == null)
                {
                    _dnevniPazarFix += $"Izveštaj po artiklima za: {fromDateTime.ToString("dd.MM.yyyy")}\r\n".PadLeft(103);
                }
                else
                {
                    _dnevniPazarFix += $"Izveštaj po artiklima za period: {fromDateTime.ToString("dd.MM.yyyy")} - {toDateTime.Value.ToString("dd.MM.yyyy")}\r\n".PadLeft(90);
                }

                _dnevniPazarFix += "                          \r\n";
                _dnevniPazarFix += "                          \r\n";
                _dnevniPazarFix += "                          \r\n";
                _dnevniPazarFix += "                          \r\n";
                _dnevniPazarFix += "                          \r\n";
                _dnevniPazarFix += "                          \r\n";
                _dnevniPazarFix += $"Artikli - prodaja:\r\n";

                _dnevniPazarItemsFix = "Br.-"; //CenterString("Br.", 4, false) + "-";
                _dnevniPazarItemsFix += $"{CenterString("Artikal", 46, false)}-".PadRight(65);
                _dnevniPazarItemsFix += $"JM-".PadRight(9);
                _dnevniPazarItemsFix += $"{CenterString("Prosečna", 16)}".PadRight(24);
                _dnevniPazarItemsFix += $"{CenterString("ulazna cena", 16, false)}-".PadLeft(17);

                _dnevniPazarItemsFix += $"{CenterString("Količina", 13, false)}-".PadLeft(18);

                _dnevniPazarItemsFix += $"{CenterString("Ukupan", 24)}";
                _dnevniPazarItemsFix += $"{CenterString("iznos", 24, false)}-".PadLeft(25);

                _dnevniPazarItemsProdaja = GetDnevniPazarItems1010(allItems20PDV,
                allItems10PDV,
                allItems0PDV,
                allItemsNoPDV,
                false);

                _dnevniPazarItemsSirovine = GetDnevniPazarItems1010(allItemsSirovina20PDV,
                allItemsSirovina10PDV,
                allItemsSirovina0PDV,
                allItemsSirovinaNoPDV,
                true);

                _end = $"UKUPNO PRODAJA:" +
                    $"{string.Format("{0:#,##0.00}", _dnevniPazarTotalAmountProdaja).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(97)}\r\n";

                _end += $"UKUPNO SIROVINE:" +
                    $"{string.Format("{0:#,##0.00}", _dnevniPazarTotalAmountSirovine).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(96)}\r\n";


                string? prName = null;
                foreach (string printer in PrinterSettings.InstalledPrinters)
                {
                    if (printer.ToLower().Contains("pdf"))
                    {
                        prName = printer;
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(prName))
                {
                    var pdoc = new PrintDocument();
                    pdoc.PrinterSettings.PrinterName = prName;
                    _width = pdoc.PrinterSettings.DefaultPageSettings.PaperSize.Width;

                    _morePage = null;
                    //pdoc.PrinterSettings.PrintFileName = nivelacija.NameNivelacije.Replace('-', '_');
                    //pdoc.PrinterSettings.PrintToFile = true;
                    pdoc.PrintPage += new PrintPageEventHandler(printDnevniPazar);
                    pdoc.Print();
                    pdoc.PrintPage -= new PrintPageEventHandler(printDnevniPazar);
                    _morePage = null;
                }
            }
            catch (Exception ex)
            {
                ClickBar_Logging.Log.Error("FormatA4 - PrintIzlaz1010 - Greska prilokom stampe dnevnog pazara: ", ex);
            }
        }
        public static void PrintKuhinja(SqlServerDbContext sqliteDbContext,
            DateTime fromDateTime,
            DateTime? toDateTime,
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
            try
            {
                _enableSirovina = enableSirovine;
                var firma = sqliteDbContext.Firmas.FirstOrDefault();

                _firma = string.Empty;
                if (firma != null)
                {
                    _firma += " \r\n";
                    _firma += string.IsNullOrEmpty(firma.Name) ? "" : $"{"Naziv firme:".PadRight(27)}{firma.Name}\r\n";
                    _firma += string.IsNullOrEmpty(firma.Pib) ? "" : $"{"PIB:".PadRight(27)}{firma.Pib}\r\n";
                    _firma += string.IsNullOrEmpty(firma.MB) ? "" : $"{"MB:".PadRight(27)}{firma.MB}\r\n";
                    _firma += string.IsNullOrEmpty(firma.NamePP) ? "" : $"{"Naziv poslovnog prostora:".PadRight(27)}{firma.NamePP}\r\n";
                    _firma += string.IsNullOrEmpty(firma.AddressPP) ? "" : $"{"Adresa poslovnog prostora:".PadRight(27)}{firma.AddressPP}\r\n";
                    _firma += string.IsNullOrEmpty(firma.Number) ? "" : $"{"Broj telefona:".PadRight(27)}{firma.Number}\r\n";
                    _firma += string.IsNullOrEmpty(firma.Email) ? "" : $"{"Email:".PadRight(27)}{firma.Email}\r\n";
                    _firma += " \r\n";
                    _firma += " \r\n";
                }
                _dnevniPazarCounter = 1;
                _dnevniPazarTotalAmountProdaja = 0;
                _dnevniPazarNivelacijaProdaja = 0;
                _dnevniPazarTotalAmountSirovine = 0;

                _start = $"Izveštaj po artiklima - KUHINJA";

                _dnevniPazarFix = "-----------------------------------------------------------------------------------------------------\r\n";

                if (toDateTime == null)
                {
                    _dnevniPazarFix += $"Izveštaj po artiklima za: {fromDateTime.ToString("dd.MM.yyyy")}\r\n".PadLeft(103);
                }
                else
                {
                    _dnevniPazarFix += $"Izveštaj po artiklima za period: {fromDateTime.ToString("dd.MM.yyyy")} - {toDateTime.Value.ToString("dd.MM.yyyy")}\r\n".PadLeft(90);
                }

                _dnevniPazarFix += "                          \r\n";
                _dnevniPazarFix += "                          \r\n";
                _dnevniPazarFix += "                          \r\n";
                _dnevniPazarFix += "                          \r\n";
                _dnevniPazarFix += "                          \r\n";
                _dnevniPazarFix += "                          \r\n";
                _dnevniPazarFix += $"Artikli - prodaja:\r\n";

                _dnevniPazarItemsFix = "Br.-"; //CenterString("Br.", 4, false) + "-";
                _dnevniPazarItemsFix += $"{CenterString("Artikal", 46, false)}-".PadRight(65);
                _dnevniPazarItemsFix += $"JM-".PadRight(9);
                _dnevniPazarItemsFix += $"{CenterString("Prosečna", 16)}".PadRight(24);
                _dnevniPazarItemsFix += $"{CenterString("prod. cena", 16, false)}-".PadLeft(17);

                _dnevniPazarItemsFix += $"{CenterString("Količina", 13, false)}-".PadLeft(18);

                _dnevniPazarItemsFix += $"{CenterString("Ukupan", 24)}";
                _dnevniPazarItemsFix += $"{CenterString("iznos", 24, false)}-".PadLeft(25);

                _dnevniPazarItemsFixSirovine = "Br.-"; //CenterString("Br.", 4, false) + "-";
                _dnevniPazarItemsFixSirovine += $"{CenterString("Artikal", 46, false)}-".PadRight(65);
                _dnevniPazarItemsFixSirovine += $"JM-".PadRight(9);
                _dnevniPazarItemsFixSirovine += $"{CenterString("Prosečna", 16)}".PadRight(24);
                _dnevniPazarItemsFixSirovine += $"{CenterString("ulazna cena", 16, false)}-".PadLeft(17);

                _dnevniPazarItemsFixSirovine += $"{CenterString("Količina", 13, false)}-".PadLeft(18);

                _dnevniPazarItemsFixSirovine += $"{CenterString("Ukupan", 24)}";
                _dnevniPazarItemsFixSirovine += $"{CenterString("iznos", 24, false)}-".PadLeft(25);

                _dnevniPazarItemsProdaja = GetDnevniPazarItems1010(allItems20PDV,
                allItems10PDV,
                allItems0PDV,
                allItemsNoPDV,
                false);

                _dnevniPazarItemsSirovine = GetDnevniPazarItems1010(allItemsSirovina20PDV,
                allItemsSirovina10PDV,
                allItemsSirovina0PDV,
                allItemsSirovinaNoPDV,
                true);

                _end = $"UKUPNO PRODAJA:" +
                    $"{string.Format("{0:#,##0.00}", _dnevniPazarTotalAmountProdaja).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(97)}\r\n";

                _end += $"UKUPNO SIROVINE:" +
                    $"{string.Format("{0:#,##0.00}", _dnevniPazarTotalAmountSirovine).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(96)}\r\n";


                // Kreiramo instancu PrinterSettings
                PrinterSettings settings = new PrinterSettings();

                // Dobijamo naziv podrazumevanog štampača
                string? prName = settings.PrinterName;

                if (string.IsNullOrEmpty(prName))
                {
                    foreach (string printer in PrinterSettings.InstalledPrinters)
                    {
                        if (printer.ToLower().Contains("pdf"))
                        {
                            prName = printer;
                            break;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(prName))
                {
                    var pdoc = new PrintDocument();
                    pdoc.PrinterSettings.PrinterName = prName;
                    _width = pdoc.PrinterSettings.DefaultPageSettings.PaperSize.Width;

                    _morePage = null;
                    //pdoc.PrinterSettings.PrintFileName = nivelacija.NameNivelacije.Replace('-', '_');
                    //pdoc.PrinterSettings.PrintToFile = true;
                    pdoc.PrintPage += new PrintPageEventHandler(printDnevniPazar);
                    pdoc.Print();
                    pdoc.PrintPage -= new PrintPageEventHandler(printDnevniPazar);
                    _morePage = null;
                }
            }
            catch (Exception ex)
            {
                ClickBar_Logging.Log.Error("FormatA4 - PrintSank - Greska prilokom stampe dnevnog pazara: ", ex);
            }
        }
        public static void PrintSank(SqlServerDbContext sqliteDbContext,
            DateTime fromDateTime,
            DateTime? toDateTime,
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
            try
            {
                _enableSirovina = enableSirovine;
                var firma = sqliteDbContext.Firmas.FirstOrDefault();

                _firma = string.Empty;
                if (firma != null)
                {
                    _firma += " \r\n";
                    _firma += string.IsNullOrEmpty(firma.Name) ? "" : $"{"Naziv firme:".PadRight(27)}{firma.Name}\r\n";
                    _firma += string.IsNullOrEmpty(firma.Pib) ? "" : $"{"PIB:".PadRight(27)}{firma.Pib}\r\n";
                    _firma += string.IsNullOrEmpty(firma.MB) ? "" : $"{"MB:".PadRight(27)}{firma.MB}\r\n";
                    _firma += string.IsNullOrEmpty(firma.NamePP) ? "" : $"{"Naziv poslovnog prostora:".PadRight(27)}{firma.NamePP}\r\n";
                    _firma += string.IsNullOrEmpty(firma.AddressPP) ? "" : $"{"Adresa poslovnog prostora:".PadRight(27)}{firma.AddressPP}\r\n";
                    _firma += string.IsNullOrEmpty(firma.Number) ? "" : $"{"Broj telefona:".PadRight(27)}{firma.Number}\r\n";
                    _firma += string.IsNullOrEmpty(firma.Email) ? "" : $"{"Email:".PadRight(27)}{firma.Email}\r\n";
                    _firma += " \r\n";
                    _firma += " \r\n";
                }
                _dnevniPazarCounter = 1;
                _dnevniPazarTotalAmountProdaja = 0;
                _dnevniPazarNivelacijaProdaja = 0;
                _dnevniPazarTotalAmountSirovine = 0;

                _start = $"Izveštaj po artiklima - ŠANK";

                _dnevniPazarFix = "-----------------------------------------------------------------------------------------------------\r\n";

                if (toDateTime == null)
                {
                    _dnevniPazarFix += $"Izveštaj po artiklima za: {fromDateTime.ToString("dd.MM.yyyy")}\r\n".PadLeft(103);
                }
                else
                {
                    _dnevniPazarFix += $"Izveštaj po artiklima za period: {fromDateTime.ToString("dd.MM.yyyy")} - {toDateTime.Value.ToString("dd.MM.yyyy")}\r\n".PadLeft(90);
                }

                _dnevniPazarFix += "                          \r\n";
                _dnevniPazarFix += "                          \r\n";
                _dnevniPazarFix += "                          \r\n";
                _dnevniPazarFix += "                          \r\n";
                _dnevniPazarFix += "                          \r\n";
                _dnevniPazarFix += "                          \r\n";
                _dnevniPazarFix += $"Artikli - prodaja:\r\n";

                _dnevniPazarItemsFix = "Br.-"; //CenterString("Br.", 4, false) + "-";
                _dnevniPazarItemsFix += $"{CenterString("Artikal", 46, false)}-".PadRight(65);
                _dnevniPazarItemsFix += $"JM-".PadRight(9);
                _dnevniPazarItemsFix += $"{CenterString("Prosečna", 16)}".PadRight(24);
                _dnevniPazarItemsFix += $"{CenterString("prod. cena", 16, false)}-".PadLeft(17);

                _dnevniPazarItemsFix += $"{CenterString("Količina", 13, false)}-".PadLeft(18);

                _dnevniPazarItemsFix += $"{CenterString("Ukupan", 24)}";
                _dnevniPazarItemsFix += $"{CenterString("iznos", 24, false)}-".PadLeft(25);

                _dnevniPazarItemsFixSirovine = "Br.-"; //CenterString("Br.", 4, false) + "-";
                _dnevniPazarItemsFixSirovine += $"{CenterString("Artikal", 46, false)}-".PadRight(65);
                _dnevniPazarItemsFixSirovine += $"JM-".PadRight(9);
                _dnevniPazarItemsFixSirovine += $"{CenterString("Prosečna", 16)}".PadRight(24);
                _dnevniPazarItemsFixSirovine += $"{CenterString("ulazna cena", 16, false)}-".PadLeft(17);

                _dnevniPazarItemsFixSirovine += $"{CenterString("Količina", 13, false)}-".PadLeft(18);

                _dnevniPazarItemsFixSirovine += $"{CenterString("Ukupan", 24)}";
                _dnevniPazarItemsFixSirovine += $"{CenterString("iznos", 24, false)}-".PadLeft(25);

                _dnevniPazarItemsProdaja = GetDnevniPazarItems1010(allItems20PDV,
                allItems10PDV,
                allItems0PDV,
                allItemsNoPDV,
                false);

                _dnevniPazarItemsSirovine = GetDnevniPazarItems1010(allItemsSirovina20PDV,
                allItemsSirovina10PDV,
                allItemsSirovina0PDV,
                allItemsSirovinaNoPDV,
                true);

                _end = $"UKUPNO PRODAJA:" +
                    $"{string.Format("{0:#,##0.00}", _dnevniPazarTotalAmountProdaja).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(97)}\r\n";

                _end += $"UKUPNO SIROVINE:" +
                    $"{string.Format("{0:#,##0.00}", _dnevniPazarTotalAmountSirovine).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(96)}\r\n";

                // Kreiramo instancu PrinterSettings
                PrinterSettings settings = new PrinterSettings();

                // Dobijamo naziv podrazumevanog štampača
                string? prName = settings.PrinterName;

                if (string.IsNullOrEmpty(prName))
                {
                    foreach (string printer in PrinterSettings.InstalledPrinters)
                    {
                        if (printer.ToLower().Contains("pdf"))
                        {
                            prName = printer;
                            break;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(prName))
                {
                    var pdoc = new PrintDocument();
                    pdoc.PrinterSettings.PrinterName = prName;
                    _width = pdoc.PrinterSettings.DefaultPageSettings.PaperSize.Width;

                    _morePage = null;
                    //pdoc.PrinterSettings.PrintFileName = nivelacija.NameNivelacije.Replace('-', '_');
                    //pdoc.PrinterSettings.PrintToFile = true;
                    pdoc.PrintPage += new PrintPageEventHandler(printDnevniPazar);
                    pdoc.Print();
                    pdoc.PrintPage -= new PrintPageEventHandler(printDnevniPazar);
                    _morePage = null;
                }
            }
            catch (Exception ex)
            {
                ClickBar_Logging.Log.Error("FormatA4 - PrintSank - Greska prilokom stampe dnevnog pazara: ", ex);
            }
        }
        public static void PrintNorms(SqlServerDbContext sqliteDbContext,
            Dictionary<string, Dictionary<string, List<NormGlobal>>> norms)
        {
            try
            {
                var firma = sqliteDbContext.Firmas.FirstOrDefault();

                _firma = string.Empty;
                if (firma != null)
                {
                    _firma += " \r\n";
                    _firma += string.IsNullOrEmpty(firma.Name) ? "" : $"{"Naziv firme:".PadRight(27)}{firma.Name}\r\n";
                    _firma += string.IsNullOrEmpty(firma.Pib) ? "" : $"{"PIB:".PadRight(27)}{firma.Pib}\r\n";
                    _firma += string.IsNullOrEmpty(firma.MB) ? "" : $"{"MB:".PadRight(27)}{firma.MB}\r\n";
                    _firma += string.IsNullOrEmpty(firma.NamePP) ? "" : $"{"Naziv poslovnog prostora:".PadRight(27)}{firma.NamePP}\r\n";
                    _firma += string.IsNullOrEmpty(firma.AddressPP) ? "" : $"{"Adresa poslovnog prostora:".PadRight(27)}{firma.AddressPP}\r\n";
                    _firma += string.IsNullOrEmpty(firma.Number) ? "" : $"{"Broj telefona:".PadRight(27)}{firma.Number}\r\n";
                    _firma += string.IsNullOrEmpty(firma.Email) ? "" : $"{"Email:".PadRight(27)}{firma.Email}\r\n";
                    _firma += " \r\n";
                    _firma += " \r\n";
                }
                _start = "NORMATIVI:";

                _norms = "\r\n";
                _norms += "\r\n";
                _norms += "\r\n";
                _norms += "\r\n";

                foreach (var supergroup in norms)
                {
                    _norms += "#####################################################################################################\r\n";
                    _norms += $"{supergroup.Key}\r\n";
                    _norms += "\r\n";
                    foreach (var group in supergroup.Value)
                    {
                        _norms += "-----------------------------------------------------------------------------------------------------\r\n";
                        _norms += $"          {group.Key}\r\n";
                        _norms += "\r\n";
                        group.Value.ForEach(norm =>
                        {
                            string normName = $"{norm.Id} - {norm.Name}";
                            _norms += $"                    {normName}\r\n";
                            norm.Items.ForEach(item =>
                            {
                                string itemName = $"{item.Id} - {item.Name}";
                                _norms += $"                              {itemName.PadRight(40)}{item.JM.PadLeft(15)}{item.Quantity.PadLeft(15)}\r\n";
                            });
                        });
                        _norms += "-----------------------------------------------------------------------------------------------------\r\n";
                    }
                    _norms += "#####################################################################################################\r\n";
                }

                string? prName = null;
                foreach (string printer in PrinterSettings.InstalledPrinters)
                {
                    if (printer.ToLower().Contains("pdf"))
                    {
                        prName = printer;
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(prName))
                {
                    var pdoc = new PrintDocument();
                    pdoc.PrinterSettings.PrinterName = prName;
                    _width = pdoc.PrinterSettings.DefaultPageSettings.PaperSize.Width;

                    _morePage = null;
                    //pdoc.PrinterSettings.PrintFileName = nivelacija.NameNivelacije.Replace('-', '_');
                    //pdoc.PrinterSettings.PrintToFile = true;
                    pdoc.PrintPage += new PrintPageEventHandler(printNorms);
                    pdoc.Print();
                    pdoc.PrintPage -= new PrintPageEventHandler(printNorms);
                    _morePage = null;
                }
            }
            catch (Exception ex)
            {
                ClickBar_Logging.Log.Error("FormatA4 - PrintNorms - Greska prilokom stampe svih normi: ", ex);
            }
        }
        public static void PrintNivelacija(SqlServerDbContext sqliteDbContext,
            NivelacijaGlobal nivelacija)
        {
            try
            {
                var firma = sqliteDbContext.Firmas.FirstOrDefault();

                _firma = string.Empty;
                if (firma != null)
                {
                    _firma += " \r\n";
                    _firma += string.IsNullOrEmpty(firma.Name) ? "" : $"{"Naziv firme:".PadRight(27)}{firma.Name}\r\n";
                    _firma += string.IsNullOrEmpty(firma.Pib) ? "" : $"{"PIB:".PadRight(27)}{firma.Pib}\r\n";
                    _firma += string.IsNullOrEmpty(firma.MB) ? "" : $"{"MB:".PadRight(27)}{firma.MB}\r\n";
                    _firma += string.IsNullOrEmpty(firma.NamePP) ? "" : $"{"Naziv poslovnog prostora:".PadRight(27)}{firma.NamePP}\r\n";
                    _firma += string.IsNullOrEmpty(firma.AddressPP) ? "" : $"{"Adresa poslovnog prostora:".PadRight(27)}{firma.AddressPP}\r\n";
                    _firma += string.IsNullOrEmpty(firma.Number) ? "" : $"{"Broj telefona:".PadRight(27)}{firma.Number}\r\n";
                    _firma += string.IsNullOrEmpty(firma.Email) ? "" : $"{"Email:".PadRight(27)}{firma.Email}\r\n";
                    _firma += " \r\n";
                    _firma += " \r\n";
                }
                _start = $"{nivelacija.NameNivelacije}";

                _totalQuantity = 0;
                _totalOldValue = 0;
                _totalNewValue = 0;
                _totalNivelacija = 0;
                _totalRazlikaPDV = 0;

                _nivelacijaFix = "-----------------------------------------------------------------------------------------------------\r\n";
                _nivelacijaFix += $"Datum nivelacije: {nivelacija.NivelacijaDate.ToString("dd.MM.yyyy")}\r\n".PadLeft(103);

                _nivelacijaFix += "                          \r\n";
                _nivelacijaFix += "                          \r\n";
                _nivelacijaFix += "                          \r\n";
                _nivelacijaFix += "                          \r\n";
                _nivelacijaFix += "                          \r\n";
                _nivelacijaFix += "                          \r\n";
                _nivelacijaFix += $"Artikli u nivelaciji:\r\n";
                _nivelacijaItemsFix = CenterString("Artikal", 16, false) + "-";
                _nivelacijaItemsFix += CenterString("JM", 10, false) + "-";
                _nivelacijaItemsFix += CenterString("Količina", 12, false) + "-";
                _nivelacijaItemsFix += CenterString("Stara", 12);
                _nivelacijaItemsFix += CenterString("cena", 12, false) + "-";
                _nivelacijaItemsFix += CenterString("Nova", 12);
                _nivelacijaItemsFix += CenterString("cena", 12, false) + "-";
                _nivelacijaItemsFix += CenterString("Prethodna", 12);
                _nivelacijaItemsFix += CenterString("vrednost", 12, false) + "-";
                _nivelacijaItemsFix += CenterString("Nova", 12);
                _nivelacijaItemsFix += CenterString("vrednost", 12, false) + "-";
                _nivelacijaItemsFix += CenterString("Vrednost", 12);
                _nivelacijaItemsFix += CenterString("niv.", 12, false) + "-";
                _nivelacijaItemsFix += CenterString("Stopa", 8);
                _nivelacijaItemsFix += CenterString("PDV", 8, false) + "-";
                _nivelacijaItemsFix += CenterString("Razlika", 12);
                _nivelacijaItemsFix += CenterString("PDV", 12, false) + "-";
                _nivelacijaItemsFix += CenterString("Niv.", 9);
                _nivelacijaItemsFix += CenterString("u %", 9, false) + "-";

                _nivelacijaItems = GetNivelacijaItems(nivelacija);
                _end = $"UKUPNO:" +
                    $"{string.Format("{0:#,##0.00}", _totalQuantity).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(30)}" +
                    $"{string.Format("{0:#,##0.00}", _totalOldValue).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(37)}" +
                    $"{string.Format("{0:#,##0.00}", _totalNewValue).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(14)}" +
                    $"{string.Format("{0:#,##0.00}", _totalNivelacija).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(14)}" +
                    $"{string.Format("{0:#,##0.00}", _totalRazlikaPDV).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(21)}";

                _end += "                          \r\n";
                _end += "                          \r\n";
                _end += "                          \r\n";
                _end += "                          \r\n";
                _end += "                          \r\n";
                _end += "                          \r\n";
                _end += "                          \r\n";
                _end += "                          \r\n";
                _end += "                          \r\n";
                _end += "___________________________________\r\n".PadLeft(135);
                _end += "                             Potpis\r\n".PadLeft(135);

                string? prName = null;
                foreach (string printer in PrinterSettings.InstalledPrinters)
                {
                    if (printer.ToLower().Contains("pdf"))
                    {
                        prName = printer;
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(prName))
                {
                    var pdoc = new PrintDocument();
                    pdoc.DocumentName = nivelacija.NameNivelacije.Replace('-', '_');
                    pdoc.PrinterSettings.PrinterName = prName;
                    _width = pdoc.PrinterSettings.DefaultPageSettings.PaperSize.Width;

                    _morePage = null;
                    //pdoc.PrinterSettings.PrintFileName = nivelacija.NameNivelacije.Replace('-', '_');
                    //pdoc.PrinterSettings.PrintToFile = true;
                    pdoc.PrintPage += new PrintPageEventHandler(printNivelacija);
                    pdoc.Print();
                    pdoc.PrintPage -= new PrintPageEventHandler(printNivelacija);
                    _morePage = null;
                }
            }
            catch (Exception ex)
            {
                ClickBar_Logging.Log.Error("FormatA4 - PrintNivelacija - Greska prilokom stampe nivelacije: ", ex);
            }
        }
        #endregion Public methods

        #region Private methods
        private static void print1010(object sender, PrintPageEventArgs e)
        {
            try
            {
                const float neededHeight = 773.7007874F;
                string newRow = "                                                                                                    \r\n";
                string line = "----------------------------------------------------------------------------------------------------\r\n";
                Graphics graphics = e.Graphics;
                graphics.PageUnit = GraphicsUnit.Point;
                Font drawFontRegularManji = new Font("Cascadia Code",
                    _fontSizeInMM * 0.753f,
                    FontStyle.Regular, GraphicsUnit.Millimeter);
                Font drawFontRegularManjiBold = new Font("Cascadia Code",
                    _fontSizeInMM * 0.753f,
                    FontStyle.Bold, GraphicsUnit.Millimeter);
                Font drawFontRegular = new Font("Cascadia Code",
                    _fontSizeInMM,
                    FontStyle.Regular, GraphicsUnit.Millimeter);
                Font drawFontRegularBold = new Font("Cascadia Code",
                    _fontSizeInMM,
                    FontStyle.Bold, GraphicsUnit.Millimeter);

                Font drawFontBiger1 = new Font("Cascadia Code",
                    _fontSizeInMM * 2.5f,
                    FontStyle.Bold, GraphicsUnit.Millimeter);
                Font drawFontBiger2 = new Font("Cascadia Code",
                    _fontSizeInMM * 2,
                    FontStyle.Regular, GraphicsUnit.Millimeter);
                Font drawFontBiger3 = new Font("Cascadia Code",
                    _fontSizeInMM * 1.5f,
                    FontStyle.Regular, GraphicsUnit.Millimeter);
                Font drawFontBiger4 = new Font("Cascadia Code",
                    _fontSizeInMM * 1.2f,
                    FontStyle.Regular, GraphicsUnit.Millimeter);

                SolidBrush drawBrush = new SolidBrush(System.Drawing.Color.Black);
                SolidBrush drawBrushWhite = new SolidBrush(System.Drawing.Color.White);
                SolidBrush drawBrushGray = new SolidBrush(System.Drawing.Color.Gray);
                string[] fix = _kepFix.Split("\r\n");
                string[] firma = _firma.Split("\r\n");
                string[] saldo1010ItemsFix = _kepItemsFix.Split("-");
                string[] saldo1010Items = _kepItems.Split("\r\n");
                string[] end = _end.Split("\r\n");
                List<string> signature = new List<string>();

                //float xL = 18.346456693F;
                float xL = 30F;
                float xR = 0;
                float yL = 18.346456693F;
                float yR = 28.346456693F;
                float width = 0; // max width I found through trial and error
                float height = 0F;

                width = graphics.MeasureString(line, drawFontRegular).Width;
                height = graphics.MeasureString(line, drawFontRegular).Height;

                xR = width / 2 + xL + xL - 4;
                int counter = 0;
                if (_morePage is null)
                {
                    graphics.DrawString(_start, drawFontBiger1, drawBrush, xL, yL);
                    yL += graphics.MeasureString(_start, drawFontBiger1).Height;

                    foreach (var row in firma)
                    {
                        graphics.DrawString(row, drawFontRegularManji, drawBrush, xL, yL);
                        yL += graphics.MeasureString(row, drawFontRegularManji).Height;
                    }
                    yL += graphics.MeasureString(newRow, drawFontRegular).Height;

                    foreach (var row in fix)
                    {
                        if (fix.Length - 1 == counter)
                        {
                            graphics.DrawString(row, drawFontBiger3, drawBrush, xL, yL);
                            yL += graphics.MeasureString(row, drawFontBiger3).Height;
                        }
                        else
                        {
                            graphics.DrawString(row, drawFontRegular, drawBrush, xL, yL);
                            yL += graphics.MeasureString(row, drawFontRegular).Height;
                        }
                        counter++;
                    }
                    yL += graphics.MeasureString(newRow, drawFontRegular).Height;

                    float x = xL;
                    foreach (var row in saldo1010ItemsFix)
                    {
                        if (row.ToLower().Contains("datum"))
                        {
                            var currentY = graphics.MeasureString(row, drawFontRegularBold).Height;
                            var rect = new RectangleF(xL, yL - currentY, width,
                                graphics.MeasureString(row, drawFontRegularBold).Height + 2 * currentY);
                            graphics.FillRectangle(drawBrushGray, rect);
                        }

                        graphics.DrawString(row, drawFontRegularBold, drawBrush, x, yL);
                        x += graphics.MeasureString(row, drawFontRegularBold).Width;
                    }
                    yL += 2 * graphics.MeasureString(newRow, drawFontRegularBold).Height;
                }

                float y = yR > yL ? yR : yL;

                int ind = 0;

                if (_morePage is not null)
                {
                    ind = _morePage.Index;

                    if (_morePage.Type == Models.Type.Items)
                    {
                        y += graphics.MeasureString(newRow, drawFontRegularManji).Height;
                        for (; ind < saldo1010Items.Length - 1; ind++)
                        {
                            if (y < neededHeight)
                            {
                                graphics.DrawString(saldo1010Items[ind], drawFontRegularManji, drawBrush, xL, y);
                                y += graphics.MeasureString(saldo1010Items[ind], drawFontRegularManji).Height;

                            }
                            else
                            {
                                e.HasMorePages = true;
                                _morePage = new MorePage()
                                {
                                    Type = Models.Type.Items,
                                    Index = ind
                                };
                                return;
                            }
                        }
                        for (int i = 0; i < end.Length; i++)
                        {
                            if (y < neededHeight)
                            {
                                graphics.DrawString(end[i], drawFontRegularBold, drawBrush, xL, y);
                                y += graphics.MeasureString(end[i], drawFontRegularBold).Height;
                            }
                            else
                            {
                                e.HasMorePages = true;
                                _morePage = new MorePage()
                                {
                                    Type = Models.Type.End,
                                    Index = i
                                };
                                return;
                            }
                        }
                    }
                    else if (_morePage.Type == Models.Type.End)
                    {
                        for (; ind < end.Length; ind++)
                        {
                            if (y < neededHeight)
                            {
                                graphics.DrawString(end[ind], drawFontRegularBold, drawBrush, xL, y);
                                y += graphics.MeasureString(end[ind], drawFontRegularBold).Height;
                            }
                            else
                            {
                                e.HasMorePages = true;
                                _morePage = new MorePage()
                                {
                                    Type = Models.Type.End,
                                    Index = ind
                                };
                                return;
                            }
                        }
                    }
                }
                else
                {
                    y += graphics.MeasureString(newRow, drawFontRegularManji).Height;
                    for (int i = 0; i < saldo1010Items.Length - 1; i++)
                    {
                        if (y < neededHeight)
                        {
                            graphics.DrawString(saldo1010Items[i], drawFontRegularManji, drawBrush, xL, y);
                            y += graphics.MeasureString(saldo1010Items[i], drawFontRegularManji).Height;

                        }
                        else
                        {
                            e.HasMorePages = true;
                            _morePage = new MorePage()
                            {
                                Type = Models.Type.Items,
                                Index = i
                            };
                            return;
                        }
                    }
                    for (int i = 0; i < end.Length; i++)
                    {
                        if (y < neededHeight)
                        {
                            graphics.DrawString(end[i], drawFontRegularBold, drawBrush, xL, y);
                            y += graphics.MeasureString(end[i], drawFontRegularBold).Height;
                        }
                        else
                        {
                            e.HasMorePages = true;
                            _morePage = new MorePage()
                            {
                                Type = Models.Type.End,
                                Index = i
                            };
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        private static void printKep(object sender, PrintPageEventArgs e)
        {
            try
            {
                const float neededHeight = 773.7007874F;
                string newRow = "                                                                                                    \r\n";
                string line = "----------------------------------------------------------------------------------------------------\r\n";
                Graphics graphics = e.Graphics;
                graphics.PageUnit = GraphicsUnit.Point;
                Font drawFontRegularManji = new Font("Cascadia Code",
                    _fontSizeInMM * 0.753f,
                    FontStyle.Regular, GraphicsUnit.Millimeter);
                Font drawFontRegularManjiBold = new Font("Cascadia Code",
                    _fontSizeInMM * 0.753f,
                    FontStyle.Bold, GraphicsUnit.Millimeter);
                Font drawFontRegular = new Font("Cascadia Code",
                    _fontSizeInMM,
                    FontStyle.Regular, GraphicsUnit.Millimeter);
                Font drawFontRegularBold = new Font("Cascadia Code",
                    _fontSizeInMM,
                    FontStyle.Bold, GraphicsUnit.Millimeter);

                Font drawFontBiger1 = new Font("Cascadia Code",
                    _fontSizeInMM * 3,
                    FontStyle.Bold, GraphicsUnit.Millimeter);
                Font drawFontBiger2 = new Font("Cascadia Code",
                    _fontSizeInMM * 2,
                    FontStyle.Regular, GraphicsUnit.Millimeter);
                Font drawFontBiger3 = new Font("Cascadia Code",
                    _fontSizeInMM * 1.5f,
                    FontStyle.Regular, GraphicsUnit.Millimeter);
                Font drawFontBiger4 = new Font("Cascadia Code",
                    _fontSizeInMM * 1.2f,
                    FontStyle.Regular, GraphicsUnit.Millimeter);

                SolidBrush drawBrush = new SolidBrush(System.Drawing.Color.Black);
                SolidBrush drawBrushWhite = new SolidBrush(System.Drawing.Color.White);
                SolidBrush drawBrushGray = new SolidBrush(System.Drawing.Color.Gray);
                string[] fix = _kepFix.Split("\r\n");
                string[] firma = _firma.Split("\r\n");
                string[] kepItemsFix = _kepItemsFix.Split("-");
                string[] kepItems = _kepItems.Split("\r\n");
                string[] end = _end.Split("\r\n");
                List<string> signature = new List<string>();

                //float xL = 18.346456693F;
                float xL = 30F;
                float xR = 0;
                float yL = 18.346456693F;
                float yR = 28.346456693F;
                float width = 0; // max width I found through trial and error
                float height = 0F;

                width = graphics.MeasureString(line, drawFontRegular).Width;
                height = graphics.MeasureString(line, drawFontRegular).Height;

                xR = width / 2 + xL + xL - 4;

                if (_morePage is null)
                {
                    graphics.DrawString(_start, drawFontBiger2, drawBrush, xL, yL);
                    yL += graphics.MeasureString(_start, drawFontBiger2).Height;

                    foreach (var row in firma)
                    {
                        graphics.DrawString(row, drawFontRegularManji, drawBrush, xL, yL);
                        yL += graphics.MeasureString(row, drawFontRegularManji).Height;
                    }
                    yL += graphics.MeasureString(newRow, drawFontRegular).Height;

                    foreach (var row in fix)
                    {
                        graphics.DrawString(row, drawFontRegular, drawBrush, xL, yL);
                        yL += graphics.MeasureString(row, drawFontRegular).Height;
                    }
                    yL += graphics.MeasureString(newRow, drawFontRegular).Height;

                    float x = xL;
                    foreach (var row in kepItemsFix)
                    {
                        if (row.ToLower().Contains("datum"))
                        {
                            var currentY = graphics.MeasureString(row, drawFontRegularBold).Height;
                            var rect = new RectangleF(xL, yL - currentY, width,
                                graphics.MeasureString(row, drawFontRegularBold).Height + 2 * currentY);
                            graphics.FillRectangle(drawBrushGray, rect);
                        }

                        graphics.DrawString(row, drawFontRegularBold, drawBrush, x, yL);
                        x += graphics.MeasureString(row, drawFontRegularBold).Width;
                    }
                    yL += 2 * graphics.MeasureString(newRow, drawFontRegularBold).Height;
                }

                float y = yR > yL ? yR : yL;

                int ind = 0;

                if (_morePage is not null)
                {
                    ind = _morePage.Index;

                    if (_morePage.Type == Models.Type.Items)
                    {
                        y += graphics.MeasureString(newRow, drawFontRegularManji).Height;
                        for (; ind < kepItems.Length - 1; ind++)
                        {
                            if (y < neededHeight)
                            {
                                graphics.DrawString(kepItems[ind], drawFontRegularManji, drawBrush, xL, y);
                                y += graphics.MeasureString(kepItems[ind], drawFontRegularManji).Height;

                            }
                            else
                            {
                                e.HasMorePages = true;
                                _morePage = new MorePage()
                                {
                                    Type = Models.Type.Items,
                                    Index = ind
                                };
                                return;
                            }
                        }
                        for (int i = 0; i < end.Length; i++)
                        {
                            if (y < neededHeight)
                            {
                                graphics.DrawString(end[i], drawFontRegularBold, drawBrush, xL, y);
                                y += graphics.MeasureString(end[i], drawFontRegularBold).Height;
                            }
                            else
                            {
                                e.HasMorePages = true;
                                _morePage = new MorePage()
                                {
                                    Type = Models.Type.End,
                                    Index = i
                                };
                                return;
                            }
                        }
                    }
                    else if (_morePage.Type == Models.Type.End)
                    {
                        for (; ind < end.Length; ind++)
                        {
                            if (y < neededHeight)
                            {
                                graphics.DrawString(end[ind], drawFontRegularBold, drawBrush, xL, y);
                                y += graphics.MeasureString(end[ind], drawFontRegularBold).Height;
                            }
                            else
                            {
                                e.HasMorePages = true;
                                _morePage = new MorePage()
                                {
                                    Type = Models.Type.End,
                                    Index = ind
                                };
                                return;
                            }
                        }
                    }
                }
                else
                {
                    y += graphics.MeasureString(newRow, drawFontRegularManji).Height;
                    for (int i = 0; i < kepItems.Length - 1; i++)
                    {
                        if (y < neededHeight)
                        {
                            graphics.DrawString(kepItems[i], drawFontRegularManji, drawBrush, xL, y);
                            y += graphics.MeasureString(kepItems[i], drawFontRegularManji).Height;

                        }
                        else
                        {
                            e.HasMorePages = true;
                            _morePage = new MorePage()
                            {
                                Type = Models.Type.Items,
                                Index = i
                            };
                            return;
                        }
                    }
                    for (int i = 0; i < end.Length; i++)
                    {
                        if (y < neededHeight)
                        {
                            graphics.DrawString(end[i], drawFontRegularBold, drawBrush, xL, y);
                            y += graphics.MeasureString(end[i], drawFontRegularBold).Height;
                        }
                        else
                        {
                            e.HasMorePages = true;
                            _morePage = new MorePage()
                            {
                                Type = Models.Type.End,
                                Index = i
                            };
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private static void printStanjeArtikla(object sender, PrintPageEventArgs e)
        {
            try
            {
                const float neededHeight = 773.7007874F;
                string newRow = "                                                                                                    \r\n";
                string line = "----------------------------------------------------------------------------------------------------\r\n";
                Graphics graphics = e.Graphics;
                graphics.PageUnit = GraphicsUnit.Point;
                Font drawFontRegularManji = new Font("Cascadia Code",
                    _fontSizeInMM * 0.753f,
                    FontStyle.Regular, GraphicsUnit.Millimeter);
                Font drawFontRegularManjiBold = new Font("Cascadia Code",
                    _fontSizeInMM * 0.753f,
                    FontStyle.Bold, GraphicsUnit.Millimeter);
                Font drawFontRegular = new Font("Cascadia Code",
                    _fontSizeInMM,
                    FontStyle.Regular, GraphicsUnit.Millimeter);
                Font drawFontRegularBold = new Font("Cascadia Code",
                    _fontSizeInMM,
                    FontStyle.Bold, GraphicsUnit.Millimeter);
                Font drawFontRegularBold2 = new Font("Cascadia Code",
                    _fontSizeInMM * 0.895f,
                    FontStyle.Bold, GraphicsUnit.Millimeter);

                Font drawFontBiger1 = new Font("Cascadia Code",
                    _fontSizeInMM * 3,
                    FontStyle.Bold, GraphicsUnit.Millimeter);
                Font drawFontBiger2 = new Font("Cascadia Code",
                    _fontSizeInMM * 2,
                    FontStyle.Regular, GraphicsUnit.Millimeter);
                Font drawFontBiger3 = new Font("Cascadia Code",
                    _fontSizeInMM * 1.5f,
                    FontStyle.Regular, GraphicsUnit.Millimeter);
                Font drawFontBiger4 = new Font("Cascadia Code",
                    _fontSizeInMM * 1.2f,
                    FontStyle.Regular, GraphicsUnit.Millimeter);

                SolidBrush drawBrush = new SolidBrush(System.Drawing.Color.Black);
                SolidBrush drawBrushGreska = new SolidBrush(System.Drawing.Color.Red);
                SolidBrush drawBrushWhite = new SolidBrush(System.Drawing.Color.White);
                SolidBrush drawBrushGray = new SolidBrush(System.Drawing.Color.Gray);
                string[] fix = _dnevniPazarFix.Split("\r\n");
                string[] firma = _firma.Split("\r\n");
                string[] pazarItemsFix = _dnevniPazarItemsFix.Split("-");
                string[] pazarItemsProdaja = _dnevniPazarItemsProdaja.Split("\r\n");
                string[] end = _end.Split("\r\n");

                List<string> signature = new List<string>();

                //float xL = 18.346456693F;
                float xL = 30F;
                float xR = 0;
                float yL = 18.346456693F;
                float yR = 28.346456693F;
                float width = 0; // max width I found through trial and error
                float height = 0F;

                width = graphics.MeasureString(line, drawFontRegular).Width;
                height = graphics.MeasureString(line, drawFontRegular).Height;

                xR = width / 2 + xL + xL - 4;

                if (_morePage is null)
                {
                    graphics.DrawString(_start, drawFontBiger2, drawBrush, xL, yL);
                    yL += graphics.MeasureString(_start, drawFontBiger2).Height;

                    foreach (var row in firma)
                    {
                        graphics.DrawString(row, drawFontRegularManji, drawBrush, xL, yL);
                        yL += graphics.MeasureString(row, drawFontRegularManji).Height;
                    }
                    yL += graphics.MeasureString(newRow, drawFontRegular).Height;

                    foreach (var row in fix)
                    {
                        graphics.DrawString(row, drawFontRegular, drawBrush, xL, yL);
                        yL += graphics.MeasureString(row, drawFontRegular).Height;
                    }
                    yL += graphics.MeasureString(newRow, drawFontRegular).Height;

                    float x = xL;
                    foreach (var row in pazarItemsFix)
                    {
                        if (row.ToLower().Contains("br"))
                        {
                            var currentY = graphics.MeasureString(row, drawFontRegularBold).Height;
                            var rect = new RectangleF(xL, yL - currentY, width,
                                graphics.MeasureString(row, drawFontRegularBold).Height + 2 * currentY);
                            graphics.FillRectangle(drawBrushGray, rect);
                        }

                        graphics.DrawString(row, drawFontRegularBold, drawBrush, x, yL);
                        x += graphics.MeasureString(row, drawFontRegularBold).Width;
                    }
                    yL += 2 * graphics.MeasureString(newRow, drawFontRegularBold).Height;
                }

                float y = yR > yL ? yR : yL;

                int ind = 0;

                if (_morePage is not null)
                {
                    ind = _morePage.Index;

                    if (_morePage.Type == Models.Type.Items)
                    {
                        y += graphics.MeasureString(newRow, drawFontRegularManji).Height;
                        for (; ind < pazarItemsProdaja.Length - 1; ind++)
                        {
                            if (y < neededHeight)
                            {
                                if (pazarItemsProdaja[ind].Contains("greska"))
                                {
                                    var splits = pazarItemsProdaja[ind].Split("greska");

                                    graphics.DrawString(splits[0], drawFontRegularManji, drawBrushGreska, xL, y);
                                    y += graphics.MeasureString(splits[0], drawFontRegularManji).Height;
                                }
                                else
                                {
                                    graphics.DrawString(pazarItemsProdaja[ind], drawFontRegularManji, drawBrush, xL, y);
                                    y += graphics.MeasureString(pazarItemsProdaja[ind], drawFontRegularManji).Height;
                                }
                            }
                            else
                            {
                                e.HasMorePages = true;
                                _morePage = new MorePage()
                                {
                                    Type = Models.Type.Items,
                                    Index = ind
                                };
                                return;
                            }
                        }

                        if (y < neededHeight)
                        {
                            graphics.DrawString(end[0], drawFontRegularBold2, drawBrush, xL, y);
                            y += graphics.MeasureString(end[0], drawFontRegularBold2).Height;
                        }
                        else
                        {
                            e.HasMorePages = true;
                            _morePage = new MorePage()
                            {
                                Type = Models.Type.End,
                                Index = 0
                            };
                            return;
                        }

                        if (y < neededHeight)
                        {
                            graphics.DrawString(end[1], drawFontRegularBold2, drawBrush, xL, y);
                            y += graphics.MeasureString(end[1], drawFontRegularBold2).Height;
                        }
                        else
                        {
                            e.HasMorePages = true;
                            _morePage = new MorePage()
                            {
                                Type = Models.Type.End,
                                Index = 1
                            };
                            return;
                        }
                    }
                    else if (_morePage.Type == Models.Type.End)
                    {
                        if (y < neededHeight)
                        {
                            graphics.DrawString(end[ind], drawFontRegularBold2, drawBrush, xL, y);
                            y += graphics.MeasureString(end[ind], drawFontRegularBold2).Height;
                        }
                        else
                        {
                            e.HasMorePages = true;
                            _morePage = new MorePage()
                            {
                                Type = Models.Type.End,
                                Index = ind
                            };
                            return;
                        }

                        if (ind == 0)
                        {
                            if (y < neededHeight)
                            {
                                graphics.DrawString(end[1], drawFontRegularBold2, drawBrush, xL, y);
                                y += graphics.MeasureString(end[1], drawFontRegularBold2).Height;
                            }
                            else
                            {
                                e.HasMorePages = true;
                                _morePage = new MorePage()
                                {
                                    Type = Models.Type.End,
                                    Index = 1
                                };
                                return;
                            }
                        }
                    }
                }
                else
                {
                    y += graphics.MeasureString(newRow, drawFontRegularManji).Height;
                    for (int i = 0; i < pazarItemsProdaja.Length - 1; i++)
                    {
                        if (y < neededHeight)
                        {
                            if (pazarItemsProdaja[i].Contains("greska"))
                            {
                                var splits = pazarItemsProdaja[i].Split("greska");

                                graphics.DrawString(splits[0], drawFontRegularManji, drawBrushGreska, xL, y);
                                y += graphics.MeasureString(splits[0], drawFontRegularManji).Height;
                            }
                            else
                            {
                                graphics.DrawString(pazarItemsProdaja[i], drawFontRegularManji, drawBrush, xL, y);
                                y += graphics.MeasureString(pazarItemsProdaja[i], drawFontRegularManji).Height;
                            }

                        }
                        else
                        {
                            e.HasMorePages = true;
                            _morePage = new MorePage()
                            {
                                Type = Models.Type.Items,
                                Index = i
                            };
                            return;
                        }
                    }
                    if (y < neededHeight)
                    {
                        graphics.DrawString(end[0], drawFontRegularBold2, drawBrush, xL, y);
                        y += graphics.MeasureString(end[0], drawFontRegularBold2).Height;
                    }
                    else
                    {
                        e.HasMorePages = true;
                        _morePage = new MorePage()
                        {
                            Type = Models.Type.End,
                            Index = 0
                        };
                        return;
                    }

                    if (y < neededHeight)
                    {
                        graphics.DrawString(end[1], drawFontRegularBold2, drawBrush, xL, y);
                        y += graphics.MeasureString(end[1], drawFontRegularBold2).Height;
                    }
                    else
                    {
                        e.HasMorePages = true;
                        _morePage = new MorePage()
                        {
                            Type = Models.Type.End,
                            Index = 1
                        };
                        return;
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        private static void printDnevniPazar(object sender, PrintPageEventArgs e)
        {
            try
            {
                const float neededHeight = 773.7007874F;
                string newRow = "                                                                                                    \r\n";
                string line = "----------------------------------------------------------------------------------------------------\r\n";
                Graphics graphics = e.Graphics;
                graphics.PageUnit = GraphicsUnit.Point;
                Font drawFontRegularManji = new Font("Cascadia Code",
                    _fontSizeInMM * 0.753f,
                    FontStyle.Regular, GraphicsUnit.Millimeter);
                Font drawFontRegularManjiBold = new Font("Cascadia Code",
                    _fontSizeInMM * 0.753f,
                    FontStyle.Bold, GraphicsUnit.Millimeter);
                Font drawFontRegular = new Font("Cascadia Code",
                    _fontSizeInMM,
                    FontStyle.Regular, GraphicsUnit.Millimeter);
                Font drawFontRegularBold = new Font("Cascadia Code",
                    _fontSizeInMM,
                    FontStyle.Bold, GraphicsUnit.Millimeter);
                Font drawFontRegularBold2 = new Font("Cascadia Code",
                    _fontSizeInMM * 0.895f,
                    FontStyle.Bold, GraphicsUnit.Millimeter);

                Font drawFontBiger1 = new Font("Cascadia Code",
                    _fontSizeInMM * 3,
                    FontStyle.Bold, GraphicsUnit.Millimeter);
                Font drawFontBiger2 = new Font("Cascadia Code",
                    _fontSizeInMM * 2,
                    FontStyle.Regular, GraphicsUnit.Millimeter);
                Font drawFontBiger3 = new Font("Cascadia Code",
                    _fontSizeInMM * 1.5f,
                    FontStyle.Regular, GraphicsUnit.Millimeter);
                Font drawFontBiger4 = new Font("Cascadia Code",
                    _fontSizeInMM * 1.2f,
                    FontStyle.Regular, GraphicsUnit.Millimeter);

                SolidBrush drawBrush = new SolidBrush(System.Drawing.Color.Black);
                SolidBrush drawBrushGreska = new SolidBrush(System.Drawing.Color.Red);
                SolidBrush drawBrushWhite = new SolidBrush(System.Drawing.Color.White);
                SolidBrush drawBrushGray = new SolidBrush(System.Drawing.Color.Gray);
                string[] fix = _dnevniPazarFix.Split("\r\n");
                string[] firma = _firma.Split("\r\n");
                string[] pazarItemsFix = _dnevniPazarItemsFix.Split("-");
                string[] pazarItemsProdaja = _dnevniPazarItemsProdaja.Split("\r\n");
                string[] pazarItemsSirovine = _dnevniPazarItemsSirovine.Split("\r\n");
                string[] end = _end.Split("\r\n");
                string[]? pazarItemsFixSirovine = _dnevniPazarItemsFixSirovine == null ? null : _dnevniPazarItemsFixSirovine.Split("-");
                
                List<string> signature = new List<string>();

                //float xL = 18.346456693F;
                float xL = 30F;
                float xR = 0;
                float yL = 28.346456693F;
                float yR = 28.346456693F;
                float width = 0; // max width I found through trial and error
                float height = 0F;

                width = graphics.MeasureString(line, drawFontRegular).Width;
                height = graphics.MeasureString(line, drawFontRegular).Height;

                xR = width / 2 + xL + xL - 4;

                if (_morePage is null)
                {
                    graphics.DrawString(_start, drawFontBiger2, drawBrush, xL, yL);
                    yL += graphics.MeasureString(_start, drawFontBiger2).Height;

                    foreach (var row in firma)
                    {
                        graphics.DrawString(row, drawFontRegularManji, drawBrush, xL, yL);
                        yL += graphics.MeasureString(row, drawFontRegularManji).Height;
                    }
                    yL += graphics.MeasureString(newRow, drawFontRegular).Height;

                    foreach (var row in fix)
                    {
                        graphics.DrawString(row, drawFontRegular, drawBrush, xL, yL);
                        yL += graphics.MeasureString(row, drawFontRegular).Height;
                    }
                    yL += graphics.MeasureString(newRow, drawFontRegular).Height;

                    float x = xL;
                    foreach (var row in pazarItemsFix)
                    {
                        if (row.ToLower().Contains("br"))
                        {
                            var currentY = graphics.MeasureString(row, drawFontRegularBold).Height;
                            var rect = new RectangleF(xL, yL - currentY, width,
                                graphics.MeasureString(row, drawFontRegularBold).Height + 2 * currentY);
                            graphics.FillRectangle(drawBrushGray, rect);
                        }

                        graphics.DrawString(row, drawFontRegularBold, drawBrush, x, yL);
                        x += graphics.MeasureString(row, drawFontRegularBold).Width;
                    }
                    yL += 2 * graphics.MeasureString(newRow, drawFontRegularBold).Height;
                }

                float y = yR > yL ? yR : yL;

                int ind = 0;

                if (_morePage is not null)
                {
                    ind = _morePage.Index;

                    if (_morePage.Type == Models.Type.Items)
                    {
                        y += graphics.MeasureString(newRow, drawFontRegularManji).Height;
                        for (; ind < pazarItemsProdaja.Length - 1; ind++)
                        {
                            if (y < neededHeight)
                            {
                                if (pazarItemsProdaja[ind].Contains("Ukupno za PDV"))
                                {
                                    graphics.DrawString(pazarItemsProdaja[ind], drawFontRegularManjiBold, drawBrush, xL, y);
                                    y += graphics.MeasureString(pazarItemsProdaja[ind], drawFontRegularManjiBold).Height;
                                    y += graphics.MeasureString(pazarItemsProdaja[ind], drawFontRegularManjiBold).Height;
                                    y += graphics.MeasureString(pazarItemsProdaja[ind], drawFontRegularManjiBold).Height;
                                }
                                else
                                {
                                    graphics.DrawString(pazarItemsProdaja[ind], drawFontRegularManji, drawBrush, xL, y);
                                    y += graphics.MeasureString(pazarItemsProdaja[ind], drawFontRegularManji).Height;
                                }
                            }
                            else
                            {
                                e.HasMorePages = true;
                                _morePage = new MorePage()
                                {
                                    Type = Models.Type.Items,
                                    Index = ind
                                };
                                return;
                            }
                        }

                        if (y < neededHeight)
                        {
                            graphics.DrawString(end[0], drawFontRegularBold2, drawBrush, xL, y);
                            y += graphics.MeasureString(end[0], drawFontRegularBold2).Height;
                        }
                        else
                        {
                            e.HasMorePages = true;
                            _morePage = new MorePage()
                            {
                                Type = Models.Type.End,
                                Index = 0
                            };
                            return;
                        }

                        if (_enableSirovina)
                        {
                            string sirovine = $"Artikli - Sirovine:\r\n";
                            y += graphics.MeasureString(newRow, drawFontRegular).Height;
                            y += graphics.MeasureString(newRow, drawFontRegular).Height;
                            graphics.DrawString(sirovine, drawFontRegular, drawBrush, xL, y);
                            y += graphics.MeasureString(sirovine, drawFontRegular).Height;
                            y += graphics.MeasureString(newRow, drawFontRegular).Height;

                            if (pazarItemsFixSirovine != null &&
                                pazarItemsFixSirovine.Any())
                            {
                                float x = xL;
                                foreach (var row in pazarItemsFixSirovine)
                                {
                                    if (row.ToLower().Contains("br"))
                                    {
                                        var currentY = graphics.MeasureString(row, drawFontRegularBold).Height;
                                        var rect = new RectangleF(xL, y - currentY, width,
                                            graphics.MeasureString(row, drawFontRegularBold).Height + 2 * currentY);
                                        graphics.FillRectangle(drawBrushGray, rect);
                                    }

                                    graphics.DrawString(row, drawFontRegularBold, drawBrush, x, y);
                                    x += graphics.MeasureString(row, drawFontRegularBold).Width;
                                }
                                y += 2 * graphics.MeasureString(newRow, drawFontRegularBold).Height;
                            }

                            for (int i = 0; i < pazarItemsSirovine.Length - 1; i++)
                            {
                                if (y < neededHeight)
                                {
                                    if (pazarItemsSirovine[i].Contains("Ukupno za PDV"))
                                    {
                                        graphics.DrawString(pazarItemsSirovine[i], drawFontRegularManjiBold, drawBrush, xL, y);
                                        y += graphics.MeasureString(pazarItemsSirovine[i], drawFontRegularManjiBold).Height;
                                        y += graphics.MeasureString(pazarItemsSirovine[i], drawFontRegularManjiBold).Height;
                                        y += graphics.MeasureString(pazarItemsSirovine[i], drawFontRegularManjiBold).Height;
                                    }
                                    else
                                    {
                                        if (pazarItemsSirovine[i].Contains("greska"))
                                        {
                                            var splits = pazarItemsSirovine[i].Split("greska");

                                            graphics.DrawString(splits[0], drawFontRegularManji, drawBrushGreska, xL, y);
                                            y += graphics.MeasureString(splits[0], drawFontRegularManji).Height;
                                        }
                                        else
                                        {
                                            graphics.DrawString(pazarItemsSirovine[i], drawFontRegularManji, drawBrush, xL, y);
                                            y += graphics.MeasureString(pazarItemsSirovine[i], drawFontRegularManji).Height;
                                        }
                                    }
                                }
                                else
                                {
                                    e.HasMorePages = true;
                                    _morePage = new MorePage()
                                    {
                                        Type = Models.Type.Sirovine,
                                        Index = i
                                    };
                                    return;
                                }
                            }
                        }

                        if (y < neededHeight)
                        {
                            graphics.DrawString(end[1], drawFontRegularBold2, drawBrush, xL, y);
                            y += graphics.MeasureString(end[1], drawFontRegularBold2).Height;
                        }
                        else
                        {
                            e.HasMorePages = true;
                            _morePage = new MorePage()
                            {
                                Type = Models.Type.End,
                                Index = 1
                            };
                            return;
                        }
                        
                    }
                    else if (_morePage.Type == Models.Type.Sirovine && _enableSirovina)
                    {
                        y += graphics.MeasureString(newRow, drawFontRegularManji).Height;
                        for (; ind < pazarItemsSirovine.Length - 1; ind++)
                        {
                            if (y < neededHeight)
                            {
                                if (pazarItemsSirovine[ind].Contains("Ukupno za PDV"))
                                {
                                    graphics.DrawString(pazarItemsSirovine[ind], drawFontRegularManjiBold, drawBrush, xL, y);
                                    y += graphics.MeasureString(pazarItemsSirovine[ind], drawFontRegularManjiBold).Height;
                                    y += graphics.MeasureString(pazarItemsSirovine[ind], drawFontRegularManjiBold).Height;
                                    y += graphics.MeasureString(pazarItemsSirovine[ind], drawFontRegularManjiBold).Height;
                                }
                                else
                                {
                                    if (pazarItemsSirovine[ind].Contains("greska"))
                                    {
                                        var splits = pazarItemsSirovine[ind].Split("greska");

                                        graphics.DrawString(splits[0], drawFontRegularManji, drawBrushGreska, xL, y);
                                        y += graphics.MeasureString(splits[0], drawFontRegularManji).Height;
                                    }
                                    else
                                    {
                                        graphics.DrawString(pazarItemsSirovine[ind], drawFontRegularManji, drawBrush, xL, y);
                                        y += graphics.MeasureString(pazarItemsSirovine[ind], drawFontRegularManji).Height;
                                    }
                                }
                            }
                            else
                            {
                                e.HasMorePages = true;
                                _morePage = new MorePage()
                                {
                                    Type = Models.Type.Sirovine,
                                    Index = ind
                                };
                                return;
                            }
                        }

                        if (y < neededHeight)
                        {
                            graphics.DrawString(end[1], drawFontRegularBold2, drawBrush, xL, y);
                            y += graphics.MeasureString(end[1], drawFontRegularBold2).Height;
                        }
                        else
                        {
                            e.HasMorePages = true;
                            _morePage = new MorePage()
                            {
                                Type = Models.Type.End,
                                Index = 1
                            };
                            return;
                        }
                    }
                    else if (_morePage.Type == Models.Type.End)
                    {
                        if (y < neededHeight)
                        {
                            graphics.DrawString(end[ind], drawFontRegularBold2, drawBrush, xL, y);
                            y += graphics.MeasureString(end[ind], drawFontRegularBold2).Height;
                        }
                        else
                        {
                            e.HasMorePages = true;
                            _morePage = new MorePage()
                            {
                                Type = Models.Type.End,
                                Index = ind
                            };
                            return;
                        }

                        if (ind == 0)
                        {
                            if (_enableSirovina)
                            {
                                string sirovine = $"Artikli - Sirovine:\r\n";
                                y += graphics.MeasureString(newRow, drawFontRegular).Height;
                                y += graphics.MeasureString(newRow, drawFontRegular).Height;
                                graphics.DrawString(sirovine, drawFontRegular, drawBrush, xL, y);
                                y += graphics.MeasureString(sirovine, drawFontRegular).Height;
                                y += graphics.MeasureString(newRow, drawFontRegular).Height;

                                for (int i = 0; i < pazarItemsSirovine.Length - 1; i++)
                                {
                                    if (y < neededHeight)
                                    {
                                        if (pazarItemsSirovine[i].Contains("Ukupno za PDV"))
                                        {
                                            graphics.DrawString(pazarItemsSirovine[i], drawFontRegularManjiBold, drawBrush, xL, y);
                                            y += graphics.MeasureString(pazarItemsSirovine[i], drawFontRegularManjiBold).Height;
                                            y += graphics.MeasureString(pazarItemsSirovine[i], drawFontRegularManjiBold).Height;
                                            y += graphics.MeasureString(pazarItemsSirovine[i], drawFontRegularManjiBold).Height;
                                        }
                                        else
                                        {
                                            if (pazarItemsSirovine[i].Contains("greska"))
                                            {
                                                var splits = pazarItemsSirovine[i].Split("greska");

                                                graphics.DrawString(splits[0], drawFontRegularManji, drawBrushGreska, xL, y);
                                                y += graphics.MeasureString(splits[0], drawFontRegularManji).Height;
                                            }
                                            else
                                            {
                                                graphics.DrawString(pazarItemsSirovine[i], drawFontRegularManji, drawBrush, xL, y);
                                                y += graphics.MeasureString(pazarItemsSirovine[i], drawFontRegularManji).Height;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        e.HasMorePages = true;
                                        _morePage = new MorePage()
                                        {
                                            Type = Models.Type.Sirovine,
                                            Index = i
                                        };
                                        return;
                                    }
                                }
                            }

                            if (y < neededHeight)
                            {
                                graphics.DrawString(end[1], drawFontRegularBold2, drawBrush, xL, y);
                                y += graphics.MeasureString(end[1], drawFontRegularBold2).Height;
                            }
                            else
                            {
                                e.HasMorePages = true;
                                _morePage = new MorePage()
                                {
                                    Type = Models.Type.End,
                                    Index = 1
                                };
                                return;
                            }
                        }
                    }
                }
                else
                {
                    y += graphics.MeasureString(newRow, drawFontRegularManji).Height;
                    for (int i = 0; i < pazarItemsProdaja.Length - 1; i++)
                    {
                        if (y < neededHeight)
                        {
                            if (pazarItemsProdaja[i].Contains("Ukupno za PDV"))
                            {
                                graphics.DrawString(pazarItemsProdaja[i], drawFontRegularManjiBold, drawBrush, xL, y);
                                y += graphics.MeasureString(pazarItemsProdaja[i], drawFontRegularManjiBold).Height;
                                y += graphics.MeasureString(pazarItemsProdaja[i], drawFontRegularManjiBold).Height;
                                y += graphics.MeasureString(pazarItemsProdaja[i], drawFontRegularManjiBold).Height;
                            }
                            else
                            {
                                graphics.DrawString(pazarItemsProdaja[i], drawFontRegularManji, drawBrush, xL, y);
                                y += graphics.MeasureString(pazarItemsProdaja[i], drawFontRegularManji).Height;
                            }
                        }
                        else
                        {
                            e.HasMorePages = true;
                            _morePage = new MorePage()
                            {
                                Type = Models.Type.Items,
                                Index = i
                            };
                            return;
                        }
                    }
                    if (y < neededHeight)
                    {
                        graphics.DrawString(end[0], drawFontRegularBold2, drawBrush, xL, y);
                        y += graphics.MeasureString(end[0], drawFontRegularBold2).Height;
                    }
                    else
                    {
                        e.HasMorePages = true;
                        _morePage = new MorePage()
                        {
                            Type = Models.Type.End,
                            Index = 0
                        };
                        return;
                    }

                    if (_enableSirovina)
                    {
                        string sirovine = $"Artikli - Sirovine:\r\n";
                        y += graphics.MeasureString(newRow, drawFontRegular).Height;
                        y += graphics.MeasureString(newRow, drawFontRegular).Height;
                        graphics.DrawString(sirovine, drawFontRegular, drawBrush, xL, y);
                        y += graphics.MeasureString(sirovine, drawFontRegular).Height;
                        y += graphics.MeasureString(newRow, drawFontRegular).Height;

                        for (int i = 0; i < pazarItemsSirovine.Length - 1; i++)
                        {
                            if (y < neededHeight)
                            {
                                if (pazarItemsSirovine[i].Contains("Ukupno za PDV"))
                                {
                                    graphics.DrawString(pazarItemsSirovine[i], drawFontRegularManjiBold, drawBrush, xL, y);
                                    y += graphics.MeasureString(pazarItemsSirovine[i], drawFontRegularManjiBold).Height;
                                    y += graphics.MeasureString(pazarItemsSirovine[i], drawFontRegularManjiBold).Height;
                                    y += graphics.MeasureString(pazarItemsSirovine[i], drawFontRegularManjiBold).Height;
                                }
                                else
                                {
                                    if (pazarItemsSirovine[i].Contains("greska"))
                                    {
                                        var splits = pazarItemsSirovine[i].Split("greska");

                                        graphics.DrawString(splits[0], drawFontRegularManji, drawBrushGreska, xL, y);
                                        y += graphics.MeasureString(splits[0], drawFontRegularManji).Height;
                                    }
                                    else
                                    {
                                        graphics.DrawString(pazarItemsSirovine[i], drawFontRegularManji, drawBrush, xL, y);
                                        y += graphics.MeasureString(pazarItemsSirovine[i], drawFontRegularManji).Height;
                                    }
                                }
                            }
                            else
                            {
                                e.HasMorePages = true;
                                _morePage = new MorePage()
                                {
                                    Type = Models.Type.Sirovine,
                                    Index = i
                                };
                                return;
                            }
                        }
                    }

                    if (y < neededHeight)
                    {
                        graphics.DrawString(end[1], drawFontRegularBold2, drawBrush, xL, y);
                        y += graphics.MeasureString(end[1], drawFontRegularBold2).Height;
                    }
                    else
                    {
                        e.HasMorePages = true;
                        _morePage = new MorePage()
                        {
                            Type = Models.Type.End,
                            Index = 1
                        };
                        return;
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        private static void printNorms(object sender, PrintPageEventArgs e)
        {
            try
            {
                const float neededHeight = 773.7007874F;
                string newRow = "                                                                                                    \r\n";
                string line = "----------------------------------------------------------------------------------------------------\r\n";
                Graphics graphics = e.Graphics;
                graphics.PageUnit = GraphicsUnit.Point;
                Font drawFontRegularManji = new Font("Cascadia Code",
                    _fontSizeInMM * 0.753f,
                    FontStyle.Regular, GraphicsUnit.Millimeter);
                Font drawFontRegularManjiBold = new Font("Cascadia Code",
                    _fontSizeInMM * 0.753f,
                    FontStyle.Bold, GraphicsUnit.Millimeter);
                Font drawFontRegular = new Font("Cascadia Code",
                    _fontSizeInMM,
                    FontStyle.Regular, GraphicsUnit.Millimeter);
                Font drawFontRegularBold = new Font("Cascadia Code",
                    _fontSizeInMM,
                    FontStyle.Bold, GraphicsUnit.Millimeter);

                Font drawFontBiger1 = new Font("Cascadia Code",
                    _fontSizeInMM * 3,
                    FontStyle.Bold, GraphicsUnit.Millimeter);
                Font drawFontBiger2 = new Font("Cascadia Code",
                    _fontSizeInMM * 2,
                    FontStyle.Regular, GraphicsUnit.Millimeter);
                Font drawFontBiger3 = new Font("Cascadia Code",
                    _fontSizeInMM * 1.5f,
                    FontStyle.Regular, GraphicsUnit.Millimeter);
                Font drawFontBiger4 = new Font("Cascadia Code",
                    _fontSizeInMM * 1.2f,
                    FontStyle.Regular, GraphicsUnit.Millimeter);

                SolidBrush drawBrush = new SolidBrush(System.Drawing.Color.Black);
                SolidBrush drawBrushWhite = new SolidBrush(System.Drawing.Color.White);
                SolidBrush drawBrushGray = new SolidBrush(System.Drawing.Color.Gray);
                string[] norms = _norms.Split("\r\n");
                string[] firma = _firma.Split("\r\n");

                List<string> signature = new List<string>();

                //float xL = 18.346456693F;
                float xL = 30F;
                float xR = 0;
                float yL = 18.346456693F;
                float yR = 28.346456693F;
                float width = 0; // max width I found through trial and error
                float height = 0F;

                width = graphics.MeasureString(line, drawFontRegular).Width;
                height = graphics.MeasureString(line, drawFontRegular).Height;

                xR = width / 2 + xL + xL - 4;

                if (_morePage is null)
                {
                    graphics.DrawString(_start, drawFontBiger2, drawBrush, xL, yL);
                    yL += graphics.MeasureString(_start, drawFontBiger2).Height;
                    yL += graphics.MeasureString(newRow, drawFontRegular).Height;

                    foreach (var row in firma)
                    {
                        graphics.DrawString(row, drawFontRegularManji, drawBrush, xL, yL);
                        yL += graphics.MeasureString(row, drawFontRegularManji).Height;
                    }
                    yL += graphics.MeasureString(newRow, drawFontRegular).Height;

                    float x = xL;
                    yL += 2 * graphics.MeasureString(newRow, drawFontRegularBold).Height;
                }

                float y = yR > yL ? yR : yL;

                int ind = 0;

                if (_morePage is not null)
                {
                    ind = _morePage.Index;

                    if (_morePage.Type == Models.Type.Items)
                    {
                        y += graphics.MeasureString(newRow, drawFontRegular).Height;
                        for (; ind < norms.Length - 1; ind++)
                        {
                            if (y < neededHeight)
                            {
                                graphics.DrawString(norms[ind], drawFontRegular, drawBrush, xL, y);
                                y += graphics.MeasureString(norms[ind], drawFontRegular).Height;
                            }
                            else
                            {
                                e.HasMorePages = true;
                                _morePage = new MorePage()
                                {
                                    Type = Models.Type.Items,
                                    Index = ind
                                };
                                return;
                            }
                        }
                    }
                }
                else
                {
                    y += graphics.MeasureString(newRow, drawFontRegularManji).Height;
                    for (int i = 0; i < norms.Length - 1; i++)
                    {
                        if (y < neededHeight)
                        {
                            graphics.DrawString(norms[i], drawFontRegular, drawBrush, xL, y);
                            y += graphics.MeasureString(norms[i], drawFontRegular).Height;
                        }
                        else
                        {
                            e.HasMorePages = true;
                            _morePage = new MorePage()
                            {
                                Type = Models.Type.Items,
                                Index = i
                            };
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        private static void printNivelacija(object sender, PrintPageEventArgs e)
        {
            try
            {
                const float neededHeight = 773.7007874F;
                string newRow = "                                                                                                    \r\n";
                string line = "----------------------------------------------------------------------------------------------------\r\n";
                Graphics graphics = e.Graphics;
                graphics.PageUnit = GraphicsUnit.Point;
                Font drawFontRegularManji = new Font("Cascadia Code",
                    _fontSizeInMM * 0.753f,
                    FontStyle.Regular, GraphicsUnit.Millimeter);
                Font drawFontRegularManjiBold = new Font("Cascadia Code",
                    _fontSizeInMM * 0.753f,
                    FontStyle.Bold, GraphicsUnit.Millimeter);
                Font drawFontRegular = new Font("Cascadia Code",
                    _fontSizeInMM,
                    FontStyle.Regular, GraphicsUnit.Millimeter);
                Font drawFontRegularBold = new Font("Cascadia Code",
                    _fontSizeInMM,
                    FontStyle.Bold, GraphicsUnit.Millimeter);

                Font drawFontBiger1 = new Font("Cascadia Code",
                    _fontSizeInMM * 3,
                    FontStyle.Bold, GraphicsUnit.Millimeter);
                Font drawFontBiger2 = new Font("Cascadia Code",
                    _fontSizeInMM * 2,
                    FontStyle.Regular, GraphicsUnit.Millimeter);
                Font drawFontBiger3 = new Font("Cascadia Code",
                    _fontSizeInMM * 1.5f,
                    FontStyle.Regular, GraphicsUnit.Millimeter);
                Font drawFontBiger4 = new Font("Cascadia Code",
                    _fontSizeInMM * 1.2f,
                    FontStyle.Regular, GraphicsUnit.Millimeter);

                SolidBrush drawBrush = new SolidBrush(System.Drawing.Color.Black);
                SolidBrush drawBrushWhite = new SolidBrush(System.Drawing.Color.White);
                SolidBrush drawBrushGray = new SolidBrush(System.Drawing.Color.Gray);
                string[] fix = _nivelacijaFix.Split("\r\n");
                string[] firma = _firma.Split("\r\n");
                string[] nivelacijaItemsFix = _nivelacijaItemsFix.Split("-");
                string[] nivelacijaItems = _nivelacijaItems.Split("\r\n");
                string[] end = _end.Split("\r\n");

                List<string> signature = new List<string>();

                //float xL = 18.346456693F;
                float xL = 30F;
                float xR = 0;
                float yL = 18.346456693F;
                float yR = 28.346456693F;
                float width = 0; // max width I found through trial and error
                float height = 0F;

                width = graphics.MeasureString(line, drawFontRegular).Width;
                height = graphics.MeasureString(line, drawFontRegular).Height;

                xR = width / 2 + xL + xL - 4;

                if (_morePage is null)
                {
                    graphics.DrawString(_start, drawFontBiger2, drawBrush, xL, yL);
                    yL += graphics.MeasureString(_start, drawFontBiger2).Height;

                    foreach (var row in firma)
                    {
                        graphics.DrawString(row, drawFontRegularManji, drawBrush, xL, yL);
                        yL += graphics.MeasureString(row, drawFontRegularManji).Height;
                    }
                    yL += graphics.MeasureString(newRow, drawFontRegular).Height;

                    foreach (var row in fix)
                    {
                        graphics.DrawString(row, drawFontRegular, drawBrush, xL, yL);
                        yL += graphics.MeasureString(row, drawFontRegular).Height;
                    }
                    yL += graphics.MeasureString(newRow, drawFontRegular).Height;

                    float x = xL;
                    foreach (var row in nivelacijaItemsFix)
                    {
                        if (row.ToLower().Contains("artikal"))
                        {
                            var currentY = graphics.MeasureString(row, drawFontRegularBold).Height;
                            var rect = new RectangleF(xL, yL - currentY, width,
                                graphics.MeasureString(row, drawFontRegularBold).Height + 2 * currentY);
                            graphics.FillRectangle(drawBrushGray, rect);
                        }

                        graphics.DrawString(row, drawFontRegularBold, drawBrush, x, yL);
                        x += graphics.MeasureString(row, drawFontRegularBold).Width;
                    }
                    yL += 2 * graphics.MeasureString(newRow, drawFontRegularBold).Height;
                }

                float y = yR > yL ? yR : yL;

                int ind = 0;

                if (_morePage is not null)
                {
                    ind = _morePage.Index;

                    if (_morePage.Type == Models.Type.Items)
                    {
                        y += graphics.MeasureString(newRow, drawFontRegularManji).Height;
                        for (; ind < nivelacijaItems.Length - 1; ind++)
                        {
                            if (y < neededHeight)
                            {
                                graphics.DrawString(nivelacijaItems[ind], drawFontRegularManji, drawBrush, xL, y);
                                y += graphics.MeasureString(nivelacijaItems[ind], drawFontRegularManji).Height;
                            }
                            else
                            {
                                e.HasMorePages = true;
                                _morePage = new MorePage()
                                {
                                    Type = Models.Type.Items,
                                    Index = ind
                                };
                                return;
                            }
                        }
                        for (int i = 0; i < end.Length - 1; i++)
                        {
                            if (y < neededHeight)
                            {
                                graphics.DrawString(end[i], drawFontRegularManjiBold, drawBrush, xL, y);
                                y += graphics.MeasureString(end[i], drawFontRegularManjiBold).Height;
                            }
                            else
                            {
                                e.HasMorePages = true;
                                _morePage = new MorePage()
                                {
                                    Type = Models.Type.End,
                                    Index = i
                                };
                                return;
                            }
                        }
                    }
                    else if (_morePage.Type == Models.Type.End)
                    {
                        for (; ind < end.Length - 1; ind++)
                        {
                            if (y < neededHeight)
                            {
                                graphics.DrawString(end[ind], drawFontRegularManjiBold, drawBrush, xL, y);
                                y += graphics.MeasureString(end[ind], drawFontRegularManjiBold).Height;
                            }
                            else
                            {
                                e.HasMorePages = true;
                                _morePage = new MorePage()
                                {
                                    Type = Models.Type.End,
                                    Index = ind
                                };
                                return;
                            }
                        }
                    }
                }
                else
                {
                    y += graphics.MeasureString(newRow, drawFontRegularManji).Height;
                    for (int i = 0; i < nivelacijaItems.Length - 1; i++)
                    {
                        if (y < neededHeight)
                        {
                            graphics.DrawString(nivelacijaItems[i], drawFontRegularManji, drawBrush, xL, y);
                            y += graphics.MeasureString(nivelacijaItems[i], drawFontRegularManji).Height;
                        }
                        else
                        {
                            e.HasMorePages = true;
                            _morePage = new MorePage()
                            {
                                Type = Models.Type.Items,
                                Index = i
                            };
                            return;
                        }
                    }
                    for (int i = 0; i < end.Length; i++)
                    {
                        if (y < neededHeight)
                        {
                            graphics.DrawString(end[i], drawFontRegularManjiBold, drawBrush, xL, y);
                            y += graphics.MeasureString(end[i], drawFontRegularManjiBold).Height;
                        }
                        else
                        {
                            e.HasMorePages = true;
                            _morePage = new MorePage()
                            {
                                Type = Models.Type.End,
                                Index = i
                            };
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        private static string GetItemDnevniPazar(Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsPDV,
            string PDV,
            bool isSirovine)
        {
            string result = string.Empty;


            decimal totalNivelacija = 0;
            decimal totalAmountDecimal = 0;
            foreach (var item in allItemsPDV)
            {
                result += $"{item.Key}\r\n";
                var items = item.Value.OrderBy(item => item.Key).ToDictionary(x => x.Key, x => x.Value);

                foreach (var i in items)
                {
                    i.Value.ForEach(item =>
                    {

                        if (isSirovine == item.IsSirovina)
                        {
                            string name = $"{item.ItemId} - {item.Name}";

                            name = SplitInParts(name, "", 32, 1);

                            string[] splitName = name.Split("\r\n");

                            if (splitName.Length > 1)
                            {
                                name = string.Empty;
                                int length = splitName.Length;
                                if (splitName[splitName.Length - 1].Length == 0)
                                {
                                    length = splitName.Length - 1;
                                }
                                for (int j = 0; j < length; j++)
                                {
                                    if (j == length - 1)
                                    {
                                        name += $"        {splitName[j]}";
                                    }
                                    else
                                    {
                                        name += $"{splitName[j]}\r\n";
                                    }
                                }
                            }

                            decimal niv = item.MPC_Average > 0 ? Decimal.Round((item.MPC_Average * 100 / item.MPC_Original) - 100, 2) : 0;

                            string counter = CenterString(_dnevniPazarCounter++.ToString(), 4, false).PadRight(8);
                            string jm = item.JM.PadLeft(6);
                            string quantity = string.Format("{0:#,##0.00}", item.Quantity).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(13);
                            string MPC = string.Format("{0:#,##0.00}", item.MPC_Original).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(16);
                            string MPC_Average = string.Format("{0:#,##0.00}", item.MPC_Average).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(16);
                            string totalAmount = string.Format("{0:#,##0.00}", item.TotalAmount).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(16);
                            string nivelacija = string.Format("{0:#,##0.00}", item.Nivelacija).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(14);
                            string marza = (string.Format("{0:#,##0.00}", niv).Replace(',', '#').Replace('.', ',').Replace('#', '.') + "%").PadLeft(12);

                            if (item.TotalAmount == 0)
                            {
                                result += $"{counter}{name}{jm}{MPC}{quantity}" +
                                    $"{MPC_Average}{totalAmount}{nivelacija}{marza}greska\r\n";
                            }
                            else
                            {
                                result += $"{counter}{name}{jm}{MPC}{quantity}" +
                                    $"{MPC_Average}{totalAmount}{nivelacija}{marza}\r\n";
                            }

                            result += "-------------------------------------------------------------------------------------------------------------------------------------\r\n";

                            totalAmountDecimal += item.TotalAmount;
                            if (!item.IsSirovina)
                            {
                                totalNivelacija += item.Nivelacija;

                                _dnevniPazarTotalAmountProdaja += item.TotalAmount;
                                _dnevniPazarNivelacijaProdaja += item.Nivelacija;
                            }
                            else
                            {
                                _dnevniPazarTotalAmountSirovine += item.TotalAmount;
                            }
                        }
                    });
                }
            }
            string totalNivelacijaString = string.Format("{0:#,##0.00}", totalNivelacija).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(14);
            string totalAmountString = string.Format("{0:#,##0.00}", totalAmountDecimal).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(77);
            string pdv = PDV.Contains("Nije") ? PDV : PDV + "%";
            pdv = pdv.PadRight(10);
            result += $"    Ukupno za PDV: {pdv} {totalAmountString}{totalNivelacijaString}\r\n\r\n";

            return result;
        }
        private static string GetItemA4InventoryStatus(List<InvertoryGlobal> inventoryStatus)
        {
            string result = string.Empty;

            foreach (var item in inventoryStatus)
            {
                string name = $"{item.Id} - {item.Name}";

                name = SplitInParts(name, "", 43, 1);

                string[] splitName = name.Split("\r\n");

                if (splitName.Length > 1)
                {
                    name = string.Empty;
                    int length = splitName.Length;
                    if (splitName[splitName.Length - 1].Length == 0)
                    {
                        length = splitName.Length - 1;
                    }
                    for (int j = 0; j < length; j++)
                    {
                        if (j == length - 1)
                        {
                            name += $"        {splitName[j]}";
                        }
                        else
                        {
                            name += $"{splitName[j]}\r\n";
                        }
                    }
                }

                string counter = CenterString(_dnevniPazarCounter++.ToString(), 4, false).PadRight(8);
                string inputPrice = string.Format("{0:#,##0.00}", item.InputUnitPrice).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(13);
                string quantity = string.Format("{0:#,##0.00}", item.Quantity).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(21);
                string jm = item.Jm.PadLeft(11);
                string sellingPrice = string.Format("{0:#,##0.00}", item.SellingUnitPrice).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(15);
                string totalAmount = string.Format("{0:#,##0.00}", item.TotalAmout).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(22);

                if(item.TotalAmout < 0 ||
                    item.Quantity < 0 ||
                    item.InputUnitPrice < 0)
                {
                    result += $"{counter}{name}{inputPrice}{quantity}{jm}{sellingPrice}{totalAmount}greska\r\n";
                }
                else
                {
                    result += $"{counter}{name}{inputPrice}{quantity}{jm}{sellingPrice}{totalAmount}\r\n";
                }

                result += "-------------------------------------------------------------------------------------------------------------------------------------\r\n";
            }

            return result;
        }
        private static string GetItemDnevniPazar1010(Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsPDV,
            string PDV,
            bool isSirovine)
        {
            string result = string.Empty;

            decimal totalAmountDecimal = 0;
            foreach (var i in allItemsPDV)
            {
                result += $"{i.Key}\r\n";

                var items = i.Value.OrderBy(item => item.Key).ToDictionary(x => x.Key, x => x.Value);

                foreach (var it in items)
                {
                    it.Value.ForEach(item =>
                    {
                        if (isSirovine == item.IsSirovina)
                        {
                            string name = $"{item.ItemId} - {item.Name}";

                            name = SplitInParts(name, "", 52, 1);

                            string[] splitName = name.Split("\r\n");

                            if (splitName.Length > 1)
                            {
                                name = string.Empty;
                                int length = splitName.Length;
                                if (splitName[splitName.Length - 1].Length == 0)
                                {
                                    length = splitName.Length - 1;
                                }
                                for (int j = 0; j < length; j++)
                                {
                                    if (j == length - 1)
                                    {
                                        name += $"        {splitName[j]}";
                                    }
                                    else
                                    {
                                        name += $"{splitName[j]}\r\n";
                                    }
                                }
                            }

                            string counter = CenterString(_dnevniPazarCounter++.ToString(), 4, false).PadRight(8);
                            string jm = item.JM.PadLeft(7);
                            string inputPrice = string.Format("{0:#,##0.00}", item.MPC_Average).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(22);
                            string quantity = string.Format("{0:#,##0.000}", item.Quantity).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(20);
                            string totalAmount = string.Format("{0:#,##0.00}", item.TotalAmount).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(24);

                            if (item.TotalAmount < 0)
                            {
                                result += $"{counter}{name}{jm}{inputPrice}{quantity}" +
                                    $"{totalAmount}greska\r\n";
                            }
                            else
                            {
                                result += $"{counter}{name}{jm}{inputPrice}{quantity}" +
                                    $"{totalAmount}\r\n";
                            }

                            result += "-------------------------------------------------------------------------------------------------------------------------------------\r\n";

                            totalAmountDecimal += item.TotalAmount;
                            if (!item.IsSirovina)
                            {

                                _dnevniPazarTotalAmountProdaja += item.TotalAmount;
                            }
                            else
                            {
                                _dnevniPazarTotalAmountSirovine += item.TotalAmount;
                            }
                        }
                    });
                }
            }
            string totalAmountString = string.Format("{0:#,##0.00}", totalAmountDecimal).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(103);
            string pdv = PDV.Contains("Nije") ? PDV : PDV + "%";
            pdv = pdv.PadRight(10);
            result += $"    Ukupno za PDV: {pdv} {totalAmountString}\r\n\r\n";

            return result;
        }
        private static string GetKepItems(List<ItemKEP> kep)
        {
            string result = string.Empty;

            kep.ForEach(k =>
            {
                string description = SplitInParts(k.Description, "", 40, 1);
                string[] splitName = description.Split("\r\n");

                if (splitName.Length > 1)
                {
                    description = string.Empty;
                    int length = splitName.Length;
                    if (splitName[splitName.Length - 1].Length == 0)
                    {
                        length = splitName.Length - 1;
                    }
                    for (int j = 0; j < length; j++)
                    {
                        if (j == length - 1)
                        {
                            description += $"               {splitName[j]}";
                        }
                        else
                        {
                            description += $"{splitName[j]}\r\n";
                        }
                    }
                }

                string date = k.KepDate.ToString("dd.MM.yyyy").PadRight(15);
                string zaduzenje = string.Format("{0:#,##0.00}", k.Zaduzenje).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(24);
                string razduzenje = string.Format("{0:#,##0.00}", k.Razduzenje).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(27); 
                string saldo = string.Format("{0:#,##0.00}", k.Saldo).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(27);

                result += $"{date}{description}{zaduzenje}{razduzenje}{saldo}\r\n";
                result += "-------------------------------------------------------------------------------------------------------------------------------------\r\n";

                _kepZaduzenje += k.Zaduzenje;
                _kepRazduzenje += k.Razduzenje;
            });

            return result;
        }
        private static string GetDnevniPazarItems(
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItems20PDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItems10PDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItems0PDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsNoPDV,
            bool isSirovine)
        {
            string result = string.Empty;

            if (allItems20PDV.Any())
            {
                result += GetItemDnevniPazar(allItems20PDV, "20", isSirovine);
            }
            if (allItems10PDV.Any())
            {
                result += GetItemDnevniPazar(allItems10PDV, "10", isSirovine);
            }
            if (allItems0PDV.Any())
            {
                result += GetItemDnevniPazar(allItems0PDV, "0", isSirovine);
            }
            if (allItemsNoPDV.Any())
            {
                result += GetItemDnevniPazar(allItemsNoPDV, "Nije u PDV", isSirovine);
            }

            return result;
        }
        private static string GetDnevniPazarItems1010(
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItems20PDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItems10PDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItems0PDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsNoPDV,
            bool isSirovine)
        {
            string result = string.Empty;

            if (allItems20PDV.Any())
            {
                result += GetItemDnevniPazar1010(allItems20PDV, "20", isSirovine);
            }
            if (allItems10PDV.Any())
            {
                result += GetItemDnevniPazar1010(allItems10PDV, "10", isSirovine);
            }
            if (allItems0PDV.Any())
            {
                result += GetItemDnevniPazar1010(allItems0PDV, "0", isSirovine);
            }
            if (allItemsNoPDV.Any())
            {
                result += GetItemDnevniPazar1010(allItemsNoPDV, "Nije u PDV", isSirovine);
            }

            return result;
        }
        private static string GetNivelacijaItems(NivelacijaGlobal nivelacija)
        {
            string result = string.Empty;

            foreach (var item in nivelacija.NivelacijaItems)
            {
                string name = $"{item.IdItem} - {item.Name}";

                name = SplitInParts(name, "", 18, 1);

                string[] splitName = name.Split("\r\n");

                if (splitName.Length > 1)
                {
                    name = string.Empty;
                    int length = splitName.Length;
                    if (splitName[splitName.Length - 1].Length == 0)
                    {
                        length = splitName.Length - 1;
                    }
                    for (int j = 0; j < length; j++)
                    {
                        if (j == length - 1)
                        {
                            name += $"{splitName[j]}";
                        }
                        else
                        {
                            name += $"{splitName[j]}\r\n";
                        }
                    }
                }

                string jm = item.Jm.PadLeft(6);
                string quantity = string.Format("{0:#,##0.00}", item.Quantity).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(13);
                string staraCena = string.Format("{0:#,##0.00}", item.OldPrice).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(12);
                string novaCena = string.Format("{0:#,##0.00}", item.NewPrice).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(12);
                string staraVrednost = string.Format("{0:#,##0.00}", item.OldTotalValue).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(13);
                string novaVrednost = string.Format("{0:#,##0.00}", item.NewTotalValue).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(14);
                string vrednostNivelacije = string.Format("{0:#,##0.00}", (item.NewTotalValue - item.OldTotalValue)).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(14);
                string stopaPDV = (string.Format("{0:#,##0.00}", item.StopaPDV).Replace(',', '#').Replace('.', ',').Replace('#', '.') + "%").PadLeft(9);
                string razlikaPDV = string.Format("{0:#,##0.00}", (item.NewTotalPDV - item.OldTotalPDV)).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(12);
                string marza = (string.Format("{0:#,##0.00}", item.Marza).Replace(',', '#').Replace('.', ',').Replace('#', '.') + "%").PadLeft(10);

                result += $"{name}{jm}{quantity}{staraCena}" +
                    $"{novaCena}{staraVrednost}{novaVrednost}" +
                    $"{vrednostNivelacije}{stopaPDV}{razlikaPDV}{marza}\r\n";

                result += "-------------------------------------------------------------------------------------------------------------------------------------\r\n";

                _totalNewValue += item.NewTotalValue;
                _totalOldValue += item.OldTotalValue;
                _totalQuantity += item.Quantity;
                _totalRazlikaPDV += item.NewTotalPDV - item.OldTotalPDV;
                _totalNivelacija += item.NewTotalValue - item.OldTotalValue;
            }

            return result;
        }
        private static string CenterString(string value, int length, bool newRow = true)
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

                if (newRow)
                {
                    return $"{value.PadLeft(padLeft).PadRight(length)}\r\n";
                }
                else
                {
                    return $"{value.PadLeft(padLeft).PadRight(length)}";
                }
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
                    journal = string.Format("{0}{1}", fixedPart, value.PadRight(length));
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
        #endregion Private methods
    }
}
