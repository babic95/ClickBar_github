using ClickBar_Common.Models.Invoice.FileSystemWatcher;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_Common.Models.Order
{
    public class Order
    {
        public string PartHall { get; set; }
        public int TableId { get; set; }
        public string CashierName { get; set; }
        public List<ItemOrder> Items { get; set; }
        public DateTime OrderTime { get; set; }
        public string OrderName { get; set; }
    }
}
