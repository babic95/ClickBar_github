using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_Common.Models.Order
{
    public class ItemOrder
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public decimal Quantity { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Zelja { get; set; }
    }
}
