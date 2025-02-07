using ClickBar.Enums.AppMain.Admin;
using ClickBar.Models.TableOverview;
using ClickBar.ViewModels.AppMain;
using ClickBar_Database;
using ClickBar_Database.Models;
using DocumentFormat.OpenXml.InkML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ClickBar.Commands.AppMain.Admin
{
    public class AddNewPaymentPlaceCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private AdminViewModel _currentViewModel;

        public AddNewPaymentPlaceCommand(AdminViewModel currentViewModel)
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
                if (_currentViewModel.IsCheckedRoundPaymentPlace)
                {
                    if (_currentViewModel.NewPaymentPlace.Diameter < 20)
                    {
                        _currentViewModel.IsCheckedRoundPaymentPlace = false;
                        MessageBox.Show("Minimalna prečnik je 20mm!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                else
                {
                    if (_currentViewModel.NewPaymentPlace.Width < 18 ||
                        _currentViewModel.NewPaymentPlace.Height < 18)
                    {
                        _currentViewModel.IsCheckedRoundPaymentPlace = false;
                        MessageBox.Show("Minimalna vrednost sirine i duzine je 18mm!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                MessageBoxResult result = MessageBox.Show("Da li ste sigurni?", 
                    "",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    using (var sqliteDbContext = new SqliteDbContext())
                    {

                        int idPaymentPlace = Convert.ToInt32(parameter);
                        if (idPaymentPlace == 0)
                        {
                            PaymentPlaceDB paymentPlaceDB = new PaymentPlaceDB()
                            {
                                LeftCanvas = _currentViewModel.NewPaymentPlace.Left,
                                TopCanvas = _currentViewModel.NewPaymentPlace.Top,
                                PartHallId = _currentViewModel.CurrentPartHall.Id,
                                HeightMobi = 20,
                                WidthMobi = 20,
                                X_Mobi = 0,
                                Y_Mobi = 0,
                                Name = _currentViewModel.NewPaymentPlace.Name,
                                Popust = _currentViewModel.NewPaymentPlace.Popust
                            };

                            if (_currentViewModel.IsCheckedRoundPaymentPlace)
                            {
                                paymentPlaceDB.Width = _currentViewModel.NewPaymentPlace.Diameter;
                                paymentPlaceDB.Height = _currentViewModel.NewPaymentPlace.Diameter;
                                paymentPlaceDB.Type = (int)PaymentPlaceTypeEnumeration.Round;
                            }
                            else
                            {
                                paymentPlaceDB.Width = _currentViewModel.NewPaymentPlace.Width;
                                paymentPlaceDB.Height = _currentViewModel.NewPaymentPlace.Height;
                                paymentPlaceDB.Type = (int)PaymentPlaceTypeEnumeration.Normal;
                            }

                            sqliteDbContext.PaymentPlaces.Add(paymentPlaceDB);
                            RetryHelper.ExecuteWithRetry(() => { sqliteDbContext.SaveChanges(); });

                            _currentViewModel.NewPaymentPlace.Id = paymentPlaceDB.Id;

                            if (!_currentViewModel.IsCheckedRoundPaymentPlace)
                            {
                                _currentViewModel.AllNormalPaymentPlaces.Add(_currentViewModel.NewPaymentPlace);
                                _currentViewModel.NormalPaymentPlaces.Add(_currentViewModel.NewPaymentPlace);
                            }
                            else
                            {
                                _currentViewModel.AllRoundPaymentPlaces.Add(_currentViewModel.NewPaymentPlace);
                                _currentViewModel.RoundPaymentPlaces.Add(_currentViewModel.NewPaymentPlace);
                            }

                            MessageBox.Show("Uspešno!", "Uspešno", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            var paymentPlaces = sqliteDbContext.PaymentPlaces.Find(idPaymentPlace);

                            if (paymentPlaces != null)
                            {
                                paymentPlaces.Popust = _currentViewModel.NewPaymentPlace.Popust;
                                paymentPlaces.Name = _currentViewModel.NewPaymentPlace.Name;
                                if ((paymentPlaces.Type == (int)PaymentPlaceTypeEnumeration.Normal || !paymentPlaces.Type.HasValue) &&
                                    _currentViewModel.IsCheckedRoundPaymentPlace)
                                {
                                    paymentPlaces.Type = (int)PaymentPlaceTypeEnumeration.Round;

                                    var payment = _currentViewModel.AllNormalPaymentPlaces.FirstOrDefault(payment => payment.Id == paymentPlaces.Id);

                                    if (payment != null)
                                    {
                                        _currentViewModel.AllRoundPaymentPlaces.Add(payment);
                                        _currentViewModel.RoundPaymentPlaces.Add(payment);

                                        _currentViewModel.AllNormalPaymentPlaces.Remove(payment);

                                        var pay = _currentViewModel.NormalPaymentPlaces.FirstOrDefault(payment => payment.Id == paymentPlaces.Id);

                                        if (pay != null)
                                        {
                                            _currentViewModel.NormalPaymentPlaces.Remove(pay);
                                        }
                                    }
                                }
                                else if (paymentPlaces.Type == (int)PaymentPlaceTypeEnumeration.Round &&
                                    !_currentViewModel.IsCheckedRoundPaymentPlace)
                                {
                                    paymentPlaces.Type = (int)PaymentPlaceTypeEnumeration.Normal;

                                    var payment = _currentViewModel.AllRoundPaymentPlaces.FirstOrDefault(payment => payment.Id == paymentPlaces.Id);

                                    if (payment != null)
                                    {
                                        _currentViewModel.AllNormalPaymentPlaces.Add(payment);
                                        _currentViewModel.NormalPaymentPlaces.Add(payment);

                                        _currentViewModel.AllRoundPaymentPlaces.Remove(payment);

                                        var pay = _currentViewModel.RoundPaymentPlaces.FirstOrDefault(payment => payment.Id == paymentPlaces.Id);

                                        if (pay != null)
                                        {
                                            _currentViewModel.RoundPaymentPlaces.Remove(pay);
                                        }
                                    }
                                }

                                if (_currentViewModel.IsCheckedRoundPaymentPlace)
                                {
                                    paymentPlaces.Width = _currentViewModel.NewPaymentPlace.Diameter;
                                    paymentPlaces.Height = _currentViewModel.NewPaymentPlace.Diameter;
                                }
                                else
                                {
                                    paymentPlaces.Width = _currentViewModel.NewPaymentPlace.Width;
                                    paymentPlaces.Height = _currentViewModel.NewPaymentPlace.Height;
                                }

                                sqliteDbContext.PaymentPlaces.Update(paymentPlaces);
                                RetryHelper.ExecuteWithRetry(() => { sqliteDbContext.SaveChanges(); });

                                MessageBox.Show("Uspešna izmena!", "Uspešno", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                    }
                }
            }
            catch
            {
                MessageBox.Show("Greška prilikom dodavanja ili izmene platnog mesta!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (_currentViewModel.AddNewPaymentPlaceWindow != null)
            {
                _currentViewModel.AddNewPaymentPlaceWindow.Close();
                _currentViewModel.AddNewPaymentPlaceWindow = null;
            }
            _currentViewModel.IsCheckedRoundPaymentPlace = false;
        }
    }
}
