using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_Printer.Models.Otpremnice
{
    public class OtpremnicaPrint
    {
        public decimal TotalAmount { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime SdcDateTime { get; set; }
        public string? BuyerId { get; set; }
        public string? BuyerName { get; set; }
        public List<OtpremnicaItemPrint> Items { get; set; }
    }
}
