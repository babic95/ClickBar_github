using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_Database_Drlja.Models
{
    public class StavkeNarudzbeDB
    {
        public string Id { get; set; }
        public decimal TR_KOL { get; set; }
        public decimal TR_KOL_STORNO { get; set; }
        public string TR_BRART { get; set; } = null!; 
        public string TR_PAK { get; set; } = null!;
        public string TR_NAZIV { get; set; } = null!;
        public int TR_RBS { get; set; }
        public int TR_BROJNARUDZBE { get; set; }
        public string? TR_ZELJA { get; set; }
        public decimal TR_MPC { get; set; }
        public string? TR_NARUDZBE_ID { get; set; }
    }
}
