namespace ClickBar_Porudzbine.Server.Models
{
    public class Nadgrupa
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Grupa> Grupe { get; set; }
    }
}
