using ClickBar.Models.Sale;
using ClickBar.ViewModels.Sale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ClickBar.Commands.Sale.Pay.SplitOrder
{
    public class MoveToPaymentCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private SplitOrderViewModel _viewModel;

        public MoveToPaymentCommand(SplitOrderViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            decimal quantity = 1;
            try
            {
                _viewModel.Quantity = _viewModel.Quantity.Replace(',', '.');

                quantity = Convert.ToDecimal(_viewModel.Quantity);
                quantity = Math.Round(quantity, 3);
            }
            catch
            {
                MessageBox.Show("Unesite ispravnu količinu!", "Ne ispravna količina", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (_viewModel.SelectedItemInvoice != null)
            {
                ItemInvoice itemInvoice = new ItemInvoice(_viewModel.SelectedItemInvoice.Item, quantity);

                if (_viewModel.SelectedItemInvoice.Quantity > quantity)
                {
                    _viewModel.SelectedItemInvoice.Quantity -= quantity;
                    _viewModel.SelectedItemInvoice.TotalAmout -= _viewModel.SelectedItemInvoice.Item.SellingUnitPrice * quantity;
                }
                else if (_viewModel.SelectedItemInvoice.Quantity == quantity)
                {
                    _viewModel.SelectedItemInvoice.TotalAmout -= _viewModel.SelectedItemInvoice.Item.SellingUnitPrice * quantity;
                    _viewModel.ItemsInvoice.Remove(_viewModel.SelectedItemInvoice);
                    _viewModel.SelectedItemInvoice = null;
                }
                else
                {
                    quantity = _viewModel.SelectedItemInvoice.Quantity;

                    _viewModel.SelectedItemInvoice.TotalAmout -= _viewModel.SelectedItemInvoice.Item.SellingUnitPrice * quantity;
                    _viewModel.ItemsInvoice.Remove(_viewModel.SelectedItemInvoice);
                    _viewModel.SelectedItemInvoice = null;
                }
                _viewModel.TotalAmount -= itemInvoice.Item.SellingUnitPrice * quantity;

                var item = _viewModel.ItemsInvoiceForPay.Where(i => i.Item.Id == itemInvoice.Item.Id).ToList().FirstOrDefault();

                if(item != null)
                {
                    item.Quantity += quantity;
                    item.TotalAmout += item.Item.SellingUnitPrice * quantity;
                }
                else
                {
                    _viewModel.ItemsInvoiceForPay.Add(itemInvoice);
                }
                _viewModel.TotalAmountForPay += itemInvoice.Item.SellingUnitPrice * quantity;
            }
        }
    }
}