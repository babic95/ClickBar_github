using ClickBar.ViewModels.AppMain.Statistic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using System.Data.Entity;
using ClickBar.Models.AppMain.Statistic.Items;
using System.Collections.ObjectModel;
using ClickBar.Views.AppMain.AuxiliaryWindows.Statistic.InventoryStatus;

namespace ClickBar.Commands.AppMain.Statistic.InventoryStatus
{
    public class OpenCardItemCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private InventoryStatusViewModel _currentViewModel;

        public OpenCardItemCommand(InventoryStatusViewModel currentViewModel)
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
                var itemDB = _currentViewModel.DbContext.Items.AsNoTracking().FirstOrDefault(i => i.Id == parameter.ToString());

                if (itemDB != null)
                {
                    var date = DateTime.Now;
                    _currentViewModel.ItemCardFromDate = new DateTime(date.Year, date.Month, date.Day, 5, 0, 0);
                    _currentViewModel.ItemCardToDate = _currentViewModel.ItemCardFromDate.AddHours(23).AddMinutes(59).AddSeconds(59);

                    _currentViewModel.CurrentItemCard = new CardForItem()
                    {
                        Id = itemDB.Id,
                        Name = itemDB.Name,
                        Jm = itemDB.Jm,
                    };
                }

                _currentViewModel.SearchCardItemCommand.Execute(null);

                ItemCardWindow itemCardWindow = new ItemCardWindow(_currentViewModel);
                itemCardWindow.ShowDialog();
            }
            catch
            {
                MessageBox.Show("Greška prilikom otvaranja kartice artikla!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}