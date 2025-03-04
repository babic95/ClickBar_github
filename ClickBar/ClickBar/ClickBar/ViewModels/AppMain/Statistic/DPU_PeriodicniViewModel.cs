using ClickBar.Commands.AppMain.Statistic.DPU;
using ClickBar.Commands.AppMain.Statistic.Knjizenje;
using ClickBar.Models.AppMain.Statistic.DPU;
using ClickBar.Models.AppMain.Statistic.Knjizenje;
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
using System.Windows.Input;

namespace ClickBar.ViewModels.AppMain.Statistic
{
    public class DPU_PeriodicniViewModel : ViewModelBase
    {
        #region Fields
        private DateTime _fromDate;
        private DateTime _toDate;
        private ObservableCollection<DPU_Item> _items;
        private readonly IServiceProvider _serviceProvider; // Dodato za korišćenje IServiceProvider
        #endregion Fields

        #region Constructors
        public DPU_PeriodicniViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            DbContext = _serviceProvider.GetRequiredService<IDbContextFactory<SqlServerDbContext>>().CreateDbContext();
            FromDate = DateTime.Now;
            ToDate = DateTime.Now;

            SearchDPUItemsCommand.Execute(null);
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
        public ObservableCollection<DPU_Item> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                OnPropertyChange(nameof(Items));
            }
        }
        #endregion Properties

        #region Commands
        public ICommand SearchDPUItemsCommand => new SearchDPUItemsCommand(this);
        #endregion Commands

        #region Public methods
        #endregion Public methods

        #region Private methods
        #endregion Private methods
    }
}