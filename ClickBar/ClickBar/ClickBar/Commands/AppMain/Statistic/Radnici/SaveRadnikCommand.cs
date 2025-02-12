using ClickBar.Enums.AppMain.Statistic;
using ClickBar.Models.AppMain.Statistic;
using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar_Common.Enums;
using ClickBar_DatabaseSQLManager;
using ClickBar_DatabaseSQLManager.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ClickBar.Commands.AppMain.Statistic.Radnici
{
    public class SaveRadnikCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private RadniciViewModel _currentViewModel;

        public SaveRadnikCommand(RadniciViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (string.IsNullOrEmpty(_currentViewModel.CurrentRadnik.Name) ||
                string.IsNullOrEmpty(_currentViewModel.CurrentRadnik.Id))
            {
                MessageBox.Show("Morate da unesete obavezna polja",
                    "Unesite obavezna polja",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                return;
            }

            AddEditRadnik();
        }
        private void AddEditRadnik()
        {
            if (!_currentViewModel.IsEdited)
            {
                try
                {
                    var r = _currentViewModel.DbContext.Cashiers.Find(_currentViewModel.CurrentRadnik.Id);

                    if (r != null)
                    {
                        MessageBox.Show("Radnik sa zadatom šifrom već postoji! Promenite šifru.", "Greška",
                            MessageBoxButton.OK, MessageBoxImage.Error);

                        return;
                    }

                    if (!string.IsNullOrEmpty(_currentViewModel.CurrentRadnik.SmartCardNumber))
                    {
                        var s = _currentViewModel.DbContext.Cashiers.FirstOrDefault(card => card.SmartCardNumber == _currentViewModel.CurrentRadnik.SmartCardNumber);

                        if (s != null)
                        {
                            MessageBox.Show("Radnik sa zadatim brojem kartice već postoji! Promenite karticu.", "Greška",
                                MessageBoxButton.OK, MessageBoxImage.Error);

                            return;
                        }
                    }

                    CashierDB radnikDB = new CashierDB()
                    {
                        Id = _currentViewModel.CurrentRadnik.Id,
                        Name = _currentViewModel.CurrentRadnik.Name,
                        Address = _currentViewModel.CurrentRadnik.Address,
                        City = _currentViewModel.CurrentRadnik.City,
                        ContactNumber = _currentViewModel.CurrentRadnik.ContractNumber,
                        Email = _currentViewModel.CurrentRadnik.Email,
                        Jmbg = _currentViewModel.CurrentRadnik.Jmbg,
                        SmartCardNumber = _currentViewModel.CurrentRadnik.SmartCardNumber,
                        Type = _currentViewModel.CurrentRadnik.RadnikStateEnumeration == RadnikStateEnumeration.Konobar ? CashierTypeEnumeration.Worker :
                        CashierTypeEnumeration.Moderator,
                    };

                    _currentViewModel.DbContext.Cashiers.Add(radnikDB);
                    _currentViewModel.DbContext.SaveChanges();

                    MessageBox.Show("Uspešno ste dodali radnika!", "Uspešno dodavanje", MessageBoxButton.OK, MessageBoxImage.Information);

                    _currentViewModel.Window.Close();
                }
                catch
                {
                    MessageBox.Show("Greška prilikom dodavanja radnika!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                var result = MessageBox.Show("Da li ste sigurni da želite da izmenite radnika?", "Izmena radnika",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var radnikDB = _currentViewModel.DbContext.Cashiers.Find(_currentViewModel.CurrentRadnik.OriginalId);

                        if (radnikDB != null)
                        {
                            if (_currentViewModel.CurrentRadnik.Id != _currentViewModel.CurrentRadnik.OriginalId)
                            {
                                var r = _currentViewModel.DbContext.Cashiers.Find(_currentViewModel.CurrentRadnik.Id);

                                if (r != null)
                                {
                                    MessageBox.Show("Radnik sa zadatom šifrom već postoji! Promenite šifru.", "Greška",
                                        MessageBoxButton.OK, MessageBoxImage.Error);

                                    return;
                                }
                            }

                            if (!string.IsNullOrEmpty(_currentViewModel.CurrentRadnik.SmartCardNumber))
                            {
                                var s = _currentViewModel.DbContext.Cashiers.FirstOrDefault(card => card.SmartCardNumber == _currentViewModel.CurrentRadnik.SmartCardNumber);

                                if (s != null)
                                {
                                    if (s.Id != radnikDB.Id)
                                    {
                                        MessageBox.Show("Radnik sa zadatim brojem kartice već postoji! Promenite karticu.", "Greška",
                                            MessageBoxButton.OK, MessageBoxImage.Error);
                                        return;
                                    }
                                }
                            }

                            radnikDB.Id = _currentViewModel.CurrentRadnik.Id;
                            radnikDB.Name = _currentViewModel.CurrentRadnik.Name;
                            radnikDB.Jmbg = _currentViewModel.CurrentRadnik.Jmbg;
                            radnikDB.Address = _currentViewModel.CurrentRadnik.Address;
                            radnikDB.City = _currentViewModel.CurrentRadnik.City;
                            radnikDB.ContactNumber = _currentViewModel.CurrentRadnik.ContractNumber;
                            radnikDB.Email = _currentViewModel.CurrentRadnik.Email;
                            radnikDB.SmartCardNumber = _currentViewModel.CurrentRadnik.SmartCardNumber;
                            radnikDB.Type = _currentViewModel.CurrentRadnik.RadnikStateEnumeration == RadnikStateEnumeration.Konobar ? CashierTypeEnumeration.Worker :
                                CashierTypeEnumeration.Moderator;

                            _currentViewModel.DbContext.Cashiers.Update(radnikDB);
                            _currentViewModel.DbContext.SaveChanges();

                            MessageBox.Show("Uspešno ste izmenili radnika!", "Uspešna izmena", MessageBoxButton.OK, MessageBoxImage.Information);

                            _currentViewModel.Window.Close();
                        }
                        else
                        {
                            MessageBox.Show("Ne postoji radnik!", "Ne postoji", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Greška prilikom izmene firme partnera!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

            _currentViewModel.RadniciAll = new List<Radnik>();
            _currentViewModel.DbContext.Cashiers.ToList().ForEach(x =>
            {
                _currentViewModel.RadniciAll.Add(new Radnik(x));
            });

            _currentViewModel.Radnici = new ObservableCollection<Radnik>(_currentViewModel.RadniciAll);
            _currentViewModel.CurrentRadnik = new Radnik();
            _currentViewModel.IsEdited = false;
        }
    }
}