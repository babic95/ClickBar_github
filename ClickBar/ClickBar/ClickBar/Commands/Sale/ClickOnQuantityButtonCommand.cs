using ClickBar.Enums;
using ClickBar.ViewModels;
using ClickBar_DatabaseSQLManager;
using DocumentFormat.OpenXml.Office2010.CustomUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClickBar.Commands.Sale
{
    public class ClickOnQuantityButtonCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private SaleViewModel _viewModel;

        public ClickOnQuantityButtonCommand(SaleViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            if (parameter is string)
            {
                string parameterString = (string) parameter;

                parameterString = parameterString.Replace(',', '.');

                if (_viewModel.FirstChangeQuantity &&
                    !parameterString.Contains("backspace"))
                {
                    _viewModel.FirstChangeQuantity = false;
                    _viewModel.Quantity = parameterString;
                }
                else
                {
                    if (parameterString.Contains("backspace"))
                    {
                        if (_viewModel.Quantity.Length == 1)
                        {
                            _viewModel.Quantity = "1";
                            _viewModel.FirstChangeQuantity = true;
                        }
                        else 
                        {
                            _viewModel.Quantity = _viewModel.Quantity.Substring(0, _viewModel.Quantity.Length - 1); 
                        }
                    }
                    else if (parameterString.Contains(","))
                    {
                        _viewModel.Quantity += ".";
                    }
                    else
                    {
                        _viewModel.Quantity += parameterString;
                    }
                } 
            }
        }
    }
}
