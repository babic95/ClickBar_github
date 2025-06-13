using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar.Models.AppMain.Statistic.Items
{
    public class CardForItem : ObservableObject
    {
        private string _id;
        private string _name;
        private string _jm;
        private decimal _totalInputQuantity;
        private decimal _totalOutputQuantity;
        private decimal _totalOtpisQuantity;
        private decimal _totalInputPrice;
        private decimal _totalOutputPrice;
        private decimal _totalOtpisPrice;
        private ObservableCollection<ItemCard> _items;

        public string Id
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChange(nameof(Id));
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
        public decimal TotalInputQuantity
        {
            get { return _totalInputQuantity; }
            set
            {
                _totalInputQuantity = value;
                OnPropertyChange(nameof(TotalInputQuantity));
            }
        }
        public decimal TotalOutputQuantity
        {
            get { return _totalOutputQuantity; }
            set
            {
                _totalOutputQuantity = value;
                OnPropertyChange(nameof(TotalOutputQuantity));
            }
        }
        public decimal TotalOtpisQuantity
        {
            get { return _totalOtpisQuantity; }
            set
            {
                _totalOtpisQuantity = value;
                OnPropertyChange(nameof(TotalOtpisQuantity));
            }
        }
        public decimal TotalInputPrice
        {
            get { return _totalInputPrice; }
            set
            {
                _totalInputPrice = value;
                OnPropertyChange(nameof(TotalInputPrice));
            }
        }
        public decimal TotalOutputPrice
        {
            get { return _totalOutputPrice; }
            set
            {
                _totalOutputPrice = value;
                OnPropertyChange(nameof(TotalOutputPrice));
            }
        }
        public decimal TotalOtpisPrice
        {
            get { return _totalOtpisPrice; }
            set
            {
                _totalOtpisPrice = value;
                OnPropertyChange(nameof(TotalOtpisPrice));
            }
        }
        public string DisplayName
        {
            get { return $"{Name} ({Jm})"; }
        }
        public ObservableCollection<ItemCard> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                OnPropertyChange(nameof(Items));
            }
        }
    }
}
