using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_Printer.Models.DPU
{
    public class DPU_Print
    {
        public int RedniBroj { get; set; }
        public string Naziv { get; set; }
        public decimal PDV { get; set; }
        public string JedinicaMere { get; set; }
        public decimal PrenetaKolicina { get; set; }
        public decimal NabavljenaKolicina { get; set; }
        public decimal Ukupno { get; set; }
        public decimal ZaliheNaKrajuDana { get; set; }
        public decimal UtrosenaKolicina { get; set; }
        public decimal ProdajnaCena { get; set; }
        public decimal PrometOdUsluga { get; set; }
        public decimal PrometOdJela { get; set; }
        public decimal ProdajnaVrednost { get; set; }
    }
}
