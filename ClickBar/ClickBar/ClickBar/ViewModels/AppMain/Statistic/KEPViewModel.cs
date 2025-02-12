using ClickBar.Commands.AppMain.Statistic.KEP;
using ClickBar.Models.AppMain.Statistic.KEP;
using ClickBar_DatabaseSQLManager;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;

namespace ClickBar.ViewModels.AppMain.Statistic
{
    public class KEPViewModel : ViewModelBase
    {
        #region Fields
        private DateTime _fromDate;
        private DateTime _toDate;

        private ObservableCollection<string> _typesKEP;
        private string _currentTypeKEP;

        private ObservableCollection<ItemKEP> _itemsKEP;

        private decimal _zaduzenje;
        private decimal _razduzenje;
        private decimal _saldo;

        private readonly IServiceProvider _serviceProvider; // Dodato za korišćenje IServiceProvider
        #endregion Fields

        #region Constructors
        public KEPViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            DbContext = serviceProvider.GetRequiredService<SqlServerDbContext>();
            TypesKEP = new ObservableCollection<string>()
            {
                "Sve",
                "Dnevni pazar",
                "Kalkulacije",
                "Nivelacije",
                "Povratnica - KUPAC",
                "Povratnica - Dobavljač",
                "Otpis"
            };

            CurrentTypeKEP = TypesKEP[0];

            DateTime dateTime = DateTime.Now;

            FromDate = new DateTime(dateTime.Year, 1, 1, 0, 0, 0);
            ToDate = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 23, 59, 59);

            SearchCommand.Execute(null);
        }
        #endregion Constructors

        #region Properties internal
        internal SqlServerDbContext DbContext { get; private set; }
        #endregion Properties internal

        #region Properties
        public DateTime FromDate
        {
            get { return _fromDate; }
            set
            {
                _fromDate = value;
                OnPropertyChange(nameof(FromDate));
            }
        }
        public DateTime ToDate
        {
            get { return _toDate; }
            set
            {
                _toDate = value;
                OnPropertyChange(nameof(ToDate));
            }
        }
        public decimal Zaduzenje
        {
            get { return _zaduzenje; }
            set
            {
                _zaduzenje = value;
                OnPropertyChange(nameof(Zaduzenje));
            }
        }
        public decimal Razduzenje
        {
            get { return _razduzenje; }
            set
            {
                _razduzenje = value;
                OnPropertyChange(nameof(Razduzenje));
            }
        }
        public decimal Saldo
        {
            get { return _saldo; }
            set
            {
                _saldo = value;
                OnPropertyChange(nameof(Saldo));
            }
        }
        public ObservableCollection<string> TypesKEP
        {
            get { return _typesKEP; }
            set
            {
                _typesKEP = value;
                OnPropertyChange(nameof(TypesKEP));
            }
        }
        public string CurrentTypeKEP
        {
            get { return _currentTypeKEP; }
            set
            {
                _currentTypeKEP = value;
                OnPropertyChange(nameof(CurrentTypeKEP));
            }
        }
        public ObservableCollection<ItemKEP> ItemsKEP
        {
            get { return _itemsKEP; }
            set
            {
                _itemsKEP = value;
                OnPropertyChange(nameof(ItemsKEP));
            }
        }
        #endregion Properties

        #region Commands
        public ICommand SearchCommand => new SearchCommand(this);
        public ICommand PrintKEPCommand => new PrintKEPCommand(this);
        #endregion Commands

        #region Public methods
        #endregion Public methods

        #region Private methods
        #endregion Private methods
    }
}