using ClickBar.Commands.AppMain;
using ClickBar.Commands.AppMain.Settings;
using ClickBar.Models.AppMain.Settings;
using ClickBar_DatabaseSQLManager;
using ClickBar_DatabaseSQLManager.Models;
using ClickBar_Settings;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;

namespace ClickBar.ViewModels.AppMain
{
    public class SettingsViewModel : ViewModelBase
    {
        #region Fields
        private CashierDB _loggedCashier;
        private AppMainViewModel _mainViewModel;
        private Settings _settings;

        private ObservableCollection<string> _allPrinters;
        private readonly IServiceProvider _serviceProvider; // Dodato za korišćenje IServiceProvider
        #endregion Fields

        #region Constructors
        public SettingsViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            DbContext = _serviceProvider.GetRequiredService<IDbContextFactory<SqlServerDbContext>>().CreateDbContext();
            _loggedCashier = serviceProvider.GetRequiredService<CashierDB>();

            AllPrinters = new ObservableCollection<string>();
            foreach (string printer in PrinterSettings.InstalledPrinters)
            {
                AllPrinters.Add(printer);
            }

            SettingsFile settingsFile = SettingsManager.Instance.GetSettingsFile();

            if (settingsFile is not null)
            {
                Settings = new Settings(settingsFile);
            }
        }
        #endregion Constructors

        #region Internal Properties
        internal SqlServerDbContext DbContext { get; private set; }
        #endregion Internal Properties

        #region Properties
        public AppMainViewModel MainViewModel
        {
            get => _mainViewModel;
            set => _mainViewModel = value;
        }
        public Settings Settings
        {
            get { return _settings; }
            set
            {
                _settings = value;
                OnPropertyChange(nameof(Settings));
            }
        }
        public ObservableCollection<string> AllPrinters
        {
            get { return _allPrinters; }
            set
            {
                _allPrinters = value;
                OnPropertyChange(nameof(AllPrinters));
            }
        }
        #endregion Properties

        #region Commands
        public ICommand SaveSettingsCommand => new SaveSettingsCommand(this);
        public ICommand SetInOrOutDirectoryCommand => new SetInOrOutDirectoryCommand(this);
        public ICommand ImportCommand => new ImportCommand(this);
        public ICommand ExportCommand => new ExportCommand(this);
        public ICommand MigrationDbCommand => new MigrationDbCommand(this);
        public ICommand UpdateCurrentViewModelCommand => new UpdateCurrentViewModelCommand(_mainViewModel);
        #endregion Commands

        #region Public methods
        #endregion Public methods

        #region Private methods
        #endregion Private methods
    }
}