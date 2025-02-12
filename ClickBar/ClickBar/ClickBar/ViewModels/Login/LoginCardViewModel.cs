using ClickBar.Commands;
using ClickBar.Commands.AppMain;
using ClickBar.Enums;
using ClickBar_API;
using ClickBar_Common.Enums;
using ClickBar_Database_Drlja;
using ClickBar_DatabaseSQLManager;
using ClickBar_DatabaseSQLManager.Models;
using ClickBar_Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
    public class LoginCardViewModel : ViewModelBase
    {
        private IServiceProvider _serviceProvider;
        private readonly Lazy<UpdateCurrentAppStateViewModelCommand> _updateCurrentAppStateViewModelCommand;
        private readonly string _fromPath = @"C:\CCS ClickBar\ClickBar_Admin\PIN.json";
        private string _message;
        private string _password;
        private ImageSource _logo;
        private Timer _timer;
        private DateTime? _validTo;
        private Task _initializationTask;
        private string _carNumber;

        private Visibility _visibilityBlack;

        public readonly CashierDB CashierAdmin2 = new CashierDB()
        {
            Id = "9876543210",
            Type = CashierTypeEnumeration.Admin,
            Name = "CleanCodeSirmium"
        };

        public LoginCardViewModel(IServiceProvider serviceProvider, IDbContextFactory<SqlServerDbContext> dbContextFactory, IDbContextFactory<SqliteDrljaDbContext> drljaDbContextFactory)
        {
            _serviceProvider = serviceProvider;
            DbContext = dbContextFactory.CreateDbContext();
            DrljaDbContext = drljaDbContextFactory.CreateDbContext();
            _updateCurrentAppStateViewModelCommand = new Lazy<UpdateCurrentAppStateViewModelCommand>(() => serviceProvider.GetRequiredService<UpdateCurrentAppStateViewModelCommand>());

            Initialization();

            AllCashiers = DbContext.Cashiers.ToList();

            string? logoUrl = SettingsManager.Instance.GetPathToLogo();

            if (File.Exists(logoUrl))
            {
                Logo = new BitmapImage(new Uri(logoUrl));
            }
        }

        #region Internal Properties
        internal SqlServerDbContext DbContext { get; private set; }
        internal SqliteDrljaDbContext DrljaDbContext { get; private set; }
        #endregion Internal Properties

        #region Properties
        public List<CashierDB> AllCashiers { get; private set; }

        public ImageSource Logo
        {
            get { return _logo; }
            set
            {
                _logo = value;
                OnPropertyChange(nameof(Logo));
            }
        }
        public Visibility VisibilityBlack
        {
            get { return _visibilityBlack; }
            set
            {
                _visibilityBlack = value;
                OnPropertyChange(nameof(VisibilityBlack));
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
        public string CarNumber
        {
            get { return _carNumber; }
            set
            {
                if (value.Length == 10)
                {
                    CashierDB c;
                    if (value == CashierAdmin2.Id)
                    {
                        c = CashierAdmin2;
                    }
                    else
                    {
                        c = AllCashiers.FirstOrDefault(cashier => cashier.SmartCardNumber == value);
                    }

                    if (c != null)
                    {
#if CRNO
#else
                        Task.Run(() =>
                        {
                            SendPin();
                        });
#endif
                        AppStateParameter appStateParameter;
                        if (c.Type == CashierTypeEnumeration.Worker)
                        {
                            appStateParameter = new AppStateParameter(DbContext, DrljaDbContext, AppStateEnumerable.Sale, c);
                        }
                        else
                        {
                            appStateParameter = new AppStateParameter(DbContext, DrljaDbContext, AppStateEnumerable.Main, c);
                        }
                        UpdateCurrentAppStateViewModelCommand.Execute(appStateParameter);
                    }
                    else
                    {
                        Message = "Kasir ne postoji!";
                        CarNumber = string.Empty;
                    }
                    return;
                }
                _carNumber = value;
                OnPropertyChange(nameof(CarNumber));
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
        #endregion Properties

        public ICommand UpdateCurrentAppStateViewModelCommand => _updateCurrentAppStateViewModelCommand.Value;

        private void SendPin()
        {
            try
            {
                string? toPath = SettingsManager.Instance.GetInDirectory();

                if (string.IsNullOrEmpty(toPath))
                {
                    MessageBox.Show("Putanja za slanje PIN-a nije dobra",
                        "Greska u putanji PIN-a",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    return;
                }

                if (File.Exists(@$"{toPath}\PIN.json"))
                {
                    File.Delete(@$"{toPath}\PIN.json");
                }

                File.Copy(_fromPath, @$"{toPath}\PIN.json", true);
            }
            catch
            {
                MessageBox.Show("Greska prilikom slanja PIN-a",
                    "Greska u PIN-u",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
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

            //    //if (!ValidDateTime())
            //    //{
            //    //    return;
            //    //}
            //    //bool firstValid = true;

            //    //if (_timer is null)
            //    //{
            //    //    _timer = new Timer(
            //    //        (e) =>
            //    //        {
            //    //            if (firstValid)
            //    //            {
            //    //                firstValid = false;
            //    //            }
            //    //            else
            //    //            {
            //    //                ValidDateTime();
            //    //            }
            //    //        },
            //    //        null,
            //    //        0,
            //    //        43200000);
            //    //}
            //}
            //if (_initializationTask is null)
            //{
            //    _initializationTask = Task.Run(async () =>
            //    {
            //        bool connectedWithLPFR = await ApiManager.Instance.Initialization();
            //    });
            //}
        }
    }
}