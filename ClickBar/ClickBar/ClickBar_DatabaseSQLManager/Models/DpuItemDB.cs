using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_DatabaseSQLManager.Models
{
    public partial class DpuItemDB
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public decimal Tax { get; set; }
        public string JM { get; set; }
        public decimal PrenetaKolicina { get; set; }
        public decimal NabavljenaKolicina { get; set; }
        public decimal Ukupno { get; set; }
        public decimal ZaliheNaKrajuDana { get; set; }
        public decimal UtrosenaKolicinaTokomDana { get; set; }
        public decimal ProdajnaCnenaPoJM { get; set; }
        public decimal? OstvarenPrometOdUslugaOdPica { get; set; }
        public decimal? OstvarenPrometOdUslugaOdJela { get; set; }
        public decimal ProdajnaVrednostJelaPicaZaKonzumacijuNaLicuMesta { get; set; }
        public virtual DpuDB Dpu { get; set; } = null!;
    }
}
