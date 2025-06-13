using ClickBar_DatabaseSQLManager.Models;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar.Models.AppMain.Statistic.Porudzbine
{
    public class OrderTodayItem : ObservableObject
    {
        private string _orderTodayId;
        private string _itemId;
        private string _itemName;
        private decimal _quantity;
        private decimal _stornoQuantity;
        private decimal _naplacenoQuantity;
        private decimal _totalPrice;

        public OrderTodayItem() { }
        public OrderTodayItem(OrderTodayItemDB orderTodayItemDB) 
        {
            OrderTodayId = orderTodayItemDB.OrderTodayId;
            ItemId = orderTodayItemDB.ItemId;
            ItemName = orderTodayItemDB.Item?.Name ?? string.Empty; // Assuming Item is a navigation property
            Quantity = orderTodayItemDB.Quantity;
            StornoQuantity = orderTodayItemDB.StornoQuantity;
            NaplacenoQuantity = orderTodayItemDB.NaplacenoQuantity;
            TotalPrice = orderTodayItemDB.TotalPrice;
        }

        public string OrderTodayId
        {
            get { return _orderTodayId; }
            set
            {
                _orderTodayId = value;
                OnPropertyChange(nameof(OrderTodayId));
            }
        }
        public string ItemId
        {
            get { return _itemName; }
            set
            {
                _itemName = value;
                OnPropertyChange(nameof(ItemId));
            }
        }
        public string ItemName
        {
            get { return _itemId; }
            set
            {
                _itemId = value;
                OnPropertyChange(nameof(ItemName));
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
        public decimal StornoQuantity
        {
            get { return _stornoQuantity; }
            set
            {
                _stornoQuantity = value;
                OnPropertyChange(nameof(StornoQuantity));
            }
        }
        public decimal NaplacenoQuantity
        {
            get { return _naplacenoQuantity; }
            set
            {
                _naplacenoQuantity = value;
                OnPropertyChange(nameof(NaplacenoQuantity));
            }
        }
        public decimal TotalPrice
        {
            get { return _totalPrice; }
            set
            {
                _totalPrice = value;
                OnPropertyChange(nameof(TotalPrice));
            }
        }
    }
}
