using ClickBar.ViewModels.AppMain;
using ClickBar_DatabaseSQLManager;
using ClickBar_DatabaseSQLManager.Models;
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
                        using (var dbContext = _currentViewModel.DbContextFactory.CreateDbContext())
                        {
                            if (_currentViewModel.Rooms.Any(room => room.Id == _currentViewModel.NewRoom.Id))
                            {
                                var room = dbContext.PartHalls.Find(_currentViewModel.NewRoom.Id);

                                if (room != null)
                                {
                                    room.Image = _currentViewModel.NewRoom.Image;
                                    room.Name = _currentViewModel.NewRoom.Name;

                                    dbContext.PartHalls.Update(room);
                                    dbContext.SaveChanges();
                                }
                                else
                                {
                                    room = new PartHallDB()
                                    {
                                        Image = _currentViewModel.NewRoom.Image,
                                        Name = _currentViewModel.NewRoom.Name,
                                    };
                                    dbContext.PartHalls.Add(room);
                                    dbContext.SaveChanges();

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
                                dbContext.PartHalls.Add(room);
                                dbContext.SaveChanges();

                                _currentViewModel.NewRoom.Id = room.Id;
                                _currentViewModel.Rooms.Add(_currentViewModel.NewRoom);
                            }
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
            catch
            {
                MessageBox.Show("Greška prilikom čuvanja izmena!", "Greška prilikom čuvanja", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
