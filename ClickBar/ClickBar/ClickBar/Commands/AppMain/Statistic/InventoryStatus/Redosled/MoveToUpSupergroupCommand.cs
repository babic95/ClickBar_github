using ClickBar.ViewModels.AppMain.Statistic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using ClickBar_Logging;
using ClickBar.Models.Sale;
using System.Collections.ObjectModel;

namespace ClickBar.Commands.AppMain.Statistic.InventoryStatus.Redosled
{
    public class MoveToUpSupergroupCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private InventoryStatusViewModel _currentViewModel;

        public MoveToUpSupergroupCommand(InventoryStatusViewModel currentViewModel)
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
                var currentSupergroup = _currentViewModel.CurrentRedosledSupergroups;

                if (currentSupergroup != null)
                {
                    var previousSupergroup = _currentViewModel.RedosledSupergroups
                        .OrderBy(s => s.Rb)
                        .Where(s => s.Rb < currentSupergroup.Rb)
                        .LastOrDefault();

                    if (previousSupergroup != null)
                    {
                        var tempRb = currentSupergroup.Rb;
                        currentSupergroup.Rb = previousSupergroup.Rb;
                        previousSupergroup.Rb = tempRb;
                        _currentViewModel.RedosledSupergroups = new ObservableCollection<Supergroup>(
                            _currentViewModel.RedosledSupergroups.OrderBy(s => s.Rb).ToList());
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("MoveToUpSupergroupCommand -> Greska prilikom premestanja na gore nadgupe: ", ex);
                MessageBox.Show("Desila se greška.\nObratite se serviseru!",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}