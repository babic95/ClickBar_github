using ClickBar.Commands.AppMain.Statistic;
using ClickBar.Commands.AppMain.Statistic.Nivelacija;
using ClickBar.Enums.AppMain.Statistic;
using ClickBar.Models.AppMain.Statistic;
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
    public class NivelacijaViewModel : ViewModelBase
    {
        #region Fields
        private IServiceProvider _serviceProvider;

        private ObservableCollection<Invertory> _items;
        private NivelacijaItem? _currentNivelacijaItem;

        private string _totalNivelacijaString;
        private decimal _totalNivelacija;
        private string _totalOldNivelacijaString;
        private decimal _totalOldNivelacija;
        private string _totalNewNivelacijaString;
        private decimal _totalNewNivelacija;
        private string _totalPdvNivelacijaString;
        private decimal _totalPdvNivelacija;

        private Nivelacija _currentNivelacija;

        private NivelacijaItem _currentItemsToNivelacija;

        private ObservableCollection<GroupItems> _allGroups;
        private GroupItems _currentGroup;

        private string _searchText;

        private bool _newPriceSelected;
        private bool _razlikaCeneSelected;
        private bool _procenatNaDosadasnjuCenuSelected;
        private bool _marzaLastCalculationSelected;

        private bool _neZaokruzujeSelected;
        private bool _zaokruzujeSelected;
        private bool _zaokruzuje10Selected;
        private bool _zaokruzuje100Selected;

        private string _newPriceString;
        private decimal _newPrice;
        private string _razlikaCeneString;
        private decimal _razlikaCene;
        private string _procenatNaDosadasnjuCenuString;
        private decimal _procenatNaDosadasnjuCenu;
        private string _marzaLastCalculationString;
        private decimal _marzaLastCalculation;
        #endregion Fields

        #region Constructors
        public NivelacijaViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            DbContext = _serviceProvider.GetRequiredService<IDbContextFactory<SqlServerDbContext>>().CreateDbContext();
            NeZaokruzujeSelected = true;
            NewPriceSelected = true;
            Reset();
        }
        #endregion Constructors

        #region Properties internal
        internal SqlServerDbContext DbContext
        {
            get; private set;
        }
        internal Window NivelacijaItemWindow;
        internal List<GroupItems> Groups;
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
        internal List<Invertory> AllItems = new List<Invertory>();
        internal List<Invertory> SearchItems = new List<Invertory>();
        #endregion Properties internal

        #region Properties
        public string NewPriceString
        {
            get { return _newPriceString; }
            set
            {
                _newPriceString = value.Replace(',', '.');
                OnPropertyChange(nameof(NewPriceString));

                NewPrice = Convert.ToDecimal(_newPriceString);
            }
        }
        public decimal NewPrice
        {
            get { return _newPrice; }
            set
            {
                if (NewPriceSelected &&
                    value != 0)
                {
                    value = SetNewPrice(value);

                    _newPrice = value;
                    OnPropertyChange(nameof(NewPrice));
                    CurrentItemsToNivelacija.NewPrice = value;
                    RazlikaCene = 0;
                    ProcenatNaDosadasnjuCenu = 0;
                    MarzaLastCalculation = 0;
                }
                else
                {
                    if(value == 0)
                    {
                        _newPrice = value;
                        OnPropertyChange(nameof(NewPrice));

                        if (CurrentItemsToNivelacija != null &&
                            NewPriceSelected)
                        {
                            CurrentItemsToNivelacija.NewPrice = CurrentItemsToNivelacija.OldPrice;
                        }
                    }
                }
            }
        }
        public string RazlikaCeneString
        {
            get { return _razlikaCeneString; }
            set
            {
                _razlikaCeneString = value.Replace(',', '.');
                OnPropertyChange(nameof(RazlikaCeneString));

                RazlikaCene = Convert.ToDecimal(_razlikaCeneString);
            }
        }
        public decimal RazlikaCene
        {
            get { return _razlikaCene; }
            set
            {
                if (RazlikaCeneSelected &&
                    value != 0)
                {
                    value = SetNewPrice(value);

                    _razlikaCene = value;
                    OnPropertyChange(nameof(RazlikaCene));
                    CurrentItemsToNivelacija.NewPrice = CurrentItemsToNivelacija.OldPrice + value;
                    NewPrice = 0;
                    ProcenatNaDosadasnjuCenu = 0;
                    MarzaLastCalculation = 0;
                }
                else
                {
                    if (value == 0)
                    {
                        _razlikaCene = value;
                        OnPropertyChange(nameof(RazlikaCene));

                        if (CurrentItemsToNivelacija != null &&
                            RazlikaCeneSelected)
                        {
                            CurrentItemsToNivelacija.NewPrice = CurrentItemsToNivelacija.OldPrice;
                        }
                    }
                }

            }
        }
        public string ProcenatNaDosadasnjuCenuString
        {
            get { return _procenatNaDosadasnjuCenuString; }
            set
            {
                _procenatNaDosadasnjuCenuString = value.Replace(',', '.');
                OnPropertyChange(nameof(ProcenatNaDosadasnjuCenuString));

                ProcenatNaDosadasnjuCenu = Convert.ToDecimal(_procenatNaDosadasnjuCenuString);
            }
        }
        public decimal ProcenatNaDosadasnjuCenu
        {
            get { return _procenatNaDosadasnjuCenu; }
            set
            {
                if (ProcenatNaDosadasnjuCenuSelected &&
                    value != 0)
                {
                    value = SetNewPrice(value);

                    _procenatNaDosadasnjuCenu = value;
                    OnPropertyChange(nameof(ProcenatNaDosadasnjuCenu));

                    decimal percentage = 1;

                    if(value != 0)
                    {
                        percentage = 1 + (value / 100);
                    }

                    CurrentItemsToNivelacija.NewPrice = CurrentItemsToNivelacija.OldPrice * percentage;
                    NewPrice = 0;
                    RazlikaCene = 0;
                    MarzaLastCalculation = 0;
                }
                else
                {
                    if (value == 0)
                    {
                        _procenatNaDosadasnjuCenu = value;
                        OnPropertyChange(nameof(ProcenatNaDosadasnjuCenu));

                        if (CurrentItemsToNivelacija != null &&
                            ProcenatNaDosadasnjuCenuSelected)
                        {
                            CurrentItemsToNivelacija.NewPrice = CurrentItemsToNivelacija.OldPrice;
                        }
                    }
                }
            }
        }
        public string MarzaLastCalculationString
        {
            get { return _marzaLastCalculationString; }
            set
            {
                _marzaLastCalculationString = value.Replace(',', '.');
                OnPropertyChange(nameof(MarzaLastCalculationString));

                MarzaLastCalculation = Convert.ToDecimal(_marzaLastCalculationString);
            }
        }
        public decimal MarzaLastCalculation
        {
            get { return _marzaLastCalculation; }
            set
            {
                if (MarzaLastCalculationSelected &&
                    value != 0)
                {
                    value = SetNewPrice(value);

                    _marzaLastCalculation = value;
                    OnPropertyChange(nameof(MarzaLastCalculation));

                    decimal percentage = 1;

                    if (value != 0)
                    {
                        percentage = 1 + (value / 100);
                        CurrentItemsToNivelacija.NewPrice = CurrentItemsToNivelacija.LastImportPrice * percentage;
                    }
                    else
                    {
                        CurrentItemsToNivelacija.NewPrice = CurrentItemsToNivelacija.OldPrice * percentage;
                    }

                    NewPrice = 0;
                    RazlikaCene = 0;
                    ProcenatNaDosadasnjuCenu = 0;
                }
                else
                {
                    if (value == 0)
                    {
                        _marzaLastCalculation = value;
                        OnPropertyChange(nameof(MarzaLastCalculation));

                        if (CurrentItemsToNivelacija != null &&
                            MarzaLastCalculationSelected)
                        {
                            CurrentItemsToNivelacija.NewPrice = CurrentItemsToNivelacija.OldPrice;
                        }
                    }
                }
            }
        }
        public bool NewPriceSelected
        {
            get { return _newPriceSelected; }
            set
            {
                _newPriceSelected = value;
                OnPropertyChange(nameof(NewPriceSelected));

                if (value)
                {
                    RazlikaCeneSelected = false;
                    ProcenatNaDosadasnjuCenuSelected = false;
                    MarzaLastCalculationSelected = false;
                }
            }
        }
        public bool RazlikaCeneSelected
        {
            get { return _razlikaCeneSelected; }
            set
            {
                _razlikaCeneSelected = value;
                OnPropertyChange(nameof(RazlikaCeneSelected));
                if (value)
                {
                    NewPriceSelected = false;
                    ProcenatNaDosadasnjuCenuSelected = false;
                    MarzaLastCalculationSelected = false;
                }
            }
        }
        public bool ProcenatNaDosadasnjuCenuSelected
        {
            get { return _procenatNaDosadasnjuCenuSelected; }
            set
            {
                _procenatNaDosadasnjuCenuSelected = value;
                OnPropertyChange(nameof(ProcenatNaDosadasnjuCenuSelected));
                if (value)
                {
                    NewPriceSelected = false;
                    RazlikaCeneSelected = false;
                    MarzaLastCalculationSelected = false;
                }
            }
        }
        public bool MarzaLastCalculationSelected
        {
            get { return _marzaLastCalculationSelected; }
            set
            {
                _marzaLastCalculationSelected = value;
                OnPropertyChange(nameof(MarzaLastCalculationSelected));
                if (value)
                {
                    NewPriceSelected = false;
                    RazlikaCeneSelected = false;
                    ProcenatNaDosadasnjuCenuSelected = false;
                }
            }
        }
        public bool NeZaokruzujeSelected
        {
            get { return _neZaokruzujeSelected; }
            set
            {
                _neZaokruzujeSelected = value;
                OnPropertyChange(nameof(NeZaokruzujeSelected));
                if (value)
                {
                    ZaokruzujeSelected = false;
                    Zaokruzuje10Selected = false;
                    Zaokruzuje100Selected = false;

                    if (CurrentItemsToNivelacija != null)
                    {
                        if (NewPriceSelected)
                        {
                            NewPrice = SetNewPrice(NewPrice);
                        }
                        else if (RazlikaCeneSelected)
                        {
                            RazlikaCene = SetNewPrice(RazlikaCene);
                        }
                        else if (ProcenatNaDosadasnjuCenuSelected)
                        {
                            ProcenatNaDosadasnjuCenu = SetNewPrice(ProcenatNaDosadasnjuCenu);
                        }
                        else if (MarzaLastCalculationSelected)
                        {
                            MarzaLastCalculation = SetNewPrice(MarzaLastCalculation);
                        }
                    }
                }
            }
        }
        public bool ZaokruzujeSelected
        {
            get { return _zaokruzujeSelected; }
            set
            {
                _zaokruzujeSelected = value;
                OnPropertyChange(nameof(ZaokruzujeSelected));
                if (value)
                {
                    NeZaokruzujeSelected = false;
                    Zaokruzuje10Selected = false;
                    Zaokruzuje100Selected = false;

                    if (CurrentItemsToNivelacija != null)
                    {
                        if (NewPriceSelected)
                        {
                            NewPrice = SetNewPrice(NewPrice);
                        }
                        else if (RazlikaCeneSelected)
                        {
                            RazlikaCene = SetNewPrice(RazlikaCene);
                        }
                        else if (ProcenatNaDosadasnjuCenuSelected)
                        {
                            ProcenatNaDosadasnjuCenu = SetNewPrice(ProcenatNaDosadasnjuCenu);
                        }
                        else if (MarzaLastCalculationSelected)
                        {
                            MarzaLastCalculation = SetNewPrice(MarzaLastCalculation);
                        }
                    }
                }
            }
        }
        public bool Zaokruzuje10Selected
        {
            get { return _zaokruzuje10Selected; }
            set
            {
                _zaokruzuje10Selected = value;
                OnPropertyChange(nameof(Zaokruzuje10Selected));
                if (value)
                {
                    NeZaokruzujeSelected = false;
                    ZaokruzujeSelected = false;
                    Zaokruzuje100Selected = false;

                    if (CurrentItemsToNivelacija != null)
                    {
                        if (NewPriceSelected)
                        {
                            NewPrice = SetNewPrice(NewPrice);
                        }
                        else if (RazlikaCeneSelected)
                        {
                            RazlikaCene = SetNewPrice(RazlikaCene);
                        }
                        else if (ProcenatNaDosadasnjuCenuSelected)
                        {
                            ProcenatNaDosadasnjuCenu = SetNewPrice(ProcenatNaDosadasnjuCenu);
                        }
                        else if (MarzaLastCalculationSelected)
                        {
                            MarzaLastCalculation = SetNewPrice(MarzaLastCalculation);
                        }
                    }
                }
            }
        }
        public bool Zaokruzuje100Selected
        {
            get { return _zaokruzuje100Selected; }
            set
            {
                _zaokruzuje100Selected = value;
                OnPropertyChange(nameof(Zaokruzuje100Selected));
                if (value)
                {
                    NeZaokruzujeSelected = false;
                    ZaokruzujeSelected = false;
                    Zaokruzuje10Selected = false;

                    if (CurrentItemsToNivelacija != null)
                    {
                        if (NewPriceSelected)
                        {
                            NewPrice = SetNewPrice(NewPrice);
                        }
                        else if (RazlikaCeneSelected)
                        {
                            RazlikaCene = SetNewPrice(RazlikaCene);
                        }
                        else if (ProcenatNaDosadasnjuCenuSelected)
                        {
                            ProcenatNaDosadasnjuCenu = SetNewPrice(ProcenatNaDosadasnjuCenu);
                        }
                        else if (MarzaLastCalculationSelected)
                        {
                            MarzaLastCalculation = SetNewPrice(MarzaLastCalculation);
                        }
                    }
                }
            }
        }
        public NivelacijaItem? CurrentNivelacijaItem
        {
            get { return _currentNivelacijaItem; }
            set
            {
                _currentNivelacijaItem = value;
                OnPropertyChange(nameof(CurrentNivelacijaItem));
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
        public GroupItems CurrentGroup
        {
            get { return _currentGroup; }
            set
            {
                _currentGroup = value;
                OnPropertyChange(nameof(CurrentGroup));
            }
        }
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                OnPropertyChange(nameof(SearchText));

                if(string.IsNullOrEmpty(value))
                {
                    Items = new ObservableCollection<Invertory>(SearchItems);
                }
                else
                {
                    Items = new ObservableCollection<Invertory>(SearchItems.Where(item => 
                    item.Item.Name.ToLower().Contains(value.ToLower())));
                }
            }
        }
        public ObservableCollection<Invertory> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                OnPropertyChange(nameof(Items));
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
        public NivelacijaItem CurrentItemsToNivelacija
        {
            get { return _currentItemsToNivelacija; }
            set
            {
                _currentItemsToNivelacija = value;
                OnPropertyChange(nameof(CurrentItemsToNivelacija));
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
        public ICommand SearchNivelacijaItemsCommand => new SearchNivelacijaItemsCommand(this);
        public ICommand OpenWindowsAddToNivelacijaCommand => new OpenWindowsAddToNivelacijaCommand(this);
        public ICommand AddToNivelacijaCommand => new AddToNivelacijaCommand(this);
        public ICommand RemoveFromNivelacijaCommand => new RemoveFromNivelacijaCommand(this);
        public ICommand SaveCommand => new SaveNivelacijaCommand(this);
        #endregion Command

        #region Public methods
        #endregion Public methods

        #region Internal methods
        internal void Reset()
        {
            CurrentNivelacija = new Nivelacija(DbContext,
                NivelacijaStateEnumeration.Nivelacija);

            Groups = new List<GroupItems>() { new GroupItems()
            {
                Id = -1,
                IdSupergroup = -1,
                Name = "Sve grupe"
            } };
            TotalNivelacija = 0;
            TotalNewNivelacija = 0;
            TotalOldNivelacija = 0;
            TotalPdvNivelacija = 0;

            if (DbContext.Items != null &&
                DbContext.Items.Any())
            {
                DbContext.Items.ToList().ForEach(x =>
                {
                    Item item = new Item(x);
                    var group = DbContext.ItemGroups.Find(x.IdItemGroup);

                    if (group != null)
                    {
                        bool isSirovina = group.Name.ToLower().Contains("sirovina") || group.Name.ToLower().Contains("sirovine") ? true : false;
                        AllItems.Add(new Invertory(item, x.IdItemGroup, x.TotalQuantity, 0, x.AlarmQuantity, isSirovina));
                    }
                });
            }

            SearchItems = new List<Invertory>(AllItems);

            if (DbContext.ItemGroups != null &&
                DbContext.ItemGroups.Any())
            {
                DbContext.ItemGroups.ToList().ForEach(group =>
                {
                    if (group.Name.ToLower().Contains("sirovine") ||
                       group.Name.ToLower().Contains("sirovina"))
                    {

                    }
                    else
                    {
                        Groups.Add(new GroupItems(group));
                    }
                });
            }

            AllGroups = new ObservableCollection<GroupItems>(Groups);
            CurrentGroup = AllGroups.FirstOrDefault();

            CurrentNivelacijaItem = null;
        }
        #endregion Internal methods

        #region Private methods
        private decimal SetNewPrice(decimal value)
        {
            if (CurrentItemsToNivelacija != null)
            {
                if (NewPriceSelected)
                {
                    if (NeZaokruzujeSelected)
                    {
                        value = Decimal.Round(value, 2);
                    }
                    else if (ZaokruzujeSelected)
                    {
                        value = Decimal.Round(value, 0, MidpointRounding.AwayFromZero);
                    }
                    else if (Zaokruzuje10Selected)
                    {
                        if (value >= 10 ||
                            value <= -10)
                        {
                            decimal pom = value % 10;

                            if (value > 0)
                            {
                                if (pom >= 6)
                                {
                                    value = value + (10 - pom);
                                }
                                else
                                {
                                    value = value - pom;
                                }
                            }
                            else
                            {
                                if (pom >= 6)
                                {
                                    value = value - (10 - pom);
                                }
                                else
                                {
                                    value = value + pom;
                                }
                            }
                        }
                        else
                        {
                            if (value > 0)
                            {
                                value = 10;
                            }
                            else
                            {
                                value = -10;
                            }
                        }
                    }
                    else if (Zaokruzuje100Selected)
                    {
                        if (value >= 100 ||
                            value <= -100)
                        {
                            decimal pom = value % 100;

                            if (value > 0)
                            {
                                if (pom >= 51)
                                {
                                    value = value + (100 - pom);
                                }
                                else
                                {
                                    value = value - pom;
                                }
                            }
                            else
                            {
                                if (pom >= 51)
                                {
                                    value = value - (100 - pom);
                                }
                                else
                                {
                                    value = value + pom;
                                }
                            }
                        }
                        else
                        {
                            if (value > 0)
                            {
                                value = 100;
                            }
                            else
                            {
                                value = -100;
                            }
                        }
                    }
                }
                else if (RazlikaCeneSelected)
                {
                    decimal newPrice = CurrentItemsToNivelacija.OldPrice + value;
                    if (NeZaokruzujeSelected)
                    {
                        value = Decimal.Round(newPrice, 2) - CurrentItemsToNivelacija.OldPrice;
                    }
                    else if (ZaokruzujeSelected)
                    {
                        value = Decimal.Round(newPrice, 0, MidpointRounding.AwayFromZero) - CurrentItemsToNivelacija.OldPrice;
                    }
                    else if (Zaokruzuje10Selected)
                    {
                        if (newPrice >= 10 ||
                            newPrice <= -10)
                        {
                            decimal pom = newPrice % 10;

                            if (value > 0)
                            {
                                if (pom >= 6)
                                {
                                    value = (newPrice + (10 - pom)) - CurrentItemsToNivelacija.OldPrice;
                                }
                                else
                                {
                                    value = (newPrice - pom) - CurrentItemsToNivelacija.OldPrice;
                                }
                            }
                            else
                            {
                                if (pom >= 6)
                                {
                                    value = (newPrice - (10 - pom)) - CurrentItemsToNivelacija.OldPrice;
                                }
                                else
                                {
                                    value = (newPrice + pom) - CurrentItemsToNivelacija.OldPrice;
                                }
                            }
                        }
                        else
                        {
                            if (value > 0)
                            {
                                value = 10 - newPrice;
                            }
                            else
                            {
                                value = -10 + newPrice;
                            }
                        }
                    }
                    else if (Zaokruzuje100Selected)
                    {
                        if (newPrice >= 100 ||
                            newPrice <= -100)
                        {
                            decimal pom = newPrice % 100;

                            if (value > 0)
                            {
                                if (pom >= 51)
                                {
                                    value = (newPrice + (100 - pom)) - CurrentItemsToNivelacija.OldPrice;
                                }
                                else
                                {
                                    value = (newPrice - pom) - CurrentItemsToNivelacija.OldPrice;
                                }
                            }
                            else
                            {
                                if (pom >= 51)
                                {
                                    value = (newPrice - (100 - pom)) - CurrentItemsToNivelacija.OldPrice;
                                }
                                else
                                {
                                    value = (newPrice + pom) - CurrentItemsToNivelacija.OldPrice;
                                }
                            }
                        }
                        else
                        {
                            if (value > 0)
                            {
                                value = 100 - newPrice;
                            }
                            else
                            {
                                value = -100 + newPrice;
                            }
                        }
                    }
                }
                else if (ProcenatNaDosadasnjuCenuSelected)
                {
                    decimal percentage = 1 + (value / 100);
                    decimal newPrice = CurrentItemsToNivelacija.OldPrice * percentage;

                    if (NeZaokruzujeSelected)
                    {
                        value = (Decimal.Round(newPrice, 2) * 100) / CurrentItemsToNivelacija.OldPrice - 100;
                    }
                    else if (ZaokruzujeSelected)
                    {
                        value = (Decimal.Round(newPrice, 0, MidpointRounding.AwayFromZero) * 100) / CurrentItemsToNivelacija.OldPrice - 100;
                    }
                    else if (Zaokruzuje10Selected)
                    {
                        if (newPrice >= 10 ||
                            newPrice <= -10)
                        {
                            decimal pom = newPrice % 10;

                            if (value > 0)
                            {
                                if (pom >= 6)
                                {
                                    value = ((newPrice + (10 - pom)) * 100) / CurrentItemsToNivelacija.OldPrice - 100;
                                }
                                else
                                {
                                    value = ((newPrice - pom) * 100) / CurrentItemsToNivelacija.OldPrice - 100;
                                }
                            }
                            else
                            {
                                if (pom >= 6)
                                {
                                    value = ((newPrice + (10 - pom)) * 100) / CurrentItemsToNivelacija.OldPrice - 100;
                                }
                                else
                                {
                                    value = ((newPrice - pom) * 100) / CurrentItemsToNivelacija.OldPrice - 100;
                                }
                            }
                        }
                        else
                        {
                            if (value > 0)
                            {
                                value = (10 - newPrice) * 100 / CurrentItemsToNivelacija.OldPrice - 100;
                            }
                            else
                            {
                                value = (-10 + newPrice) * 100 / CurrentItemsToNivelacija.OldPrice - 100;
                            }
                        }
                    }
                    else if (Zaokruzuje100Selected)
                    {
                        if (newPrice >= 100 ||
                            newPrice <= -100)
                        {
                            decimal pom = newPrice % 100;

                            if (value > 0)
                            {
                                if (pom >= 51)
                                {
                                    value = ((newPrice + (100 - pom)) * 100) / CurrentItemsToNivelacija.OldPrice - 100;
                                }
                                else
                                {
                                    value = ((newPrice - pom) * 100) / CurrentItemsToNivelacija.OldPrice - 100;
                                }
                            }
                            else
                            {
                                if (pom >= 51)
                                {
                                    value = ((newPrice + (100 - pom)) * 100) / CurrentItemsToNivelacija.OldPrice - 100;
                                }
                                else
                                {
                                    value = ((newPrice - pom) * 100) / CurrentItemsToNivelacija.OldPrice - 100;
                                }
                            }
                        }
                        else
                        {
                            if (value > 0)
                            {
                                value = (100 - newPrice) * 100 / CurrentItemsToNivelacija.OldPrice - 100;
                            }
                            else
                            {
                                value = (-100 + newPrice) * 100 / CurrentItemsToNivelacija.OldPrice - 100;
                            }
                        }
                    }
                }
                else if (MarzaLastCalculationSelected)
                {
                    decimal percentage = 1 + (value / 100);
                    decimal newPrice = CurrentItemsToNivelacija.LastImportPrice * percentage;

                    if (NeZaokruzujeSelected)
                    {
                        value = (Decimal.Round(newPrice, 2) * 100) / CurrentItemsToNivelacija.LastImportPrice - 100;
                    }
                    else if (ZaokruzujeSelected)
                    {
                        value = (Decimal.Round(newPrice, 0, MidpointRounding.AwayFromZero) * 100) / CurrentItemsToNivelacija.LastImportPrice - 100;
                    }
                    else if (Zaokruzuje10Selected)
                    {
                        if (newPrice >= 10 ||
                            newPrice <= -10)
                        {
                            decimal pom = newPrice % 10;

                            if (value > 0)
                            {
                                if (pom >= 6)
                                {
                                    value = ((newPrice + (10 - pom)) * 100) / CurrentItemsToNivelacija.LastImportPrice - 100;
                                }
                                else
                                {
                                    value = ((newPrice - pom) * 100) / CurrentItemsToNivelacija.LastImportPrice - 100;
                                }
                            }
                            else
                            {
                                if (pom >= 6)
                                {
                                    value = ((newPrice - (10 - pom)) * 100) / CurrentItemsToNivelacija.LastImportPrice - 100;
                                }
                                else
                                {
                                    value = ((newPrice + pom) * 100) / CurrentItemsToNivelacija.LastImportPrice - 100;
                                }
                            }
                        }
                        else
                        {
                            if (value > 0)
                            {
                                value = (10 - newPrice) * 100 / CurrentItemsToNivelacija.LastImportPrice - 100;
                            }
                            else
                            {
                                value = (-10 + newPrice) * 100 / CurrentItemsToNivelacija.LastImportPrice - 100;
                            }
                        }
                    }
                    else if (Zaokruzuje100Selected)
                    {
                        if (newPrice >= 100 ||
                            newPrice <= -100)
                        {
                            decimal pom = newPrice % 100;

                            if (value > 0)
                            {
                                if (pom >= 51)
                                {
                                    value = ((newPrice + (100 - pom)) * 100) / CurrentItemsToNivelacija.LastImportPrice - 100;
                                }
                                else
                                {
                                    value = ((newPrice - pom) * 100) / CurrentItemsToNivelacija.LastImportPrice - 100;
                                }
                            }
                            else
                            {
                                if (pom >= 51)
                                {
                                    value = ((newPrice - (100 - pom)) * 100) / CurrentItemsToNivelacija.LastImportPrice - 100;
                                }
                                else
                                {
                                    value = ((newPrice + pom) * 100) / CurrentItemsToNivelacija.LastImportPrice - 100;
                                }
                            }
                        }
                        else
                        {
                            if (value > 0)
                            {
                                value = (100 - newPrice) * 100 / CurrentItemsToNivelacija.LastImportPrice - 100;
                            }
                            else
                            {
                                value = (-100 + newPrice) * 100 / CurrentItemsToNivelacija.LastImportPrice - 100;
                            }
                        }
                    }
                }
            }
            return value;
        }
        #endregion Private methods
    }
}
