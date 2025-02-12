using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_DatabaseSQLManager.Models
{
    public class KepDB
    {
        public string Id { get; set; } = null!;
        public DateTime KepDate { get; set; }
        public int Type { get; set; }
        public string Description { get; set; }
        public decimal Zaduzenje { get; set; }
        public decimal Razduzenje { get; set; }
    }
}
