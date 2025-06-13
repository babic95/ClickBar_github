using ClickBar.Enums.AppMain.Statistic;
using ClickBar_DatabaseSQLManager.Models;
using System;

namespace ClickBar.Models.AppMain.Statistic.Porudzbine
{
    public class OrderToday : ObservableObject
    {
        private string _id;
        private int _tableId;
        private string _cashierId;
        private string _cashierName;
        private DateTime _orderDateTime;
        private string _name;
        private decimal _totalPrice;
        private OrdersTodayEnumeration _status;

        public OrderToday() { }
        public OrderToday(OrderTodayDB orderTodayDB) 
        {
            Id = orderTodayDB.Id;
            TableId = orderTodayDB.TableId.Value;
            CashierId = orderTodayDB.CashierId;
            CashierName = orderTodayDB.Cashier?.Name ?? string.Empty;
            OrderDateTime = orderTodayDB.OrderDateTime;
            Name = orderTodayDB.Name;
            TotalPrice = orderTodayDB.TotalPrice;
            Status = orderTodayDB.Faza == 4 ? OrdersTodayEnumeration.Naplaćeno :
                     orderTodayDB.Faza == 5 ? OrdersTodayEnumeration.Obrisano :
                     OrdersTodayEnumeration.Neodređeno;
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
        public int TableId {
            get { return _tableId; }
            set
            {
                _tableId = value;
                OnPropertyChange(nameof(TableId));
            }
        }
        public string CashierId
        {
            get { return _cashierId; }
            set
            {
                _cashierId = value;
                OnPropertyChange(nameof(CashierId));
            }
        }
        public string CashierName
        {
            get { return _cashierName; }
            set
            {
                _cashierName = value;
                OnPropertyChange(nameof(CashierName));
            }
        }
        public DateTime OrderDateTime
        {
            get { return _orderDateTime; }
            set
            {
                _orderDateTime = value;
                OnPropertyChange(nameof(OrderDateTime));
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
        public decimal TotalPrice
        {
            get { return _totalPrice; }
            set
            {
                _totalPrice = value;
                OnPropertyChange(nameof(TotalPrice));
            }
        }
        public OrdersTodayEnumeration Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnPropertyChange(nameof(Status));
            }
        }
    }
}
