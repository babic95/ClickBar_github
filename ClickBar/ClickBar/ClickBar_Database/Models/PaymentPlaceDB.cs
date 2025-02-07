using System;
using System.Collections.Generic;

namespace ClickBar_Database.Models
{
    public partial class PaymentPlaceDB
    {
        public int Id { get; set; }
        public int PartHallId { get; set; }
        public decimal? LeftCanvas { get; set; }
        public decimal? TopCanvas { get; set; }
        public int? Type { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public decimal? X_Mobi { get; set; }
        public decimal? Y_Mobi { get; set; }
        public decimal? WidthMobi { get; set; }
        public decimal? HeightMobi { get; set; }
        public string? Name { get; set; }
        public decimal Popust { get; set; }

        public virtual PartHallDB PartHall { get; set; } = null!;
        public virtual ICollection<UnprocessedOrderDB> UnprocessedOrders { get; set; }
    }
}
