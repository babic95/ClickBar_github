using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar.Views.AppMain.AuxiliaryWindows.Statistic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace ClickBar.Commands.AppMain.Statistic.Radnici
{
    public class EditRadnikCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private RadniciViewModel _currentViewModel;

        public EditRadnikCommand(RadniciViewModel currentViewModel)
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
                var radnik = _currentViewModel.Radnici.Where(radnik => radnik.Id == parameter.ToString()).FirstOrDefault();

                if (radnik != null)
                {
                    _currentViewModel.CurrentRadnik = radnik;

                    _currentViewModel.IsEdited = true;

                    AddEditRadnikWindow addEditRadnikWindow = new AddEditRadnikWindow(_currentViewModel);
                    addEditRadnikWindow.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Ne postoji radnik!", "Ne postoji", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch
            {
                MessageBox.Show("Greška prilikom učitavanja radnika!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}