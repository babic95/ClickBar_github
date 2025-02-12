using ClickBar.Commands.AppMain.Report;
using ClickBar_DatabaseSQLManager;
using ClickBar_DatabaseSQLManager.Models;
using ClickBar_Report;
using Microsoft.EntityFrameworkCore;
using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;

namespace ClickBar.ViewModels.AppMain
{
    public class ReportViewModel : ViewModelBase
    {
        #region Fields
        private CashierDB _loggedCashier;

        private string _report;
        private string _title;

        private DateTime _startReport;
        private DateTime _endReport;

        private bool _items;
        private string _smartCard;
        private readonly IServiceProvider _serviceProvider; // Dodato za korišćenje IServiceProvider
        #endregion Fields

        #region Constructors
        public ReportViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            DbContext = _serviceProvider.GetRequiredService<IDbContextFactory<SqlServerDbContext>>().CreateDbContext();
            _loggedCashier = serviceProvider.GetRequiredService<CashierDB>();
        }
        #endregion Constructors

        #region Internal Properties
        internal SqlServerDbContext DbContext { get; private set; }
        #endregion Internal Properties

        #region Properties
        public Window AuxiliaryWindow { get; set; }
        public Report CurrentReport { get; set; }
        public DateTime StartReport
        {
            get { return _startReport; }
            set
            {
                _startReport = value;
                OnPropertyChange(nameof(StartReport));
            }
        }
        public DateTime EndReport
        {
            get { return _endReport; }
            set
            {
                _endReport = value;
                OnPropertyChange(nameof(EndReport));
            }
        }
        public string Report
        {
            get { return _report; }
            set
            {
                _report = value;
                OnPropertyChange(nameof(Report));
            }
        }
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChange(nameof(Title));
            }
        }
        public bool Items
        {
            get { return _items; }
            set
            {
                _items = value;
                OnPropertyChange(nameof(Items));
            }
        }
        public string SmartCard
        {
            get { return _smartCard; }
            set
            {
                _smartCard = value;
                OnPropertyChange(nameof(SmartCard));
            }
        }
        #endregion Properties

        #region Commands
        public ICommand ReportCommand => new ReportCommand(this);
        public ICommand DayReportCommand => new DayReportCommand(this);
        public ICommand PeriodicReportCommand => new PeriodicReportCommand(this);
        public ICommand PrintReportCommand => new PrintReportCommand(this);
        public ICommand SetDateCommand => new SetDateCommand(this);
        public ICommand ExportReportCommand => new ExportReportCommand(this);
        #endregion Commands

        #region Public methods
        #endregion Public methods

        #region Private methods
        #endregion Private methods
    }
}