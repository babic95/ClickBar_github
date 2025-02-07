using ClickBar.Models.AppMain.Statistic.Knjizenje;
using ClickBar.Models.Sale;
using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar_Database.Models;
using ClickBar_Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using ClickBar.Views.AppMain.AuxiliaryWindows.Statistic.Knjizenje;
using ClickBar.ViewModels;

namespace ClickBar.Commands.AppMain.Statistic.Knjizenje
{
    public class OpenItemsInInvoicesCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ViewModelBase _currentViewModel;

        public OpenItemsInInvoicesCommand(ViewModelBase currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (_currentViewModel is KnjizenjeViewModel)
            {
                KnjizenjePazara(parameter);
            }
            else if (_currentViewModel is PregledPazaraViewModel)
            {
                PregledPazara(parameter);
            }
            else
            {
                return;
            }
        }
        private void KnjizenjePazara(object parameter)
        {
            KnjizenjeViewModel knjizenjeViewModel = (KnjizenjeViewModel)_currentViewModel;

            if (parameter != null &&
                parameter is string)
            {
                string invoiceId = (string)parameter;

                using (SqliteDbContext sqliteDbContext = new SqliteDbContext())
                {

                    var invoice = knjizenjeViewModel.Invoices.FirstOrDefault(inv => inv.Id == invoiceId);

                    if (invoice != null)
                    {
                        knjizenjeViewModel.ItemsInInvoice = new ObservableCollection<ItemInvoice>();

                        var itemsDB = sqliteDbContext.ItemInvoices.Where(inv => inv.InvoiceId == invoiceId &&
                                    (inv.IsSirovina == null || inv.IsSirovina == 0));

                        if (itemsDB != null &&
                            itemsDB.Any())
                        {
                            itemsDB.ToList().ForEach(itemInvoiceDB =>
                            {

                                if (!string.IsNullOrEmpty(itemInvoiceDB.ItemCode))
                                {
                                    var itemDB = sqliteDbContext.Items.FirstOrDefault(item => item.Id == itemInvoiceDB.ItemCode);

                                    if (itemDB != null &&
                                    itemInvoiceDB.Quantity.HasValue)
                                    {
                                        Item item = new Item(itemDB);
                                        ItemInvoice itemInvoice = new ItemInvoice(item, itemInvoiceDB);
                                        knjizenjeViewModel.ItemsInInvoice.Add(itemInvoice);
                                    }
                                }
                            });
                        }

                        if (knjizenjeViewModel.Window != null &&
                            knjizenjeViewModel.Window.IsActive)
                        {
                            knjizenjeViewModel.Window.Close();
                        }
                        knjizenjeViewModel.Window = new ItemsInInvoiceWindow(knjizenjeViewModel);
                        knjizenjeViewModel.Window.ShowDialog();
                    }
                }
            }
        }
        private void PregledPazara(object parameter)
        {
            PregledPazaraViewModel pregledPazaraViewModel = (PregledPazaraViewModel)_currentViewModel;

            if (parameter != null &&
                parameter is string)
            {
                string invoiceId = (string)parameter;

                using (SqliteDbContext sqliteDbContext = new SqliteDbContext())
                {

                    var invoice = pregledPazaraViewModel.Invoices.FirstOrDefault(inv => inv.Id == invoiceId);

                    if (invoice != null)
                    {
                        pregledPazaraViewModel.ItemsInInvoice = new ObservableCollection<ItemInvoice>();

                        var itemsDB = sqliteDbContext.ItemInvoices.Where(inv => inv.InvoiceId == invoiceId);

                        if (itemsDB != null &&
                            itemsDB.Any())
                        {
                            itemsDB.ToList().ForEach(itemInvoiceDB =>
                            {

                                if (!string.IsNullOrEmpty(itemInvoiceDB.ItemCode))
                                {
                                    var itemDB = sqliteDbContext.Items.FirstOrDefault(item => item.Id == itemInvoiceDB.ItemCode);

                                    if (itemDB != null &&
                                    itemInvoiceDB.Quantity.HasValue)
                                    {
                                        Item item = new Item(itemDB);
                                        ItemInvoice itemInvoice = new ItemInvoice(item, itemInvoiceDB);
                                        pregledPazaraViewModel.ItemsInInvoice.Add(itemInvoice);
                                    }
                                }
                            });
                        }

                        if (pregledPazaraViewModel.Window != null &&
                            pregledPazaraViewModel.Window.IsActive)
                        {
                            pregledPazaraViewModel.Window.Close();
                        }
                        pregledPazaraViewModel.Window = new ItemsInInvoiceWindow(pregledPazaraViewModel);
                        pregledPazaraViewModel.Window.ShowDialog();
                    }
                }
            }
        }
    }
}