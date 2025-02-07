using ClickBar.Models.AppMain.Statistic;
using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar.ViewModels;
using ClickBar_Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using ClickBar.Enums.AppMain.Statistic;
using ClickBar_Database.Models;
using ClickBar.Models.AppMain.Statistic.KEP;

namespace ClickBar.Commands.AppMain.Statistic.KEP
{
    public class SearchCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private KEPViewModel _currentViewModel;

        public SearchCommand(KEPViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if(_currentViewModel.FromDate > _currentViewModel.ToDate)
            {
                MessageBox.Show("Početni datum ne sme biti mlađi od krajnjeg!", "Greška u datumu", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if(_currentViewModel.FromDate.Date > DateTime.Now.Date)
            {
                MessageBox.Show("Početni datum mora biti u sadašnjisti ili prošlosti!", "Datum je u budućnosti", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int searchType = -1;

            switch (_currentViewModel.CurrentTypeKEP)
            {
                case "Dnevni pazar":
                    searchType = 0;
                    break;
                case "Kalkulacije":
                    searchType = (int)KepStateEnumeration.Kalkulacija;
                    break;
                case "Nivelacije":
                    searchType = (int)KepStateEnumeration.Nivelacija;
                    break;
                case "Povratnica - KUPAC":
                    searchType = (int)KepStateEnumeration.Povratnica_Kupac;
                    break;
                case "Povratnica - Dobavljač":
                    searchType = (int)KepStateEnumeration.Povratnica_Dobavljac;
                    break;
                case "Otpis":
                    searchType = (int)KepStateEnumeration.Otpis;
                    break;
            }

            using (SqliteDbContext sqliteDbContext = new SqliteDbContext())
            {

                IEnumerable<KepDB>? keps;

                if (searchType < 0)
                {
                    keps = sqliteDbContext.Kep.Where(kep => kep.KepDate >= _currentViewModel.FromDate &&
                    kep.KepDate <= _currentViewModel.ToDate);
                }
                else
                {
                    if (searchType == 0)
                    {
                        keps = sqliteDbContext.Kep.Where(kep => kep.KepDate >= _currentViewModel.FromDate &&
                        kep.KepDate <= _currentViewModel.ToDate &&
                        (kep.Type >= (int)KepStateEnumeration.Dnevni_Pazar_Prodaja_Gotovina &&
                        kep.Type <= (int)KepStateEnumeration.Dnevni_Pazar_Refundacija_Virman));
                    }
                    else
                    {
                        keps = sqliteDbContext.Kep.Where(kep => kep.KepDate >= _currentViewModel.FromDate &&
                        kep.KepDate <= _currentViewModel.ToDate &&
                        kep.Type == searchType);
                    }
                }

                if (keps != null)
                {
                    _currentViewModel.Zaduzenje = 0;
                    _currentViewModel.Razduzenje = 0;
                    _currentViewModel.Saldo = 0;

                    _currentViewModel.ItemsKEP = new ObservableCollection<ItemKEP>();

                    if (keps.Any())
                    {
                        decimal saldo = 0;
                        keps = keps.OrderBy(kep => kep.KepDate);

                        foreach (var kep in keps)
                        {
                            _currentViewModel.Zaduzenje += kep.Zaduzenje;
                            _currentViewModel.Razduzenje += kep.Razduzenje;
                            _currentViewModel.Saldo += kep.Zaduzenje - kep.Razduzenje;

                            saldo += kep.Zaduzenje - kep.Razduzenje;
                            ItemKEP itemKEP = new ItemKEP(kep, saldo);

                            _currentViewModel.ItemsKEP.Add(itemKEP);
                        }
                    }
                }
            }
        }
    }
}