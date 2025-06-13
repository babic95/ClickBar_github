using ClickBar_DatabaseSQLManager.Models;

namespace ClickBar_Porudzbine.Server.Models
{
    public class DeoSalePorudzbina
    {
        public DeoSalePorudzbina() { }
        public DeoSalePorudzbina(PartHallDB deoSaleDB)
        {
            Id = deoSaleDB.Id;
            Name = deoSaleDB.Name;
            Stolovi = new List<StoPorudzbina>();
        }
        public int? Id { get; set; }
        public string Name { get; set; }
        public List<StoPorudzbina>? Stolovi { get; set; }
    }
}
