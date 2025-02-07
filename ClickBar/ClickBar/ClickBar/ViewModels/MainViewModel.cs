using ClickBar.Commands;
using ClickBar.State.Navigators;
using ClickBar.ViewModels.Activation;
using ClickBar.ViewModels.Login;
using ClickBar_Database;
using ClickBar_Database.Models;
using ClickBar_Settings;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Threading;
using System.IO;
using System.Diagnostics;
using ClickBar_Logging;
using ClickBar_Database_Drlja;


namespace ClickBar.ViewModels
{
    public class MainViewModel : ViewModelBase, INavigator
    {
        private ProcessManager _processManager;

        private ViewModelBase _currentViewModel;

        private System.Windows.Forms.NotifyIcon _notifyIcon;
        public MainViewModel(Window window)
        {
            Window = window;
            string pathToDB = SettingsManager.Instance.GetPathToDB();

            string? pathToMainDB = SettingsManager.Instance.GetPathToMainDB();

            if (!string.IsNullOrEmpty(pathToMainDB))
            {
                if (File.Exists(pathToMainDB))
                {
                    pathToDB = pathToMainDB;
                }
                else
                {
                    MessageBox.Show("Greška u konfiguraciji putanje do baze podataka!\nProverite da li je GLAVNI računar pokrenut.",
                        "Greška u konfiguraciji",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }
            }

            using (SqliteDbContext sqliteDbContext = new SqliteDbContext())
            {
                var isConnected = sqliteDbContext.ConfigureDatabase(pathToDB).Result;

                if (!isConnected)
                {
                    MessageBox.Show("Greška u konekciji sa bazom podataka!\nObratite se serviseru.",
                        "Greška u konekciji",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                string? pathToDrljaDB = SettingsManager.Instance.GetPathToDrljaKuhinjaDB();
                if (!string.IsNullOrEmpty(pathToDrljaDB))
                {
                    using (SqliteDrljaDbContext sqliteDrljaDbContext = new SqliteDrljaDbContext())
                    {
                        var isConnectedDrlja = sqliteDrljaDbContext.ConfigureDatabase(pathToDrljaDB).Result;

                        if (!isConnectedDrlja)
                        {
                            MessageBox.Show("Greška u konekciji sa bazom podataka KUHINJE!\nObratite se serviseru.",
                                "Greška u konekciji",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                            return;
                        }
                    }
                }

                bool isActivationCodeNumberRequired = SettingsManager.Instance.IsActivationCodeNumberRequired();

                if (isActivationCodeNumberRequired)
                {
                    CurrentViewModel = new ActivationViewModel(this);
                }
                else
                {
                    if (SettingsManager.Instance.EnableSmartCard())
                    {
                        CurrentViewModel = new LoginCardViewModel(UpdateCurrentViewModelCommand);
                    }
                    else
                    {
                        CurrentViewModel = new LoginViewModel(UpdateCurrentViewModelCommand);
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
                //try
                //{
                //    ApiHelper.StartApi(new string[] { });
                //}
                //catch (Exception ex)
                //{
                //    MessageBox.Show($"An error occurred: {ex.Message}");
                //}
            }
        }

        public bool IsExit { get; private set; }
        public Window Window { get; private set; }

        public ViewModelBase CurrentViewModel
        {
            get { return _currentViewModel; }
            set
            {
                _currentViewModel = value;
                OnPropertyChange(nameof(CurrentViewModel));
            }
        }
        public ICommand UpdateCurrentViewModelCommand => new UpdateCurrentAppStateViewModelCommand(this);
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
            //ApiHelper.StopApi();
            IsExit = true;
            Application.Current.Shutdown();
            _notifyIcon.Dispose();
            _notifyIcon = null;
        }
        private void ShowMainWindow()
        {
            if (Window.IsVisible)
            {
                if (Window.WindowState == WindowState.Minimized)
                {
                    Window.WindowState = WindowState.Normal;
                }
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
                    // If process is already running, kill it
                    if (_processManager.IsProcessRunning())
                    {
                        _processManager.KillProcess();
                    }

                    // Start new process and store its GUID
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
