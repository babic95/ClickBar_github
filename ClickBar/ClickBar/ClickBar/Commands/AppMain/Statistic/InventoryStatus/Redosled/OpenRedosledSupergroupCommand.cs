using ClickBar.Models.AppMain.Statistic;
using ClickBar.Models.Sale;
using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar.Views.AppMain.AuxiliaryWindows.Statistic.InventoryStatus.Redosled;
using ClickBar_Common.Models.Statistic;
using ClickBar_Logging;
using ClickBar_Printer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ClickBar.Commands.AppMain.Statistic.InventoryStatus.Redosled
{
    public class OpenRedosledSupergroupCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private InventoryStatusViewModel _currentViewModel;

        public OpenRedosledSupergroupCommand(InventoryStatusViewModel currentViewModel)
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
                    _currentViewModel.CurrentRedosledSupergroups = null;

                    _currentViewModel.RasporedWindow = new RedosledSupergroupWindow(_currentViewModel);
                    _currentViewModel.RasporedWindow.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                Log.Error("OpenRedosledSupergroupCommand -> Greska prilikom otvaranja prozora za raspored nadgupa: ", ex);
                MessageBox.Show("Greška prilikom otvaranja prozora za raspored nadgupa.\nObratite se serviseru!",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}