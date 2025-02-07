using ClickBar.ViewModels;
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
    public class ClickOnNumberButtonCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private PaySaleViewModel _viewModel;
        private DateTime _timer;
        private string _oldParameter;
        private int _tapNumber;

        public ClickOnNumberButtonCommand(PaySaleViewModel viewModel)
        {
            _viewModel = viewModel;
            _tapNumber = 0;
            _timer = DateTime.Now;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            PaySale(parameter.ToString());
        }
        private void PaySale(string parameter)
        {
            switch (_viewModel.Focus)
            {
                case FocusEnumeration.BuyerId:
                    if (parameter == "backspace")
                    {
                        if (_viewModel.BuyerId.Any())
                        {
                            _viewModel.BuyerId = _viewModel.BuyerId.Remove(_viewModel.BuyerId.Length - 1);
                        }
                    }
                    else
                    {
                        _viewModel.BuyerId += parameter;
                    }
                    break;
                case FocusEnumeration.Cash:
                    if (parameter == "backspace")
                    {
                        if (_viewModel.Cash.Any())
                        {
                            _viewModel.Cash = _viewModel.Cash.Remove(_viewModel.Cash.Length - 1);
                        }
                        else
                        {
                            _viewModel.Cash = "0";
                        }
                    }
                    else
                    {
                        if(_viewModel.Cash == "0" && parameter != ".")
                        {
                            _viewModel.Cash = parameter;
                        }
                        else
                        {
                            if(parameter == ".")
                            {
                                _viewModel.Cash += parameter + "0";
                            }
                            else
                            {
                                if (_viewModel.Cash.Length > 1)
                                {
                                    if (_viewModel.Cash[_viewModel.Cash.Length - 2] == '.' &&
                                        _viewModel.Cash[_viewModel.Cash.Length - 1] == '0' &&
                                        parameter != "0")
                                    {
                                        _viewModel.Cash = _viewModel.Cash.Replace(_viewModel.Cash[_viewModel.Cash.Length - 1],
                                            parameter.First());
                                    }
                                    else
                                    {
                                        if (_viewModel.Cash[_viewModel.Cash.Length - 2] == '.' &&
                                        _viewModel.Cash[_viewModel.Cash.Length - 1] == '0' &&
                                        parameter == "0")
                                        {

                                        }
                                        else
                                        {
                                            _viewModel.Cash += parameter;
                                        }
                                    }
                                }
                                else
                                {
                                    _viewModel.Cash += parameter;
                                }
                            }
                        }
                    }
                    break;
                case FocusEnumeration.Card:
                    if (parameter == "backspace")
                    {
                        if (_viewModel.Card.Any())
                        {
                            _viewModel.Card = _viewModel.Card.Remove(_viewModel.Card.Length - 1);
                        }
                        else
                        {
                            _viewModel.Card = "0";
                        }
                    }
                    else
                    {
                        if (_viewModel.Card == "0" && parameter != ".")
                        {
                            _viewModel.Card = parameter;
                        }
                        else
                        {
                            if (parameter == ".")
                            {
                                _viewModel.Card += parameter + "0";
                            }
                            else
                            {
                                if (_viewModel.Card.Length > 1)
                                {
                                    if (_viewModel.Card[_viewModel.Card.Length - 2] == '.' &&
                                        _viewModel.Card[_viewModel.Card.Length - 1] == '0' &&
                                        parameter != "0")
                                    {
                                        _viewModel.Card = _viewModel.Card.Replace(_viewModel.Card[_viewModel.Card.Length - 1],
                                            parameter.First());
                                    }
                                    else
                                    {
                                        if (_viewModel.Card[_viewModel.Card.Length - 2] == '.' &&
                                        _viewModel.Card[_viewModel.Card.Length - 1] == '0' &&
                                        parameter == "0")
                                        {

                                        }
                                        else
                                        {
                                            _viewModel.Card += parameter;
                                        }
                                    }
                                }
                                else
                                {
                                    _viewModel.Card += parameter;
                                }
                            }
                        }
                    }
                    break;
                case FocusEnumeration.Check:
                    if (parameter == "backspace")
                    {
                        if (_viewModel.Check.Any())
                        {
                            _viewModel.Check = _viewModel.Check.Remove(_viewModel.Check.Length - 1);
                        }
                        else
                        {
                            _viewModel.Check = "0";
                        }
                    }
                    else
                    {
                        if (_viewModel.Check == "0" && parameter != ".")
                        {
                            _viewModel.Check = parameter;
                        }
                        else
                        {
                            if (parameter == ".")
                            {
                                _viewModel.Check += parameter + "0";
                            }
                            else
                            {
                                if (_viewModel.Check.Length > 1)
                                {
                                    if (_viewModel.Check[_viewModel.Check.Length - 2] == '.' &&
                                        _viewModel.Check[_viewModel.Check.Length - 1] == '0' &&
                                        parameter != "0")
                                    {
                                        _viewModel.Check = _viewModel.Check.Replace(_viewModel.Check[_viewModel.Check.Length - 1],
                                            parameter.First());
                                    }
                                    else
                                    {
                                        if (_viewModel.Check[_viewModel.Check.Length - 2] == '.' &&
                                        _viewModel.Check[_viewModel.Check.Length - 1] == '0' &&
                                        parameter == "0")
                                        {

                                        }
                                        else
                                        {
                                            _viewModel.Check += parameter;
                                        }
                                    }
                                }
                                else
                                {
                                    _viewModel.Check += parameter;
                                }
                            }
                        }
                    }
                    break;
                case FocusEnumeration.Other:
                    if (parameter == "backspace")
                    {
                        if (_viewModel.Other.Any())
                        {
                            _viewModel.Other = _viewModel.Other.Remove(_viewModel.Other.Length - 1);
                        }
                        else
                        {
                            _viewModel.Other = "0";
                        }
                    }
                    else
                    {
                        if (_viewModel.Other == "0" && parameter != ".")
                        {
                            _viewModel.Other = parameter;
                        }
                        else
                        {
                            if (parameter == ".")
                            {
                                _viewModel.Other += parameter + "0";
                            }
                            else
                            {
                                if (_viewModel.Other.Length > 1)
                                {
                                    if (_viewModel.Other[_viewModel.Other.Length - 2] == '.' &&
                                        _viewModel.Other[_viewModel.Other.Length - 1] == '0' &&
                                        parameter != "0")
                                    {
                                        _viewModel.Other = _viewModel.Other.Replace(_viewModel.Other[_viewModel.Other.Length - 1],
                                            parameter.First());
                                    }
                                    else
                                    {
                                        if (_viewModel.Other[_viewModel.Other.Length - 2] == '.' &&
                                        _viewModel.Other[_viewModel.Other.Length - 1] == '0' &&
                                        parameter == "0")
                                        {

                                        }
                                        else
                                        {
                                            _viewModel.Other += parameter;
                                        }
                                    }
                                }
                                else
                                {
                                    _viewModel.Other += parameter;
                                }
                            }
                        }
                    }
                    break;
                case FocusEnumeration.Voucher:
                    if (parameter == "backspace")
                    {
                        if (_viewModel.Voucher.Any())
                        {
                            _viewModel.Voucher = _viewModel.Voucher.Remove(_viewModel.Voucher.Length - 1);
                        }
                        else
                        {
                            _viewModel.Voucher = "0";
                        }
                    }
                    else
                    {
                        if (_viewModel.Voucher == "0" && parameter != ".")
                        {
                            _viewModel.Voucher = parameter;
                        }
                        else
                        {
                            if (parameter == ".")
                            {
                                _viewModel.Voucher += parameter + "0";
                            }
                            else
                            {
                                if (_viewModel.Voucher.Length > 1)
                                {
                                    if (_viewModel.Voucher[_viewModel.Voucher.Length - 2] == '.' &&
                                        _viewModel.Voucher[_viewModel.Voucher.Length - 1] == '0' &&
                                        parameter != "0")
                                    {
                                        _viewModel.Voucher = _viewModel.Voucher.Replace(_viewModel.Voucher[_viewModel.Voucher.Length - 1],
                                            parameter.First());
                                    }
                                    else
                                    {
                                        if (_viewModel.Voucher[_viewModel.Voucher.Length - 2] == '.' &&
                                        _viewModel.Voucher[_viewModel.Voucher.Length - 1] == '0' &&
                                        parameter == "0")
                                        {

                                        }
                                        else
                                        {
                                            _viewModel.Voucher += parameter;
                                        }
                                    }
                                }
                                else
                                {
                                    _viewModel.Voucher += parameter;
                                }
                            }
                        }
                    }
                    break;
                case FocusEnumeration.WireTransfer:
                    if (parameter == "backspace")
                    {
                        if (_viewModel.WireTransfer.Any())
                        {
                            _viewModel.WireTransfer = _viewModel.WireTransfer.Remove(_viewModel.WireTransfer.Length - 1);
                        }
                        else
                        {
                            _viewModel.WireTransfer = "0";
                        }
                    }
                    else
                    {
                        if (_viewModel.WireTransfer == "0" && parameter != ".")
                        {
                            _viewModel.WireTransfer = parameter;
                        }
                        else
                        {
                            if (parameter == ".")
                            {
                                _viewModel.WireTransfer += parameter + "0";
                            }
                            else
                            {
                                if (_viewModel.WireTransfer.Length > 1)
                                {
                                    if (_viewModel.WireTransfer[_viewModel.WireTransfer.Length - 2] == '.' &&
                                        _viewModel.WireTransfer[_viewModel.WireTransfer.Length - 1] == '0' &&
                                        parameter != "0")
                                    {
                                        _viewModel.WireTransfer = _viewModel.WireTransfer.Replace(_viewModel.WireTransfer[_viewModel.WireTransfer.Length - 1],
                                            parameter.First());
                                    }
                                    else
                                    {
                                        if (_viewModel.WireTransfer[_viewModel.WireTransfer.Length - 2] == '.' &&
                                        _viewModel.WireTransfer[_viewModel.WireTransfer.Length - 1] == '0' &&
                                        parameter == "0")
                                        {

                                        }
                                        else
                                        {
                                            _viewModel.WireTransfer += parameter;
                                        }
                                    }
                                }
                                else
                                {
                                    _viewModel.WireTransfer += parameter;
                                }
                            }
                        }
                    }
                    break;
                case FocusEnumeration.MobileMoney:
                    if (parameter == "backspace")
                    {
                        if (_viewModel.MobileMoney.Any())
                        {
                            _viewModel.MobileMoney = _viewModel.MobileMoney.Remove(_viewModel.MobileMoney.Length - 1);
                        }
                        else
                        {
                            _viewModel.MobileMoney = "0";
                        }
                    }
                    else
                    {
                        if (_viewModel.MobileMoney == "0" && parameter != ".")
                        {
                            _viewModel.MobileMoney = parameter;
                        }
                        else
                        {
                            if (parameter == ".")
                            {
                                _viewModel.MobileMoney += parameter + "0";
                            }
                            else
                            {
                                _viewModel.MobileMoney += parameter;
                            }
                        }
                    }
                    break;
            }
        }
        private string GetChar(string parameter, string value)
        {
            switch (parameter)
            {
                case "1":
                    switch (_tapNumber)
                    {
                        case 0:
                            value += "A";
                            break;
                        case 1:
                            value = value.Remove(value.Length - 1, 1);
                            value += "B";
                            break;
                        case 2:
                            value = value.Remove(value.Length - 1, 1);
                            value += "C";
                            break;
                        case 3:
                            value = value.Remove(value.Length - 1, 1);
                            value += "1";
                            break;
                    }
                    break;
                case "2":
                    switch (_tapNumber)
                    {
                        case 0:
                            value += "D";
                            break;
                        case 1:
                            value = value.Remove(value.Length - 1, 1);
                            value += "E";
                            break;
                        case 2:
                            value = value.Remove(value.Length - 1, 1);
                            value += "F";
                            break;
                        case 3:
                            value = value.Remove(value.Length - 1, 1);
                            value += "2";
                            break;
                    }
                    break;
                case "3":
                    switch (_tapNumber)
                    {
                        case 0:
                            value += "G";
                            break;
                        case 1:
                            value = value.Remove(value.Length - 1, 1);
                            value += "H";
                            break;
                        case 2:
                            value = value.Remove(value.Length - 1, 1);
                            value += "I";
                            break;
                        case 3:
                            value = value.Remove(value.Length - 1, 1);
                            value += "3";
                            break;
                    }
                    break;
                case "4":
                    switch (_tapNumber)
                    {
                        case 0:
                            value += "J";
                            break;
                        case 1:
                            value = value.Remove(value.Length - 1, 1);
                            value += "K";
                            break;
                        case 2:
                            value = value.Remove(value.Length - 1, 1);
                            value += "L";
                            break;
                        case 3:
                            value = value.Remove(value.Length - 1, 1);
                            value += "4";
                            break;
                    }
                    break;
                case "5":
                    switch (_tapNumber)
                    {
                        case 0:
                            value += "M";
                            break;
                        case 1:
                            value = value.Remove(value.Length - 1, 1);
                            value += "N";
                            break;
                        case 2:
                            value = value.Remove(value.Length - 1, 1);
                            value += "O";
                            break;
                        case 3:
                            value = value.Remove(value.Length - 1, 1);
                            value += "5";
                            break;
                    }
                    break;
                case "6":
                    switch (_tapNumber)
                    {
                        case 0:
                            value += "P";
                            break;
                        case 1:
                            value = value.Remove(value.Length - 1, 1);
                            value += "Q";
                            break;
                        case 2:
                            value = value.Remove(value.Length - 1, 1);
                            value += "R";
                            break;
                        case 3:
                            value = value.Remove(value.Length - 1, 1);
                            value += "6";
                            break;
                    }
                    break;
                case "7":
                    switch (_tapNumber)
                    {
                        case 0:
                            value += "S";
                            break;
                        case 1:
                            value = value.Remove(value.Length - 1, 1);
                            value += "T";
                            break;
                        case 2:
                            value = value.Remove(value.Length - 1, 1);
                            value += "U";
                            break;
                        case 3:
                            value = value.Remove(value.Length - 1, 1);
                            value += "7";
                            break;
                    }
                    break;
                case "8":
                    switch (_tapNumber)
                    {
                        case 0:
                            value += "V";
                            break;
                        case 1:
                            value = value.Remove(value.Length - 1, 1);
                            value += "W";
                            break;
                        case 2:
                            value = value.Remove(value.Length - 1, 1);
                            value += "X";
                            break;
                        case 3:
                            value = value.Remove(value.Length - 1, 1);
                            value += "8";
                            break;
                    }
                    break;
                case "9":
                    switch (_tapNumber)
                    {
                        case 0:
                            value += "Y";
                            break;
                        case 1:
                            value = value.Remove(value.Length - 1, 1);
                            value += "Z";
                            break;
                        case 2:
                            value = value.Remove(value.Length - 1, 1);
                            value += "9";
                            break;
                        case 3:
                            value += "Y";
                            break;
                    }
                    break;
                case "0":
                    value += "0";
                    break;
            }

            return value;
        }
    }
}