using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_Printer.Models.Otpremnice
{
    public class OtpremnicaItemPrint
    {
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal Quantity { get; set; }
        public string ItemName { get; set; }
    }
}
