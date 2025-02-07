using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_Report.Models
{
    public class ReportPerItems
    {
        public string ItemId { get; set; }
        public string Name { get; set; }
        public string JM { get; set; }
        public decimal MPC_Average { get; set; }
        public decimal MPC_Original { get; set; }
        public decimal Quantity { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Nivelacija { get; set; }
        public bool IsSirovina { get; set; }
    }
}
