using ClickBar.Models.AppMain.Statistic;
using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar_DatabaseSQLManager;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using ClickBar.Views.AppMain.AuxiliaryWindows.Statistic;

namespace ClickBar.Commands.AppMain.Statistic.Parner
{
    public class EditPartnerCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private PartnerViewModel _currentViewModel;

        public EditPartnerCommand(PartnerViewModel currentViewModel)
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
                var partner = _currentViewModel.Partners.Where(partner => partner.Id == Convert.ToInt32(parameter)).FirstOrDefault();

                if (partner != null)
                {
                    _currentViewModel.CurrentPartner = partner;

                    AddEditPartnerWindow addEditPartnerWindow = new AddEditPartnerWindow(_currentViewModel);
                    addEditPartnerWindow.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Ne postoji firma partner!", "Ne postoji", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch
            {
                MessageBox.Show("Greška prilikom učitavanja firme partnera!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}