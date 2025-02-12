using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_DatabaseSQLManager.Models
{
    public class PocetnoStanjeItemDB
    {
        public string IdPocetnoStanje { get; set; } = null!;
        public string IdItem { get; set; } = null!;
        public decimal OldQuantity { get; set; }
        public decimal NewQuantity { get; set; }
        public decimal InputPrice { get; set; }
        public decimal OutputPrice { get; set; }

        public virtual PocetnoStanjeDB PocetnoStanje { get; set; } = null!;
        public virtual ItemDB Item { get; set; } = null!;
    }
}
