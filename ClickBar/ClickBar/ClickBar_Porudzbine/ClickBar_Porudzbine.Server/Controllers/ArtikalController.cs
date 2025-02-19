using ClickBar_Database_Drlja;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClickBar_Porudzbine.Server.Models;
using ClickBar_Database_Drlja.Models;
using System.Text.RegularExpressions;
using ClickBar_DatabaseSQLManager;

namespace ClickBar_Porudzbine.Server.Controllers
{
    [ApiController]
    [Route("api/artikal")]
    public class ArtikalController : ControllerBase
    {
        private readonly SqlServerDbContext _sqlServerDbContext;
        private readonly SqliteDrljaDbContext _sqliteDrljaDbContext;

        public ArtikalController(SqlServerDbContext sqlServerDbContext, SqliteDrljaDbContext sqliteDrljaDbContext)
        {
            _sqlServerDbContext = sqlServerDbContext;
            _sqliteDrljaDbContext = sqliteDrljaDbContext;
        }

        [HttpGet("allArtikli")]
        public async Task<IActionResult> AllArtikli()
        {
            List<Nadgrupa> nadgrupe = new List<Nadgrupa>();

            var artikliDB = _sqlServerDbContext.Items.Join(_sqlServerDbContext.ItemGroups,
                item => item.IdItemGroup,
                group => group.Id,
                (item, group) => new { Item = item, Group = group }).Join(_sqlServerDbContext.Supergroups,
                group => group.Group.IdSupergroup,
                supergroup => supergroup.Id,
                (group, supergroup) => new { Group = group, SuperGroup = supergroup });

            if (artikliDB != null &&
                artikliDB.Any())
            {
                foreach(var artikalDB in artikliDB)
                {
                    if (artikalDB.Group.Group.Name.ToLower() != "sirovina" &&
                       artikalDB.Group.Group.Name.ToLower() != "sirovine" &&
                       artikalDB.SuperGroup.Name.ToLower() != "osnovna")
                    {
                        Artikal artikal = new Artikal(artikalDB.Group.Item);

                        var nadgrupa = nadgrupe.FirstOrDefault(n => n.Id == artikalDB.SuperGroup.Id);

                        if (nadgrupa == null)
                        {
                            nadgrupa = new Nadgrupa()
                            {
                                Id = artikalDB.SuperGroup.Id,
                                Name = artikalDB.SuperGroup.Name,
                                Grupe = new List<Grupa>()
                            };
                            nadgrupe.Add(nadgrupa);
                        }

                        var grupa = nadgrupa.Grupe.FirstOrDefault(g => g.Id == artikalDB.Group.Group.Id);
                        if (grupa == null)
                        {
                            grupa = new Grupa(artikalDB.Group.Group);
                            nadgrupa.Grupe.Add(grupa);
                        }

                        grupa.Artikli.Add(artikal);

                        var zelje = _sqliteDrljaDbContext.Zelje.Where(z => z.TR_BRART == artikal.Id_String);

                        if (zelje != null &&
                        zelje.Any())
                        {
                            foreach(var z in zelje)
                            {
                                artikal.Zelje.Add(new Zelja(z));
                            }
                        }

                        Zelja zelja = new Zelja(new ZeljaDB()
                        {
                            TR_IDZELJA = -1 * artikal.Id,
                            TR_ZELJA = "Dodaj zelju",
                            TR_BRART = artikal.Id_String
                        });
                        artikal.Zelje.Add(zelja);
                    }
                }

                return Ok(nadgrupe);
            }
            return NotFound(null);
        }
    }
}
