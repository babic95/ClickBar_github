﻿using ClickBar.Commands.AppMain.Statistic;
using ClickBar.Commands.AppMain.Statistic.LagerLista;
using ClickBar.Models.AppMain.Statistic;
using ClickBar.Models.AppMain.Statistic.DPU;
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
using System.Windows.Input;

namespace ClickBar.ViewModels.AppMain.Statistic
{
    public class LagerListaViewModel : ViewModelBase
    {
        #region Fields
        private IServiceProvider _serviceProvider;

        private ObservableCollection<Invertory> _allItems;

        private DateTime _selectedDate;

        private string _searchText;
        private ObservableCollection<GroupItems> _allGroups;
        private GroupItems _currentGroup;

        private decimal _totalLagerLista;
        private decimal _totalUlazLagerLista;
        #endregion Fields

        #region Constructors
        public LagerListaViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            DbContext = _serviceProvider.GetRequiredService<IDbContextFactory<SqlServerDbContext>>().CreateDbContext();
            Initialize();
        }
        #endregion Constructors

        #region Properties internal
        internal SqlServerDbContext DbContext
        {
            get; private set;
        }
        internal List<Invertory> Items { get; set; }
        #endregion Properties internal

        #region Properties
        public ObservableCollection<Invertory> AllItems
        {
            get { return _allItems; }
            set
            {
                _allItems = value;
                OnPropertyChange(nameof(AllItems));
            }
        }
        public decimal TotalLagerLista
        {
            get { return _totalLagerLista; }
            set
            {
                _totalLagerLista = value;
                OnPropertyChange(nameof(TotalLagerLista));
            }
        }
        public decimal TotalUlazLagerLista
        {
            get { return _totalUlazLagerLista; }
            set
            {
                _totalUlazLagerLista = value;
                OnPropertyChange(nameof(TotalUlazLagerLista));
            }
        }
        public DateTime SelectedDate
        {
            get { return _selectedDate; }
            set
            {
                _selectedDate = value;
                OnPropertyChange(nameof(SelectedDate));

                if (value != null)
                {
                    ChangeDate();
                }
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
                    AllItems = new ObservableCollection<Invertory>(Items);
                }
                else
                {
                    AllItems = new ObservableCollection<Invertory>(Items.Where(item => item.Item.Name.ToLower().Contains(value.ToLower())));
                }
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

                if (value.Id == -1)
                {
                    AllItems = new ObservableCollection<Invertory>(Items);
                }
                else
                {
                    AllItems = new ObservableCollection<Invertory>(Items.Where(inventory => inventory.IdGroupItems == value.Id));
                }
            }
        }
        #endregion Properties

        #region Commands
        public ICommand PrintLagerListaCommand => new PrintLagerListaCommand(this);
        #endregion Commands

        #region Public methods
        #endregion Public methods

        #region Internal methods
        #endregion Internal methods

        #region Private methods
        private async void Initialize()
        {
            AllGroups = new ObservableCollection<GroupItems>() { new GroupItems()
            {
                Id = -1,
                IdSupergroup = -1,
                Name = "Sve grupe"
            } };

            if (DbContext.ItemGroups != null &&
                DbContext.ItemGroups.Any())
            {
                foreach(var gropu in DbContext.ItemGroups)
                {
                    AllGroups.Add(new GroupItems(gropu));
                }
            }

            SelectedDate = DateTime.Now;
        }
        private async void ChangeDate()
        {
            TotalUlazLagerLista = 0;
            TotalLagerLista = 0;
            Items = new List<Invertory>();

            PocetnoStanjeDB? pocetnoStanjeDB = null;

            if (DbContext.PocetnaStanja != null &&
                DbContext.PocetnaStanja.Any())
            {
                pocetnoStanjeDB = DbContext.PocetnaStanja.Where(p => p.PopisDate.Date < SelectedDate.Date).OrderByDescending(p => p.PopisDate).FirstOrDefault();
            }

            DateTime pocetnoStanjeDate = new DateTime(DateTime.Now.Year, 1, 1, 0, 0, 0);

            if (pocetnoStanjeDB != null)
            {
                if (pocetnoStanjeDB.PopisDate.Date > pocetnoStanjeDate.Date)
                {
                    pocetnoStanjeDate = pocetnoStanjeDB.PopisDate.Date;
                }
            }

            //var allCalculations = DbContext.Calculations.Join(DbContext.CalculationItems,
            //    calculation => calculation.Id,
            //    calculationItem => calculationItem.CalculationId,
            //    (calculation, calculationItem) => new { Calculation = calculation, CalculationItem = calculationItem })
            //    .Where(cal => cal.Calculation.CalculationDate.Date > pocetnoStanjeDate.Date &&
            //    cal.Calculation.CalculationDate.Date <= SelectedDate.Date);

            //var pazar = DbContext.Invoices.Join(DbContext.ItemInvoices,
            //    invoice => invoice.Id,
            //    invoiceItem => invoiceItem.InvoiceId,
            //    (invoice, invoiceItem) => new { Invoice = invoice, InvoiceItem = invoiceItem })
            //    .Where(inv => inv.Invoice.SdcDateTime != null && 
            //    inv.Invoice.SdcDateTime.Value.Date > pocetnoStanjeDate.Date &&
            //    inv.Invoice.SdcDateTime.Value.Date <= SelectedDate.Date);

            var itemsInvoice = DbContext.ItemInvoices.Include(i => i.Invoice).
                Join(DbContext.Items,
                invoiceItem => invoiceItem.ItemCode,
                item => item.Id,
                (invoiceItem, item) => new { InvoiceItem = invoiceItem, Item = item })
                .Where(x => x.InvoiceItem.Invoice.SdcDateTime.HasValue &&
                x.InvoiceItem.Invoice.SdcDateTime.Value.Date >= pocetnoStanjeDate.Date &&
                x.InvoiceItem.Invoice.SdcDateTime.Value.Date <= SelectedDate.Date).ToList();

            var itemsCalculation = DbContext.CalculationItems.Include(i => i.Calculation)
                .Join(DbContext.Items,
                ci => ci.ItemId,
                i => i.Id, 
                (ci, i) => new { CalculationItem = ci, Item = i })
                .Where(x => x.CalculationItem.Calculation.CalculationDate.Date >= pocetnoStanjeDate.Date &&
                x.CalculationItem.Calculation.CalculationDate.Date <= SelectedDate.Date).ToList();

            if (DbContext.Items != null &&
                DbContext.Items.Any())
            {
                foreach(var x in DbContext.Items.Where(i => i.IdNorm == null))
                {
                    if(x.Id == "000045")
                    {
                        int a = 2;
                    }

                    decimal totalQuantityPocetnoStanje = 0;

                    if (pocetnoStanjeDB != null)
                    {
                        var pocetnoStanjeItemDB = DbContext.PocetnaStanjaItems.FirstOrDefault(p => p.IdPocetnoStanje == pocetnoStanjeDB.Id &&
                        p.IdItem == x.Id);

                        if (pocetnoStanjeItemDB != null)
                        {
                            totalQuantityPocetnoStanje = pocetnoStanjeItemDB.NewQuantity;
                        }
                    }

                    Item item = new Item(x);
                    var group = DbContext.ItemGroups.Find(x.IdItemGroup);

                    if (group != null)
                    {
                        decimal quantity = totalQuantityPocetnoStanje;

                        if (x.IdNorm == null)
                        {
                            if (itemsCalculation != null &&
                            itemsCalculation.Any())
                            {
                                var itemsInCal = itemsCalculation.Where(cal => cal.Item.Id == x.Id);

                                if (itemsInCal != null &&
                                itemsInCal.Any())
                                {
                                    foreach(var i in itemsInCal)
                                    {
                                        quantity += i.CalculationItem.Quantity;
                                    }
                                }
                            }

                            if (itemsInvoice != null &&
                            itemsInvoice.Any())
                            {
                                var itemsInPazar = itemsInvoice.Where(paz => paz.InvoiceItem.ItemCode == x.Id);

                                if (itemsInPazar != null &&
                                itemsInPazar.Any())
                                {
                                    foreach(var i in itemsInPazar)
                                    {
                                        if (i.InvoiceItem.Quantity != null)
                                        {
                                            if(i.InvoiceItem.Invoice.TransactionType == 0)
                                            {
                                                quantity -= i.InvoiceItem.Quantity.Value;
                                            }
                                            else
                                            {
                                                quantity += i.InvoiceItem.Quantity.Value;
                                            }
                                        }

                                    }
                                }
                            }
                        }

                        bool isSirovina = group.Name.ToLower().Contains("sirovina") || group.Name.ToLower().Contains("sirovine") ? true : false;

                        var it = new Invertory(item, x.IdItemGroup, quantity, 0, x.AlarmQuantity, isSirovina);
                        Items.Add(it);

                        TotalLagerLista += it.TotalAmout;

                        if (it.Item.InputUnitPrice.HasValue)
                        {
                            TotalUlazLagerLista += Decimal.Round(it.Item.InputUnitPrice.Value * it.Quantity, 2);
                        }
                    }
                }
            }

            CurrentGroup = AllGroups.FirstOrDefault();
            SearchText = string.Empty;
        }
        #endregion Private methods
    }
}