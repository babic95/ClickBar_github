using ClickBar.Commands.AppMain.Statistic.Calculation;
using ClickBar.Commands.AppMain.Statistic.Norm;
using ClickBar.Commands.AppMain.Statistic.ViewCalculation;
using ClickBar.Models.AppMain.Statistic;
using ClickBar.Models.Sale;
using ClickBar_DatabaseSQLManager;
using ClickBar_DatabaseSQLManager.Models;
using ClickBar_Printer.Enums;
using DocumentFormat.OpenXml.Spreadsheet;
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
    public class ViewCalculationViewModel : ViewModelBase
    {
        #region Fields
        private IServiceProvider _serviceProvider;

        private ObservableCollection<Calculation> _calculationsAll;
        private ObservableCollection<Calculation> _calculations;

        private Calculation _currentCalculation;

        private string _searchInvoiceNumber;
        private ObservableCollection<Supplier> _suppliers;
        private Supplier _selectedSupplier;
        private Supplier? _searchSupplier;
        private DateTime? _searchFromCalculationDate;
        private DateTime? _searchToCalculationDate;

        private Visibility _visibilityProsecnaPrice;
        private Visibility _visibilityOldPrice;

        private string _calculationQuantityString;
        private decimal _calculationQuantity;
        private string _calculationPriceString;
        private decimal _calculationPrice;
        private decimal _prosecnaPrice;
        private decimal _oldPrice;
        private string _jm;

        private ObservableCollection<Invertory> _inventoryStatusCalculation;
        private Invertory _currentInventoryStatusCalculation;

        private ObservableCollection<Models.Sale.GroupItems> _allGroups;
        private Models.Sale.GroupItems _currentGroup;

        private string _searchText;
        private Visibility _visibilityNext;

        private decimal _totalInputPrice;
        private decimal _totalOutputPrice;
        #endregion Fields

        #region Constructors
        public ViewCalculationViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            DbContext = _serviceProvider.GetRequiredService<IDbContextFactory<SqlServerDbContext>>().CreateDbContext();
            TotalInputPrice = 0;
            TotalOutputPrice = 0;
            SearchToCalculationDate = DateTime.Now;
            SearchFromCalculationDate = SearchToCalculationDate.Value.AddDays(-30);

            Suppliers = new ObservableCollection<Supplier>() { new Supplier() { Name = string.Empty } };
            SearchSupplier = Suppliers.FirstOrDefault();
            CalculationsAll = new ObservableCollection<Calculation>();
            Calculations = new ObservableCollection<Calculation>();

            DbContext.Suppliers.ToList().ForEach(x =>
            {
                Suppliers.Add(new Supplier(x));
            });

            var calculations = DbContext.Calculations.Where(cal => cal.CalculationDate.Date >= SearchFromCalculationDate.Value.Date &&
            cal.CalculationDate.Date <= SearchToCalculationDate.Value.Date)
                .Join(DbContext.Suppliers,
                cal => cal.SupplierId,
                supp => supp.Id,
                (cal, supp) => new { Cal = cal, Supp = supp })
                .Join(DbContext.Cashiers,
                cal => cal.Cal.CashierId,
                cash => cash.Id,
                (cal, cash) => new { Cal = cal, Cash = cash });

            InventoryStatusCalculation = new ObservableCollection<Invertory>(InventoryStatusAll);

            if (calculations != null &&
                calculations.Any())
            {
                foreach(var cal in calculations)
                {
                    Calculation calculation = new Calculation()
                    {
                        Id = cal.Cal.Cal.Id,
                        CalculationDate = cal.Cal.Cal.CalculationDate,
                        InputTotalPrice = cal.Cal.Cal.InputTotalPrice,
                        InvoiceNumber = cal.Cal.Cal.InvoiceNumber,
                        OutputTotalPrice = cal.Cal.Cal.OutputTotalPrice,
                        Counter = cal.Cal.Cal.Counter,
                        Name = $"Kalkulacija_{cal.Cal.Cal.Counter}-{cal.Cal.Cal.CalculationDate.Year}",
                        Supplier = cal.Cal.Supp == null ? null : new Supplier(cal.Cal.Supp),
                        Cashier = cal.Cash
                    };
                    //calculation.CalculationItems = await GetAllItemsInCalculation(cal);
                    //calculation.Supplier = GetSupplierForCalculation(cal);
                    //calculation.Cashier = GetCashierNameForCalculation(cal);

                    CalculationsAll.Add(calculation);
                    TotalInputPrice += calculation.InputTotalPrice;
                    TotalOutputPrice += calculation.OutputTotalPrice;
                }
            }
            Calculations = new ObservableCollection<Calculation>(CalculationsAll.OrderBy(cal => cal.CalculationDate));
        }
        #endregion Constructors

        #region Properties internal
        internal SqlServerDbContext DbContext
        {
            get; private set;
        }
        internal List<Models.Sale.GroupItems> Groups;
        internal List<Invertory> SearchItems = new List<Invertory>();
        internal List<Invertory> InventoryStatusAll = new List<Invertory>();
        internal Window CurrentWindow { get; set; }
        internal Window EditWindow { get; set; }
        internal Window EditQuantityWindow { get; set; }
        internal Window AllItemsWindow { get; set; }
        #endregion Properties internal

        #region Properties
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
        public Visibility VisibilityNext
        {
            get { return _visibilityNext; }
            set
            {
                _visibilityNext = value;
                OnPropertyChange(nameof(VisibilityNext));
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
        public ObservableCollection<Models.Sale.GroupItems> AllGroups
        {
            get { return _allGroups; }
            set
            {
                _allGroups = value;
                OnPropertyChange(nameof(AllGroups));
            }
        }
        public string SearchInvoiceNumber
        {
            get { return _searchInvoiceNumber; }
            set
            {
                _searchInvoiceNumber = value;
                OnPropertyChange(nameof(SearchInvoiceNumber));
            }
        }
        public ObservableCollection<Supplier> Suppliers
        {
            get { return _suppliers; }
            set
            {
                _suppliers = value;
                OnPropertyChange(nameof(Suppliers));
            }
        }
        public Supplier? SearchSupplier
        {
            get { return _searchSupplier; }
            set
            {
                _searchSupplier = value;
                OnPropertyChange(nameof(SearchSupplier));
            }
        }
        public DateTime? SearchFromCalculationDate
        {
            get { return _searchFromCalculationDate; }
            set
            {
                if (value != null &&
                    SearchToCalculationDate != null)
                {
                    if (value > SearchToCalculationDate)
                    {
                        MessageBox.Show("Datum mora biti stariji od datuma do kad se pretražuje", "Greška pretrage", MessageBoxButton.OK, MessageBoxImage.Error);
                        value = null;
                    }
                }
                _searchFromCalculationDate = value;
                OnPropertyChange(nameof(SearchFromCalculationDate));
            }
        }
        public DateTime? SearchToCalculationDate
        {
            get { return _searchToCalculationDate; }
            set
            {
                if (value != null &&
                    SearchFromCalculationDate != null)
                {
                    if (value < SearchFromCalculationDate)
                    {
                        MessageBox.Show("Datum mora biti mlađi od datuma od kad se pretražuje", "Greška pretrage", MessageBoxButton.OK, MessageBoxImage.Error);
                        value = null;
                    }
                }
                _searchToCalculationDate = value;
                OnPropertyChange(nameof(SearchToCalculationDate));
            }
        }
        public Supplier SelectedSupplier
        {
            get { return _selectedSupplier; }
            set
            {
                _selectedSupplier = value;
                OnPropertyChange(nameof(SelectedSupplier));

                if (value != null &&
                    value.Id != -1 &&
                    CurrentCalculation != null)
                {
                    if (CurrentCalculation.Supplier != null &&
                        CurrentCalculation.Supplier.Pib != value.Pib)
                    {
                        CurrentCalculation.Supplier = value;
                    }
                }
            }
        }
        public Calculation CurrentCalculation
        {
            get { return _currentCalculation; }
            set
            {
                _currentCalculation = value;
                OnPropertyChange(nameof(CurrentCalculation));

                if (value != null &&
                    value.Supplier != null)
                {
                    var supplier = Suppliers.FirstOrDefault(sup => sup.Pib == value.Supplier.Pib);

                    if (supplier != null)
                    {
                        SelectedSupplier = supplier;
                    }
                }
            }
        }
        public ObservableCollection<Calculation> CalculationsAll
        {
            get { return _calculationsAll; }
            set
            {
                _calculationsAll = value;
                OnPropertyChange(nameof(CalculationsAll));
            }
        }
        public ObservableCollection<Calculation> Calculations
        {
            get { return _calculations; }
            set
            {
                _calculations = value;
                OnPropertyChange(nameof(Calculations));
            }
        }
        public Visibility VisibilityProsecnaPrice
        {
            get { return _visibilityProsecnaPrice; }
            set
            {
                _visibilityProsecnaPrice = value;
                OnPropertyChange(nameof(VisibilityProsecnaPrice));

                if (value == Visibility.Visible)
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
        public decimal CalculationQuantity
        {
            get { return _calculationQuantity; }
            set
            {
                _calculationQuantity = value;
                OnPropertyChange(nameof(CalculationQuantity));
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
        public string JM
        {
            get { return _jm; }
            set
            {
                _jm = value;
                OnPropertyChange(nameof(JM));
            }
        }
        public decimal TotalInputPrice
        {
            get { return _totalInputPrice; }
            set
            {
                _totalInputPrice = value;
                OnPropertyChange(nameof(TotalInputPrice));
            }
        }
        public decimal TotalOutputPrice
        {
            get { return _totalOutputPrice; }
            set
            {
                _totalOutputPrice = value;
                OnPropertyChange(nameof(TotalOutputPrice));
            }
        }
        #endregion Properties

        #region Commands
        public ICommand CancelDateCommand => new CancelDateCommand(this);
        public ICommand SearchCommand => new SearchCommand(this);
        public ICommand ViewItemsInCalculationCommand => new ViewItemsInCalculationCommand(this);
        public ICommand PrintCalculationCommand => new PrintCalculationCommand(this);
        public ICommand SaveCalculationCommand => new SaveCalculationCommand(this);
        public ICommand OpenAllItemsWindowCommand => new OpenAllItemsWindowCommand(this);
        public ICommand OpenCalculationItemEditWindowCommand => new OpenCalculationItemEditWindowCommand(this);
        public ICommand OpenEditCalculationItemEditWindowCommand => new OpenEditCalculationItemEditWindowCommand(this);
        public ICommand EditCalculationItemEditWindowCommand => new EditCalculationItemEditWindowCommand(this);
        public ICommand RemoveCalculationItemEditWindowCommand => new RemoveCalculationItemEditWindowCommand(this);
        public ICommand SearchGroupsCommand => new SearchGroupsCommand(this);
        public ICommand AddItemForCalculationCommand => new AddItemForCalculationCommand(this);
        public ICommand PrintSaldo1010Command => new PrintSaldo1010Command(this);
        #endregion Commands

        #region Public methods
        #endregion Public methods

        #region Private methods
        internal async Task<ObservableCollection<Invertory>> GetAllItemsInCalculation(CalculationDB calculationDB)
        {
            ObservableCollection<Invertory> calculationItems = new ObservableCollection<Invertory>();

            var calculationItemsDB = DbContext.CalculationItems.Where(item => item.CalculationId == calculationDB.Id);

            if (calculationItemsDB != null &&
                calculationItemsDB.Any())
            {
                foreach(var item in calculationItemsDB.ToList())
                {
                    var itemDB = await DbContext.Items.FindAsync(item.ItemId);

                    if (itemDB != null)
                    {
                        Invertory calculationItem = new Invertory()
                        {
                            Item = new Models.Sale.Item(itemDB),
                            Alarm = itemDB.AlarmQuantity,
                            InputPrice = item.InputPrice * item.Quantity,
                            Quantity = item.Quantity,
                            TotalAmout = item.OutputPrice,
                            IdGroupItems = itemDB.IdItemGroup,
                        };

                        calculationItems.Add(calculationItem);
                    }
                }

            }
            return calculationItems;
        }
            //internal Supplier? GetSupplierForCalculation(CalculationDB calculationDB)
            //{
            //    SqlServerDbContext sqliteDbContext = new SqlServerDbContext();

            //    var supplierDB = sqliteDbContext.Suppliers.Find(calculationDB.SupplierId);

            //    if(supplierDB != null)
            //    {
            //        Supplier supplier = new Supplier(supplierDB);
            //        return supplier;
            //    }

            //    return null;
            //}
            //internal CashierDB? GetCashierNameForCalculation(CalculationDB calculationDB)
            //{
            //    SqlServerDbContext sqliteDbContext = new SqlServerDbContext();

            //    var cashierDB = sqliteDbContext.Cashiers.Find(calculationDB.CashierId);

            //    if(cashierDB != null)
            //    {
            //        return cashierDB;
            //    }

            //    return null;
            //}
        #endregion Private methods
    }
}