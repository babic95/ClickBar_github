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

namespace ClickBar.Commands.AppMain.Statistic.InventoryStatus.Redosled
{
    public class MoveToUpGroupCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private InventoryStatusViewModel _currentViewModel;

        public MoveToUpGroupCommand(InventoryStatusViewModel currentViewModel)
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
                var currentGroup = _currentViewModel.CurrentRedosledGroups;

                if (currentGroup != null)
                {
                    var previousGroup = _currentViewModel.RedosledGroups
                        .OrderBy(s => s.Rb)
                        .Where(s => s.Rb < currentGroup.Rb)
                        .LastOrDefault();

                    if (previousGroup != null)
                    {
                        var tempRb = currentGroup.Rb;
                        currentGroup.Rb = previousGroup.Rb;
                        previousGroup.Rb = tempRb;

                        _currentViewModel.RedosledGroups = new ObservableCollection<GroupItems>(
                            _currentViewModel.RedosledGroups.OrderBy(s => s.Rb).ToList());
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("MoveToUpGroupCommand -> Greska prilikom premestanja na gore grupe: ", ex);
                MessageBox.Show("Desila se greška.\nObratite se serviseru!",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}