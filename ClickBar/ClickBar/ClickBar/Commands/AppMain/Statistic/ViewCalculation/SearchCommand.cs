using ClickBar.Models.AppMain.Statistic;
using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar_DatabaseSQLManager;
using ClickBar_DatabaseSQLManager.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ClickBar.Commands.AppMain.Statistic.ViewCalculation
{
    public class SearchCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ViewCalculationViewModel _currentViewModel;

        public SearchCommand(ViewCalculationViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _currentViewModel.TotalInputPrice = 0;
            _currentViewModel.TotalOutputPrice = 0;

            _currentViewModel.CalculationsAll = new ObservableCollection<Models.AppMain.Statistic.Calculation>();

            if (_currentViewModel.SearchFromCalculationDate != null &&
                _currentViewModel.SearchFromCalculationDate.HasValue &&
                _currentViewModel.SearchToCalculationDate != null &&
                _currentViewModel.SearchToCalculationDate.HasValue)
            {
                //calculations = sqliteDbContext.Calculations.Where(cal => cal.CalculationDate.Date >= _currentViewModel.SearchFromCalculationDate.Value.Date &&
                //    cal.CalculationDate.Date <= _currentViewModel.SearchToCalculationDate.Value.Date);

                var calculations = _currentViewModel.DbContext.Calculations.Where(cal => cal.CalculationDate.Date >= _currentViewModel.SearchFromCalculationDate.Value.Date &&
                cal.CalculationDate.Date <= _currentViewModel.SearchToCalculationDate.Value.Date)
                .Join(_currentViewModel.DbContext.Suppliers,
                cal => cal.SupplierId,
                supp => supp.Id,
                (cal, supp) => new { Cal = cal, Supp = supp })
                .Join(_currentViewModel.DbContext.Cashiers,
                cal => cal.Cal.CashierId,
                cash => cash.Id,
                (cal, cash) => new { Cal = cal, Cash = cash });

                if (calculations != null &&
                    calculations.Any())
                {
                    calculations.ForEachAsync(cal =>
                    {
                        Models.AppMain.Statistic.Calculation calculation = new Models.AppMain.Statistic.Calculation()
                        {
                            Id = cal.Cal.Cal.Id,
                            CalculationDate = cal.Cal.Cal.CalculationDate,
                            InputTotalPrice = cal.Cal.Cal.InputTotalPrice,
                            InvoiceNumber = cal.Cal.Cal.InvoiceNumber,
                            OutputTotalPrice = cal.Cal.Cal.OutputTotalPrice,
                            Counter = cal.Cal.Cal.Counter,
                            Name = $"Kalkulacija_{cal.Cal.Cal.Counter}-{cal.Cal.Cal.CalculationDate.Year}",
                            Supplier = cal.Cal.Supp == null ? null : new Supplier(cal.Cal.Supp),
                            Cashier = cal.Cash
                        };

                        _currentViewModel.CalculationsAll.Add(calculation);

                        _currentViewModel.TotalInputPrice += calculation.InputTotalPrice;
                        _currentViewModel.TotalOutputPrice += calculation.OutputTotalPrice;
                    });
                }
            }
            else
            {
                if (_currentViewModel.SearchFromCalculationDate != null &&
                _currentViewModel.SearchFromCalculationDate.HasValue)
                {
                    //calculations = sqliteDbContext.Calculations.Where(cal => cal.CalculationDate.Date >= _currentViewModel.SearchFromCalculationDate.Value.Date);

                    var calculations = _currentViewModel.DbContext.Calculations.Where(cal => cal.CalculationDate.Date >= _currentViewModel.SearchFromCalculationDate.Value.Date)
                    .Join(_currentViewModel.DbContext.Suppliers,
                    cal => cal.SupplierId,
                    supp => supp.Id,
                    (cal, supp) => new { Cal = cal, Supp = supp })
                    .Join(_currentViewModel.DbContext.Cashiers,
                    cal => cal.Cal.CashierId,
                    cash => cash.Id,
                    (cal, cash) => new { Cal = cal, Cash = cash });

                    if (calculations != null &&
                        calculations.Any())
                    {
                        calculations.ForEachAsync(cal =>
                        {
                            Models.AppMain.Statistic.Calculation calculation = new Models.AppMain.Statistic.Calculation()
                            {
                                Id = cal.Cal.Cal.Id,
                                CalculationDate = cal.Cal.Cal.CalculationDate,
                                InputTotalPrice = cal.Cal.Cal.InputTotalPrice,
                                InvoiceNumber = cal.Cal.Cal.InvoiceNumber,
                                OutputTotalPrice = cal.Cal.Cal.OutputTotalPrice,
                                Counter = cal.Cal.Cal.Counter,
                                Name = $"Kalkulacija_{cal.Cal.Cal.Counter}-{cal.Cal.Cal.CalculationDate.Year}",
                                Supplier = cal.Cal.Supp == null ? null : new Supplier(cal.Cal.Supp),
                                Cashier = cal.Cash
                            };

                            _currentViewModel.CalculationsAll.Add(calculation);

                            _currentViewModel.TotalInputPrice += calculation.InputTotalPrice;
                            _currentViewModel.TotalOutputPrice += calculation.OutputTotalPrice;
                        });
                    }
                }
                else if (_currentViewModel.SearchToCalculationDate != null &&
                _currentViewModel.SearchToCalculationDate.HasValue)
                {
                    //calculations = sqliteDbContext.Calculations.Where(cal => cal.CalculationDate.Date <= _currentViewModel.SearchToCalculationDate.Value.Date);

                    var calculations = _currentViewModel.DbContext.Calculations.Where(cal => cal.CalculationDate.Date <= _currentViewModel.SearchToCalculationDate.Value.Date)
                    .Join(_currentViewModel.DbContext.Suppliers,
                    cal => cal.SupplierId,
                    supp => supp.Id,
                    (cal, supp) => new { Cal = cal, Supp = supp })
                    .Join(_currentViewModel.DbContext.Cashiers,
                    cal => cal.Cal.CashierId,
                    cash => cash.Id,
                    (cal, cash) => new { Cal = cal, Cash = cash });

                    if (calculations != null &&
                        calculations.Any())
                    {
                        calculations.ForEachAsync(cal =>
                        {
                            Models.AppMain.Statistic.Calculation calculation = new Models.AppMain.Statistic.Calculation()
                            {
                                Id = cal.Cal.Cal.Id,
                                CalculationDate = cal.Cal.Cal.CalculationDate,
                                InputTotalPrice = cal.Cal.Cal.InputTotalPrice,
                                InvoiceNumber = cal.Cal.Cal.InvoiceNumber,
                                OutputTotalPrice = cal.Cal.Cal.OutputTotalPrice,
                                Counter = cal.Cal.Cal.Counter,
                                Name = $"Kalkulacija_{cal.Cal.Cal.Counter}-{cal.Cal.Cal.CalculationDate.Year}",
                                Supplier = cal.Cal.Supp == null ? null : new Supplier(cal.Cal.Supp),
                                Cashier = cal.Cash
                            };

                            _currentViewModel.CalculationsAll.Add(calculation);

                            _currentViewModel.TotalInputPrice += calculation.InputTotalPrice;
                            _currentViewModel.TotalOutputPrice += calculation.OutputTotalPrice;
                        });
                    }
                }
                else
                {
                    //calculations = sqliteDbContext.Calculations;

                    var calculations = _currentViewModel.DbContext.Calculations
                    .Join(_currentViewModel.DbContext.Suppliers,
                    cal => cal.SupplierId,
                    supp => supp.Id,
                    (cal, supp) => new { Cal = cal, Supp = supp })
                    .Join(_currentViewModel.DbContext.Cashiers,
                    cal => cal.Cal.CashierId,
                    cash => cash.Id,
                    (cal, cash) => new { Cal = cal, Cash = cash });

                    if (calculations != null &&
                        calculations.Any())
                    {
                        calculations.ForEachAsync(cal =>
                        {
                            Models.AppMain.Statistic.Calculation calculation = new Models.AppMain.Statistic.Calculation()
                            {
                                Id = cal.Cal.Cal.Id,
                                CalculationDate = cal.Cal.Cal.CalculationDate,
                                InputTotalPrice = cal.Cal.Cal.InputTotalPrice,
                                InvoiceNumber = cal.Cal.Cal.InvoiceNumber,
                                OutputTotalPrice = cal.Cal.Cal.OutputTotalPrice,
                                Counter = cal.Cal.Cal.Counter,
                                Name = $"Kalkulacija_{cal.Cal.Cal.Counter}-{cal.Cal.Cal.CalculationDate.Year}",
                                Supplier = cal.Cal.Supp == null ? null : new Supplier(cal.Cal.Supp),
                                Cashier = cal.Cash
                            };

                            _currentViewModel.CalculationsAll.Add(calculation);

                            _currentViewModel.TotalInputPrice += calculation.InputTotalPrice;
                            _currentViewModel.TotalOutputPrice += calculation.OutputTotalPrice;
                        });
                    }
                }
            }

            if (!string.IsNullOrEmpty(_currentViewModel.SearchInvoiceNumber))
            {
                SearhInvoiceNumber();
            }
            else
            {
                _currentViewModel.Calculations = new ObservableCollection<Models.AppMain.Statistic.Calculation>(_currentViewModel.CalculationsAll.OrderBy(cal => cal.CalculationDate));
            }
            if (_currentViewModel.SearchSupplier != null && !string.IsNullOrEmpty(_currentViewModel.SearchSupplier.Name))
            {
                SearhSupplier();
            }
            else
            {
                if (string.IsNullOrEmpty(_currentViewModel.SearchInvoiceNumber))
                {
                    _currentViewModel.Calculations = new ObservableCollection<Models.AppMain.Statistic.Calculation>(_currentViewModel.CalculationsAll.OrderBy(cal => cal.CalculationDate));
                }
            }
            if (_currentViewModel.SearchFromCalculationDate != null ||
                _currentViewModel.SearchToCalculationDate != null)
            {
                SearhCalculationDate();
            }
            else
            {
                if (string.IsNullOrEmpty(_currentViewModel.SearchInvoiceNumber) &&
                    _currentViewModel.SearchSupplier == null)
                {
                    _currentViewModel.Calculations = new ObservableCollection<Models.AppMain.Statistic.Calculation>(_currentViewModel.CalculationsAll.OrderBy(cal => cal.CalculationDate));
                }
            }
        }

        private void SearhInvoiceNumber()
        {
            var calculations = _currentViewModel.CalculationsAll.Where(calculation => !string.IsNullOrEmpty(calculation.InvoiceNumber) &&
            calculation.InvoiceNumber.Contains(_currentViewModel.SearchInvoiceNumber));

            if (calculations != null)
            {
                _currentViewModel.Calculations = new ObservableCollection<Models.AppMain.Statistic.Calculation>(calculations.OrderBy(cal => cal.CalculationDate));
            }
        }

        private void SearhSupplier()
        {
            var calculations = _currentViewModel.Calculations.Where(calculation => calculation.Supplier != null &&
            _currentViewModel.SearchSupplier != null &&
            calculation.Supplier.Id == _currentViewModel.SearchSupplier.Id);

            if (calculations != null)
            {
                _currentViewModel.Calculations = new ObservableCollection<Models.AppMain.Statistic.Calculation>(calculations.OrderBy(cal => cal.CalculationDate));
            }
        }
        private void SearhCalculationDate()
        {
            DateTime? fromDate = null;
            DateTime? toDate = null;

            if(_currentViewModel.SearchFromCalculationDate.HasValue)
            {
                fromDate = new DateTime(_currentViewModel.SearchFromCalculationDate.Value.Year,
                    _currentViewModel.SearchFromCalculationDate.Value.Month,
                    _currentViewModel.SearchFromCalculationDate.Value.Day,
                    0,
                    0,
                    0);
            }
            if (_currentViewModel.SearchToCalculationDate.HasValue)
            {
                toDate = new DateTime(_currentViewModel.SearchToCalculationDate.Value.Year,
                    _currentViewModel.SearchToCalculationDate.Value.Month,
                    _currentViewModel.SearchToCalculationDate.Value.Day,
                    23,
                    59,
                    59);
            }

            if(fromDate != null && toDate != null)
            {
                var calculations = _currentViewModel.Calculations.Where(calculation => calculation.CalculationDate >= fromDate &&
                calculation.CalculationDate <= toDate);

                if (calculations != null)
                {
                    _currentViewModel.Calculations = new ObservableCollection<Models.AppMain.Statistic.Calculation>(calculations.OrderBy(cal => cal.CalculationDate));
                }
            }
            else
            {
                if(toDate != null)
                {
                    var calculations = _currentViewModel.Calculations.Where(calculation => calculation.CalculationDate <= toDate);

                    if (calculations != null)
                    {
                        _currentViewModel.Calculations = new ObservableCollection<Models.AppMain.Statistic.Calculation>(calculations.OrderBy(cal => cal.CalculationDate));
                    }
                }
                else
                {
                    var calculations = _currentViewModel.Calculations.Where(calculation => calculation.CalculationDate >= fromDate);

                    if (calculations != null)
                    {
                        _currentViewModel.Calculations = new ObservableCollection<Models.AppMain.Statistic.Calculation>(calculations.OrderBy(cal => cal.CalculationDate));
                    }
                }
            }
        }
    }
}