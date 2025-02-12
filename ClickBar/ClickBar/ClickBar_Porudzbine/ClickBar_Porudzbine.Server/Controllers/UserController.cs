using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClickBar_Porudzbine.Server.Models;
using ClickBar_Database_Drlja;
using ClickBar_Database_Drlja.Models;
using ClickBar_DatabaseSQLManager;

namespace ClickBar_Porudzbine.Server.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly SqlServerDbContext sqliteDbContext;
        private readonly SqliteDrljaDbContext sqliteDrljaDbContext;

        public UserController(SqlServerDbContext sqliteDbContext, SqliteDrljaDbContext sqliteDrljaDbContext)
        {
            this.sqliteDbContext = sqliteDbContext;
            this.sqliteDrljaDbContext = sqliteDrljaDbContext;
        }

        [HttpGet("allUsers")]
        public IActionResult AllUsers()
        {
            if (sqliteDbContext.Cashiers != null &&
                sqliteDbContext.Cashiers.Any())
            {
                List<User> users = new List<User>();
                sqliteDbContext.Cashiers.Where(c => c.Type == 0)
                    .ForEachAsync(radnik =>
                {
                    User user = new User(radnik);
                    users.Add(user);
                });

                return Ok(users);
            }
            return NotFound(null);
        }
    }
}
