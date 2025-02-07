using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_Database_Drlja.Models
{
    public class NarudzbeDB
    {
        public string Id { get; set; }
        public string TR_DATUM { get; set; }
        public string TR_RADNIK { get; set; } = null!; 
        public int TR_RBS { get; set; }
        public string TR_STO { get; set; } = null!;
        public DateTime TR_VREMENARUDZBE { get; set; }
        public int TR_BROJNARUDZBE { get; set; }
        public int TR_FAZA { get; set; }
        public string? TR_NARUDZBE_ID { get; set; }
        public string? TR_SMENA { get; set; }
        public string? TR_STORNORAZLOG { get; set; }
    }
}
