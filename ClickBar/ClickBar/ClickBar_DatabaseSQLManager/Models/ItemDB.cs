using System;
using System.Collections.Generic;

namespace ClickBar_DatabaseSQLManager.Models
{
    public partial class ItemDB
    {
        public ItemDB()
        {
            Procurements = new HashSet<ProcurementDB>();
            ItemInNorms = new HashSet<ItemInNormDB>();
            ItemsInUnprocessedOrder = new HashSet<ItemInUnprocessedOrderDB>();
            CalculationItems = new HashSet<CalculationItemDB>();
            ItemsNivelacija = new HashSet<ItemNivelacijaDB>();
            PocetnoStanjeItems = new HashSet<PocetnoStanjeItemDB>();
            OrderTodayItems = new HashSet<OrderTodayItemDB>();
            OtpisItems = new HashSet<OtpisItemDB>();
            Zelje = new HashSet<ItemZeljaDB>();
        }

        public string Id { get; set; } = null!;
        public int IdItemGroup { get; set; }
        public int? IdNorm { get; set; }
        public string Name { get; set; } = null!;
        public decimal SellingUnitPrice { get; set; }
        public decimal SellingNocnaUnitPrice { get; set; }
        public decimal SellingDnevnaUnitPrice { get; set; }
        public decimal? InputUnitPrice { get; set; }
        public string Label { get; set; } = null!;
        public string Jm { get; set; } = null!;
        public decimal TotalQuantity { get; set; }
        public decimal AlarmQuantity { get; set; }
        public int DisableItem { get; set; }
        public int IsCheckedZabraniPopust { get; set; }
        public int Rb { get; set; }

        public virtual ItemGroupDB ItemGroupNavigation { get; set; } = null!;
        public virtual NormDB Norm { get; set; } = null!;
        public virtual ICollection<ProcurementDB> Procurements { get; set; }
        public virtual ICollection<ItemInNormDB> ItemInNorms { get; set; }
        public virtual ICollection<ItemInUnprocessedOrderDB> ItemsInUnprocessedOrder { get; set; }
        public virtual ICollection<CalculationItemDB> CalculationItems { get; set; }
        public virtual ICollection<ItemNivelacijaDB> ItemsNivelacija { get; set; }
        public virtual ICollection<PocetnoStanjeItemDB> PocetnoStanjeItems { get; set; }
        public virtual ICollection<OrderTodayItemDB> OrderTodayItems { get; set; }
        public virtual ICollection<OtpisItemDB> OtpisItems { get; set; }
        public virtual ICollection<ItemZeljaDB> Zelje { get; set; }

    }
}
