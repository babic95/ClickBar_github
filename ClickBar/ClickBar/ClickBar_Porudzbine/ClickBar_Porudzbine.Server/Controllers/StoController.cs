using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClickBar_Porudzbine.Server.Models;
using ClickBar_Database_Drlja;
using ClickBar_Database_Drlja.Models;
using ClickBar_Logging;
using ClickBar_DatabaseSQLManager;
using ClickBar_Common.Models.Invoice;
using ClickBar_DatabaseSQLManager.Models;

namespace ClickBar_Porudzbine.Server.Controllers
{
    [ApiController]
    [Route("api/sto")]
    public class StoController : ControllerBase
    {
        private readonly SqlServerDbContext sqliteDbContext;
        private readonly SqliteDrljaDbContext sqliteDrljaDbContext;

        public StoController(SqlServerDbContext SqlServerDbContext, SqliteDrljaDbContext SqliteDrljaDbContext)
        {
            sqliteDbContext = SqlServerDbContext;
            sqliteDrljaDbContext = SqliteDrljaDbContext;
        }

        [HttpGet("allStolovi")]
        public IActionResult Index()
        {
            try
            {
                //SqliteDrljaDbContext sqliteDrljaDbContext = new SqliteDrljaDbContext();

                if (sqliteDbContext.PartHalls != null &&
                    sqliteDbContext.PartHalls.Any())
                {
                    List<DeoSale> deloviSale = new List<DeoSale>();
                    sqliteDbContext.PartHalls.ForEachAsync(deoSaleDB =>
                    {
                        DeoSale deoSale = new DeoSale(deoSaleDB);

                        if (deoSale.Stolovi != null)
                        {
                            sqliteDbContext.PaymentPlaces.Where(s => s.PartHallId == deoSaleDB.Id)
                            .ForEachAsync(stoDB =>
                            {
                                Sto sto = new Sto(stoDB);
                                string stoName = $"S{stoDB.Id}";

                                var unprocessedOrders = sqliteDbContext.UnprocessedOrders.FirstOrDefault(order => order.PaymentPlaceId == stoDB.Id);
                                if (unprocessedOrders != null)
                                {
                                    var itemsInUnprocessedOrder = sqliteDbContext.Items.Join(sqliteDbContext.ItemsInUnprocessedOrder,
                                        item => item.Id,
                                        itemInUnprocessedOrder => itemInUnprocessedOrder.ItemId,
                                        (item, itemInUnprocessedOrder) => new { Item = item, ItemInUnprocessedOrder = itemInUnprocessedOrder })
                                    .Where(item => item.ItemInUnprocessedOrder.UnprocessedOrderId == unprocessedOrders.Id);

                                    if (itemsInUnprocessedOrder != null &&
                                        itemsInUnprocessedOrder.Any())
                                    {
                                        sto.Color = "#ff2c2c";
                                    }
                                }

                                deoSale.Stolovi.Add(sto);
                            });
                        }
                        deloviSale.Add(deoSale);
                    });

                    return Ok(deloviSale);
                }

                return NotFound(null);
            }
            catch (Exception ex)
            {
                Log.Error("StoController -> Index -> Greska: ", ex);
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("allStoloviAdmin")]
        public IActionResult AllStoloviAdmin()
        {
            try
            {
                if (sqliteDbContext.PartHalls != null &&
                    sqliteDbContext.PartHalls.Any())
                {
                    List<DeoSale> deloviSale = new List<DeoSale>();
                    sqliteDbContext.PartHalls.ForEachAsync(deoSaleDB =>
                    {
                        DeoSale deoSale = new DeoSale(deoSaleDB);

                        sqliteDbContext.PaymentPlaces.Where(s => s.PartHallId == deoSaleDB.Id)
                        .ForEachAsync(stoDB =>
                        {
                            Sto sto = new Sto(stoDB);

                            deoSale.Stolovi.Add(sto);
                        });

                        deloviSale.Add(deoSale);
                    });

                    return Ok(deloviSale);
                }
                return NotFound(null);
            }
            catch (Exception ex)
            {
                Log.Error("StoController -> Index -> Greska: ", ex);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("addDeoSale")]
        public IActionResult AddDeoSale(DeoSale deoSale)
        {
            try
            {
                int id = 1;

                if (sqliteDrljaDbContext.DeloviSale != null &&
                    sqliteDrljaDbContext.DeloviSale.Any())
                {
                    var deoSaleIdMax = sqliteDrljaDbContext.DeloviSale.Max(d => d.Id);
                    id = deoSaleIdMax + 1;
                }

                DeoSaleDB deoSaleDB = new DeoSaleDB()
                {
                    Id = id,
                    Name = deoSale.Name,
                };

                sqliteDrljaDbContext.DeloviSale.Add(deoSaleDB);
                sqliteDbContext.SaveChanges();

                return Ok();
            }
            catch (Exception ex)
            {
                Log.Error("StoController -> AddDeoSale -> Greska: ", ex);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("addSto")]
        public IActionResult AddSto(Sto sto)
        {
            try
            {
                if (sto.DeoSaleId > 0)
                {
                    var deoSale = sqliteDrljaDbContext.DeloviSale.FirstOrDefault(d => d.Id == sto.DeoSaleId);

                    if(deoSale == null)
                    {
                        return NotFound();
                    }

                    int name = 1;

                    if (sqliteDrljaDbContext.Stolovi != null &&
                        sqliteDrljaDbContext.Stolovi.Any())
                    {
                        var nameMax = sqliteDrljaDbContext.Stolovi.Max(s => s.Name);
                        name = nameMax + 1;
                    }

                    StoDB stoDB = new StoDB()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = name,
                        DeoSaleId = sto.DeoSaleId.Value,
                        Height = 20,
                        Width = 20,
                        X = 0,
                        Y = 0,
                    };

                    sqliteDrljaDbContext.Stolovi.Add(stoDB);
                    sqliteDrljaDbContext.SaveChanges();

                    return Ok();
                }

                return BadRequest("Nema dela sale");
            }
            catch (Exception ex)
            {
                Log.Error("StoController -> addSto -> Greska: ", ex);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("update")]
        public IActionResult UpdateStolovi(List<Sto> stolovi)
        {
            try
            {
                //SqliteDrljaDbContext sqliteDbContext = new SqliteDrljaDbContext();

                stolovi.ForEach(sto =>
                {
                    var deoSaleDB = sqliteDbContext.PartHalls.Find(sto.DeoSaleId);

                    if (deoSaleDB != null)
                    {
                        if (!string.IsNullOrEmpty(sto.Id))
                        {
                            var stoDB = sqliteDbContext.PaymentPlaces.Find(sto.Name);

                            if (stoDB != null)
                            {
                                stoDB.X_Mobi = sto.X.Value;
                                stoDB.Y_Mobi = sto.Y.Value;
                                stoDB.WidthMobi = sto.Width.Value;
                                stoDB.HeightMobi = sto.Height.Value;

                                sqliteDbContext.PaymentPlaces.Update(stoDB);
                            }
                        }

                        sqliteDbContext.SaveChanges();
                    }
                });
                return Ok();
            }
            catch (Exception ex)
            {
                Log.Error("StoController -> UpdateStolovi -> Greska: ", ex);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("pay")]
        public IActionResult Pay(Sto sto)
        {
            try
            {
                string stoString = $"S{sto.Name}";

                var porudzbineDB = sqliteDrljaDbContext.Narudzbine.Where(n => n.TR_STO == stoString &&
                n.TR_FAZA == 3);

                if (porudzbineDB != null &&
                    porudzbineDB.Any())
                {
                    porudzbineDB.ForEachAsync(porDB =>
                    {
                        porDB.TR_FAZA = 4;
                        sqliteDrljaDbContext.Narudzbine.Update(porDB);
                    });

                    sqliteDrljaDbContext.SaveChanges();
                }

                return Ok();
            }
            catch (Exception ex)
            {
                Log.Error("StoController -> Pay -> Greska: ", ex);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("moveOrder")]
        public IActionResult MoveOrder(MovePorudzbine movePorudzbine)
        {
            try
            {
                //SqliteDrljaDbContext sqliteDbContext = new SqliteDrljaDbContext();

                if (movePorudzbine.FromSto.Name.HasValue &&
                    movePorudzbine.ToSto.Name.HasValue)
                {
                    var fromStoDB = sqliteDbContext.UnprocessedOrders.FirstOrDefault(order => order.PaymentPlaceId == movePorudzbine.FromSto.Name);
                    var toStoDB = sqliteDbContext.UnprocessedOrders.FirstOrDefault(order => order.PaymentPlaceId == movePorudzbine.ToSto.Name);

                    if (fromStoDB != null)
                    {
                        if (toStoDB != null)
                        {
                            var itemsFromSto = sqliteDbContext.ItemsInUnprocessedOrder.Where(item => item.UnprocessedOrderId == fromStoDB.Id);
                            var itemsToSto = sqliteDbContext.ItemsInUnprocessedOrder.Where(item => item.UnprocessedOrderId == toStoDB.Id);

                            if (itemsFromSto != null &&
                                itemsFromSto.Any())
                            {
                                itemsFromSto.ForEachAsync(item =>
                                {
                                    var itemInToSto = itemsToSto.FirstOrDefault(i => i.ItemId == item.ItemId);
                                    if (itemInToSto != null)
                                    {
                                        itemInToSto.Quantity += item.Quantity;
                                    }
                                    else
                                    {
                                        item.UnprocessedOrderId = toStoDB.Id;
                                    }
                                    sqliteDbContext.ItemsInUnprocessedOrder.Update(item);
                                });
                            }

                            toStoDB.TotalAmount += fromStoDB.TotalAmount;
                            sqliteDbContext.UnprocessedOrders.Update(toStoDB);
                        }
                        else
                        {
                            fromStoDB.PaymentPlaceId = movePorudzbine.ToSto.Name.Value;
                            sqliteDbContext.UnprocessedOrders.Update(fromStoDB);
                        }
                        sqliteDbContext.SaveChanges();
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                Log.Error("StoController -> MoveOrder -> Greska: ", ex);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("moveWorker")]
        public IActionResult MoveWorker(MoveKonobar moveKonobar)
        {
            try
            {
                var porudzbineFromDB = sqliteDrljaDbContext.Narudzbine.Where(n => n.TR_RADNIK == moveKonobar.FromKonobarId &&
                n.TR_FAZA < 4 && n.TR_STO == moveKonobar.StoId);

                if (porudzbineFromDB != null &&
                    porudzbineFromDB.Any())
                {
                    porudzbineFromDB.ForEachAsync(porDB =>
                    {
                        porDB.TR_RADNIK = moveKonobar.ToKonobarId;
                        sqliteDrljaDbContext.Narudzbine.Update(porDB);
                    });

                    sqliteDrljaDbContext.SaveChanges();
                }

                return Ok();
            }
            catch (Exception ex)
            {
                Log.Error("StoController -> MoveOrder -> Greska: ", ex);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("stoPorudzbine")]
        public IActionResult StoPorudzbine()
        {
            try
            {
                //SqliteDrljaDbContext sqliteDrljaDbContext = new SqliteDrljaDbContext();

                if (sqliteDbContext.PartHalls != null &&
                sqliteDbContext.PartHalls.Any())
                {
                    List<DeoSalePorudzbina> deloviSale = new List<DeoSalePorudzbina>();
                    sqliteDbContext.PartHalls.ForEachAsync(deoSaleDB =>
                    {
                        DeoSalePorudzbina deoSale = new DeoSalePorudzbina(deoSaleDB);

                        if (deoSale.Stolovi != null)
                        {
                            sqliteDbContext.PaymentPlaces.Where(s => s.PartHallId == deoSaleDB.Id)
                            .ForEachAsync(stoDB =>
                            {
                                Sto sto = new Sto(stoDB);
                                StoPorudzbina stoPorudzbina = new StoPorudzbina()
                                {
                                    Sto = sto,
                                    Items = new List<PorudzbinaItem>()
                                };

                                string stoName = $"S{stoDB.Id}";

                                var unprocessedOrders = sqliteDbContext.UnprocessedOrders.FirstOrDefault(order => order.PaymentPlaceId == stoDB.Id);
                                if (unprocessedOrders != null)
                                {
                                    var itemsInUnprocessedOrder = sqliteDbContext.Items.Join(sqliteDbContext.ItemsInUnprocessedOrder,
                                        item => item.Id,
                                        itemInUnprocessedOrder => itemInUnprocessedOrder.ItemId,
                                        (item, itemInUnprocessedOrder) => new { Item = item, ItemInUnprocessedOrder = itemInUnprocessedOrder })
                                    .Where(item => item.ItemInUnprocessedOrder.UnprocessedOrderId == unprocessedOrders.Id);

                                    if (itemsInUnprocessedOrder != null &&
                                        itemsInUnprocessedOrder.Any())
                                    {
                                        itemsInUnprocessedOrder.ToList().ForEach(item =>
                                        {
                                            PorudzbinaItem porudzbinaItem = new PorudzbinaItem()
                                            {
                                                ItemIdString = item.Item.Id,
                                                Jm = item.Item.Jm,
                                                Kolicina = item.ItemInUnprocessedOrder.Quantity,
                                                Naziv = item.Item.Name,
                                                //BrojNarudzbe = porDB.TR_BROJNARUDZBE,
                                                MPC = item.Item.SellingUnitPrice,
                                                //Zelje = itemDB.TR_ZELJA,
                                                //RBS = itemDB.TR_RBS,
                                            };

                                            stoPorudzbina.Items.Add(porudzbinaItem);
                                        });

                                        stoPorudzbina.Sto.Color = "#ff2c2c";
                                    }
                                }

                                //var porudzbineDB = sqliteDrljaDbContext.Narudzbine.Where(n => n.TR_STO == stoName &&
                                //n.TR_FAZA == 3);

                                //if (porudzbineDB != null &&
                                //porudzbineDB.Any())
                                //{
                                //    porudzbineDB.ForEachAsync(porDB =>
                                //    {
                                //        var itemsDB = sqliteDrljaDbContext.StavkeNarudzbine.Where(i => i.TR_BROJNARUDZBE == porDB.TR_BROJNARUDZBE);

                                //        if (itemsDB != null &&
                                //        itemsDB.Any())
                                //        {
                                //            itemsDB.ForEachAsync(itemDB =>
                                //            {
                                //                PorudzbinaItem porudzbinaItem = new PorudzbinaItem()
                                //                {
                                //                    ItemIdString = itemDB.TR_BRART,
                                //                    Jm = itemDB.TR_PAK,
                                //                    Kolicina = itemDB.TR_KOL,
                                //                    Naziv = itemDB.TR_NAZIV,
                                //                    BrojNarudzbe = porDB.TR_BROJNARUDZBE,
                                //                    MPC = itemDB.TR_MPC,
                                //                    Zelje = itemDB.TR_ZELJA,
                                //                    RBS = itemDB.TR_RBS,
                                //                };

                                //                stoPorudzbina.Items.Add(porudzbinaItem);
                                //            });

                                //            stoPorudzbina.Sto.Color = "#ff2c2c";
                                //        }
                                //    });
                                //}

                                deoSale.Stolovi.Add(stoPorudzbina);
                            });
                        }

                        deloviSale.Add(deoSale);
                    });

                    return Ok(deloviSale);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                Log.Error("StoController -> StoPorudzbine -> Greska: ", ex);
                return BadRequest(ex.Message);
            }
        }
    }
}
