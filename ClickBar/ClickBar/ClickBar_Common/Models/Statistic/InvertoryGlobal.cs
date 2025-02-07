using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_Common.Models.Statistic
{
    public class InvertoryGlobal
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Jm { get; set; }
        public decimal Quantity { get; set; }
        public decimal SellingUnitPrice { get; set; }
        public decimal InputUnitPrice { get; set; }
        public decimal TotalAmout { get; set; }
    }
}
