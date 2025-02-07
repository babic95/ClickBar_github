using ClickBar_Database.Models;
using ClickBar_Database_Drlja.Models;

namespace ClickBar_Porudzbine.Server.Models
{
    public class User
    {
        public User() { }
        public User(CashierDB radnikDB)
        {
            Id = radnikDB.Id;
            Name = radnikDB.Name;
        }
        public string Id { get; set; }
        public string? Name { get; set; }
    }
}
