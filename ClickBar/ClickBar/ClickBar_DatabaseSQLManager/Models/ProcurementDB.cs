using System;
using System.Collections.Generic;

namespace ClickBar_DatabaseSQLManager.Models
{
    public partial class ProcurementDB
    {
        public string Id { get; set; } = null!;
        public int SupplierId { get; set; }
        public string ItemId { get; set; } = null!;
        public DateTime DateProcurement { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public virtual ItemDB Item { get; set; } = null!;
        public virtual SupplierDB Supplier { get; set; } = null!;
    }
}
