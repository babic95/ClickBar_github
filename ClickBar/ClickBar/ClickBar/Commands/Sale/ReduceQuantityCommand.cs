using ClickBar.Models.Sale;
using ClickBar.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ClickBar.Commands.Sale
{
    public class ReduceQuantityCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private SaleViewModel _currentViewModel;

        public ReduceQuantityCommand(SaleViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            string id = parameter as string;

            var items = _currentViewModel.ItemsInvoice.Where(item => item.Item.Id == id).FirstOrDefault();

            if (items is not null)
            {
                decimal quantity = 1;

                try
                {
                    quantity = Convert.ToDecimal(_currentViewModel.Quantity);
                    quantity = Math.Round(quantity, 2);
                }
                catch
                {
                    MessageBox.Show("Unesite ispravnu količinu!", "Ne ispravna količina", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (items.Quantity > quantity)
                {
                    items.Quantity -= quantity;

                    items.TotalAmout -= items.Item.SellingUnitPrice * quantity;

                    if (items.Zelje != null)
                    {
                        for (int i = 0; i < quantity; i++)
                        {
                            items.Zelje.RemoveAt(items.Zelje.Count - 1 - i);
                        }
                    }

                    _currentViewModel.TotalAmount -= items.Item.SellingUnitPrice * quantity;
                }
                else if(items.Quantity == quantity)
                {
                    _currentViewModel.TotalAmount -= items.Item.SellingUnitPrice * quantity;
                    _currentViewModel.ItemsInvoice.Remove(items);
                }
                else
                {
                    _currentViewModel.TotalAmount -= items.Item.SellingUnitPrice * items.Quantity;
                    _currentViewModel.ItemsInvoice.Remove(items);
                }

                _currentViewModel.FirstChangeQuantity = true;
            }

            if (_currentViewModel.ItemsInvoice.Any())
            {
                _currentViewModel.HookOrderEnable = true;
            }
            else
            {
                _currentViewModel.HookOrderEnable = false;
            }
        }
    }
}
