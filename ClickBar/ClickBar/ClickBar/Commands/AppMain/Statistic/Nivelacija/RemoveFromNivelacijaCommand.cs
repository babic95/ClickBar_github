using ClickBar.ViewModels.AppMain.Statistic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ClickBar.Commands.AppMain.Statistic.Nivelacija
{
    public class RemoveFromNivelacijaCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private NivelacijaViewModel _currentViewModel;

        public RemoveFromNivelacijaCommand(NivelacijaViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            MessageBoxResult result = MessageBox.Show("Da li ste sigurni da želite da izbacite artikal iz nivelacije?",
                           "Brisanje artikla iz nivelacije",
                           MessageBoxButton.YesNo,
                           MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                if (parameter != null &&
                parameter is string)
                {
                    var item = _currentViewModel.CurrentNivelacija.NivelacijaItems.FirstOrDefault(i => i.IdItem == parameter.ToString());

                    if (item != null)
                    {
                        _currentViewModel.TotalNivelacija -= item.NewTotalValue - item.OldTotalValue;
                        _currentViewModel.TotalPdvNivelacija -= item.NewTotalPDV - item.OldTotalPDV;
                        _currentViewModel.TotalNewNivelacija -= item.NewTotalValue;
                        _currentViewModel.TotalOldNivelacija -= item.OldTotalValue;

                        _currentViewModel.CurrentNivelacija.NivelacijaItems.Remove(item);
                    }
                }
            }
        }
    }
}