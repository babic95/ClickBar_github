using ClickBar_Common.Enums.Drlja;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_Common.Models.Order.Drlja
{
    public class PorudzbinaItemDrlja
    {

        [JsonProperty("kolicina")]
        public decimal Kolicina { get; set; }
        [JsonProperty("itemIdString")]
        public string ItemIdString { get; set; } = null!;
        [JsonProperty("jm")]
        public string Jm { get; set; } = null!;
        [JsonProperty("naziv")]
        public string Naziv { get; set; } = null!;
        [JsonProperty("rBS")]
        public int RBS { get; set; }
        [JsonProperty("brojNarudzbe")]
        public int BrojNarudzbe { get; set; }
        [JsonProperty("zelje")]
        public string? Zelje { get; set; } = null!;
        [JsonProperty("mPC")]
        public decimal MPC { get; set; }
        [JsonProperty("type")]
        public PorudzbinaTypeEnumeration? Type { get; set; }
    }
}
