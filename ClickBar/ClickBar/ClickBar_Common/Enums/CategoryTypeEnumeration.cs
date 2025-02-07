using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_Common.Enums
{
    public enum CategoryTypeEnumeration
    {
        /// <summary>
        /// all tax rates from this category are proportional,
        /// and shall be applied on the net price;
        /// </summary>
        TaxOnNet = 0,
        /// <summary>
        /// all tax rates from this category are proportional, 
        /// and shall be applied on the total amount;
        /// </summary>
        TaxOnTotal = 1,
        /// <summary>
        /// all tax rates from this category are fixed tax amounts, 
        /// which shall be multiplied with item quantity.
        /// </summary>
        AmountPerQuantity = 2
    }
}
