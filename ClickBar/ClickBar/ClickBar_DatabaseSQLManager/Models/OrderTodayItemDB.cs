using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_DatabaseSQLManager.Models
{
    public class OrderTodayItemDB
    {
        public string OrderTodayId { get; set; } = null!;
        public string ItemId { get; set; } = null!;
        public decimal Quantity { get; set; }
        public decimal TotalPrice { get; set; }

        public virtual OrderTodayDB OrderToday { get; set; } = null!;
        public virtual ItemDB Item { get; set; } = null!;
    }
}
