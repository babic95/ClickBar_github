using ClickBar_Porudzbine.Server.Enums;

namespace ClickBar_Porudzbine.Server.Models
{
    public class PorudzbinaItem
    {
        public string ItemIdString { get; set; } = null!;
        public decimal Kolicina { get; set; }
        public string Jm { get; set; } = null!;
        public string Naziv { get; set; } = null!;
        public int RBS { get; set; }
        public int BrojNarudzbe { get; set; }
        public string? Zelje { get; set; } = null!;
        public decimal MPC { get; set; }
        public PorudzbinaTypeEnumeration? Type { get; set; }
    }
}
