using ClickBar_Porudzbine.Server.Enums;

namespace ClickBar_Porudzbine.Server.Models
{
    public class PorudzbinaItem
    {
        public string ItemId { get; set; } = null!;
        public decimal Kolicina { get; set; }
        public string Name { get; set; }
        public string? Zelje { get; set; } = null!;
        public decimal Mpc { get; set; }
    }
}
