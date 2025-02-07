using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_eFaktura.Models.Response
{
    public class ResponseUBL
    {
        [JsonProperty("invoiceId")]
        public int InvoiceId { get; set; }
        [JsonProperty("purchaseInvoiceId")]
        public int PurchaseInvoiceId { get; set; }
        [JsonProperty("salesInvoiceId")]
        public int SalesInvoiceId { get; set; }
    }
}
