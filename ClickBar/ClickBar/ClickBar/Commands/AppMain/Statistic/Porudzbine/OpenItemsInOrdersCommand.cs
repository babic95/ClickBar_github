using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows;
using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar.Models.AppMain.Statistic.Porudzbine;
using ClickBar_Logging;
using ClickBar.Enums.AppMain.Statistic;
using ClickBar.Views.AppMain.AuxiliaryWindows.Statistic.Porudzbine;
using Microsoft.EntityFrameworkCore;

namespace ClickBar.Commands.AppMain.Statistic.Porudzbine
{
    public class OpenItemsInOrdersCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private PregledPorudzbinaNaDanViewModel _currentViewModel;

        public OpenItemsInOrdersCommand(PregledPorudzbinaNaDanViewModel currentViewModel)
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
                if (parameter != null &&
                parameter is string)
                {
                    string orderId = (string)parameter;

                    var order = _currentViewModel.Orders.FirstOrDefault(ord => ord.Id == orderId);

                    if (order != null)
                    {
                        _currentViewModel.ItemsInOrder = new ObservableCollection<OrderTodayItem>();

                        var itemsDB = _currentViewModel.DbContext.OrderTodayItems
                            .Include(o => o.Item)
                            .Where(ord => ord.OrderTodayId == orderId)
                            .ToList();

                        if (itemsDB != null &&
                            itemsDB.Any())
                        {
                            _currentViewModel.ItemsInOrder = new ObservableCollection<OrderTodayItem>(itemsDB.Select(i => new OrderTodayItem(i)));
                        }

                        if (_currentViewModel.Window != null &&
                            _currentViewModel.Window.IsActive)
                        {
                            _currentViewModel.Window.Close();
                        }
                        _currentViewModel.Window = new ItemsInOrderWindow(_currentViewModel);
                        _currentViewModel.Window.ShowDialog();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("OpenItemsInOrdersCommand -> Desila se greska prilkom otvaranja porudzbine: ", ex);

                MessageBox.Show("Desila se greška prilikom otvaranja porudžbine. Molimo pokušajte ponovo.",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}