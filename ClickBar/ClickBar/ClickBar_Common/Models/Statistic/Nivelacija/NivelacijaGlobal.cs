using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_Common.Models.Statistic.Nivelacija
{
    public class NivelacijaGlobal
    {
        public string Id { get; set; }
        public DateTime NivelacijaDate { get; set; }
        public int CounterNivelacije { get; set; }
        public int Type { get; set; }
        public string NameNivelacije { get; set; }
        public string? Description { get; set; }
        public List<NivelacijaItemGlobal> NivelacijaItems { get; set; }
    }
}
