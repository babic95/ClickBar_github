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
    public class SaveRoomCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private AdminViewModel _currentViewModel;

        public SaveRoomCommand(AdminViewModel currentViewModel)
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
                MessageBoxResult result = MessageBox.Show("Da li želite da sačuvate promene?", "", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    if (_currentViewModel.NewRoom != null)
                    {
                        using (SqliteDbContext sqliteDbContext = new SqliteDbContext())
                        {
                            if (_currentViewModel.Rooms.Any(room => room.Id == _currentViewModel.NewRoom.Id))
                            {
                                var room = sqliteDbContext.PartHalls.Find(_currentViewModel.NewRoom.Id);

                                if (room != null)
                                {
                                    room.Image = _currentViewModel.NewRoom.Image;
                                    room.Name = _currentViewModel.NewRoom.Name;

                                    sqliteDbContext.PartHalls.Update(room);
                                    RetryHelper.ExecuteWithRetry(() => { sqliteDbContext.SaveChanges(); });
                                }
                                else
                                {
                                    room = new PartHallDB()
                                    {
                                        Image = _currentViewModel.NewRoom.Image,
                                        Name = _currentViewModel.NewRoom.Name,
                                    };
                                    sqliteDbContext.PartHalls.Add(room);
                                    RetryHelper.ExecuteWithRetry(() => { sqliteDbContext.SaveChanges(); });

                                    _currentViewModel.NewRoom.Id = room.Id;
                                    _currentViewModel.Rooms.Add(_currentViewModel.NewRoom);
                                }
                            }
                            else
                            {
                                PartHallDB room = new PartHallDB()
                                {
                                    Image = _currentViewModel.NewRoom.Image,
                                    Name = _currentViewModel.NewRoom.Name,
                                };
                                sqliteDbContext.PartHalls.Add(room);
                                RetryHelper.ExecuteWithRetry(() => { sqliteDbContext.SaveChanges(); });

                                _currentViewModel.NewRoom.Id = room.Id;
                                _currentViewModel.Rooms.Add(_currentViewModel.NewRoom);
                            }
                        }
                        MessageBox.Show("Uspešno ste sačuvali izmene?", "Uspešno čuvanje", MessageBoxButton.OK, MessageBoxImage.Information);

                        if (_currentViewModel.AddNewRoomWindow != null)
                        {
                            _currentViewModel.AddNewRoomWindow.Close();
                            _currentViewModel.AddNewRoomWindow = null;
                        }
                    }
                }
            }
            catch
            {
                MessageBox.Show("Greška prilikom čuvanja izmena!", "Greška prilikom čuvanja", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
