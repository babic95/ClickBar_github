using ClickBar_DatabaseSQLManager.Models;
using ClickBar_Database_Drlja.Models;

namespace ClickBar_Porudzbine.Server.Models
{
    public class Sto
    {
        public Sto() { }
        public Sto(PaymentPlaceDB stoDB)
        {
            Id = stoDB.Id.ToString();
            Name = stoDB.Id;
            DeoSaleId = stoDB.PartHallId;
            X = stoDB.X_Mobi;
            Y = stoDB.Y_Mobi;
            Width = stoDB.WidthMobi;
            Height = stoDB.HeightMobi;
            Color = "#EC8B5E";
        }

        public string? Id { get; set; }
        public int? Name { get; set; }
        public int? DeoSaleId { get; set; }
        public float? X { get; set; }
        public float? Y { get; set; }
        public float? Width { get; set; }
        public float? Height { get; set; }
        public string? Color { get; set; }
    }
}
