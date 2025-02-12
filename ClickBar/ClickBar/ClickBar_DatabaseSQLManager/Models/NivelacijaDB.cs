using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_DatabaseSQLManager.Models
{
    public partial class NivelacijaDB
    {
        public NivelacijaDB()
        {
            ItemsNivelacija = new HashSet<ItemNivelacijaDB>();
        }
        public string Id { get; set; } = null!;
        public DateTime DateNivelacije { get; set; }
        public int Counter { get; set; }
        public int Type { get; set; }
        public string? Description { get; set; }
        public virtual ICollection<ItemNivelacijaDB> ItemsNivelacija { get; set; }
    }
}
