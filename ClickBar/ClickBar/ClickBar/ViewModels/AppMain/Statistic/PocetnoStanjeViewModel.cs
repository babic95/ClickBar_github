using ClickBar.Commands.AppMain.Statistic;
using ClickBar.Models.AppMain.Statistic;
using ClickBar.Models.AppMain.Statistic.PocetnaStanja;
using ClickBar_Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ClickBar.Models.Sale;
using ClickBar.Commands.AppMain.Statistic.PocetnoStanje;
using System.ComponentModel;
using System.Windows.Data;

namespace ClickBar.ViewModels.AppMain.Statistic
{
    public class PocetnoStanjeViewModel : ViewModelBase
    {
        #region Fields
        private PocetnoStanje _currentPocetnoStanje;
        private ICollectionView _filteredItems;

        private DateTime _popisDate;

        private string _searchText; 
        #endregion Fields

        #region Constructors
        public PocetnoStanjeViewModel()
        {
            PopisDate = DateTime.Now;
            //PopisDate = new DateTime(2025, 1, 22);
        }
        #endregion Constructors

        #region Properties internal
        #endregion Properties internal

        #region Properties
        public DateTime PopisDate
        {
            get { return _popisDate; }
            set
            {
                _popisDate = value;
                OnPropertyChange(nameof(PopisDate));

                if(value != null)
                {
                    using (SqliteDbContext sqliteDbContext = new SqliteDbContext())
                    {
                        var pocetnoStanjeDB = sqliteDbContext.PocetnaStanja.FirstOrDefault(p => p.PopisDate.Date == value.Date);

                        if (pocetnoStanjeDB != null)
                        {
                            List<PocetnoStanjeItem> items = new List<PocetnoStanjeItem>();

                            sqliteDbContext.Items.Where(i => i.IdNorm == null).ForEachAsync(itemDB =>
                            {
                                var pocetnoStanjeItemDB = sqliteDbContext.PocetnaStanjaItems.FirstOrDefault(p => p.IdPocetnoStanje == pocetnoStanjeDB.Id &&
                                p.IdItem == itemDB.Id);

                                if (pocetnoStanjeItemDB != null)
                                {
                                    Item item = new Item(itemDB);
                                    var group = sqliteDbContext.ItemGroups.Find(itemDB.IdItemGroup);

                                    if (group != null)
                                    {
                                        bool isSirovina = group.Name.ToLower().Contains("sirovina") || group.Name.ToLower().Contains("sirovine") ? true : false;
                                        Invertory invertory = new Invertory(item,
                                            itemDB.IdItemGroup,
                                            pocetnoStanjeItemDB.NewQuantity,
                                            pocetnoStanjeItemDB.InputPrice,
                                            itemDB.AlarmQuantity,
                                            isSirovina);

                                        PocetnoStanjeItem pocetnoStanjeItem = new PocetnoStanjeItem(invertory);

                                        items.Add(pocetnoStanjeItem);
                                    }
                                }
                                else
                                {
                                    Item item = new Item(itemDB);
                                    var group = sqliteDbContext.ItemGroups.Find(itemDB.IdItemGroup);

                                    if (group != null)
                                    {
                                        bool isSirovina = group.Name.ToLower().Contains("sirovina") || group.Name.ToLower().Contains("sirovine") ? true : false;
                                        Invertory invertory = new Invertory(item,
                                            itemDB.IdItemGroup,
                                            itemDB.TotalQuantity,
                                            itemDB.InputUnitPrice != null && itemDB.InputUnitPrice.HasValue ? itemDB.InputUnitPrice.Value : 0,
                                            itemDB.AlarmQuantity,
                                            isSirovina);

                                        PocetnoStanjeItem pocetnoStanjeItem = new PocetnoStanjeItem(invertory);

                                        items.Add(pocetnoStanjeItem);
                                    }
                                }
                            });

                            if(CurrentPocetnoStanje == null)
                            {
                                CurrentPocetnoStanje = new PocetnoStanje(items);
                            }

                            CurrentPocetnoStanje.Id = pocetnoStanjeDB.Id;
                            CurrentPocetnoStanje.Items = new ObservableCollection<PocetnoStanjeItem>(items);
                            FilteredItems = CollectionViewSource.GetDefaultView(CurrentPocetnoStanje.Items);
                            FilteredItems.Filter = FilterItems;
                        }
                        else
                        {
                            List<PocetnoStanjeItem> items = new List<PocetnoStanjeItem>();

                            sqliteDbContext.Items.Where(i => i.IdNorm == null).ForEachAsync(itemDB =>
                            {
                                Item item = new Item(itemDB);
                                var group = sqliteDbContext.ItemGroups.Find(itemDB.IdItemGroup);

                                if (group != null)
                                {
                                    bool isSirovina = group.Name.ToLower().Contains("sirovina") || group.Name.ToLower().Contains("sirovine") ? true : false;
                                    Invertory invertory = new Invertory(item,
                                        itemDB.IdItemGroup,
                                        itemDB.TotalQuantity,
                                        itemDB.InputUnitPrice != null && itemDB.InputUnitPrice.HasValue ? itemDB.InputUnitPrice.Value : 0,
                                        itemDB.AlarmQuantity, 
                                        isSirovina);

                                    PocetnoStanjeItem pocetnoStanjeItem = new PocetnoStanjeItem(invertory);

                                    items.Add(pocetnoStanjeItem);
                                }
                            });

                            CurrentPocetnoStanje = new PocetnoStanje(items);
                            FilteredItems = CollectionViewSource.GetDefaultView(CurrentPocetnoStanje.Items);
                            FilteredItems.Filter = FilterItems;
                        }
                    }

                    CurrentPocetnoStanje.PopisDate = value;
                }
            }
        }

        public PocetnoStanje CurrentPocetnoStanje
        {
            get { return _currentPocetnoStanje; }
            set
            {
                _currentPocetnoStanje = value;
                OnPropertyChange(nameof(CurrentPocetnoStanje));
            }
        }
        public ICollectionView FilteredItems
        {
            get { return _filteredItems; }
            set
            {
                _filteredItems = value;
                OnPropertyChange(nameof(FilteredItems));
            }
        }
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                OnPropertyChange(nameof(SearchText));
                _filteredItems.Refresh();
            }
        }
        #endregion Properties

        #region Commands
        public ICommand CreatePocetnoStanjeCommand => new CreatePocetnoStanjeCommand(this);
        #endregion Commands

        #region Public methods
        #endregion Public methods

        #region Private methods
        private bool FilterItems(object obj)
        {
            if (obj is PocetnoStanjeItem item)
            {
                return string.IsNullOrWhiteSpace(_searchText) ||
                       item.Item.Item.Name.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0;
            }
            return false;
        }
        #endregion Private methods
    }
}