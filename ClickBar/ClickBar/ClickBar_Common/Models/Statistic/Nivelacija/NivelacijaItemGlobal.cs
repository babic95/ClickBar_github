using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_Common.Models.Statistic.Nivelacija
{
    public class NivelacijaItemGlobal
    {
        public decimal Marza { get; set; }
        public decimal NewTotalValue { get; set; }
        public decimal OldTotalValue { get; set; }
        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
        public decimal Quantity { get; set; }
        public decimal StopaPDV { get; set; }
        public decimal NewTotalPDV { get; set; }
        public decimal OldTotalPDV { get; set; }
        public string Name { get; set; }
        public string Jm { get; set; }
        public string IdItem { get; set; }
    }
}
