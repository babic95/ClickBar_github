using System;
using System.Collections.Generic;

namespace ClickBar_Database.Models
{
    public partial class SupplierDB
    {
        public SupplierDB()
        {
            Procurements = new HashSet<ProcurementDB>();
            Calculations = new HashSet<CalculationDB>();
        }

        public int Id { get; set; }
        public string Pib { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Address { get; set; }
        public string? ContractNumber { get; set; }
        public string? Email { get; set; }
        public string? City { get; set; }
        public string? Mb { get; set; }

        public virtual ICollection<ProcurementDB> Procurements { get; set; }
        public virtual ICollection<CalculationDB> Calculations { get; set; }
    }
}
