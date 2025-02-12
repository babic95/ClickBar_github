using ClickBar_DatabaseSQLManager.Models;
using ClickBar_Database_Drlja.Models;

namespace ClickBar_Porudzbine.Server.Models
{
    public class Grupa
    {
        public Grupa(ItemGroupDB kategorijaDB)
        {
            Id = kategorijaDB.Id;
            Id_String = kategorijaDB.Id.ToString("000000");
            Name = kategorijaDB.Name;
            Artikli = new List<Artikal>();
        }
        public int Id { get; set; }
        public string Id_String { get; set; }
        public string Name { get; set; }
        public List<Artikal> Artikli { get; set; }
    }
}
