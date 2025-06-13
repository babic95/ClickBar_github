using ClickBar.ViewModels.AppMain.Statistic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using ClickBar.Models.Sale;
using ClickBar_Logging;

namespace ClickBar.Commands.AppMain.Statistic.InventoryStatus.Redosled
{
    public class MoveToDownItemCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private InventoryStatusViewModel _currentViewModel;

        public MoveToDownItemCommand(InventoryStatusViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            try
            {
                var currentItem = _currentViewModel.CurrentRedosledItem;

                if (currentItem != null)
                {
                    var previousItem = _currentViewModel.RedosledItems
                        .OrderBy(s => s.Rb)
                        .Where(s => s.Rb > currentItem.Rb)
                        .FirstOrDefault();

                    if (previousItem != null)
                    {
                        var tempRb = currentItem.Rb;
                        currentItem.Rb = previousItem.Rb;
                        previousItem.Rb = tempRb;

                        _currentViewModel.RedosledItems = new ObservableCollection<Item>(
                            _currentViewModel.RedosledItems.OrderBy(s => s.Rb).ToList());
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("MoveToDownItemCommand -> Greska prilikom premestanja na dole artikal: ", ex);
                MessageBox.Show("Desila se greška.\nObratite se serviseru!",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}