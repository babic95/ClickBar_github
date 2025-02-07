using ClickBar.Models.AppMain.Statistic;
using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar_Database.Models;
using ClickBar_Database;
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
    public class SavePartnerCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private PartnerViewModel _currentViewModel;

        public SavePartnerCommand(PartnerViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (_currentViewModel.CurrentPartner == null ||
                _currentViewModel.CurrentPartner.Id == null ||
                _currentViewModel.CurrentPartner.Id < 1)
            {
                AddEditPartner();
            }
            else
            {
                AddEditPartner(_currentViewModel.CurrentPartner.Id);
            }
        }
        private void AddEditPartner(int? id = null)
        {
            using (SqliteDbContext sqliteDbContext = new SqliteDbContext())
            {

                if (id == null)
                {
                    PartnerDB Partner = new PartnerDB();
                    try
                    {
                        Partner.Name = _currentViewModel.CurrentPartner.Name;
                        Partner.Pib = _currentViewModel.CurrentPartner.Pib;
                        Partner.Mb = _currentViewModel.CurrentPartner.MB;
                        Partner.Address = _currentViewModel.CurrentPartner.Address;
                        Partner.City = _currentViewModel.CurrentPartner.City;
                        Partner.ContractNumber = _currentViewModel.CurrentPartner.ContractNumber;
                        Partner.Email = _currentViewModel.CurrentPartner.Email;

                        sqliteDbContext.Add(Partner);
                        RetryHelper.ExecuteWithRetry(() => { sqliteDbContext.SaveChanges(); });

                        MessageBox.Show("Uspešno ste dodali firmu partnera!", "Uspešno dodavanje", MessageBoxButton.OK, MessageBoxImage.Information);

                        _currentViewModel.Window.Close();
                    }
                    catch
                    {
                        MessageBox.Show("Greška prilikom dodavanja firme partnera!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    var result = MessageBox.Show("Da li ste sigurni da želite da izmenite firmu partnera?", "Izmena firme partnera",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        try
                        {
                            var Partner = sqliteDbContext.Partners.Find(id);

                            if (Partner != null)
                            {
                                Partner.Name = _currentViewModel.CurrentPartner.Name;
                                Partner.Pib = _currentViewModel.CurrentPartner.Pib;
                                Partner.Mb = _currentViewModel.CurrentPartner.MB;
                                Partner.Address = _currentViewModel.CurrentPartner.Address;
                                Partner.City = _currentViewModel.CurrentPartner.City;
                                Partner.ContractNumber = _currentViewModel.CurrentPartner.ContractNumber;
                                Partner.Email = _currentViewModel.CurrentPartner.Email;

                                sqliteDbContext.Partners.Update(Partner);
                                RetryHelper.ExecuteWithRetry(() => { sqliteDbContext.SaveChanges(); });

                                MessageBox.Show("Uspešno ste izmenili firmu partnera!", "Uspešna izmena", MessageBoxButton.OK, MessageBoxImage.Information);

                                _currentViewModel.Window.Close();
                            }
                            else
                            {
                                MessageBox.Show("Ne postoji firma partner!", "Ne postoji", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                        catch
                        {
                            MessageBox.Show("Greška prilikom izmene firme partnera!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }

                _currentViewModel.PartnersAll = new List<Partner>();
                sqliteDbContext.Partners.ToList().ForEach(x =>
                {
                    _currentViewModel.PartnersAll.Add(new Partner(x));
                });

                _currentViewModel.Partners = new ObservableCollection<Partner>(_currentViewModel.PartnersAll);
                _currentViewModel.CurrentPartner = new Partner();
            }
        }
    }
}