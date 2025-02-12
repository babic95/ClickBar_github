using System;
using System.Linq;
using ClickBar_DatabaseSQLManager;
using ClickBar_DatabaseSQLManager.Models;
using ClickBar_Logging;
using ClickBar_Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ClickBar_Database
{
    public class DataMigration
    {
        private SqlServerDbContext _dbContext;
        public DataMigration(SqlServerDbContext dbContext)
        {
            _dbContext = dbContext;
            string pathToDB = SettingsManager.Instance.GetPathToDB();
            using (SqliteDbContext sqliteDbContext = new SqliteDbContext())
            {
                var isConnected = sqliteDbContext.ConfigureDatabase(pathToDB).Result;

                if (!isConnected)
                {
                    return;
                }
            }
        }

        public void MigrateData()
        {
            try
            {
                using (var sqliteDbContext = new SqliteDbContext())
                {
                    var supergroups = sqliteDbContext.Supergroups.AsNoTracking().ToList();
                    var itemGroups = sqliteDbContext.ItemGroups.AsNoTracking().ToList();
                    var norms = sqliteDbContext.Norms.AsNoTracking().ToList();
                    var items = sqliteDbContext.Items.AsNoTracking().ToList();
                    var cashiers = sqliteDbContext.Cashiers.AsNoTracking().ToList();
                    var invoices = sqliteDbContext.Invoices.AsNoTracking().ToList();
                    var itemInvoices = sqliteDbContext.ItemInvoices.AsNoTracking().ToList();
                    var paymentInvoices = sqliteDbContext.PaymentInvoices.AsNoTracking().ToList();
                    var taxItemInvoices = sqliteDbContext.TaxItemInvoices.AsNoTracking().ToList();
                    var smartCards = sqliteDbContext.SmartCards.AsNoTracking().ToList();
                    var suppliers = sqliteDbContext.Suppliers.AsNoTracking().ToList();
                    var procurements = sqliteDbContext.Procurements.AsNoTracking().ToList();
                    var partHalls = sqliteDbContext.PartHalls.AsNoTracking().ToList();
                    var paymentPlaces = sqliteDbContext.PaymentPlaces.AsNoTracking().ToList();
                    var orders = sqliteDbContext.Orders.AsNoTracking().ToList();
                    var itemsInNorm = sqliteDbContext.ItemsInNorm.AsNoTracking().ToList();
                    var unprocessedOrders = sqliteDbContext.UnprocessedOrders.AsNoTracking().ToList();
                    var itemsInUnprocessedOrder = sqliteDbContext.ItemsInUnprocessedOrder.AsNoTracking().ToList();
                    var calculations = sqliteDbContext.Calculations.AsNoTracking().ToList();
                    var calculationItems = sqliteDbContext.CalculationItems.AsNoTracking().ToList();
                    var nivelacijas = sqliteDbContext.Nivelacijas.AsNoTracking().ToList();
                    var itemsNivelacija = sqliteDbContext.ItemsNivelacija.AsNoTracking().ToList();
                    var knjizenjePazara = sqliteDbContext.KnjizenjePazara.AsNoTracking().ToList();
                    var kep = sqliteDbContext.Kep.AsNoTracking().ToList();
                    var firmas = sqliteDbContext.Firmas.AsNoTracking().ToList();
                    var partners = sqliteDbContext.Partners.AsNoTracking().ToList();
                    var pocetnaStanja = sqliteDbContext.PocetnaStanja.AsNoTracking().ToList();
                    var pocetnaStanjaItems = sqliteDbContext.PocetnaStanjaItems.AsNoTracking().ToList();
                    var ordersToday = sqliteDbContext.OrdersToday.AsNoTracking().ToList();
                    var orderTodayItems = sqliteDbContext.OrderTodayItems.AsNoTracking().ToList();
                    var otpisi = sqliteDbContext.Otpisi.AsNoTracking().ToList();
                    var otpisItems = sqliteDbContext.OtpisItems.AsNoTracking().ToList();
                    var zelje = sqliteDbContext.Zelje.AsNoTracking().ToList();

                    using (IDbContextTransaction transaction = _dbContext.Database.BeginTransaction())
                    {
                        try
                        {
                            // Supergroup
                            EnableIdentityInsert(_dbContext, "Supergroup");
                            _dbContext.Supergroups.AddRange(supergroups);
                            _dbContext.SaveChanges();
                            DisableIdentityInsert(_dbContext, "Supergroup");

                            // ItemGroup
                            EnableIdentityInsert(_dbContext, "ItemGroup");
                            _dbContext.ItemGroups.AddRange(itemGroups);
                            _dbContext.SaveChanges();
                            DisableIdentityInsert(_dbContext, "ItemGroup");

                            // Norm
                            EnableIdentityInsert(_dbContext, "Norm");
                            _dbContext.Norms.AddRange(norms);
                            _dbContext.SaveChanges();
                            DisableIdentityInsert(_dbContext, "Norm");

                            _dbContext.Items.AddRange(items);
                            _dbContext.SaveChanges();

                            _dbContext.Cashiers.AddRange(cashiers);
                            _dbContext.SaveChanges();

                            _dbContext.Invoices.AddRange(invoices);
                            _dbContext.SaveChanges();

                            _dbContext.ItemInvoices.AddRange(itemInvoices);
                            _dbContext.SaveChanges();

                            _dbContext.PaymentInvoices.AddRange(paymentInvoices);
                            _dbContext.SaveChanges();

                            _dbContext.TaxItemInvoices.AddRange(taxItemInvoices);
                            _dbContext.SaveChanges();

                            _dbContext.SmartCards.AddRange(smartCards);
                            _dbContext.SaveChanges();

                            // Supplier
                            EnableIdentityInsert(_dbContext, "Supplier");
                            _dbContext.Suppliers.AddRange(suppliers);
                            _dbContext.SaveChanges();
                            DisableIdentityInsert(_dbContext, "Supplier");

                            _dbContext.Procurements.AddRange(procurements);
                            _dbContext.SaveChanges();

                            // PartHall
                            EnableIdentityInsert(_dbContext, "PartHall");
                            _dbContext.PartHalls.AddRange(partHalls);
                            _dbContext.SaveChanges();
                            DisableIdentityInsert(_dbContext, "PartHall");

                            // PaymentPlace
                            EnableIdentityInsert(_dbContext, "PaymentPlace");
                            _dbContext.PaymentPlaces.AddRange(paymentPlaces);
                            _dbContext.SaveChanges();
                            DisableIdentityInsert(_dbContext, "PaymentPlace");

                            _dbContext.Orders.AddRange(orders);
                            _dbContext.SaveChanges();

                            _dbContext.ItemsInNorm.AddRange(itemsInNorm);
                            _dbContext.SaveChanges();

                            _dbContext.UnprocessedOrders.AddRange(unprocessedOrders);
                            _dbContext.SaveChanges();

                            _dbContext.ItemsInUnprocessedOrder.AddRange(itemsInUnprocessedOrder);
                            _dbContext.SaveChanges();

                            _dbContext.Calculations.AddRange(calculations);
                            _dbContext.SaveChanges();

                            _dbContext.CalculationItems.AddRange(calculationItems);
                            _dbContext.SaveChanges();

                            _dbContext.Nivelacijas.AddRange(nivelacijas);
                            _dbContext.SaveChanges();

                            _dbContext.ItemsNivelacija.AddRange(itemsNivelacija);
                            _dbContext.SaveChanges();

                            _dbContext.KnjizenjePazara.AddRange(knjizenjePazara);
                            _dbContext.SaveChanges();

                            _dbContext.Kep.AddRange(kep);
                            _dbContext.SaveChanges();

                            // Firma
                            EnableIdentityInsert(_dbContext, "Firma");
                            _dbContext.Firmas.AddRange(firmas);
                            _dbContext.SaveChanges();
                            DisableIdentityInsert(_dbContext, "Firma");

                            // Partner
                            EnableIdentityInsert(_dbContext, "Partner");
                            _dbContext.Partners.AddRange(partners);
                            _dbContext.SaveChanges();
                            DisableIdentityInsert(_dbContext, "Partner");

                            _dbContext.PocetnaStanja.AddRange(pocetnaStanja);
                            _dbContext.SaveChanges();

                            _dbContext.PocetnaStanjaItems.AddRange(pocetnaStanjaItems);
                            _dbContext.SaveChanges();

                            _dbContext.OrdersToday.AddRange(ordersToday);
                            _dbContext.SaveChanges();

                            _dbContext.OrderTodayItems.AddRange(orderTodayItems);
                            _dbContext.SaveChanges();

                            _dbContext.Otpisi.AddRange(otpisi);
                            _dbContext.SaveChanges();

                            _dbContext.OtpisItems.AddRange(otpisItems);
                            _dbContext.SaveChanges();

                            _dbContext.Zelje.AddRange(zelje);
                            _dbContext.SaveChanges();

                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            Console.WriteLine("An error occurred while saving the entity changes.");
                            Console.WriteLine("Error Message: " + ex.Message);
                            if (ex.InnerException != null)
                            {
                                Console.WriteLine("Inner Exception Message: " + ex.InnerException.Message);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while saving the entity changes.");
                Console.WriteLine("Error Message: " + ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine("Inner Exception Message: " + ex.InnerException.Message);
                }
            }
        }

        private void EnableIdentityInsert(SqlServerDbContext context, string tableName)
        {
            context.Database.ExecuteSqlRaw($"SET IDENTITY_INSERT {tableName} ON;");
        }

        private void DisableIdentityInsert(SqlServerDbContext context, string tableName)
        {
            context.Database.ExecuteSqlRaw($"SET IDENTITY_INSERT {tableName} OFF;");
        }
    }
}