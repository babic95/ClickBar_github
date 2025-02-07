using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClickBar_Porudzbine.Server.Models;
using ClickBar_Database_Drlja;
using ClickBar_Database_Drlja.Models;
using System.Drawing.Printing;
using ClickBar_Logging;
using ClickBar_Database;
using ClickBar_Printer.Models.DrljaKuhinja;
using ClickBar_Printer;
using ClickBar_Database.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ClickBar_Common.Enums;
using ClickBar_Printer.PaperFormat;
using ClickBar_Settings;
using System.Drawing;

namespace ClickBar_Porudzbine.Server.Controllers
{
    [ApiController]
    [Route("api/porudzbina")]
    public class PorudzbinaController : ControllerBase
    {
        [HttpPost("create")]
        public IActionResult Index(Porudzbina porudzbina)
        {
            try
            {
                int rbs = 1;
                OprstiDB? opstiDB = null;
                //PorudzbinaPrint porudzbinaPrint = new PorudzbinaPrint();
                using (SqliteDrljaDbContext sqliteDrljaDbContext = new SqliteDrljaDbContext())
                {
                    opstiDB = sqliteDrljaDbContext.Oprsti.FirstOrDefault();

                    if (opstiDB != null)
                    {
                        while (opstiDB.ww_zakljucano == "D")
                        {
                            opstiDB = sqliteDrljaDbContext.Oprsti.FirstOrDefault();
                        }

                        opstiDB.ww_zakljucano = "D";
                        opstiDB.ww_zvuk = "D";
                        while (true)
                        {
                            try
                            {
                                RetryHelperDrlja.ExecuteWithRetry(() => { sqliteDrljaDbContext.SaveChanges(); });
                                break;
                            }
                            catch { }
                        }
                        opstiDB.ww_brojnarudzbe = opstiDB.ww_brojnarudzbe + 1;
                        sqliteDrljaDbContext.Oprsti.Update(opstiDB);
                        while (true)
                        {
                            try
                            {
                                RetryHelperDrlja.ExecuteWithRetry(() => { sqliteDrljaDbContext.SaveChanges(); });
                                break;
                            }
                            catch { }
                        }
                        opstiDB.ww_zakljucano = "N";
                        while (true)
                        {
                            try
                            {
                                RetryHelperDrlja.ExecuteWithRetry(() => { sqliteDrljaDbContext.SaveChanges(); });
                                break;
                            }
                            catch { }
                        }

                        if (sqliteDrljaDbContext.Narudzbine != null &&
                            sqliteDrljaDbContext.Narudzbine.Any())
                        {
                            var n = sqliteDrljaDbContext.Narudzbine.Max(a => a.TR_RBS);

                            rbs = n + 1;

                        }
                    }
                }

                if (opstiDB != null) {
                    DateTime porudzbinaDateTime = DateTime.Now;

                    ClickBar_Common.Models.Order.Order orderKuhinja = new ClickBar_Common.Models.Order.Order()
                    {
                        CashierName = porudzbina.RadnikName,
                        TableId = Convert.ToInt32(porudzbina.StoBr),
                        Items = new List<ClickBar_Common.Models.Order.ItemOrder>(),
                        OrderTime = porudzbinaDateTime,
                        OrderName = $"K_{opstiDB.ww_brojnarudzbe}"
                    };
                    ClickBar_Common.Models.Order.Order orderSank = new ClickBar_Common.Models.Order.Order()
                    {
                        CashierName = porudzbina.RadnikName,
                        TableId = Convert.ToInt32(porudzbina.StoBr),
                        Items = new List<ClickBar_Common.Models.Order.ItemOrder>(),
                        OrderTime = porudzbinaDateTime,
                        OrderName = "S"
                    };
                    ClickBar_Common.Models.Order.Order orderDrugo = new ClickBar_Common.Models.Order.Order()
                    {
                        CashierName = porudzbina.RadnikName,
                        TableId = Convert.ToInt32(porudzbina.StoBr),
                        Items = new List<ClickBar_Common.Models.Order.ItemOrder>(),
                        OrderTime = porudzbinaDateTime
                    };

                    UnprocessedOrderDB? unprocessedOrderDB = null;
                    using (SqliteDbContext sqliteDbContext = new SqliteDbContext())
                    {
                        unprocessedOrderDB = sqliteDbContext.UnprocessedOrders.FirstOrDefault(u => u.PaymentPlaceId == Convert.ToInt32(porudzbina.StoBr));

                        if (unprocessedOrderDB == null)
                        {
                            unprocessedOrderDB = new UnprocessedOrderDB()
                            {
                                PaymentPlaceId = Convert.ToInt32(porudzbina.StoBr),
                                CashierId = porudzbina.RadnikId,
                                Id = Guid.NewGuid().ToString(),
                                TotalAmount = 0,
                            };

                            sqliteDbContext.UnprocessedOrders.Add(unprocessedOrderDB);
                            RetryHelper.ExecuteWithRetry(() => { sqliteDbContext.SaveChanges(); });
                        }

                        var partHall = sqliteDbContext.PartHalls.Join(sqliteDbContext.PaymentPlaces,
                            partHall => partHall.Id,
                            table => table.PartHallId,
                            (partHall, table) => new { PartHall = partHall, Table = table })
                            .FirstOrDefault(t => t.Table.Id == Convert.ToInt32(porudzbina.StoBr));

                        if (partHall != null)
                        {
                            orderKuhinja.PartHall = partHall.PartHall.Name;
                            orderSank.PartHall = partHall.PartHall.Name;
                            orderDrugo.PartHall = partHall.PartHall.Name;
                        }
                    }

                    //porudzbinaPrint.Sto = $"S{porudzbina.StoBr}";
                    //porudzbinaPrint.PorudzbinaNumber = opstiDB.ww_brojnarudzbe;
                    //porudzbinaPrint.Worker = porudzbina.RadnikName;
                    //porudzbinaPrint.PorudzbinaDateTime = porudzbinaDateTime.ToString("dd.MM.yyyy HH:mm:ss");
                    //porudzbinaPrint.Items = new List<PorudzbinaItemPrint>();

                    NarudzbeDB? narudzbeDB = null;
                    Log.Debug($"Nova porudzbina od radnika {porudzbina.RadnikId} za smenu -> {(porudzbina.RadnikId == "1111" || porudzbina.RadnikId == "3333" ? "1" : "2")}");
                    using (SqliteDrljaDbContext sqliteDrljaDbContext = new SqliteDrljaDbContext())
                    {
                        narudzbeDB = new NarudzbeDB()
                        {
                            Id = Guid.NewGuid().ToString(),
                            TR_DATUM = porudzbinaDateTime.Date.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                            TR_RADNIK = porudzbina.RadnikId,
                            TR_STO = $"S{porudzbina.StoBr}",
                            TR_VREMENARUDZBE = porudzbinaDateTime,
                            TR_FAZA = 1,
                            TR_BROJNARUDZBE = opstiDB.ww_brojnarudzbe,
                            TR_RBS = rbs,
                            TR_SMENA = porudzbina.RadnikId == "1111" || porudzbina.RadnikId == "3333" ? "1" : "2",
                            TR_NARUDZBE_ID = !string.IsNullOrEmpty(porudzbina.PorudzbinaId) ? porudzbina.PorudzbinaId :
                                unprocessedOrderDB != null ? unprocessedOrderDB.Id : null
                        };

                        sqliteDrljaDbContext.Narudzbine.Add(narudzbeDB);
                        RetryHelperDrlja.ExecuteWithRetry(() => { sqliteDrljaDbContext.SaveChanges(); });
                    }

                    int orderCounter = 1;

                    using (SqliteDbContext sqliteDbContext = new SqliteDbContext())
                    {
                        var ordersTodayDB = sqliteDbContext.OrdersToday.Where(o => o.OrderDateTime.Date == DateTime.Now.Date);

                        if (ordersTodayDB != null &&
                            ordersTodayDB.Any())
                        {
                            orderCounter = ordersTodayDB.Max(o => o.Counter);
                            orderCounter++;
                        }

                        int rbs_item = 1;

                        var stavkaRbsMaxDB = 0;
                        using (SqliteDrljaDbContext sqliteDrljaDbContext = new SqliteDrljaDbContext())
                        {
                            if (sqliteDrljaDbContext.StavkeNarudzbine != null &&
                                    sqliteDrljaDbContext.StavkeNarudzbine.Any())
                            {
                                stavkaRbsMaxDB = sqliteDrljaDbContext.StavkeNarudzbine.Max(s => s.TR_RBS);
                            }

                            rbs_item = stavkaRbsMaxDB + 1;

                            porudzbina.Items.ForEach(item =>
                            {
                                var itemNadgroup = sqliteDbContext.Items.Join(sqliteDbContext.ItemGroups,
                                item => item.IdItemGroup,
                                itemGroup => itemGroup.Id,
                                (item, itemGroup) => new { Item = item, ItemGroup = itemGroup })
                                .Join(sqliteDbContext.Supergroups,
                                group => group.ItemGroup.IdSupergroup,
                                supergroup => supergroup.Id,
                                (group, supergroup) => new { Group = group, Supergroup = supergroup })
                                .FirstOrDefault(it => it.Group.Item.Id == item.ItemIdString);

                                if (itemNadgroup != null)
                                {
                                    if (itemNadgroup.Supergroup.Name.ToLower().Contains("hrana") ||
                                    itemNadgroup.Supergroup.Name.ToLower().Contains("kuhinja"))
                                    {
                                        orderKuhinja.Items.Add(new ClickBar_Common.Models.Order.ItemOrder()
                                        {
                                            Name = item.Naziv,
                                            Quantity = item.Kolicina,
                                            Id = item.ItemIdString,
                                            TotalAmount = decimal.Round(item.MPC * item.Kolicina, 2),
                                            Zelja = item.Zelje
                                        });
                                    }
                                    else
                                    {
                                        if (itemNadgroup.Supergroup.Name.ToLower().Contains("pice") ||
                                        itemNadgroup.Supergroup.Name.ToLower().Contains("piće") ||
                                        itemNadgroup.Supergroup.Name.ToLower().Contains("sank") ||
                                        itemNadgroup.Supergroup.Name.ToLower().Contains("šank"))
                                        {
                                            orderSank.Items.Add(new ClickBar_Common.Models.Order.ItemOrder()
                                            {
                                                Name = item.Naziv,
                                                Quantity = item.Kolicina,
                                                Id = item.ItemIdString,
                                                TotalAmount = decimal.Round(item.MPC * item.Kolicina, 2),
                                                Zelja = item.Zelje
                                            });
                                        }
                                        else
                                        {
                                            orderDrugo.OrderName = $"{itemNadgroup.Supergroup.Name[0]}";
                                            orderDrugo.Items.Add(new ClickBar_Common.Models.Order.ItemOrder()
                                            {
                                                Name = item.Naziv,
                                                Quantity = item.Kolicina,
                                                Id = item.ItemIdString,
                                                TotalAmount = decimal.Round(item.MPC * item.Kolicina, 2),
                                                Zelja = item.Zelje
                                            });
                                        }
                                    }
                                }
                                else
                                {
                                    orderSank.Items.Add(new ClickBar_Common.Models.Order.ItemOrder()
                                    {
                                        Name = item.Naziv,
                                        Quantity = item.Kolicina,
                                        Id = item.ItemIdString,
                                        TotalAmount = decimal.Round(item.MPC * item.Kolicina, 2),
                                        Zelja = item.Zelje
                                    });
                                }

                                var artDB = sqliteDrljaDbContext.Artikli.FirstOrDefault(i => i.TR_BRART == item.ItemIdString);

                                if (artDB == null)
                                {
                                    var artClickBarDB = sqliteDbContext.Items.Join(sqliteDbContext.ItemGroups,
                                        item => item.IdItemGroup,
                                        group => group.Id,
                                        (item, group) => new { Item = item, Group = group }).Join(sqliteDbContext.Supergroups,
                                        group => group.Group.IdSupergroup,
                                        supergroup => supergroup.Id,
                                        (group, supergroup) => new { Group = group, SuperGroup = supergroup }).FirstOrDefault(i => i.Group.Item.Id == item.ItemIdString);

                                    if (artClickBarDB != null)
                                    {
                                        var katDB = sqliteDrljaDbContext.Kategorije.FirstOrDefault(k => k.TR_KAT == artClickBarDB.Group.Group.Id);

                                        if (katDB == null)
                                        {
                                            katDB = new KategorijaDB()
                                            {
                                                TR_KAT = artClickBarDB.Group.Group.Id,
                                                TR_KATEGORIJA = artClickBarDB.Group.Group.Id.ToString("000000"),
                                                TR_NAZIV = artClickBarDB.Group.Group.Name
                                            };
                                            sqliteDrljaDbContext.Kategorije.Add(katDB);
                                            RetryHelperDrlja.ExecuteWithRetry(() => { sqliteDrljaDbContext.SaveChanges(); });
                                        }

                                        artDB = new ArtikliDB()
                                        {
                                            TR_BRART = artClickBarDB.Group.Item.Id,
                                            TR_NAZIV = artClickBarDB.Group.Item.Name,
                                            TR_PAK = artClickBarDB.Group.Item.Jm,
                                            TR_MPC = Convert.ToSingle(artClickBarDB.Group.Item.SellingUnitPrice),
                                            TR_ART = Convert.ToInt32(artClickBarDB.Group.Item.Id),
                                            TR_KATEGORIJA = artClickBarDB.Group.Group.Id,
                                            TR_VRSTA = artClickBarDB.SuperGroup == null ? "K" : artClickBarDB.SuperGroup.Name.Substring(0, 1) == "H" ? "K" : artClickBarDB.SuperGroup.Name.Substring(0, 1)
                                        };

                                        sqliteDrljaDbContext.Artikli.Add(artDB);
                                        RetryHelperDrlja.ExecuteWithRetry(() => { sqliteDrljaDbContext.SaveChanges(); });
                                    }
                                }

                                if (artDB != null)
                                {
                                    if (porudzbina.InsertInDB == null ||
                                    porudzbina.InsertInDB == true)
                                    {
                                        //PorudzbinaItemPrint porudzbinaItemPrint = new PorudzbinaItemPrint()
                                        //{
                                        //    Name = item.Naziv,
                                        //    Quantity = item.Kolicina,
                                        //    Price = item.MPC,
                                        //    Zelje = item.Zelje,
                                        //    Type = ClickBar_Printer.Enums.OrderTypeEnumeration.Kuhinja
                                        //    //Type = artDB.TR_VRSTA == "K" ? Porudzbine_Printer.Enums.OrderTypeEnumeration.Kuhinja :
                                        //    //Porudzbine_Printer.Enums.OrderTypeEnumeration.Sank
                                        //};
                                        //porudzbinaPrint.Items.Add(porudzbinaItemPrint);

                                        var unprocessedOrderItemDB = sqliteDbContext.ItemsInUnprocessedOrder.FirstOrDefault(i => i.UnprocessedOrderId == unprocessedOrderDB.Id &&
                                        i.ItemId == item.ItemIdString);

                                        if (unprocessedOrderItemDB == null)
                                        {
                                            unprocessedOrderItemDB = new ItemInUnprocessedOrderDB()
                                            {
                                                ItemId = item.ItemIdString,
                                                UnprocessedOrderId = unprocessedOrderDB.Id,
                                                Quantity = item.Kolicina,
                                            };
                                            sqliteDbContext.ItemsInUnprocessedOrder.Add(unprocessedOrderItemDB);
                                        }
                                        else
                                        {
                                            unprocessedOrderItemDB.Quantity += item.Kolicina;
                                            sqliteDbContext.ItemsInUnprocessedOrder.Update(unprocessedOrderItemDB);
                                        }
                                        unprocessedOrderDB.TotalAmount += decimal.Round(item.Kolicina * item.MPC, 2);
                                        sqliteDbContext.UnprocessedOrders.Update(unprocessedOrderDB);
                                        RetryHelper.ExecuteWithRetry(() => { sqliteDbContext.SaveChanges(); });
                                    }

                                    StavkeNarudzbeDB stavkeNarudzbeDB = new StavkeNarudzbeDB()
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        TR_KOL = item.Kolicina,
                                        TR_BRART = item.ItemIdString,
                                        TR_BROJNARUDZBE = narudzbeDB.TR_BROJNARUDZBE,
                                        TR_MPC = item.MPC,
                                        TR_NAZIV = item.Naziv,
                                        TR_PAK = item.Jm,
                                        TR_ZELJA = item.Zelje,
                                        TR_RBS = rbs_item++,
                                        TR_KOL_STORNO = 0,
                                        TR_NARUDZBE_ID = unprocessedOrderDB != null ? unprocessedOrderDB.Id : null
                                    };

                                    sqliteDrljaDbContext.StavkeNarudzbine.Add(stavkeNarudzbeDB);
                                }
                            });
                            RetryHelper.ExecuteWithRetry(() => { sqliteDbContext.SaveChanges(); });
                            RetryHelperDrlja.ExecuteWithRetry(() => { sqliteDrljaDbContext.SaveChanges(); });
                        }
                    }
                    try
                    {
                        using (SqliteDbContext sqliteDbContext = new SqliteDbContext())
                        {
                            var posType = SettingsManager.Instance.GetPrinterFormat() == PrinterFormatEnumeration.Pos80mm ?
                ClickBar_Printer.Enums.PosTypeEnumeration.Pos80mm : ClickBar_Printer.Enums.PosTypeEnumeration.Pos58mm;
                            if (orderSank.Items.Any())
                            {
                                if (porudzbina.InsertInDB == null ||
                                    porudzbina.InsertInDB == true)
                                {
                                    int orderCounterType = 1;

                                    var ordersTodayTypeDB = sqliteDbContext.OrdersToday.Where(o => o.OrderDateTime.Date == DateTime.Now.Date &&
                                    !string.IsNullOrEmpty(o.Name) &&
                                    o.Name.ToLower().Contains(orderSank.OrderName.ToLower()));

                                    if (ordersTodayTypeDB != null &&
                                        ordersTodayTypeDB.Any())
                                    {
                                        orderCounterType = ordersTodayTypeDB.Max(o => o.CounterType);
                                        orderCounterType++;
                                    }

                                    orderSank.OrderName += orderCounterType.ToString() + "_" + orderCounter.ToString();

                                    decimal totalAmount = orderSank.Items.Sum(i => i.TotalAmount);

                                    OrderTodayDB orderTodayDB = new OrderTodayDB()
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        CashierId = porudzbina.RadnikId,
                                        Counter = orderCounter,
                                        CounterType = orderCounterType,
                                        OrderDateTime = DateTime.Now,
                                        TotalPrice = totalAmount,
                                        Name = orderSank.OrderName,
                                        TableId = Convert.ToInt32(porudzbina.StoBr)
                                    };

                                    sqliteDbContext.OrdersToday.Add(orderTodayDB);
                                    RetryHelper.ExecuteWithRetry(() => { sqliteDbContext.SaveChanges(); });

                                    orderSank.Items.ForEach(item =>
                                    {
                                        OrderTodayItemDB orderTodayItemDB = new OrderTodayItemDB()
                                        {
                                            ItemId = item.Id,
                                            OrderTodayId = orderTodayDB.Id,
                                            Quantity = item.Quantity,
                                            TotalPrice = item.TotalAmount,
                                        };
                                        sqliteDbContext.OrderTodayItems.Add(orderTodayItemDB);
                                    });
                                    RetryHelper.ExecuteWithRetry(() => { sqliteDbContext.SaveChanges(); });
                                }

                                FormatPos.PrintOrder(orderSank, posType, ClickBar_Printer.Enums.OrderTypeEnumeration.Sank);
                            }
                            if (orderKuhinja.Items.Any())
                            {
                                if (porudzbina.InsertInDB == null ||
                                    porudzbina.InsertInDB == true)
                                {
                                    int orderCounterType = 1;

                                    var ordersTodayTypeDB = sqliteDbContext.OrdersToday.Where(o => o.OrderDateTime.Date == DateTime.Now.Date &&
                                    !string.IsNullOrEmpty(o.Name) &&
                                    o.Name.ToLower().Contains(orderKuhinja.OrderName.ToLower()));

                                    if (ordersTodayTypeDB != null &&
                                        ordersTodayTypeDB.Any())
                                    {
                                        orderCounterType = ordersTodayTypeDB.Max(o => o.CounterType);
                                        orderCounterType++;
                                    }

                                    orderKuhinja.OrderName += orderCounterType.ToString() + "_" + orderCounter.ToString();

                                    decimal totalAmount = orderKuhinja.Items.Sum(i => i.TotalAmount);

                                    OrderTodayDB orderTodayDB = new OrderTodayDB()
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        CashierId = porudzbina.RadnikId,
                                        Counter = orderCounter,
                                        CounterType = orderCounterType,
                                        OrderDateTime = DateTime.Now,
                                        TotalPrice = totalAmount,
                                        Name = orderKuhinja.OrderName,
                                        TableId = Convert.ToInt32(porudzbina.StoBr)
                                    };

                                    sqliteDbContext.OrdersToday.Add(orderTodayDB);
                                    RetryHelper.ExecuteWithRetry(() => { sqliteDbContext.SaveChanges(); });

                                    orderKuhinja.Items.ForEach(item =>
                                    {
                                        var orderTodayItemDB = sqliteDbContext.OrderTodayItems.FirstOrDefault(o => o.ItemId == item.Id &&
                                        o.OrderTodayId == orderTodayDB.Id);

                                        if (orderTodayItemDB != null)
                                        {
                                            orderTodayItemDB.Quantity += item.Quantity;
                                            orderTodayItemDB.TotalPrice += item.TotalAmount;
                                            sqliteDbContext.OrderTodayItems.Update(orderTodayItemDB);
                                        }
                                        else
                                        {
                                            orderTodayItemDB = new OrderTodayItemDB()
                                            {
                                                ItemId = item.Id,
                                                OrderTodayId = orderTodayDB.Id,
                                                Quantity = item.Quantity,
                                                TotalPrice = item.TotalAmount,
                                            };
                                            sqliteDbContext.OrderTodayItems.Add(orderTodayItemDB);
                                        }
                                        RetryHelper.ExecuteWithRetry(() => { sqliteDbContext.SaveChanges(); });
                                    });
                                }
                                FormatPos.PrintOrder(orderKuhinja, posType, ClickBar_Printer.Enums.OrderTypeEnumeration.Kuhinja);
                            }
                            if (orderDrugo.Items.Any())
                            {
                                if (porudzbina.InsertInDB == null ||
                                    porudzbina.InsertInDB == true)
                                {
                                    int orderCounterType = 1;

                                    var ordersTodayTypeDB = sqliteDbContext.OrdersToday.Where(o => o.OrderDateTime.Date == DateTime.Now.Date &&
                                    !string.IsNullOrEmpty(o.Name) &&
                                    o.Name.ToLower().Contains(orderDrugo.OrderName.ToLower()));

                                    if (ordersTodayTypeDB != null &&
                                        ordersTodayTypeDB.Any())
                                    {
                                        orderCounterType = ordersTodayTypeDB.Max(o => o.CounterType);
                                        orderCounterType++;
                                    }

                                    orderDrugo.OrderName += orderCounterType.ToString() + "_" + orderCounter.ToString();

                                    decimal totalAmount = orderDrugo.Items.Sum(i => i.TotalAmount);

                                    OrderTodayDB orderTodayDB = new OrderTodayDB()
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        CashierId = porudzbina.RadnikId,
                                        Counter = orderCounter,
                                        CounterType = orderCounterType,
                                        OrderDateTime = DateTime.Now,
                                        TotalPrice = totalAmount,
                                        Name = orderDrugo.OrderName,
                                        TableId = Convert.ToInt32(porudzbina.StoBr)
                                    };

                                    sqliteDbContext.OrdersToday.Add(orderTodayDB);
                                    RetryHelper.ExecuteWithRetry(() => { sqliteDbContext.SaveChanges(); });

                                    orderDrugo.Items.ForEach(item =>
                                    {
                                        OrderTodayItemDB orderTodayItemDB = new OrderTodayItemDB()
                                        {
                                            ItemId = item.Id,
                                            OrderTodayId = orderTodayDB.Id,
                                            Quantity = item.Quantity,
                                            TotalPrice = item.TotalAmount,
                                        };
                                        sqliteDbContext.OrderTodayItems.Add(orderTodayItemDB);
                                    });
                                    RetryHelper.ExecuteWithRetry(() => { sqliteDbContext.SaveChanges(); });
                                }
                                FormatPos.PrintOrder(orderDrugo, posType, ClickBar_Printer.Enums.OrderTypeEnumeration.Sank);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error("PorudzbinaController -> Greska prilikom stampe: ", ex);

                        //foreach (string printer in PrinterSettings.InstalledPrinters)
                        //{
                        //    Log.Debug($"PorudzbinaController -> STAMPAC {printer} ");
                        //}
                    }

                    return Ok();
                }

