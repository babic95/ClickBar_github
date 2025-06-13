using ClickBar_DatabaseSQLManager.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar.Models.Sale
{
    public class ItemInvoice : ObservableObject
    {
        private Item _item;
        private decimal _quantity;
        private decimal _totalAmout;
        private decimal? _inputUnitPrice;
        private string _totalAmoutString;
        private bool _isSirovina;
        private ObservableCollection<Zelja> _zelje;
        private string? _globalZelja;

        public ItemInvoice(Item item, decimal quantity, decimal popust = 0)
        {
            item.SellingUnitPrice = item.SellingUnitPrice * ((100 - popust) / 100);
            Item = item;
            Quantity = quantity;
            TotalAmout = Decimal.Round(item.SellingUnitPrice * quantity, 2);
        }
        public ItemInvoice(Item item, ItemInvoiceDB itemInvoiceDB)
        {
            if (itemInvoiceDB.Quantity.HasValue &&
                itemInvoiceDB.TotalAmout.HasValue)
            {
                Item = item;
                //Item.SellingUnitPrice = itemInvoiceDB.TotalAmout.Value / itemInvoiceDB.Quantity.Value;
                Item.SellingUnitPrice = itemInvoiceDB.UnitPrice.HasValue ? itemInvoiceDB.UnitPrice.Value : Decimal.Round(itemInvoiceDB.TotalAmout.Value / itemInvoiceDB.Quantity.Value, 2);
                Item.OriginalUnitPrice = itemInvoiceDB.OriginalUnitPrice.HasValue ? itemInvoiceDB.OriginalUnitPrice.Value : Decimal.Round(itemInvoiceDB.TotalAmout.Value / itemInvoiceDB.Quantity.Value, 2);
                Quantity = itemInvoiceDB.Quantity.Value;
                TotalAmout = itemInvoiceDB.TotalAmout.Value;
                InputUnitPrice = itemInvoiceDB.InputUnitPrice;

                var stopaPDV = item.Label == "Ђ" || item.Label == "6" ? 20 :
                item.Label == "Е" || item.Label == "7" ? 10 :
                item.Label == "А" || item.Label == "1" ? 0 :
                item.Label == "Г" || item.Label == "4" ? 0 :
                item.Label == "Ж" || item.Label == "8" ? 19 :
                item.Label == "A" || item.Label == "31" ? 9 : 0;

                Item.Label = $"{stopaPDV}%"; 

                IsSirovina = itemInvoiceDB.IsSirovina == null ? false : itemInvoiceDB.IsSirovina.Value == 1 ? true : false;
            }
        }
        public string? GlobalZelja
        {
            get { return _globalZelja; }
            set
            {
                _globalZelja = value;
                OnPropertyChange(nameof(GlobalZelja));
            }
        }
        public bool IsSirovina
        {
            get { return _isSirovina; }
            set
            {
                _isSirovina = value;
                OnPropertyChange(nameof(IsSirovina));
            }
        }
        public Item Item
        {
            get { return _item; }
            set
            {
                _item = value;
                OnPropertyChange(nameof(Item));
            }
        }
        public decimal Quantity
        {
            get { return _quantity; }
            set
            {
                _quantity = Decimal.Round(value, 4);
                OnPropertyChange(nameof(Quantity));
            }
        }
        public decimal TotalAmout
        {
            get { return _totalAmout; }
            set
            {
                _totalAmout = Decimal.Round(value, 2);
                OnPropertyChange(nameof(TotalAmout));

                TotalAmoutString = string.Format("{0:#,##0.00}", _totalAmout).Replace(',', '#').Replace('.', ',').Replace('#', '.');
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
                    OnPropertyChange(nameof(InputUnitPrice));
                }
            }
        }
        public string TotalAmoutString
        {
            get { return _totalAmoutString; }
            set
            {
                _totalAmoutString = value;
                OnPropertyChange(nameof(TotalAmoutString));
            }
        }
        public ObservableCollection<Zelja> Zelje
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
