using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_Database.Models
{
    public class KnjizenjePazaraDB
    {
        public KnjizenjePazaraDB()
        {
            Invoices = new HashSet<InvoiceDB>();
        }

        public string Id { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime IssueDateTime { get; set; }
        public decimal NormalSaleCash { get; set; }
        public decimal NormalSaleCard { get; set; }
        public decimal NormalSaleWireTransfer { get; set; }
        public decimal NormalRefundCash { get; set; }
        public decimal NormalRefundCard { get; set; }
        public decimal NormalRefundWireTransfer { get; set; }

        public virtual ICollection<InvoiceDB> Invoices { get; set; }
    }
}
