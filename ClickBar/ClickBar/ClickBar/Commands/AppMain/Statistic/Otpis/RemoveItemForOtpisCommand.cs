using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar.Views.AppMain.AuxiliaryWindows.Statistic.Otpis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using ClickBar_Logging;

namespace ClickBar.Commands.AppMain.Statistic.Otpis
{
    public class RemoveItemForOtpisCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private OtpisViewModel _currentViewModel;

        public RemoveItemForOtpisCommand(OtpisViewModel currentViewModel)
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
                var result = MessageBox.Show("Da li zaista želite da obrišete artikal?",
                    "Brisanje",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var item = _currentViewModel.ItemsInOtpis.FirstOrDefault(i => i.ItemInOtpis.Id == parameter.ToString());

                    if (item != null)
                    {
                        _currentViewModel.ItemsInOtpis.Remove(item);
                    }

                    MessageBox.Show("Uspešno obrisan artikal iz otpisa",
                        "Uspešno brisanje",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                Log.Error("RemoveItemForOtpisCommand -> Execute -> Greska prilikom brisanja artikla za otpis", ex);
                MessageBox.Show("Greška prilikom brisanja!",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}