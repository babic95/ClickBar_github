using ClickBar.Commands.AppMain.Statistic.Nivelacija;
using ClickBar.Models.AppMain.Statistic;
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
    public class ViewNivelacijaViewModel : ViewModelBase
    {
        #region Fields
        private string _totalNivelacijaString;
        private decimal _totalNivelacija;
        private string _totalOldNivelacijaString;
        private decimal _totalOldNivelacija;
        private string _totalNewNivelacijaString;
        private decimal _totalNewNivelacija;
        private string _totalPdvNivelacijaString;
        private decimal _totalPdvNivelacija;
        private ObservableCollection<Nivelacija> _nivelacije;
        private Nivelacija _currentNivelacija;

        private readonly IServiceProvider _serviceProvider; // Dodato za korišćenje IServiceProvider
        #endregion Fields

        #region Constructors
        public ViewNivelacijaViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            DbContext = serviceProvider.GetRequiredService<SqlServerDbContext>();
            Nivelacije = new ObservableCollection<Nivelacija>();

            var nivelacije = DbContext.Nivelacijas;

            if (nivelacije != null &&
                nivelacije.Any())
            {
                nivelacije.ToList().ForEach(nivelacija =>
                {
                    var niv = new Nivelacija(DbContext, nivelacija);

                    Nivelacije.Add(niv);
                });
            }
        }
        #endregion Constructors

        #region Properties internal
        internal SqlServerDbContext DbContext
        {
            get; private set;
        }
        internal decimal TotalNivelacija
        {
            get { return _totalNivelacija; }
            set
            {
                _totalNivelacija = value;

                TotalNivelacijaString = string.Format("{0:#,##0.00}", value).Replace(',', '#').Replace('.', ',').Replace('#', '.');
            }
        }
        internal decimal TotalOldNivelacija
        {
            get { return _totalOldNivelacija; }
            set
            {
                _totalOldNivelacija = value;

                TotalOldNivelacijaString = string.Format("{0:#,##0.00}", value).Replace(',', '#').Replace('.', ',').Replace('#', '.');
            }
        }
        internal decimal TotalNewNivelacija
        {
            get { return _totalNewNivelacija; }
            set
            {
                _totalNewNivelacija = value;

                TotalNewNivelacijaString = string.Format("{0:#,##0.00}", value).Replace(',', '#').Replace('.', ',').Replace('#', '.');
            }
        }
        internal decimal TotalPdvNivelacija
        {
            get { return _totalPdvNivelacija; }
            set
            {
                _totalPdvNivelacija = value;

                TotalPdvNivelacijaString = string.Format("{0:#,##0.00}", value).Replace(',', '#').Replace('.', ',').Replace('#', '.');
            }
        }
        #endregion Properties internal

        #region Properties
        public ObservableCollection<Nivelacija> Nivelacije
        {
            get { return _nivelacije; }
            set
            {
                _nivelacije = value;
                OnPropertyChange(nameof(Nivelacije));
            }
        }
        public Nivelacija CurrentNivelacija
        {
            get { return _currentNivelacija; }
            set
            {
                _currentNivelacija = value;
                OnPropertyChange(nameof(CurrentNivelacija));
            }
        }
        public string TotalNivelacijaString
        {
            get { return _totalNivelacijaString; }
            set
            {
                _totalNivelacijaString = value;
                OnPropertyChange(nameof(TotalNivelacijaString));
            }
        }
        public string TotalOldNivelacijaString
        {
            get { return _totalOldNivelacijaString; }
            set
            {
                _totalOldNivelacijaString = value;
                OnPropertyChange(nameof(TotalOldNivelacijaString));
            }
        }
        public string TotalNewNivelacijaString
        {
            get { return _totalNewNivelacijaString; }
            set
            {
                _totalNewNivelacijaString = value;
                OnPropertyChange(nameof(TotalNewNivelacijaString));
            }
        }
        public string TotalPdvNivelacijaString
        {
            get { return _totalPdvNivelacijaString; }
            set
            {
                _totalPdvNivelacijaString = value;
                OnPropertyChange(nameof(TotalPdvNivelacijaString));
            }
        }
        #endregion Properties

        #region Command
        public ICommand OpenNivelacijaItemsCommand => new OpenNivelacijaItemsCommand(this);
        public ICommand PrintNivelacijaCommand => new PrintNivelacijaCommand(this);
        #endregion Command

        #region Public methods
        #endregion Public methods

        #region Private methods
        #endregion Private methods
    }
}