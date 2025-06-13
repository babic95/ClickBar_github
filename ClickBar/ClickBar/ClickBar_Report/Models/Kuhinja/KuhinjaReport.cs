using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_Report.Models.Kuhinja
{
    public class KuhinjaReport
    {
        public string Name { get; set; }
        public decimal Total { get; set; }
        public List<KuhinjaReportItem> Items { get; set; }
    }
}
