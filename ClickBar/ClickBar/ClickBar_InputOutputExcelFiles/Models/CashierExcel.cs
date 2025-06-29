using ClickBar_Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_InputOutputExcelFiles.Models
{
    internal class CashierExcel
    {
        public string Šifra { get; set; }
        public string Ime { get; set; }
        public string Jmbg { get; set; }
        public string Grad { get; set; }
        public string Adresa { get; set; }
        public string Telefon { get; set; }
        public string Email { get; set; }
        public CashierTypeEnumeration Pozicija_Radnika { get; set; }
        public string? Broj_Kartice_Za_Prijavu { get; set; }
    }
}
