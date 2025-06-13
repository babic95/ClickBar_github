using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows;
using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar.Models.AppMain.Statistic.Porudzbine;
using ClickBar_Logging;
using ClickBar.Enums.AppMain.Statistic;
using ClickBar_DatabaseSQLManager;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace ClickBar.Commands.AppMain.Statistic.Porudzbine
{
    public class SearchOrdersCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private PregledPorudzbinaNaDanViewModel _currentViewModel;

        public SearchOrdersCommand(PregledPorudzbinaNaDanViewModel currentViewModel)
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
                _currentViewModel.ProdatoTotal = 0;
                _currentViewModel.ObrisanoTotal = 0;
                _currentViewModel.NeobradjenoTotal = 0;

                var ordersDB = _currentViewModel.DbContext.OrdersToday
                    .Include(i => i.Cashier)
                    .Where(i => i.OrderDateTime.Date == _currentViewModel.CurrentDate.Date)
                    .OrderBy(i => i.OrderDateTime).ToList();

                if (ordersDB == null || !ordersDB.Any())
                {
                    MessageBox.Show("Nema porudžbina za odabran datum.", "Informacija", MessageBoxButton.OK, MessageBoxImage.Information);
                    _currentViewModel.Orders = new ObservableCollection<OrderToday>();
                    return;
                }

                _currentViewModel.Orders = new ObservableCollection<OrderToday>(ordersDB.Select(i => new OrderToday(i)));

                _currentViewModel.ProdatoTotal = _currentViewModel.Orders
                    .Where(i => i.Status == OrdersTodayEnumeration.Naplaćeno)
                    .Sum(i => i.TotalPrice);
                _currentViewModel.ObrisanoTotal = _currentViewModel.Orders
                    .Where(i => i.Status == OrdersTodayEnumeration.Obrisano)
                    .Sum(i => i.TotalPrice);
                _currentViewModel.NeobradjenoTotal = _currentViewModel.Orders
                    .Where(i => i.Status == OrdersTodayEnumeration.Neodređeno)
                    .Sum(i => i.TotalPrice);
            }
            catch (Exception ex)
            {
                Log.Error("SearchOrdersCommand -> Desila se greska prilkom pretrage porudzbina: ", ex);

                MessageBox.Show("Desila se greška prilikom pretrage porudžbina. Molimo pokušajte ponovo.",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}