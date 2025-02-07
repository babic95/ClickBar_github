using ClickBar_Common.Enums;
using ClickBar_Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ClickBar.Models.AppMain.Settings
{
    public class Settings : ObservableObject
    {
        private string _urlToLPFR;
        private bool _enableSuperGroup;
        private bool _enableSmartCard;
        private bool _enableTableOverview;
        private string _printerName;
        private string? _printerNameSank1;
        private string? _printerNameKuhinja;
        private bool _a4Format;
        private bool _pos80mmFormat;
        private bool _enableFileSystemWatcher;
        private string _inDirectory;
        private string _outDirectory;
        private string? _efakturaDirectory;
        private Visibility _posVisibility;
        private Visibility _fileSystemWatcherVisibility;
        private bool _cancelOrderFromTable;
        private Visibility _cancelOrderFromTableVisibility;
        private string? _pathToDrljaKuhinjaDB;
        private string? _pathToMainDB;
        private string? _hostPC_IP;
        private bool _runPorudzbineServis;

        public Settings(SettingsFile settingsFile)
        {
            UrlToLPFR = settingsFile.UrlToLPFR;
            EnableTableOverview = settingsFile.EnableTableOverview;
            EnableSuperGroup = settingsFile.EnableSuperGroup;
            EnableSmartCard = settingsFile.EnableSmartCard;
            CancelOrderFromTable = settingsFile.CancelOrderFromTable;
            PrinterName = settingsFile.PrinterName;
            PrinterNameSank1 = settingsFile.PrinterNameSank1;
            PrinterNameKuhinja = settingsFile.PrinterNameKuhinja;
            PathToDrljaKuhinjaDB = settingsFile.PathToDrljaKuhinjaDB;
            PathToMainDB = settingsFile.PathToMainDB;
            HostPC_IP = settingsFile.HostPC_IP;

            switch (settingsFile.PrinterFormat)
            {
                case PrinterFormatEnumeration.A4:
                    A4Format = true;
                    Pos80mmFormat = false;
                    break;
                case PrinterFormatEnumeration.Pos80mm:
                    A4Format = false;
                    Pos80mmFormat = true;
                    break;
                case PrinterFormatEnumeration.Pos58mm:
                    A4Format = false;
                    Pos80mmFormat = false;
                    break;
            }

            EnableFileSystemWatcher = settingsFile.EnableFileSystemWatcher;
            InDirectory = settingsFile.InDirectory;
            OutDirectory = settingsFile.OutDirectory;
            EfakturaDirectory = settingsFile.EfakturaDirectory;
            RunPorudzbineServis = settingsFile.RunPorudzbineServis;
        }
        public bool RunPorudzbineServis
        {
            get { return _runPorudzbineServis; }
            set
            {
                _runPorudzbineServis = value;
                OnPropertyChange(nameof(RunPorudzbineServis));
            }
        }
        public Visibility CancelOrderFromTableVisibility
        {
            get { return _cancelOrderFromTableVisibility; }
            set
            {
                _cancelOrderFromTableVisibility = value;
                OnPropertyChange(nameof(CancelOrderFromTableVisibility));
            }
        }
        public Visibility FileSystemWatcherVisibility
        {
            get { return _fileSystemWatcherVisibility; }
            set
            {
                _fileSystemWatcherVisibility = value;
                OnPropertyChange(nameof(FileSystemWatcherVisibility));
            }
        }
        public Visibility PosVisibility
        {
            get { return _posVisibility; }
            set
            {
                _posVisibility = value;
                OnPropertyChange(nameof(PosVisibility));
            }
        }
        public string UrlToLPFR
        {
            get { return _urlToLPFR; }
            set
            {
                _urlToLPFR = value;
                OnPropertyChange(nameof(UrlToLPFR));
            }
        }

        public bool EnableSuperGroup
        {
            get { return _enableSuperGroup; }
            set
            {
                _enableSuperGroup = value;
                OnPropertyChange(nameof(EnableSuperGroup));
            }
        }
        public bool EnableSmartCard
        {
            get { return _enableSmartCard; }
            set
            {
                _enableSmartCard = value;
                OnPropertyChange(nameof(EnableSmartCard));
            }
        }
        public bool CancelOrderFromTable
        {
            get { return _cancelOrderFromTable; }
            set
            {
                _cancelOrderFromTable = value;
                OnPropertyChange(nameof(CancelOrderFromTable));
            }
        }
        public bool EnableTableOverview
        {
            get { return _enableTableOverview; }
            set
            {
                _enableTableOverview = value;
                OnPropertyChange(nameof(EnableTableOverview));

                if (value)
                {
                    CancelOrderFromTableVisibility = Visibility.Visible;
                }
                else
                {
                    CancelOrderFromTableVisibility = Visibility.Hidden;
                }
            }
        }

        public string PrinterName
        {
            get { return _printerName; }
            set
            {
                _printerName = value;
                OnPropertyChange(nameof(PrinterName));
            }
        }

        public string? PrinterNameSank1
        {
            get { return _printerNameSank1; }
            set
            {
                _printerNameSank1 = value;
                OnPropertyChange(nameof(PrinterNameSank1));
            }
        }

        public string? PrinterNameKuhinja
        {
            get { return _printerNameKuhinja; }
            set
            {
                _printerNameKuhinja = value;
                OnPropertyChange(nameof(PrinterNameKuhinja));
            }
        }

        public string? PathToDrljaKuhinjaDB
        {
            get { return _pathToDrljaKuhinjaDB; }
            set
            {
                _pathToDrljaKuhinjaDB = value;
                OnPropertyChange(nameof(PathToDrljaKuhinjaDB));
            }
        }
        public string? PathToMainDB
        {
            get { return _pathToMainDB; }
            set
            {
                _pathToMainDB = value;
                OnPropertyChange(nameof(PathToMainDB));
            }
        }
        public string? HostPC_IP
        {
            get { return _hostPC_IP; }
            set
            {
                _hostPC_IP = value;
                OnPropertyChange(nameof(HostPC_IP));
            }
        }

        public bool A4Format
        {
            get { return _a4Format; }
            set
            {
                _a4Format = value;
                OnPropertyChange(nameof(A4Format));

                if (_a4Format)
                {
                    PosVisibility = Visibility.Hidden;
                }
                else
                {
                    PosVisibility = Visibility.Visible;
                }
            }
        }

        public bool Pos80mmFormat
        {
            get { return _pos80mmFormat; }
            set
            {
                _pos80mmFormat = value;
                OnPropertyChange(nameof(Pos80mmFormat));
            }
        }

        public bool EnableFileSystemWatcher
        {
            get { return _enableFileSystemWatcher; }
            set
            {
                _enableFileSystemWatcher = value;
                OnPropertyChange(nameof(EnableFileSystemWatcher));

                if (_enableFileSystemWatcher)
                {
                    FileSystemWatcherVisibility = Visibility.Visible;
                }
                else
                {
                    FileSystemWatcherVisibility = Visibility.Hidden;
                }
            }
        }

        public string InDirectory
        {
            get { return _inDirectory; }
            set
            {
                _inDirectory = value;
                OnPropertyChange(nameof(InDirectory));
            }
        }

        public string OutDirectory
        {
            get { return _outDirectory; }
            set
            {
                _outDirectory = value;
                OnPropertyChange(nameof(OutDirectory));
            }
        }

        public string? EfakturaDirectory
        {
            get { return _efakturaDirectory; }
            set
            {
                _efakturaDirectory = value;
                OnPropertyChange(nameof(EfakturaDirectory));
            }
        }
        
    }
}
