using ClickBar.Commands.AppMain.Statistic.Knjizenje;
using ClickBar.Commands.AppMain.Statistic.Porudzbine;
using ClickBar.Models.AppMain.Statistic.Knjizenje;
using ClickBar.Models.AppMain.Statistic.Porudzbine;
using ClickBar.Models.Sale;
using ClickBar_DatabaseSQLManager;
using Microsoft.EntityFrameworkCore;
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
    public class PregledPorudzbinaNaDanViewModel : ViewModelBase
    {
        #region Fields
        private DateTime _currentDate;
        private ObservableCollection<OrderToday> _orders;
        private ObservableCollection<OrderTodayItem> _itemsInOrder;
        private readonly IServiceProvider _serviceProvider; // Dodato za korišćenje IServiceProvider

        private decimal _prodatoTotal;
        private decimal _obrisanoTotal;
        private decimal _neobradjenoTotal;
        #endregion Fields

        #region Constructors
        public PregledPorudzbinaNaDanViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            DbContext = _serviceProvider.GetRequiredService<IDbContextFactory<SqlServerDbContext>>().CreateDbContext();
            CurrentDate = DateTime.Now;

            SearchOrdersCommand.Execute(null);
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
        public ObservableCollection<OrderToday> Orders
        {
            get { return _orders; }
            set
            {
                _orders = value;
                OnPropertyChange(nameof(Orders));
            }
        }
        public ObservableCollection<OrderTodayItem> ItemsInOrder
        {
            get { return _itemsInOrder; }
            set
            {
                _itemsInOrder = value;
                OnPropertyChange(nameof(ItemsInOrder));
            }
        }
        public decimal ProdatoTotal
        {
            get { return _prodatoTotal; }
            set
            {
                _prodatoTotal = value;
                OnPropertyChange(nameof(ProdatoTotal));
            }
        }
        public decimal ObrisanoTotal
        {
            get { return _obrisanoTotal; }
            set
            {
                _obrisanoTotal = value;
                OnPropertyChange(nameof(ObrisanoTotal));
            }
        }
        public decimal NeobradjenoTotal
        {
            get { return _neobradjenoTotal; }
            set
            {
                _neobradjenoTotal = value;
                OnPropertyChange(nameof(NeobradjenoTotal));
            }
        }
        #endregion Properties

        #region Commands
        public ICommand SearchOrdersCommand => new SearchOrdersCommand(this);
        public ICommand OpenItemsInOrdersCommand => new OpenItemsInOrdersCommand(this);
        #endregion Commands

        #region Public methods
        #endregion Public methods

        #region Private methods
        #endregion Private methods
    }
}