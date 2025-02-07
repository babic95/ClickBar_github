using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_InputOutputExcelFiles.Models
{
    public class Report
    {
        public string Šifra { get; set; }
        public string Naziv { get; set; }
        public decimal Količina { get; set; }
        public decimal Ukupno { get; set; }
        public string Grupa { get; set; }
    }
}
