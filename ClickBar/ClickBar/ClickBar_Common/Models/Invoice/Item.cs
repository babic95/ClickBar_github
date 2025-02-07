using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_Common.Models.Invoice
{
    public class Item
    {
        /// <summary>
        /// Global Trade Item Number (GTIN) is an identifier for trade items, 
        /// incorporated the ISBN, ISSN, ISMN, IAN (which includes the European Article Number and Japanese Article Number) 
        /// and some Universal Product Codes, into a universal number space.
        /// </summary>
        [JsonProperty("gtin")]
        public string? Gtin { get; set; }
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
        [NotMapped]
        [JsonProperty("labels")]
        public IEnumerable<string> Labels { get; set; }
        /// <summary>
        /// Gross price for the line item.
        /// </summary>
        [JsonProperty("totalAmount")]
        public decimal TotalAmount { get; set; }
        /// <summary>
        /// JM.
        /// </summary>
        [JsonProperty("jm")]
        public string Jm { get; set; }
        [JsonProperty("itemCode")]
        public string ItemCode { get; set; }
    }
}
