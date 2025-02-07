using ClickBar.Models.AppMain.Statistic;
using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar_Database;
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
    public class DeleteRadnikCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private RadniciViewModel _currentViewModel;

        public DeleteRadnikCommand(RadniciViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            var result = MessageBox.Show("Da li ste sigurni da želite da obrišete radnika?", "Brisanje radnika",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqliteDbContext sqliteDbContext = new SqliteDbContext())
                    {

                        var radnik = sqliteDbContext.Cashiers.Find(parameter.ToString());

                        if (radnik != null)
                        {
                            sqliteDbContext.Cashiers.Remove(radnik);
                            RetryHelper.ExecuteWithRetry(() => { sqliteDbContext.SaveChanges(); });

                            _currentViewModel.RadniciAll = new List<Radnik>();
                            sqliteDbContext.Cashiers.ToList().ForEach(x =>
                            {
                                _currentViewModel.RadniciAll.Add(new Radnik(x));
                            });

                            _currentViewModel.Radnici = new ObservableCollection<Radnik>(_currentViewModel.RadniciAll);
                            _currentViewModel.CurrentRadnik = new Radnik();

                            MessageBox.Show("Uspešno ste obrisali radnika!", "Uspešno brisanje", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Ne postoji radnik!", "Ne postoji", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
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