using ClickBar.Commands.AppMain;
using ClickBar.Commands.AppMain.Settings;
using ClickBar.Models.AppMain.Settings;
using ClickBar_Database.Models;
using ClickBar_Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ClickBar.ViewModels.AppMain
{
    public class SettingsViewModel : ViewModelBase
    {
        #region Fields
        private CashierDB _loggedCashier;
        private AppMainViewModel _mainViewModel;
        private Settings _settings;

        private ObservableCollection<string> _allPrinters;
        #endregion Fields

        #region Constructors
        public SettingsViewModel(CashierDB loggedCashier, AppMainViewModel mainViewModel)
        {
            _loggedCashier = loggedCashier;
            _mainViewModel = mainViewModel;

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

        #region Properties
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
        public ICommand ImportCommand => new ImportCommand();
        public ICommand ExportCommand => new ExportCommand();
        public ICommand UpdateCurrentViewModelCommand => new UpdateCurrentViewModelCommand(_mainViewModel);
        #endregion Commands

        #region Public methods
        #endregion Public methods

        #region Private methods
        #endregion Private methods
    }
}
