using ClickBar.ViewModels.Sale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace ClickBar.Commands.Sale.Pay
{
    public class ChangeFocusCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private PaySaleViewModel _viewModel;

        public ChangeFocusCommand(PaySaleViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            PaySale(parameter.ToString());
        }
        private void PaySale(string focus)
        {
            decimal Other = Convert.ToDecimal(_viewModel.Other);
            decimal Cash = Convert.ToDecimal(_viewModel.Cash);
            decimal Card = Convert.ToDecimal(_viewModel.Card);
            decimal Check = Convert.ToDecimal(_viewModel.Check);
            decimal WireTransfer = Convert.ToDecimal(_viewModel.WireTransfer);
            decimal Voucher = Convert.ToDecimal(_viewModel.Voucher);
            decimal MobileMoney = Convert.ToDecimal(_viewModel.MobileMoney);

            _viewModel.Amount = Other + Cash + Card + Check + WireTransfer + Voucher + MobileMoney;

            if (focus.Contains("enter"))
            {
                if (_viewModel.Focus == FocusEnumeration.Pay)
                {
                    _viewModel.PayCommand.Execute(null);
                }
                else
                {
                    //if(paySaleViewModel.SaleViewModel.InvoiceType == Enums.InvoiceTypeEnumeration.Avans &&
                    //    paySaleViewModel.Amount >= 1m)
                    //{
                    //    paySaleViewModel.AmountBorderBrush = Brushes.Transparent;
                    //    if (paySaleViewModel.IsRefaction)
                    //    {
                    //        if (string.IsNullOrEmpty(paySaleViewModel.BuyerId))
                    //        {
                    //            paySaleViewModel.Focus = ViewModels.AppMain.Sale.FocusEnumeration.BuyerId;
                    //            paySaleViewModel.IsEnablePay = false;
                    //            return;
                    //        }
                    //        if (string.IsNullOrEmpty(paySaleViewModel.RefactionFormNumber))
                    //        {
                    //            paySaleViewModel.Focus = ViewModels.AppMain.Sale.FocusEnumeration.RefactionFormNumber;
                    //            paySaleViewModel.IsEnablePay = false;
                    //            return;
                    //        }
                    //    }
                    //    paySaleViewModel.IsEnablePay = true;
                    //    paySaleViewModel.Focus = ViewModels.AppMain.Sale.FocusEnumeration.Pay;
                    //}
                    if (_viewModel.Amount >= _viewModel.SaleViewModel.TotalAmount)
                    {
                        _viewModel.AmountBorderBrush = Brushes.Transparent;
                        _viewModel.IsEnablePay = true;
                        _viewModel.Focus = FocusEnumeration.Pay;
                    }
                    else
                    {
                        if (_viewModel.Amount >= _viewModel.SaleViewModel.TotalAmount)
                        {
                            _viewModel.AmountBorderBrush = Brushes.Transparent;
                        }
                        else
                        {
                            _viewModel.AmountBorderBrush = Brushes.Red;
                        }

                        _viewModel.IsEnablePay = false;

                        switch (_viewModel.Focus)
                        {
                            case FocusEnumeration.BuyerId:
                                _viewModel.Focus = FocusEnumeration.BuyerId;
                                break;
                            case FocusEnumeration.Cash:
                                _viewModel.Focus = FocusEnumeration.Card;
                                break;
                            case FocusEnumeration.Card:
                                _viewModel.Focus = FocusEnumeration.Check;
                                break;
                            case FocusEnumeration.Check:
                                _viewModel.Focus = FocusEnumeration.Other;
                                break;
                            case FocusEnumeration.Other:
                                _viewModel.Focus = FocusEnumeration.Voucher;
                                break;
                            case FocusEnumeration.Voucher:
                                _viewModel.Focus = FocusEnumeration.WireTransfer;
                                break;
                            case FocusEnumeration.WireTransfer:
                                _viewModel.Focus = FocusEnumeration.MobileMoney;
                                break;
                            case FocusEnumeration.MobileMoney:
                                _viewModel.Focus = FocusEnumeration.Cash;
                                break;
                        }
                    }
                }
            }
            else
            {
                switch (focus)
                {
                    case "BuyerId":
                        _viewModel.Focus = FocusEnumeration.BuyerId;
                        break;
                    case "Cash":
                        _viewModel.Focus = FocusEnumeration.Cash;
                        break;
                    case "Card":
                        _viewModel.Focus = FocusEnumeration.Card;
                        break;
                    case "Check":
                        _viewModel.Focus = FocusEnumeration.Check;
                        break;
                    case "Other":
                        _viewModel.Focus = FocusEnumeration.Other;
                        break;
                    case "Voucher":
                        _viewModel.Focus = FocusEnumeration.Voucher;
                        break;
                    case "WireTransfer":
                        _viewModel.Focus = FocusEnumeration.WireTransfer;
                        break;
                    case "MobileMoney":
                        _viewModel.Focus = FocusEnumeration.MobileMoney;
                        break;
                    case "Pay":
                        _viewModel.Focus = FocusEnumeration.Pay;
                        break;
                }
            }
        }
    }
}
