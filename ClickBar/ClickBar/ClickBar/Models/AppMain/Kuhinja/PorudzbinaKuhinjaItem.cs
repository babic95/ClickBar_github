using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar.Models.AppMain.Kuhinja
{
    public class PorudzbinaKuhinjaItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Jm { get; set; }
        public decimal Quantity { get; set; }
        public decimal Total { get; set; }
        public decimal UnitPrice { get; set; }
        public string? Zelja { get; set; }
    }
}
