namespace ClickBar_Porudzbine.Server.Models
{
    public class Porudzbina
    {
        public Porudzbina() { }

        public int? BrPorudzbine { get; set; }
        public string RadnikId { get; set; }
        public string? PorudzbinaId { get; set; }
        public string? RadnikName { get; set; }
        public string StoBr { get; set; }
        public bool? InsertInDB { get; set; }
        public List<PorudzbinaItem>? Items { get; set; }
    }
}
