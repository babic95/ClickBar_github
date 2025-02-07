using ClickBar.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using ClickBar.Models.Sale;
using ClickBar.Views.Sale;

namespace ClickBar.Commands.Sale
{
    public class OpenListaZeljaCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private SaleViewModel _currentViewModel;

        public OpenListaZeljaCommand(SaleViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if(parameter is ItemInvoice itemInvoice)
            {
                _currentViewModel.WindowZelja = new ItemInvoiceAddZeljeWindow(itemInvoice);
                _currentViewModel.WindowZelja.ShowDialog();
            }
        }
    }
}
