namespace ClickBar_Porudzbine.Server.Models
{
    public class Porudzbina
    {
        public Porudzbina() { }

        public string RadnikId { get; set; }
        public int StoId { get; set; }
        public List<PorudzbinaItem>? Items { get; set; }
    }
}
