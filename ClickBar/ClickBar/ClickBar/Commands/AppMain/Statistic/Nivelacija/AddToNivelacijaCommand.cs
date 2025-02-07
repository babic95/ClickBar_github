using ClickBar.Models.AppMain.Statistic;
using ClickBar.ViewModels.AppMain.Statistic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ClickBar.Commands.AppMain.Statistic.Nivelacija
{
    public class AddToNivelacijaCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private NivelacijaViewModel _currentViewModel;

        public AddToNivelacijaCommand(NivelacijaViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            MessageBoxResult result = MessageBox.Show("Da li ste sigurni da želite da dodate ovaj artikal u nivelaciju?\nProverite još jednom Nova JC.",
                           "Dodavanje artikla u nivelaciju",
                           MessageBoxButton.YesNo,
                           MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                if (_currentViewModel.CurrentItemsToNivelacija != null)
                {
                    if (_currentViewModel.CurrentNivelacija != null)
                    {
                        if (_currentViewModel.CurrentNivelacija.NivelacijaItems == null)
                        {
                            _currentViewModel.CurrentNivelacija.NivelacijaItems = new ObservableCollection<NivelacijaItem>();
                        }

                        if(_currentViewModel.CurrentNivelacija.NivelacijaItems.FirstOrDefault(niv => niv.IdItem == 
                        _currentViewModel.CurrentItemsToNivelacija.IdItem) != null)
                        {
                            MessageBox.Show("Već ste uneli artikal u nivelaciju. Ako želite da ga ispravite, prvo morate da ga obrišete iz trenutne nivelacije!",
                                "Greška u dodavanju nove nivelacije", MessageBoxButton.OK, MessageBoxImage.Error);

                            _currentViewModel.NivelacijaItemWindow.Close();
                            _currentViewModel.CurrentItemsToNivelacija = null;

                            return;
                        }
                        _currentViewModel.CurrentNivelacija.NivelacijaItems.Add(_currentViewModel.CurrentItemsToNivelacija);

                        _currentViewModel.TotalNivelacija += _currentViewModel.CurrentItemsToNivelacija.NewTotalValue - _currentViewModel.CurrentItemsToNivelacija.OldTotalValue;
                        _currentViewModel.TotalPdvNivelacija += _currentViewModel.CurrentItemsToNivelacija.NewTotalPDV - _currentViewModel.CurrentItemsToNivelacija.OldTotalPDV;
                        _currentViewModel.TotalNewNivelacija += _currentViewModel.CurrentItemsToNivelacija.NewTotalValue;
                        _currentViewModel.TotalOldNivelacija += _currentViewModel.CurrentItemsToNivelacija.OldTotalValue;
                    }

                    _currentViewModel.NivelacijaItemWindow.Close();
                    _currentViewModel.CurrentItemsToNivelacija = null;
                }
            }
        }
    }
}