using System;
using System.Collections.Generic;

namespace ClickBar_Database.Models
{
    public partial class PartHallDB
    {
        public PartHallDB()
        {
            Paymentplaces = new HashSet<PaymentPlaceDB>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Image { get; set; }

        public virtual ICollection<PaymentPlaceDB> Paymentplaces { get; set; }
    }
}
