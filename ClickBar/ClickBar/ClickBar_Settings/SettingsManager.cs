using ClickBar_Common.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_Settings
{
    public sealed class SettingsManager
    {
        #region Fields Singleton
        private static readonly object lockObject = new object();
        private static SettingsManager instance = null;
        #endregion Fields Singleton

        #region Fields
#if CRNO
        private static readonly string _mainFolderPath = @"c:\ClickBar";
        private static readonly string _adminFolderName = @"Admin";
#else
        private static readonly string _mainFolderPath = @"c:\CCS ClickBar";
        private static readonly string _adminFolderName = @"ClickBar_Admin";
#endif
        private static readonly string _loggingFolderName = @"Logging";
        private static readonly string _excelFolderName = @"Excel";
        private static readonly string _excelImportFolderName = @"Import";
        private static readonly string _excelExportFolderName = @"Export";
        private static readonly string _databaseName = @"ClickBar_DB.db";
        private static readonly string _sqlServerDatabaseName = "ClickBar_DB";
        private static readonly string _masterDatabaseConnectionString = @"Data Source=.\ClickBar_SSMS;Initial Catalog=master;User ID=sa;Password=Jajajaja95!;MultipleActiveResultSets=True;TrustServerCertificate=True";
        private static readonly string _databaseConnectionString = @"Data Source=.\ClickBar_SSMS;Initial Catalog=ClickBar_DB;User ID=sa;Password=Jajajaja95!;MultipleActiveResultSets=True;TrustServerCertificate=True";
        private static readonly string _appFileName = @"app.json";
        private static readonly string _model = "CCS ClickBar";
        private static readonly string _logo = "logo.png";
        private static readonly string _urlToCCS_Server_Local = @"http://localhost:5106/";
        private static readonly string _urlToCCS_Server = @"https://api.cleancodesirmium.com";

        private static readonly string _ib = "79";
        private static readonly string _version = "1.0.0";
        private static readonly string _make = "NEMANJA BABIĆ PR ZA RAČUNARSKO PROGRAMIRANJE CLEAN CODE SIRMIUM";

        private static readonly string _supergroups = "Nadgrupe.xlsx";
        private static readonly string _groups = "Grupe.xlsx";
        private static readonly string _cashiers = "Kasiri.xlsx";
        private static readonly string _items = "Proizvodi.xlsx";

        private static SettingsFile _settingsFile;
        private static readonly string _pathToLogo = Path.Combine(_mainFolderPath, _logo);

        private static readonly string _pathToAdminFolder = Path.Combine(_mainFolderPath, _adminFolderName);
        private static readonly string _pathToLoggingFolder = Path.Combine(_mainFolderPath, _loggingFolderName);
        private static readonly string _pathToAppJsonFile = Path.Combine(_pathToAdminFolder, _appFileName);

        private static readonly string _pathToExcelFolder = Path.Combine(_mainFolderPath, _excelFolderName);
        private static readonly string _pathToImportExcelFolder = Path.Combine(_pathToExcelFolder, _excelImportFolderName);
        private static readonly string _pathToExportExcelFolder = Path.Combine(_pathToExcelFolder, _excelExportFolderName);

        private static readonly string _pathToImportCashiersExcelFolder = Path.Combine(_pathToImportExcelFolder, _cashiers);
        private static readonly string _pathToImportItemsExcelFolder = Path.Combine(_pathToImportExcelFolder, _items);
        private static readonly string _pathToImportGroupsExcelFolder = Path.Combine(_pathToImportExcelFolder, _groups);
        private static readonly string _pathToImportSupergroupsExcelFolder = Path.Combine(_pathToImportExcelFolder, _supergroups);

        private static readonly string _pathToExportGroupsExcelFolder = Path.Combine(_pathToExportExcelFolder, _groups);
        private static readonly string _pathToExportCashiersExcelFolder = Path.Combine(_pathToExportExcelFolder, _cashiers);
        private static readonly string _pathToExportItemsExcelFolder = Path.Combine(_pathToExportExcelFolder, _items);
#endregion Fields

        #region Constructors
        private SettingsManager() { }
        public static SettingsManager Instance
        {
            get
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = new SettingsManager();
                        instance.CreateDefaultSettingsFile();
                        instance.LoadSettingsFile();
                    }
                    return instance;
                }
            }
        }
        #endregion Constructors

        #region Public methods
        public bool SetSettingsFile(SettingsFile settingsFile)
        {
            try
            {
                _settingsFile.EnableFileSystemWatcher = settingsFile.EnableFileSystemWatcher;
                _settingsFile.PrinterName = settingsFile.PrinterName;
                _settingsFile.EnableTableOverview = settingsFile.EnableTableOverview;
                _settingsFile.EnableSmartCard = settingsFile.EnableSmartCard;
                _settingsFile.CancelOrderFromTable = settingsFile.CancelOrderFromTable;
                _settingsFile.EnableSuperGroup = settingsFile.EnableSuperGroup;
                _settingsFile.InDirectory = settingsFile.InDirectory;
                _settingsFile.OutDirectory = settingsFile.OutDirectory;
                _settingsFile.PrinterFormat = settingsFile.PrinterFormat;
                _settingsFile.UrlToLPFR = settingsFile.UrlToLPFR;
                _settingsFile.PrinterNameKuhinja = settingsFile.PrinterNameKuhinja;
                _settingsFile.PrinterNameSank1 = settingsFile.PrinterNameSank1;
                _settingsFile.EfakturaDirectory = settingsFile.EfakturaDirectory;
                _settingsFile.PathToDrljaKuhinjaDB = settingsFile.PathToDrljaKuhinjaDB;
                _settingsFile.PathToMainDB = settingsFile.PathToMainDB;
                _settingsFile.HostPC_IP = settingsFile.HostPC_IP;
                _settingsFile.RunPorudzbineServis = settingsFile.RunPorudzbineServis;

                SaveSettingsFile();
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
        public string GetSqlServerDatabaseName()
        {
            return _sqlServerDatabaseName;
        }
        public string GetConnectionString()
        {
            return _databaseConnectionString;
        }
        public string GetConnectionStringMaster()
        {
            return _masterDatabaseConnectionString;
        }
        public bool GetRunPorudzbineServis()
        {
            return _settingsFile.RunPorudzbineServis;
        }
        public string? GetHOstPC_IP()
        {
            return string.IsNullOrEmpty(_settingsFile.HostPC_IP) ? null : _settingsFile.HostPC_IP;
        }
        public string GetPathToPorudzbineServer()
        {
            return Path.Combine(_pathToAdminFolder, @"PorudzbineServer");
        }
        public string? GetPathToMainDB()
        {
            return string.IsNullOrEmpty(_settingsFile.PathToMainDB) ? null : Path.Combine(_settingsFile.PathToMainDB, _databaseName);
        }
        public string? GetPathToDrljaKuhinjaDB()
        {
            return string.IsNullOrEmpty(_settingsFile.PathToDrljaKuhinjaDB) ? null : _settingsFile.PathToDrljaKuhinjaDB;
        }
        public string? GetEfakturaDirectory()
        {
            return string.IsNullOrEmpty(_settingsFile.EfakturaDirectory) ? null : _settingsFile.EfakturaDirectory;
        }
        public string? GetComPort()
        {
            return string.IsNullOrEmpty(_settingsFile.CopPort) ? null : _settingsFile.CopPort;
        }

        public bool GetEnableCCS_Server()
        {
            return _settingsFile.EnableCCS_Server;
        }
        public string GetUrlToCCS_Server()
        {
            return _urlToCCS_Server;
        }
        public void SetValidTo(DateTime validTo)
        {
            _settingsFile.ValidTo = validTo;
            SaveSettingsFile();
        }
        public DateTime? GetValidTo()
        {
            return _settingsFile.ValidTo;
        }
        public string GetEsirId()
        {
            return _settingsFile.EsirId;
        }
        public string GetPosNumber()
        {
            return $"{_ib}/{_version}";
        }
        public string GetPosVersion()
        {
            return $"{_version}";
        }
        public string GetPosMake()
        {
            return $"{_make}";
        }
        public SettingsFile GetSettingsFile()
        {
            return _settingsFile;
        }
        public string GetPathToDB()
        {
            return Path.Combine(_pathToAdminFolder, _databaseName);
        }
        public string? GetPathToLogo()
        {
            return _pathToLogo;
        }
        public string? GetInDirectory()
        {
            return _settingsFile is null ? null : _settingsFile.InDirectory;
        }
        public string? GetOutDirectory()
        {
            return _settingsFile is null ? null : _settingsFile.OutDirectory;
        }
        public bool EnableFileSystemWatcher()
        {
            return _settingsFile.EnableFileSystemWatcher;
        }
        public bool EnableSuperGroup()
        {
            return _settingsFile.EnableSuperGroup;
        }
        public bool EnableTableOverview()
        {
            return _settingsFile.EnableTableOverview;
        }
        public bool EnableSmartCard()
        {
            return _settingsFile.EnableSmartCard;
        }
        public bool CancelOrderFromTable()
        {
            return _settingsFile.CancelOrderFromTable;
        }
        public string? GetPathToImportCashiers()
        {
            return _pathToImportCashiersExcelFolder;
        }
        public string? GetPathToImportItems()
        {
            return _pathToImportItemsExcelFolder;
        }
        public string? GetPathToImportSupergroups()
        {
            return _pathToImportSupergroupsExcelFolder;
        }
        public string? GetPathToImportGroups()
        {
            return _pathToImportGroupsExcelFolder;
        }
        public string? GetPathToExportCashiers()
        {
            return _pathToExportCashiersExcelFolder;
        }
        public string? GetPathToExportGroups()
        {
            return _pathToExportGroupsExcelFolder;
        }
        public string? GetPathToExportItems()
        {
            return _pathToExportItemsExcelFolder;
        }
        public string? GetPathToExportExcelFolder()
        {
            return _pathToExportExcelFolder;
        }
        public string? GetPrinterName()
        {
            return _settingsFile is null ? null : _settingsFile.PrinterName;
        }
        public string? GetPrinterNameKuhinja()
        {
            return _settingsFile is null ? null : _settingsFile.PrinterNameKuhinja;
        }
        public string? GetPrinterNameSank1()
        {
            return _settingsFile is null ? null : _settingsFile.PrinterNameSank1;
        }
        public int GetNumberCopy()
        {
            return _settingsFile == null ? 2 : _settingsFile.NumberCopy;
        }
        public PrinterFormatEnumeration? GetPrinterFormat()
        {
            return _settingsFile is null ? PrinterFormatEnumeration.Pos80mm : _settingsFile.PrinterFormat;
        }
        public string? GetUrlToLPFR()
        {
            return _settingsFile is null ? null : _settingsFile.UrlToLPFR;
        }
        public string GetLoggingFolderPath()
        {
            return _pathToLoggingFolder;
        }
        public string? SetActivationCodeNumber(string activationCode)
        {
            if (!string.IsNullOrEmpty(_settingsFile.ActivationCode))
            {
                return null;
            }
            _settingsFile.ActivationCode = activationCode;
            _settingsFile.EsirId = Guid.NewGuid().ToString();

            SaveSettingsFile();

            return _settingsFile.EsirId;
        }
        public bool IsActivationCodeNumberRequired()
        {
            if (_settingsFile == null)
            {
                return true;
            }
            return string.IsNullOrEmpty(_settingsFile.ActivationCode);
        }
        public void LoadSettingsFile()
        {
            try
            {
                if (File.Exists(_pathToAppJsonFile))
                {
                    using (StreamReader r = new StreamReader(_pathToAppJsonFile))
                    {
                        string jsonString = r.ReadToEnd();
                        SettingsFile settingsJson = JsonConvert.DeserializeObject<SettingsFile>(jsonString);

                        _settingsFile = settingsJson;
                    }
                }
                else
                {
                    _settingsFile = null;
                }
            }
            catch
            {
                _settingsFile = null;
            }
        }

        public void DeleteOldLogs()
        {
            DirectoryInfo dir = new DirectoryInfo(_pathToLoggingFolder);

            List<FileInfo> logFiles = dir.GetFiles("*.log").ToList();

            if (logFiles.Count > 1)
            {
                DateTime dateTimeFromDeleting = DateTime.Now.AddDays(-30);

                var logsForDeleting = logFiles.Where(f => f.CreationTimeUtc < dateTimeFromDeleting);
                logsForDeleting.ToList().ForEach(f =>
                {
                    f.Delete();
                });
            }
        }
#endregion Public methods

        #region Private methods
        private void CreateDefaultSettingsFile()
        {
            if (!File.Exists(_pathToAppJsonFile))
            {
                if (!Directory.Exists(_mainFolderPath))
                {
                    Directory.CreateDirectory(_mainFolderPath);
                }

                if (!Directory.Exists(_pathToAdminFolder))
                {
                    CreateHiddenFolder(_pathToAdminFolder);
                }

                CreateDefaultAppJsonFile();
            }

            if (!Directory.Exists(_pathToLoggingFolder))
            {
                Directory.CreateDirectory(_pathToLoggingFolder);
            }

            if (!Directory.Exists(_pathToExcelFolder))
            {
                Directory.CreateDirectory(_pathToExcelFolder);
            }

            if (!Directory.Exists(_pathToImportExcelFolder))
            {
                Directory.CreateDirectory(_pathToImportExcelFolder);
            }

            if (!Directory.Exists(_pathToExportExcelFolder))
            {
                Directory.CreateDirectory(_pathToExportExcelFolder);
            }

            if(!Directory.Exists(GetPathToPorudzbineServer()))
            {
                Directory.CreateDirectory(GetPathToPorudzbineServer());
            }
        }
        private static void CreateHiddenFolder(string pathToHiddenFolder)
        {
            DirectoryInfo di = Directory.CreateDirectory(pathToHiddenFolder);
            di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
        }
        private void CreateDefaultAppJsonFile()
        {
#if DEBUG
            _settingsFile = new SettingsFile()
            {
                ActivationCode = null,
                EsirId = null,
                InDirectory = null,
                OutDirectory = null,
                PrinterFormat = PrinterFormatEnumeration.Pos80mm,
                PrinterName = null,
                EnableCCS_Server = false,
                UrlToCCS_Server = _urlToCCS_Server_Local,
                UrlToLPFR = "http://localhost:8989/",
                EnableFileSystemWatcher = false,
                EnableTableOverview = true,
                EnableSmartCard = false,
                CancelOrderFromTable = false,
                EnableSuperGroup = false,
                ValidTo = null,
                NumberCopy = 2,
                PrinterNameKuhinja= null,
                PrinterNameSank1 = null,
                CopPort = null,
                EfakturaDirectory = null,
                PathToDrljaKuhinjaDB = null,
                PathToMainDB = null,
                HostPC_IP = null,
                RunPorudzbineServis = false,
            };
#else
            _settingsFile = new SettingsFile()
            {
                ActivationCode = null,
                EsirId = null,
                InDirectory = null,
                OutDirectory = null,
                PrinterFormat = PrinterFormatEnumeration.Pos80mm,
                PrinterName = null,
                EnableCCS_Server = false,
                UrlToCCS_Server = _urlToCCS_Server,
                UrlToLPFR = "http://localhost:8989/",
                EnableFileSystemWatcher = false,
                EnableTableOverview = true,
                EnableSmartCard = false,
                CancelOrderFromTable = false,
                EnableSuperGroup = false,
                ValidTo = null,
                NumberCopy = 2,
                PrinterNameKuhinja= null,
                PrinterNameSank1 = null,
                CopPort = null,
                EfakturaDirectory = null,
                PathToDrljaKuhinjaDB = null,
                PathToMainDB = null,
                HostPC_IP = null,
                RunPorudzbineServis = false,
            };

#endif
            SaveSettingsFile();
        }

        private void SaveSettingsFile()
        {
            string json = JsonConvert.SerializeObject(_settingsFile);

            File.WriteAllText(_pathToAppJsonFile, json);
        }
        #endregion Private methods
    }
}
