using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_DatabaseSQLManager.Models
{
    public partial class OtpisItemDB
    {
        public string OtpisId { get; set; } = null!;
        public string ItemId { get; set; } = null!;
        public decimal Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public string? Description { get; set; }

        public virtual OtpisDB Otpis { get; set; } = null!;
        public virtual ItemDB Item { get; set; } = null!;
    }
}
