using ClickBar.Models.AppMain.Statistic.Radnici;
using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar.Views.AppMain.AuxiliaryWindows.Statistic.Radnici;
using ClickBar_DatabaseSQLManager;
using ClickBar_Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ClickBar.Commands.AppMain.Statistic.Radnici
{
    public class WhatDidWorkerSellCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private RadniciViewModel _currentViewModel;

        public WhatDidWorkerSellCommand(RadniciViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            try
            {
                var radnikDB = _currentViewModel.DbContext.Cashiers.Find(parameter.ToString());

                if (radnikDB != null)
                {
                    var items = _currentViewModel.DbContext.Invoices.Join(_currentViewModel.DbContext.ItemInvoices,
                        invoice => invoice.Id,
                        item => item.InvoiceId,
                        (invoice, item) => new { Invoice = invoice, Item = item })
                        .Where(i => i.Invoice.SdcDateTime.HasValue &&
                        i.Invoice.SdcDateTime.Value.Date == DateTime.Now.Date &&
                        !string.IsNullOrEmpty(i.Invoice.Cashier) &&
                        i.Invoice.Cashier == radnikDB.Id);

                    if (items != null &&
                        items.Any())
                    {
                        _currentViewModel.WhatDidWorkerSells = new ObservableCollection<WhatDidWorkerSell>();
                        _currentViewModel.Total = 0;

                        await items.ForEachAsync(item =>
                        {
                            var itemDB = _currentViewModel.DbContext.Items.Find(item.Item.ItemCode);
                            if (itemDB != null &&
                            item.Item.TotalAmout != null)
                            {
                                WhatDidWorkerSell whatDidWorkerSell = new WhatDidWorkerSell()
                                {
                                    Item = item.Item,
                                    Invoice = item.Invoice,
                                    ItemName = itemDB.Name,
                                    ItemJm = itemDB.Jm
                                };

                                _currentViewModel.WhatDidWorkerSells.Add(whatDidWorkerSell);

                                _currentViewModel.Total += item.Item.TotalAmout.Value;
                            }
                        });

                        WhatDidWorkerSellWindow whatDidWorkerSellWindow = new WhatDidWorkerSellWindow(_currentViewModel);
                        whatDidWorkerSellWindow.ShowDialog();
                    }
                    else
                    {
                        MessageBox.Show("Radnik nije nista prodao.",
                            "Informacija",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Desila se neočekivana greška.\nObratite se serviseru.",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Log.Error("Desila se neočekivana greška.\nObratite se serviseru.", ex);
            }
        }
    }
}