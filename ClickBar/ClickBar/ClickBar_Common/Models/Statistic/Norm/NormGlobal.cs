using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_Common.Models.Statistic.Norm
{
    public class NormGlobal
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<NormItemGlobal> Items { get; set; }
    }
}
