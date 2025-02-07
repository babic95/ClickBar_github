using ClickBar.ViewModels;
using ClickBar.ViewModels.AppMain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClickBar.Commands.AppMain.Admin
{
    public class SelectRoomCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ViewModelBase _currentViewModel;

        public SelectRoomCommand(ViewModelBase currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is int)
            {
                if(_currentViewModel is AdminViewModel)
                {
                    AdminViewModel adminViewModel = (AdminViewModel)_currentViewModel;

                    int partHallId = Convert.ToInt32(parameter);

                    adminViewModel.CurrentPartHall = adminViewModel.Rooms.Where(partHall => partHall.Id == partHallId).FirstOrDefault();
                }
                else
                {
                    TableOverviewViewModel tableOverviewViewModel = (TableOverviewViewModel)_currentViewModel;

                    int partHallId = Convert.ToInt32(parameter);

                    tableOverviewViewModel.CurrentPartHall = tableOverviewViewModel.Rooms.Where(partHall => partHall.Id == partHallId).FirstOrDefault();

                    if (tableOverviewViewModel.CurrentPartHall != null)
                    {
                        tableOverviewViewModel.SaleViewModel.CurrentPartHall = tableOverviewViewModel.CurrentPartHall;
                    }
                }
            }
        }
    }
}
