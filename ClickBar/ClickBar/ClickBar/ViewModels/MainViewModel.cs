using ClickBar.Commands;
using ClickBar.State.Navigators;
using ClickBar.ViewModels.Activation;
using ClickBar.ViewModels.Login;
using ClickBar_DatabaseSQLManager;
using ClickBar_DatabaseSQLManager.Models;
using ClickBar_Settings;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Input;
using ClickBar_Logging;
using ClickBar_Database_Drlja;
using ClickBar.Commands.AppMain;
using ClickBar.ViewModels;

namespace ClickBar.ViewModels
{
    public class MainViewModel : ViewModelBase, INavigator
    {
        private readonly IServiceProvider _serviceProvider;
        private ProcessManager _processManager;
        private ViewModelBase _currentViewModel;
        private System.Windows.Forms.NotifyIcon _notifyIcon;

        public MainViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            bool isActivationCodeNumberRequired = SettingsManager.Instance.IsActivationCodeNumberRequired();

            if (isActivationCodeNumberRequired)
            {
                CurrentViewModel = _serviceProvider.GetRequiredService<ActivationViewModel>();
            }
            else
            {
                if (SettingsManager.Instance.EnableSmartCard())
                {
                    CurrentViewModel = _serviceProvider.GetRequiredService<LoginCardViewModel>();
                }
                else
                {
                    CurrentViewModel = _serviceProvider.GetRequiredService<LoginViewModel>();
                }
            }

            _notifyIcon = new System.Windows.Forms.NotifyIcon();
            _notifyIcon.DoubleClick += (s, args) => ShowMainWindow();
            _notifyIcon.Icon = new Icon("icon.ico");
            _notifyIcon.Visible = true;
            CreateContextMenu();

            try
            {
                if (SettingsManager.Instance.GetRunPorudzbineServis())
                {
                    backgroundWorker_DoWork();
                }
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("MainViewModel - MainViewModel -> greska prilikom pokretanja servera za porudzbine: "), ex);
            }

            UpdateCurrentViewModelCommand = serviceProvider.GetRequiredService<UpdateCurrentAppStateViewModelCommand>();
        }

        #region Properties
        public bool IsExit { get; private set; }
        public Window Window { get; set; }

        public ViewModelBase CurrentViewModel
        {
            get { return _currentViewModel; }
            set
            {
                if (_currentViewModel != value)
                {
                    _currentViewModel = value;
                    OnPropertyChange(nameof(CurrentViewModel));
                }
            }
        }
        #endregion Properties

        public ICommand UpdateCurrentViewModelCommand { get; }
        public ICommand HiddenWindowCommand => new HiddenWindowCommand(Window);

        public CashierDB LoggedCashier { get; set; }

        #region Private Method
        private void CreateContextMenu()
        {
            _notifyIcon.ContextMenuStrip =
              new System.Windows.Forms.ContextMenuStrip();
            _notifyIcon.Text = "CSS ClickBar";
            _notifyIcon.ContextMenuStrip.Items.Add("Otvori...").Click += (s, e) => ShowMainWindow();
            _notifyIcon.ContextMenuStrip.Items.Add("Zatvori").Click += (s, e) => ExitApplication();
        }
        private void ExitApplication()
        {
            if (SettingsManager.Instance.GetRunPorudzbineServis())
            {
                if (_processManager.IsProcessRunning())
                {
                    _processManager.KillProcess();
                }
            }
            IsExit = true;
            Application.Current.Shutdown();
            _notifyIcon.Dispose();
            _notifyIcon = null;
        }
        private void ShowMainWindow()
        {
            if (!Window.IsVisible || Window.WindowState == WindowState.Minimized)
            {
                Window.WindowState = WindowState.Normal;
                Window.Activate();
            }
            else
            {
                Window.Show();
            }
        }
        private void backgroundWorker_DoWork()
        {
            try
            {
                string processName = "ClickBar_Porudzbine.Server";
                string adminPath = SettingsManager.Instance.GetPathToPorudzbineServer();
                string processPath = Path.Combine(adminPath, $"{processName}.exe");

                _processManager = new ProcessManager(processName, processPath);

                if (!string.IsNullOrEmpty(adminPath) && File.Exists(processPath))
                {
                    if (_processManager.IsProcessRunning())
                    {
                        _processManager.KillProcess();
                    }

                    var isRuning = _processManager.StartProcess(
                        arguments: "--urls \"https://*:44323;http://*:5000\"",
                        workingDirectory: adminPath
                    );
                    Log.Debug("MainViewModel -> backgroundWorker_DoWork -> Server uspešno pokrenut sa Name: " + processName);
                }
                else
                {
                    MessageBox.Show("MainViewModel -> backgroundWorker_DoWork -> Greška prilikom pokretanja servisa!",
                        "Greška",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    Log.Error("MainViewModel -> backgroundWorker_DoWork -> Putanja do servera nije ispravna ili exe fajl ne postoji.");
                }
            }
            catch (Exception ex)
            {
                Log.Error("MainViewModel -> backgroundWorker_DoWork -> Greška prilikom pokretanja _PORUDZBINE", ex);
            }
        }
        #endregion Private Method
    }
}