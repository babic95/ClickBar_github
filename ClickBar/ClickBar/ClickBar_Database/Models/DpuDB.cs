using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_Database.Models
{
    public partial class DpuDB
    {
        public DpuDB() 
        {
            Items = new HashSet<DpuItemDB>();
        }

        public DateTime Datum { get; set; }
        public int Id { get; set; }
        public ICollection<DpuItemDB> Items { get; set; }
    }
}
