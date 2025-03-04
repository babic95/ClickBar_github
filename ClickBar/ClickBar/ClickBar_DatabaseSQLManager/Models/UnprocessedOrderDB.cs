using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_DatabaseSQLManager.Models
{
    public class UnprocessedOrderDB
    {
        public UnprocessedOrderDB()
        {
            ItemsInUnprocessedOrder = new HashSet<ItemInUnprocessedOrderDB>();
            OrdersToday = new HashSet<OrderTodayDB>();
        }

        public string Id { get; set; } = null!;
        public int PaymentPlaceId { get; set; }
        public string CashierId { get; set; } = null!;
        public decimal TotalAmount { get; set; }

        public virtual CashierDB Cashier { get; set; } = null!;
        public virtual PaymentPlaceDB PaymentPlace { get; set; } = null!;
        public virtual ICollection<ItemInUnprocessedOrderDB> ItemsInUnprocessedOrder { get; set; }
        public virtual ICollection<OrderTodayDB> OrdersToday { get; set; }
        
    }
}
