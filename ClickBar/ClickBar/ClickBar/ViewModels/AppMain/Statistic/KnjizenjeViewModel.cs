using ClickBar.Commands.AppMain.Statistic;
using ClickBar.Commands.AppMain.Statistic.Calculation;
using ClickBar.Commands.AppMain.Statistic.Knjizenje;
using ClickBar.Models.AppMain.Statistic;
using ClickBar.Models.AppMain.Statistic.Knjizenje;
using ClickBar.Models.Sale;
using ClickBar_DatabaseSQLManager;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;

namespace ClickBar.ViewModels.AppMain.Statistic
{
    public class KnjizenjeViewModel : ViewModelBase
    {
        #region Fields
        private DateTime _currentDate;
        private ObservableCollection<Invoice> _invoices;
        private KnjizenjePazara _currentKnjizenjePazara;
        private ObservableCollection<ItemInvoice> _itemsInInvoice;
        private readonly IServiceProvider _serviceProvider; // Dodato za korišćenje IServiceProvider
        #endregion Fields

        #region Constructors
        public KnjizenjeViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            DbContext = serviceProvider.GetRequiredService<SqlServerDbContext>();
            CurrentDate = DateTime.Now;
            Invoices = new ObservableCollection<Invoice>();

            SearchInvoicesCommand.Execute(null);
        }
        #endregion Constructors

        #region Properties internal
        internal SqlServerDbContext DbContext { get; private set; }
        internal Window Window { get; set; }
        #endregion Properties internal

        #region Properties
        public DateTime CurrentDate
        {
            get { return _currentDate; }
            set
            {
                _currentDate = value;
                OnPropertyChange(nameof(CurrentDate));
            }
        }
        public ObservableCollection<Invoice> Invoices
        {
            get { return _invoices; }
            set
            {
                _invoices = value;
                OnPropertyChange(nameof(Invoices));
            }
        }
        public KnjizenjePazara CurrentKnjizenjePazara
        {
            get { return _currentKnjizenjePazara; }
            set
            {
                _currentKnjizenjePazara = value;
                OnPropertyChange(nameof(CurrentKnjizenjePazara));
            }
        }
        public ObservableCollection<ItemInvoice> ItemsInInvoice
        {
            get { return _itemsInInvoice; }
            set
            {
                _itemsInInvoice = value;
                OnPropertyChange(nameof(ItemsInInvoice));
            }
        }
        #endregion Properties

        #region Commands
        public ICommand SearchInvoicesCommand => new SearchInvoicesCommand(this);
        public ICommand OpenItemsInInvoicesCommand => new OpenItemsInInvoicesCommand(this);
        public ICommand KnjizenjeCommand => new KnjizenjeCommand(this);
        public ICommand CloseWindowCommand => new CloseWindowCommand(this);
        public ICommand PrintDnevniPazarCommand => new PrintDnevniPazarCommand(this);
        #endregion Commands

        #region Public methods
        #endregion Public methods

        #region Private methods
        #endregion Private methods
    }
}