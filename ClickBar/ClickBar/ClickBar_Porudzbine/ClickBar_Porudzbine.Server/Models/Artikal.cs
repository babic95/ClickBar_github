using ClickBar_DatabaseSQLManager.Models;
using ClickBar_Database_Drlja.Models;

namespace ClickBar_Porudzbine.Server.Models
{
    public class Artikal
    {
        public Artikal(ItemDB artikliDB) 
        {
            Id = Convert.ToInt32(artikliDB.Id);
            Id_String = artikliDB.Id;
            Name = artikliDB.Name;
            Jm = artikliDB.Jm;
            Mpc = Decimal.Round(Convert.ToDecimal(artikliDB.SellingUnitPrice), 2);
            //BrzoBiranje = artikliDB.TR_GLAVNI;
            Zelje = new List<Zelja>();
            IsOpen = false;
            TotalOrderNumber = 0;
            IsAdded = false;
        }

        public int Id { get; set; }
        public string Id_String { get; set; }
        public string Name { get; set; }
        public string Jm { get; set; }
        public decimal Mpc { get; set; }
        public string? BrzoBiranje { get; set; }
        public List<Zelja> Zelje { get; set; }
        public bool IsOpen { get; set; }
        public int TotalOrderNumber { get; set; }
        public bool IsAdded { get; set; }
    }
}
