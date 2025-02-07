using ClickBar.Models.Sale;
using ClickBar.ViewModels.Sale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using ClickBar_Logging;

namespace ClickBar.Commands.Sale.Pay.SplitOrder
{
    public class MoveAllToOrderCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private SplitOrderViewModel _viewModel;

        public MoveAllToOrderCommand(SplitOrderViewModel viewModel)
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
                if (_viewModel.ItemsInvoiceForPay.Any())
                {
                    _viewModel.ItemsInvoiceForPay.ToList().ForEach(item =>
                    {
                        var itemInOrder = _viewModel.ItemsInvoice.FirstOrDefault(i => i.Item.Id == item.Item.Id);

                        if (itemInOrder != null)
                        {
                            itemInOrder.Quantity += item.Quantity;
                        }
                        else
                        {
                            ItemInvoice itemInvoice = new ItemInvoice(item.Item, item.Quantity);
                            _viewModel.ItemsInvoice.Add(itemInvoice);
                        }
                        _viewModel.TotalAmount += decimal.Round(item.Item.SellingUnitPrice * item.Quantity, 2);
                    });

                    _viewModel.TotalAmountForPay = 0;
                    _viewModel.ItemsInvoiceForPay.Clear();
                }
            }
            catch(Exception ex)
            {
                Log.Error("MoveAllToOrderCommand -> Execute -> Desila se greska prilikom prebacivanja svih stavki na racun", ex);
                MessageBox.Show("Desila se neočekivana greška prilikom prebacivanja svih stavki na račun.", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}