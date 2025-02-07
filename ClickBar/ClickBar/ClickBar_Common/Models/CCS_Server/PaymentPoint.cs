using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_Common.Models.CCS_Server
{
    public partial class PaymentPoint
    {
        [JsonProperty("id")]
        public string Id { get; set; } = null!;
        [JsonProperty("idStore")]
        public string IdStore { get; set; } = null!;
    }
}
