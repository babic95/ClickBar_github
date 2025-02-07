using System;
using System.Collections.Generic;

namespace ClickBar_Database.Models
{
    public partial class SmartCardDB
    {
        public string Id { get; set; } = null!;
        public string CashierId { get; set; } = null!;

        public virtual CashierDB Cashier { get; set; } = null!;
    }
}
