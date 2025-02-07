using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_Common.Models.Invoice.Tax
{
    public class TaxRate
    {
        /// <summary>
        /// Label for a tax rate, unique within a tax group, always belongs to one tax category
        /// </summary>
        public string Label { get; set; }
        /// <summary>
        /// Rate percentage for a proportional tax, or tax amount for a fixed tax.
        /// </summary>
        public decimal Rate { get; set; }
    }
}
