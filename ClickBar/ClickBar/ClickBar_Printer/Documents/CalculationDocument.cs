using ClickBar_Common.Models.Statistic;
using ClickBar_Logging;
using ClickBar_Printer.Models;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClickBar_DatabaseSQLManager.Models;

namespace ClickBar_Printer.Documents
{
    internal class CalculationDocument
    {
        #region Fields
        private static MorePage? _morePage;

        private static readonly float _fontSizeInMM = 3.12f;
        private static int _width;

        private static string _start;
        private static string _end;

        private static string _firma;
        private static string _centralno;
        private static string _dobavljac;

        private static string _naslov;
        private static List<string> _calculationFix;
        private static List<string> _calculationItems;

        private static string _potpis;

        private static List<CCS_Tabela> _tabela;

        private static decimal _totalInputPrice;
        private static decimal _totalSellingPrice;
        private static decimal _totalSellingBezPDVPrice;
        private static decimal _totalMarza;

        private static string _ukupno;
        #endregion Fields

        #region Public methods
        public static void PrintCalculation(CalculationDB calculationDB,
            List<InvertoryGlobal> items,
            SupplierDB supplierDB,
            FirmaDB firma)
        {
            try
            {
                _firma = string.Empty;
                _firma += string.IsNullOrEmpty(firma.Name) ? "" : $"{"Naziv firme: "}{firma.Name}\r\n";
                _firma += string.IsNullOrEmpty(firma.Pib) ? "" : $"{"PIB: "}{firma.Pib}\r\n";
                _firma += string.IsNullOrEmpty(firma.MB) ? "" : $"{"MB: "}{firma.MB}\r\n";
                _firma += string.IsNullOrEmpty(firma.AddressPP) ? "" : $"{"Adresa: "}{firma.AddressPP}\r\n";
                _firma += string.IsNullOrEmpty(firma.Email) ? "" : $"{"Email: "}{firma.Email}\r\n";
                _firma += string.IsNullOrEmpty(firma.BankAcc) ? "" : $"{"Račun: "}{firma.BankAcc}\r\n";

                _centralno = string.Empty;
                _centralno += CenterString($"Datum: {calculationDB.CalculationDate.ToString("dd.MM.yyyy")}", 109);
                _centralno += string.IsNullOrEmpty(calculationDB.InvoiceNumber) ? string.Empty :
                    CenterString($"Po dokumentu: {calculationDB.InvoiceNumber}", 109);

                _dobavljac = "Komitent:\r\n".PadLeft(85);
                _dobavljac += string.IsNullOrEmpty(supplierDB.Name) ? "" : $"{"Naziv firme: "}{supplierDB.Name}\r\n".PadLeft(109);
                _dobavljac += string.IsNullOrEmpty(supplierDB.Pib) ? "" : $"{"PIB: "}{supplierDB.Pib}\r\n".PadLeft(109);
                _dobavljac += string.IsNullOrEmpty(supplierDB.Mb) ? "" : $"{"MB: "}{supplierDB.Mb}\r\n".PadLeft(109);
                _dobavljac += string.IsNullOrEmpty(supplierDB.City) ? "" : $"{"Grad: "}{supplierDB.City}\r\n".PadLeft(109);
                _dobavljac += string.IsNullOrEmpty(supplierDB.Address) ? "" : $"{"Adresa: "}{supplierDB.Address}\r\n".PadLeft(109);
                _dobavljac += string.IsNullOrEmpty(supplierDB.Email) ? "" : $"{"Email: "}{supplierDB.Email}\r\n".PadLeft(109);
                _dobavljac += string.IsNullOrEmpty(supplierDB.ContractNumber) ? "" : $"{"Kontakt: "}{supplierDB.ContractNumber}\r\n".PadLeft(109);

                _naslov = CenterString("KALKULACIJA", 73);
                _naslov += CenterString($"BROJ KALKULACIJE: {calculationDB.Counter}", 73);

                _totalInputPrice = 0;
                _totalSellingPrice = 0;
                _totalSellingBezPDVPrice = 0;
                _totalMarza = 0;

                _calculationFix = new List<string>()
                {
                    "Rbr",
                    "Artikal",
                    "JM",
                    "Količina",
                    "Ulazna cena",
                    "Iznos ulaza",
                    "Rabat",
                    "Marža",
                    "PDV",
                    "Cena\r\nbez PDV",
                    "Vrednost\r\nbez PDV",
                    "Prodajna\r\ncena",
                    "Prodajna\r\nvrednost"
                };

                int counter = 1;
                _calculationItems = new List<string>();
                items.ForEach(item =>
                {
                    //decimal pdv = ;
                    //decimal neto = item.TotalAmout * 100 / (100 + item.);

                    _calculationItems.Add(counter.ToString());
                    _calculationItems.Add(SplitInParts($"{item.Id} - {item.Name}", "", 48, 1));
                    _calculationItems.Add(item.Jm);
                    _calculationItems.Add(string.Format("{0:#,##0.000}", Decimal.Round(item.Quantity, 3)).Replace(',', '#').Replace('.', ',').Replace('#', '.'));
                    _calculationItems.Add(string.Format("{0:#,##0.00}", item.InputUnitPrice).Replace(',', '#').Replace('.', ',').Replace('#', '.'));
                    _calculationItems.Add(string.Format("{0:#,##0.00}", Decimal.Round(item.Quantity * item.InputUnitPrice, 2)).Replace(',', '#').Replace('.', ',').Replace('#', '.'));
                    _calculationItems.Add(string.Format("{0:#,##0.00}", 0).Replace(',', '#').Replace('.', ',').Replace('#', '.'));
                    _calculationItems.Add(string.Format("{0:#,##0.00}", item.SellingUnitPrice - item.InputUnitPrice).Replace(',', '#').Replace('.', ',').Replace('#', '.'));
                    _calculationItems.Add(string.Format("{0:#,##0.00}", 0).Replace(',', '#').Replace('.', ',').Replace('#', '.'));
                    _calculationItems.Add(string.Format("{0:#,##0.00}", item.SellingUnitPrice).Replace(',', '#').Replace('.', ',').Replace('#', '.'));
                    _calculationItems.Add(string.Format("{0:#,##0.00}", item.TotalAmout).Replace(',', '#').Replace('.', ',').Replace('#', '.'));
                    _calculationItems.Add(string.Format("{0:#,##0.00}", item.SellingUnitPrice).Replace(',', '#').Replace('.', ',').Replace('#', '.'));
                    _calculationItems.Add(string.Format("{0:#,##0.00}", item.TotalAmout).Replace(',', '#').Replace('.', ',').Replace('#', '.'));

                    counter++;

                    _totalInputPrice += Decimal.Round(item.Quantity * item.InputUnitPrice, 2);
                    _totalSellingPrice += item.TotalAmout;
                    _totalSellingBezPDVPrice += item.TotalAmout;
                    _totalMarza += item.TotalAmout - Decimal.Round(item.Quantity * item.InputUnitPrice, 2);
                });

                string totalInputPriceString = string.Format("{0:#,##0.00}", _totalInputPrice).Replace(',', '#').Replace('.', ',').Replace('#', '.');
                string totalSellingPriceString = string.Format("{0:#,##0.00}", _totalSellingPrice).Replace(',', '#').Replace('.', ',').Replace('#', '.');
                string totalSellingBezPDVPriceString = string.Format("{0:#,##0.00}", _totalSellingBezPDVPrice).Replace(',', '#').Replace('.', ',').Replace('#', '.');
                string totalMarzaString = string.Format("{0:#,##0.00}", _totalMarza).Replace(',', '#').Replace('.', ',').Replace('#', '.');

                _ukupno = $"UKUPNO: {totalInputPriceString.PadLeft(93)}{totalMarzaString.PadLeft(29)}{totalSellingBezPDVPriceString.PadLeft(36)}{totalSellingPriceString.PadLeft(29)}";

                _tabela = new List<CCS_Tabela>();
                _tabela.Add(new CCS_Tabela(23.244094488F, 23.244094488F));
                _tabela.Add(new CCS_Tabela(191.055118108F, 23.244094488F));
                _tabela.Add(new CCS_Tabela(23.244094488F, 23.244094488F));
                _tabela.Add(new CCS_Tabela(57.25984252F, 23.244094488F));
                _tabela.Add(new CCS_Tabela(57.25984252F, 23.244094488F));
                _tabela.Add(new CCS_Tabela(57.25984252F, 23.244094488F));
                _tabela.Add(new CCS_Tabela(57.25984252F, 23.244094488F));
                _tabela.Add(new CCS_Tabela(57.25984252F, 23.244094488F));
                _tabela.Add(new CCS_Tabela(23.244094488F, 23.244094488F));
                _tabela.Add(new CCS_Tabela(57.25984252F, 23.244094488F));
                _tabela.Add(new CCS_Tabela(57.25984252F, 23.244094488F));
                _tabela.Add(new CCS_Tabela(57.25984252F, 23.244094488F));
                _tabela.Add(new CCS_Tabela(57.25984252F, 23.244094488F));

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

                    pdoc.DefaultPageSettings.Landscape = true;
                    _morePage = null;
                    pdoc.PrintPage += new PrintPageEventHandler(dailyDep);
                    pdoc.Print();
                    pdoc.PrintPage -= new PrintPageEventHandler(dailyDep);
                    _morePage = null;
                }
            }
            catch (Exception ex)
            {
                Log.Error("CalculationDocument - PrintCalculation - Greska prilokom stampe kalkulacije: ", ex);
            }
        }
        #endregion Public methods

