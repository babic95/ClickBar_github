using ClickBar.ViewModels.AppMain.Statistic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using ClickBar_Printer;
using ClickBar_Report.Models;
using DocumentFormat.OpenXml.Wordprocessing;

namespace ClickBar.Commands.AppMain.Statistic.ViewCalculation
{
    public class PrintSaldo1010Command : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ViewCalculationViewModel _currentViewModel;

        public PrintSaldo1010Command(ViewCalculationViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (_currentViewModel.Calculations != null &&
                _currentViewModel.Calculations.Any() &&
                _currentViewModel.SearchFromCalculationDate.HasValue &&
                 _currentViewModel.SearchToCalculationDate.HasValue)
            {
                List<ItemKEP> kep = new List<ItemKEP>();

                decimal saldo = 0;
                int year = DateTime.Now.Year;
                _currentViewModel.Calculations.ToList().ForEach(cal =>
                {
                    saldo += cal.InputTotalPrice;
                    string invoiceNumber = !string.IsNullOrEmpty(cal.InvoiceNumber) ? $" - {cal.InvoiceNumber}" : string.Empty;
                    ItemKEP itemKEP = new ItemKEP()
                    {
                        Id = cal.Id,
                        Description = $"{cal.Name}{invoiceNumber}",
                        KepDate = cal.CalculationDate,
                        Razduzenje = 0,
                        Zaduzenje = cal.InputTotalPrice,//dugovanje,
                        Saldo = saldo,
                    };
                    kep.Add(itemKEP);
                });

                PrinterManager.Instance.Print1010(_currentViewModel.DbContext,
                    _currentViewModel.SearchFromCalculationDate.Value,
                    _currentViewModel.SearchToCalculationDate.Value,
                    kep);
            }
        }
    }
}