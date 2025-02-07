using ClickBar.Models.AppMain.Statistic;
using ClickBar.Models.Sale;
using ClickBar.ViewModels;
using ClickBar_Database;
using ClickBar_Database.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ClickBar.Commands.Sale
{
    public class SelectItemCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private SaleViewModel _viewModel;

        public SelectItemCommand(SaleViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            string idItem = (string)parameter;

            decimal quantity = 1;
            try
            {
                quantity = Convert.ToDecimal(_viewModel.Quantity);
                quantity = Math.Round(quantity, 3);
            }
            catch
            {
                MessageBox.Show("Unesite ispravnu količinu!", "Ne ispravna količina", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ItemDB? itemDB = _viewModel.AllItems.Where(item => item.Id == idItem).FirstOrDefault();

            if(itemDB is null)
            {
                return;
            }

            ItemInvoice? itemInvoice = _viewModel.ItemsInvoice.Where(item => item.Item.Id == itemDB.Id).FirstOrDefault();

            ObservableCollection<ItemZelja> fixneZelje = new ObservableCollection<ItemZelja>();
            //using (SqliteDbContext sqliteDbContext = new SqliteDbContext())
            //{
            //    var fixneZeljeDB = sqliteDbContext.Zelje.Where(z => z.ItemId == itemDB.Id);

            //    if (fixneZeljeDB != null &&
            //        fixneZeljeDB.Any())
            //    {
            //        fixneZeljeDB.ForEachAsync(z =>
            //        {
            //            ItemZelja itemZelja = new ItemZelja()
            //            {
            //                Id = z.Id,
            //                ItemId = z.ItemId,
            //                Zelja = z.Zelja
            //            };

            //            fixneZelje.Add(itemZelja);
            //        });
            //    }
            //}

            if (itemInvoice is not null)
            {
                itemInvoice.Quantity += quantity;

                itemInvoice.TotalAmout += itemDB.SellingUnitPrice * quantity;

                if(itemInvoice.Zelje is null)
                {
                    itemInvoice.Zelje = new ObservableCollection<Zelja>();
                }

                var zeljeCounter = itemInvoice.Zelje.Count;

                for (int i = zeljeCounter + 1; i <= zeljeCounter + quantity; i++)
                {
                    itemInvoice.Zelje.Add(new Zelja(i, itemDB.Name, fixneZelje));
                }

                _viewModel.TotalAmount += itemDB.SellingUnitPrice * quantity;
            }
            else
            {
                var item = new ItemInvoice(new Item(itemDB), quantity) 
                {
                    Zelje = new ObservableCollection<Zelja>()
                };

                for (int i = 1; i <= quantity; i++)
                {
                    item.Zelje.Add(new Zelja(i, itemDB.Name, fixneZelje));
                }

                _viewModel.ItemsInvoice.Add(item);

                _viewModel.TotalAmount += itemDB.SellingUnitPrice * quantity;
            }

            _viewModel.SendToDisplay(itemDB.Name, string.Format("{0:#,##0.00}", itemDB.SellingUnitPrice).Replace(',', '#').Replace('.', ',').Replace('#', '.'));

            if (_viewModel.ItemsInvoice.Any())
            {
                _viewModel.HookOrderEnable = true;
            }
            else
            {
                _viewModel.HookOrderEnable = false;
            }

            _viewModel.FirstChangeQuantity = true;
        }
    }
}
