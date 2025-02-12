using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_DatabaseSQLManager.Models
{
    public class ItemInUnprocessedOrderDB
    {
        public string ItemId { get; set; } = null!;
        public string UnprocessedOrderId { get; set; } = null!;
        public decimal Quantity { get; set; }

        public virtual ItemDB Item { get; set; } = null!;
        public virtual UnprocessedOrderDB UnprocessedOrder { get; set; } = null!;
    }
}
