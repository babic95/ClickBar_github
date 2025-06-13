using ClickBar.Models.Sale;
using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar.Views.AppMain.AuxiliaryWindows.Statistic.InventoryStatus.Redosled;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using ClickBar_Logging;

namespace ClickBar.Commands.AppMain.Statistic.InventoryStatus.Redosled
{
    public class OpenRedosledGroupItemsCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private InventoryStatusViewModel _currentViewModel;

        public OpenRedosledGroupItemsCommand(InventoryStatusViewModel currentViewModel)
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
                _currentViewModel.RedosledSupergroups = new ObservableCollection<Supergroup>();

                var supergroups = _currentViewModel.DbContext.Supergroups
                    .OrderBy(s => s.Rb)
                    .Select(s => new Supergroup(s))
                    .ToList();

                if (supergroups.Any())
                {
                    supergroups.ForEach(s => _currentViewModel.RedosledSupergroups.Add(s));
                    _currentViewModel.CurrentRedosledSupergroupForGroup = null;

                    _currentViewModel.RasporedWindow = new RedosledGroupWindow(_currentViewModel);
                    _currentViewModel.RasporedWindow.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                Log.Error("OpenRedosledGroupItemsCommand -> Greska prilikom otvaranja prozora za raspored grupa: ", ex);
                MessageBox.Show("Greška prilikom otvaranja prozora za raspored grupa.\nObratite se serviseru!",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}