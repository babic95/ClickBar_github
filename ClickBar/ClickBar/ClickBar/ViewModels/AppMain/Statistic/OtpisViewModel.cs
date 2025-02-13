using ClickBar.Commands.AppMain.Statistic.Otpis;
using ClickBar.Commands.AppMain.Statistic.ViewCalculation;
using ClickBar.Models.AppMain.Statistic;
using ClickBar.Models.AppMain.Statistic.Otpis;
using ClickBar.Models.Sale;
using ClickBar_DatabaseSQLManager;
using ClickBar_DatabaseSQLManager.Models;
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
    public class OtpisViewModel : ViewModelBase
    {
        #region Fields
        private ObservableCollection<OtpisItem> _itemsInOtpis;
        private OtpisItem _currentItem;

        private List<Invertory> _allItems;
        private ObservableCollection<Invertory> _items;

        private string _textSearch;
        private Invertory _selectedItem;

        private string _quantityString;

        private Visibility _visibilityNext;

        private readonly IServiceProvider _serviceProvider; // Dodato za korišćenje IServiceProvider
        #endregion Fields

        #region Constructors
        public OtpisViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            DbContext = _serviceProvider.GetRequiredService<IDbContextFactory<SqlServerDbContext>>().CreateDbContext();
            LoggedCashier = serviceProvider.GetRequiredService<CashierDB>();

            ItemsInOtpis = new ObservableCollection<OtpisItem>();
            CurrentItem = new OtpisItem();

            _allItems = new List<Invertory>();

            foreach(var itemDB in DbContext.Items)
            {
                Invertory item = new Invertory(new Item(itemDB), -1, itemDB.TotalQuantity, 0, 0, false);
                _allItems.Add(item);
            }

            Items = new ObservableCollection<Invertory>(_allItems);

            VisibilityNext = Visibility.Hidden;
        }
        #endregion Constructors

        #region Properties internal
        internal SqlServerDbContext DbContext
        {
            get; private set;
        }
        internal CashierDB LoggedCashier { get; private set; }
        internal Window AllItemsWindow { get; set; }
        internal Window QuantityWindow { get; set; }
        #endregion Properties internal

        #region Properties
        public ObservableCollection<OtpisItem> ItemsInOtpis
        {
            get { return _itemsInOtpis; }
            set
            {
                _itemsInOtpis = value;
                OnPropertyChange(nameof(ItemsInOtpis));
            }
        }
        public OtpisItem CurrentItem
        {
            get { return _currentItem; }
            set
            {
                _currentItem = value;
                OnPropertyChange(nameof(CurrentItem));
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
        public string TextSearch
        {
            get { return _textSearch; }
            set
            {
                _textSearch = value;
                OnPropertyChange(nameof(TextSearch));

                if (string.IsNullOrEmpty(value))
                {
                    Items = new ObservableCollection<Invertory>(_allItems);
                }
                else
                {
                    Items = new ObservableCollection<Invertory>(_allItems.Where(i => i.Item.Name.ToLower().Contains(value)));
                }
            }
        }
        public Invertory SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                OnPropertyChange(nameof(SelectedItem));

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
        public string QuantityString
        {
            get { return _quantityString; }
            set
            {
                _quantityString = value;
                OnPropertyChange(nameof(QuantityString));
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
        #endregion Properties

        #region Commands
        public ICommand OpenAllItemsCommand => new OpenAllItemsCommand(this);
        public ICommand NextOtpisCommand => new NextOtpisCommand(this);
        public ICommand AddItemToOtpisCommand => new AddItemToOtpisCommand(this);
        public ICommand CreateOtpisCommand => new CreateOtpisCommand(this);
        public ICommand RemoveItemForOtpisCommand => new RemoveItemForOtpisCommand(this);
        #endregion Commands

        #region Public methods
        #endregion Public methods

        #region Private methods
        #endregion Private methods
    }
}