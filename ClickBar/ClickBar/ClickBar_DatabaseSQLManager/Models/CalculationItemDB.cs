using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_DatabaseSQLManager.Models
{
    public partial class CalculationItemDB
    {
        public string CalculationId { get; set; } = null!;
        public string ItemId { get; set; } = null!;
        public decimal InputPrice { get; set; }
        public decimal OutputPrice { get; set; }
        public decimal Quantity { get; set; }

        public virtual CalculationDB Calculation { get; set; } = null!;
        public virtual ItemDB Item { get; set; } = null!;
    }
}
