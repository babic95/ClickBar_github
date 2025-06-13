using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClickBar_Porudzbine.Server.Models;
using ClickBar_DatabaseSQLManager;

namespace ClickBar_Porudzbine.Server.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly SqlServerDbContext _sqlServerDbContext;

        public UserController(SqlServerDbContext sqlServerDbContext)
        {
            _sqlServerDbContext = sqlServerDbContext;
        }

        [HttpGet("allUsers")]
        public async Task<IActionResult> AllUsers()
        {
            if (_sqlServerDbContext.Cashiers != null &&
                _sqlServerDbContext.Cashiers.Any())
            {
                List<User> users = new List<User>();
                foreach (var radnik in _sqlServerDbContext.Cashiers.Where(c => c.Type == 0))
                {
                    users.Add(new User(radnik));
                }

                return Ok(users);
            }
            return NotFound(null);
        }
    }
}
