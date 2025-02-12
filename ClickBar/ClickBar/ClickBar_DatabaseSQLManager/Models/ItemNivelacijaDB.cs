using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_DatabaseSQLManager.Models
{
    public partial class ItemNivelacijaDB
    {
        public string IdItem { get; set; } = null!;
        public string IdNivelacija { get; set; } = null!;
        public decimal OldUnitPrice { get; set; }
        public decimal NewUnitPrice { get; set; }
        public decimal StopaPDV { get; set; }
        public decimal TotalQuantity { get; set; }
        public virtual NivelacijaDB Nivelacija { get; set; } = null!;
        public virtual ItemDB Item { get; set; } = null!;
    }
}
