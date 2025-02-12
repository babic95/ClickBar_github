using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Windows;
using System.Windows.Input;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Drawing;
using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar_Printer.Models.DPU;
using ClickBar_DatabaseSQLManager;
using ClickBar_Printer;
using ClickBar_Logging;
using ClickBar_Report.Models;
using ClickBar.Models.AppMain.Statistic;
using ClickBar_DatabaseSQLManager.Models;
using System.Linq;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace ClickBar.Commands.AppMain.Statistic.LagerLista
{
    public class PrintLagerListaCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private LagerListaViewModel _currentViewModel;

        public PrintLagerListaCommand(LagerListaViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            try
            {
                List<DPU_Print> podaciPice = new List<DPU_Print>();
                List<DPU_Print> podaciKuhinja = new List<DPU_Print>();

                SupergroupDB? supergroupKuhinjaDB = null;
                SupergroupDB? supergroupSankDB = null;
                PocetnoStanjeDB? pocetnoStanjeDB = null;

                var pocetnaStanja = _currentViewModel.DbContext.PocetnaStanja.Where(p => p.PopisDate.Date <= _currentViewModel.SelectedDate.Date);


                if (pocetnaStanja != null &&
                    pocetnaStanja.Any())
                {
                    pocetnoStanjeDB = pocetnaStanja.OrderByDescending(p => p.PopisDate).FirstOrDefault();
                }

                _currentViewModel.DbContext.Supergroups.ForEachAsync(s =>
                {
                    var ss = s.Name.ToLower();

                    if (s.Name.ToLower().Equals("piće") ||
                    s.Name.ToLower().Equals("pice") ||
                    s.Name.ToLower().Equals("šank") ||
                    s.Name.ToLower().Equals("sank"))
                    {
                        supergroupSankDB = s;
                    }
                    else if (s.Name.ToLower().Equals("kuhinja") ||
                        s.Name.ToLower().Equals("hrana"))
                    {
                        supergroupKuhinjaDB = s;
                    }
                });
                if (supergroupKuhinjaDB != null)
                {
                    podaciKuhinja = GetKuhinja(supergroupKuhinjaDB,
                        pocetnoStanjeDB,
                        _currentViewModel.SelectedDate).Result;
                }

                if (supergroupSankDB != null)
                {
                    podaciPice = GetSank(supergroupSankDB,
                        pocetnoStanjeDB,
                        _currentViewModel.SelectedDate).Result;
                }

                List<DPU_Print> podaci = new List<DPU_Print>();
                int rb = 1;
                podaciPice.Concat(podaciKuhinja).ToList().ForEach(podatak =>
                {
                    podatak.RedniBroj = rb++;
                    podaci.Add(podatak);
                });
                // Generišite PDF tok koristeći metodu iz Class Library
                MemoryStream pdfStream = PrinterManager.Instance.PrintDPU(podaci);

                // Pokaži dijalog za izbor štampača
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    string printerName = printDialog.PrintQueue.FullName;
                    PdfDocument pdfDocument = PdfReader.Open(pdfStream, PdfDocumentOpenMode.Import);

                    PrintDocument printDocument = new PrintDocument();
                    printDocument.PrinterSettings.PrinterName = printerName;
                    printDocument.PrintPage += (s, ev) =>
                    {
                        using (MemoryStream tempStream = new MemoryStream())
                        {
                            // Konvertujte prvu stranicu PDF-a u sliku
                            PdfPage pdfPage = pdfDocument.Pages[0];
                            XGraphics gfx = XGraphics.FromPdfPage(pdfPage);
                            XImage img = XImage.FromStream(tempStream);

                            gfx.DrawImage(img, 0, 0, pdfPage.Width, pdfPage.Height);
                            tempStream.Position = 0;

                            // Kreirajte Graphics objekt iz PrintPageEventArgs
                            System.Drawing.Image drawingImage = System.Drawing.Image.FromStream(tempStream);
                            ev.Graphics.DrawImage(drawingImage, ev.PageBounds);
                        }
                    };
                    printDocument.Print();
                }
            }
            catch (Exception ex)
            {
                Log.Error("PrintLagerListaCommand - Greska prilikom stampe Lager Liste na dan -> ", ex);
            }
        }
        private async Task<List<DPU_Print>> GetKuhinja(SupergroupDB supergroupDB,
            PocetnoStanjeDB? pocetnoStanjeDB,
            DateTime toDate)
        {
            List<DPU_Print> kuhinja = new List<DPU_Print>();
            var allInvoicesInPeriod = _currentViewModel.DbContext.Invoices.Where(invoice => invoice.SdcDateTime != null &&
            invoice.SdcDateTime.HasValue &&
            invoice.SdcDateTime.Value.Date <= toDate.Date &&
            invoice.SdcDateTime.Value.Date >= (pocetnoStanjeDB != null ? pocetnoStanjeDB.PopisDate.Date :
            new DateTime(DateTime.Now.Year, 1, 1)));

            var itemsInKuhinja = _currentViewModel.DbContext.Items.Join(_currentViewModel.DbContext.ItemGroups,
                item => item.IdItemGroup,
                group => group.Id,
                (item, group) => new { Item = item, Group = group })
                .Join(_currentViewModel.DbContext.Supergroups,
                item => item.Group.IdSupergroup,
                supergroup => supergroup.Id,
                (item, supergroup) => new { Item = item, Supergroup = supergroup })
                .Where(i => i.Supergroup.Id == supergroupDB.Id && !i.Item.Item.Name.ToLower().Contains("sirovina"));

            if (itemsInKuhinja.Any())
            {
                await itemsInKuhinja.ForEachAsync(itemDB =>
                {

                });
            }

            await _currentViewModel.DbContext.Items.ForEachAsync(itemDB =>
            {
                PocetnoStanjeItemDB? pocetnoStanjeItemDB = pocetnoStanjeDB != null ? _currentViewModel.DbContext.PocetnaStanjaItems.FirstOrDefault(pi =>
                pi.IdPocetnoStanje == pocetnoStanjeDB.Id && pi.IdItem == itemDB.Id) : null;

                DPU_Print dPU_Print = new DPU_Print()
                {
                    RedniBroj = 0,
                    Naziv = $"{itemDB.Id} - {itemDB.Name}",
                    JedinicaMere = itemDB.Jm,
                    PrenetaKolicina = pocetnoStanjeItemDB == null ? 0 : pocetnoStanjeItemDB.NewQuantity,
                    NabavljenaKolicina = 0,
                    UtrosenaKolicina = 0,
                    ProdajnaCena = 0,
                    ProdajnaVrednost = 0,
                    PDV = 0,
                    PrometOdJela = 0,
                    PrometOdUsluga = 0,
                    Ukupno = 0,
                    ZaliheNaKrajuDana = 0,
                };


            });
            return kuhinja;
        }
        private async Task<List<DPU_Print>> GetSank(SupergroupDB supergroupDB,
            PocetnoStanjeDB? pocetnoStanjeDB,
            DateTime toDate)
        {
            List<DPU_Print> sank = new List<DPU_Print>();
            
            return sank;
        }
    }
}