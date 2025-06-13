using ClickBar_DatabaseSQLManager.Models;

namespace ClickBar_Porudzbine.Server.Models
{
    public class DeoSale
    {
        public DeoSale() { }
        public DeoSale(PartHallDB deoSaleDB)
        {
            Id = deoSaleDB.Id;
            Name = deoSaleDB.Name;
            Stolovi = new List<Sto>();
        }
        public int? Id { get; set; }
        public string Name { get; set; }
        public List<Sto>? Stolovi { get; set; }
    }
}
