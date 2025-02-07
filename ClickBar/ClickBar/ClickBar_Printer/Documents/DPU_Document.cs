using ClickBar_Printer.Models.DPU;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ClickBar_Printer.Documents
{
    public static class DPU_Document
    {
        public static MemoryStream PrintDPU(List<DPU_Print> podaci)
        {
            PdfDocument document = new PdfDocument();
            PdfPage page = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);
            XFont font = new XFont("Arial", 10);
            double yPoint = 60;

            // Naslovi kolona
            gfx.DrawString("Redni broj", font, XBrushes.Black, new XRect(40, yPoint, 50, 20));
            gfx.DrawString("Naziv jela i pića za konzumaciju na licu mesta", font, XBrushes.Black, new XRect(90, yPoint, 200, 20));
            gfx.DrawString("Stopa PDV", font, XBrushes.Black, new XRect(300, yPoint, 60, 20));
            gfx.DrawString("Jedinica mere", font, XBrushes.Black, new XRect(370, yPoint, 80, 20));
            gfx.DrawString("Preneta količina", font, XBrushes.Black, new XRect(450, yPoint, 100, 20));
            gfx.DrawString("Nabavljena količina", font, XBrushes.Black, new XRect(560, yPoint, 120, 20));
            gfx.DrawString("Ukupno", font, XBrushes.Black, new XRect(680, yPoint, 60, 20));
            gfx.DrawString("Zalihe na kraju dana", font, XBrushes.Black, new XRect(750, yPoint, 140, 20));
            gfx.DrawString("Utrošena količina u toku dana", font, XBrushes.Black, new XRect(900, yPoint, 180, 20));
            gfx.DrawString("Prodajna cena po jedinici mere sa PDV", font, XBrushes.Black, new XRect(1090, yPoint, 220, 20));
            gfx.DrawString("Ostvareni promet od usluga", font, XBrushes.Black, new XRect(1310, yPoint, 180, 20));
            gfx.DrawString("Ostvareni promet od jela", font, XBrushes.Black, new XRect(1500, yPoint, 160, 20));
            gfx.DrawString("Prodajna vrednost jela i pića za konzumaciju na licu mesta", font, XBrushes.Black, new XRect(1680, yPoint, 220, 20));

            yPoint += 20;

            foreach (var podatak in podaci)
            {
                if (yPoint + 40 > page.Height)
                {
                    page = document.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    yPoint = 60;

                    // Naslovi kolona na novoj stranici
                    gfx.DrawString("Redni broj", font, XBrushes.Black, new XRect(40, yPoint, 50, 20));
                    gfx.DrawString("Naziv jela i pića za konzumaciju na licu mesta", font, XBrushes.Black, new XRect(90, yPoint, 200, 20));
                    gfx.DrawString("Stopa PDV", font, XBrushes.Black, new XRect(300, yPoint, 60, 20));
                    gfx.DrawString("Jedinica mere", font, XBrushes.Black, new XRect(370, yPoint, 80, 20));
                    gfx.DrawString("Preneta količina", font, XBrushes.Black, new XRect(450, yPoint, 100, 20));
                    gfx.DrawString("Nabavljena količina", font, XBrushes.Black, new XRect(560, yPoint, 120, 20));
                    gfx.DrawString("Ukupno", font, XBrushes.Black, new XRect(680, yPoint, 60, 20));
                    gfx.DrawString("Zalihe na kraju dana", font, XBrushes.Black, new XRect(750, yPoint, 140, 20));
                    gfx.DrawString("Utrošena količina u toku dana", font, XBrushes.Black, new XRect(900, yPoint, 180, 20));
                    gfx.DrawString("Prodajna cena po jedinici mere sa PDV", font, XBrushes.Black, new XRect(1090, yPoint, 220, 20));
                    gfx.DrawString("Ostvareni promet od usluga", font, XBrushes.Black, new XRect(1310, yPoint, 180, 20));
                    gfx.DrawString("Ostvareni promet od jela", font, XBrushes.Black, new XRect(1500, yPoint, 160, 20));
                    gfx.DrawString("Prodajna vrednost jela i pića za konzumaciju na licu mesta", font, XBrushes.Black, new XRect(1680, yPoint, 220, 20));

                    yPoint += 20;
                }

                gfx.DrawString(podatak.RedniBroj.ToString(), font, XBrushes.Black, new XRect(40, yPoint, 50, 20));
                gfx.DrawString(podatak.Naziv, font, XBrushes.Black, new XRect(90, yPoint, 200, 20));
                gfx.DrawString(podatak.PDV.ToString(), font, XBrushes.Black, new XRect(300, yPoint, 60, 20));
                gfx.DrawString(podatak.JedinicaMere, font, XBrushes.Black, new XRect(370, yPoint, 80, 20));
                gfx.DrawString(podatak.PrenetaKolicina.ToString(), font, XBrushes.Black, new XRect(450, yPoint, 100, 20));
                gfx.DrawString(podatak.NabavljenaKolicina.ToString(), font, XBrushes.Black, new XRect(560, yPoint, 120, 20));
                gfx.DrawString(podatak.Ukupno.ToString(), font, XBrushes.Black, new XRect(680, yPoint, 60, 20));
                gfx.DrawString(podatak.ZaliheNaKrajuDana.ToString(), font, XBrushes.Black, new XRect(750, yPoint, 140, 20));
                gfx.DrawString(podatak.UtrosenaKolicina.ToString(), font, XBrushes.Black, new XRect(900, yPoint, 180, 20));
                gfx.DrawString(podatak.ProdajnaCena.ToString(), font, XBrushes.Black, new XRect(1090, yPoint, 220, 20));
                gfx.DrawString(podatak.PrometOdUsluga.ToString(), font, XBrushes.Black, new XRect(1310, yPoint, 180, 20));
                gfx.DrawString(podatak.PrometOdJela.ToString(), font, XBrushes.Black, new XRect(1500, yPoint, 160, 20));
                gfx.DrawString(podatak.ProdajnaVrednost.ToString(), font, XBrushes.Black, new XRect(1680, yPoint, 220, 20));

                yPoint += 20;
            }

            MemoryStream stream = new MemoryStream();
            document.Save(stream, false);
            stream.Position = 0;

            return stream;
        }
    }
}