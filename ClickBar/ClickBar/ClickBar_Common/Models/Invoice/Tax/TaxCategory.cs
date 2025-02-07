using ClickBar_Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_Common.Models.Invoice.Tax
{
    public class TaxCategory
    {
        /// <summary>
        /// Name of a tax (tax category)
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// One of the following tax category types:
        /// 0 (tax-on-net) - all tax rates from this category are proportional, and shall be applied on the net price;
        /// 1 (tax-on-total) - all tax rates from this category are proportional, and shall be applied on the total amount;
        /// 2 (amount-per-quantity) - all tax rates from this category are fixed tax amounts, which shall be multiplied with item quantity.
        /// </summary>
        public CategoryTypeEnumeration CategoryType { get; set; }
        /// <summary>
        /// Order number for a tax category. It uniquely identifies the tax. 
        /// It is related ¬to the category name meaning even if the name changes for the tax category, 
        /// it’s orderId will remain the same, pointing to the same tax. It is crucial for Sign Invoice APDU command
        /// </summary>
        public int OrderId { get; set; }
        /// <summary>
        /// All tax rates for a tax (category)
        /// </summary>
        public IEnumerable<TaxRate> TaxRates { get; set; }
    }
}
