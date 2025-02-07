using ClickBar_Database_Drlja.Models;

namespace ClickBar_Porudzbine.Server.Models
{
    public class Zelja
    {
        public Zelja(ZeljaDB zeljaDB)
        {
            Id = zeljaDB.TR_IDZELJA;
            ArtikalId = zeljaDB.TR_BRART;
            Name = zeljaDB.TR_ZELJA;
            isCheck = false;

            if (Id < 0)
            {
                Description = string.Empty;
            }
            else
            {
                Description = zeljaDB.TR_ZELJA;
            }
        }
        public int Id { get; set; }
        public string ArtikalId { get; set; }
        public string Name { get; set; }
        public bool isCheck { get; set; }
        public string? Description { get; set; }
    }
}