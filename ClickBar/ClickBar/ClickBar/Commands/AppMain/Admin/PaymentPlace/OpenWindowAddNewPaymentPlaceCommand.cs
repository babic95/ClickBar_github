using ClickBar.Enums.AppMain.Admin;
using ClickBar.Models.TableOverview;
using ClickBar.ViewModels.AppMain;
using ClickBar.Views.AppMain.AuxiliaryWindows.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ClickBar.Commands.AppMain.Admin
{
    public class OpenWindowAddNewPaymentPlaceCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private AdminViewModel _currentViewModel;

        public OpenWindowAddNewPaymentPlaceCommand(AdminViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (_currentViewModel.CurrentPartHall != null)
            {
                _currentViewModel.NewPaymentPlace = new PaymentPlace()
                {
                    Background = Brushes.Green,
                    Left = 10,
                    Top = 10,
                    PartHallId = _currentViewModel.CurrentPartHall.Id
                };

                _currentViewModel.AddNewPaymentPlaceWindow = new AddNewPaymentPlaceWindow(_currentViewModel);
                _currentViewModel.AddNewPaymentPlaceWindow.ShowDialog();
            }
            else
            {
                MessageBox.Show("Mora biti izabrana prostorija gde se dodaje platno mesto!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
