using ClickBar.Enums.AppMain.Admin;
using ClickBar.Models.TableOverview;
using ClickBar.ViewModels.AppMain;
using ClickBar.Views.AppMain.AuxiliaryWindows.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClickBar.Commands.AppMain.Admin
{
    public class EditPaymentPlaceCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private AdminViewModel _currentViewModel;

        public EditPaymentPlaceCommand(AdminViewModel currentViewModel)
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
                int id = Convert.ToInt32(parameter);
                PaymentPlace? paymentPlace = _currentViewModel.NormalPaymentPlaces.FirstOrDefault(p => p.Id == id);

                if (paymentPlace == null)
                {
                    paymentPlace = _currentViewModel.RoundPaymentPlaces.FirstOrDefault(p => p.Id == id);
                }

                if (paymentPlace != null)
                {
                    _currentViewModel.NewPaymentPlace = paymentPlace;

                    if(paymentPlace.Type == PaymentPlaceTypeEnumeration.Round)
                    {
                        _currentViewModel.IsCheckedRoundPaymentPlace = true;
                    }
                    else
                    {
                        _currentViewModel.IsCheckedRoundPaymentPlace = false;
                    }

                    _currentViewModel.AddNewPaymentPlaceWindow = new AddNewPaymentPlaceWindow(_currentViewModel);
                    _currentViewModel.AddNewPaymentPlaceWindow.ShowDialog();
                }
            }
            catch
            {

            }
        }
    }
}