        #region Private methods
        private static void dailyDep(object sender, PrintPageEventArgs e)
        {
            try
            {
                const float debljinaLinije = 0.8503937008F;
                const float fontHeight = 7.7480314961F;
                const float neededHeight = 524.40944882F;
                const float neededWidth = 776.125984252F;
                const float sirinaSlova = 3.842207843F;
                Graphics graphics = e.Graphics;
                graphics.PageUnit = GraphicsUnit.Point;

                Font naslovFont = new Font("Cascadia Code",
                    18, FontStyle.Bold);

                Font calculationInformationFont = new Font("Cascadia Code",
                    12);

                Font calculationFixFont = new Font("Cascadia Code",
                    11);

                Font calculation8Font = new Font("Cascadia Code",
                    8);
                Font calculationItemsFont = new Font("Cascadia Code",
                    6.5F);

                SolidBrush drawBrush = new SolidBrush(System.Drawing.Color.Black);
                SolidBrush drawBrushWhite = new SolidBrush(System.Drawing.Color.White);

                string[] naslov = _naslov.Split("\r\n");
                string[] firma = _firma.Split("\r\n");
                string[] centar = _centralno.Split("\r\n");
                string[] dobavljac = _dobavljac.Split("\r\n");
                //string[] potpis = _potpis.Split("\r\n");

                float xL = 35.433070866F;
                float yL = 35.433070866F;
                float yC = 35.433070866F;
                float yR = 35.433070866F;

                if (_morePage is null)
                {
                    foreach (var row in naslov)
                    {
                        graphics.DrawString(row, naslovFont, drawBrush, xL, yL);
                        yL += graphics.MeasureString(row, naslovFont).Height;
                    }

                    yL += 2 * fontHeight;

                    yC = yL;
                    yR = yL;
                    foreach (var row in firma)
                    {
                        graphics.DrawString(row, calculationInformationFont, drawBrush, xL, yL);
                        yL += graphics.MeasureString(row, calculationInformationFont).Height;
                    }

                    foreach (var row in centar)
                    {
                        graphics.DrawString(row, calculationInformationFont, drawBrush, xL, yC);
                        yC += graphics.MeasureString(row, calculationInformationFont).Height;
                    }

                    foreach (var row in dobavljac)
                    {
                        graphics.DrawString(row, calculationInformationFont, drawBrush, xL, yR);
                        yR += graphics.MeasureString(row, calculationInformationFont).Height;
                    }

                    if (yC > yL)
                    {
                        yL = yC;
                    }
                    if (yR > yL)
                    {
                        yL = yR;
                    }

                    yL += 3 * fontHeight;

                    for (int j = 0; j <= _calculationItems.Count / _tabela.Count; j++)
                    {
                        float x = xL;
                        for (int i = 0; i < _tabela.Count; i++)
                        {
                            var rect = new RectangleF(x, yL, _tabela[i].Sirina, _tabela[i].Visina);
                            graphics.FillRectangle(drawBrush, rect);

                            if (i < _tabela.Count - 1)
                            {
                                rect = new RectangleF(x + debljinaLinije,
                                    yL + debljinaLinije,
                                    _tabela[i].Sirina - debljinaLinije,
                                    _tabela[i].Visina - debljinaLinije);
                                graphics.FillRectangle(drawBrushWhite, rect);
                            }
                            else
                            {
                                rect = new RectangleF(x + debljinaLinije,
                                    yL + debljinaLinije,
                                    _tabela[i].Sirina - 2 * debljinaLinije,
                                    _tabela[i].Visina - debljinaLinije);
                                graphics.FillRectangle(drawBrushWhite, rect);
                            }

                            if (j == 0)
                            {
                                var ukupnoSlova = _tabela[i].Sirina / sirinaSlova;

                                if (_calculationFix[i].Contains("\r\n"))
                                {
                                    var splitRow = _calculationFix[i].Split("\r\n");

                                    float sredinaPolja = 0;

                                    if (splitRow.Count() == 2)
                                    {
                                        sredinaPolja = fontHeight / 2;
                                    }
                                    else
                                    {
                                        sredinaPolja = fontHeight;
                                    }
                                    float rowCounter = 1;
                                    foreach (var r in splitRow)
                                    {
                                        string row = CenterString(r, Convert.ToInt32(ukupnoSlova));
                                        graphics.DrawString(row, calculationItemsFont, drawBrush, x, yL + fontHeight * rowCounter - sredinaPolja);
                                        rowCounter++;
                                    }
                                }
                                else
                                {
                                    string row = CenterString(_calculationFix[i], Convert.ToInt32(ukupnoSlova));
                                    graphics.DrawString(row, calculationItemsFont, drawBrush, x, yL + fontHeight);
                                }
                            }
                            else
                            {
                                var ukupnoSlova = _tabela[i].Sirina / sirinaSlova;

                                int indexItems = i + (j - 1) * _tabela.Count;

                                if (_calculationItems[indexItems].Contains("\r\n"))
                                {
                                    var splitRow = _calculationItems[indexItems].Split("\r\n");

                                    if (string.IsNullOrEmpty(splitRow[splitRow.Count() - 1]))
                                    {
                                        splitRow = splitRow.Take(splitRow.Count() - 1).ToArray();
                                    }

                                    float sredinaPolja = 0;

                                    if (splitRow.Count() == 2)
                                    {
                                        sredinaPolja = fontHeight / 2;
                                    }
                                    else
                                    {
                                        sredinaPolja = fontHeight;
                                    }
                                    float rowCounter = 1;
                                    foreach (var r in splitRow)
                                    {
                                        string row = CenterString(r, Convert.ToInt32(ukupnoSlova));
                                        graphics.DrawString(row, calculationItemsFont, drawBrush, x, yL + fontHeight * rowCounter - sredinaPolja);
                                        rowCounter++;
                                    }
                                }
                                else
                                {
                                    string row = CenterString(_calculationItems[indexItems], Convert.ToInt32(ukupnoSlova));
                                    graphics.DrawString(row, calculationItemsFont, drawBrush, x, yL + fontHeight);
                                }
                            }
                            x += _tabela[i].Sirina;
                        }

                        yL += fontHeight * 3;

                        if (j == _calculationItems.Count / _tabela.Count)
                        {
                            x = xL;

                            var rect = new RectangleF(x, yL, neededWidth, debljinaLinije);
                            graphics.FillRectangle(drawBrush, rect);
                        }
                    }
                }

                yL += fontHeight;
                graphics.DrawString(_ukupno, calculationItemsFont, drawBrush, xL, yL);
            }
            catch (Exception ex)
            {
                Log.Error($"CalculationDocument -> dailyDep -> Desila se greska prilikom kreiranja kalkulacije a4: ", ex);
            }
        }

        private static string CenterString(string value, int length, bool newRow = true)
        {
            string journal = string.Empty;
            if (string.IsNullOrEmpty(value))
            {
                value = string.Empty;
            }

            if (value.Length <= length)
            {
                int spaces = length - value.Length;
                int padL = spaces / 2 + value.Length;

                if (newRow)
                {
                    return $"{value.PadLeft(padL).PadRight(length)}\r\n";
                }
                else
                {
                    var result = $"{value.PadLeft(padL).PadRight(length)}";
                    return result;
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
                int padL = spaces / 2 + s.Length;

                journal += $"{s.PadLeft(padL).PadRight(length)}\r\n";

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

            int prebaceno = 0;
            int currentLength = 0;

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

                string s = str.Substring(currentLength, len);

                prebaceno = 0;
                while (journalLength > length &&
                    s[s.Length - 1] != ' ')
                {
                    s = s.Remove(s.Length - 1, 1);

                    prebaceno++;
                }

                journal += string.Format("{0}\r\n", s.PadRight(length));

                currentLength += s.Length;
                journalLength -= s.Length;
                counter++;
            }

            return journal;
        }
        #endregion Private methods
    }
}
