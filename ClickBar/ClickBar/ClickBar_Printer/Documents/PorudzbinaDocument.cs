using ClickBar_Printer.Enums;
using ClickBar_Printer.Models.DrljaKuhinja;
using ClickBar_Settings;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_Printer.Documents
{
    internal class PorudzbinaDocument
    {
        private static readonly float _fontSize80mm = 3.90f;
        private static readonly float _fontSize58mm = 2.90f;

        private static string _journal;
        private static int _width;
        private static decimal _totalAmount;

        public static void PrintPorudzbina(PorudzbinaPrint order, OrderTypeEnumeration orderTypeEnumeration)
        {
            string? prName = null;

            if (orderTypeEnumeration == OrderTypeEnumeration.Sank)
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
                _journal = CreatePorudzbina(order);

                var pdoc = new PrintDocument();
                PrinterSettings ps = new PrinterSettings();
                pdoc.PrinterSettings.PrinterName = prName;

                int width = Convert.ToInt32(pdoc.PrinterSettings.DefaultPageSettings.PaperSize.Width / 100 * 25.4);

                if (width > 70)
                {
                    width = 70;
                }

                _width = width;

                pdoc.PrintPage += new PrintPageEventHandler(dailyDep);

                pdoc.Print();

                pdoc.PrintPage -= new PrintPageEventHandler(dailyDep);
            }
        }

        private static void dailyDep(object sender, PrintPageEventArgs e)
        {
            try
            {
                Graphics graphics = e.Graphics;
                graphics.PageUnit = GraphicsUnit.Point;
                //Font drawFontRegular = new Font("Cascadia Code",
                //    _fontSize80mm,
                //    System.Drawing.FontStyle.Regular, GraphicsUnit.Millimeter);
                //Font drawFontUpperBold = new Font("Cascadia Code",
                //    drawFontRegular.SizeInPoints * 1.5f,
                //    System.Drawing.FontStyle.Bold, GraphicsUnit.Point);
                Font drawFontRegular = new Font("Cascadia Code",
                    10);
                Font drawFontUpperBold = new Font("Cascadia Code",
                    16,
                    System.Drawing.FontStyle.Bold);
                Font drawFontBold = new Font("Cascadia Code",
                    8,
                    System.Drawing.FontStyle.Bold);

                SolidBrush drawBrush = new SolidBrush(System.Drawing.Color.Black);

                string[] splitForPrint = _journal.Split("\r\n");

                float x = 0;
                float y = 0;
                float width = 0; // max width I found through trial and error
                float height = 0F;

                bool nextIsZelja = false;

                for (int i = 0; i < splitForPrint.Length; i++)
                {
                    if (i == 0)
                    {
                        graphics.DrawString(splitForPrint[i], drawFontUpperBold, drawBrush, x, y);
                        y += graphics.MeasureString(splitForPrint[i], drawFontUpperBold).Height;
                    }
                    else
                    {
                        if (splitForPrint[i].Contains("nemanjaCarina"))
                        {
                            if (!splitForPrint[i + 1].Contains("-------------------------"))
                            {
                                nextIsZelja = true;
                            }
                            var rowSplit = splitForPrint[i].Split("nemanjaCarina");

                            graphics.DrawString(rowSplit[1], drawFontRegular, drawBrush, x, y);
                            y += graphics.MeasureString(rowSplit[1], drawFontRegular).Height;
                        }
                        else
                        {
                            if (nextIsZelja)
                            {
                                graphics.DrawString(splitForPrint[i], drawFontBold, drawBrush, x, y);
                                y += graphics.MeasureString(splitForPrint[i], drawFontBold).Height;

                                if (splitForPrint[i + 1].Contains("-------------------------"))
                                {
                                    nextIsZelja = false;
                                }
                            }
                            else
                            {
                                graphics.DrawString(splitForPrint[i], drawFontRegular, drawBrush, x, y);
                                y += graphics.MeasureString(splitForPrint[i], drawFontRegular).Height;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private static string CreatePorudzbina(PorudzbinaPrint porudzbinaPrint)
        {
            _totalAmount = 0;
            string oredr = SplitInParts($"-{porudzbinaPrint.Sto}", $"nar. {porudzbinaPrint.PorudzbinaNumber}", 20);
            //oredr += "============================\r\n";
            //oredr += SplitInParts($"{porudzbinaPrint.Sto}", "Sto:", 33);
            oredr += SplitInParts($"{porudzbinaPrint.PorudzbinaDateTime}", "Vreme:", 33);
            oredr += SplitInParts($"{porudzbinaPrint.Worker}", "Radnik:", 33);
            //oredr += SplitInParts($"{order.PartHall}", "Део сале:", 28);
            oredr += " \r\n";
            oredr += string.Format("{0}{1}\r\n", "Naziv".PadRight(27), "Kolic.");
            oredr += "=================================\r\n";
            //oredr += "---------------------------------\r\n";

            foreach (var item in porudzbinaPrint.Items)
            {
                string name;
                if (item.Name.Length > 19)
                {
                    name = item.Name.Substring(0, 19);
                }
                else
                {
                    name = item.Name.Substring(0, item.Name.Length);
                }
                string kol = item.Quantity.ToString("0.00");
                string mpc = item.Price.ToString("0.00");

                oredr += string.Format("nemanjaCarina{0}{1}\r\n", name.PadRight(27), kol.PadLeft(6));

                if (!string.IsNullOrEmpty(item.Zelje))
                {
                    string zelje = SplitInParts("", $"{item.Zelje}", 41);
                    oredr += $"{zelje}";
                }

                _totalAmount += item.Quantity * item.Price;

                oredr += "---------------------------------\r\n";
            }
            //oredr += $"{_totalAmount.ToString("0.00").PadLeft(33)}\r\n";
            //oredr += "=================================\r\n";
            oredr += " \r\n";
            oredr += " \r\n";

            oredr += CenterString($"{porudzbinaPrint.PorudzbinaDateTime}", 33);

            return oredr;
        }


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
