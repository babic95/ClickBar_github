using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClickBar_Porudzbine.Server.Models;
using System.Drawing.Printing;
using ClickBar_Logging;
using ClickBar_DatabaseSQLManager;
using ClickBar_Printer.Models.DrljaKuhinja;
using ClickBar_Printer;
using ClickBar_DatabaseSQLManager.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ClickBar_Common.Enums;
using ClickBar_Printer.PaperFormat;
using ClickBar_Settings;
using System.Drawing;
using ClickBar_Porudzbine.Server.Enums;

namespace ClickBar_Porudzbine.Server.Controllers
{
    [ApiController]
    [Route("api/porudzbina")]
    public class PorudzbinaController : ControllerBase
    {
        private readonly SqlServerDbContext _sqlServerDbContext;

        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public PorudzbinaController(SqlServerDbContext sqlServerDbContext)
        {
            _sqlServerDbContext = sqlServerDbContext;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Index(Porudzbina porudzbina)
        {
            try
            {
                var tableDB = await _sqlServerDbContext.PaymentPlaces.FindAsync(porudzbina.StoId);

                if (tableDB != null && tableDB.Popust > 0 && tableDB.Popust < 100)
                {
                    var itemIds = porudzbina.Items.Select(x => x.ItemId).ToList();

                    bool postojiZabranjen = await _sqlServerDbContext.Items
                        .AnyAsync(i => itemIds.Contains(i.Id) && i.IsCheckedZabraniPopust == 1);

                    if (postojiZabranjen)
                    {
                        return BadRequest("Na ovaj sto ne možete staviti artikle koji su označeni kao ZABRANJEN popust!");
                    }
                }

                var unprocessedOrderDB = await _sqlServerDbContext.UnprocessedOrders
                    .FirstOrDefaultAsync(table => table.PaymentPlaceId == porudzbina.StoId);

                if (unprocessedOrderDB != null)
                {
                    //decimal totalSum = _viewModel.ItemsInvoice.Sum(i => i.TotalAmout);

                    //unprocessedOrderDB.TotalAmount += Decimal.Round(totalSum * ((100 - tableDB.Popust) / 100), 2);

                    unprocessedOrderDB.CashierId = porudzbina.RadnikId;
                    _sqlServerDbContext.UnprocessedOrders.Update(unprocessedOrderDB);
                }
                else
                {
                    unprocessedOrderDB = new UnprocessedOrderDB()
                    {
                        Id = Guid.NewGuid().ToString(),
                        CashierId = porudzbina.RadnikId,
                        PaymentPlaceId = porudzbina.StoId,
                        //TotalAmount = _viewModel.ItemsInvoice.Sum(i => i.TotalAmout),
                        ItemsInUnprocessedOrder = new List<ItemInUnprocessedOrderDB>(),
                    };

                    await _sqlServerDbContext.UnprocessedOrders.AddAsync(unprocessedOrderDB);
                }

                bool saveFailed;
                int retryCount = 0;
                do
                {
                    saveFailed = false;
                    try
                    {
                        await _sqlServerDbContext.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        saveFailed = true;
                        retryCount++;

                        // Update the values of the entity that failed to save from the store
                        ex.Entries.Single().Reload();

                        if (retryCount >= 3)
                        {
                            throw;
                        }
                    }
                } while (saveFailed);

                var loggedCashier = await _sqlServerDbContext.Cashiers.FirstOrDefaultAsync(c => c.Id == porudzbina.RadnikId);
                var tableId = porudzbina.StoId;
                var paymentPlace = await _sqlServerDbContext.PaymentPlaces.Include(p => p.PartHall).FirstOrDefaultAsync(p => p.Id == tableId);
                var itemsInvoice = porudzbina.Items.ToList();
                var unprocessedOrder = unprocessedOrderDB;

                await PrintOrder(loggedCashier, paymentPlace, itemsInvoice, unprocessedOrder);

                return Ok();
            }
            catch (Exception ex)
            {
                Log.Error("PorudzbinaController -> Index -> Greska: ", ex);
                return BadRequest(ex.Message);
            }
        }
        //[HttpPost("stornoKuhinja")]
        //public async Task<IActionResult> StornoKuhinja(Porudzbina porudzbina)
        //{
        //    await _semaphore.WaitAsync();
        //    IActionResult result = NoContent();
        //    var strategy = sqliteDrljaDbContext.Database.CreateExecutionStrategy();
        //    await strategy.ExecuteAsync(async () =>
        //    {
        //        await using var transaction = await sqliteDrljaDbContext.Database.BeginTransactionAsync(); // Koristimo SQL Server bazu
        //        Log.Debug("PorudzbinaController -> StornoKuhinja -> usao u stornoKuhinja: ");
        //        try
        //        {
        //            if (!string.IsNullOrEmpty(porudzbina.PorudzbinaId))
        //            {
        //                var narudzbine = sqliteDrljaDbContext.StavkeNarudzbine
        //                    .Where(x => x.TR_NARUDZBE_ID == porudzbina.PorudzbinaId);

        //                if (narudzbine != null &&
        //                    porudzbina.Items != null &&
        //                    porudzbina.Items.Any())
        //                {
        //                    ClickBar_Common.Models.Order.Order orderKuhinja = new ClickBar_Common.Models.Order.Order()
        //                    {
        //                        CashierName = porudzbina.RadnikId,
        //                        TableId = -1,
        //                        Items = new List<ClickBar_Common.Models.Order.ItemOrder>(),
        //                        OrderTime = DateTime.Now,
        //                        OrderName = $"K",
        //                        PartHall = "storno"
        //                    };

        //                    foreach (var item in porudzbina.Items)
        //                    {
        //                        orderKuhinja.Items.Add(new ClickBar_Common.Models.Order.ItemOrder
        //                        {
        //                            Id = item.ItemIdString,
        //                            Name = item.Naziv,
        //                            Quantity = -1 * item.Kolicina
        //                        });
        //                        decimal quantity = item.Kolicina;

        //                        var itemsInNarudbina = narudzbine.Where(x => x.TR_BRART == item.ItemIdString);

        //                        if (itemsInNarudbina != null &&
        //                            itemsInNarudbina.Any())
        //                        {
        //                            foreach (var i in itemsInNarudbina)
        //                            {
        //                                if (quantity == 0)
        //                                {
        //                                    break;
        //                                }

        //                                var narudzbinaDB = await sqliteDrljaDbContext.Narudzbine.FirstOrDefaultAsync(n => n.TR_BROJNARUDZBE == i.TR_BROJNARUDZBE);

        //                                if (narudzbinaDB != null)
        //                                {
        //                                    // Proverite da li string počinje sa "S"
        //                                    if (narudzbinaDB.TR_STO.StartsWith("S"))
        //                                    {
        //                                        // Izvucite deo stringa nakon "S"
        //                                        string numberPart = narudzbinaDB.TR_STO.Substring(1);

        //                                        // Pokušajte da konvertujete deo stringa u broj
        //                                        if (int.TryParse(numberPart, out int stoId))
        //                                        {
        //                                            orderKuhinja.TableId = stoId;
        //                                        }
        //                                        else
        //                                        {
        //                                            Log.Error("PorudzbinaController -> StornoKuhinja -> Greska prilikom konvertovanja stringa u broj za broj stola.");
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        Log.Error("PorudzbinaController -> StornoKuhinja -> String ne počinje sa 'S' za broj stola iz Drljine baze.");
        //                                    }

        //                                    narudzbinaDB.TR_STORNORAZLOG = $"Rucno storniranje konobara {porudzbina.RadnikName}";
        //                                    sqliteDrljaDbContext.Narudzbine.Update(narudzbinaDB);

        //                                    orderKuhinja.OrderName += $"_{i.TR_BROJNARUDZBE}";

        //                                    if (quantity >= i.TR_KOL - i.TR_KOL_STORNO)
        //                                    {
        //                                        quantity -= i.TR_KOL - i.TR_KOL_STORNO;
        //                                        i.TR_KOL_STORNO = i.TR_KOL;
        //                                    }
        //                                    else
        //                                    {
        //                                        i.TR_KOL_STORNO = quantity;
        //                                        quantity = 0;
        //                                    }
        //                                    sqliteDrljaDbContext.StavkeNarudzbine.Update(i);
        //                                    Log.Debug($"Pre save: STORNO JE {i.TR_KOL_STORNO}");
        //                                    await sqliteDrljaDbContext.SaveChangesAsync();
        //                                    Log.Debug($"Posle save: STORNO JE {i.TR_KOL_STORNO}");

        //                                    Log.Debug("Usresno sacuvan storno");
        //                                }
        //                            }
        //                        }
        //                    }

        //                    if (orderKuhinja.Items.Any())
        //                    {
        //                        var posType = SettingsManager.Instance.GetPrinterFormat() == PrinterFormatEnumeration.Pos80mm ?
        //                        ClickBar_Printer.Enums.PosTypeEnumeration.Pos80mm : ClickBar_Printer.Enums.PosTypeEnumeration.Pos58mm;

        //                        FormatPos.PrintOrder(orderKuhinja, posType, ClickBar_Printer.Enums.OrderTypeEnumeration.Kuhinja);
        //                    }
        //                }
        //            }

        //            await transaction.CommitAsync(); // Commit transakcije
        //            result = Ok();
        //        }
        //        catch (Exception ex)
        //        {
        //            await transaction.RollbackAsync(); // Rollback transakcije u slučaju greške
        //            Log.Error("PorudzbinaController -> StornoKuhinja -> Greska: ", ex);
        //            result = BadRequest(ex.Message);
        //        }
        //        finally
        //        {
        //            _semaphore.Release();
        //        }
        //    });
        //    return result;
        //}
        //[HttpPost("movePorudzbina")]
        //public async Task<IActionResult> MovePorudzbina(MovePorudzbinaClickBar movePorudzbinaClickBar)
        //{
        //    await _semaphore.WaitAsync();
        //    IActionResult result = NoContent();
        //    var strategy = sqliteDrljaDbContext.Database.CreateExecutionStrategy();
        //    await strategy.ExecuteAsync(async () =>
        //    {
        //        await using var transaction = await sqliteDrljaDbContext.Database.BeginTransactionAsync(); // Koristimo SQL Server bazu
        //        try
        //        {
        //            var newNarudzbina = await sqliteDrljaDbContext.Narudzbine.FirstOrDefaultAsync(n => n.TR_NARUDZBE_ID == movePorudzbinaClickBar.NewUnprocessedOrderId);
        //            var oldNarudzbina = await sqliteDrljaDbContext.Narudzbine.FirstOrDefaultAsync(n => n.TR_NARUDZBE_ID == movePorudzbinaClickBar.OldUnprocessedOrderId);

        //            if (oldNarudzbina != null)
        //            {
        //                if (newNarudzbina == null)
        //                {
        //                    newNarudzbina = new NarudzbeDB()
        //                    {
        //                        Id = Guid.NewGuid().ToString(),
        //                        TR_BROJNARUDZBE = oldNarudzbina.TR_BROJNARUDZBE,
        //                        TR_FAZA = oldNarudzbina.TR_FAZA,
        //                        TR_DATUM = oldNarudzbina.TR_DATUM,
        //                        TR_NARUDZBE_ID = movePorudzbinaClickBar.NewUnprocessedOrderId,
        //                        TR_RADNIK = oldNarudzbina.TR_RADNIK,
        //                        TR_RBS = oldNarudzbina.TR_RBS,
        //                        TR_STO = $"S{movePorudzbinaClickBar.NewSto}",
        //                        TR_VREMENARUDZBE = oldNarudzbina.TR_VREMENARUDZBE,
        //                        TR_SMENA = oldNarudzbina.TR_SMENA
        //                    };

        //                    sqliteDrljaDbContext.Narudzbine.Add(newNarudzbina);
        //                    await sqliteDrljaDbContext.SaveChangesAsync();
        //                }

        //                var itemsInOldNarudzbina = sqliteDrljaDbContext.StavkeNarudzbine.Where(s => s.TR_NARUDZBE_ID == movePorudzbinaClickBar.OldUnprocessedOrderId);

        //                if (itemsInOldNarudzbina != null && itemsInOldNarudzbina.Any())
        //                {
        //                    if (movePorudzbinaClickBar.Items != null && movePorudzbinaClickBar.Items.Any())
        //                    {
        //                        foreach (var item in movePorudzbinaClickBar.Items)
        //                        {
        //                            decimal quantity = item.Kolicina;

        //                            var itemsInNarudbina = itemsInOldNarudzbina.Where(x => x.TR_BRART == item.ItemIdString);

        //                            if (itemsInNarudbina != null && itemsInNarudbina.Any())
        //                            {
        //                                foreach (var i in itemsInNarudbina)
        //                                {
        //                                    if (quantity == 0)
        //                                    {
        //                                        break;
        //                                    }

        //                                    if (quantity >= i.TR_KOL)
        //                                    {
        //                                        quantity -= i.TR_KOL;
        //                                        i.TR_NARUDZBE_ID = movePorudzbinaClickBar.NewUnprocessedOrderId;
        //                                    }
        //                                    else
        //                                    {
        //                                        i.TR_KOL -= quantity;

        //                                        StavkeNarudzbeDB stavkeNarudzbeDB = new StavkeNarudzbeDB()
        //                                        {
        //                                            Id = Guid.NewGuid().ToString(),
        //                                            TR_BRART = i.TR_BRART,
        //                                            TR_BROJNARUDZBE = i.TR_BROJNARUDZBE,
        //                                            TR_KOL = quantity,
        //                                            TR_KOL_STORNO = 0,
        //                                            TR_MPC = i.TR_MPC,
        //                                            TR_NARUDZBE_ID = movePorudzbinaClickBar.NewUnprocessedOrderId,
        //                                            TR_NAZIV = i.TR_NAZIV,
        //                                            TR_PAK = i.TR_PAK,
        //                                            TR_RBS = i.TR_RBS,
        //                                            TR_ZELJA = i.TR_ZELJA
        //                                        };

        //                                        sqliteDrljaDbContext.StavkeNarudzbine.Add(stavkeNarudzbeDB);

        //                                        quantity = 0;
        //                                    }
        //                                    sqliteDrljaDbContext.StavkeNarudzbine.Update(i);
        //                                }
        //                            }
        //                        }
        //                        await sqliteDrljaDbContext.SaveChangesAsync();
        //                        await transaction.CommitAsync();
        //                        Log.Debug("Uspesno prebacivanje porudzbine");
        //                    }
        //                }
        //            }
        //            result = Ok();
        //        }
        //        catch (Exception ex)
        //        {
        //            await transaction.RollbackAsync(); // Rollback transakcije u slučaju greške
        //            Log.Error("PorudzbinaController -> MovePorudzbina -> Greska: ", ex);
        //            result = BadRequest(ex.Message);
        //        }
        //        finally
        //        {
        //            _semaphore.Release();
        //        }
        //    });
        //    return result;
        //}

        //[HttpPost("checkIsFinish")]
        //public async Task<IActionResult> CheckIsFinish(User user)
        //{
        //    await _semaphore.WaitAsync();
        //    IActionResult result = NoContent();
        //    var strategy = sqliteDrljaDbContext.Database.CreateExecutionStrategy();
        //    await strategy.ExecuteAsync(async () =>
        //    {
        //        await using var transaction = await sqliteDrljaDbContext.Database.BeginTransactionAsync(); // Koristimo SQL Server bazu
        //        try
        //        {
        //            List<Porudzbina> notifications = new List<Porudzbina>();

        //            var porudzbineDB = sqliteDrljaDbContext.Narudzbine.Where(n => n.TR_RADNIK == user.Id &&
        //            n.TR_FAZA == 2);

        //            if (porudzbineDB != null &&
        //                porudzbineDB.Any())
        //            {
        //                foreach (var n in porudzbineDB)
        //                {
        //                    var itemsDB = sqliteDrljaDbContext.StavkeNarudzbine.Where(i => i.TR_BROJNARUDZBE == n.TR_BROJNARUDZBE);

        //                    if (itemsDB != null &&
        //                    itemsDB.Any())
        //                    {
        //                        Porudzbina porudzbina = new Porudzbina()
        //                        {
        //                            BrPorudzbine = n.TR_BROJNARUDZBE,
        //                            StoBr = n.TR_STO,
        //                            RadnikId = n.TR_RADNIK,
        //                            Items = new List<PorudzbinaItem>()
        //                        };

        //                        foreach (var itemDB in itemsDB)
        //                        {
        //                            PorudzbinaItem porudzbinaItem = new PorudzbinaItem()
        //                            {
        //                                ItemIdString = itemDB.TR_BRART,
        //                                Jm = itemDB.TR_PAK,
        //                                Kolicina = itemDB.TR_KOL,
        //                                Naziv = itemDB.TR_NAZIV,
        //                                BrojNarudzbe = n.TR_BROJNARUDZBE,
        //                                MPC = itemDB.TR_MPC,
        //                                Zelje = itemDB.TR_ZELJA,
        //                                RBS = itemDB.TR_RBS,
        //                            };

        //                            porudzbina.Items.Add(porudzbinaItem);
        //                        }

        //                        notifications.Add(porudzbina);
        //                    }
        //                }
        //            }

        //            result = Ok(notifications);
        //        }
        //        catch (Exception ex)
        //        {
        //            await transaction.RollbackAsync(); // Rollback transakcije u slučaju greške
        //            Log.Error("PorudzbinaController -> CheckIsFinish -> Greska: ", ex);
        //            result = BadRequest(ex.Message);
        //        }

        //        finally
        //        {
        //            _semaphore.Release();
        //        }
        //    });

        //    return result;
        //}

        //[HttpPost("seenOrder")]
        //public async Task<IActionResult> SeenOrder(Porudzbina porudzbina)
        //{
        //    await _semaphore.WaitAsync();
        //    IActionResult result = NoContent();
        //    var strategy = sqliteDrljaDbContext.Database.CreateExecutionStrategy();
        //    await strategy.ExecuteAsync(async () =>
        //    {
        //        await using var transaction = await sqliteDrljaDbContext.Database.BeginTransactionAsync(); // Koristimo SQL Server bazu
        //        try
        //        {
        //            var porDB = await sqliteDrljaDbContext.Narudzbine.FirstOrDefaultAsync(n => n.TR_BROJNARUDZBE == porudzbina.BrPorudzbine &&
        //            n.TR_FAZA == 2);

        //            if (porDB != null)
        //            {
        //                porDB.TR_FAZA = 3;
        //                sqliteDrljaDbContext.Narudzbine.Update(porDB);
        //                sqliteDbContext.SaveChanges();
        //            }

        //            result = Ok();
        //        }
        //        catch (Exception ex)
        //        {
        //            Log.Error("PorudzbinaController -> SeenOrder -> Greska: ", ex);
        //            result = BadRequest(ex.Message);
        //        }

        //        finally
        //        {
        //            _semaphore.Release();
        //        }
        //    });

        //    return result;
        //}
        private async Task PrintOrder(CashierDB cashierDB,
            PaymentPlaceDB paymentPlaceDB,
            List<PorudzbinaItem> items,
            UnprocessedOrderDB unprocessedOrderDB)
        {
            try
            {
                DateTime orderTime = DateTime.Now;

                ClickBar_Common.Models.Order.Order orderKuhinja = new ClickBar_Common.Models.Order.Order()
                {
                    CashierName = cashierDB.Name,
                    TableId = paymentPlaceDB.Id,
                    Items = new List<ClickBar_Common.Models.Order.ItemOrder>(),
                    OrderTime = orderTime,
                    OrderName = "K"
                };
                ClickBar_Common.Models.Order.Order orderSank = new ClickBar_Common.Models.Order.Order()
                {
                    CashierName = cashierDB.Name,
                    TableId = paymentPlaceDB.Id,
                    Items = new List<ClickBar_Common.Models.Order.ItemOrder>(),
                    OrderTime = orderTime,
                    OrderName = "S"
                };
                ClickBar_Common.Models.Order.Order orderDrugo = new ClickBar_Common.Models.Order.Order()
                {
                    CashierName = cashierDB.Name,
                    TableId = paymentPlaceDB.Id,
                    Items = new List<ClickBar_Common.Models.Order.ItemOrder>(),
                    OrderTime = orderTime
                };

                orderKuhinja.PartHall = paymentPlaceDB.PartHall.Name;
                orderSank.PartHall = paymentPlaceDB.PartHall.Name;
                orderDrugo.PartHall = paymentPlaceDB.PartHall.Name;

                var allItemsDB = await _sqlServerDbContext.Items.Include(i => i.ItemGroupNavigation)
                    .ThenInclude(ig => ig.IdSupergroupNavigation).ToListAsync();

                foreach (var item in items)
                {
                    var itemDB = allItemsDB.FirstOrDefault(i => i.Id == item.ItemId);

                    decimal popust = (100 - paymentPlaceDB.Popust) / 100;
                    //var itemNadgroup = context.Items.Join(context.ItemGroups,
                    //item => item.IdItemGroup,
                    //itemGroup => itemGroup.Id,
                    //(item, itemGroup) => new { Item = item, ItemGroup = itemGroup })
                    //.Join(context.Supergroups,
                    //group => group.ItemGroup.IdSupergroup,
                    //supergroup => supergroup.Id,
                    //(group, supergroup) => new { Group = group, Supergroup = supergroup })
                    //.FirstOrDefault(it => it.Group.Item.Id == item.Item.Id);

                    if (itemDB != null)
                    {

                        if (itemDB.ItemGroupNavigation.IdSupergroupNavigation.Name.ToLower().Contains("hrana") ||
                        itemDB.ItemGroupNavigation.IdSupergroupNavigation.Name.ToLower().Contains("kuhinja"))
                        {
                            if (!string.IsNullOrEmpty(item.Zelje))
                            {
                                orderKuhinja.Items.Add(new ClickBar_Common.Models.Order.ItemOrder()
                                {
                                    Name = item.Name,
                                    Quantity = item.Kolicina,
                                    Id = item.ItemId,
                                    TotalAmount = Decimal.Round(item.Mpc * item.Kolicina, 2) * popust,
                                    Zelja = item.Zelje
                                });
                            }
                            //else if (item.Zelje.FirstOrDefault(f => !string.IsNullOrEmpty(f.Description)) != null)
                            //{
                            //    var saZeljama = item.Zelje.Where(z => !string.IsNullOrEmpty(z.Description));

                            //    if (saZeljama.Any())
                            //    {
                            //        foreach (var z in saZeljama)
                            //        {
                            //            decimal quantityz = item.Kolicina;
                            //            orderKuhinja.Items.Add(new ClickBar_Common.Models.Order.ItemOrder()
                            //            {
                            //                Name = item.Name,
                            //                Quantity = 1,
                            //                Id = item.ItemId,
                            //                TotalAmount = item.TotalAmount * popust,
                            //                Zelja = z.Description
                            //            });
                            //        }
                            //    }

                            //    decimal quantity = item.Kolicina - (saZeljama != null ? saZeljama.Count() : 0);

                            //    if (quantity > 0)
                            //    {
                            //        orderKuhinja.Items.Add(new ClickBar_Common.Models.Order.ItemOrder()
                            //        {
                            //            Name = item.Name,
                            //            Quantity = quantity,
                            //            Id = item.ItemId,
                            //            TotalAmount = decimal.Round(item.TotalAmount, 2) * popust,
                            //        });
                            //    }
                            //}
                            else
                            {
                                orderKuhinja.Items.Add(new ClickBar_Common.Models.Order.ItemOrder()
                                {
                                    Name = item.Name,
                                    Quantity = item.Kolicina,
                                    Id = item.ItemId,
                                    TotalAmount = Decimal.Round(item.Mpc * item.Kolicina, 2) * popust,
                                });
                            }
                        }
                        else
                        {
                            if (itemDB.ItemGroupNavigation.IdSupergroupNavigation.Name.ToLower().Contains("pice") ||
                            itemDB.ItemGroupNavigation.IdSupergroupNavigation.Name.ToLower().Contains("piće") ||
                            itemDB.ItemGroupNavigation.IdSupergroupNavigation.Name.ToLower().Contains("sank") ||
                            itemDB.ItemGroupNavigation.IdSupergroupNavigation.Name.ToLower().Contains("šank"))
                            {
                                orderSank.Items.Add(new ClickBar_Common.Models.Order.ItemOrder()
                                {
                                    Name = item.Name,
                                    Quantity = item.Kolicina,
                                    Id = item.ItemId,
                                    TotalAmount = Decimal.Round(item.Mpc * item.Kolicina, 2) * popust,
                                });
                            }
                            else
                            {
                                orderDrugo.OrderName = $"{itemDB.ItemGroupNavigation.IdSupergroupNavigation.Name[0]}";
                                orderDrugo.Items.Add(new ClickBar_Common.Models.Order.ItemOrder()
                                {
                                    Name = item.Name,
                                    Quantity = item.Kolicina,
                                    Id = item.ItemId,
                                    TotalAmount = Decimal.Round(item.Mpc * item.Kolicina, 2) * popust,
                                });
                            }
                        }
                    }
                    else
                    {
                        orderSank.Items.Add(new ClickBar_Common.Models.Order.ItemOrder()
                        {
                            Name = item.Name,
                            Quantity = item.Kolicina,
                            Id = item.ItemId,
                            TotalAmount = Decimal.Round(item.Mpc * item.Kolicina, 2) * popust,
                        });
                    }
                }

                int orderCounter = 1;

                var ordersTodayDB = _sqlServerDbContext.OrdersToday.Where(o => o.OrderDateTime.Date == DateTime.Now.Date);

                if (ordersTodayDB != null &&
                    ordersTodayDB.Any())
                {
                    orderCounter = ordersTodayDB.Max(o => o.Counter);
                    orderCounter++;
                }

                var posType = SettingsManager.Instance.GetPrinterFormat() == PrinterFormatEnumeration.Pos80mm ?
                ClickBar_Printer.Enums.PosTypeEnumeration.Pos80mm : ClickBar_Printer.Enums.PosTypeEnumeration.Pos58mm;
                if (orderSank.Items.Any())
                {
                    int orderCounterType = 1;

                    var ordersTodayTypeDB = _sqlServerDbContext.OrdersToday.Where(o => o.OrderDateTime.Date == DateTime.Now.Date &&
                    !string.IsNullOrEmpty(o.Name) &&
                    o.Name.ToLower().Contains(orderSank.OrderName.ToLower()));

                    if (ordersTodayTypeDB != null &&
                        ordersTodayTypeDB.Any())
                    {
                        orderCounterType = ordersTodayTypeDB.Max(o => o.CounterType);
                        orderCounterType++;
                    }

                    orderSank.OrderName += orderCounterType.ToString() + "__" + orderCounter.ToString();

                    decimal totalAmount = orderSank.Items.Sum(i => i.TotalAmount);

                    OrderTodayDB orderTodayDB = new OrderTodayDB()
                    {
                        Id = Guid.NewGuid().ToString(),
                        UnprocessedOrderId = unprocessedOrderDB.Id,
                        CashierId = !string.IsNullOrEmpty(paymentPlaceDB.Name) && paymentPlaceDB.Name.ToLower().Contains("dostava 1") ? "3333" :
                        !string.IsNullOrEmpty(paymentPlaceDB.Name) && paymentPlaceDB.Name.ToLower().Contains("dostava 2") ? "4444" : cashierDB.Id,
                        Counter = orderCounter,
                        CounterType = orderCounterType,
                        OrderDateTime = DateTime.Now,
                        TotalPrice = totalAmount,
                        Name = orderSank.OrderName,
                        TableId = paymentPlaceDB.Id,
                        OrderTodayItems = new List<OrderTodayItemDB>(),
                        Faza = (int)FazaPorudzbineEnumeration.Nova
                    };

                    //sqliteDbContext.OrdersToday.Add(orderTodayDB);
                    //sqliteDbContext.SaveChanges();

                    foreach (var item in orderSank.Items)
                    {
                        var orderTodayItemDB = orderTodayDB.OrderTodayItems.FirstOrDefault(o => o.ItemId == item.Id);

                        if (orderTodayItemDB != null)
                        {
                            orderTodayItemDB.Quantity += item.Quantity;
                            orderTodayItemDB.TotalPrice += item.TotalAmount;
                            //sqliteDbContext.OrderTodayItems.Update(orderTodayItemDB);
                        }
                        else
                        {
                            orderTodayItemDB = new OrderTodayItemDB()
                            {
                                Id = Guid.NewGuid().ToString(),
                                ItemId = item.Id,
                                OrderTodayId = orderTodayDB.Id,
                                Quantity = item.Quantity,
                                TotalPrice = item.TotalAmount,
                            };
                            orderTodayDB.OrderTodayItems.Add(orderTodayItemDB);
                        }
                    }
                    await _sqlServerDbContext.OrdersToday.AddAsync(orderTodayDB);
                    await _sqlServerDbContext.SaveChangesAsync();

                    FormatPos.PrintOrder(orderSank, posType, ClickBar_Printer.Enums.OrderTypeEnumeration.Sank);
                }
                if (orderKuhinja.Items.Any())
                {
                    int orderCounterType = 1;

                    var ordersTodayTypeDB = _sqlServerDbContext.OrdersToday.Where(o => o.OrderDateTime.Date == DateTime.Now.Date &&
                    !string.IsNullOrEmpty(o.Name) &&
                    o.Name.ToLower().Contains(orderKuhinja.OrderName.ToLower()));

                    if (ordersTodayTypeDB != null &&
                        ordersTodayTypeDB.Any())
                    {
                        orderCounterType = ordersTodayTypeDB.Max(o => o.CounterType);
                        orderCounterType++;
                    }

                    orderKuhinja.OrderName += orderCounterType.ToString() + "__" + orderCounter.ToString();

                    decimal totalAmount = orderKuhinja.Items.Sum(i => i.TotalAmount);

                    OrderTodayDB orderTodayDB = new OrderTodayDB()
                    {
                        Id = Guid.NewGuid().ToString(),
                        UnprocessedOrderId = unprocessedOrderDB.Id,
                        CashierId = !string.IsNullOrEmpty(paymentPlaceDB.Name) && paymentPlaceDB.Name.ToLower().Contains("dostava 1") ? "3333" :
                        !string.IsNullOrEmpty(paymentPlaceDB.Name) && paymentPlaceDB.Name.ToLower().Contains("dostava 2") ? "4444" : cashierDB.Id,
                        Counter = orderCounter,
                        CounterType = orderCounterType,
                        OrderDateTime = DateTime.Now,
                        TotalPrice = totalAmount,
                        Name = orderKuhinja.OrderName,
                        TableId = paymentPlaceDB.Id,
                        OrderTodayItems = new List<OrderTodayItemDB>(),
                        Faza = (int)FazaPorudzbineEnumeration.Nova
                    };

                    //sqliteDbContext.OrdersToday.Add(orderTodayDB);
                    //sqliteDbContext.SaveChanges();

                    foreach (var item in orderKuhinja.Items)
                    {
                        var orderTodayItemDB = orderTodayDB.OrderTodayItems.FirstOrDefault(o => o.ItemId == item.Id);

                        //if (orderTodayItemDB != null)
                        //{
                        //    orderTodayItemDB.Quantity += item.Quantity;
                        //    orderTodayItemDB.TotalPrice += item.TotalAmount;
                        //    //sqliteDbContext.OrderTodayItems.Update(orderTodayItemDB);
                        //}
                        //else
                        //{
                        orderTodayItemDB = new OrderTodayItemDB()
                        {
                            Id = Guid.NewGuid().ToString(),
                            ItemId = item.Id,
                            OrderTodayId = orderTodayDB.Id,
                            Quantity = item.Quantity,
                            TotalPrice = item.TotalAmount,
                            Zelja = item.Zelja
                        };
                        orderTodayDB.OrderTodayItems.Add(orderTodayItemDB);
                        //}
                        //sqliteDbContext.SaveChanges();
                    }
                    await _sqlServerDbContext.OrdersToday.AddAsync(orderTodayDB);
                    await _sqlServerDbContext.SaveChangesAsync();


                    if (string.IsNullOrEmpty(SettingsManager.Instance.GetPathToDrljaKuhinjaDB()))
                    {
                        FormatPos.PrintOrder(orderKuhinja, posType, ClickBar_Printer.Enums.OrderTypeEnumeration.Kuhinja);
                    }
                }
                if (orderDrugo.Items.Any())
                {
                    int orderCounterType = 1;

                    var ordersTodayTypeDB = _sqlServerDbContext.OrdersToday.Where(o => o.OrderDateTime.Date == DateTime.Now.Date &&
                    !string.IsNullOrEmpty(o.Name) &&
                    o.Name.ToLower().Contains(orderDrugo.OrderName.ToLower()));

                    if (ordersTodayTypeDB != null &&
                        ordersTodayTypeDB.Any())
                    {
                        orderCounterType = ordersTodayTypeDB.Max(o => o.CounterType);
                        orderCounterType++;
                    }

                    orderDrugo.OrderName += orderCounterType.ToString() + "__" + orderCounter.ToString();

                    decimal totalAmount = orderDrugo.Items.Sum(i => i.TotalAmount);

                    OrderTodayDB orderTodayDB = new OrderTodayDB()
                    {
                        Id = Guid.NewGuid().ToString(),
                        UnprocessedOrderId = unprocessedOrderDB.Id,
                        CashierId = !string.IsNullOrEmpty(paymentPlaceDB.Name) && paymentPlaceDB.Name.ToLower().Contains("dostava 1") ? "3333" :
                        !string.IsNullOrEmpty(paymentPlaceDB.Name) && paymentPlaceDB.Name.ToLower().Contains("dostava 2") ? "4444" : cashierDB.Id,
                        Counter = orderCounter,
                        CounterType = orderCounterType,
                        OrderDateTime = DateTime.Now,
                        TotalPrice = totalAmount,
                        Name = orderDrugo.OrderName,
                        TableId = paymentPlaceDB.Id,
                        OrderTodayItems = new List<OrderTodayItemDB>(),
                        Faza = (int)FazaPorudzbineEnumeration.Nova
                    };

                    foreach (var item in orderDrugo.Items)
                    {
                        var orderTodayItemDB = orderTodayDB.OrderTodayItems.FirstOrDefault(o => o.ItemId == item.Id);

                        if (orderTodayItemDB != null)
                        {
                            orderTodayItemDB.Quantity += item.Quantity;
                            orderTodayItemDB.TotalPrice += item.TotalAmount;
                        }
                        else
                        {
                            orderTodayItemDB = new OrderTodayItemDB()
                            {
                                Id = Guid.NewGuid().ToString(),
                                ItemId = item.Id,
                                OrderTodayId = orderTodayDB.Id,
                                Quantity = item.Quantity,
                                TotalPrice = item.TotalAmount,
                            };
                            orderTodayDB.OrderTodayItems.Add(orderTodayItemDB);
                        }
                    }

                    await _sqlServerDbContext.OrdersToday.AddAsync(orderTodayDB);
                    await _sqlServerDbContext.SaveChangesAsync();

                    FormatPos.PrintOrder(orderDrugo, posType, ClickBar_Printer.Enums.OrderTypeEnumeration.Sank);
                }
            }
            catch (Exception ex)
            {
                Log.Error("HookOrderOnTableCommand -> PrintOrder -> Greska prilikom printanja porudzbine: ", ex);
                throw;
            }
        }
    }
}
