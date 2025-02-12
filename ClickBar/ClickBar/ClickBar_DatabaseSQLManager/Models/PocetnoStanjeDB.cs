using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_DatabaseSQLManager.Models
{
    public class PocetnoStanjeDB
    {
        public PocetnoStanjeDB()
        {
            PocetnoStanjeItems = new HashSet<PocetnoStanjeItemDB>();
        }

        public string Id { get; set; }
        public DateTime PopisDate { get; set; }
        public string? Description { get; set; }
        public virtual ICollection<PocetnoStanjeItemDB> PocetnoStanjeItems { get; set; }
    }
}
