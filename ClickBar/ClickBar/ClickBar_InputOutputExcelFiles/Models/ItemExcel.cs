using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_InputOutputExcelFiles.Models
{
    internal class ItemExcel
    {
        public string Šifra { get; set; }
        public string Naziv { get; set; }
        public int Grupa { get; set; }
        public string JM { get; set; }
        public decimal Cena { get; set; }
        public string Oznaka { get; set; }
        public decimal Stanje { get; set; }
        public decimal Alarm { get; set; }
    }
}
