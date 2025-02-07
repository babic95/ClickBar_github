using ClickBar.Commands.Login;
using ClickBar_API;
using ClickBar_Common.Enums;
using ClickBar_Database;
using ClickBar_Database.Models;
using ClickBar_Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ClickBar.ViewModels.Login
{
    public class LoginViewModel : ViewModelBase
    {
        private string _message;
        private string _password;
        private ImageSource _logo;
        private Timer _timer;
        private DateTime? _validTo;
        private Task _initializationTask;

        private Visibility _visibilityBlack;

        public readonly CashierDB CashierAdmin = new CashierDB()
        {
            Id = "1807",
            Type = CashierTypeEnumeration.Admin,
            Name = "CleanCodeSirmium"
        };

        public LoginViewModel(ICommand updateCurrentViewModelCommand)
        {
            Initialization();

            using (SqliteDbContext sqliteDbContext = new SqliteDbContext())
            {
                AllCashiers = sqliteDbContext.Cashiers.ToList();
                UpdateCurrentViewModelCommand = updateCurrentViewModelCommand;

                string? logoUrl = SettingsManager.Instance.GetPathToLogo();

                if (File.Exists(logoUrl))
                {
                    Logo = new BitmapImage(new Uri(logoUrl));
                }
            }
        }

        public List<CashierDB> AllCashiers { get; private set; }

        public Visibility VisibilityBlack
        {
            get { return _visibilityBlack; }
            set
            {
                _visibilityBlack = value;
                OnPropertyChange(nameof(VisibilityBlack));
            }
        }
        public ImageSource Logo
        {
            get { return _logo; }
            set
            {
                _logo = value;
                OnPropertyChange(nameof(Logo));
            }
        }
        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                OnPropertyChange(nameof(Message));
            }
        }
        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChange(nameof(Password));
            }
        }

        public ICommand UpdateCurrentViewModelCommand { get; set; }
        public ICommand ClickOnLoginButtonCommand => new ClickOnLoginButtonCommand(this);

        private void KillApp()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                App.Current.Shutdown();
            });
        }

        private bool ValidDateTime()
        {
            string esirId = SettingsManager.Instance.GetEsirId();

            if (string.IsNullOrWhiteSpace(esirId))
            {
                MessageBox.Show("CCS ClickBar nije pravilno aktiviran! Obratite se proizvođaču.", "Greška aktivacije", MessageBoxButton.OK, MessageBoxImage.Error);

                KillApp();
                return false;
            }

            try
            {
                _validTo = CCS_Fiscalization_ApiManager.Instance.GetValidTo(esirId).Result;

                if (_validTo.HasValue)
                {
                    SettingsManager.Instance.SetValidTo(_validTo.Value);
                }
            }
            catch
            {
                _validTo = SettingsManager.Instance.GetValidTo();
            }

            DateTime currentDateTime = DateTime.Now.Date;

            if (_validTo < currentDateTime)
            {
                MessageBox.Show("Istekla Vam je licenca! Obratite se proizvođaču da bi mogli dalje da radite.", "Istek aktivacije",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                KillApp();
                return false;
            }

            int day = _validTo.Value.Date.Subtract(currentDateTime).Days;

            if (day <= 10)
            {
                MessageBox.Show($"Vaša licenca ističe za {day} dana! Obratite se proizvođaču za produženje.", "Istek aktivacije",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }

            return true;
        }
        private void Initialization()
        {

#if CRNO
            VisibilityBlack = Visibility.Hidden;
#else
            VisibilityBlack = Visibility.Visible;
#endif
            //if (SettingsManager.Instance.GetEnableCCS_Server())
            //{
            //    bool initializationCCS_Server = CCS_Fiscalization_ApiManager.Instance.Initialization().Result;

            //    if (!initializationCCS_Server)
            //    {
            //        MessageBox.Show("CCS ESIR nije pravilno aktiviran! Putanja do CCS SERVER-a ne postoji. Obratite se proizvođaču.", 
            //            "Greška", MessageBoxButton.OK, MessageBoxImage.Error);

            //        KillApp();
            //    }

            //    if (!ValidDateTime())
            //    {
            //        return;
            //    }
            //    bool firstValid = true;

            //    if (_timer is null)
            //    {
            //        _timer = new Timer(
            //            (e) =>
            //            {
            //                if (firstValid)
            //                {
            //                    firstValid = false;
            //                }
            //                else
            //                {
            //                    ValidDateTime();
            //                }
            //            },
            //            null,
            //            0,
            //            43200000);
            //    }
            //}
            ////if (_initializationTask is null)
            ////{
            ////    _initializationTask = Task.Run(async () =>
            ////    {
            ////        bool connectedWithLPFR = await ApiManager.Instance.Initialization();
            ////    });
            ////}
        }
    }
}