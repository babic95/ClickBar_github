using ClickBar_Printer.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_Printer.Models.DrljaKuhinja
{
    public class PorudzbinaItemPrint
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public string Zelje { get; set; }
        public OrderTypeEnumeration Type { get; set; }
    }
}
