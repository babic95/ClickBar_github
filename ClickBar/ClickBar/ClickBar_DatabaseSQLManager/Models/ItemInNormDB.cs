using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_DatabaseSQLManager.Models
{
    public partial class ItemInNormDB
    {
        public int IdNorm { get; set; }
        public string IdItem { get; set; } = null!;
        public decimal Quantity { get; set; }
        public virtual NormDB Norm { get; set; } = null!;
        public virtual ItemDB Item { get; set; } = null!;
    }
}
