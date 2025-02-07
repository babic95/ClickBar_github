using ClickBar_Common.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_Common.Models.Invoice
{
    public class Payment
    {
        /// <summary>
        /// Decimal amount of the payment
        /// </summary>
        [JsonProperty("amount")]
        public decimal Amount { get; set; }
        /// <summary>
        /// Payment Type enumeration value: 0 - Other, 1 - Cash, 2 - Card, 3 - Check, 4 - Wire Transfer, 5 - Voucher, 6 - Mobile Money
        /// </summary>
        [JsonProperty("paymentType")]
        public PaymentTypeEnumeration PaymentType { get; set; }
    }
}
