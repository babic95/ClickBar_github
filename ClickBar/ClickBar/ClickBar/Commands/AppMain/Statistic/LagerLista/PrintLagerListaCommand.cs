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
using ClickBar_Database;
using ClickBar_Printer;
using ClickBar_Logging;
using ClickBar_Report.Models;
using ClickBar.Models.AppMain.Statistic;
using ClickBar_Database.Models;
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

                using (SqliteDbContext sqliteDbContext = new SqliteDbContext())
                {
                    var pocetnaStanja = sqliteDbContext.PocetnaStanja.Where(p => p.PopisDate.Date <= _currentViewModel.SelectedDate.Date);

                    PocetnoStanjeDB? pocetnoStanjeDB = null;

                    if(pocetnaStanja != null &&
                        pocetnaStanja.Any())
                    {
                        pocetnoStanjeDB = pocetnaStanja.OrderByDescending(p => p.PopisDate).FirstOrDefault();
                    }

                    SupergroupDB? supergroupKuhinjaDB = null; 
                    SupergroupDB? supergroupSankDB = null; 

                    sqliteDbContext.Supergroups.ForEachAsync(s =>
                    {
                        var ss = s.Name.ToLower();

                        if (s.Name.ToLower().Equals("piće") ||
                        s.Name.ToLower().Equals("pice") ||
                        s.Name.ToLower().Equals("šank") ||
                        s.Name.ToLower().Equals("sank"))
                        {
                            supergroupSankDB = s;
                        }
                        else if(s.Name.ToLower().Equals("kuhinja") ||
                            s.Name.ToLower().Equals("hrana"))
                        {
                            supergroupKuhinjaDB = s;
                        }
                    });

                    if(supergroupKuhinjaDB != null)
                    {
                        podaciKuhinja = GetKuhinja(supergroupKuhinjaDB, pocetnoStanjeDB).Result;
                    }

                    if(supergroupSankDB != null)
                    {
                        podaciPice = GetSank(supergroupSankDB, pocetnoStanjeDB).Result;
                    }

                    int rb = 1;

                    sqliteDbContext.Items.ForEachAsync(itemDB =>
                    {
                        PocetnoStanjeItemDB? pocetnoStanjeItemDB = pocetnoStanjeDB != null ? sqliteDbContext.PocetnaStanjaItems.FirstOrDefault(pi => 
                        pi.IdPocetnoStanje == pocetnoStanjeDB.Id && pi.IdItem == itemDB.Id) : null;

                        DPU_Print dPU_Print = new DPU_Print()
                        {
                            RedniBroj = rb++,
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
                }

                List<DPU_Print> podaci = podaciPice.Concat(podaciKuhinja).ToList();
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

                //using (SqliteDbContext sqliteDbContext = new SqliteDbContext())
                //{
                //    Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItems10PDV = new Dictionary<string, Dictionary<string, List<ReportPerItems>>>();
                //    Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItems20PDV = new Dictionary<string, Dictionary<string, List<ReportPerItems>>>();
                //    Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItems0PDV = new Dictionary<string, Dictionary<string, List<ReportPerItems>>>();
                //    Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsNoPDV = new Dictionary<string, Dictionary<string, List<ReportPerItems>>>();

                //    Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsSirovina10PDV = new Dictionary<string, Dictionary<string, List<ReportPerItems>>>();
                //    Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsSirovina20PDV = new Dictionary<string, Dictionary<string, List<ReportPerItems>>>();
                //    Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsSirovina0PDV = new Dictionary<string, Dictionary<string, List<ReportPerItems>>>();
                //    Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsSirovinaNoPDV = new Dictionary<string, Dictionary<string, List<ReportPerItems>>>();
                //    _currentViewModel.Items.ForEach(item =>
                //    {
                //        if (item.Quantity != 0)
                //        {
                //            var itemGroupDB = sqliteDbContext.ItemGroups.Find(item.IdGroupItems);

                //            if (itemGroupDB != null)
                //            {
                //                switch (item.Item.Label)
                //                {
                //                    case "Ђ":
                //                        if (!item.IsSirovina)
                //                        {
                //                            SetItemsPDV(allItems20PDV, item, itemGroupDB);
                //                        }
                //                        else
                //                        {
                //                            SetSirovinaItemsPDV(allItems20PDV, allItemsSirovina20PDV, item, itemGroupDB);
                //                        }
                //                        break;
                //                    case "6":
                //                        if (!item.IsSirovina)
                //                        {
                //                            SetItemsPDV(allItems20PDV, item, itemGroupDB);
                //                        }
                //                        else
                //                        {
                //                            SetSirovinaItemsPDV(allItems20PDV, allItemsSirovina20PDV, item, itemGroupDB);
                //                        }
                //                        break;
                //                    case "Е":
                //                        if (!item.IsSirovina)
                //                        {
                //                            SetItemsPDV(allItems10PDV, item, itemGroupDB);
                //                        }
                //                        else
                //                        {
                //                            SetSirovinaItemsPDV(allItems10PDV, allItemsSirovina10PDV, item, itemGroupDB);
                //                        }
                //                        break;
                //                    case "7":
                //                        if (!item.IsSirovina)
                //                        {
                //                            SetItemsPDV(allItems10PDV, item, itemGroupDB);
                //                        }
                //                        else
                //                        {
                //                            SetSirovinaItemsPDV(allItems10PDV, allItemsSirovina10PDV, item, itemGroupDB);
                //                        }
                //                        break;
                //                    case "Г":
                //                        if (!item.IsSirovina)
                //                        {
                //                            SetItemsPDV(allItems0PDV, item, itemGroupDB);
                //                        }
                //                        else
                //                        {
                //                            SetSirovinaItemsPDV(allItems0PDV, allItemsSirovina0PDV, item, itemGroupDB);
                //                        }
                //                        break;
                //                    case "4":
                //                        if (!item.IsSirovina)
                //                        {
                //                            SetItemsPDV(allItems0PDV, item, itemGroupDB);
                //                        }
                //                        else
                //                        {
                //                            SetSirovinaItemsPDV(allItems0PDV, allItemsSirovina0PDV, item, itemGroupDB);
                //                        }
                //                        break;
                //                    case "А":
                //                        if (!item.IsSirovina)
                //                        {
                //                            SetItemsPDV(allItemsNoPDV, item, itemGroupDB);
                //                        }
                //                        else
                //                        {
                //                            SetSirovinaItemsPDV(allItemsNoPDV, allItemsSirovinaNoPDV, item, itemGroupDB);
                //                        }
                //                        break;
                //                    case "1":
                //                        if (!item.IsSirovina)
                //                        {
                //                            SetItemsPDV(allItemsNoPDV, item, itemGroupDB);
                //                        }
                //                        else
                //                        {
                //                            SetSirovinaItemsPDV(allItemsNoPDV, allItemsSirovinaNoPDV, item, itemGroupDB);
                //                        }
                //                        break;
                //                    case "Ж":
                //                        if (!item.IsSirovina)
                //                        {
                //                            SetItemsPDV(allItems20PDV, item, itemGroupDB);
                //                        }
                //                        else
                //                        {
                //                            SetSirovinaItemsPDV(allItems20PDV, allItemsSirovina20PDV, item, itemGroupDB);
                //                        }
                //                        break;
                //                    case "8":
                //                        if (!item.IsSirovina)
                //                        {
                //                            SetItemsPDV(allItems20PDV, item, itemGroupDB);
                //                        }
                //                        else
                //                        {
                //                            SetSirovinaItemsPDV(allItems20PDV, allItemsSirovina20PDV, item, itemGroupDB);
                //                        }
                //                        break;
                //                    case "A":
                //                        if (!item.IsSirovina)
                //                        {
                //                            SetItemsPDV(allItems10PDV, item, itemGroupDB);
                //                        }
                //                        else
                //                        {
                //                            SetSirovinaItemsPDV(allItems10PDV, allItemsSirovina10PDV, item, itemGroupDB);
                //                        }
                //                        break;
                //                    case "31":
                //                        if (!item.IsSirovina)
                //                        {
                //                            SetItemsPDV(allItems10PDV, item, itemGroupDB);
                //                        }
                //                        else
                //                        {
                //                            SetSirovinaItemsPDV(allItems10PDV, allItemsSirovina10PDV, item, itemGroupDB);
                //                        }
                //                        break;
                //                    case "N":
                //                        if (!item.IsSirovina)
                //                        {
                //                            SetItemsPDV(allItemsNoPDV, item, itemGroupDB);
                //                        }
                //                        else
                //                        {
                //                            SetSirovinaItemsPDV(allItemsNoPDV, allItemsSirovinaNoPDV, item, itemGroupDB);
                //                        }
                //                        break;
                //                    case "47":
                //                        if (!item.IsSirovina)
                //                        {
                //                            SetItemsPDV(allItemsNoPDV, item, itemGroupDB);
                //                        }
                //                        else
                //                        {
                //                            SetSirovinaItemsPDV(allItemsNoPDV, allItemsSirovinaNoPDV, item, itemGroupDB);
                //                        }
                //                        break;
                //                    case "P":
                //                        if (!item.IsSirovina)
                //                        {
                //                            SetItemsPDV(allItems0PDV, item, itemGroupDB);
                //                        }
                //                        else
                //                        {
                //                            SetSirovinaItemsPDV(allItems0PDV, allItemsSirovina0PDV, item, itemGroupDB);
                //                        }
                //                        break;
                //                    case "49":
                //                        if (!item.IsSirovina)
                //                        {
                //                            SetItemsPDV(allItems0PDV, item, itemGroupDB);
                //                        }
                //                        else
                //                        {
                //                            SetSirovinaItemsPDV(allItems0PDV, allItemsSirovina0PDV, item, itemGroupDB);
                //                        }
                //                        break;
                //                }
                //            }
                //        }
                //    });

                //    PrinterManager.Instance.LagerListaNaDan(_currentViewModel.SelectedDate,
                //        allItems20PDV,
                //        allItems10PDV,
                //        allItems0PDV,
                //        allItemsNoPDV,
                //        allItemsSirovina20PDV,
                //        allItemsSirovina10PDV,
                //        allItemsSirovina0PDV,
                //        allItemsSirovinaNoPDV);
                //}
            }
            catch (Exception ex)
            {
                Log.Error("PrintLagerListaCommand - Greska prilikom stampe Lager Liste na dan -> ", ex);
            }
        }
        private async Task<List<DPU_Print>> GetKuhinja(SupergroupDB supergroupDB,
            PocetnoStanjeDB? pocetnoStanjeDB)
        {
            List<DPU_Print> kuhinja = new List<DPU_Print>();
            using (SqliteDbContext sqliteDbContext = new SqliteDbContext())
            {

            }
            return kuhinja;
        }
        private async Task<List<DPU_Print>> GetSank(SupergroupDB supergroupDB,
            PocetnoStanjeDB? pocetnoStanjeDB)
        {
            List<DPU_Print> sank = new List<DPU_Print>();
            using (SqliteDbContext sqliteDbContext = new SqliteDbContext())
            {

            }
            return sank;
        }
        private void SetItemsPDV(
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsPDV,
            Invertory item,
            ItemGroupDB itemGroupDB)
        {
            if (!allItemsPDV.ContainsKey(itemGroupDB.Name))
            {
                allItemsPDV.Add(itemGroupDB.Name, new Dictionary<string, List<ReportPerItems>>());
            }

            if (allItemsPDV[itemGroupDB.Name].ContainsKey(item.Item.Id))
            {
                var it = allItemsPDV[itemGroupDB.Name][item.Item.Id].FirstOrDefault(i => i.MPC_Original == item.Item.OriginalUnitPrice);

                if (it != null)
                {
                    it.Quantity += item.Quantity;

                    if (item.Item.InputUnitPrice != null &&
                    item.Item.InputUnitPrice.HasValue)
                    {
                        it.TotalAmount += Decimal.Round(item.Item.InputUnitPrice.Value * item.Quantity, 2);
                        it.MPC_Average = item.Item.InputUnitPrice.Value;
                    }
                }
                else
                {
                    ReportPerItems reportPerItems = new ReportPerItems()
                    {
                        ItemId = item.Item.Id,
                        JM = item.Item.Jm,
                        Name = item.Item.Name,
                        Quantity = item.Quantity,
                        //TotalAmount = isRefund ? -1 * item.TotalAmout : item.TotalAmout,
                        //MPC_Average = item.Item.SellingUnitPrice,
                        MPC_Original = item.Item.OriginalUnitPrice,
                        IsSirovina = item.IsSirovina,
                    };

                    if (item.Item.InputUnitPrice != null &&
                            item.Item.InputUnitPrice.HasValue)
                    {
                        reportPerItems.TotalAmount = Decimal.Round(item.Item.InputUnitPrice.Value * item.Quantity, 2);
                        reportPerItems.MPC_Average = item.Item.InputUnitPrice.Value;
                    }

                    //decimal niv = -1 * Decimal.Round((item.Item.OriginalUnitPrice * item.Quantity) - item.TotalAmout, 2);

                    //reportPerItems.Nivelacija = niv > -1 && niv < 1 ? 0 : niv;
                    allItemsPDV[itemGroupDB.Name][item.Item.Id].Add(reportPerItems);
                }
            }
            else
            {
                ReportPerItems reportPerItems = new ReportPerItems()
                {
                    ItemId = item.Item.Id,
                    JM = item.Item.Jm,
                    Name = item.Item.Name,
                    Quantity = item.Quantity,
                    //TotalAmount = isRefund ? -1 * item.TotalAmout : item.TotalAmout,
                    //MPC_Average = item.Item.SellingUnitPrice,
                    MPC_Original = item.Item.OriginalUnitPrice,
                    IsSirovina = item.IsSirovina,
                };

                if (item.Item.InputUnitPrice != null &&
                        item.Item.InputUnitPrice.HasValue)
                {
                    reportPerItems.TotalAmount = Decimal.Round(item.Item.InputUnitPrice.Value * item.Quantity, 2);
                    reportPerItems.MPC_Average = item.Item.InputUnitPrice.Value;
                }

                //decimal niv = -1 * Decimal.Round((item.Item.OriginalUnitPrice * item.Quantity) - item.TotalAmout, 2);

                //reportPerItems.Nivelacija = niv > -1 && niv < 1 ? 0 : niv;

                allItemsPDV[itemGroupDB.Name].Add(item.Item.Id, new List<ReportPerItems>() { reportPerItems });
            }
        }
        private void SetSirovinaItemsPDV(
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allItemsPDV,
            Dictionary<string, Dictionary<string, List<ReportPerItems>>> allSirovinaItemsPDV,
            Invertory item,
            ItemGroupDB itemGroupDB)
        {
            if(itemGroupDB.Name.ToLower().Contains("sirovina") ||
                itemGroupDB.Name.ToLower().Contains("sirovine"))
            {
                if (!allSirovinaItemsPDV.ContainsKey(itemGroupDB.Name))
                {
                    allSirovinaItemsPDV.Add(itemGroupDB.Name, new Dictionary<string, List<ReportPerItems>>());
                }
            }
            else
            {
                if (!allItemsPDV.ContainsKey(itemGroupDB.Name))
                {
                    allItemsPDV.Add(itemGroupDB.Name, new Dictionary<string, List<ReportPerItems>>());
                }
            }

            if (allItemsPDV[itemGroupDB.Name].ContainsKey(item.Item.Id))
            {
                var it = allItemsPDV[itemGroupDB.Name][item.Item.Id].FirstOrDefault();

                if (it != null)
                {
                    it.Quantity += item.Quantity;

                    if (item.Item.InputUnitPrice != null &&
                        item.Item.InputUnitPrice.HasValue)
                    {
                        it.TotalAmount += Decimal.Round(item.Item.InputUnitPrice.Value * item.Quantity, 2);
                        it.MPC_Average = item.Item.InputUnitPrice.Value;
                    }
                }
            }
            else if (allSirovinaItemsPDV[itemGroupDB.Name].ContainsKey(item.Item.Id))
            {
                var it = allSirovinaItemsPDV[itemGroupDB.Name][item.Item.Id].FirstOrDefault();

                if (it != null)
                {
                    it.Quantity += item.Quantity;

                    if (item.Item.InputUnitPrice != null &&
                        item.Item.InputUnitPrice.HasValue)
                    {
                        it.TotalAmount += Decimal.Round(item.Item.InputUnitPrice.Value * item.Quantity, 2);
                        it.MPC_Average = item.Item.InputUnitPrice.Value;
                    }
                }
            }
            else
            {
                ReportPerItems reportPerItems = new ReportPerItems()
                {
                    ItemId = item.Item.Id,
                    JM = item.Item.Jm,
                    Name = item.Item.Name,
                    Quantity = item.Quantity,
                    //TotalAmount = isRefund ? -1 * item.TotalAmout : item.TotalAmout,
                    //MPC_Average = item.Item.SellingUnitPrice,
                    MPC_Original = item.Item.OriginalUnitPrice,
                    IsSirovina = item.IsSirovina,
                };

                if (item.Item.InputUnitPrice != null &&
                        item.Item.InputUnitPrice.HasValue)
                {
                    reportPerItems.TotalAmount = Decimal.Round(item.Item.InputUnitPrice.Value * item.Quantity, 2);
                    reportPerItems.MPC_Average = item.Item.InputUnitPrice.Value;
                }

                allSirovinaItemsPDV[itemGroupDB.Name].Add(item.Item.Id, new List<ReportPerItems>() { reportPerItems });
            }
        }
    }
}