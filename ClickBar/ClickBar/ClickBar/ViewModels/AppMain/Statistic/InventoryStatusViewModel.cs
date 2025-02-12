using ClickBar.Commands.AppMain.Statistic;
using ClickBar.Commands.AppMain.Statistic.Calculation;
using ClickBar.Commands.AppMain.Statistic.InventoryStatus;
using ClickBar.Commands.AppMain.Statistic.Norm;
using ClickBar.Models.AppMain.Statistic;
using ClickBar.Models.Sale;
using ClickBar_DatabaseSQLManager;
using ClickBar_DatabaseSQLManager.Models;
using ClickBar_Settings;
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
    public class InventoryStatusViewModel : ViewModelBase
    {
        #region Fields
        private IServiceProvider _serviceProvider;

        private Supergroup? _currentSupergroupSearch;
        private Supergroup? _currentSupergroup;
        private GroupItems? _currentGroupItems;

        private ObservableCollection<Supergroup> _allSupergroups;
        private ObservableCollection<GroupItems> _allGroupItems;

        private Invertory _currentInventoryStatus;
        private ObservableCollection<Invertory> _inventoryStatus;
        private ObservableCollection<Invertory> _inventoryStatusNorm;
        private Invertory _currentInventoryStatusNorm;

        private ObservableCollection<Invertory> _norma;
        private ObservableCollection<ItemZelja> _zelje;

        private string _searchText;
        private decimal _normQuantity;
        private string _normQuantityString;
        private string _searchItems;

        private bool _editItemIsReadOnly;

        private Visibility _visibilityNext;
        private Visibility _visibilityAllSupergroup;
        private Visibility _visibilityAllGroupItems;
        private Visibility _nadgrupeVisibility;

        private string _quantityCommandParameter;
        private ObservableCollection<GroupItems> _allGroups;
        private GroupItems _currentGroup;

        private ObservableCollection<TaxLabel> _allLabels;
        private TaxLabel _currentLabel;
#if DEBUG
        private List<TaxLabel> _labels = new List<TaxLabel>()
        {
            new TaxLabel("31", "A - 9% PDV"),
            new TaxLabel("47", "N - 0% PDV"),
            new TaxLabel("8", "Ж - 19% PDV"),
            new TaxLabel("39", "F - 11% PDV"),
        };
#else
        private List<TaxLabel> _labels = new List<TaxLabel>()
        {
            new TaxLabel("1", "A - Nije u PDV"),
            new TaxLabel("4", "Г - 0% PDV"),
            new TaxLabel("6", "Ђ - 20% PDV"),
            new TaxLabel("7", "Е - 10% PDV"),
        };
#endif
        #endregion Fields

        #region Constructors
        public InventoryStatusViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            DbContext = serviceProvider.GetRequiredService<SqlServerDbContext>();
            AllGroupItems = new ObservableCollection<GroupItems>();
            AllSupergroups = new ObservableCollection<Supergroup>() { new Supergroup(-1, "Sve nadgrupe") };
            AllGroups = new ObservableCollection<GroupItems>() { new GroupItems(-1, -1, "Sve grupe") };

            AllLabels = new ObservableCollection<TaxLabel>(_labels);

            LoadData();
        }
        #endregion Constructors

        #region Properties internal
        internal SqlServerDbContext DbContext
        {
            get; private set;
        }
        internal List<Invertory> InventoryStatusAll = new List<Invertory>();
        internal Window Window { get; set; }
        internal Window WindowHelper { get; set; }
        internal int CurrentNorm { get; set; }
        internal Window PrintTypeWindow { get; set; }
        #endregion Properties internal

        #region Properties
        public ObservableCollection<Supergroup> AllSupergroups
        {
            get { return _allSupergroups; }
            set
            {
                _allSupergroups = value;
                OnPropertyChange(nameof(AllSupergroups));
            }
        }
        public ObservableCollection<GroupItems> AllGroupItems
        {
            get { return _allGroupItems; }
            set
            {
                _allGroupItems = value;
                OnPropertyChange(nameof(AllGroupItems));
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
        public Visibility NadgrupeVisibility
        {
            get { return _nadgrupeVisibility; }
            set
            {
                _nadgrupeVisibility = value;
                OnPropertyChange(nameof(NadgrupeVisibility));
            }
        }

        public ObservableCollection<TaxLabel> AllLabels
        {
            get { return _allLabels; }
            set
            {
                _allLabels = value;
                OnPropertyChange(nameof(AllLabels));
            }
        }
        public TaxLabel CurrentLabel
        {
            get { return _currentLabel; }
            set
            {
                _currentLabel = value;
                OnPropertyChange(nameof(CurrentLabel));

                if (value != null &&
                    CurrentInventoryStatus != null &&
                    CurrentInventoryStatus.Item != null)
                {
                    CurrentInventoryStatus.Item.Label = value.Id;
                }
            }
        }
        public Supergroup? CurrentSupergroup
        {
            get { return _currentSupergroup; }
            set
            {
                _currentSupergroup = value;
                OnPropertyChange(nameof(CurrentSupergroup));
            }
        }
        public Supergroup? CurrentSupergroupSearch
        {
            get { return _currentSupergroupSearch; }
            set
            {
                _currentSupergroupSearch = value;
                OnPropertyChange(nameof(CurrentSupergroupSearch));

                if (value != null)
                {
                    if (value.Id == -1)
                    {
                        InventoryStatus = new ObservableCollection<Invertory>(InventoryStatusAll);
                    }
                    else
                    {
                        InventoryStatus = new ObservableCollection<Invertory>(InventoryStatusAll.Join(AllGroups,
                            i => i.IdGroupItems,
                            g => g.Id,
                            (i, g) => new { I = i, G = g }).Where(inventory => inventory.G.IdSupergroup == value.Id)
                            .Select(i => i.I));
                    }
                }
                else
                {
                    InventoryStatus = new ObservableCollection<Invertory>(InventoryStatusAll);
                }
            }
        }
        public GroupItems? CurrentGroupItems
        {
            get { return _currentGroupItems; }
            set
            {
                _currentGroupItems = value;

                if (value != null)
                {
                    CurrentSupergroup = AllSupergroups.FirstOrDefault(supergroup => supergroup.Id == value.IdSupergroup);

                    if (CurrentInventoryStatus != null)
                    {
                        if (value.Name.ToLower().Contains("sirovina") ||
                            value.Name.ToLower().Contains("sirovine"))
                        {
                            CurrentInventoryStatus.VisibilityJC = Visibility.Hidden;
                        }
                        else
                        {
                            CurrentInventoryStatus.VisibilityJC = Visibility.Visible;
                        }
                    }
                }
                OnPropertyChange(nameof(CurrentGroupItems));
            }
        }
        public GroupItems CurrentGroup
        {
            get { return _currentGroup; }
            set
            {
                _currentGroup = value;
                OnPropertyChange(nameof(CurrentGroup));

                if (value != null)
                {
                    if (value.Id == -1)
                    {
                        InventoryStatus = new ObservableCollection<Invertory>(InventoryStatusAll);
                    }
                    else
                    {
                        InventoryStatus = new ObservableCollection<Invertory>(InventoryStatusAll.Where(inventory => inventory.IdGroupItems == value.Id));
                    }
                }
                else
                {
                    InventoryStatus = new ObservableCollection<Invertory>(InventoryStatusAll);
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
        public Visibility VisibilityAllSupergroup
        {
            get { return _visibilityAllSupergroup; }
            set
            {
                _visibilityAllSupergroup = value;
                OnPropertyChange(nameof(VisibilityAllSupergroup));
            }
        }
        public Visibility VisibilityAllGroupItems
        {
            get { return _visibilityAllGroupItems; }
            set
            {
                _visibilityAllGroupItems = value;
                OnPropertyChange(nameof(VisibilityAllGroupItems));
            }
        }
        public decimal NormQuantity
        {
            get { return _normQuantity; }
            set
            {
                _normQuantity = value;
                OnPropertyChange(nameof(NormQuantity));
            }
        }
        public string NormQuantityString
        {
            get { return _normQuantityString; }
            set
            {
                _normQuantityString = value.Replace(',', '.');
                OnPropertyChange(nameof(NormQuantityString));

                try
                {
                    NormQuantity = Convert.ToDecimal(_normQuantityString);
                }
                catch
                {
                    NormQuantityString = "0";
                }
            }
        }

        public bool EditItemIsReadOnly
        {
            get { return _editItemIsReadOnly; }
            set
            {
                _editItemIsReadOnly = value;
                OnPropertyChange(nameof(EditItemIsReadOnly));
            }
        }
        public Invertory CurrentInventoryStatus
        {
            get { return _currentInventoryStatus; }
            set
            {
                _currentInventoryStatus = value;
                OnPropertyChange(nameof(CurrentInventoryStatus));

                if (value != null)
                {
                    VisibilityNext = Visibility.Visible;

                    if (value.Item != null &&
                        !string.IsNullOrEmpty(value.Item.Label))
                    {
                        var label = AllLabels.FirstOrDefault(lab => lab.Id == value.Item.Label);

                        if (label != null)
                        {
                            CurrentLabel = label;
                        }
                    }
                }
                else
                {
                    VisibilityNext = Visibility.Hidden;
                }
            }
        }
        public Invertory CurrentInventoryStatusNorm
        {
            get { return _currentInventoryStatusNorm; }
            set
            {
                _currentInventoryStatusNorm = value;
                OnPropertyChange(nameof(CurrentInventoryStatusNorm));
            }
        }
        public ObservableCollection<Invertory> Norma
        {
            get { return _norma; }
            set
            {
                _norma = value;
                OnPropertyChange(nameof(Norma));
            }
        }
        public ObservableCollection<ItemZelja> Zelje
        {
            get { return _zelje; }
            set
            {
                _zelje = value;
                OnPropertyChange(nameof(Zelje));
            }
        }
        public ObservableCollection<Invertory> InventoryStatus
        {
            get { return _inventoryStatus; }
            set
            {
                _inventoryStatus = value;
                OnPropertyChange(nameof(InventoryStatus));
            }
        }
        public ObservableCollection<Invertory> InventoryStatusNorm
        {
            get { return _inventoryStatusNorm; }
            set
            {
                _inventoryStatusNorm = value;
                OnPropertyChange(nameof(InventoryStatusNorm));
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
                    InventoryStatus = new ObservableCollection<Invertory>(InventoryStatusAll);
                }
                else
                {
                    InventoryStatus = new ObservableCollection<Invertory>(InventoryStatusAll.Where(inventory => inventory.Item.Name.ToLower().Contains(value.ToLower())));
                }
            }
        }
        public string SearchItems
        {
            get { return _searchItems; }
            set
            {
                _searchItems = value;
                OnPropertyChange(nameof(SearchItems));

                if (string.IsNullOrEmpty(value))
                {
                    InventoryStatusNorm = new ObservableCollection<Invertory>(InventoryStatusAll);
                }
                else
                {
                    InventoryStatusNorm = new ObservableCollection<Invertory>(InventoryStatusAll.Where(inventory => inventory.Item.Name.ToLower().Contains(value.ToLower())));

                    if (CurrentInventoryStatusNorm != null &&
                        !InventoryStatusNorm.Where(inventory => inventory.Item.Id == CurrentInventoryStatusNorm.Item.Id).Any())
                    {
                        VisibilityNext = Visibility.Hidden;
                        CurrentInventoryStatusNorm = null;
                    }
                }
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
        #endregion Properties

        #region Commands
        public ICommand OpenAddEditWindow => new OpenAddEditWindow(this);
        public ICommand OpenPrintCommand => new OpenPrintCommand(this);
        public ICommand PrintCommand => new PrintCommand(this);
        public ICommand PrintA4Command => new PrintA4Command(this);
        public ICommand SaveCommand => new SaveCommand(this);
        public ICommand EditCommand => new EditCommand(this);
        public ICommand DeleteCommand => new DeleteCommand(this);
        public ICommand EditNormCommand => new EditNormCommand(this);
        public ICommand DeleteNormCommand => new DeleteNormCommand(this);
        public ICommand NextCommand => new NextCommand(this);
        public ICommand OpenNormativWindowCommand => new OpenNormativWindowCommand(this);
        public ICommand OpenAddOrEditSupergroupCommand => new OpenAddOrEditSupergroupCommand(this);
        public ICommand OpenAddOrEditGroupItemsCommand => new OpenAddOrEditGroupItemsCommand(this);
        public ICommand SaveSupergroupCommand => new SaveSupergroupCommand(this);
        public ICommand SaveGroupItemsCommand => new SaveGroupItemsCommand(this);
        public ICommand FixInputPriceCommand => new FixInputPriceCommand(this);
        public ICommand FixQuantityCommand => new FixQuantityCommand(this);
        public ICommand DeleteZeljaCommand => new DeleteZeljaCommand(this);
        public ICommand AddNewZeljaCommand => new AddNewZeljaCommand(this);
        #endregion Commands

        #region Private methods
        private void LoadData()
        {
            LoadItems();
            LoadSupergroups();
            LoadItemGroups();
            UpdateNormItems();
        }

        private void LoadItems()
        {
            if (DbContext.Items != null && DbContext.Items.Any())
            {
                var items = DbContext.Items.AsNoTracking().ToList();
                foreach (var x in items)
                {
                    Item item = new Item(x);
                    var group = DbContext.ItemGroups.AsNoTracking().FirstOrDefault(g => g.Id == x.IdItemGroup);

                    if (group != null)
                    {
                        bool isSirovina = group.Name.ToLower().Contains("sirovina") || group.Name.ToLower().Contains("sirovine");
                        InventoryStatusAll.Add(new Invertory(item, x.IdItemGroup, x.TotalQuantity, 0, x.AlarmQuantity, isSirovina));
                    }
                }
            }
        }

        private void LoadSupergroups()
        {
            if (DbContext.Supergroups != null && DbContext.Supergroups.Any())
            {
                foreach (var supergroup in DbContext.Supergroups.AsNoTracking().ToList())
                {
                    AllSupergroups.Add(new Supergroup(supergroup.Id, supergroup.Name));
                }

                CurrentSupergroup = AllSupergroups.FirstOrDefault();
            }

            if (SettingsManager.Instance.EnableSuperGroup())
            {
                NadgrupeVisibility = Visibility.Visible;
                CurrentSupergroupSearch = AllSupergroups.FirstOrDefault();
            }
            else
            {
                NadgrupeVisibility = Visibility.Collapsed;
            }
        }

        private void LoadItemGroups()
        {
            if (DbContext.ItemGroups != null && DbContext.ItemGroups.Any())
            {
                foreach (var group in DbContext.ItemGroups.AsNoTracking().ToList())
                {
                    AllGroupItems.Add(new GroupItems(group.Id, group.IdSupergroup, group.Name));
                    AllGroups.Add(new GroupItems(group.Id, group.IdSupergroup, group.Name));
                }

                CurrentGroupItems = AllGroupItems.FirstOrDefault();
                CurrentGroup = AllGroups.FirstOrDefault();
            }
        }

        private void UpdateNormItems()
        {
            if (DbContext.Items != null && DbContext.Items.Any())
            {
                var normItems = DbContext.Items.Where(i => i.IdNorm != null).ToList();

                foreach (var itemDB in normItems)
                {
                    var itemInNorm = DbContext.ItemsInNorm.Where(i => i.IdNorm == itemDB.IdNorm);

                    if (itemInNorm == null || !itemInNorm.Any())
                    {
                        itemDB.IdNorm = null;
                        DbContext.Items.Update(itemDB);
                    }
                }

                DbContext.SaveChanges();
            }

            Norma = new ObservableCollection<Invertory>();
            InventoryStatus = new ObservableCollection<Invertory>(InventoryStatusAll);
            InventoryStatusNorm = new ObservableCollection<Invertory>(InventoryStatusAll);

            NormQuantityString = "0";
            VisibilityNext = Visibility.Hidden;
        }
        #endregion Private methods
    }
}