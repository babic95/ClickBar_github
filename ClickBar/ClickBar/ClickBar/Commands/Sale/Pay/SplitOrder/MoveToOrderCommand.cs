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
    public class MoveToOrderCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private SplitOrderViewModel _viewModel;

        public MoveToOrderCommand(SplitOrderViewModel viewModel)
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

            if (_viewModel.SelectedItemInvoiceForPay != null)
            {
                ItemInvoice itemInvoice = new ItemInvoice(_viewModel.SelectedItemInvoiceForPay.Item, quantity);

                if (_viewModel.SelectedItemInvoiceForPay.Quantity > quantity)
                {
                    _viewModel.SelectedItemInvoiceForPay.Quantity -= quantity;
                    _viewModel.SelectedItemInvoiceForPay.TotalAmout -= _viewModel.SelectedItemInvoiceForPay.Item.SellingUnitPrice * quantity;
                }
                else if (_viewModel.SelectedItemInvoiceForPay.Quantity == quantity)
                {
                    _viewModel.SelectedItemInvoiceForPay.TotalAmout -= _viewModel.SelectedItemInvoiceForPay.Item.SellingUnitPrice * quantity;
                    _viewModel.ItemsInvoiceForPay.Remove(_viewModel.SelectedItemInvoiceForPay);
                    _viewModel.SelectedItemInvoiceForPay = null;
                }
                else
                {
                    quantity = _viewModel.SelectedItemInvoice.Quantity;

                    _viewModel.SelectedItemInvoiceForPay.TotalAmout -= _viewModel.SelectedItemInvoiceForPay.Item.SellingUnitPrice * quantity;
                    _viewModel.ItemsInvoiceForPay.Remove(_viewModel.SelectedItemInvoiceForPay);
                    _viewModel.SelectedItemInvoiceForPay = null;
                }
                _viewModel.TotalAmountForPay -= itemInvoice.Item.SellingUnitPrice * quantity;

                var item = _viewModel.ItemsInvoice.Where(i => i.Item.Id == itemInvoice.Item.Id).ToList().FirstOrDefault();

                if (item != null)
                {
                    item.Quantity += quantity;
                    item.TotalAmout += item.Item.SellingUnitPrice * quantity;
                }
                else
                {
                    _viewModel.ItemsInvoice.Add(itemInvoice);
                }
                _viewModel.TotalAmount += itemInvoice.Item.SellingUnitPrice * quantity;
            }
        }
    }
}