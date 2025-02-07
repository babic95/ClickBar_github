using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_Database.Models
{
    public class OrderTodayDB
    {
        public OrderTodayDB()
        {
            OrderTodayItems = new HashSet<OrderTodayItemDB>();
        }

        public string Id { get; set; } = null!;
        public string CashierId { get; set; } = null!;
        public DateTime OrderDateTime { get; set; }
        public int Counter { get; set; }
        public int CounterType { get; set; }
        public string? Name { get; set; }
        public decimal TotalPrice { get; set; }
        public int? TableId { get; set; }

        public virtual CashierDB Cashier { get; set; } = null!;
        public virtual ICollection<OrderTodayItemDB> OrderTodayItems { get; set; }
    }
}
