using ClickBar.Enums.Sale.Buyer;
using ClickBar.Models.Sale.Buyer;
using ClickBar_Common.Models.Invoice.Helpers;
using ClickBar_Common.Models.Invoice;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ClickBar.Commands.Sale.Pay;
using System.Windows.Input;
using ClickBar.Commands.AppMain.Statistic.Refaund;
using System.Windows.Media;
using ClickBar_Common.Enums;
using ClickBar_DatabaseSQLManager;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ClickBar.ViewModels.AppMain.Statistic
{
    public class PayRefaundViewModel : ViewModelBase
    {
        #region Fields
        private ObservableCollection<BuyerIdElement> _buyerIdElements;
        private BuyerIdElement _currentBuyerIdElement;

        private string _buyerId;

        private decimal _amount;
        private decimal _rest;
        private Brush _amountBorderBrush;
        private bool _isEnableRefaund;

        private string _cash;
        private string _card;
        private string _wireTransfer;

        private string _journal;

        private Visibility _isRefundationCash;

        private readonly IServiceProvider _serviceProvider; // Dodato za korišćenje IServiceProvider
        #endregion Fields

        #region Constructors
        public PayRefaundViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            RefaundCommand = _serviceProvider.GetRequiredService<RefaundCommand>();
        }

        #endregion Constructors

        #region Properties Internal
        internal SqlServerDbContext DbContext
        {
            get; private set;
        }
        #endregion Properties Internal

        #region Properties
        public List<Payment> Payment { get; set; }
        public RefaundViewModel RefaundViewModel { get; set; }
        public Window Window { get; set; }

        public decimal Amount
        {
            get { return _amount; }
            set
            {
                _amount = value;
                OnPropertyChange(nameof(Amount));

                if (value == RefaundViewModel.CurrentInvoice.TotalAmount)
                {
                    AmountBorderBrush = Brushes.Transparent;
                    IsEnableRefaund = true;
                }
                else
                {
                    AmountBorderBrush = Brushes.Red;
                    IsEnableRefaund = false;
                }
                Rest = RefaundViewModel.CurrentInvoice.TotalAmount - value;
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
        public Brush AmountBorderBrush
        {
            get { return _amountBorderBrush; }
            set
            {
                _amountBorderBrush = value;
                OnPropertyChange(nameof(AmountBorderBrush));
            }
        }
        public bool IsEnableRefaund
        {
            get { return _isEnableRefaund; }
            set
            {
                if (value)
                {
                    if (Rest != 0)
                    {
                        value = false;
                    }
                    if (string.IsNullOrEmpty(BuyerId))
                    {
                        value = false;
                    }
                    else
                    {
                        if (BuyerId.Length < 9)
                        {
                            value = false;
                        }
                    }
                }
                _isEnableRefaund = value;
                OnPropertyChange(nameof(IsEnableRefaund));
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
        public string BuyerId
        {
            get { return _buyerId; }
            set
            {
                _buyerId = value;
                OnPropertyChange(nameof(BuyerId));

                if (value.Length > 8)
                {
                    IsEnableRefaund = true;
                }
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

                    Amount = Convert.ToDecimal(value) + Convert.ToDecimal(Card) + Convert.ToDecimal(WireTransfer);
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

                    Amount = Convert.ToDecimal(Cash) + Convert.ToDecimal(value) + Convert.ToDecimal(WireTransfer);
                }
                catch
                {
                    MessageBox.Show("Polje 'Platna kartica' mora biti broj!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
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

                    Amount = Convert.ToDecimal(Cash) + Convert.ToDecimal(Card) + Convert.ToDecimal(value);
                }
                catch
                {
                    MessageBox.Show("Polje 'Prenos na račun' mora biti broj!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        public string Journal
        {
            get { return _journal; }
            set
            {
                _journal = value;
                OnPropertyChange(nameof(Journal));
            }
        }
        #endregion Properties

        #region Commands
        public ICommand RefaundCommand { get; }
        #endregion Commands

        #region Public methods
        public void Initialize(SqlServerDbContext dbContext, Window window, RefaundViewModel refaundViewModel)
        {
            DbContext = dbContext;
            Window = window;
            RefaundViewModel = refaundViewModel;

            Payment = new List<Payment>();

            Amount = 0;

            BuyerIdElements = new ObservableCollection<BuyerIdElement>();
            var buyerIdElements = Enum.GetValues(typeof(BuyerIdElementEnumeration));

            foreach (var buyerIdElement in buyerIdElements)
            {
                BuyerIdElements.Add(new BuyerIdElement((BuyerIdElementEnumeration)buyerIdElement));
            }
            CurrentBuyerIdElement = BuyerIdElements.FirstOrDefault();

            AmountBorderBrush = Brushes.Red;

            Cash = "0";
            Card = "0";
            WireTransfer = "0";

            RefaundViewModel.CurrentInvoiceRequest.Payment.ToList().ForEach(pay =>
            {
                if (pay != null)
                {
                    switch (pay.PaymentType)
                    {
                        case PaymentTypeEnumeration.Cash:
                            Cash = pay.Amount.ToString().Replace(',', '.');
                            break;
                        case PaymentTypeEnumeration.Card:
                            Card = pay.Amount.ToString().Replace(',', '.');
                            break;
                        case PaymentTypeEnumeration.WireTransfer:
                            WireTransfer = pay.Amount.ToString().Replace(',', '.');
                            break;
                    }
                }
            });

            if (!string.IsNullOrEmpty(RefaundViewModel.CurrentInvoiceRequest.BuyerId))
            {
                BuyerId = RefaundViewModel.CurrentInvoiceRequest.BuyerId;
            }

            InvoiceRequest invoiceRequest = new InvoiceRequest()
            {
                Cashier = RefaundViewModel.CurrentInvoiceRequest.Cashier,
                TransactionType = RefaundViewModel.CurrentInvoiceRequest.TransactionType,
                InvoiceType = RefaundViewModel.CurrentInvoiceRequest.InvoiceType,
                BuyerId = RefaundViewModel.CurrentInvoiceRequest.BuyerId,
                BuyerName = RefaundViewModel.CurrentInvoiceRequest.BuyerName,
                BuyerAddress = RefaundViewModel.CurrentInvoiceRequest.BuyerAddress,
                Payment = RefaundViewModel.CurrentInvoiceRequest.Payment,
                ReferentDocumentDT = RefaundViewModel.CurrentInvoiceRequest.ReferentDocumentDT,
                ReferentDocumentNumber = RefaundViewModel.CurrentInvoiceRequest.ReferentDocumentNumber,
            };
            List<Item> items = new List<Item>();
            RefaundViewModel.CurrentInvoiceRequest.Items.ToList().ForEach(item =>
            {
                items.Add(new Item()
                {
                    ItemCode = item.Id,
                    Jm = item.Jm,
                    Name = item.Name,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalAmount = item.TotalAmount,
                    Labels = new List<string>() { item.Label }
                });
            });

            invoiceRequest.Items = items;
            Journal = JournalHelper.CreateJournal(invoiceRequest, RefaundViewModel.CurrentInvoiceResult);
        }
        #endregion Public methods

        #region Private methods
        #endregion Private methods
    }
}