using System;
using System.Collections.Generic;

namespace ClickBar_Database.Models
{
    public partial class PaymentPlaceDB
    {
        public int Id { get; set; }
        public int PartHallId { get; set; }
        public float? LeftCanvas { get; set; }
        public float? TopCanvas { get; set; }
        public int? Type { get; set; }
        public float? Width { get; set; }
        public float? Height { get; set; }
        public float? X_Mobi { get; set; }
        public float? Y_Mobi { get; set; }
        public float? WidthMobi { get; set; }
        public float? HeightMobi { get; set; }
        public float? Name { get; set; }
        public decimal Popust { get; set; }

        public virtual PartHallDB PartHall { get; set; } = null!;
        public virtual ICollection<UnprocessedOrderDB> UnprocessedOrders { get; set; }
    }
}
