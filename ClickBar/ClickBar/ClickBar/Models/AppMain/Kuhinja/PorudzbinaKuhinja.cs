using ClickBar_DatabaseSQLManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar.Models.AppMain.Kuhinja
{
    public class PorudzbinaKuhinja : ObservableObject
    {
        public OrderTodayDB OrderTodayDB { get; set; }
        public List<PorudzbinaKuhinjaItem> Items { get; set; }
        public PorudzbinaKuhinja(OrderTodayDB orderTodayDB, List<PorudzbinaKuhinjaItem> items)
        {
            OrderTodayDB = orderTodayDB;
            Items = items;
        }
    }
}
