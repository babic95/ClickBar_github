﻿using ClickBar.Commands;
using ClickBar.Commands.AppMain.Report;
using ClickBar.Commands.Login;
using ClickBar.Commands.Sale;
using ClickBar.Models.Sale;
using ClickBar.Models.TableOverview;
using ClickBar_DatabaseSQLManager;
using ClickBar_DatabaseSQLManager.Models;
using ClickBar_Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ClickBar.ViewModels
{
    public class SaleViewModel : ViewModelBase
    {
        #region Fields
        private IServiceProvider _serviceProvider;
        private readonly Lazy<UpdateCurrentAppStateViewModelCommand> _updateCurrentAppStateViewModelCommand;
        private readonly Lazy<PayCommand<SaleViewModel>> _payCommand;

        private AppMainViewModel _mainViewModel;
        private string _cashierNema;

        private Visibility _tableOverviewVisibility;
        private bool _hookOrderEnable;

        private Visibility _visibilitySupergroups;

        private Supergroup _currentSupergroup;
        private GroupItems _currentGroup;
        private ObservableCollection<Supergroup> _supergroups;
        private ObservableCollection<GroupItems> _groups;
        private ObservableCollection<Item> _items;

        private ObservableCollection<ItemInvoice> _itemsInvoice;
        private ObservableCollection<OldOrder> _oldOrders;

        private decimal _totalAmount;
        private int _tableId;

        private Timer _timer;
        private string _currentDateTime;

        private Order _currentOrder;

        private Visibility _visibilityBlack;
        private bool _isEnabledRemoveOrder;

        private SerialPort? _serialPort = null;

        private string _quantity;
        private bool _firstChangeQuantity;

        private Visibility _oldOrdersVisibility;
        #endregion Fields

        #region Constructors
        public SaleViewModel(IServiceProvider serviceProvider,
            IDbContextFactory<SqlServerDbContext> dbContextFactory)
        {
            _serviceProvider = serviceProvider;
            DbContextFactory = dbContextFactory;
            _updateCurrentAppStateViewModelCommand = new Lazy<UpdateCurrentAppStateViewModelCommand>(() => serviceProvider.GetRequiredService<UpdateCurrentAppStateViewModelCommand>());
            
            //TableOverviewCommand = serviceProvider.GetRequiredService<TableOverviewCommand>();
            //HookOrderOnTableCommand = serviceProvider.GetRequiredService<HookOrderOnTableCommand>();
            //PayCommand = _serviceProvider.GetRequiredService<PayCommand<SaleViewModel>>();

            //SetCashier();

            _payCommand = new Lazy<PayCommand<SaleViewModel>>(() => new PayCommand<SaleViewModel>(_serviceProvider, this));

            var comPort = SettingsManager.Instance.GetComPort();

            if (!string.IsNullOrEmpty(comPort))
            {
                _serialPort = new SerialPort(comPort, 9600);
                _serialPort.WriteTimeout = 500;
            }

            UpdateSaleViewModel();

            RunTimer();
        }
        #endregion Constructors

        #region Internal Properties
        internal IDbContextFactory<SqlServerDbContext> DbContextFactory
        {
            get; private set;
        }
        internal List<ItemGroupDB> AllGroups { get; set; }
        internal List<ItemDB> AllItems { get; set; }
        internal CashierDB LoggedCashier { get; set; }
        internal Window WindowZelja { get; set; }
        internal Window PayWindow { get; set; }
        #endregion Internal Properties

        #region Properties
        public PartHall? CurrentPartHall { get; set; }
        //public TableOverviewViewModel TableOverviewViewModel { get; set; }
        public Order CurrentOrder
        {
            get { return _currentOrder; }
            set
            {
                _currentOrder = value;
                OnPropertyChange(nameof(CurrentOrder));
            }
        }
        public string CurrentDateTime
        {
            get { return _currentDateTime; }
            set
            {
                _currentDateTime = value;
                OnPropertyChange(nameof(CurrentDateTime));
            }
        }
        public string CashierNema
        {
            get { return _cashierNema; }
            set
            {
                _cashierNema = value;
                OnPropertyChange(nameof(CashierNema));
            }
        }
        public bool IsEnabledRemoveOrder
        {
            get { return _isEnabledRemoveOrder; }
            set
            {
                _isEnabledRemoveOrder = value;
                OnPropertyChange(nameof(IsEnabledRemoveOrder));
            }
        }
        public Visibility VisibilitySupergroups
        {
            get { return _visibilitySupergroups; }
            set
            {
                _visibilitySupergroups = value;
                OnPropertyChange(nameof(VisibilitySupergroups));
            }
        }
        public Visibility TableOverviewVisibility
        {
            get { return _tableOverviewVisibility; }
            set
            {
                _tableOverviewVisibility = value;
                OnPropertyChange(nameof(TableOverviewVisibility));
            }
        }
        public bool HookOrderEnable
        {
            get { return _hookOrderEnable; }
            set
            {
                _hookOrderEnable = value;
                OnPropertyChange(nameof(HookOrderEnable));
            }
        }

        public Supergroup CurrentSupergroup
        {
            get { return _currentSupergroup; }
            set
            {
                _currentSupergroup = value;
                OnPropertyChange(nameof(CurrentSupergroup));
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

        public ObservableCollection<Supergroup> Supergroups
        {
            get { return _supergroups; }
            set
            {
                _supergroups = value;
                OnPropertyChange(nameof(Supergroups));
            }
        }
        public ObservableCollection<GroupItems> Groups
        {
            get { return _groups; }
            set
            {
                _groups = value;
                OnPropertyChange(nameof(Groups));
            }
        }
        public ObservableCollection<Item> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                OnPropertyChange(nameof(Items));
            }
        }
        public Visibility OldOrdersVisibility
        {
            get { return _oldOrdersVisibility; }
            set
            {
                _oldOrdersVisibility = value;
                OnPropertyChange(nameof(OldOrdersVisibility));
            }
        }
        public ObservableCollection<OldOrder> OldOrders
        {
            get { return _oldOrders; }
            set
            {
                _oldOrders = value;
                OnPropertyChange(nameof(OldOrders));

                if (value != null && value.Any())
                {
                    OldOrdersVisibility = Visibility.Visible;
                }
                else
                {
                    OldOrdersVisibility = Visibility.Collapsed;
                }
            }
        }
        public ObservableCollection<ItemInvoice> ItemsInvoice
        {
            get { return _itemsInvoice; }
            set
            {
                _itemsInvoice = value;
                OnPropertyChange(nameof(ItemsInvoice));

                if (value != null && value.Any())
                {
                    HookOrderEnable = true;
                }
                else
                {
                    HookOrderEnable = false;
                }
            }
        }
        public decimal TotalAmount
        {
            get { return _totalAmount; }
            set
            {
                _totalAmount = Decimal.Round(value, 2);
                OnPropertyChange(nameof(TotalAmount));
            }
        }
        public int TableId
        {
            get { return _tableId; }
            set
            {
                _tableId = value;
                OnPropertyChange(nameof(TableId));
            }
        }
        public string Quantity
        {
            get { return _quantity; }
            set
            {
                _quantity = value;
                OnPropertyChange(nameof(Quantity));
            }
        }
        public bool FirstChangeQuantity
        {
            get { return _firstChangeQuantity; }
            set
            {
                _firstChangeQuantity = value;
                OnPropertyChange(nameof(FirstChangeQuantity));
                if (value)
                {
                    Quantity = "1";
                }
            }
        }
        #endregion Properties

        #region Commands
        public ICommand ClickOnQuantityButtonCommand => new ClickOnQuantityButtonCommand(this);
        public ICommand UpdateAppViewModelCommand => _updateCurrentAppStateViewModelCommand.Value;
        public ICommand LogoutCommand => new LogoutCommand(this, _serviceProvider);
        public ICommand SelectSupergroupCommand => new SelectSupergroupCommand(this);
        public ICommand SelectGroupCommand => new SelectGroupCommand(this);
        public ICommand SelectItemCommand => new SelectItemCommand(this);
        public ICommand ResetAllCommand => new ResetAllCommand(this);
        public ICommand PayCommand => _payCommand.Value;
        public ICommand HookOrderOnTableCommand => new HookOrderOnTableCommand(this, _serviceProvider);
        //public ICommand HookOrderOnTableCommand { get; }
        //public ICommand TableOverviewCommand { get; }
        public ICommand TableOverviewCommand => new TableOverviewCommand(this);
        public ICommand ReduceQuantityCommand => new ReduceQuantityCommand(this);
        public ICommand PrintReportCommand => new PrintReportCommand(this);
        public ICommand RemoveOrderCommand => new RemoveOrderCommand(this);
        public ICommand OpenListaZeljaCommand => new OpenListaZeljaCommand(this);
        #endregion Commands

        #region Public methods
        public void UpdateSaleViewModel()
        {
            if (DbContextFactory != null)
            {
                Supergroups = new ObservableCollection<Supergroup>();
                Groups = new ObservableCollection<GroupItems>();
                Items = new ObservableCollection<Item>();

                using (var dbContext = DbContextFactory.CreateDbContext())
                {
                    AllItems = dbContext.Items.AsNoTracking().Where(i => i.DisableItem == 0).Include(i => i.Zelje).ToList();
                    AllGroups = dbContext.ItemGroups
                        .AsNoTracking()
                        .Where(g => g.Name.ToLower() != "sirovine" && g.Name.ToLower() != "sirovina")
                        .ToList();

                    if (SettingsManager.Instance.EnableSuperGroup())
                    {
                        var supergroups = dbContext.Supergroups
                            .AsNoTracking()
                            .Where(s => !s.Name.ToLower().Contains("osnovna"))
                            .OrderBy(s => s.Rb)
                            .Select(s => new Supergroup(s))
                            .ToList();

                        supergroups.ForEach(s => Supergroups.Add(s));

                        VisibilitySupergroups = Visibility.Visible;
                    }
                    else
                    {
                        var groups = AllGroups.OrderBy(g => g.Rb)
                            .Select(g => new GroupItems(g))
                            .ToList();

                        groups.ForEach(g => Groups.Add(g));

                        VisibilitySupergroups = Visibility.Hidden;
                    }
                }

                ItemsInvoice = new ObservableCollection<ItemInvoice>();
                OldOrders = new ObservableCollection<OldOrder>();
                TotalAmount = 0;

                if (SettingsManager.Instance.EnableTableOverview())
                {
                    TableOverviewVisibility = Visibility.Visible;
                }
                else
                {
                    TableOverviewVisibility = Visibility.Hidden;
                }

                if (SettingsManager.Instance.CancelOrderFromTable())
                {
                    IsEnabledRemoveOrder = true;
                }
                else
                {
                    IsEnabledRemoveOrder = false;
                }
                FirstChangeQuantity = true;
            }
        }
        public void SetCashier()
        {
            LoggedCashier = _serviceProvider.GetRequiredService<CashierDB>();
            CashierNema = LoggedCashier.Name;
        }
        #endregion Public methods

        #region Internal methods
        //internal void SetOrder(Order order)
        //{
        //    TableId = order.TableId;
        //    CashierNema = order.Cashier.Name;
        //    TotalAmount = 0;
        //    ItemsInvoice = order.Items;
        //    order.Items.ToList().ForEach(item =>
        //    {
        //        TotalAmount += item.TotalAmout;
        //    });
        //}
        internal void Reset()
        {
            CurrentOrder = null;
            CashierNema = LoggedCashier != null ? LoggedCashier.Name : string.Empty;
            TableId = 0;
            TotalAmount = 0;
            ItemsInvoice = new ObservableCollection<ItemInvoice>();
            OldOrders = new ObservableCollection<OldOrder>();
            HookOrderEnable = false;

            //var dbContextFactory = _serviceProvider.GetRequiredService<IDbContextFactory<SqlServerDbContext>>();

            //TableOverviewViewModel = _serviceProvider.GetRequiredService<TableOverviewViewModel>();
        }

        internal void SendToDisplay(string nameItem, string? priceItem = null)
        {
            try
            {
                if (_serialPort != null)
                {
                    string totalMesage = string.Empty;

                    if (string.IsNullOrEmpty(priceItem))
                    {
                        totalMesage += CenterString(nameItem, 20);
                        totalMesage += CenterString("PRIJATAN DAN!", 20);
                    }
                    else
                    {
                        totalMesage += SplitMessageToParts(priceItem, $"{nameItem}:", 20);
                        totalMesage += SplitMessageToParts(string.Format("{0:#,##0.00}", TotalAmount).Replace(',', '#').Replace('.', ',').Replace('#', '.'), "Ukupno:", 20);
                    }

                    _serialPort.Open();
                    _serialPort.Write(totalMesage);
                    _serialPort.Close();
                }
            }
            catch (Exception ex)
            {
                ClickBar_Logging.Log.Error($"SaleViewModel - SendToDisplay - Greska prilikom slanja na COM port -> ", ex);
            }
        }
        #endregion Internal methods

        #region Private methods

        private void RunTimer()
        {
            _timer = new Timer(
                async (e) =>
                {
                    CurrentDateTime = DateTime.Now.ToString("dd.MM.yyyy  HH:mm:ss");
                },
                null,
                0,
                1000);
        }
        private static string SplitMessageToParts(string value, string fixedPart, int length)
        {
            string totalValue = value + fixedPart;

            int totalLength = totalValue.Length - length;
            if (totalLength > 0)
            {
                fixedPart = fixedPart.Substring(0, fixedPart.Length - totalLength - 2) + fixedPart[fixedPart.Length - 1];
            }

            string journal = string.Empty;

            if (string.IsNullOrEmpty(value))
            {
                value = string.Empty;
            }

            journal = string.Format("{0}{1}", fixedPart, value.PadLeft(length - fixedPart.Length));

            return journal;
        }
        private static string CenterString(string value, int length)
        {
            string journal = string.Empty;
            if (string.IsNullOrEmpty(value))
            {
                value = string.Empty;
            }

            int spaces = length - value.Length;
            int padLeft = spaces / 2 + value.Length;

            return $"{value.PadLeft(padLeft).PadRight(length)}\r\n";
        }
        #endregion Private methods
    }
}