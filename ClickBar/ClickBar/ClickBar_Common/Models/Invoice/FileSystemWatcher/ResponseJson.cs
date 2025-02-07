using ClickBar_Common.Models.Invoice.Tax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_Common.Models.Invoice.FileSystemWatcher
{
    public class ResponseJson
    {
        public string Message { get; set; }
        public string DateTime { get; set; }
        public string InvoiceNumber { get; set; }
        public string TotalInvoiceNumber { get; set; }
        public IEnumerable<TaxItem> TaxItems { get; set; }
    }
}
