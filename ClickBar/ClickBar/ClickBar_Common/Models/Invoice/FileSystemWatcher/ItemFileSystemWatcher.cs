using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_Common.Models.Invoice.FileSystemWatcher
{
    public class ItemFileSystemWatcher
    {
        /// <summary>
        /// Id.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }
        /// <summary>
        /// Human-readable name of the product or service.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
        /// <summary>
        /// Quantity of an item, with a maximum of 3 decimals. Example: 2 (pieces), 0.100 (grams).
        /// </summary>
        [JsonProperty("quantity")]
        public decimal Quantity { get; set; }
        /// <summary>
        /// Unit price of the line item. It does not take part in tax calculation.
        /// </summary>
        [JsonProperty("unitPrice")]
        public decimal UnitPrice { get; set; }
        /// <summary>
        /// The array of labels. Each Label represents one of the Tax Rates applied on the invoice item. 
        /// Tax Items are calculated based on totalAmount and applied labels as described in the Calculate Taxes section. 
        /// This field is mandatory (i.e. the caller must submit a non-empty collection) for each item, 
        /// even when the price is 0.00.
        /// </summary>
        [JsonProperty("label")]
        public string Label { get; set; }
        /// <summary>
        /// Gross price for the line item.
        /// </summary>
        [JsonProperty("totalAmount")]
        public decimal TotalAmount { get; set; }

        [JsonProperty("jm")]
        public string Jm { get; set; }
    }
}
