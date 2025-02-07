using ClickBar.ViewModels.AppMain.Statistic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using ClickBar_Logging;
using ClickBar.Models.AppMain.Statistic.Otpis;

namespace ClickBar.Commands.AppMain.Statistic.Otpis
{
    public class AddItemToOtpisCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private OtpisViewModel _currentViewModel;

        public AddItemToOtpisCommand(OtpisViewModel currentViewModel)
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
                if (_currentViewModel.QuantityString.Contains(","))
                {
                    _currentViewModel.QuantityString = _currentViewModel.QuantityString.Replace(",", ".");
                }

                decimal quantity = Convert.ToDecimal(_currentViewModel.QuantityString);

                try
                {
                    quantity = Convert.ToDecimal(_currentViewModel.QuantityString);
                }
                catch
                {
                    MessageBox.Show("Niste uneli ispravnu količinu!",
                        "",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                var itemInOtpis = _currentViewModel.ItemsInOtpis.FirstOrDefault(i => i.ItemInOtpis.Id == _currentViewModel.CurrentItem.ItemInOtpis.Id);

                if(itemInOtpis != null )
                {
                    itemInOtpis.Quantity += Decimal.Round(quantity, 3);
                }
                else
                {
                    OtpisItem otpisItem = new OtpisItem()
                    {
                        Quantity = quantity,
                        ItemInOtpis = _currentViewModel.CurrentItem.ItemInOtpis
                    };

                    _currentViewModel.ItemsInOtpis.Add(otpisItem);
                }

                _currentViewModel.QuantityString = "0";
                _currentViewModel.SelectedItem = null;
                _currentViewModel.CurrentItem = null;
                _currentViewModel.TextSearch = string.Empty;
                _currentViewModel.QuantityWindow.Close();
            }
            catch (Exception ex)
            {
                Log.Error("AddItemToOtpisCommand -> Execute -> Greska prilikom dodavanja artikla na otpis", ex);
                MessageBox.Show("Desila se greška! Obratite se serviseru.",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}