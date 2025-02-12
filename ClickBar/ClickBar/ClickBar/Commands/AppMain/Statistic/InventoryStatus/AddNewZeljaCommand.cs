using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar_DatabaseSQLManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace ClickBar.Commands.AppMain.Statistic.InventoryStatus
{
    public class AddNewZeljaCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private InventoryStatusViewModel _currentViewModel;

        public AddNewZeljaCommand(InventoryStatusViewModel currentViewModel)
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
                if (parameter is string idItem)
                {
                    _currentViewModel.Zelje.Add(new Models.AppMain.Statistic.ItemZelja()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Zelja = string.Empty,
                        ItemId = idItem
                    });
                }
            }
            catch
            {
                MessageBox.Show("Greška prilikom brisanja normativa iz artikla!", "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}