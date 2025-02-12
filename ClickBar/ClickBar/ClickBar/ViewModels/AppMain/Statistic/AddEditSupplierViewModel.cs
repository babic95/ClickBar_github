using ClickBar.Commands.AppMain.Statistic;
using ClickBar.Models.AppMain.Statistic;
using ClickBar_DatabaseSQLManager;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace ClickBar.ViewModels.AppMain.Statistic
{
    public class AddEditSupplierViewModel : ViewModelBase
    {
        #region Fields
        private Supplier _currentSupplier;
        private ObservableCollection<Supplier> _suppliers;

        private string _searchText;
        private string _title;

        private readonly IServiceProvider _serviceProvider;  // Dodato za korišćenje IServiceProvider
        #endregion Fields

        #region Constructors
        public AddEditSupplierViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            DbContext = _serviceProvider.GetRequiredService<IDbContextFactory<SqlServerDbContext>>().CreateDbContext();
            DbContext.Suppliers.ToList().ForEach(x =>
            {
                SuppliersAll.Add(new Supplier(x));
            });

            Suppliers = new ObservableCollection<Supplier>(SuppliersAll);
            CurrentSupplier = new Supplier();
        }
        #endregion Constructors

        #region Properties internal
        internal SqlServerDbContext DbContext { get; private set; }
        internal List<Supplier> SuppliersAll = new List<Supplier>();
        internal Window Window { get; set; }
        #endregion Properties internal

        #region Properties
        public Supplier CurrentSupplier
        {
            get { return _currentSupplier; }
            set
            {
                _currentSupplier = value;
                OnPropertyChange(nameof(CurrentSupplier));
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

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                OnPropertyChange(nameof(SearchText));

                if (string.IsNullOrEmpty(value))
                {
                    Suppliers = new ObservableCollection<Supplier>(SuppliersAll);
                }
                else
                {
                    Suppliers = new ObservableCollection<Supplier>(SuppliersAll.Where(supplier => supplier.Name.ToLower().Contains(value.ToLower())));
                    if (!Suppliers.Any())
                    {
                        Suppliers = new ObservableCollection<Supplier>(SuppliersAll.Where(supplier => supplier.Pib.Contains(value)));
                    }
                }
            }
        }

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChange(nameof(Title));
            }
        }
        #endregion Properties

        #region Commands
        public ICommand DeleteCommand => new DeleteCommand(this);
        public ICommand EditCommand => new EditCommand(this);
        public ICommand OpenAddEditWindow => new OpenAddEditWindow(this);
        public ICommand SaveCommand => new SaveCommand(this);
        #endregion Commands

        #region Public methods
        #endregion Public methods

        #region Private methods
        #endregion Private methods
    }
}