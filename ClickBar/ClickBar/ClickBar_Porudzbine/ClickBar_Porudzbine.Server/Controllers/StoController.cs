using ClickBar_Common.Models.Invoice;
using ClickBar_DatabaseSQLManager;
using ClickBar_DatabaseSQLManager.Models;
using ClickBar_Logging;
using ClickBar_Porudzbine.Server.Enums;
using ClickBar_Porudzbine.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClickBar_Porudzbine.Server.Controllers
{
    [ApiController]
    [Route("api/sto")]
    public class StoController : ControllerBase
    {
        private readonly SqlServerDbContext _sqlServerDbContext;

        public StoController(SqlServerDbContext sqlServerDbContext)
        {
            _sqlServerDbContext = sqlServerDbContext;
        }

        [HttpGet("allStolovi")]
        public async Task<IActionResult> Index()
        {
            try
            {
                if (_sqlServerDbContext.PartHalls != null &&
                    _sqlServerDbContext.PartHalls.Any())
                {
                    List<DeoSale> deloviSale = new List<DeoSale>();
                    foreach (var deoSaleDB in _sqlServerDbContext.PartHalls)
                    {
                        DeoSale deoSale = new DeoSale(deoSaleDB);

                        if (deoSale.Stolovi != null)
                        {
                            foreach (var stoDB in _sqlServerDbContext.PaymentPlaces.Where(s => s.PartHallId == deoSaleDB.Id))
                            {
                                Sto sto = new Sto(stoDB);

                                var unprocessedOrders = _sqlServerDbContext.UnprocessedOrders.FirstOrDefault(order => order.PaymentPlaceId == stoDB.Id);
                                if (unprocessedOrders != null)
                                {
                                    var ordersOld = _sqlServerDbContext.OrdersToday.Include(o => o.OrderTodayItems).AsNoTracking()
                                        .Where(o => o.TableId == stoDB.Id &&
                                        o.UnprocessedOrderId == unprocessedOrders.Id &&
                                        o.Faza != (int)FazaPorudzbineEnumeration.Naplacena &&
                                        o.Faza != (int)FazaPorudzbineEnumeration.Obrisana);

                                    decimal total = Decimal.Round(ordersOld
                                        .Sum(o => o.OrderTodayItems.Where(oti => oti.Quantity != 0 && oti.Quantity - oti.StornoQuantity - oti.NaplacenoQuantity > 0).Sum(oti => Decimal.Round((oti.Quantity - oti.StornoQuantity - oti.NaplacenoQuantity) * (oti.TotalPrice / oti.Quantity), 2))), 2);

                                    if (total != 0 || ordersOld.Any())
                                    {
                                        sto.Color = "#ff2c2c";
                                        sto.TotalPrice = total;
                                    }
                                }

                                deoSale.Stolovi.Add(sto);
                            }
                        }
                        deloviSale.Add(deoSale);
                    }

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
        public async Task<IActionResult> AllStoloviAdmin()
        {
            try
            {
                if (_sqlServerDbContext.PartHalls != null &&
                    _sqlServerDbContext.PartHalls.Any())
                {
                    List<DeoSale> deloviSale = new List<DeoSale>();
                    foreach (var deoSaleDB in _sqlServerDbContext.PartHalls)
                    {
                        DeoSale deoSale = new DeoSale(deoSaleDB);

                        foreach (var stoDB in _sqlServerDbContext.PaymentPlaces.Where(s => s.PartHallId == deoSaleDB.Id))
                        {
                            deoSale.Stolovi.Add(new Sto(stoDB));
                        }

                        deloviSale.Add(deoSale);
                    }

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

        [HttpPost("update")]
        public async Task<IActionResult> UpdateStolovi([FromBody] List<Sto> stolovi)
        {
            try
            {
                foreach (var sto in stolovi)
                {
                    var deoSaleDB = await _sqlServerDbContext.PartHalls.FindAsync(sto.DeoSaleId);

                    if (deoSaleDB != null && !string.IsNullOrEmpty(sto.Id))
                    {
                        var stoDB = await _sqlServerDbContext.PaymentPlaces.FindAsync(sto.Name);

                        if (stoDB != null)
                        {
                            stoDB.X_Mobi = sto.X.Value;
                            stoDB.Y_Mobi = sto.Y.Value;
                            stoDB.WidthMobi = sto.Width.Value;
                            stoDB.HeightMobi = sto.Height.Value;

                            _sqlServerDbContext.PaymentPlaces.Update(stoDB);
                        }
                    }
                }

                await _sqlServerDbContext.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                Log.Error("StoController -> UpdateStolovi -> Greska: ", ex);
                return BadRequest(ex.Message);
            }
        }

        //[HttpGet("stoPorudzbine")]
        //public async Task<IActionResult> StoPorudzbine()
        //{
        //    try
        //    {
        //        if (_sqlServerDbContext.PartHalls != null &&
        //        _sqlServerDbContext.PartHalls.Any())
        //        {
        //            List<DeoSalePorudzbina> deloviSale = new List<DeoSalePorudzbina>();
        //            foreach (var deoSaleDB in _sqlServerDbContext.PartHalls)
        //            {
        //                DeoSalePorudzbina deoSale = new DeoSalePorudzbina(deoSaleDB);

        //                if (deoSale.Stolovi != null)
        //                {
        //                    foreach (var stoDB in _sqlServerDbContext.PaymentPlaces.Where(s => s.PartHallId == deoSaleDB.Id))
        //                    {
        //                        Sto sto = new Sto(stoDB);
        //                        StoPorudzbina stoPorudzbina = new StoPorudzbina()
        //                        {
        //                            Sto = sto,
        //                            Items = new List<PorudzbinaItem>()
        //                        };

        //                        var unprocessedOrders = _sqlServerDbContext.UnprocessedOrders.FirstOrDefault(order => order.PaymentPlaceId == stoDB.Id);
        //                        if (unprocessedOrders != null)
        //                        {
        //                            var ordersOld = _sqlServerDbContext.OrdersToday.Include(o => o.OrderTodayItems).AsNoTracking()
        //                                .Where(o => o.TableId == stoDB.Id &&
        //                                o.UnprocessedOrderId == unprocessedOrders.Id &&
        //                                o.Faza != (int)FazaPorudzbineEnumeration.Naplacena &&
        //                                o.Faza != (int)FazaPorudzbineEnumeration.Obrisana);

        //                            decimal total = Decimal.Round(ordersOld
        //                                .Sum(o => o.OrderTodayItems.Where(oti => oti.Quantity != 0 && oti.Quantity - oti.StornoQuantity - oti.NaplacenoQuantity > 0).Sum(oti => Decimal.Round((oti.Quantity - oti.StornoQuantity - oti.NaplacenoQuantity) * (oti.TotalPrice / oti.Quantity), 2))), 2);

        //                            if (total != 0 || ordersOld.Any())
        //                            {
        //                                stoPorudzbina.Sto.Color = "#ff2c2c";
        //                                stoPorudzbina.Sto.TotalPrice = total;

        //                                foreach(var order in ordersOld)
        //                                {
        //                                    foreach(var item in order.OrderTodayItems)
        //                                    {

        //                                    }
        //                                }
        //                            }

        //                            var itemsInUnprocessedOrder = sqliteDbContext.Items.Join(sqliteDbContext.ItemsInUnprocessedOrder,
        //                                item => item.Id,
        //                                itemInUnprocessedOrder => itemInUnprocessedOrder.ItemId,
        //                                (item, itemInUnprocessedOrder) => new { Item = item, ItemInUnprocessedOrder = itemInUnprocessedOrder })
        //                            .Where(item => item.ItemInUnprocessedOrder.UnprocessedOrderId == unprocessedOrders.Id);

        //                            if (itemsInUnprocessedOrder != null &&
        //                                itemsInUnprocessedOrder.Any())
        //                            {
        //                                foreach (var item in itemsInUnprocessedOrder)
        //                                {
        //                                    PorudzbinaItem porudzbinaItem = new PorudzbinaItem()
        //                                    {
        //                                        ItemIdString = item.Item.Id,
        //                                        Jm = item.Item.Jm,
        //                                        Kolicina = item.ItemInUnprocessedOrder.Quantity,
        //                                        Naziv = item.Item.Name,
        //                                        //BrojNarudzbe = porDB.TR_BROJNARUDZBE,
        //                                        MPC = item.Item.SellingUnitPrice,
        //                                        //Zelje = itemDB.TR_ZELJA,
        //                                        //RBS = itemDB.TR_RBS,
        //                                    };

        //                                    stoPorudzbina.Items.Add(porudzbinaItem);
        //                                }

        //                                stoPorudzbina.Sto.Color = "#ff2c2c";
        //                            }
        //                        }

        //                        //var porudzbineDB = sqliteDrljaDbContext.Narudzbine.Where(n => n.TR_STO == stoName &&
        //                        //n.TR_FAZA == 3);

        //                        //if (porudzbineDB != null &&
        //                        //porudzbineDB.Any())
        //                        //{
        //                        //    foreach(var porDB in porudzbineDB)
        //                        //    {
        //                        //        var itemsDB = sqliteDrljaDbContext.StavkeNarudzbine.Where(i => i.TR_BROJNARUDZBE == porDB.TR_BROJNARUDZBE);

        //                        //        if (itemsDB != null &&
        //                        //        itemsDB.Any())
        //                        //        {
        //                        //            foreach(var itemDB in itemsDB)
        //                        //            {
        //                        //                PorudzbinaItem porudzbinaItem = new PorudzbinaItem()
        //                        //                {
        //                        //                    ItemIdString = itemDB.TR_BRART,
        //                        //                    Jm = itemDB.TR_PAK,
        //                        //                    Kolicina = itemDB.TR_KOL,
        //                        //                    Naziv = itemDB.TR_NAZIV,
        //                        //                    BrojNarudzbe = porDB.TR_BROJNARUDZBE,
        //                        //                    MPC = itemDB.TR_MPC,
        //                        //                    Zelje = itemDB.TR_ZELJA,
        //                        //                    RBS = itemDB.TR_RBS,
        //                        //                };

        //                        //                stoPorudzbina.Items.Add(porudzbinaItem);
        //                        //            }

        //                        //            stoPorudzbina.Sto.Color = "#ff2c2c";
        //                        //        }
        //                        //    }
        //                        //}

        //                        deoSale.Stolovi.Add(stoPorudzbina);
        //                    }
        //                }

        //                deloviSale.Add(deoSale);
        //            }

        //            return Ok(deloviSale);
        //        }
        //        return NotFound();
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error("StoController -> StoPorudzbine -> Greska: ", ex);
        //        return BadRequest(ex.Message);
        //    }
        //}

        //[HttpPost("pay")]
        //public async Task<IActionResult> Pay(Sto sto)
        //{
        //    try
        //    {
        //        string stoString = $"S{sto.Name}";

        //        var porudzbineDB = sqliteDrljaDbContext.Narudzbine.Where(n => n.TR_STO == stoString &&
        //        n.TR_FAZA == 3);

        //        if (porudzbineDB != null &&
        //            porudzbineDB.Any())
        //        {
        //            foreach (var porDB in porudzbineDB)
        //            {
        //                porDB.TR_FAZA = 4;
        //                sqliteDrljaDbContext.Narudzbine.Update(porDB);
        //            }

        //            sqliteDrljaDbContext.SaveChanges();
        //        }

        //        return Ok();
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error("StoController -> Pay -> Greska: ", ex);
        //        return BadRequest(ex.Message);
        //    }
        //}

        //[HttpPost("moveOrder")]
        //public async Task<IActionResult> MoveOrder(MovePorudzbine movePorudzbine)
        //{
        //    try
        //    {
        //        if (movePorudzbine.FromSto.Name.HasValue &&
        //            movePorudzbine.ToSto.Name.HasValue)
        //        {
        //            var fromStoDB = _sqlServerDbContext.UnprocessedOrders.FirstOrDefault(order => order.PaymentPlaceId == movePorudzbine.FromSto.Name);
        //            var toStoDB = _sqlServerDbContext.UnprocessedOrders.FirstOrDefault(order => order.PaymentPlaceId == movePorudzbine.ToSto.Name);

        //            if (fromStoDB != null)
        //            {
        //                if (toStoDB != null)
        //                {
        //                    var itemsFromSto = _sqlServerDbContext.ItemsInUnprocessedOrder.Where(item => item.UnprocessedOrderId == fromStoDB.Id);
        //                    var itemsToSto = _sqlServerDbContext.ItemsInUnprocessedOrder.Where(item => item.UnprocessedOrderId == toStoDB.Id);

        //                    if (itemsFromSto != null &&
        //                        itemsFromSto.Any())
        //                    {
        //                        foreach (var item in itemsFromSto)
        //                        {
        //                            var itemInToSto = itemsToSto.FirstOrDefault(i => i.ItemId == item.ItemId);
        //                            if (itemInToSto != null)
        //                            {
        //                                itemInToSto.Quantity += item.Quantity;
        //                            }
        //                            else
        //                            {
        //                                item.UnprocessedOrderId = toStoDB.Id;
        //                            }
        //                            sqliteDbContext.ItemsInUnprocessedOrder.Update(item);
        //                        }
        //                    }

        //                    toStoDB.TotalAmount += fromStoDB.TotalAmount;
        //                    sqliteDbContext.UnprocessedOrders.Update(toStoDB);
        //                }
        //                else
        //                {
        //                    fromStoDB.PaymentPlaceId = movePorudzbine.ToSto.Name.Value;
        //                    _sqlServerDbContext.UnprocessedOrders.Update(fromStoDB);
        //                }
        //                sqliteDbContext.SaveChanges();
        //            }
        //        }
        //        return Ok();
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error("StoController -> MoveOrder -> Greska: ", ex);
        //        return BadRequest(ex.Message);
        //    }
        //}

        //[HttpPost("moveWorker")]
        //public async Task<IActionResult> MoveWorker(MoveKonobar moveKonobar)
        //{
        //    try
        //    {
        //        var porudzbineFromDB = sqliteDrljaDbContext.Narudzbine.Where(n => n.TR_RADNIK == moveKonobar.FromKonobarId &&
        //        n.TR_FAZA < 4 && n.TR_STO == moveKonobar.StoId);

        //        if (porudzbineFromDB != null &&
        //            porudzbineFromDB.Any())
        //        {
        //            foreach (var porDB in porudzbineFromDB)
        //            {
        //                porDB.TR_RADNIK = moveKonobar.ToKonobarId;
        //                sqliteDrljaDbContext.Narudzbine.Update(porDB);
        //            }

        //            sqliteDrljaDbContext.SaveChanges();
        //        }

        //        return Ok();
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error("StoController -> MoveOrder -> Greska: ", ex);
        //        return BadRequest(ex.Message);
        //    }
        //}
    }
}
