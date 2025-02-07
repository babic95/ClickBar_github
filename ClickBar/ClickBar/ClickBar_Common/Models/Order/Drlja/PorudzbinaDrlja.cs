using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_Common.Models.Order.Drlja
{
    public class PorudzbinaDrlja
    {
        [JsonProperty("radnikId")]
        public string RadnikId { get; set; }
        [JsonProperty("porudzbinaId")]
        public string? PorudzbinaId { get; set; }
        [JsonProperty("radnikName")]
        public string? RadnikName { get; set; }
        [JsonProperty("stoBr")]
        public string StoBr { get; set; }
        [JsonProperty("insertInDB")]
        public bool? InsertInDB { get; set; }
        [JsonProperty("items")]
        public List<PorudzbinaItemDrlja>? Items { get; set; }
    }
}
