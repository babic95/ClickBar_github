using ClickBar_Common.Models.Invoice.Helpers;
using ClickBar_Common.Models.Invoice.Tax;
using ClickBar_Common.Models.Invoice.Verification;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_Common.Models.Invoice
{
    public class InvoiceResult : IRespone
    {
        /// <summary>
        /// UID of client's Secure Element digital certificate.
        /// </summary>
        [JsonProperty("requestedBy")]
        public string RequestedBy { get; set; }
        /// <summary>
        /// UID of SDC's Secure Element digital certificate.
        /// </summary>
        [JsonProperty("signedBy")]
        public string SignedBy { get; set; }
        /// <summary>
        /// Local date and time in ISO 8601 format provided by E-SDC.
        /// </summary>
        [JsonProperty("sdcDateTime")]
        public DateTime SdcDateTime { get; set; }
        /// <summary>
        /// Invoice Counter in format transactionTypeCounter/totalCounter invoiceCounterExtension
        /// For Example: 14/17NS
        /// </summary>
        [JsonProperty("invoiceCounter")]
        public string InvoiceCounter { get; set; }
        /// <summary>
        /// First letters of Transaction Type and Invoice Type of the invoice. NS for Normal Sale, CR – Copy Refund, TS – Training Sale, etc.
        /// </summary>
        [JsonProperty("invoiceCounterExtension")]
        public string InvoiceCounterExtension { get; set; }
        /// <summary>
        /// SDC Invoice Number in format requestedBy-signedBy-totalCounter
        /// </summary>
        [JsonProperty("invoiceNumber")]
        public string InvoiceNumber { get; set; }
        /// <summary>
        /// VerificationURL generated in the Create Verification URL process
        /// </summary>
        [JsonProperty("verificationUrl")]
        public string VerificationUrl { get; set; }
        /// <summary>
        /// Base64 encoded byte array of GIF image created in the Create QR Code process
        /// </summary>
        public string VerificationQRCode { get; set; }
        /// <summary>
        /// Textual Representation of the invoice created in the Create a Textual Representation of an Invoice (Receipt) process
        /// </summary>
        [JsonProperty("journal")]
        public string Journal { get; set; }
        /// <summary>
        /// Total number of invoices signed by Secure Element. Returned by Sign Invoice APDU command
        /// </summary>
        [JsonProperty("totalCounter")]
        public int? TotalCounter { get; set; }
        /// <summary>
        /// Total number of invoices for a requested type. Returned by Sign Invoice APDU command
        /// </summary>
        [JsonProperty("transactionTypeCounter")]
        public int? TransactionTypeCounter { get; set; }
        /// <summary>
        /// Sum of all Items – total payable by the customer
        /// </summary>
        [JsonProperty("totalAmount")]
        public decimal? TotalAmount { get; set; }
        /// <summary>
        /// Base64 encoded byte array returned by Sign Invoice APDU command
        /// </summary>
        [JsonProperty("encryptedInternalData")]
        public string EncryptedInternalData { get; set; }
        /// <summary>
        /// Base64 encoded byte array returned by Sign Invoice APDU command
        /// </summary>
        [JsonProperty("signature")]
        public string Signature { get; set; }
        /// <summary>
        /// Array of TaxItem entities
        /// </summary>
        [NotMapped]
        [JsonProperty("taxItems")]
        public IEnumerable<TaxItem> TaxItems { get; set; }
        /// <summary>
        /// Taxpayer Business Name obtained from digital certificate subject field
        /// </summary>
        [JsonProperty("businessName")]
        public string BusinessName { get; set; }
        /// <summary>
        /// Location Name obtained from digital certificate subject field
        /// </summary>
        [JsonProperty("locationName")]
        public string LocationName { get; set; }
        /// <summary>
        /// Street address obtained from digital certificate subject field
        /// </summary>
        [JsonProperty("address")]
        public string Address { get; set; }
        /// <summary>
        /// Tax Identification Number obtained from digital certificate subject field
        /// </summary>
        [JsonProperty("tin")]
        public string Tin { get; set; }
        /// <summary>
        /// District obtained from digital certificate subject field
        /// </summary>
        [JsonProperty("district")]
        public string District { get; set; }
        /// <summary>
        /// Revision of taxes used in the calculation
        /// </summary>
        [JsonProperty("taxGroupRevision")]
        public int? TaxGroupRevision { get; set; }
        /// <summary>
        /// Manufacturer Registration Code is mandatory for audit package sent to the tax authority database, 
        /// but it's optional for invoice response sent to POS. It always has the format MakeCode-SoftwareVersionCode-DeviceSerialNumber. 
        /// Explanation: MakeCode -unique 2 characters received from the tax authority during accreditation. 
        /// SoftwareVersionCode - unique 4 characters received from the tax authroty during accreditation. 
        /// DeviceSerialNumber - manufacturer serial number (max 32 characters) for each E-SDC installation. 
        /// All 3 elements of MRC are mandatory.
        /// </summary>
        [JsonProperty("mrc")]
        public string Mrc { get; set; }
        /// <summary>
        /// Custom human-readable message that shall be printed or displayed by POS.
        /// </summary>
        [NotMapped]
        [JsonProperty("messages")]
        public string Messages { get; set; }
        [NotMapped]
        public string AdvanceAddition { get; set; }

        public void CreateVerificationUrl(InvoiceRequest invoiceRequest, int version, string url)
        {
            VerificationURL verificationUrl = new()
            {
                BuyerId = invoiceRequest.BuyerId,
                DateAndTime = this.SdcDateTime,
                EncryptedInternalData = Convert.FromBase64String(this.EncryptedInternalData),
                InvoiceType = (Enums.InvoiceTypeEenumeration)invoiceRequest.InvoiceType,
                RequestedBy = this.RequestedBy,
                Signature = Convert.FromBase64String(this.Signature),
                SignedBy = this.SignedBy,
                TotalAmount = this.TotalAmount,
                TotalCounter = this.TotalCounter,
                TransactionType = (Enums.TransactionTypeEnumeration)invoiceRequest.TransactionType,
                TransactionTypeCounter = this.TransactionTypeCounter,
                Version = version
            };
            this.VerificationUrl = string.Format("{0}{1}", url, verificationUrl.GetVerificationURL());
        }
        public void CreateVerificationQRCode()
        {
            VerificationQRcode verificationQRcode = new VerificationQRcode(this.VerificationUrl);
            this.VerificationQRCode = verificationQRcode.GetQRcode();
        }

        public void CreateJournal(InvoiceRequest invoiceRequest)
        {
            this.Journal = JournalHelper.CreateJournal(invoiceRequest, this);
        }
    }
}
