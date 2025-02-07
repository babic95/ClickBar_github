using ClickBar.Enums.AppMain.Admin;
using ClickBar.Models.Sale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace ClickBar.Models.TableOverview
{
    public class PaymentPlace : ObservableObject
    {
        private int _id;
        private int _partHallId;
        private Order _order;
        private decimal _left;
        private decimal _top;
        private decimal _width;
        private decimal _height;
        private decimal _diameter;
        private decimal _total;
        private decimal _popust;
        private string _name;
        private Brush _background;
        private PaymentPlaceTypeEnumeration _type;

        public decimal Popust
        {
            get { return _popust; }
            set
            {
                _popust = value;
                OnPropertyChange(nameof(Popust));
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
        public int Id
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChange(nameof(Id));
            }
        }

        public int PartHallId
        {
            get { return _partHallId; }
            set
            {
                _partHallId = value;
                OnPropertyChange(nameof(PartHallId));
            }
        }
        public Order Order
        {
            get { return _order; }
            set
            {
                _order = value;
                OnPropertyChange(nameof(Order));
            }
        }
        public decimal Left
        {
            get { return _left; }
            set
            {
                _left = value;
                OnPropertyChange(nameof(Left));
            }
        }
        public decimal Top
        {
            get { return _top; }
            set
            {
                _top = value;
                OnPropertyChange(nameof(Top));
            }
        }
        public decimal Width
        {
            get { return _width; }
            set
            {
                _width = value;
                OnPropertyChange(nameof(Width));
            }
        }
        public decimal Height
        {
            get { return _height; }
            set
            {
                _height = value;
                OnPropertyChange(nameof(Height));
            }
        }
        public decimal Diameter
        {
            get { return _diameter; }
            set
            {
                _diameter = value;
                OnPropertyChange(nameof(Diameter));
            }
        }
        public decimal Total
        {
            get { return _total; }
            set
            {
                _total = value;
                OnPropertyChange(nameof(Total));
            }
        }
        public Brush Background
        {
            get { return _background; }
            set
            {
                _background = value;
                OnPropertyChange(nameof(Background));
            }
        }
        public PaymentPlaceTypeEnumeration Type
        {
            get { return _type; }
            set
            {
                _type = value;
                OnPropertyChange(nameof(Type));
            }
        }
    }
}
