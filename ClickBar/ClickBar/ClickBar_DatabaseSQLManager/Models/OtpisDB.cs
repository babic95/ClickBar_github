using ClickBar_Common.Models.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_DatabaseSQLManager.Models
{
    public partial class OtpisDB
    {
        public OtpisDB()
        {
            OtpisItems = new HashSet<OtpisItemDB>();
        }

        public string Id { get; set; } = null!;
        public string CashierId { get; set; } = null!;
        public DateTime OtpisDate { get; set; }
        public int Counter { get; set; }
        public string Name { get; set; }

        public virtual ICollection<OtpisItemDB> OtpisItems { get; set; }
        public virtual CashierDB Cashier { get; set; } = null!;
    }
}
