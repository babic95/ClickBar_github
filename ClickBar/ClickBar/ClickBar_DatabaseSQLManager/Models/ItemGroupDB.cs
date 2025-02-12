using System;
using System.Collections.Generic;

namespace ClickBar_DatabaseSQLManager.Models
{
    public partial class ItemGroupDB
    {
        public ItemGroupDB()
        {
            Items = new HashSet<ItemDB>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int IdSupergroup { get; set; }

        public virtual SupergroupDB IdSupergroupNavigation { get; set; } = null!;
        public virtual ICollection<ItemDB> Items { get; set; }
    }
}
