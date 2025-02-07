namespace ClickBar_Porudzbine.Server.Models
{
    public class MovePorudzbinaClickBar
    {
        public string OldUnprocessedOrderId { get; set; }
        public string NewUnprocessedOrderId { get; set; }
        public int NewSto { get; set; }
        public List<PorudzbinaItem> Items { get; set; }
    }
}
