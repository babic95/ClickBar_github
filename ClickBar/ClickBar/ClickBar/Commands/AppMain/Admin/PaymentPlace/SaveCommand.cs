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

namespace ClickBar.Commands.AppMain.Admin
{
    public class SaveCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private AdminViewModel _currentViewModel;

        public SaveCommand(AdminViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            try
            {
                if (_currentViewModel.Change)
                {
                    MessageBoxResult result = MessageBox.Show("Da li želite da sačuvate promene?", "", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        await Task.Run(() =>
                        {
                            _currentViewModel.NormalPaymentPlaces.ToList().ForEach(p =>
                            {
                                var paymentPlace = _currentViewModel.DbContext.PaymentPlaces.Find(p.Id);

                                if (paymentPlace != null)
                                {
                                    paymentPlace.TopCanvas = p.Top;
                                    paymentPlace.LeftCanvas = p.Left;
                                }
                                else
                                {
                                    PaymentPlaceDB paymentPlaceDB = new PaymentPlaceDB()
                                    {
                                        LeftCanvas = p.Left,
                                        TopCanvas = p.Top,
                                        Height = p.Height,
                                        Width = p.Width,
                                        PartHallId = p.PartHallId,
                                        Type = (int)p.Type
                                    };
                                    _currentViewModel.DbContext.PaymentPlaces.Add(paymentPlaceDB);
                                }
                                _currentViewModel.DbContext.SaveChanges();
                            });

                        });
                        await Task.Run(() =>
                        {
                            _currentViewModel.RoundPaymentPlaces.ToList().ForEach(p =>
                            {
                                var paymentPlace = _currentViewModel.DbContext.PaymentPlaces.Find(p.Id);

                                if (paymentPlace != null)
                                {
                                    paymentPlace.TopCanvas = p.Top;
                                    paymentPlace.LeftCanvas = p.Left;
                                }
                                else
                                {
                                    PaymentPlaceDB paymentPlaceDB = new PaymentPlaceDB()
                                    {
                                        LeftCanvas = p.Left,
                                        TopCanvas = p.Top,
                                        Height = p.Height,
                                        Width = p.Width,
                                        PartHallId = p.PartHallId,
                                        Type = (int)p.Type
                                    };
                                    _currentViewModel.DbContext.PaymentPlaces.Add(paymentPlaceDB);
                                }
                                _currentViewModel.DbContext.SaveChanges();
                            });
                        });
                        MessageBox.Show("Uspešno ste sačuvali izmene?", "Uspešno čuvanje", MessageBoxButton.OK, MessageBoxImage.Information);
                        _currentViewModel.Change = false;
                    }
                    else
                    {
                        _currentViewModel.Change = false;
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
