using ClickBar.Models.Sale;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ClickBar.Models.AppMain.Statistic
{
    public class Invertory : ObservableObject
    {
        private Item _item;
        private int _idGroupItems;
        private decimal _quantity;
        private decimal _inputPrice;
        private decimal _totalAmout;
        private string _colorSet;
        private decimal _alarm;
        private Visibility _visibilityJC;
        private bool _isSirovina;

        public Invertory()
        {
            Item = new Item();
        }
        public Invertory(Item item, 
            int idGroupItems,
            decimal quantity,
            decimal inputPrice,
            decimal alarm,
            bool isSirovina)
        {
            Item = item;
            IdGroupItems = idGroupItems;
            Quantity = quantity;
            InputPrice = inputPrice;
            IsSirovina = isSirovina;
            if (isSirovina)
            {
                VisibilityJC = Visibility.Hidden;
                TotalAmout = quantity * (item.InputUnitPrice != null && item.InputUnitPrice.HasValue ? item.InputUnitPrice.Value : 0);
            }
            else
            {
                VisibilityJC = Visibility.Visible;
                TotalAmout = quantity * item.SellingUnitPrice;
            }
            Alarm = alarm;

            if (quantity <= alarm)
            {
                ColorSet = "Red";
            }
            else
            {
                ColorSet = "Transparent";
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
        public Visibility VisibilityJC
        {
            get { return _visibilityJC; }
            set
            {
                _visibilityJC = value;
                OnPropertyChange(nameof(VisibilityJC));
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
        public int IdGroupItems
        {
            get { return _idGroupItems; }
            set
            {
                _idGroupItems = value;
                OnPropertyChange(nameof(IdGroupItems));
            }
        }
        public decimal Alarm
        {
            get { return _alarm; }
            set
            {
                _alarm = value;
                OnPropertyChange(nameof(Alarm));
            }
        }
        public decimal Quantity
        {
            get { return _quantity; }
            set
            {
                _quantity = value;
                OnPropertyChange(nameof(Quantity));
            }
        }
        public decimal InputPrice
        {
            get { return _inputPrice; }
            set
            {
                _inputPrice = value;
                OnPropertyChange(nameof(InputPrice));
            }
        }
        public decimal TotalAmout
        {
            get { return _totalAmout; }
            set
            {
                _totalAmout = value;
                OnPropertyChange(nameof(TotalAmout));
            }
        }
        public string ColorSet
        {
            get { return _colorSet; }
            set
            {
                _colorSet = value;
                OnPropertyChange(nameof(ColorSet));
            }
        }
    }
}
