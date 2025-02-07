using System;
using System.Collections.Generic;

namespace ClickBar_Database.Models
{
    public partial class TaxItemInvoiceDB
    {
        public string Label { get; set; } = null!;
        public string InvoiceId { get; set; } = null!;
        public string? CategoryName { get; set; }
        public int? CategoryType { get; set; }
        public decimal? Rate { get; set; }
        public decimal? Amount { get; set; }

        public virtual InvoiceDB Invoice { get; set; } = null!;
    }
}
