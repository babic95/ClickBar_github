using ClickBar_DatabaseSQLManager.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar.Models.Sale
{
    public class Order : ObservableObject
    {
        private int _partHall;
        private int _tableId;
        private CashierDB _cashier;
        private string _cashierName;

        public Order(int tableId, int partHall)
        {
            TableId = tableId;
            PartHall = partHall;
        }
        public Order(CashierDB cashier)
        {
            Cashier = cashier;
        }

        public int PartHall
        {
            get { return _partHall; }
            set
            {
                _partHall = value;
                OnPropertyChange(nameof(PartHall));
            }
        }
        public int TableId
        {
            get { return _tableId; }
            set
            {
                _tableId = value;
                OnPropertyChange(nameof(TableId));
            }
        }
        public CashierDB Cashier
        {
            get { return _cashier; }
            set
            {
                _cashier = value;
                CashierName = value.Name;
                OnPropertyChange(nameof(Cashier));
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
    }
}
