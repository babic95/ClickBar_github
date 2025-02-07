using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_Database_Drlja.Models
{
    public class ArtikliDB
    {
        public string TR_BRART { get; set; } = null!;
        public string TR_NAZIV { get; set; } = null!;
        public string? TR_MEMO { get; set; }
        public string TR_PAK { get; set; } = null!;
        public string? TR_VRSTA { get; set; }
        public float TR_MPC { get; set; }
        public string? TR_PRO { get; set; }
        public string? TR_VRA { get; set; }
        public int TR_KATEGORIJA { get; set; }
        public string? TR_GLAVNI { get; set; }
        public int TR_ART { get; set; }
    }
}
