using ClickBar.Commands.Sale;
using ClickBar.Commands.Sale.Pay;
using ClickBar.Enums.Sale;
using ClickBar.Enums.Sale.Buyer;
using ClickBar.Models.AppMain.Statistic;
using ClickBar.Models.Sale;
using ClickBar.Models.Sale.Buyer;
using ClickBar_Common.Models.Invoice;
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
using System.Windows.Media;

namespace ClickBar.ViewModels.Sale
{
    public enum FocusEnumeration
    {
        BuyerId = 0,
        Cash = 1,
        Card = 2,
        Check = 3,
        Voucher = 4,
        Other = 5,
        WireTransfer = 6,
        MobileMoney = 7,
        Pay = 8,
    }
    public class PaySaleViewModel : ViewModelBase
    {
        #region Fields
        private IServiceProvider _serviceProvider;
        private readonly Lazy<PayCommand<PaySaleViewModel>> _payCommand;

        private FocusEnumeration _focus;

        private ObservableCollection<ItemInvoice> _itemsInvoice;
        private InvoiceTypeEnumeration _invoiceType;
        private decimal _totalAmount;
        private decimal _amount;
        private decimal _rest;
        private Brush _amountBorderBrush;
        private bool _isEnablePay;

        private Visibility _buyerVisibility;

        private string _cashierNema;
        private string _buyerId;
        private string _buyerName;
        private string _buyerAdress;
        private string _popust;
        private ObservableCollection<BuyerIdElement> _buyerIdElements;
        private BuyerIdElement _currentBuyerIdElement;
        private ObservableCollection<Partner> _partners;
        private Partner _currentPartner;

        private string _cash;
        private string _card;
        private string _check;
        private string _voucher;
        private string _other;
        private string _wireTransfer;
        private string _mobileMoney;

        private Visibility _visibilityBlack;
        private Visibility _visibilityBlackView;
        private Visibility _visibilityKartica; 
        #endregion Fields

        #region Constructors
        public PaySaleViewModel(IServiceProvider serviceProvider, SaleViewModel saleViewModel)
        {
            _serviceProvider = serviceProvider;
            DbContext = _serviceProvider.GetRequiredService<IDbContextFactory<SqlServerDbContext>>();

            _payCommand = new Lazy<PayCommand<PaySaleViewModel>>(() => new PayCommand<PaySaleViewModel>(serviceProvider, this));
            //PayCommand = _serviceProvider.GetRequiredService<PayCommand<PaySaleViewModel>>();
            //SplitOrderCommand = _serviceProvider.GetRequiredService<SplitOrderCommand>();

            if (SettingsManager.Instance.GetEnabledKartica())
            {
                VisibilityKartica = Visibility.Visible;
            }
            else
            {
                VisibilityKartica = Visibility.Collapsed;
            }

#if CRNO
            VisibilityBlack = Visibility.Hidden;
#else
                VisibilityBlack = Visibility.Visible;
#endif

            SaleViewModel = saleViewModel;
            CashierNema = saleViewModel.CashierNema;
            BuyerVisibility = Visibility.Hidden;
            SplitOrder = false;

            Payment = new List<Payment>();

            ItemsInvoice = SaleViewModel.ItemsInvoice;
            TotalAmount = SaleViewModel.TotalAmount;
            InvoiceType = InvoiceTypeEnumeration.Promet;

            BuyerIdElements = new ObservableCollection<BuyerIdElement>();
            var buyerIdElements = Enum.GetValues(typeof(BuyerIdElementEnumeration));

            foreach (var buyerIdElement in buyerIdElements)
            {
                BuyerIdElements.Add(new BuyerIdElement((BuyerIdElementEnumeration)buyerIdElement));
            }
            CurrentBuyerIdElement = BuyerIdElements.FirstOrDefault();

            using (var dbContext = DbContext.CreateDbContext())
            {
                Partners = new ObservableCollection<Partner>();
                foreach(var partner in dbContext.Partners)
                {
                    Partners.Add(new Partner(partner));
                }
            }
            CurrentPartner = new Partner();

        }
        #endregion Constructors

