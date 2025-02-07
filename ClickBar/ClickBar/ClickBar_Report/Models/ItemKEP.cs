using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_Report.Models
{
    public class ItemKEP
    {
        public string Id { get; set; }
        public DateTime KepDate { get; set; }
        public string Description { get; set; }
        public decimal Zaduzenje { get; set; }
        public decimal Razduzenje { get; set; }
        public decimal Saldo { get; set; }
    }
}
