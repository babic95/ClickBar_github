using ClickBar_Common.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_Common.Models.Invoice.Tax
{
    public class TaxItem
    {
        /// <summary>
        /// Tax Label (A, F, G, N, P…)
        /// </summary>
        [JsonProperty("label")]
        public string Label { get; set; }
        /// <summary>
        /// Tax Category Name (e.g. VAT, Consumption)
        /// </summary>
        [JsonProperty("categoryName")]
        public string CategoryName { get; set; }
        /// <summary>
        /// Tax Category Type (0 - Tax on net, 1 - Tax on total, 2 - Amount per quantity)
        /// </summary>
        [JsonProperty("categoryType")]
        public CategoryTypeEnumeration? CategoryType { get; set; }
        /// <summary>
        /// 	Tax rate percentage for Label (i.e. 12.50%)
        /// </summary>
        [JsonProperty("rate")]
        public decimal Rate { get; set; }
        /// <summary>
        /// Tax amount calculated by E-SDC during invoice fiscalization
        /// </summary>
        [JsonProperty("amount")]
        public decimal Amount { get; set; }
    }
}