        #region Properties
        public Window SplitOrderWindow { get; set; }
        public Visibility VisibilityBlackView
        {
            get { return _visibilityBlackView; }
            set
            {
                _visibilityBlackView = value;
                OnPropertyChange(nameof(VisibilityBlackView));
            }
        }
        
        public Visibility VisibilityKartica
        {
            get { return _visibilityKartica; }
            set
            {
                _visibilityKartica = value;
                OnPropertyChange(nameof(VisibilityKartica));
            }
        }
        public Visibility VisibilityBlack
        {
            get { return _visibilityBlack; }
            set
            {
                _visibilityBlack = value;
                OnPropertyChange(nameof(VisibilityBlack));
                if (value == Visibility.Visible)
                {
                    VisibilityBlackView = Visibility.Collapsed;
                }
                else
                {
                    VisibilityBlackView = Visibility.Visible;
                    VisibilityKartica = Visibility.Collapsed;
                }
            }
        }
        public bool SplitOrder { get; set; }
        public Visibility BuyerVisibility
        {
            get { return _buyerVisibility; }
            set
            {
                _buyerVisibility = value;
                OnPropertyChange(nameof(BuyerVisibility));
            }
        }
        public FocusEnumeration Focus
        {
            get { return _focus; }
            set
            {
                _focus = value;
                OnPropertyChange(nameof(Focus));
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
        public bool IsEnablePay
        {
            get { return _isEnablePay; }
            set
            {
                _isEnablePay = value;
                OnPropertyChange(nameof(IsEnablePay));
            }
        }
        public ObservableCollection<ItemInvoice> ItemsInvoice
        {
            get { return _itemsInvoice; }
            set
            {
                _itemsInvoice = value;
                OnPropertyChange(nameof(ItemsInvoice));
            }
        }
        public InvoiceTypeEnumeration InvoiceType
        {
            get { return _invoiceType; }
            set
            {
                _invoiceType = value;
                OnPropertyChange(nameof(InvoiceType));
            }
        }
        public decimal TotalAmount
        {
            get { return _totalAmount; }
            set
            {
                _totalAmount = value;
                OnPropertyChange(nameof(TotalAmount));

                AmountBorderBrush = Brushes.Red;

                Cash = value.ToString();// "0";
                Card = "0";
                Check = "0";
                Voucher = "0";
                Other = "0";
                WireTransfer = "0";
                MobileMoney = "0";

                Focus = FocusEnumeration.Cash;

                Rest = value;
            }
        }
        public decimal Amount
        {
            get { return _amount; }
            set
            {
                _amount = value;
                OnPropertyChange(nameof(Amount));

                if (value >= TotalAmount)
                {
                    AmountBorderBrush = Brushes.Transparent;
                    IsEnablePay = true;
                }
                else
                {
                    AmountBorderBrush = Brushes.Red;
                    IsEnablePay = false;
                }
                Rest = TotalAmount - value;
            }
        }
        public decimal Rest
        {
            get { return _rest; }
            set
            {
                _rest = value;
                OnPropertyChange(nameof(Rest));
            }
        }
        
        public ObservableCollection<Partner> Partners
        {
            get { return _partners; }
            set
            {
                _partners = value;
                OnPropertyChange(nameof(Partners));
            }
        }
        public Partner CurrentPartner
        {
            get { return _currentPartner; }
            set
            {
                _currentPartner = value;
                OnPropertyChange(nameof(CurrentPartner));

                if(value != null)
                {
                    BuyerId = value.Pib;
                    BuyerName = value.Name;
                    if (!string.IsNullOrEmpty(value.Address))
                    {
                        BuyerAdress = value.Address;

                        if (!string.IsNullOrEmpty(value.City))
                        {
                            BuyerAdress += $" {value.City}";
                        }
                    }
                }
            }
        }
        public ObservableCollection<BuyerIdElement> BuyerIdElements
        {
            get { return _buyerIdElements; }
            set
            {
                _buyerIdElements = value;
                OnPropertyChange(nameof(BuyerIdElements));
            }
        }
        public BuyerIdElement CurrentBuyerIdElement
        {
            get { return _currentBuyerIdElement; }
            set
            {
                _currentBuyerIdElement = value;
                OnPropertyChange(nameof(CurrentBuyerIdElement));
            }
        }
        public Brush AmountBorderBrush
        {
            get { return _amountBorderBrush; }
            set
            {
                _amountBorderBrush = value;
                OnPropertyChange(nameof(AmountBorderBrush));
            }
        }
        public string BuyerId
        {
            get { return _buyerId; }
            set
            {
                _buyerId = value;
                OnPropertyChange(nameof(BuyerId));

                if (!string.IsNullOrEmpty(value))
                {
                    BuyerVisibility = Visibility.Visible;
                }
                else
                {
                    BuyerVisibility = Visibility.Hidden;
                }

                if (!string.IsNullOrEmpty(value) &&
                    Amount >= SaleViewModel.TotalAmount)
                {
                    AmountBorderBrush = Brushes.Transparent;
                    IsEnablePay = true;
                }
                else
                {
                    AmountBorderBrush = Brushes.Red;
                    IsEnablePay = false;
                }
            }
        }
        public string BuyerName
        {
            get { return _buyerName; }
            set
            {
                _buyerName = value;
                OnPropertyChange(nameof(BuyerName));
            }
        }
        public string BuyerAdress
        {
            get { return _buyerAdress; }
            set
            {
                _buyerAdress = value;
                OnPropertyChange(nameof(BuyerAdress));
            }
        }
        public string Popust
        {
            get { return _popust; }
            set
            {
                _popust = value;
                OnPropertyChange(nameof(Popust));
            }
        }
        public string Cash
        {
            get { return _cash; }
            set
            {
                try
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        Convert.ToDecimal(value);

                        if (value[value.Length - 1] == '.' || value[value.Length - 1] == ',')
                        {
                            if (value.Length > _cash.Length)
                            {
                                value += "0";
                            }
                            else
                            {
                                value = value.Remove(value.Length - 1, 1);
                            }
                        }
                    }
                    else
                    {
                        value = "0";
                    }

                    _cash = value;
                    OnPropertyChange(nameof(Cash));

                    if (Amount >= SaleViewModel.TotalAmount)
                    {
                        AmountBorderBrush = Brushes.Transparent;
                        IsEnablePay = true;
                        Focus = FocusEnumeration.Pay;
                    }
                }
                catch
                {
                    MessageBox.Show("Polje 'Gotovina' mora biti broj!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        public string Card
        {
            get { return _card; }
            set
            {
                try
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        Convert.ToDecimal(value);

                        if (value[value.Length - 1] == '.' || value[value.Length - 1] == ',')
                        {
                            if (value.Length > _card.Length)
                            {
                                value += "0";
                            }
                            else
                            {
                                value = value.Remove(value.Length - 1, 1);
                            }
                        }
                    }
                    else
                    {
                        value = "0";
                    }

                    _card = value;
                    OnPropertyChange(nameof(Card));

                    if (Amount >= SaleViewModel.TotalAmount)
                    {
                        AmountBorderBrush = Brushes.Transparent;
                        IsEnablePay = true;
                        Focus = FocusEnumeration.Pay;
                    }
                }
                catch
                {
                    MessageBox.Show("Polje 'Platna kartica' mora biti broj!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        public string Check
        {
            get { return _check; }
            set
            {
                try
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        Convert.ToDecimal(value);

                        if (value[value.Length - 1] == '.' || value[value.Length - 1] == ',')
                        {
                            if (value.Length > _check.Length)
                            {
                                value += "0";
                            }
                            else
                            {
                                value = value.Remove(value.Length - 1, 1);
                            }
                        }
                    }
                    else
                    {
                        value = "0";
                    }

                    _check = value;
                    OnPropertyChange(nameof(Check));

                    if (Amount >= SaleViewModel.TotalAmount)
                    {
                        AmountBorderBrush = Brushes.Transparent;
                        IsEnablePay = true;
                        Focus = FocusEnumeration.Pay;
                    }
                }
                catch
                {
                    MessageBox.Show("Polje 'Ček' mora biti broj!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        public string Voucher
        {
            get { return _voucher; }
            set
            {
                try
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        Convert.ToDecimal(value);

                        if (value[value.Length - 1] == '.' || value[value.Length - 1] == ',')
                        {
                            if (value.Length > _voucher.Length)
                            {
                                value += "0";
                            }
                            else
                            {
                                value = value.Remove(value.Length - 1, 1);
                            }
                        }
                    }
                    else
                    {
                        value = "0";
                    }

                    _voucher = value;
                    OnPropertyChange(nameof(Voucher));

                    if (Amount >= SaleViewModel.TotalAmount)
                    {
                        AmountBorderBrush = Brushes.Transparent;
                        IsEnablePay = true;
                        Focus = FocusEnumeration.Pay;
                    }
                }
                catch
                {
                    MessageBox.Show("Polje 'Vaučer' mora biti broj!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        public string Other
        {
            get { return _other; }
            set
            {
                try
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        Convert.ToDecimal(value);

                        if (value[value.Length - 1] == '.' || value[value.Length - 1] == ',')
                        {
                            if (value.Length > _other.Length)
                            {
                                value += "0";
                            }
                            else
                            {
                                value = value.Remove(value.Length - 1, 1);
                            }
                        }
                    }
                    else
                    {
                        value = "0";
                    }

                    _other = value;
                    OnPropertyChange(nameof(Other));

                    if (Amount >= SaleViewModel.TotalAmount)
                    {
                        AmountBorderBrush = Brushes.Transparent;
                        IsEnablePay = true;
                        Focus = FocusEnumeration.Pay;
                    }
                }
                catch
                {
                    MessageBox.Show("Polje 'Drugo bezgotovinsko plaćanje' mora biti broj!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        public string WireTransfer
        {
            get { return _wireTransfer; }
            set
            {
                try
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        Convert.ToDecimal(value);

                        if (value[value.Length - 1] == '.' || value[value.Length - 1] == ',')
                        {
                            if (value.Length > _wireTransfer.Length)
                            {
                                value += "0";
                            }
                            else
                            {
                                value = value.Remove(value.Length - 1, 1);
                            }
                        }
                    }
                    else
                    {
                        value = "0";
                    }

                    _wireTransfer = value;
                    OnPropertyChange(nameof(WireTransfer));

                    if (Amount >= SaleViewModel.TotalAmount)
                    {
                        AmountBorderBrush = Brushes.Transparent;
                        IsEnablePay = true;
                        Focus = FocusEnumeration.Pay;
                    }
                }
                catch
                {
                    MessageBox.Show("Polje 'Prenos na račun' mora biti broj!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        public string MobileMoney
        {
            get { return _mobileMoney; }
            set
            {
                try
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        Convert.ToDecimal(value);

                        if (value[value.Length - 1] == '.' || value[value.Length - 1] == ',')
                        {
                            if (value.Length > _mobileMoney.Length)
                            {
                                value += "0";
                            }
                            else
                            {
                                value = value.Remove(value.Length - 1, 1);
                            }
                        }
                    }
                    else
                    {
                        value = "0";
                    }

                    _mobileMoney = value;
                    OnPropertyChange(nameof(MobileMoney));

                    if (Amount >= SaleViewModel.TotalAmount)
                    {
                        AmountBorderBrush = Brushes.Transparent;
                        IsEnablePay = true;
                        Focus = FocusEnumeration.Pay;
                    }
                }
                catch
                {
                    MessageBox.Show("Polje 'Instant plaćanje' mora biti broj!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        #endregion Properties

        #region Internal Properties
        internal IDbContextFactory<SqlServerDbContext> DbContext
        {
            get; private set;
        }
        internal List<Payment> Payment { get; set; }
        internal SaleViewModel SaleViewModel { get; set; }
        #endregion Internal Properties

        #region Commands
        public ICommand CancelCommand => new CancelCommand(this);
        public ICommand ClickOnNumberButtonCommand => new ClickOnNumberButtonCommand(this);
        public ICommand PayCommand => _payCommand.Value;
        public ICommand SplitOrderCommand => new SplitOrderCommand(this, _serviceProvider);
        public ICommand ChangeFocusCommand => new ChangeFocusCommand(this);
        #endregion Commands

        #region Public methods
        #endregion Public methods

        #region Private methods
        #endregion Private methods
    }
}