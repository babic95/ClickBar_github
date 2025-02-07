using System;
using System.Collections.Generic;

namespace ClickBar_Database.Models
{
    public partial class ItemInvoiceDB
    {
        public int Id { get; set; }
        public string InvoiceId { get; set; } = null!;
        public string? ItemCode { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? TotalAmout { get; set; }
        public string? Name { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? OriginalUnitPrice { get; set; }
        public decimal? InputUnitPrice { get; set; }
        public string? Label { get; set; }
        public int? IsSirovina { get; set; }

        public virtual InvoiceDB Invoice { get; set; } = null!;
    }
}
