using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_Database.Models
{
    public partial class NormDB
    {
        public NormDB()
        {
            ItemsInNorm = new HashSet<ItemInNormDB>();
        }
        public int Id { get; set; }
        public virtual ICollection<ItemInNormDB> ItemsInNorm { get; set; }
        public virtual ItemDB Item { get; set; } = null!;
    }
}
