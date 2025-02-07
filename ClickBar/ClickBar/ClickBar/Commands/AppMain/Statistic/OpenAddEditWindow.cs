using ClickBar.Models.AppMain.Statistic;
using ClickBar.Models.Sale;
using ClickBar.ViewModels;
using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar.Views.AppMain.AuxiliaryWindows.Statistic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClickBar.Commands.AppMain.Statistic
{
    public class OpenAddEditWindow : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ViewModelBase _currentViewModel;

        public OpenAddEditWindow(ViewModelBase currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (_currentViewModel is AddEditSupplierViewModel)
            {
                AddEditSupplierViewModel addEditSupplierViewModel = (AddEditSupplierViewModel)_currentViewModel;

                AddEditSupplierWindow addEditSupplierWindow = new AddEditSupplierWindow(addEditSupplierViewModel);
                addEditSupplierWindow.ShowDialog();
            }
            else if (_currentViewModel is InventoryStatusViewModel)
            {
                InventoryStatusViewModel inventoryStatusViewModel = (InventoryStatusViewModel)_currentViewModel;

                inventoryStatusViewModel.CurrentGroupItems = inventoryStatusViewModel.AllGroupItems.FirstOrDefault();
                inventoryStatusViewModel.Norma = new ObservableCollection<Invertory>();
                inventoryStatusViewModel.Zelje = new ObservableCollection<ItemZelja>();
                inventoryStatusViewModel.CurrentInventoryStatus = new Invertory();
                inventoryStatusViewModel.CurrentNorm = -1;
                inventoryStatusViewModel.EditItemIsReadOnly = false;
                AddEditItemWindow addEditItemWindow = new AddEditItemWindow(inventoryStatusViewModel);
                addEditItemWindow.ShowDialog();
            }
        }
    }
}