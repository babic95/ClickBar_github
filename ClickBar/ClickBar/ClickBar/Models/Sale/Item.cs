using ClickBar.Models.AppMain.Statistic;
using ClickBar_DatabaseSQLManager.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar.Models.Sale
{
    public class Item : ObservableObject
    {
        private string _id;
        private string _name;
        private decimal _sellingNocnaUnitPrice;
        private decimal _sellingDnevnaUnitPrice; 
        private decimal _sellingUnitPrice;
        private decimal? _inputUnitPrice;
        private decimal _originalUnitPrice;
        private decimal _quantity;
        private string _label;
        private string _jm;
        private bool _isCheckedDesableItem;
        private bool _isCheckedZabraniPopust;
        private int _rb;
        private ObservableCollection<ItemZelja> _zelje;
        private int? _normId;

        public Item() { }
        public Item(ItemDB itemDB)
        {
            Id = itemDB.Id;
            NormId = itemDB.IdNorm;
            Rb = itemDB.Rb;
            Name = itemDB.Name;
            InputUnitPrice = itemDB.InputUnitPrice;
            SellingUnitPrice = itemDB.SellingUnitPrice;
            SellingNocnaUnitPrice = itemDB.SellingNocnaUnitPrice;
            SellingDnevnaUnitPrice = itemDB.SellingDnevnaUnitPrice;
            OriginalUnitPrice = itemDB.SellingUnitPrice;
            Label = itemDB.Label;
            Jm = itemDB.Jm;
            Quantity = itemDB.TotalQuantity;
            IsCheckedDesableItem = itemDB.DisableItem == 0 ? false : true;
            IsCheckedZabraniPopust = itemDB.IsCheckedZabraniPopust == 0 ? false : true;

            Zelje = new ObservableCollection<ItemZelja>();
            if (itemDB.Zelje != null &&
                itemDB.Zelje.Any())
            {
                foreach(var zeljaDB in itemDB.Zelje)
                {
                    Zelje.Add(new ItemZelja()
                    {
                        Id = zeljaDB.Id,
                        ItemId = zeljaDB.ItemId,
                        Zelja = zeljaDB.Zelja
                    });
                }
            }
        }
        public int? NormId
        {
            get { return _normId; }
            set
            {
                _normId = value;
                OnPropertyChange(nameof(NormId));
            }
        }
        public string Id
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChange(nameof(Id));
            }
        }
        public int Rb
        {
            get { return _rb; }
            set
            {
                _rb = value;
                OnPropertyChange(nameof(Rb));
            }
        }
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChange(nameof(Name));
            }
        }
        public string Jm
        {
            get { return _jm; }
            set
            {
                _jm = value;
                OnPropertyChange(nameof(Jm));
            }
        }
        public decimal SellingUnitPrice
        {
            get { return _sellingUnitPrice; }
            set
            {
                _sellingUnitPrice = Decimal.Round(value, 2);
                OnPropertyChange(nameof(SellingUnitPrice));
            }
        }
        public decimal SellingNocnaUnitPrice
        {
            get { return _sellingNocnaUnitPrice; }
            set
            {
                _sellingNocnaUnitPrice = Decimal.Round(value, 2);
                OnPropertyChange(nameof(SellingNocnaUnitPrice));
            }
        }
        public decimal SellingDnevnaUnitPrice
        {
            get { return _sellingDnevnaUnitPrice; }
            set
            {
                _sellingDnevnaUnitPrice = Decimal.Round(value, 2);
                OnPropertyChange(nameof(SellingDnevnaUnitPrice));
            }
        }
        public decimal? InputUnitPrice
        {
            get { return _inputUnitPrice; }
            set
            {
                if (value != null &&
                    value.HasValue)
                {
                    _inputUnitPrice = Decimal.Round(value.Value, 2);
                }
                else
                {
                    _inputUnitPrice = null;
                }
                OnPropertyChange(nameof(InputUnitPrice));
            }
        }
        public decimal OriginalUnitPrice
        {
            get { return _originalUnitPrice; }
            set
            {
                _originalUnitPrice = Decimal.Round(value, 2);
                OnPropertyChange(nameof(OriginalUnitPrice));
            }
        }
        public decimal Quantity
        {
            get { return _quantity; }
            set
            {
                _quantity = Decimal.Round(value, 3);
                OnPropertyChange(nameof(Quantity));
            }
        }
        public string Label
        {
            get { return _label; }
            set
            {
                _label = value;
                OnPropertyChange(nameof(Label));
            }
        }
        public bool IsCheckedDesableItem
        {
            get { return _isCheckedDesableItem; }
            set
            {
                _isCheckedDesableItem = value;
                OnPropertyChange(nameof(IsCheckedDesableItem));
            }
        }
        public bool IsCheckedZabraniPopust
        {
            get { return _isCheckedZabraniPopust; }
            set
            {
                _isCheckedZabraniPopust = value;
                OnPropertyChange(nameof(IsCheckedZabraniPopust));
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
    }
}
