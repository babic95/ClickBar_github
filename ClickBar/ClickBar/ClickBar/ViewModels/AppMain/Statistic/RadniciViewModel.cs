using ClickBar.Commands.AppMain.Statistic.Radnici;
using ClickBar.Models.AppMain.Statistic;
using ClickBar.Models.AppMain.Statistic.Radnici;
using ClickBar_DatabaseSQLManager;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ClickBar.ViewModels.AppMain.Statistic
{
    public class RadniciViewModel : ViewModelBase
    {
        #region Fields
        private Radnik _currentRadnik;
        private ObservableCollection<Radnik> _radnici;

        private string _searchText;

        private string _title;

        private bool _isEdited;

        private ObservableCollection<WhatDidWorkerSell> _whatDidWorkerSells;
        private decimal _total;

        private readonly IServiceProvider _serviceProvider; // Dodato za korišćenje IServiceProvider
        #endregion Fields

        #region Constructors
        public RadniciViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            DbContext = serviceProvider.GetRequiredService<SqlServerDbContext>();

            DbContext.Cashiers.ToList().ForEach(x =>
            {
                RadniciAll.Add(new Radnik(x));
            });

            Radnici = new ObservableCollection<Radnik>(RadniciAll);
            CurrentRadnik = new Radnik();

            IsEdited = false;
        }
        #endregion Constructors

        #region Properties internal
        internal SqlServerDbContext DbContext
        {
            get; private set;
        }
        internal List<Radnik> RadniciAll = new List<Radnik>();
        internal Window Window { get; set; }
        #endregion Properties internal

        #region Properties
        public bool IsEdited
        {
            get { return _isEdited; }
            set
            {
                _isEdited = value;
                OnPropertyChange(nameof(IsEdited));
            }
        }
        public Radnik CurrentRadnik
        {
            get { return _currentRadnik; }
            set
            {
                _currentRadnik = value;
                OnPropertyChange(nameof(CurrentRadnik));
            }
        }
        public ObservableCollection<Radnik> Radnici
        {
            get { return _radnici; }
            set
            {
                _radnici = value;
                OnPropertyChange(nameof(Radnici));
            }
        }

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                OnPropertyChange(nameof(SearchText));

                if (string.IsNullOrEmpty(value))
                {
                    Radnici = new ObservableCollection<Radnik>(RadniciAll);
                }
                else
                {
                    Radnici = new ObservableCollection<Radnik>(RadniciAll.Where(radnik => radnik.Name.ToLower().Contains(value.ToLower())));
                }
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
        public ObservableCollection<WhatDidWorkerSell> WhatDidWorkerSells
        {
            get { return _whatDidWorkerSells; }
            set
            {
                _whatDidWorkerSells = value;
                OnPropertyChange(nameof(WhatDidWorkerSells));
            }
        }
        public decimal Total
        {
            get { return _total; }
            set
            {
                _total = Decimal.Round(value, 2);
                OnPropertyChange(nameof(Total));
            }
        }
        #endregion Properties

        #region Commands
        public ICommand DeleteRadnikCommand => new DeleteRadnikCommand(this);
        public ICommand EditRadnikCommand => new EditRadnikCommand(this);
        public ICommand OpenAddEditRadnikWindow => new OpenAddEditRadnikWindow(this);
        public ICommand SaveRadnikCommand => new SaveRadnikCommand(this);
        public ICommand WhatDidWorkerSellCommand => new WhatDidWorkerSellCommand(this);
        #endregion Commands

        #region Public methods
        #endregion Public methods

        #region Private methods
        #endregion Private methods
    }
}