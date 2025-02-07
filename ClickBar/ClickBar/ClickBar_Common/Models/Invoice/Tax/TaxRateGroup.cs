using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_Common.Models.Invoice.Tax
{
    public class TaxRateGroup
    {
        /// <summary>
        /// Date when a tax rates group shall enter into force
        /// </summary>
        [JsonProperty("validFrom")]
        public DateTime ValidFrom { get; set; }
        /// <summary>
        /// Revision number for all taxes under a tax rates group
        /// </summary>
        [JsonProperty("groupId")]
        public int GroupId { get; set; }
        /// <summary>
        /// All tax categories under one tax rates group
        /// </summary>
        [JsonProperty("taxCategories")]
        public IEnumerable<TaxCategory> TaxCategories { get; set; }
    }
}
