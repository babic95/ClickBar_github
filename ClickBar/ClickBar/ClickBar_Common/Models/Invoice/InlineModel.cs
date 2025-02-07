using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_Common.Models.Invoice
{
    public class InlineModel
    {
        /// <summary>
        /// Value: "1" to omit QR Code generation by E-SDC and "0" to generate and return QR code.
        /// </summary>
        [JsonProperty("omitQRCodeGen")]
        public string OmitQRCodeGen { get; set; }
        /// <summary>
        /// Value: "1" to omit generation of textual representation by E-SDC and "0" to generate return textual representation to POS.
        /// </summary>
        [JsonProperty("omitTextualRepresentation")]
        public string OmitTextualRepresentation { get; set; }
    }
}
