
using ClickBar_DatabaseSQLManager.Models;

namespace ClickBar_Porudzbine.Server.Models
{
    public class Zelja
    {
        public Zelja(ItemZeljaDB zeljaDB)
        {
            Id = zeljaDB.Id;
            ArtikalId = zeljaDB.ItemId;
            Name = zeljaDB.Zelja;
            isCheck = false;

            if (Convert.ToInt32(Id) < 0)
            {
                Description = string.Empty;
            }
            else
            {
                Description = zeljaDB.Zelja;
            }
        }
        public string Id { get; set; }
        public string ArtikalId { get; set; }
        public string Name { get; set; }
        public bool isCheck { get; set; }
        public string? Description { get; set; }
    }
}