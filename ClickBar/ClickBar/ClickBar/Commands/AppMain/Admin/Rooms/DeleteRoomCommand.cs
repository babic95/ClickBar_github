using ClickBar.ViewModels.AppMain;
using ClickBar_Database;
using ClickBar_Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ClickBar.Commands.AppMain.Admin.Rooms
{
    public class DeleteRoomCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private AdminViewModel _currentViewModel;

        public DeleteRoomCommand(AdminViewModel currentViewModel)
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
                if (_currentViewModel.CurrentPartHall != null)
                {
                    MessageBoxResult result = MessageBox.Show("Da li želite da obrišete prostoriju?",
                        "Brisanje prostorije",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        using (SqliteDbContext sqliteDbContext = new SqliteDbContext())
                        {

                            PartHallDB? partHallDB = sqliteDbContext.PartHalls.Find(_currentViewModel.CurrentPartHall.Id);

                            if (partHallDB != null)
                            {
                                sqliteDbContext.PartHalls.Remove(partHallDB);
                                RetryHelper.ExecuteWithRetry(() => { sqliteDbContext.SaveChanges(); });
                                MessageBox.Show("Uspešno ste obrisali prostoriju!", "Uspešno brisanje", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Mora biti izabrana prostorija koja se briše!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch
            {
                MessageBox.Show("Greška prilikom brisanja prostorije!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
