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
using ClickBar.Models.Sale;

namespace ClickBar.Commands.AppMain.Statistic.InventoryStatus.Redosled
{
    public class MoveToDownGroupCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private InventoryStatusViewModel _currentViewModel;

        public MoveToDownGroupCommand(InventoryStatusViewModel currentViewModel)
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
                    var nextGroup = _currentViewModel.RedosledGroups
                        .OrderBy(s => s.Rb)
                        .Where(s => s.Rb > currentGroup.Rb)
                        .FirstOrDefault();

                    if (nextGroup != null)
                    {
                        var tempRb = currentGroup.Rb;
                        currentGroup.Rb = nextGroup.Rb;
                        nextGroup.Rb = tempRb;

                        _currentViewModel.RedosledGroups = new ObservableCollection<GroupItems>(
                            _currentViewModel.RedosledGroups.OrderBy(s => s.Rb).ToList());
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("MoveToDownGroupCommand -> Greska prilikom premestanja na dole grupe: ", ex);
                MessageBox.Show("Desila se greška.\nObratite se serviseru!",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}