using ClickBar.ViewModels.Sale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using ClickBar_Logging;
using ClickBar.ViewModels.AppMain;
using ClickBar.ViewModels;

namespace ClickBar.Commands.Sale.Pay.SplitOrder.ChangePaymentPlace
{
    public class SelectRoomChangePaymentPlaceCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ChangePaymentPlaceViewModel _viewModel;

        public SelectRoomChangePaymentPlaceCommand(ChangePaymentPlaceViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            try
            {
                if (parameter is int)
                {
                    int partHallId = Convert.ToInt32(parameter);

                    _viewModel.CurrentPartHall = _viewModel.Rooms.FirstOrDefault(partHall => partHall.Id == partHallId);

                    if (_viewModel.CurrentPartHall != null)
                    {
                        _viewModel.Title = _viewModel.CurrentPartHall.Name;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Desila se greška prilikom promene prostorije!",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Log.Error("SelectRoomChangePaymentPlaceCommand -> Greška u prebacivanju prostorije", ex);
            }
        }
    }
}