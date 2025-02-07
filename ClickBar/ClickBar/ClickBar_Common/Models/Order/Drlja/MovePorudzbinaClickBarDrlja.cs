using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_Common.Models.Order.Drlja
{
    public class MovePorudzbinaClickBarDrlja
    {
        [JsonProperty("oldUnprocessedOrderId")]
        public string OldUnprocessedOrderId { get; set; }
        [JsonProperty("newUnprocessedOrderId")]
        public string NewUnprocessedOrderId { get; set; }
        [JsonProperty("newSto")]
        public int NewSto { get; set; }
        [JsonProperty("items")]
        public List<PorudzbinaItemDrlja> Items { get; set; }
    }
}
