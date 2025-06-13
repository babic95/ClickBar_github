using ClickBar.Models.Sale;
using ClickBar.ViewModels.AppMain.Statistic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using ClickBar_Logging;
using ClickBar.Views.AppMain.AuxiliaryWindows.Statistic.InventoryStatus.Redosled;

namespace ClickBar.Commands.AppMain.Statistic.InventoryStatus.Redosled
{
    public class OpenRedosledItemsCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private InventoryStatusViewModel _currentViewModel;

        public OpenRedosledItemsCommand(InventoryStatusViewModel currentViewModel)
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
                _currentViewModel.RedosledGroups = new ObservableCollection<GroupItems>();

                var groups = _currentViewModel.DbContext.ItemGroups
                    .OrderBy(g => g.Rb)
                    .Select(g => new GroupItems(g))
                    .ToList();

                if (groups.Any())
                {
                    groups.ForEach(s => _currentViewModel.RedosledGroups.Add(s));
                    _currentViewModel.CurrentRedosledGroupForItem = null;

                    _currentViewModel.RasporedWindow = new RedosledItemsWindow(_currentViewModel);
                    _currentViewModel.RasporedWindow.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                Log.Error("OpenRedosledItemsCommand -> Greska prilikom otvaranja prozora za raspored artikala: ", ex);
                MessageBox.Show("Greška prilikom otvaranja prozora za raspored artikala.\nObratite se serviseru!",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}