using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_DatabaseSQLManager.Models
{
    public partial class CalculationDB
    {
        public CalculationDB()
        {
            CalculationItems = new HashSet<CalculationItemDB>();
        }

        public string Id { get; set; } = null!;
        public int SupplierId { get; set; }
        public string CashierId { get; set; }
        public DateTime CalculationDate { get; set; }
        public string? InvoiceNumber { get; set; }
        public decimal InputTotalPrice { get; set; }
        public decimal OutputTotalPrice { get; set; }
        public int Counter { get; set; }

        public virtual SupplierDB Supplier { get; set; }
        public virtual CashierDB Cashier { get; set; }
        public virtual ICollection<CalculationItemDB> CalculationItems { get; set; }
    }
}
