using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_DatabaseSQLManager.Models
{
    public class OrderTodayItemDB
    {
        public string Id { get; set; } = null!;
        public string OrderTodayId { get; set; } = null!;
        public string ItemId { get; set; } = null!;
        public decimal Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public string? Zelja { get; set; }
        public decimal StornoQuantity { get; set; }
        public decimal NaplacenoQuantity { get; set; }

        public virtual OrderTodayDB OrderToday { get; set; } = null!;
        public virtual ItemDB Item { get; set; } = null!;
    }
}
