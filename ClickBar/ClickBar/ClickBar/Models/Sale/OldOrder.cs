using ClickBar_Database.Models;
using ClickBar_DatabaseSQLManager.Models;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar.Models.Sale
{
    public class OldOrder : ObservableObject
    {
        private DateTime _orderDateTime;
        private string _cashierName;
        private string _cashierId;
        private string _name;
        private ObservableCollection<ItemInvoice> _items;

        public OldOrder(DateTime orderDateTime,
            string cashierName,
            string cashierId,
            string name,
            ObservableCollection<ItemInvoice> items)
        {
            OrderDateTime = orderDateTime;
            CashierName = cashierName;
            CashierId = cashierId;
            Name = name;
            Items = items;
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
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChange(nameof(Name));
            }
        }
        public ObservableCollection<ItemInvoice> Items
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
