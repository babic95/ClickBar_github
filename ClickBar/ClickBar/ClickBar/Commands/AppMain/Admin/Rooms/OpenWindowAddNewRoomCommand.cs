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
    public class OpenWindowAddNewRoomCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private AdminViewModel _currentViewModel;

        public OpenWindowAddNewRoomCommand(AdminViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _currentViewModel.NewRoom = new PartHall()
            {
                Id = -1
            };

            _currentViewModel.AddNewRoomWindow = new AddNewRoomWindow(_currentViewModel);
            _currentViewModel.AddNewRoomWindow.ShowDialog();
        }
    }
}
