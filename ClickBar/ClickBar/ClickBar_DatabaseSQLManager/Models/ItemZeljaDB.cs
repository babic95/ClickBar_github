using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_DatabaseSQLManager.Models
{
    public class ItemZeljaDB
    {
        public string Id { get; set; } = null!;
        public string ItemId { get; set; } = null!;
        public string Zelja { get; set; } = null!;

        public virtual ItemDB Item { get; set; } = null!;
    }
}
