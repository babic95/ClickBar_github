using ClickBar.Models.AppMain.Statistic;
using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar_Common.Models.Statistic;
using ClickBar_DatabaseSQLManager;
using ClickBar_DatabaseSQLManager.Models;
using ClickBar_Printer;
using ClickBar_Printer.PaperFormat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ClickBar.Commands.AppMain.Statistic.ViewCalculation
{
    public class PrintCalculationCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ViewCalculationViewModel _currentViewModel;

        public PrintCalculationCommand(ViewCalculationViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            if (parameter is string)
            {
                string calculationId = (string)parameter;

                var calculation = _currentViewModel.Calculations.FirstOrDefault(x => x.Id == calculationId);
                var calculationDB = _currentViewModel.DbContext.Calculations.FirstOrDefault(x => x.Id == calculationId);

                if (calculationDB != null)
                {
                    List<InvertoryGlobal> invertoryGlobals = new List<InvertoryGlobal>();

                    var calculationItems = await _currentViewModel.GetAllItemsInCalculation(calculationDB);
                    if (calculationItems != null &&
                        calculationItems.Any())
                    {
                        calculationItems.ToList().ForEach(item =>
                        {
                            var itemDB = _currentViewModel.DbContext.Items.Find(item.Item.Id);

                            if (itemDB != null)
                            {
                                var group = _currentViewModel.DbContext.ItemGroups.Find(itemDB.IdItemGroup);

                                if (group == null)
                                {
                                    MessageBox.Show("ARTIKAL MORA DA PRIPADA NEKOJ GRUPI!",
                                        "Greška",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Error);

                                    return;
                                }
                                decimal inputUnitPrice = Decimal.Round(item.InputPrice / item.Quantity, 2);
                                if (group.Name.ToLower().Contains("sirovine") ||
                                    group.Name.ToLower().Contains("sirovina"))
                                {
                                    invertoryGlobals.Add(new InvertoryGlobal()
                                    {
                                        Id = item.Item.Id,
                                        Name = item.Item.Name,
                                        Jm = item.Item.Jm,
                                        InputUnitPrice = inputUnitPrice,
                                        SellingUnitPrice = 0,
                                        Quantity = item.Quantity,
                                        TotalAmout = Decimal.Round(item.InputPrice, 2)
                                    });
                                }
                                else
                                {
                                    invertoryGlobals.Add(new InvertoryGlobal()
                                    {
                                        Id = item.Item.Id,
                                        Name = item.Item.Name,
                                        Jm = item.Item.Jm,
                                        InputUnitPrice = inputUnitPrice,
                                        SellingUnitPrice = item.Item.SellingUnitPrice,
                                        Quantity = item.Quantity,
                                        TotalAmout = Decimal.Round(item.TotalAmout * item.Quantity, 2)
                                    });
                                }
                            }
                        });
                    }

                    if (calculation != null &&
                        calculation.Supplier != null)
                    {
                        SupplierGlobal supplierGlobal = new SupplierGlobal()
                        {
                            Name = calculation.Supplier.Name,
                            Pib = calculation.Supplier.Pib,
                            Address = calculation.Supplier.Address,
                            City = calculation.Supplier.City,
                            ContractNumber = calculation.Supplier.ContractNumber,
                            Email = calculation.Supplier.Email,
                            Mb = calculation.Supplier.MB,
                            InvoiceNumber = calculation.InvoiceNumber
                        };

                        PrinterManager.Instance.PrintInventoryStatus(invertoryGlobals,
                            $"KALKULACIJA_{calculation.Counter}-{calculation.CalculationDate.Year}",
                            calculation.CalculationDate,
                            false,
                            supplierGlobal);
                    }
                }
            }
        }
    }
}