using ClickBar.Commands.AppMain;
using ClickBar.Commands.AppMain.AuxiliaryWindows;
using ClickBar.Commands.Login;
using ClickBar.State.Navigators;
using ClickBar.ViewModels.AppMain;
using ClickBar_Common.Enums;
using ClickBar_Database_Drlja;
using ClickBar_DatabaseSQLManager;
using ClickBar_DatabaseSQLManager.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using ClickBar.Commands;

namespace ClickBar.ViewModels
{
    public class AppMainViewModel : ViewModelBase, INavigator
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Lazy<UpdateCurrentAppStateViewModelCommand> _updateCurrentAppStateViewModelCommand;

        private CashierDB _loggedCashier;
        private string _cashierNema;
        private string _resizeWindowIconPath;

        private ViewModelBase _currentViewModel;

        private Uri _connectionWithLPFR;
        private Visibility _adminVisibility;
        private bool _isCheckedReport;
        private bool _isCheckedSettings;
        private bool _isCheckedAdmin;
        private bool _isCheckedStatistics;

        private readonly static string _connectedImagePath = @"pack://application:,,,/Icons/signal.png";
        private readonly static string _notConnectedImagePath = @"pack://application:,,,/Icons/no-signal.png";
        private readonly Uri _connected = new Uri(_connectedImagePath);
        private readonly Uri _notConnected = new Uri(_notConnectedImagePath);

        private SettingsViewModel _settingsViewModel;
        private AdminViewModel _adminViewModel;

        private Timer _timer;

        public AppMainViewModel(IServiceProvider serviceProvider, 
            IDbContextFactory<SqlServerDbContext> dbContextFactory,
            IDbContextFactory<SqliteDrljaDbContext>? drljaDbContextFactory)
        {
            _serviceProvider = serviceProvider;
            DbContext = dbContextFactory.CreateDbContext();
            DrljaDbContext = drljaDbContextFactory != null ? drljaDbContextFactory.CreateDbContext() : null;
            _loggedCashier = serviceProvider.GetRequiredService<CashierDB>();
            _updateCurrentAppStateViewModelCommand = new Lazy<UpdateCurrentAppStateViewModelCommand>(() => serviceProvider.GetRequiredService<UpdateCurrentAppStateViewModelCommand>());

            ConnectionWithLPFR = _notConnected;
            Initialization();

            _settingsViewModel = serviceProvider.GetRequiredService<SettingsViewModel>();
            _settingsViewModel.MainViewModel = this;

            if (_loggedCashier.Type == CashierTypeEnumeration.Admin)
            {
                _adminViewModel = serviceProvider.GetRequiredService<AdminViewModel>();
            }

            LoggedCashier = _loggedCashier;
            CashierNema = _loggedCashier.Name;

            CheckedReport();

            AdminVisibility = _loggedCashier.Type == CashierTypeEnumeration.Admin ? Visibility.Visible : Visibility.Hidden;
        }

        #region Internal Properties
        internal SqlServerDbContext DbContext { get; private set; }
        internal SqliteDrljaDbContext DrljaDbContext { get; private set; }
        #endregion Internal Properties

        #region Properties
        public Uri ConnectionWithLPFR
        {
            get { return _connectionWithLPFR; }
            set
            {
                _connectionWithLPFR = value;
                OnPropertyChange(nameof(ConnectionWithLPFR));
            }
        }
        public bool IsCheckedReport
        {
            get { return _isCheckedReport; }
            set
            {
                _isCheckedReport = value;
                OnPropertyChange(nameof(IsCheckedReport));
            }
        }
        public bool IsCheckedSettings
        {
            get { return _isCheckedSettings; }
            set
            {
                _isCheckedSettings = value;
                OnPropertyChange(nameof(IsCheckedSettings));
            }
        }
        public bool IsCheckedStatistics
        {
            get { return _isCheckedStatistics; }
            set
            {
                _isCheckedStatistics = value;
                OnPropertyChange(nameof(IsCheckedStatistics));
            }
        }
        public bool IsCheckedAdmin
        {
            get { return _isCheckedAdmin; }
            set
            {
                _isCheckedAdmin = value;
                OnPropertyChange(nameof(IsCheckedAdmin));
            }
        }
        public Visibility AdminVisibility
        {
            get { return _adminVisibility; }
            set
            {
                _adminVisibility = value;
                OnPropertyChange(nameof(AdminVisibility));
            }
        }
        public string ResizeWindowIconPath
        {
            get { return _resizeWindowIconPath; }
            set
            {
                _resizeWindowIconPath = value;
                OnPropertyChange(nameof(ResizeWindowIconPath));
            }
        }
        public string CashierNema
        {
            get { return _cashierNema; }
            set
            {
                _cashierNema = value;
                OnPropertyChange(nameof(CashierNema));
            }
        }

        public ViewModelBase CurrentViewModel
        {
            get { return _currentViewModel; }
            set
            {
                _currentViewModel = value;
                OnPropertyChange(nameof(CurrentViewModel));
            }
        }
        public CashierDB LoggedCashier { get; set; }
        #endregion Properties

        #region Commands
        public ICommand UpdateAppViewModelCommand => _updateCurrentAppStateViewModelCommand.Value;
        public ICommand UpdateCurrentViewModelCommand => new UpdateCurrentViewModelCommand(this);
        public ICommand LogoutCommand => new LogoutCommand(this, _serviceProvider);
        public ICommand InformationCommand => new InformationCommand();
        #endregion Commands

        public void CheckedReport()
        {
            CurrentViewModel = new ReportViewModel(_serviceProvider);
            IsCheckedReport = true;
            IsCheckedAdmin = false;
            IsCheckedStatistics = false;
            IsCheckedSettings = false;
        }
        public void CheckedStatistics()
        {
            CurrentViewModel = new StatisticsViewModel(_serviceProvider) 
            {
                MainViewModel = this
            };
            IsCheckedReport = false;
            IsCheckedAdmin = false;
            IsCheckedStatistics = true;
            IsCheckedSettings = false;
        }
        public void CheckedSettings()
        {
            if (_loggedCashier.Type == CashierTypeEnumeration.Admin)
            {
                CurrentViewModel = _settingsViewModel;
                IsCheckedReport = false;
                IsCheckedAdmin = false;
                IsCheckedStatistics = false;
                IsCheckedSettings = true;
            }
        }
        public void CheckedAdmin()
        {
            if (_loggedCashier.Type == CashierTypeEnumeration.Admin)
            {
                CurrentViewModel = _adminViewModel;
                IsCheckedReport = false;
                IsCheckedAdmin = true;
                IsCheckedStatistics = false;
                IsCheckedSettings = false;
            }
        }

        private void Initialization()
        {
            //if (_timer is null)
            //{
            //    _timer = new Timer(
            //        async (e) =>
            //        {
            //            bool connectedWithLPFR = await ApiManager.Instance.Attention();

            //            if (connectedWithLPFR)
            //            {
            //                ConnectionWithLPFR = _connected;
            //            }
            //            else
            //            {
            //                ConnectionWithLPFR = _notConnected;
            //            }
            //        },
            //        null,
            //        0,
            //        3000);
            //}
        }
    }
}