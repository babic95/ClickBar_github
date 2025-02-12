using ClickBar.Commands.AppMain.Statistic.Norm;
using ClickBar.Models.AppMain.Statistic;
using ClickBar.Models.Sale;
using ClickBar_DatabaseSQLManager;
using DocumentFormat.OpenXml.VariantTypes;
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
    public class NormViewModel : ViewModelBase
    {
        #region Fields
        private DateTime? _fromDate;
        private DateTime? _toDate;

        private Supergroup? _currentSupergroupSearch;
        private ObservableCollection<Supergroup> _allSupergroups;

        private readonly IServiceProvider _serviceProvider; // Dodato za korišćenje IServiceProvider
        #endregion Fields

        #region Constructors
        public NormViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            DbContext = serviceProvider.GetRequiredService<SqlServerDbContext>();
            FromDate = null;
            ToDate = null;

            AllSupergroups = new ObservableCollection<Supergroup>() { new Supergroup(-1, "Sve nadgrupe") };

            if (DbContext.Supergroups != null &&
                DbContext.Supergroups.Any())
            {
                DbContext.Supergroups.ForEachAsync(supergroup =>
                {
                    AllSupergroups.Add(new Supergroup(supergroup.Id, supergroup.Name));
                });

                CurrentSupergroupSearch = AllSupergroups.FirstOrDefault();
            }
        }
        #endregion Constructors

        #region Properties internal
        internal SqlServerDbContext DbContext
        {
            get; private set;
        }
        #endregion Properties internal

        #region Properties
        public Supergroup? CurrentSupergroupSearch
        {
            get { return _currentSupergroupSearch; }
            set
            {
                _currentSupergroupSearch = value;
                OnPropertyChange(nameof(CurrentSupergroupSearch));
            }
        }
        public ObservableCollection<Supergroup> AllSupergroups
        {
            get { return _allSupergroups; }
            set
            {
                _allSupergroups = value;
                OnPropertyChange(nameof(AllSupergroups));
            }
        }
        public DateTime? FromDate
        {
            get { return _fromDate; }
            set
            {
                if (value != null &&
                    value.HasValue)
                {
                    _fromDate = new DateTime(value.Value.Year, value.Value.Month, value.Value.Day, 5, 0, 0);
                }
                else
                {
                    _fromDate = value;
                }
                OnPropertyChange(nameof(FromDate));
            }
        }
        public DateTime? ToDate
        {
            get { return _toDate; }
            set
            {
                if (value != null &&
                    value.HasValue)
                {
                    value = value.Value.AddDays(1);
                    _toDate = new DateTime(value.Value.Year, value.Value.Month, value.Value.Day, 4, 59, 59);
                }
                else
                {
                    _toDate = value;
                }
                OnPropertyChange(nameof(ToDate));
            }
        }
        #endregion Properties

        #region Commands
        public ICommand FixNormCommand => new FixNormCommand(this);
        public ICommand PrintAllNormCommand => new PrintAllNormCommand(this);
        #endregion Commands

        #region Public methods
        #endregion Public methods

        #region Private methods
        #endregion Private methods
    }
}