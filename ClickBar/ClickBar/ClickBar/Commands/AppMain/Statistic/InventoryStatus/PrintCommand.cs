using ClickBar.Models.AppMain.Statistic;
using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar_Common.Models.Statistic;
using ClickBar_Printer;
using ClickBar_Printer.PaperFormat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClickBar.Commands.AppMain.Statistic
{
    public class PrintCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private InventoryStatusViewModel _currentViewModel;

        public PrintCommand(InventoryStatusViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            List<InvertoryGlobal> invertoryGlobals = new List<InvertoryGlobal>();

            IEnumerable<Invertory>? items = null;
            if (_currentViewModel.CurrentGroup.Id > 0)
            {
                items = _currentViewModel.InventoryStatusAll.Where(item => item.IdGroupItems == _currentViewModel.CurrentGroup.Id);
            }
            else
            {
                items = _currentViewModel.InventoryStatusAll;
            }

            if (items != null &&
                items.Any())
            {
                items.ToList().ForEach(inventory =>
                {
                    decimal inputUnitPrice = inventory.Item.InputUnitPrice != null && inventory.Item.InputUnitPrice.HasValue ? 
                    inventory.Item.InputUnitPrice.Value : 0;

                    invertoryGlobals.Add(new InvertoryGlobal()
                    {
                        Id = inventory.Item.Id,
                        Name = inventory.Item.Name,
                        Jm = inventory.Item.Jm,
                        InputUnitPrice = inputUnitPrice,
                        SellingUnitPrice = inventory.Item.SellingUnitPrice,
                        Quantity = inventory.Quantity,
                        TotalAmout = inventory.TotalAmout
                    });
                });

                PrinterManager.Instance.PrintInventoryStatus(invertoryGlobals, $"STANJE ZALUHA - {_currentViewModel.CurrentGroup.Name}", DateTime.Now);

                if (_currentViewModel.PrintTypeWindow.Activate())
                {
                    _currentViewModel.PrintTypeWindow.Close();
                }
            }
        }
    }
}