using ClickBar.Models.AppMain.Statistic;
using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar_DatabaseSQLManager;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ClickBar.Commands.AppMain.Statistic.Parner
{
    public class DeletePartnerCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private PartnerViewModel _currentViewModel;

        public DeletePartnerCommand(PartnerViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            var result = MessageBox.Show("Da li ste sigurni da želite da obrišete firmu partnera?", "Brisanje firme partnera", 
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var partner = _currentViewModel.DbContext.Partners.Find(Convert.ToInt32(parameter));

                    if (partner != null)
                    {
                        _currentViewModel.DbContext.Partners.Remove(partner);
                        _currentViewModel.DbContext.SaveChanges();

                        _currentViewModel.PartnersAll = new List<Partner>();
                        _currentViewModel.DbContext.Partners.ToList().ForEach(x =>
                        {
                            _currentViewModel.PartnersAll.Add(new Partner(x));
                        });

                        _currentViewModel.Partners = new ObservableCollection<Partner>(_currentViewModel.PartnersAll);
                        _currentViewModel.CurrentPartner = new Partner();

                        MessageBox.Show("Uspešno ste obrisali firmu partnera!", "Uspešno brisanje", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Ne postoji firma partner!", "Ne postoji", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch
                {
                    MessageBox.Show("Greška prilikom brisanja firme partnera!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}