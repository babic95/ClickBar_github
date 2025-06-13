using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_Printer.Models
{
    internal class CCS_Tabela
    {
        public CCS_Tabela(float sirina, float visina)
        {
            Visina = visina;
            Sirina = sirina;
        }
        public float Visina { get; set; }
        public float Sirina { get; set; }
    }
}
