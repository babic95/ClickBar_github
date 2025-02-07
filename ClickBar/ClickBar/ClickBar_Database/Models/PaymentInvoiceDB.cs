using ClickBar_Common.Enums;
using System;
using System.Collections.Generic;

namespace ClickBar_Database.Models
{
    public partial class PaymentInvoiceDB
    {
        public PaymentTypeEnumeration PaymentType { get; set; }
        public string InvoiceId { get; set; } = null!;
        public decimal? Amout { get; set; }

        public virtual InvoiceDB Invoice { get; set; } = null!;
    }
}
