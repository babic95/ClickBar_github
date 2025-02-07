using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_Printer.Models.DrljaKuhinja
{
    public class PorudzbinaPrint
    {
        public int PorudzbinaNumber { get; set; }
        public string PorudzbinaDateTime { get; set; }
        public string Worker { get; set; }
        public string Sto { get; set; }
        public List<PorudzbinaItemPrint> Items { get; set; }
    }
}
