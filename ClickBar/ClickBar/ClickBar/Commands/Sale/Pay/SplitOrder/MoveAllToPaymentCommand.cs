using ClickBar.ViewModels.Sale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using ClickBar.Models.Sale;
using ClickBar_Logging;

namespace ClickBar.Commands.Sale.Pay.SplitOrder
{
    public class MoveAllToPaymentCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private SplitOrderViewModel _viewModel;

        public MoveAllToPaymentCommand(SplitOrderViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            try
            {
                if (_viewModel.ItemsInvoice.Any())
                {
                    _viewModel.ItemsInvoice.ToList().ForEach(item =>
                    {
                        var itemIninvoice = _viewModel.ItemsInvoiceForPay.FirstOrDefault(i => i.Item.Id == item.Item.Id);

                        if (itemIninvoice != null)
                        {
                            itemIninvoice.Quantity += item.Quantity;
                        }
                        else
                        {
                            ItemInvoice itemInvoice = new ItemInvoice(item.Item, item.Quantity);
                            _viewModel.ItemsInvoiceForPay.Add(itemInvoice);
                        }
                        _viewModel.TotalAmountForPay += decimal.Round(item.Quantity * item.Item.SellingUnitPrice, 2);
                    });

                    _viewModel.TotalAmount = 0;
                    _viewModel.ItemsInvoice.Clear();
                }
            }
            catch (Exception ex)
            {
                Log.Error("MoveAllToPaymentCommand -> Execute -> Desila se greska prilikom prebacivanja svih stavki", ex);
                MessageBox.Show("Desila se neočekivana greška prilikom prebacivanja svih stavki.", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}