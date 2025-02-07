using ClickBar.Commands.AppMain.Statistic;
using ClickBar.Commands.AppMain.Statistic.Calculation;
using ClickBar.Commands.AppMain.Statistic.Norm;
using ClickBar.Models.AppMain.Statistic;
using ClickBar.Models.Sale;
using ClickBar_Database;
using ClickBar_Database.Models;
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
    public class CalculationViewModel : ViewModelBase
    {
        #region Fields
        private ObservableCollection<Supplier> _suppliers;
        private Supplier _selectedSupplier;

        private ObservableCollection<Invertory> _inventoryStatusCalculation;
        private Invertory _currentInventoryStatusCalculation;

        private string _searchPIB;

        private ObservableCollection<Invertory> _calculations;
        private string _searchText;
        private decimal _totalCalculation;
        private string _calculationQuantityString;
        private decimal _calculationQuantity;
        private string _calculationPriceString;
        private decimal _calculationPrice; 
        private decimal _prosecnaPrice;
        private decimal _oldPrice;
        private string _newPriceString;
        private decimal _newPrice;
        private Visibility _visibilityNext;
        private string _invoiceNumber;

        private string _quantityCommandParameter;

        private ObservableCollection<GroupItems> _allGroups;
        private GroupItems _currentGroup;

        private DateTime _calculationDate;
        private Visibility _visibilityProsecnaPrice;
        private Visibility _visibilityOldPrice;
        #endregion Fields

        #region Constructors
        public CalculationViewModel(CashierDB cashierDB)
        {
            CalculationDate = DateTime.Now;
            Groups = new List<GroupItems>() { new GroupItems(-1, -1, "Sve grupe") };
            LoggedCashier = cashierDB;
            using (SqliteDbContext sqliteDbContext = new SqliteDbContext())
            {

                Suppliers = new ObservableCollection<Supplier>();
                InventoryStatusCalculation = new ObservableCollection<Invertory>();

                Calculations = new ObservableCollection<Invertory>();

                CalculationQuantity = 0;
                CalculationPrice = 0;
                NewPrice = 0;
                OldPrice = 0;
                VisibilityNext = Visibility.Hidden;

                sqliteDbContext.Suppliers.ToList().ForEach(x =>
                {
                    SuppliersAll.Add(new Supplier(x));
                });
                sqliteDbContext.Items.ToList().ForEach(x =>
                {
                    Item item = new Item(x);

                    var group = sqliteDbContext.ItemGroups.Find(x.IdItemGroup);
                    if (group != null)
                    {
                        bool isSirovina = group.Name.ToLower().Contains("sirovina") || group.Name.ToLower().Contains("sirovine") ? true : false;

                        InventoryStatusAll.Add(new Invertory(item, x.IdItemGroup, x.TotalQuantity, 0, x.AlarmQuantity, isSirovina));
                    }
                });
                SearchItems = new List<Invertory>(InventoryStatusAll);

                if (sqliteDbContext.ItemGroups != null &&
                    sqliteDbContext.ItemGroups.Any())
                {
                    sqliteDbContext.ItemGroups.ToList().ForEach(gropu =>
                    {
                        Groups.Add(new GroupItems(gropu.Id, gropu.IdSupergroup, gropu.Name));
                    });
                }
                AllGroups = new ObservableCollection<GroupItems>(Groups);
                CurrentGroup = AllGroups.FirstOrDefault();

                Suppliers = new ObservableCollection<Supplier>(SuppliersAll);
                InventoryStatusCalculation = new ObservableCollection<Invertory>(InventoryStatusAll);

                if (Suppliers.Any())
                {
                    SelectedSupplier = Suppliers.FirstOrDefault();
                }
            }
        }
        #endregion Constructors

        #region Properties internal
        internal List<GroupItems> Groups;
        internal List<Invertory> SearchItems = new List<Invertory>();
        internal CashierDB LoggedCashier;
        internal List<Supplier> SuppliersAll = new List<Supplier>();
        internal List<Invertory> InventoryStatusAll = new List<Invertory>();
        internal Window Window { get; set; }
        #endregion Properties internal

        #region Properties
        public DateTime CalculationDate
        {
            get { return _calculationDate; }
            set
            {
                value = new DateTime(value.Year, value.Month, value.Day, 6, 0, 0);

                _calculationDate = value;
                OnPropertyChange(nameof(CalculationDate));
            }
        }
        public ObservableCollection<Supplier> Suppliers
        {
            get { return _suppliers; }
            set
            {
                _suppliers = value;
                OnPropertyChange(nameof(Suppliers));

                if (Suppliers.Count == 1)
                {
                    SelectedSupplier = Suppliers.FirstOrDefault();
                }
            }
        }
        public Supplier SelectedSupplier
        {
            get { return _selectedSupplier; }
            set
            {
                _selectedSupplier = value;
                OnPropertyChange(nameof(SelectedSupplier));
            }
        }
        public string SearchPIB
        {
            get { return _searchPIB; }
            set
            {
                _searchPIB = value;
                OnPropertyChange(nameof(SearchPIB));

                if (string.IsNullOrEmpty(value))
                {
                    Suppliers = new ObservableCollection<Supplier>(SuppliersAll);
                }
                else
                {
                    Suppliers = new ObservableCollection<Supplier>(SuppliersAll.Where(supplier => supplier.Pib.ToLower().Contains(value.ToLower())));
                }
            }
        }
        public string InvoiceNumber
        {
            get { return _invoiceNumber; }
            set
            {
                _invoiceNumber = value;
                OnPropertyChange(nameof(InvoiceNumber));
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
                    InventoryStatusCalculation = new ObservableCollection<Invertory>(SearchItems);
                }
                else
                {
                    InventoryStatusCalculation = new ObservableCollection<Invertory>(SearchItems.Where(item =>
                    item.Item.Name.ToLower().Contains(value.ToLower())));
                }
            }
        }

        public ObservableCollection<Invertory> InventoryStatusCalculation
        {
            get { return _inventoryStatusCalculation; }
            set
            {
                _inventoryStatusCalculation = value;
                OnPropertyChange(nameof(InventoryStatusCalculation));
            }
        }

        public ObservableCollection<Invertory> Calculations
        {
            get { return _calculations; }
            set
            {
                _calculations = value;
                OnPropertyChange(nameof(Calculations));
            }
        }
        public Invertory CurrentInventoryStatusCalculation
        {
            get { return _currentInventoryStatusCalculation; }
            set
            {
                _currentInventoryStatusCalculation = value;
                OnPropertyChange(nameof(CurrentInventoryStatusCalculation));

                if (value != null)
                {
                    VisibilityNext = Visibility.Visible;
                }
                else
                {
                    VisibilityNext = Visibility.Hidden;
                }
            }
        }
        public string CalculationQuantityString
        {
            get { return _calculationQuantityString; }
            set
            {
                _calculationQuantityString = value.Replace(',', '.');
                OnPropertyChange(nameof(CalculationQuantityString));

                try
                {
                    CalculationQuantity = Convert.ToDecimal(_calculationQuantityString);
                }
                catch
                {
                    CalculationQuantityString = "0";
                }
            }
        }
        public decimal TotalCalculation
        {
            get { return _totalCalculation; }
            set
            {
                _totalCalculation = value;
                OnPropertyChange(nameof(TotalCalculation));
            }
        }
        public decimal CalculationQuantity
        {
            get { return _calculationQuantity; }
            set
            {
                _calculationQuantity = value;
                OnPropertyChange(nameof(CalculationQuantity));
            }
        }
        public string QuantityCommandParameter
        {
            get { return _quantityCommandParameter; }
            set
            {
                _quantityCommandParameter = value;
                OnPropertyChange(nameof(QuantityCommandParameter));
            }
        }
        public string CalculationPriceString
        {
            get { return _calculationPriceString; }
            set
            {
                _calculationPriceString = value.Replace(',', '.');
                OnPropertyChange(nameof(CalculationPriceString));

                try
                {
                    CalculationPrice = Convert.ToDecimal(_calculationPriceString);
                }
                catch
                {
                    CalculationPriceString = "0";
                }
            }
        }
        public decimal CalculationPrice
        {
            get { return _calculationPrice; }
            set
            {
                _calculationPrice = value;
                OnPropertyChange(nameof(CalculationPrice));
            }
        }
        public decimal OldPrice
        {
            get { return _oldPrice; }
            set
            {
                _oldPrice = value;
                OnPropertyChange(nameof(OldPrice));
            }
        }
        public decimal ProsecnaPrice
        {
            get { return _prosecnaPrice; }
            set
            {
                _prosecnaPrice = value;
                OnPropertyChange(nameof(ProsecnaPrice));
            }
        }
        public string NewPriceString
        {
            get { return _newPriceString; }
            set
            {
                _newPriceString = value.Replace(',', '.'); ;
                OnPropertyChange(nameof(NewPriceString));

                NewPrice = Convert.ToDecimal(_newPriceString);
            }
        }
        public decimal NewPrice
        {
            get { return _newPrice; }
            set
            {
                _newPrice = value;
                OnPropertyChange(nameof(NewPrice));
            }
        }
        public Visibility VisibilityNext
        {
            get { return _visibilityNext; }
            set
            {
                _visibilityNext = value;
                OnPropertyChange(nameof(VisibilityNext));
            }
        }
        public Visibility VisibilityProsecnaPrice
        {
            get { return _visibilityProsecnaPrice; }
            set
            {
                _visibilityProsecnaPrice = value;
                OnPropertyChange(nameof(VisibilityProsecnaPrice));

                if(value == Visibility.Visible)
                {
                    VisibilityOldPrice = Visibility.Hidden;
                }
                else
                {
                    VisibilityOldPrice = Visibility.Visible;
                }
            }
        }
        public Visibility VisibilityOldPrice
        {
            get { return _visibilityOldPrice; }
            set
            {
                _visibilityOldPrice = value;
                OnPropertyChange(nameof(VisibilityOldPrice));
            }
        }

        public GroupItems CurrentGroup
        {
            get { return _currentGroup; }
            set
            {
                _currentGroup = value;
                OnPropertyChange(nameof(CurrentGroup));
            }
        }
        public ObservableCollection<GroupItems> AllGroups
        {
            get { return _allGroups; }
            set
            {
                _allGroups = value;
                OnPropertyChange(nameof(AllGroups));
            }
        }
        #endregion Properties

        #region Command
        public ICommand OpenCalculationWindowCommand => new OpenCalculationWindowCommand(this);
        public ICommand SaveCalculationCommand => new SaveCalculationCommand(this);
        public ICommand NextCommand => new NextCommand(this);
        public ICommand EditCalculationCommand => new EditCalculationItemCommand(this);
        public ICommand DeleteCalculationCommand => new DeleteCalculationItemCommand(this);
        public ICommand SearchGroupsCommand => new SearchGroupsCommand(this);
        
        #endregion Command

        #region Public methods
        #endregion Public methods

        #region Private methods
        #endregion Private methods
    }
}
