using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_Report.Models.Kuhinja
{
    public class KuhinjaReportItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public decimal Total { get; set; }
    }
}
