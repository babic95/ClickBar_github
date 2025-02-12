using ClickBar.ViewModels.AppMain;
using ClickBar_DatabaseSQLManager;
using ClickBar_InputOutputExcelFiles;
using ClickBar_Printer;
using ClickBar_Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows;
using System.IO;

namespace ClickBar.Commands.AppMain.Report
{
    public class ExportReportCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ReportViewModel _currentViewModel;

        public ExportReportCommand(ReportViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            FolderBrowserDialog openFileDlg = new FolderBrowserDialog();
            var result = openFileDlg.ShowDialog();
            if (result.ToString() == string.Empty)
            {
                System.Windows.MessageBox.Show("Morate da izaberete direktorijum!", "Greška!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string pathToReportFile = Path.Combine(openFileDlg.SelectedPath, $"Izveštaj_{_currentViewModel.StartReport.ToString("dd-MM-yyyy_HH-mm-ss")} - {_currentViewModel.EndReport.ToString("dd-MM-yyyy_HH-mm-ss")}.xlsx");

            Dictionary<string, List<ClickBar_InputOutputExcelFiles.Models.Report>> items = new Dictionary<string, List<ClickBar_InputOutputExcelFiles.Models.Report>>() 
            {
                { "UKUPNO", new List<ClickBar_InputOutputExcelFiles.Models.Report>() }
            };

            _currentViewModel.CurrentReport.ReportItems.ToList().ForEach(group =>
            {
                if (!items.ContainsKey(group.Key))
                {
                    items.Add(group.Key, new List<ClickBar_InputOutputExcelFiles.Models.Report>());
                }

                group.Value.ToList().ForEach(item =>
                {
                    ClickBar_InputOutputExcelFiles.Models.Report report = new ClickBar_InputOutputExcelFiles.Models.Report()
                    {
                        Količina = item.Value.Quantity,
                        Ukupno = item.Value.Gross,
                        Naziv = item.Value.Name,
                        Šifra = item.Key,
                        Grupa = group.Key
                    };

                    items["UKUPNO"].Add(report);
                    items[group.Key].Add(report);
                });
            });

            bool succes = await InputOutputExcelFilesManager.Instance.ExportReport(pathToReportFile, items);

            if (succes)
            {
                System.Windows.MessageBox.Show("Uspešno ste izvezli izveštaj na izabranu lokaciju!", "Uspešan izvoz izveštaja!", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                System.Windows.MessageBox.Show("Greška prilikom izvoza izveštaja!", "Greška!", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}