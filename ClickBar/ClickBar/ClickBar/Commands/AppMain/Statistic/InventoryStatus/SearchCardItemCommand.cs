using ClickBar.ViewModels.AppMain.Statistic;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using ClickBar_Logging;
using ClickBar.Models.AppMain.Statistic.Items;
using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ClickBar.Commands.AppMain.Statistic.InventoryStatus
{
    public class SearchCardItemCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private InventoryStatusViewModel _currentViewModel;

        public SearchCardItemCommand(InventoryStatusViewModel currentViewModel)
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
                if (_currentViewModel.CurrentItemCard != null)
                {
                    var calculationItem = _currentViewModel.DbContext.CalculationItems.Include(c => c.Calculation)
                        .Where(ci => ci.ItemId == _currentViewModel.CurrentItemCard.Id &&
                        ci.Calculation.CalculationDate >= _currentViewModel.ItemCardFromDate &&
                        ci.Calculation.CalculationDate <= _currentViewModel.ItemCardToDate)
                        .ToList();

                    var otpisItem = _currentViewModel.DbContext.OtpisItems.Include(oi => oi.Otpis)
                        .Where(oi => oi.ItemId == _currentViewModel.CurrentItemCard.Id &&
                        oi.Otpis.OtpisDate >= _currentViewModel.ItemCardFromDate &&
                        oi.Otpis.OtpisDate <= _currentViewModel.ItemCardToDate)
                        .ToList();

                    var pazarItem = _currentViewModel.DbContext.ItemInvoices.AsNoTracking().Join(_currentViewModel.DbContext.Invoices.AsNoTracking(),
                        ii => ii.InvoiceId,
                        i => i.Id,
                        (ii, i) => new { ii, i })
                        .Where(ii => ii.ii.ItemCode == _currentViewModel.CurrentItemCard.Id &&
                        ii.i.TransactionType == 0 &&
                        ii.ii.Quantity.HasValue &&
                        ii.i.SdcDateTime != null &&
                        ii.i.SdcDateTime >= _currentViewModel.ItemCardFromDate &&
                        ii.i.SdcDateTime <= _currentViewModel.ItemCardToDate)
                        .ToList();

                    var pazarRefundacijaItem = _currentViewModel.DbContext.ItemInvoices.AsNoTracking().Join(_currentViewModel.DbContext.Invoices.AsNoTracking(),
                        ii => ii.InvoiceId,
                        i => i.Id,
                        (ii, i) => new { ii, i })
                        .Where(ii => ii.ii.ItemCode == _currentViewModel.CurrentItemCard.Id &&
                        ii.i.TransactionType == 1 &&
                        ii.ii.Quantity.HasValue &&
                        ii.i.SdcDateTime != null &&
                        ii.i.SdcDateTime >= _currentViewModel.ItemCardFromDate &&
                        ii.i.SdcDateTime <= _currentViewModel.ItemCardToDate)
                        .ToList();

                    _currentViewModel.CurrentItemCard.TotalInputQuantity = calculationItem.Sum(ci => ci.Quantity);
                    _currentViewModel.CurrentItemCard.TotalOtpisQuantity = otpisItem.Sum(oi => oi.Quantity);
                    _currentViewModel.CurrentItemCard.TotalOutputQuantity = pazarItem.Sum(ii => ii.ii.Quantity.Value) - pazarRefundacijaItem.Sum(ii => ii.ii.Quantity.Value);
                    _currentViewModel.CurrentItemCard.TotalInputPrice = calculationItem.Sum(ci => Decimal.Round(ci.InputPrice * ci.Quantity, 2));
                    _currentViewModel.CurrentItemCard.TotalOutputPrice = pazarItem.Sum(ii => ii.ii.TotalAmout.Value) - pazarRefundacijaItem.Sum(ii => ii.ii.TotalAmout.Value);
                    _currentViewModel.CurrentItemCard.TotalOtpisPrice = otpisItem.Sum(oi => oi.TotalPrice);

                    // Kombinovanje i sortiranje
                    var combinedItems = calculationItem.Select(ci => new ItemCard
                    {
                        Description = $"Kalkulacija {ci.Calculation.Counter}_{ci.Calculation.CalculationDate.Year}",
                        Date = ci.Calculation.CalculationDate,
                        InputQuantity = ci.Quantity,
                        OutputQuantity = 0,
                        OtpisQuantity = 0,
                        InputPrice = Decimal.Round(ci.InputPrice * ci.Quantity),
                        OutputPrice = 0,
                        OtpisPrice = 0
                    })
                    .Union(otpisItem.Select(oi => new ItemCard
                    {
                        Description = $"Otpis {oi.Otpis.Counter}",
                        Date = oi.Otpis.OtpisDate,
                        InputQuantity = 0,
                        OutputQuantity = 0,
                        OtpisQuantity = oi.Quantity,
                        InputPrice = 0,
                        OutputPrice = 0,
                        OtpisPrice = oi.TotalPrice
                    }))
                    .Union(pazarItem.Select(pi => new ItemCard
                    {
                        Description = $"Pazar - {pi.i.InvoiceNumberResult}",
                        Date = pi.i.SdcDateTime.Value,
                        InputQuantity = 0,
                        OutputQuantity = pi.ii.Quantity.Value,
                        OtpisQuantity = 0,
                        InputPrice = 0,
                        OutputPrice = pi.ii.TotalAmout.Value,
                        OtpisPrice = 0
                    }))
                    .Union(pazarRefundacijaItem.Select(pi => new ItemCard
                    {
                        Description = $"Refundacija - {pi.i.InvoiceNumberResult}",
                        Date = pi.i.SdcDateTime.Value,
                        InputQuantity = 0,
                        OutputQuantity = -1 * pi.ii.Quantity.Value,
                        OtpisQuantity = 0,
                        InputPrice = 0,
                        OutputPrice = -1 * pi.ii.TotalAmout.Value,
                        OtpisPrice = 0
                    }))
                    .OrderBy(item => item.Date)
                    .ToList();

                    _currentViewModel.CurrentItemCard.Items = new ObservableCollection<ItemCard>(
                        combinedItems.Select(ci => new ItemCard
                        {
                            Description = ci.Description,
                            Date = ci.Date,
                            InputQuantity = ci.InputQuantity,
                            OutputQuantity = ci.OutputQuantity,
                            OtpisQuantity = ci.OtpisQuantity,
                            InputPrice = ci.InputPrice,
                            OutputPrice = ci.OutputPrice,
                            OtpisPrice = ci.OtpisPrice
                        })
                    );
                }
            }
            catch (Exception ex)
            {
                Log.Error("SearchCardItemCommand -> Desila se greska prilikom pretrage kartice artikla: ", ex);
                MessageBox.Show("Greška!\nObratite se serviseru.", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}