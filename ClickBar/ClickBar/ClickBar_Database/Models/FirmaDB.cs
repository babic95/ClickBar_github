using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_Database.Models
{
    public class FirmaDB
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Pib { get; set; }
        public string? MB { get; set; }
        public string? NamePP { get; set; }
        public string? AddressPP { get; set; }
        public string? Number { get; set; }
        public string? Email { get; set; }
        public string? BankAcc { get; set; }
        public string? AuthenticationKey { get; set; }
    }
}
