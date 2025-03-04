using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_Report.Models
{
    public class ReportTax
    {
        public decimal Net { get; set; }
        public decimal Pdv { get; set; }
        public decimal Gross { get; set; }
        public decimal Rate { get; set; }
    }
}
