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

namespace ClickBar.Commands.AppMain.Admin.Rooms
{
    public class EditRoomCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private AdminViewModel _currentViewModel;

        public EditRoomCommand(AdminViewModel currentViewModel)
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
                _currentViewModel.NewRoom = _currentViewModel.CurrentPartHall;

                _currentViewModel.AddNewRoomWindow = new AddNewRoomWindow(_currentViewModel);
                _currentViewModel.AddNewRoomWindow.ShowDialog();
            }
            else
            {
                MessageBox.Show("Mora biti izabrana prostorija koja se menja!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
