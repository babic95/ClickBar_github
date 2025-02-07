using ClickBar_Common.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_Common.Models.Invoice.FileSystemWatcher
{
    public class InvoceRequestFileSystemWatcher
    {
        /// <summary>
        /// Cashier’s identification.
        /// </summary>
        [JsonProperty("cashier")]
        public string Cashier { get; set; }
        /// <summary>
        /// Taxpayer ID of the Buyer. It is mandatory for B2B transactions; otherwise, it's optional.
        /// </summary>
        [JsonProperty("buyerId")]
        public string? BuyerId { get; set; }
        [JsonProperty("buyerName")]
        public string? BuyerName { get; set; }
        [JsonProperty("buyerAddress")]
        public string? BuyerAddress { get; set; }
        /// <summary>
        /// Cost Center ID provided by the buyer to the cashier in case Buyer’s company wants to track spending in Taxpayer Portal. 
        /// It is optional and may exist only for B2B transactions; otherwise, it shall be ignored by E-SDC.
        /// </summary>
        [JsonProperty("buyerCostCenterId")]
        public string? BuyerCostCenterId { get; set; }
        /// <summary>
        /// Invoice Type enumeration value: 0 - Normal, 1 - Proforma, 2 - Copy, 3 - Training, 4 - Advance
        /// </summary>
        [JsonProperty("invoiceType")]
        public InvoiceTypeEenumeration InvoiceType { get; set; }
        /// <summary>
        /// Transaction Type enumeration value: 0 - Sale, 1 - Refund
        /// </summary>
        [JsonProperty("transactionType")]
        public TransactionTypeEnumeration TransactionType { get; set; }
        /// <summary>
        /// List of Payments for the invoice, where each Payment defines it's method and amount
        /// </summary>
        [JsonProperty("payment")]
        public IEnumerable<Payment> Payment { get; set; }
        /// <summary>
        /// Mandatory only in case Invoice Type is Refund, Copy or Normal Sale connected to an Advance Sale. 
        /// In all cases, this field must contain Invoice Number of previously issued invoice. 
        /// In any other case this field is optional. ASCII, in the requestedBy-signedBy-Ordinal_Number format. 
        /// Unicode MaxLength : 50
        /// </summary>
        [JsonProperty("referentDocumentNumber")]
        public string? ReferentDocumentNumber { get; set; }
        /// <summary>
        /// SDC Date and time of the document referenced in referentDocumentDT field. 
        /// It is used to calculate taxes on the date of issue of the original document that is refunded or copied.
        /// </summary>
        [JsonProperty("referentDocumentDT")]
        public DateTime? ReferentDocumentDT { get; set; }
        /// <summary>
        /// Each invoice contains at least one Item in Items collection (E-SDC should support minimum 250, recommended up to 500)
        /// </summary>
        [JsonProperty("items")]
        public IEnumerable<ItemFileSystemWatcher> Items { get; set; }
    }
}