                Log.Error("PorudzbinaController -> Greska prilikom kreiranja porudzbine. Nema OPSTI tabele u bazi");
                return BadRequest("Greška u komunikaciji sa bazom");
            }
                
            
            catch (Exception ex)
            {
                Log.Error("PorudzbinaController -> Greska: ", ex);
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("stornoKuhinja")]
        public IActionResult StornoKuhinja(Porudzbina porudzbina)
        {
            Log.Debug("PorudzbinaController -> StornoKuhinja -> usao u stornoKuhinja: ");
            try
            {
                using (SqliteDrljaDbContext sqliteDrljaDbContext = new SqliteDrljaDbContext())
                {

                    if (!string.IsNullOrEmpty(porudzbina.PorudzbinaId))
                    {
                        var narudzbine = sqliteDrljaDbContext.StavkeNarudzbine
                            .Where(x => x.TR_NARUDZBE_ID == porudzbina.PorudzbinaId);

                        if (narudzbine != null &&
                            porudzbina.Items != null &&
                            porudzbina.Items.Any())
                        {
                            ClickBar_Common.Models.Order.Order orderKuhinja = new ClickBar_Common.Models.Order.Order()
                            {
                                CashierName = porudzbina.RadnikId,
                                TableId = -1,
                                Items = new List<ClickBar_Common.Models.Order.ItemOrder>(),
                                OrderTime = DateTime.Now,
                                OrderName = $"K",
                                PartHall = "storno"
                            };

                            porudzbina.Items.ForEach(item =>
                            {
                                orderKuhinja.Items.Add(new ClickBar_Common.Models.Order.ItemOrder
                                {
                                    Id = item.ItemIdString,
                                    Name = item.Naziv,
                                    Quantity = -1 * item.Kolicina
                                });
                                decimal quantity = item.Kolicina;

                                var itemsInNarudbina = narudzbine.Where(x => x.TR_BRART == item.ItemIdString);

                                if (itemsInNarudbina != null &&
                                    itemsInNarudbina.Any())
                                {
                                    foreach (var i in itemsInNarudbina)
                                    {
                                        if (quantity == 0)
                                        {
                                            break;
                                        }

                                        var narudzbinaDB = sqliteDrljaDbContext.Narudzbine.FirstOrDefault(n => n.TR_BROJNARUDZBE == i.TR_BROJNARUDZBE);

                                        if (narudzbinaDB != null)
                                        {
                                            // Proverite da li string počinje sa "S"
                                            if (narudzbinaDB.TR_STO.StartsWith("S"))
                                            {
                                                // Izvucite deo stringa nakon "S"
                                                string numberPart = narudzbinaDB.TR_STO.Substring(1);

                                                // Pokušajte da konvertujete deo stringa u broj
                                                if (int.TryParse(numberPart, out int stoId))
                                                {
                                                    orderKuhinja.TableId = stoId;
                                                }
                                                else
                                                {
                                                    Log.Error("PorudzbinaController -> StornoKuhinja -> Greska prilikom konvertovanja stringa u broj za broj stola.");
                                                }
                                            }
                                            else
                                            {
                                                Log.Error("PorudzbinaController -> StornoKuhinja -> String ne počinje sa 'S' za broj stola iz Drljine baze.");
                                            }

                                            narudzbinaDB.TR_STORNORAZLOG = $"Rucno storniranje konobara {porudzbina.RadnikName}";
                                            sqliteDrljaDbContext.Narudzbine.Update(narudzbinaDB);

                                            orderKuhinja.OrderName += $"_{i.TR_BROJNARUDZBE}";

                                            if (quantity >= i.TR_KOL - i.TR_KOL_STORNO)
                                            {
                                                quantity -= i.TR_KOL - i.TR_KOL_STORNO;
                                                i.TR_KOL_STORNO = i.TR_KOL;
                                            }
                                            else
                                            {
                                                i.TR_KOL_STORNO = quantity;
                                                quantity = 0;
                                            }
                                            sqliteDrljaDbContext.StavkeNarudzbine.Update(i);
                                            RetryHelperDrlja.ExecuteWithRetry(() => { sqliteDrljaDbContext.SaveChanges(); });
                                        }
                                    }
                                }
                            });

                            if (orderKuhinja.Items.Any())
                            {
                                var posType = SettingsManager.Instance.GetPrinterFormat() == PrinterFormatEnumeration.Pos80mm ?
                                ClickBar_Printer.Enums.PosTypeEnumeration.Pos80mm : ClickBar_Printer.Enums.PosTypeEnumeration.Pos58mm;

                                FormatPos.PrintOrder(orderKuhinja, posType, ClickBar_Printer.Enums.OrderTypeEnumeration.Kuhinja);
                            }
                        }
                    }

                    return Ok();
                }
            }
            catch (Exception ex)
            {
                Log.Error("PorudzbinaController -> StornoKuhinja -> Greska: ", ex);
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("movePorudzbina")]
        public IActionResult MovePorudzbina(MovePorudzbinaClickBar movePorudzbinaClickBar)
        {
            try
            {
                using (SqliteDrljaDbContext sqliteDrljaDbContext = new SqliteDrljaDbContext())
                {
                    var newNarudzbina = sqliteDrljaDbContext.Narudzbine.FirstOrDefault(n => n.TR_NARUDZBE_ID == movePorudzbinaClickBar.NewUnprocessedOrderId);
                    var oldNarudzbina = sqliteDrljaDbContext.Narudzbine.FirstOrDefault(n => n.TR_NARUDZBE_ID == movePorudzbinaClickBar.OldUnprocessedOrderId);

                    if (oldNarudzbina != null)
                    {
                        if (newNarudzbina == null)
                        {
                            newNarudzbina = new NarudzbeDB()
                            {
                                Id = Guid.NewGuid().ToString(),
                                TR_BROJNARUDZBE = oldNarudzbina.TR_BROJNARUDZBE,
                                TR_FAZA = oldNarudzbina.TR_FAZA,
                                TR_DATUM = oldNarudzbina.TR_DATUM,
                                TR_NARUDZBE_ID = movePorudzbinaClickBar.NewUnprocessedOrderId,
                                TR_RADNIK = oldNarudzbina.TR_RADNIK,
                                TR_RBS = oldNarudzbina.TR_RBS,
                                TR_STO = $"S{movePorudzbinaClickBar.NewSto}",
                                TR_VREMENARUDZBE = oldNarudzbina.TR_VREMENARUDZBE,
                            };

                            sqliteDrljaDbContext.Narudzbine.Add(newNarudzbina);
                            RetryHelperDrlja.ExecuteWithRetry(() => { sqliteDrljaDbContext.SaveChanges(); });
                        }

                        var itemsInOldNarudzbina = sqliteDrljaDbContext.StavkeNarudzbine.Where(s => s.TR_NARUDZBE_ID == movePorudzbinaClickBar.OldUnprocessedOrderId);

                        if (itemsInOldNarudzbina != null &&
                            itemsInOldNarudzbina.Any())
                        {
                            if (movePorudzbinaClickBar.Items != null &&
                                movePorudzbinaClickBar.Items.Any())
                            {
                                movePorudzbinaClickBar.Items.ForEach(item =>
                                {
                                    decimal quantity = item.Kolicina;

                                    var itemsInNarudbina = itemsInOldNarudzbina.Where(x => x.TR_BRART == item.ItemIdString);

                                    if (itemsInNarudbina != null &&
                                        itemsInNarudbina.Any())
                                    {
                                        foreach (var i in itemsInNarudbina)
                                        {
                                            if (quantity == 0)
                                            {
                                                break;
                                            }

                                            if (quantity >= i.TR_KOL)
                                            {
                                                quantity -= i.TR_KOL;
                                                i.TR_NARUDZBE_ID = movePorudzbinaClickBar.NewUnprocessedOrderId;
                                            }
                                            else
                                            {
                                                i.TR_KOL -= quantity;

                                                StavkeNarudzbeDB stavkeNarudzbeDB = new StavkeNarudzbeDB()
                                                {
                                                    Id = Guid.NewGuid().ToString(),
                                                    TR_BRART = i.TR_BRART,
                                                    TR_BROJNARUDZBE = i.TR_BROJNARUDZBE,
                                                    TR_KOL = quantity,
                                                    TR_KOL_STORNO = 0,
                                                    TR_MPC = i.TR_MPC,
                                                    TR_NARUDZBE_ID = movePorudzbinaClickBar.NewUnprocessedOrderId,
                                                    TR_NAZIV = i.TR_NAZIV,
                                                    TR_PAK = i.TR_PAK,
                                                    TR_RBS = i.TR_RBS,
                                                    TR_ZELJA = i.TR_ZELJA
                                                };

                                                sqliteDrljaDbContext.StavkeNarudzbine.Add(stavkeNarudzbeDB);

                                                quantity = 0;
                                            }
                                            sqliteDrljaDbContext.StavkeNarudzbine.Update(i);
                                            RetryHelperDrlja.ExecuteWithRetry(() => { sqliteDrljaDbContext.SaveChanges(); });
                                        }
                                    }
                                });
                            }
                        }
                    }
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                Log.Error("PorudzbinaController -> StornoKuhinja -> Greska: ", ex);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("checkIsFinish")]
        public IActionResult CheckIsFinish(User user)
        {
            try
            {
                SqliteDrljaDbContext sqliteDbContext = new SqliteDrljaDbContext();

                List<Porudzbina> notifications = new List<Porudzbina>();

                var porudzbineDB = sqliteDbContext.Narudzbine.Where(n => n.TR_RADNIK == user.Id &&
                n.TR_FAZA == 2);

                if (porudzbineDB != null &&
                    porudzbineDB.Any())
                {
                    porudzbineDB.ForEachAsync(n =>
                    {
                        var itemsDB = sqliteDbContext.StavkeNarudzbine.Where(i => i.TR_BROJNARUDZBE == n.TR_BROJNARUDZBE);

                        if(itemsDB != null &&
                        itemsDB.Any())
                        {
                            Porudzbina porudzbina = new Porudzbina()
                            {
                                BrPorudzbine = n.TR_BROJNARUDZBE,
                                StoBr = n.TR_STO,
                                RadnikId = n.TR_RADNIK,
                                Items = new List<PorudzbinaItem>()
                            };

                            itemsDB.ForEachAsync(itemDB =>
                            {
                                PorudzbinaItem porudzbinaItem = new PorudzbinaItem()
                                {
                                    ItemIdString = itemDB.TR_BRART,
                                    Jm = itemDB.TR_PAK,
                                    Kolicina = itemDB.TR_KOL,
                                    Naziv = itemDB.TR_NAZIV,
                                    BrojNarudzbe = n.TR_BROJNARUDZBE,
                                    MPC = itemDB.TR_MPC,
                                    Zelje = itemDB.TR_ZELJA,
                                    RBS = itemDB.TR_RBS,
                                };

                                porudzbina.Items.Add(porudzbinaItem);
                            });

                            notifications.Add(porudzbina);
                        }
                    });
                }

                return Ok(notifications);
            }
            catch (Exception ex)
            {
                Log.Error("PorudzbinaController -> CheckIsFinish -> Greska: ", ex);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("seenOrder")]
        public IActionResult SeenOrder(Porudzbina porudzbina)
        {
            try
            {
                SqliteDrljaDbContext sqliteDbContext = new SqliteDrljaDbContext();

                var porDB = sqliteDbContext.Narudzbine.FirstOrDefault(n => n.TR_BROJNARUDZBE == porudzbina.BrPorudzbine &&
                n.TR_FAZA == 2);

                if(porDB != null)
                {
                    porDB.TR_FAZA = 3;
                    sqliteDbContext.Narudzbine.Update(porDB);
                    RetryHelper.ExecuteWithRetry(() => { sqliteDbContext.SaveChanges(); });
                }

                return Ok();
            }
            catch (Exception ex)
            {
                Log.Error("PorudzbinaController -> SeenOrder -> Greska: ", ex);
                return BadRequest(ex.Message);
            }
        }
    }
}
