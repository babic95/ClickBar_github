﻿using ClickBar.Commands.Sale;
using ClickBar.Commands.Sale.Pay;
using ClickBar.Commands.Sale.Pay.SplitOrder;
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

namespace ClickBar.ViewModels.Sale
{
    public class SplitOrderViewModel : ViewModelBase
    {
        #region Fields
        private ObservableCollection<ItemInvoice> _itemsInvoice;
        private ObservableCollection<ItemInvoice> _itemsInvoiceForPay;
        private ItemInvoice _selectedItemInvoice;
        private ItemInvoice _selectedItemInvoiceForPay;
        private decimal _totalAmount;
        private decimal _totalAmountForPay;
        private string _quantity;
        private bool _firstChangeQuantity;
        private readonly IServiceProvider _serviceProvider;
        private readonly Lazy<PayCommand<SplitOrderViewModel>> _payCommand;

        private PaySaleViewModel _paySaleViewModel;
        #endregion Fields

        #region Constructors
        public SplitOrderViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            DbContextFactory = _serviceProvider.GetRequiredService<IDbContextFactory<SqlServerDbContext>>();
            ItemsInvoiceForPay = new ObservableCollection<ItemInvoice>();
            _payCommand = new Lazy<PayCommand<SplitOrderViewModel>>(() => new PayCommand<SplitOrderViewModel>(serviceProvider, this));
            //PayCommand = _serviceProvider.GetRequiredService<PayCommand<SplitOrderViewModel>>();
            //ChangePaymentPlaceCommand = _serviceProvider.GetRequiredService<ChangePaymentPlaceCommand>();

            Quantity = "1";
        }
        #endregion Constructors

        #region Internal Properties
        internal IDbContextFactory<SqlServerDbContext> DbContextFactory
        {
            get; private set;
        }
        #endregion Internal Properties

        #region Properties
        public ObservableCollection<ItemInvoice> ItemsInvoice
        {
            get { return _itemsInvoice; }
            set
            {
                _itemsInvoice = value;
                OnPropertyChange(nameof(ItemsInvoice));
            }
        }
        public ObservableCollection<ItemInvoice> ItemsInvoiceForPay
        {
            get { return _itemsInvoiceForPay; }
            set
            {
                _itemsInvoiceForPay = value;
                OnPropertyChange(nameof(ItemsInvoiceForPay));
            }
        }

        public ItemInvoice SelectedItemInvoice
        {
            get { return _selectedItemInvoice; }
            set
            {
                _selectedItemInvoice = value;
                OnPropertyChange(nameof(SelectedItemInvoice));
            }
        }
        public ItemInvoice SelectedItemInvoiceForPay
        {
            get { return _selectedItemInvoiceForPay; }
            set
            {
                _selectedItemInvoiceForPay = value;
                OnPropertyChange(nameof(SelectedItemInvoiceForPay));
            }
        }
        public decimal TotalAmount
        {
            get { return _totalAmount; }
            set
            {
                _totalAmount = value;
                OnPropertyChange(nameof(TotalAmount));
            }
        }
        public decimal TotalAmountForPay
        {
            get { return _totalAmountForPay; }
            set
            {
                _totalAmountForPay = value;
                OnPropertyChange(nameof(TotalAmountForPay));
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
        #endregion Properties

        #region Internal Properties
        internal PaySaleViewModel PaySaleViewModel
        {   get  { return _paySaleViewModel; } 
            set 
            {
                _paySaleViewModel = value;
                if (value != null)
                {
                    if (value.SaleViewModel != null)
                    {
                        if (value.SaleViewModel.ItemsInvoice != null)
                        {
                            ItemsInvoice = value.SaleViewModel.ItemsInvoice;
                        }
                        if (value.SaleViewModel.TotalAmount != 0)
                        {
                            TotalAmount = value.SaleViewModel.TotalAmount;
                        }
                    }
                }
            } 
        }
        internal Window ChangePaymentPlaceWindow { get; set; }
        #endregion Internal Properties

        #region Commands
        public ICommand CancelCommand => new CancelCommand(this);
        public ICommand PayCommand => _payCommand.Value;
        public ICommand StornoKuhinjaCommand => new StornoKuhinjaCommand(this);
        public ICommand ChangePaymentPlaceCommand => new ChangePaymentPlaceCommand(_serviceProvider, this);
        public ICommand MoveToOrderCommand => new MoveToOrderCommand(this);
        public ICommand MoveToPaymentCommand => new MoveToPaymentCommand(this);
        public ICommand MoveAllToPaymentCommand => new MoveAllToPaymentCommand(this);
        public ICommand MoveAllToOrderCommand => new MoveAllToOrderCommand(this); 
        #endregion Commands

        #region Public methods
        #endregion Public methods

        #region Private methods
        #endregion Private methods
    }
}