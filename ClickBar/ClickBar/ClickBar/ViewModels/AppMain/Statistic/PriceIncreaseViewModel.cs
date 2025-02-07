using ClickBar.Commands.AppMain.Statistic.PriceIncrease;
using ClickBar.Models.Sale;
using ClickBar_Database;
using DocumentFormat.OpenXml.Spreadsheet;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace ClickBar.ViewModels.AppMain.Statistic
{
    public class PriceIncreaseViewModel : ViewModelBase
    {
        #region Fields
        private decimal _total;
        private Brush _foregroundTotal;

        private ObservableCollection<Models.Sale.GroupItems> _allGroups;
        private Models.Sale.GroupItems _currentGroup;
        #endregion Fields

        #region Constructors
        public PriceIncreaseViewModel()
        {
            using (SqliteDbContext sqliteDbContext = new SqliteDbContext())
            {

                AllGroups = new ObservableCollection<Models.Sale.GroupItems>() { new Models.Sale.GroupItems(-1, -1, "Sve grupe") };

                if (sqliteDbContext.ItemGroups != null &&
                    sqliteDbContext.ItemGroups.Any())
                {
                    sqliteDbContext.ItemGroups.ToList().ForEach(gropu =>
                    {
                        AllGroups.Add(new Models.Sale.GroupItems(gropu.Id, gropu.IdSupergroup, gropu.Name));
                    });
                }

                CurrentGroup = AllGroups.FirstOrDefault();

                Total = 0;
            }
        }
        #endregion Constructors

        #region Properties internal
        #endregion Properties internal

        #region Properties
        public decimal Total
        {
            get { return _total; }
            set
            {
                _total = value;
                OnPropertyChange(nameof(Total));

                if(value > 0)
                {
                    ForegroundTotal = Brushes.Green;
                }
                else if(value == 0)
                {
                    ForegroundTotal = Brushes.Black;
                }
                else
                {
                    ForegroundTotal = Brushes.Red;
                }
            }
        }
        public Brush ForegroundTotal
        {
            get { return _foregroundTotal; }
            set
            {
                _foregroundTotal = value;
                OnPropertyChange(nameof(ForegroundTotal));
            }
        }
        public ObservableCollection<Models.Sale.GroupItems> AllGroups
        {
            get { return _allGroups; }
            set
            {
                _allGroups = value;
                OnPropertyChange(nameof(AllGroups));
            }
        }
        public Models.Sale.GroupItems CurrentGroup
        {
            get { return _currentGroup; }
            set
            {
                _currentGroup = value;
                OnPropertyChange(nameof(CurrentGroup));
            }
        }
        #endregion Properties

        #region Commands
        public ICommand IncreasePricesCommand => new IncreasePricesCommand(this);
        public ICommand LowerPricesCommand => new LowerPricesCommand(this);
        public ICommand SaveCommand => new SaveCommand(this);
        public ICommand SetNocneCeneCommand => new SetNocneCeneCommand(this);
        public ICommand SetDnevneCeneCommand => new SetDnevneCeneCommand(this);
        #endregion Commands

        #region Public methods
        #endregion Public methods

        #region Private methods
        #endregion Private methods
    }
}