using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar_DatabaseSQLManager;
using ClickBar_DatabaseSQLManager.Models;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ClickBar.Commands.AppMain.Statistic.Firma
{
    public class SaveFirmaCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private FirmaViewModel _currentViewModel;

        public SaveFirmaCommand(FirmaViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            var result = MessageBox.Show("Da li ste sigurni da želite da sačuvate podatke o poslovnom prostoru?", "", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    if (_currentViewModel.FirmaDB != null)
                    {
                        _currentViewModel.FirmaDB.Name = _currentViewModel.Firma.Name;
                        _currentViewModel.FirmaDB.Pib = _currentViewModel.Firma.Pib;
                        _currentViewModel.FirmaDB.MB = _currentViewModel.Firma.MB;
                        _currentViewModel.FirmaDB.NamePP = _currentViewModel.Firma.NamePP;
                        _currentViewModel.FirmaDB.AddressPP = _currentViewModel.Firma.AddressPP;
                        _currentViewModel.FirmaDB.Number = _currentViewModel.Firma.Number;
                        _currentViewModel.FirmaDB.Email = _currentViewModel.Firma.Email;
                        _currentViewModel.FirmaDB.BankAcc = _currentViewModel.Firma.BankAcc;
                        _currentViewModel.FirmaDB.AuthenticationKey = _currentViewModel.Firma.AuthenticationKey;

                        _currentViewModel.DbContext.Firmas.Update(_currentViewModel.FirmaDB);
                    }
                    else
                    {
                        FirmaDB firmaDB = new FirmaDB()
                        {
                            Name = _currentViewModel.Firma.Name,
                            Pib = _currentViewModel.Firma.Pib,
                            MB = _currentViewModel.Firma.MB,
                            NamePP = _currentViewModel.Firma.NamePP,
                            AddressPP = _currentViewModel.Firma.AddressPP,
                            Number = _currentViewModel.Firma.Number,
                            Email = _currentViewModel.Firma.Email,
                            BankAcc = _currentViewModel.Firma.BankAcc,
                            AuthenticationKey = _currentViewModel.Firma.AuthenticationKey
                        };
                        _currentViewModel.DbContext.Firmas.Add(firmaDB);
                    }
                    _currentViewModel.DbContext.SaveChanges();

                    MessageBox.Show("Uspešno ste sačuvali podatke o poslovnom prostorul!", "", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch
                {
                    MessageBox.Show("Greška prilikom čuvanja podataka o poslovnom prostoru!", "", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}