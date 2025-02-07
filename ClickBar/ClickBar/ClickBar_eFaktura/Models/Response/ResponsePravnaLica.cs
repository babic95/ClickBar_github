using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_eFaktura.Models.Response
{
    public class ResponsePravnaLica
    {
        public string mb { get; set; }
        public string pib { get; set; }
        public string naziv { get; set; }
        public string mesto { get; set; }
        public string postanskiBroj { get; set; }
        public string adresa { get; set; }
        public string email { get; set; }
    }
}
