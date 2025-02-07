using System;
using System.Collections.Generic;
using System.Data;
using ClickBar_Common.Enums;
using ClickBar_Database.Models;
using ClickBar_Logging;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ClickBar_Database
{
    public partial class SqliteDbContext : DbContext
    {
        #region Fields
        private string _connectionString;
        private static string _fileDestination;
        private SqliteConnection _sqliteConnection;
        #endregion Fields
        public SqliteDbContext()
        {
            if (!string.IsNullOrEmpty(_fileDestination))
            {
                Connection();
            }
        }

        public SqliteDbContext(DbContextOptions<SqliteDbContext> options)
            : base(options)
        {
        }
        public virtual DbSet<CashierDB> Cashiers { get; set; } = null!;
        public virtual DbSet<InvoiceDB> Invoices { get; set; } = null!;
        public virtual DbSet<ItemDB> Items { get; set; } = null!;
        public virtual DbSet<SupergroupDB> Supergroups { get; set; } = null!;
        public virtual DbSet<ItemGroupDB> ItemGroups { get; set; } = null!;
        public virtual DbSet<ItemInvoiceDB> ItemInvoices { get; set; } = null!;
        public virtual DbSet<OrderDB> Orders { get; set; } = null!;
        public virtual DbSet<PartHallDB> PartHalls { get; set; } = null!;
        public virtual DbSet<PaymentInvoiceDB> PaymentInvoices { get; set; } = null!;
        public virtual DbSet<PaymentPlaceDB> PaymentPlaces { get; set; } = null!;
        public virtual DbSet<ProcurementDB> Procurements { get; set; } = null!;
        public virtual DbSet<SmartCardDB> SmartCards { get; set; } = null!;
        public virtual DbSet<SupplierDB> Suppliers { get; set; } = null!;
        public virtual DbSet<TaxItemInvoiceDB> TaxItemInvoices { get; set; } = null!;
        public virtual DbSet<NormDB> Norms { get; set; } = null!;
        public virtual DbSet<ItemInNormDB> ItemsInNorm { get; set; } = null!;
        public virtual DbSet<UnprocessedOrderDB> UnprocessedOrders { get; set; } = null!;
        public virtual DbSet<ItemInUnprocessedOrderDB> ItemsInUnprocessedOrder { get; set; } = null!;
        public virtual DbSet<CalculationDB> Calculations { get; set; } = null!;
        public virtual DbSet<CalculationItemDB> CalculationItems { get; set; } = null!;
        public virtual DbSet<NivelacijaDB> Nivelacijas { get; set; } = null!;
        public virtual DbSet<ItemNivelacijaDB> ItemsNivelacija { get; set; } = null!;
        public virtual DbSet<KnjizenjePazaraDB> KnjizenjePazara { get; set; } = null!;
        public virtual DbSet<KepDB> Kep { get; set; } = null!;
        public virtual DbSet<FirmaDB> Firmas { get; set; } = null!;
        public virtual DbSet<PartnerDB> Partners { get; set; } = null!;
        public virtual DbSet<PocetnoStanjeDB> PocetnaStanja { get; set; } = null!;
        public virtual DbSet<PocetnoStanjeItemDB> PocetnaStanjaItems { get; set; } = null!;
        public virtual DbSet<OrderTodayDB> OrdersToday { get; set; } = null!;
        public virtual DbSet<OrderTodayItemDB> OrderTodayItems { get; set; } = null!;
        public virtual DbSet<OtpisDB> Otpisi { get; set; } = null!;
        public virtual DbSet<OtpisItemDB> OtpisItems { get; set; } = null!;
        public virtual DbSet<ItemZeljaDB> Zelje { get; set; } = null!;

        #region Public method

        public async Task<List<TaxItemInvoiceDB>> GetAllTaxFromInvoice(string invoiceId)
        {
            if (TaxItemInvoices == null)
            {
                return new List<TaxItemInvoiceDB>();
            }

            var taxItems = TaxItemInvoices.Where(tax => tax.InvoiceId == invoiceId);

            if (taxItems != null &&
                taxItems.Any())
            {
                List<TaxItemInvoiceDB> itemsTax = new List<TaxItemInvoiceDB>();

                taxItems.ToList().ForEach(async taxItemInvoice =>
                {
                    var invoice = await Invoices.FindAsync(invoiceId);

                    if (invoice != null)
                    {
                        taxItemInvoice.Invoice = invoice;

                        itemsTax.Add(taxItemInvoice);
                    }
                });

                return itemsTax;
            }

            return new List<TaxItemInvoiceDB>();
        }
        public async Task<bool> ConfigureDatabase(string databaseFilePath)
        {
            try
            {
                _fileDestination = databaseFilePath;

                return await Connection();
            }
            catch
            {
                return false;
            }
        }

        public List<InvoiceDB> GetInvoiceForReport(DateTime fromDateTime, DateTime toDateTime)
        {
            if (Invoices == null)
            {
                return new List<InvoiceDB>();
            }

            var invoices = Invoices.Where(invoice => invoice.SdcDateTime >= fromDateTime && invoice.SdcDateTime <= toDateTime
            && (invoice.InvoiceType == Convert.ToInt32(InvoiceTypeEenumeration.Normal) ||
            invoice.InvoiceType == Convert.ToInt32(InvoiceTypeEenumeration.Advance))).ToList();

            return invoices;
        }
        public async Task<List<ItemInvoiceDB>> GetAllItemsFromInvoice(string invoiceId)
        {
            if (ItemInvoices == null)
            {
                return new List<ItemInvoiceDB>();
            }

            List<ItemInvoiceDB> itemsInvoice = ItemInvoices.Where(invoice => invoice.InvoiceId == invoiceId && 
            (invoice.IsSirovina == null || invoice.IsSirovina == 0)).ToList();

            itemsInvoice.ForEach(async itemInvoice =>
            {
                itemInvoice.Invoice = await Invoices.FindAsync(invoiceId);
            });

            return itemsInvoice;
        }
        public async Task<List<PaymentInvoiceDB>> GetAllPaymentFromInvoice(string invoiceId)
        {
            if (PaymentInvoices == null)
            {
                return new List<PaymentInvoiceDB>();
            }

            List<PaymentInvoiceDB> payments = PaymentInvoices.Where(payment => payment.InvoiceId == invoiceId).ToList();

            payments.ForEach(async paymentInvoice =>
            {
                paymentInvoice.Invoice = await Invoices.FindAsync(invoiceId);
            });

            return payments;
        }
        public List<InvoiceDB> GetInvoiceForReport(DateTime fromDateTime, DateTime toDateTime, string smartCard)
        {
            if (Invoices == null)
            {
                return new List<InvoiceDB>();
            }

            CashierDB? cashier = Cashiers.Find(smartCard);

            if (cashier != null)
            {
                var invoices = Invoices.Where(invoice => invoice.SdcDateTime >= fromDateTime && invoice.SdcDateTime <= toDateTime
                && invoice.Cashier == cashier.Id
                && (invoice.InvoiceType == Convert.ToInt32(InvoiceTypeEenumeration.Normal) ||
                invoice.InvoiceType == Convert.ToInt32(InvoiceTypeEenumeration.Advance))).ToList();
                return invoices;
            }
            else
            {
                return new List<InvoiceDB>();
            }
        }
        public List<InvoiceDB> GetInvoiceForReport(DateTime fromDateTime, DateTime toDateTime, CashierDB cashier)
        {
            if (Invoices == null)
            {
                return new List<InvoiceDB>();
            }

            var invoices = Invoices.Where(invoice => invoice.SdcDateTime >= fromDateTime && invoice.SdcDateTime <= toDateTime
            && invoice.Cashier == cashier.Id
            && (invoice.InvoiceType == Convert.ToInt32(InvoiceTypeEenumeration.Normal) ||
            invoice.InvoiceType == Convert.ToInt32(InvoiceTypeEenumeration.Advance))).ToList();

            return invoices;
        }
        #endregion Public method

        #region Protected method
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite(_connectionString);
                base.OnConfiguring(optionsBuilder);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CashierDB>(entity =>
            {
                entity.ToTable(SQLiteManagerTableNames.Cashier);

                entity.Property(e => e.Id).HasMaxLength(4);

                entity.Property(e => e.Address).HasMaxLength(65);

                entity.Property(e => e.City).HasMaxLength(45);

                entity.Property(e => e.ContactNumber).HasMaxLength(20);

                entity.Property(e => e.Email).HasMaxLength(60);

                entity.Property(e => e.Jmbg).HasMaxLength(13);

                entity.Property(e => e.Name).HasMaxLength(45);
            });

            modelBuilder.Entity<InvoiceDB>(entity =>
            {
                entity.ToTable(SQLiteManagerTableNames.Invoice);

                entity.Property(e => e.Id).HasMaxLength(36);

                entity.Property(e => e.KnjizenjePazaraId).HasMaxLength(36);

                entity.Property(e => e.Address).HasMaxLength(95);

                entity.Property(e => e.BusinessName).HasMaxLength(75);

                entity.Property(e => e.BuyerAddress).HasMaxLength(45);

                entity.Property(e => e.BuyerCostCenterId).HasMaxLength(45);

                entity.Property(e => e.BuyerId).HasMaxLength(45);

                entity.Property(e => e.BuyerName).HasMaxLength(75);

                entity.Property(e => e.Cashier).HasMaxLength(45);

                entity.Property(e => e.DateAndTimeOfIssue).HasColumnType("datetime");

                entity.Property(e => e.District).HasMaxLength(95);

                entity.Property(e => e.EncryptedInternalData).HasMaxLength(512);

                entity.Property(e => e.InvoiceCounter).HasMaxLength(50);

                entity.Property(e => e.InvoiceCounterExtension).HasMaxLength(50);

                entity.Property(e => e.InvoiceNumber).HasMaxLength(50);

                entity.Property(e => e.InvoiceNumberResult).HasMaxLength(50);

                entity.Property(e => e.LocationName).HasMaxLength(95);

                entity.Property(e => e.Mrc).HasMaxLength(55);

                entity.Property(e => e.ReferentDocumentDt)
                    .HasColumnType("datetime")
                    .HasColumnName("ReferentDocumentDT");

                entity.Property(e => e.ReferentDocumentNumber).HasMaxLength(50);

                entity.Property(e => e.RequestedBy).HasMaxLength(10);

                entity.Property(e => e.SdcDateTime).HasColumnType("datetime");

                entity.Property(e => e.Signature).HasMaxLength(512);

                entity.Property(e => e.SignedBy).HasMaxLength(10);

                entity.Property(e => e.Tin).HasMaxLength(35);

                entity.Property(e => e.TotalAmount).HasPrecision(4);
            });

            modelBuilder.Entity<ItemDB>(entity =>
            {
                entity.ToTable(SQLiteManagerTableNames.Item);

                entity.HasIndex(e => e.IdItemGroup, "fk_Item_ItemGroup_idx");

                entity.HasIndex(e => e.IdNorm, "fk_Item_Norm_idx");

                entity.Property(e => e.Id).HasMaxLength(15);

                entity.Property(e => e.AlarmQuantity).HasPrecision(4);

                entity.Property(e => e.Jm)
                    .HasMaxLength(5)
                    .HasColumnName("JM");

                entity.Property(e => e.Label).HasMaxLength(1);

                entity.Property(e => e.Name).HasMaxLength(500);

                entity.Property(e => e.TotalQuantity).HasPrecision(4);

                entity.Property(e => e.SellingUnitPrice).HasPrecision(2);

                entity.Property(e => e.InputUnitPrice).HasPrecision(2);

                entity.HasOne(d => d.ItemGroupNavigation)
                    .WithMany(p => p.Items)
                    .HasForeignKey(d => d.IdItemGroup)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_Item_ItemGroup");

                entity.HasOne(d => d.Norm)
                    .WithOne(p => p.Item)
                    .HasForeignKey<ItemDB>(d => d.IdNorm)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_Item_Norm_idx");
            });

            modelBuilder.Entity<SupergroupDB>(entity =>
            {
                entity.ToTable(SQLiteManagerTableNames.Supergroup);

                entity.Property(e => e.Name).HasMaxLength(45);
            });

            modelBuilder.Entity<ItemGroupDB>(entity =>
            {
                entity.ToTable(SQLiteManagerTableNames.ItemGroup);

                entity.HasIndex(e => e.IdSupergroup, "fk_ItemGroup_Supergroup_idx");

                entity.Property(e => e.Name).HasMaxLength(45);

                entity.HasOne(d => d.IdSupergroupNavigation)
                    .WithMany(p => p.ItemGroups)
                    .HasForeignKey(d => d.IdSupergroup)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_ItemGroup_Supergroup");
            });

            modelBuilder.Entity<ItemInvoiceDB>(entity =>
            {
                entity.HasKey(e => new { e.Id, e.InvoiceId })
                    .HasName("PRIMARY");

                entity.ToTable(SQLiteManagerTableNames.ItemInvoice);

                entity.HasIndex(e => e.InvoiceId, "fk_ItemInvoice_Invoice1_idx");

                entity.Property(e => e.InvoiceId).HasMaxLength(36);

                entity.Property(e => e.ItemCode).HasMaxLength(15);

                entity.Property(e => e.Label).HasMaxLength(1);

                entity.Property(e => e.Name).HasMaxLength(500);

                entity.Property(e => e.Quantity).HasPrecision(4);

                entity.Property(e => e.TotalAmout).HasPrecision(4);

                entity.Property(e => e.UnitPrice).HasPrecision(2);

                entity.Property(e => e.OriginalUnitPrice).HasPrecision(2);

                entity.Property(e => e.InputUnitPrice).HasPrecision(2);

                entity.Property(e => e.IsSirovina);

                entity.HasOne(d => d.Invoice)
                    .WithMany(p => p.ItemInvoices)
                    .HasForeignKey(d => d.InvoiceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_ItemInvoice_Invoice1");
            });

            modelBuilder.Entity<OrderDB>(entity =>
            {
                entity.HasKey(e => new { e.PaymentPlaceId, e.InvoiceId, e.CashierId })
                    .HasName("PRIMARY");

                entity.ToTable(SQLiteManagerTableNames.Order);

                entity.HasIndex(e => e.CashierId, "fk_Order_Cashier1_idx");

                entity.HasIndex(e => e.InvoiceId, "fk_Order_Invoice1_idx");

                entity.Property(e => e.InvoiceId).HasMaxLength(36);

                entity.Property(e => e.CashierId).HasMaxLength(4);

                entity.HasOne(d => d.Cashier)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.CashierId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_Order_Cashier1");

                entity.HasOne(d => d.Invoice)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.InvoiceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_Order_Invoice1");
            });

            modelBuilder.Entity<PartHallDB>(entity =>
            {
                entity.ToTable(SQLiteManagerTableNames.PartHall);

                entity.Property(e => e.Image).HasMaxLength(4096);

                entity.Property(e => e.Name).HasMaxLength(45);
            });

            modelBuilder.Entity<PaymentInvoiceDB>(entity =>
            {
                entity.HasKey(e => new { e.PaymentType, e.InvoiceId })
                    .HasName("PRIMARY");

                entity.ToTable(SQLiteManagerTableNames.PaymentInvoice);

                entity.HasIndex(e => e.InvoiceId, "fk_PaymentInvoice_Invoice1_idx");

                entity.Property(e => e.InvoiceId).HasMaxLength(36);

                entity.Property(e => e.Amout).HasPrecision(2);

                entity.HasOne(d => d.Invoice)
                    .WithMany(p => p.PaymentInvoices)
                    .HasForeignKey(d => d.InvoiceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_PaymentInvoice_Invoice1");
            });

            modelBuilder.Entity<PaymentPlaceDB>(entity =>
            {
                entity.HasKey(e => new { e.Id })
                    .HasName("PRIMARY");

                entity.ToTable(SQLiteManagerTableNames.PaymentPlace);

                entity.HasIndex(e => e.PartHallId, "fk_Table_PartHall1_idx");

                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Height).HasPrecision(2);

                entity.Property(e => e.Width).HasPrecision(2);

                entity.Property(e => e.LeftCanvas).HasPrecision(2);

                entity.Property(e => e.TopCanvas).HasPrecision(2);

                entity.Property(e => e.HeightMobi).HasPrecision(2);

                entity.Property(e => e.WidthMobi).HasPrecision(2);

                entity.Property(e => e.X_Mobi).HasPrecision(2);

                entity.Property(e => e.Y_Mobi).HasPrecision(2);

                entity.Property(e => e.Popust).HasPrecision(2);

                entity.HasOne(d => d.PartHall)
                    .WithMany(p => p.Paymentplaces)
                    .HasForeignKey(d => d.PartHallId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_Table_PartHall1");
            });

            modelBuilder.Entity<ProcurementDB>(entity =>
            {
                entity.ToTable(SQLiteManagerTableNames.Procurement);

                entity.HasIndex(e => e.ItemId, "fk_Procurement_Item1_idx");

                entity.HasIndex(e => e.SupplierId, "fk_Procurement_Supplier1_idx");

                entity.Property(e => e.Id).HasMaxLength(36);

                entity.Property(e => e.DateProcurement).HasColumnType("datetime");

                entity.Property(e => e.ItemId).HasMaxLength(15);

                entity.Property(e => e.Quantity).HasPrecision(4);

                entity.Property(e => e.UnitPrice).HasPrecision(2);

                entity.HasOne(d => d.Item)
                    .WithMany(p => p.Procurements)
                    .HasForeignKey(d => d.ItemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_Procurement_Item1");

                entity.HasOne(d => d.Supplier)
                    .WithMany(p => p.Procurements)
                    .HasForeignKey(d => d.SupplierId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_Procurement_Supplier1");
            });

            modelBuilder.Entity<SmartCardDB>(entity =>
            {
                entity.ToTable(SQLiteManagerTableNames.SmartCard);

                entity.HasIndex(e => e.CashierId, "fk_SmartCard_Cashier1_idx");

                entity.Property(e => e.Id).HasMaxLength(15);

                entity.Property(e => e.CashierId).HasMaxLength(4);

                entity.HasOne(d => d.Cashier)
                    .WithMany(p => p.SmartCards)
                    .HasForeignKey(d => d.CashierId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_SmartCard_Cashier1");
            });

            modelBuilder.Entity<SupplierDB>(entity =>
            {
                entity.ToTable(SQLiteManagerTableNames.Supplier);

                entity.Property(e => e.Address).HasMaxLength(75);

                entity.Property(e => e.City).HasMaxLength(45);

                entity.Property(e => e.ContractNumber).HasMaxLength(20);

                entity.Property(e => e.Email).HasMaxLength(60);

                entity.Property(e => e.Mb)
                    .HasMaxLength(45)
                    .HasColumnName("MB");

                entity.Property(e => e.Name).HasMaxLength(95);

                entity.Property(e => e.Pib)
                    .HasMaxLength(45)
                    .HasColumnName("PIB");
            });

            modelBuilder.Entity<TaxItemInvoiceDB>(entity =>
            {
                entity.HasKey(e => new { e.Label, e.InvoiceId })
                    .HasName("PRIMARY");

                entity.ToTable(SQLiteManagerTableNames.TaxItemInvoice);

                entity.HasIndex(e => e.InvoiceId, "fk_TaxItemInvoice_Invoice1_idx");

                entity.Property(e => e.Label).HasMaxLength(1);

                entity.Property(e => e.InvoiceId)
                    .HasMaxLength(36)
                    .HasColumnName("InvoiceId");

                entity.Property(e => e.Amount).HasPrecision(4);

                entity.Property(e => e.CategoryName).HasMaxLength(45);

                entity.Property(e => e.Rate).HasPrecision(4);

                entity.HasOne(d => d.Invoice)
                    .WithMany(p => p.TaxItemInvoices)
                    .HasForeignKey(d => d.InvoiceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_TaxItemInvoice_Invoice1");
            });

            modelBuilder.Entity<NormDB>(entity =>
            {
                entity.ToTable(SQLiteManagerTableNames.Norm);
            });

            modelBuilder.Entity<ItemInNormDB>(entity =>
            {
                entity.HasKey(e => new { e.IdItem, e.IdNorm })
                    .HasName("PRIMARY");

                entity.ToTable(SQLiteManagerTableNames.ItemInNorm);

                entity.HasIndex(e => e.IdNorm, "fk_ItemInNorm_Norm1_idx");

                entity.HasIndex(e => e.IdItem, "fk_ItemInNorm_Item1_idx");

                entity.Property(e => e.IdItem).HasMaxLength(36);

                entity.Property(e => e.IdNorm);

                entity.HasOne(d => d.Norm)
                    .WithMany(p => p.ItemsInNorm)
                    .HasForeignKey(d => d.IdNorm)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_ItemInNorm_Norm1");

                entity.HasOne(d => d.Item)
                    .WithMany(p => p.ItemInNorms)
                    .HasForeignKey(d => d.IdItem)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_ItemInNorm_Item1");
            });

            modelBuilder.Entity<UnprocessedOrderDB>(entity => 
            {
                entity.HasKey(e => new { e.Id })
                    .HasName("PRIMARY");

                entity.ToTable(SQLiteManagerTableNames.UnprocessedOrder);

                entity.HasIndex(e => e.PaymentPlaceId, "fk_UnprocessedOrder_PaymentPlace1_idx");

                entity.HasIndex(e => e.CashierId, "fk_UnprocessedOrder_Cashier1_idx");

                entity.Property(e => e.CashierId).HasMaxLength(36);

                entity.Property(e => e.TotalAmount).HasPrecision(4);

                entity.HasOne(d => d.Cashier)
                    .WithMany(p => p.UnprocessedOrders)
                    .HasForeignKey(d => d.CashierId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_UnprocessedOrder_Cashier1");

                entity.HasOne(d => d.PaymentPlace)
                    .WithMany(p => p.UnprocessedOrders)
                    .HasForeignKey(d => d.PaymentPlaceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_UnprocessedOrder_PaymentPlace1");
            });

            modelBuilder.Entity<ItemInUnprocessedOrderDB>(entity =>
            {
                entity.HasKey(e => new { e.UnprocessedOrderId, e.ItemId })
                    .HasName("PRIMARY");

                entity.ToTable(SQLiteManagerTableNames.ItemInUnprocessedOrder);

                entity.HasIndex(e => e.ItemId, "fk_ItemInUnprocessedOrder_Item1_idx");

                entity.HasIndex(e => e.UnprocessedOrderId, "fk_ItemInUnprocessedOrder_UnprocessedOrder1_idx");

                entity.Property(e => e.ItemId).HasMaxLength(36);

                entity.Property(e => e.UnprocessedOrderId).HasMaxLength(36);

                entity.Property(e => e.Quantity).HasPrecision(4);

                entity.HasOne(d => d.Item)
                    .WithMany(p => p.ItemsInUnprocessedOrder)
                    .HasForeignKey(d => d.ItemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_ItemInUnprocessedOrder_Item1");

                entity.HasOne(d => d.UnprocessedOrder)
                    .WithMany(p => p.ItemsInUnprocessedOrder)
                    .HasForeignKey(d => d.UnprocessedOrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_ItemInUnprocessedOrder_UnprocessedOrder1");
            });

            modelBuilder.Entity<CalculationDB>(entity =>
            {
                entity.HasKey(e => new { e.Id })
                    .HasName("PRIMARY");

                entity.ToTable(SQLiteManagerTableNames.Calculation);

                entity.HasIndex(e => e.SupplierId, "fk_Calculation_Supplier1_idx");

                entity.HasIndex(e => e.CashierId, "fk_Calculation_Cashier1_idx");

                entity.Property(e => e.Id).HasMaxLength(36);

                entity.Property(e => e.CashierId).HasMaxLength(36);

                entity.Property(e => e.CalculationDate).HasColumnType("datetime");

                entity.Property(e => e.InvoiceNumber).HasMaxLength(50);

                entity.Property(e => e.InputTotalPrice).HasPrecision(4);

                entity.Property(e => e.OutputTotalPrice).HasPrecision(4);

                entity.HasOne(d => d.Supplier)
                    .WithMany(p => p.Calculations)
                    .HasForeignKey(d => d.SupplierId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_Calculation_Supplier1");

                entity.HasOne(d => d.Cashier)
                    .WithMany(p => p.Calculations)
                    .HasForeignKey(d => d.CashierId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_Calculation_Cashier1");
            });

            modelBuilder.Entity<CalculationItemDB>(entity =>
            {
                entity.HasKey(e => new { e.CalculationId, e.ItemId })
                    .HasName("PRIMARY");

                entity.ToTable(SQLiteManagerTableNames.CalculationItem);

                entity.HasIndex(e => e.CalculationId, "fk_CalculationItem_Calculation1_idx");

                entity.HasIndex(e => e.ItemId, "fk_CalculationItem_Item1_idx");

                entity.Property(e => e.CalculationId).HasMaxLength(36);

                entity.Property(e => e.ItemId).HasMaxLength(36);

                entity.Property(e => e.InputPrice).HasPrecision(4);

                entity.Property(e => e.OutputPrice).HasPrecision(4);

                entity.Property(e => e.Quantity).HasPrecision(4);

                entity.HasOne(d => d.Calculation)
                    .WithMany(p => p.CalculationItems)
                    .HasForeignKey(d => d.CalculationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_CalculationItem_Calculation1");

                entity.HasOne(d => d.Item)
                    .WithMany(p => p.CalculationItems)
                    .HasForeignKey(d => d.ItemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_CalculationItem_Item1");
            });
            modelBuilder.Entity<NivelacijaDB>(entity =>
            {
                entity.ToTable(SQLiteManagerTableNames.Nivelacija);
            });

            modelBuilder.Entity<ItemNivelacijaDB>(entity =>
            {
                entity.HasKey(e => new { e.IdItem, e.IdNivelacija })
                    .HasName("PRIMARY");

                entity.ToTable(SQLiteManagerTableNames.ItemNivelacija);

                entity.HasIndex(e => e.IdNivelacija, "fk_ItemNivelacija_Niv1_idx");  

                entity.HasIndex(e => e.IdItem, "fk_ItemNivelacija_Item1_idx");

                entity.Property(e => e.IdItem).HasMaxLength(36);

                entity.Property(e => e.IdNivelacija).HasMaxLength(36);

                entity.HasOne(d => d.Nivelacija)
                    .WithMany(p => p.ItemsNivelacija)
                    .HasForeignKey(d => d.IdNivelacija)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_ItemNivelacija_Niv1");

                entity.HasOne(d => d.Item)
                    .WithMany(p => p.ItemsNivelacija)
                    .HasForeignKey(d => d.IdItem)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_ItemNivelacija_Item1");
            });

            modelBuilder.Entity<KnjizenjePazaraDB>(entity =>
            {
                entity.ToTable(SQLiteManagerTableNames.KnjizenjePazara);
            });

            modelBuilder.Entity<KepDB>(entity =>
            {
                entity.ToTable(SQLiteManagerTableNames.KEP);
            });

            modelBuilder.Entity<FirmaDB>(entity =>
            {
                entity.ToTable(SQLiteManagerTableNames.Firma);
            });


            modelBuilder.Entity<PartnerDB>(entity =>
            {
                entity.ToTable(SQLiteManagerTableNames.Partner);

                entity.Property(e => e.Address).HasMaxLength(75);

                entity.Property(e => e.City).HasMaxLength(45);

                entity.Property(e => e.ContractNumber).HasMaxLength(20);

                entity.Property(e => e.Email).HasMaxLength(60);

                entity.Property(e => e.Mb)
                    .HasMaxLength(45)
                    .HasColumnName("MB");

                entity.Property(e => e.Name).HasMaxLength(95);

                entity.Property(e => e.Pib)
                    .HasMaxLength(45)
                    .HasColumnName("PIB");
            });

            modelBuilder.Entity<PocetnoStanjeDB>(entity =>
            {
                entity.ToTable(SQLiteManagerTableNames.PocetnoStanje);

                entity.Property(e => e.Id).HasMaxLength(36);
                entity.Property(e => e.PopisDate).HasColumnType("datetime");
            });


            modelBuilder.Entity<PocetnoStanjeItemDB>(entity =>
            {
                entity.HasKey(e => new { e.IdItem, e.IdPocetnoStanje })
                    .HasName("PRIMARY");

                entity.ToTable(SQLiteManagerTableNames.PocetnoStanjeItem);

                entity.HasIndex(e => e.IdPocetnoStanje, "fk_PocetnoStanjeItem_PocetnoStanje1_idx");

                entity.HasIndex(e => e.IdItem, "fk_PocetnoStanjeItem_Item1_idx");

                entity.Property(e => e.IdItem).HasMaxLength(36);

                entity.Property(e => e.IdPocetnoStanje).HasMaxLength(36);

                entity.HasOne(d => d.PocetnoStanje)
                    .WithMany(p => p.PocetnoStanjeItems)
                    .HasForeignKey(d => d.IdPocetnoStanje)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_PocetnoStanjeItem_PocetnoStanje1");

                entity.HasOne(d => d.Item)
                    .WithMany(p => p.PocetnoStanjeItems)
                    .HasForeignKey(d => d.IdItem)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_PocetnoStanjeItem_Item1");
            });

            modelBuilder.Entity<OrderTodayDB>(entity =>
            {
                entity.HasKey(e => new { e.Id })
                    .HasName("PRIMARY");

                entity.ToTable(SQLiteManagerTableNames.OrderToday);

                entity.HasIndex(e => e.CashierId, "fk_OrderToday_Cashier1_idx");

                entity.Property(e => e.Id).HasMaxLength(36);
                entity.Property(e => e.OrderDateTime).HasColumnType("datetime");
                entity.Property(e => e.Counter);
                entity.Property(e => e.CounterType);
                entity.Property(e => e.TableId);
                entity.Property(e => e.TotalPrice).HasPrecision(4);

                entity.HasOne(d => d.Cashier)
                    .WithMany(p => p.OrdersToday)
                    .HasForeignKey(d => d.CashierId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_OrderToday_Cashier1");
            });

            modelBuilder.Entity<OrderTodayItemDB>(entity =>
            {
                entity.HasKey(e => new { e.OrderTodayId, e.ItemId })
                    .HasName("PRIMARY");

                entity.ToTable(SQLiteManagerTableNames.OrderTodayItem);

                entity.HasIndex(e => e.OrderTodayId, "fk_OrderTodayItem_OrderToday1_idx");
                entity.HasIndex(e => e.ItemId, "fk_OrderTodayItem_Item1_idx");

                entity.Property(e => e.Quantity).HasPrecision(4);
                entity.Property(e => e.TotalPrice).HasPrecision(4);

                entity.HasOne(d => d.Item)
                    .WithMany(p => p.OrderTodayItems)
                    .HasForeignKey(d => d.ItemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_OrderTodayItem_Item1");

                entity.HasOne(d => d.OrderToday)
                    .WithMany(p => p.OrderTodayItems)
                    .HasForeignKey(d => d.OrderTodayId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_OrderTodayItem_OrderToday1");
            });

            modelBuilder.Entity<OtpisDB>(entity =>
            {
                entity.HasKey(e => new { e.Id })
                    .HasName("PRIMARY");

                entity.ToTable(SQLiteManagerTableNames.Otpis);

                entity.HasIndex(e => e.CashierId, "fk_Otpis_Cashier1_idx");

                entity.Property(e => e.Id).HasMaxLength(36);
                entity.Property(e => e.OtpisDate).HasColumnType("datetime");
                entity.Property(e => e.Counter);
                entity.Property(e => e.Name);

                entity.HasOne(d => d.Cashier)
                    .WithMany(p => p.Otpisi)
                    .HasForeignKey(d => d.CashierId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_Otpis_Cashier1");
            });

            modelBuilder.Entity<OtpisItemDB>(entity =>
            {
                entity.HasKey(e => new { e.OtpisId, e.ItemId })
                    .HasName("PRIMARY");

                entity.ToTable(SQLiteManagerTableNames.OtpisItem);

                entity.HasIndex(e => e.OtpisId, "fk_OtpisItem_Otpis1_idx");
                entity.HasIndex(e => e.ItemId, "fk_OtpisItem_Item1_idx");

                entity.Property(e => e.Quantity).HasPrecision(4);
                entity.Property(e => e.TotalPrice).HasPrecision(2);

                entity.HasOne(d => d.Item)
                    .WithMany(p => p.OtpisItems)
                    .HasForeignKey(d => d.ItemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_OtpisItem_Item1");

                entity.HasOne(d => d.Otpis)
                    .WithMany(p => p.OtpisItems)
                    .HasForeignKey(d => d.OtpisId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_OtpisItem_Otpis1");
            });

            modelBuilder.Entity<ItemZeljaDB>(entity =>
            {
                entity.HasKey(e => new { e.Id, e.ItemId })
                    .HasName("PRIMARY");

                entity.ToTable(SQLiteManagerTableNames.ItemZelja);

                entity.HasIndex(e => e.ItemId, "fk_ItemZelja_Item1_idx");

                entity.Property(e => e.Id).HasMaxLength(36);
                entity.Property(e => e.Zelja);

                entity.HasOne(d => d.Item)
                    .WithMany(p => p.Zelje)
                    .HasForeignKey(d => d.ItemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_ItemZelja_Item1");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
        #endregion Protected method

        #region Private methods
        private async Task<bool> Connection()
        {
            try
            {
                _connectionString = CreateConnectionString(_fileDestination);

                if (!File.Exists(_fileDestination))
                {
                    CreateDatabaseFile(_fileDestination);
                }

                await OpenConnection(_fileDestination);
                CreateTables();
                _sqliteConnection.Close();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        private static string CreateConnectionString(string connString)
        {
            SqliteConnectionStringBuilder connectionString = new SqliteConnectionStringBuilder()
            {
                DataSource = connString,
                Cache = SqliteCacheMode.Shared,
                Mode = SqliteOpenMode.ReadWriteCreate,
                DefaultTimeout = 20,
            };

            return connectionString.ConnectionString;
        }
        /// <summary>
        /// Creates database if it does not exist
        /// </summary>
        /// <param name="databaseFilePath"></param>
        /// <returns>True if database created successfully</returns>
        private static bool CreateDatabaseFile(string databaseFilePath)
        {
            try
            {
                FileStream fs = File.Create(databaseFilePath);
                fs.Close();
                //Log.Debug("SqliteDbContext - CreateDatabaseFile - Database created. Path: " + databaseFilePath);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("SqliteDbContext - CreateDatabaseFile - Database creation failed. Path: " + databaseFilePath);
                Log.Error("SqliteDbContext - CreateDatabaseFile - Exception: ", ex);
                return false;
            }
        }
        /// <summary>
        /// Opens sqlite database connection.
        /// </summary>
        /// <param name="filePath">Database file path</param>
        /// <returns>success</returns>
        public async Task<bool> OpenConnection(string filePath)
        {
            try
            {
                if (!System.IO.File.Exists(filePath))
                {
                    //Log.Debug(string.Format("SqliteDbContext - OpenConnection - File '{0}' doesn't exist.", filePath));
                    return false;
                }

                _sqliteConnection = new SqliteConnection(string.Format(_connectionString, _fileDestination));
                await _sqliteConnection.OpenAsync();

                //Log.Debug("SqliteDbContext - OpenConnection - Connection to sqlite database opened...");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("SqliteDbContext - OpenConnection - Open conn  :  " + ex);
                return false;
            }
        }
        /// <summary>
        /// Creates tables in SQLite database
        /// <para>
        /// Checks if each tables exist in the database. If not, creates them one by one.
        /// </para>
        /// </summary>
        private void CreateTables()
        {
            try
            {
                string sql = string.Empty;
                if (!TableExists(SQLiteManagerTableNames.Supergroup, _sqliteConnection))
                {
                    sql = string.Format("CREATE TABLE {0} ( " +
                        "'Id'	INTEGER NOT NULL, " +
                        "'Name'    TEXT NOT NULL, " +
                        "PRIMARY KEY(Id AUTOINCREMENT) " +
                        "); ", SQLiteManagerTableNames.Supergroup);
                    CreateTable(SQLiteManagerTableNames.Supergroup, sql);
                }
                if (!TableExists(SQLiteManagerTableNames.ItemGroup, _sqliteConnection))
                {
                    sql = string.Format("CREATE TABLE {0} ( " +
                        "'Id'	INTEGER NOT NULL, " +
                        "'IdSupergroup'    INTEGER NOT NULL, " +
                        "'Name'    TEXT NOT NULL, " +
                        "PRIMARY KEY(Id AUTOINCREMENT), " +
                        "CONSTRAINT `fk_ItemGroup_Supergroup` " +
                        "  FOREIGN KEY(IdSupergroup) " +
                        "  REFERENCES Supergroup (Id) " +
                        "  ON DELETE NO ACTION " +
                        "  ON UPDATE NO ACTION" +
                        "); ", SQLiteManagerTableNames.ItemGroup);
                    CreateTable(SQLiteManagerTableNames.ItemGroup, sql);
                }
                if (!TableExists(SQLiteManagerTableNames.Item, _sqliteConnection))
                {
                    sql = string.Format("CREATE TABLE {0} ( " +
                        "'Id'	TEXT NOT NULL, " +
                        "'IdItemGroup'   INTEGER NOT NULL, " +
                        "'Name'    TEXT NOT NULL, " + 
                        "'SellingUnitPrice'   NUMERIC NOT NULL, " +
                        "'SellingNocnaUnitPrice'   NUMERIC NOT NULL DEFAULT 0, " +
                        "'SellingDnevnaUnitPrice'   NUMERIC NOT NULL DEFAULT 0, " +
                        "'InputUnitPrice'   NUMERIC, " +
                        "'Label'   TEXT NOT NULL, " +
                        "'JM'   TEXT NOT NULL, " +
                        "'TotalQuantity'   NUMERIC NOT NULL, " +
                        "'AlarmQuantity'   NUMERIC NOT NULL, " +
                        "'IdNorm'   INTEGER, " +
                        "'DisableItem'   INTEGER NOT NULL DEFAULT 0, " +
                        "'IsCheckedZabraniPopust'   INTEGER NOT NULL DEFAULT 0, " + 
                        "PRIMARY KEY(Id), " +
                        "FOREIGN KEY(IdNorm) REFERENCES Norm(Id), " +
                        "CONSTRAINT `fk_Item_ItemGroup` " +
                        "  FOREIGN KEY(IdItemGroup) " +
                        "  REFERENCES ItemGroup (Id) " +
                        "  ON DELETE NO ACTION " +
                        "  ON UPDATE NO ACTION" +
                        "); ", SQLiteManagerTableNames.Item);
                    CreateTable(SQLiteManagerTableNames.Item, sql);
                }
                else
                {
                    try
                    {
                        sql = string.Format("ALTER TABLE {0}  " +
                           "ADD COLUMN 'IsCheckedZabraniPopust'    INTEGER NOT NULL DEFAULT 0" +
                           "; ", SQLiteManagerTableNames.Item);
                        CreateTable(SQLiteManagerTableNames.Item, sql);

                        sql = string.Format("ALTER TABLE {0}  " +
                           "ADD COLUMN 'DisableItem'    INTEGER NOT NULL DEFAULT 0" +
                           "; ", SQLiteManagerTableNames.Item);
                        CreateTable(SQLiteManagerTableNames.Item, sql);

                        sql = string.Format("ALTER TABLE {0}  " +
                           "ADD COLUMN 'SellingDnevnaUnitPrice'    NUMERIC NOT NULL DEFAULT 0" +
                           "; ", SQLiteManagerTableNames.Item);
                        CreateTable(SQLiteManagerTableNames.Item, sql);

                        sql = string.Format("ALTER TABLE {0}  " +
                           "ADD COLUMN 'SellingNocnaUnitPrice'    NUMERIC NOT NULL DEFAULT 0" +
                           "; ", SQLiteManagerTableNames.Item);
                        CreateTable(SQLiteManagerTableNames.Item, sql);

                        sql = string.Format("ALTER TABLE {0}  " +
                           "RENAME COLUMN 'UnitPrice' TO 'SellingUnitPrice'" +
                           "; ", SQLiteManagerTableNames.Item);
                        CreateTable(SQLiteManagerTableNames.Item, sql);

                        sql = string.Format("ALTER TABLE {0}  " +
                           "ADD COLUMN 'InputUnitPrice'    NUMERIC" +
                           "; ", SQLiteManagerTableNames.Item);
                        CreateTable(SQLiteManagerTableNames.Item, sql);
                    }
                    catch (Exception e)
                    {

                    }
                }
                if (!TableExists(SQLiteManagerTableNames.Cashier, _sqliteConnection))
                {
                    sql = string.Format("CREATE TABLE {0} (" +
                        "'Id' TEXT NOT NULL," +
                        "'Type'   INTEGER NOT NULL," +
                        "'Name'   TEXT NOT NULL," +
                        "'SmartCardNumber'   TEXT," +
                        "'Jmbg'   TEXT," + 
                        "'City'    TEXT," +
                        "'Address'   TEXT," +
                        "'ContactNumber'   TEXT," +
                        "'Email'    TEXT," +
                        "PRIMARY KEY(Id)" +
                        "); ", SQLiteManagerTableNames.Cashier);
                    CreateTable(SQLiteManagerTableNames.Cashier, sql);
                }
                else
                {
                    try
                    {
                        sql = string.Format("ALTER TABLE {0}  " +
                           "ADD COLUMN 'SmartCardNumber'    TEXT" +
                           "; ", SQLiteManagerTableNames.Cashier);
                        CreateTable(SQLiteManagerTableNames.Cashier, sql);
                    }
                    catch (Exception e)
                    {

                    }
                }
                if (!TableExists(SQLiteManagerTableNames.Invoice, _sqliteConnection))
                {
                    sql = string.Format("CREATE TABLE {0} (" +
                        "'Id' TEXT NOT NULL," +
                        "'DateAndTimeOfIssue'   TEXT," +
                        "'Cashier'   TEXT," +
                        "'BuyerId'   TEXT," +
                        "'BuyerName'   TEXT," +
                        "'BuyerAddress'   TEXT," +
                        "'BuyerCostCenterId'   TEXT," +
                        "'InvoiceType'   INTEGER," +
                        "'TransactionType'   INTEGER," +
                        "'ReferentDocumentNumber'   TEXT," +
                        "'ReferentDocumentDT'   TEXT," +
                        "'InvoiceNumber'   TEXT," +
                        "'RequestedBy'   TEXT," +
                        "'InvoiceNumberResult'   TEXT," +
                        "'SdcDateTime'   TEXT," +
                        "'InvoiceCounter'    TEXT ," +
                        "'InvoiceCounterExtension'   TEXT," +
                        "'SignedBy'  TEXT," +
                        "'EncryptedInternalData' TEXT," +
                        "'Signature' TEXT," +
                        "'TotalCounter'  INTEGER," +
                        "'TransactionTypeCounter'    INTEGER," +
                        "'TotalAmount'   NUMERIC," +
                        "'TaxGroupRevision'  INTEGER," +
                        "'BusinessName'  TEXT," +
                        "'Tin'   TEXT," +
                        "'LocationName'  TEXT," +
                        "'Address'   TEXT," +
                        "'District'  TEXT," +
                        "'Mrc'   TEXT," +
                        "'KnjizenjePazaraId'   TEXT," +
                        "'SendEfaktura'   INTEGER DEFAULT 0," + 
                        "PRIMARY KEY(Id)" +
                        "); ", SQLiteManagerTableNames.Invoice);
                    CreateTable(SQLiteManagerTableNames.Invoice, sql);
                }
                else
                {
                    try
                    {
                        sql = string.Format("ALTER TABLE {0}  " +
                           "ADD COLUMN 'SendEfaktura'    INTEGER DEFAULT 0 " +
                           "; ", SQLiteManagerTableNames.Invoice);
                        CreateTable(SQLiteManagerTableNames.Invoice, sql);

                        sql = string.Format("ALTER TABLE {0}  " +
                           "ADD COLUMN 'KnjizenjePazaraId'    TEXT " +
                           "; ", SQLiteManagerTableNames.Invoice);
                        CreateTable(SQLiteManagerTableNames.Invoice, sql);
                    }
                    catch (Exception e)
                    {
                        Log.Error("SqliteDbContext - CreateTables - Invoice - Polje 'KnjizenjePazaraId' je vec dodato -> ", e);
                    }
                }
                if (!TableExists(SQLiteManagerTableNames.ItemInvoice, _sqliteConnection))
                {
                    sql = string.Format("CREATE TABLE {0} (" +
                        "'Id'    INTEGER NOT NULL, " +
                        "'IsSirovina'    INTEGER, " +
                        "'InvoiceId'  TEXT NOT NULL," +
                        "'Quantity' NUMERIC," +
                        "'TotalAmout' NUMERIC," +
                        "'Name' TEXT," +
                        "'UnitPrice' NUMERIC," +
                        "'OriginalUnitPrice' NUMERIC," +
                        "'InputUnitPrice' NUMERIC," + 
                        "'Label' TEXT," +
                        "'ItemCode' TEXT," +
                        "PRIMARY KEY(Id, InvoiceId), " +
                        "CONSTRAINT `fk_ItemInvoice_Invoice1` " +
                        "    FOREIGN KEY(InvoiceId) " +
                        "    REFERENCES Invoice (Id) " +
                        "    ON DELETE NO ACTION " +
                        "    ON UPDATE NO ACTION" +
                        "); ", SQLiteManagerTableNames.ItemInvoice);
                    CreateTable(SQLiteManagerTableNames.ItemInvoice, sql);
                }
                else
                {
                    try
                    {
                        sql = string.Format("ALTER TABLE {0}  " +
                           "ADD COLUMN 'OriginalUnitPrice'    NUMERIC " +
                           "; ", SQLiteManagerTableNames.ItemInvoice);
                        CreateTable(SQLiteManagerTableNames.ItemInvoice, sql);
                        sql = string.Format("ALTER TABLE {0}  " +
                           "ADD COLUMN 'IsSirovina'    INTEGER " +
                           "; ", SQLiteManagerTableNames.ItemInvoice);
                        CreateTable(SQLiteManagerTableNames.ItemInvoice, sql);
                        sql = string.Format("ALTER TABLE {0}  " +
                           "ADD COLUMN 'InputUnitPrice'    NUMERIC " +
                           "; ", SQLiteManagerTableNames.ItemInvoice);
                        CreateTable(SQLiteManagerTableNames.ItemInvoice, sql);
                    }
                    catch(Exception e)
                    {
                        Log.Error("SqliteDbContext - CreateTables - ItemInvoice - Polje 'OriginalUnitPrice' je vec dodato -> ", e);
                    }
                }
                if (!TableExists(SQLiteManagerTableNames.PaymentInvoice, _sqliteConnection))
                {
                    sql = string.Format("CREATE TABLE {0} (" +
                        "'InvoiceId'    TEXT NOT NULL," +
                        "'PaymentType'  INTEGER NOT NULL," +
                        "'Amout'  NUMERIC," +
                        "PRIMARY KEY(InvoiceId, PaymentType), " +
                        "CONSTRAINT `fk_PaymentInvoice_Invoice1`" +
                        "    FOREIGN KEY(InvoiceId)" +
                        "    REFERENCES Invoice (Id)" +
                        "    ON DELETE NO ACTION" +
                        "    ON UPDATE NO ACTION" +
                        "); ", SQLiteManagerTableNames.PaymentInvoice);
                    CreateTable(SQLiteManagerTableNames.PaymentInvoice, sql);
                }
                if (!TableExists(SQLiteManagerTableNames.TaxItemInvoice, _sqliteConnection))
                {
                    sql = string.Format("CREATE TABLE {0} (" +
                        "'InvoiceId'  TEXT NOT NULL," +
                        "'Label'  TEXT NOT NULL," +
                        "'CategoryName'  TEXT," +
                        "'CategoryType'  INTEGER," +
                        "'Rate'  NUMERIC," +
                        "'Amount'  NUMERIC," +
                        "PRIMARY KEY(InvoiceId, Label), " +
                        "CONSTRAINT `fk_TaxItemInvoice_Invoice1`" +
                        "    FOREIGN KEY(InvoiceId)" +
                        "    REFERENCES Invoice(Id)" +
                        "    ON DELETE NO ACTION" +
                        "    ON UPDATE NO ACTION" +
                        "); ", SQLiteManagerTableNames.TaxItemInvoice);
                    CreateTable(SQLiteManagerTableNames.TaxItemInvoice, sql);
                }
                if (!TableExists(SQLiteManagerTableNames.SmartCard, _sqliteConnection))
                {
                    sql = string.Format("CREATE TABLE {0} (" +
                        "'Id'  TEXT NOT NULL," +
                        "'CashierId'  TEXT NOT NULL," +
                        "PRIMARY KEY(Id), " +
                        "CONSTRAINT `fk_SmartCard_Cashier1`" +
                        "    FOREIGN KEY(CashierId)" +
                        "    REFERENCES Cashier(Id)" +
                        "    ON DELETE NO ACTION" +
                        "    ON UPDATE NO ACTION" +
                        "); ", SQLiteManagerTableNames.SmartCard);
                    CreateTable(SQLiteManagerTableNames.SmartCard, sql);
                }
                if (!TableExists(SQLiteManagerTableNames.Supplier, _sqliteConnection))
                {
                    sql = string.Format("CREATE TABLE {0} ( " +
                        "'Id'	INTEGER NOT NULL, " +
                        "'PIB'    TEXT NOT NULL, " +
                        "'Name'    TEXT NOT NULL, " +
                        "'Address'    TEXT, " +
                        "'ContractNumber'    TEXT, " +
                        "'Email'    TEXT, " +
                        "'City'    TEXT, " +
                        "'MB'    TEXT, " +
                        "PRIMARY KEY(Id AUTOINCREMENT) " +
                        "); ", SQLiteManagerTableNames.Supplier);
                    CreateTable(SQLiteManagerTableNames.Supplier, sql);
                }
                if (!TableExists(SQLiteManagerTableNames.Procurement, _sqliteConnection))
                {
                    sql = string.Format("CREATE TABLE {0} (" +
                        "'Id'  TEXT NOT NULL," +
                        "'SupplierId'  INTEGER NOT NULL," +
                        "'ItemId'  TEXT NOT NULL," +
                        "'DateProcurement'  TEXT NOT NULL," +
                        "'Quantity'  NUMERIC NOT NULL," +
                        "'UnitPrice'  NUMERIC NOT NULL," +
                        "PRIMARY KEY(Id), " +
                        "CONSTRAINT `fk_Procurement_Supplier1`" +
                        "    FOREIGN KEY(SupplierId)" +
                        "    REFERENCES Supplier(Id)" +
                        "    ON DELETE NO ACTION" +
                        "    ON UPDATE NO ACTION, " +
                        "CONSTRAINT `fk_Procurement_Item1`" +
                        "    FOREIGN KEY(ItemId)" +
                        "    REFERENCES Item(Id)" +
                        "    ON DELETE NO ACTION" +
                        "    ON UPDATE NO ACTION" +
                        "); ", SQLiteManagerTableNames.Procurement);
                    CreateTable(SQLiteManagerTableNames.Procurement, sql);
                }
                if (!TableExists(SQLiteManagerTableNames.PartHall, _sqliteConnection))
                {
                    sql = string.Format("CREATE TABLE {0} ( " +
                        "'Id'	INTEGER NOT NULL, " +
                        "'Name'    TEXT NOT NULL, " +
                        "'Image'    TEXT, " +
                        "PRIMARY KEY(Id AUTOINCREMENT) " +
                        "); ", SQLiteManagerTableNames.PartHall);
                    CreateTable(SQLiteManagerTableNames.PartHall, sql);
                }
                if (!TableExists(SQLiteManagerTableNames.PaymentPlace, _sqliteConnection))
                {
                    sql = string.Format("CREATE TABLE {0} (" +
                        "'Id'  INTEGER NOT NULL," +
                        "'PartHallId'  INTEGER NOT NULL," +
                        "'LeftCanvas'  NUMERIC," +
                        "'TopCanvas'  NUMERIC," +
                        "'Type'  INTEGER," +
                        "'Width'  NUMERIC," +
                        "'Height'  NUMERIC," +
                        "'X_Mobi'  NUMERIC," +
                        "'Y_Mobi'  NUMERIC," +
                        "'WidthMobi'  NUMERIC," +
                        "'HeightMobi'  NUMERIC," +
                        "'Popust'  NUMERIC NOT NULL DEFAULT 0," +
                        "'Name'  TEXT," +
                        "PRIMARY KEY(Id AUTOINCREMENT), " +
                        "CONSTRAINT `fk_Table_PartHall1`" +
                        "    FOREIGN KEY(PartHallId)" +
                        "    REFERENCES PartHall(Id)" +
                        "    ON DELETE NO ACTION" +
                        "    ON UPDATE NO ACTION" +
                        "); ", SQLiteManagerTableNames.PaymentPlace);
                    CreateTable(SQLiteManagerTableNames.PaymentPlace, sql);
                }
                else
                {
                    try
                    {
                        sql = string.Format("ALTER TABLE {0}  " +
                           "ADD COLUMN 'Name'    TEXT " +
                           "; ", SQLiteManagerTableNames.PaymentPlace);
                        CreateTable(SQLiteManagerTableNames.PaymentPlace, sql);
                        sql = string.Format("ALTER TABLE {0}  " +
                           "ADD COLUMN 'Popust'    NUMERIC NOT NULL DEFAULT 0 " +
                           "; ", SQLiteManagerTableNames.PaymentPlace);
                        CreateTable(SQLiteManagerTableNames.PaymentPlace, sql);
                        sql = string.Format("ALTER TABLE {0}  " +
                           "ADD COLUMN 'X_Mobi'    NUMERIC " +
                           "; ", SQLiteManagerTableNames.PaymentPlace);
                        CreateTable(SQLiteManagerTableNames.PaymentPlace, sql);
                        sql = string.Format("ALTER TABLE {0}  " +
                           "ADD COLUMN 'Y_Mobi'    NUMERIC " +
                           "; ", SQLiteManagerTableNames.PaymentPlace);
                        CreateTable(SQLiteManagerTableNames.PaymentPlace, sql);
                        sql = string.Format("ALTER TABLE {0}  " +
                           "ADD COLUMN 'WidthMobi'    NUMERIC " +
                           "; ", SQLiteManagerTableNames.PaymentPlace);
                        CreateTable(SQLiteManagerTableNames.PaymentPlace, sql);
                        sql = string.Format("ALTER TABLE {0}  " +
                           "ADD COLUMN 'HeightMobi'    NUMERIC " +
                           "; ", SQLiteManagerTableNames.PaymentPlace);
                        CreateTable(SQLiteManagerTableNames.PaymentPlace, sql);
                    }
                    catch (Exception e)
                    {
                        Log.Error("SqliteDbContext - CreateTables - PaymentPlace - Polje je vec dodato -> ", e);
                    }
                }
                if (!TableExists(SQLiteManagerTableNames.Order, _sqliteConnection))
                {
                    sql = string.Format("CREATE TABLE '{0}' (" +
                        "'PaymentPlaceId'  INTEGER NOT NULL, " +
                        "'InvoiceId'  TEXT NOT NULL, " +
                        "'CashierId'  TEXT NOT NULL, " +
                        "PRIMARY KEY(PaymentPlaceId, InvoiceId, CashierId), " +
                        "CONSTRAINT `fk_Order_Table1`" +
                        "    FOREIGN KEY(PaymentPlaceId)" +
                        "    REFERENCES PaymentPlace(Id)" +
                        "    ON DELETE NO ACTION" +
                        "    ON UPDATE NO ACTION, " +
                        "CONSTRAINT `fk_Order_Invoice1`" +
                        "    FOREIGN KEY(InvoiceId)" +
                        "    REFERENCES Invoice(Id)" +
                        "    ON DELETE NO ACTION" +
                        "    ON UPDATE NO ACTION," +
                        "CONSTRAINT `fk_Order_Cashier1`" +
                        "    FOREIGN KEY(CashierId)" +
                        "    REFERENCES Cashier(Id)" +
                        "    ON DELETE NO ACTION" +
                        "    ON UPDATE NO ACTION" +
                        "); ", SQLiteManagerTableNames.Order);
                    CreateTable(SQLiteManagerTableNames.Order, sql);
                }
                if (!TableExists(SQLiteManagerTableNames.Norm, _sqliteConnection))
                {
                    sql = string.Format("CREATE TABLE '{0}' (" +
                        "'Id'  INTEGER NOT NULL, " +
                        "PRIMARY KEY(Id AUTOINCREMENT)" +
                        "); ", SQLiteManagerTableNames.Norm);
                    CreateTable(SQLiteManagerTableNames.Norm, sql);
                }
                if (!TableExists(SQLiteManagerTableNames.ItemInNorm, _sqliteConnection))
                {
                    sql = string.Format("CREATE TABLE '{0}' (" +
                        "'IdNorm'  INTEGER NOT NULL, " +
                        "'IdItem'  TEXT NOT NULL, " +
                        "'Quantity'  NUMERIC NOT NULL, " +
                        "PRIMARY KEY(IdNorm, IdItem), " +
                        "CONSTRAINT `fk_ItemInNorm_Norm1`" +
                        "    FOREIGN KEY(IdNorm)" +
                        "    REFERENCES Norm(Id)" +
                        "    ON DELETE NO ACTION" +
                        "    ON UPDATE NO ACTION, " +
                        "CONSTRAINT `fk_ItemInNorm_Item`" +
                        "    FOREIGN KEY(IdItem)" +
                        "    REFERENCES Item(Id)" +
                        "    ON DELETE NO ACTION" +
                        "    ON UPDATE NO ACTION" +
                        "); ", SQLiteManagerTableNames.ItemInNorm);
                    CreateTable(SQLiteManagerTableNames.ItemInNorm, sql);
                }
                if (!TableExists(SQLiteManagerTableNames.UnprocessedOrder, _sqliteConnection))
                {
                    sql = string.Format("CREATE TABLE '{0}' (" +
                        "'Id'    TEXT NOT NULL, " +
                        "'PaymentPlaceId'    INTEGER NOT NULL, " +
                        "'CashierId' TEXT NOT NULL, " +
                        "'TotalAmount'   NUMERIC, " +
                        "PRIMARY KEY(Id), " +
                        "CONSTRAINT `fk_UnprocessedOrder_Cashier1`" +
                        "   FOREIGN KEY(CashierId)" +
                        "   REFERENCES Cashier(Id)" +
                        "   ON DELETE NO ACTION" +
                        "   ON UPDATE NO ACTION, " +
                        "CONSTRAINT `fk_UnprocessedOrder_PaymentPlace1`" +
                        "   FOREIGN KEY(PaymentPlaceId)" +
                        "   REFERENCES PaymentPlace(Id)" +
                        "   ON DELETE NO ACTION" +
                        "   ON UPDATE NO ACTION" +
                        "); ", SQLiteManagerTableNames.UnprocessedOrder);
                    CreateTable(SQLiteManagerTableNames.UnprocessedOrder, sql);
                }
                if (!TableExists(SQLiteManagerTableNames.ItemInUnprocessedOrder, _sqliteConnection))
                {
                    sql = string.Format("CREATE TABLE '{0}' (" +
                        "'ItemId'    TEXT NOT NULL, " +
                        "'UnprocessedOrderId'    TEXT NOT NULL, " +
                        "'Quantity'  NUMERIC NOT NULL, " +
                        "PRIMARY KEY(ItemId, UnprocessedOrderId), " +
                        "CONSTRAINT `fk_ItemInUnprocessedOrder_UnprocessedOrder1` " +
                        "   FOREIGN KEY(UnprocessedOrderId)" +
                        "   REFERENCES UnprocessedOrder(Id)" +
                        "   ON DELETE NO ACTION" +
                        "   ON UPDATE NO ACTION, " +
                        "CONSTRAINT `fk_ItemInUnprocessedOrder_Item1` " +
                        "   FOREIGN KEY(ItemId)" +
                        "   REFERENCES Item(Id)" +
                        "   ON DELETE NO ACTION" +
                        "   ON UPDATE NO ACTION" +
                        "); ", SQLiteManagerTableNames.ItemInUnprocessedOrder);
                    CreateTable(SQLiteManagerTableNames.ItemInUnprocessedOrder, sql);
                }
                if (!TableExists(SQLiteManagerTableNames.Calculation, _sqliteConnection))
                {
                    sql = string.Format("CREATE TABLE '{0}' (" +
                        "'Id'	TEXT NOT NULL, " +
                        "'SupplierId'    INTEGER NOT NULL, " +
                        "'CashierId' TEXT NOT NULL, " +
                        "'Counter'    INTEGER, " +
                        "'CalculationDate'   TEXT NOT NULL, " +
                        "'InvoiceNumber' TEXT, " +
                        "'InputTotalPrice'   NUMERIC NOT NULL, " +
                        "'OutputTotalPrice'   NUMERIC NOT NULL, " +
                        "PRIMARY KEY(Id), " +
                        "CONSTRAINT `fk_Calculation_Cashier1` " +
                        "   FOREIGN KEY(CashierId)" +
                        "   REFERENCES Cashier(Id)" +
                        "   ON DELETE NO ACTION" +
                        "   ON UPDATE NO ACTION, " +
                        "CONSTRAINT `fk_Calculation_Supplier1` " +
                        "   FOREIGN KEY(SupplierId)" +
                        "   REFERENCES Supplier(Id)" +
                        "   ON DELETE NO ACTION" +
                        "   ON UPDATE NO ACTION" +
                        "); ", SQLiteManagerTableNames.Calculation);
                    CreateTable(SQLiteManagerTableNames.Calculation, sql);
                }
                else
                {
                    try
                    {
                        sql = string.Format("ALTER TABLE {0}  " +
                           "ADD COLUMN 'Counter'    INTEGER " +
                           "; ", SQLiteManagerTableNames.Calculation);
                        CreateTable(SQLiteManagerTableNames.Calculation, sql);
                    }
                    catch (Exception e)
                    {
                        Log.Error("SqliteDbContext - CreateTables - Calculation - Polje 'Counter' je vec dodato -> ", e);
                    }
                }
                if (!TableExists(SQLiteManagerTableNames.CalculationItem, _sqliteConnection))
                {
                    sql = string.Format("CREATE TABLE '{0}' (" +
                        "'CalculationId'	TEXT NOT NULL, " +
                        "'ItemId'    TEXT NOT NULL, " +
                        "'InputPrice' NUMERIC NOT NULL, " +
                        "'OutputPrice' NUMERIC NOT NULL, " +
                        "'Quantity'   NUMERIC NOT NULL, " +
                        "PRIMARY KEY(CalculationId, ItemId), " +
                        "CONSTRAINT `fk_CalculationItem_Item1` " +
                        "   FOREIGN KEY(ItemId)" +
                        "   REFERENCES Item(Id)" +
                        "   ON DELETE NO ACTION" +
                        "   ON UPDATE NO ACTION, " +
                        "CONSTRAINT `fk_CalculationItem_Calculation1` " +
                        "   FOREIGN KEY(CalculationId)" +
                        "   REFERENCES Calculation(Id)" +
                        "   ON DELETE NO ACTION" +
                        "   ON UPDATE NO ACTION" +
                        "); ", SQLiteManagerTableNames.CalculationItem);
                    CreateTable(SQLiteManagerTableNames.CalculationItem, sql);
                }
                if (!TableExists(SQLiteManagerTableNames.Nivelacija, _sqliteConnection))
                {
                    sql = string.Format("CREATE TABLE '{0}' (" +
                        "'Id'  TEXT NOT NULL, " +
                        "'Counter'    INTEGER NOT NULL, " +
                        "'Type'    INTEGER NOT NULL, " + 
                        "'DateNivelacije'    TEXT NOT NULL, " +
                        "'Description'    TEXT, " +
                        "PRIMARY KEY(Id)" +
                        "); ", SQLiteManagerTableNames.Nivelacija);
                    CreateTable(SQLiteManagerTableNames.Nivelacija, sql);
                }
                if (!TableExists(SQLiteManagerTableNames.ItemNivelacija, _sqliteConnection))
                {
                    sql = string.Format("CREATE TABLE '{0}' (" +
                        "'IdNivelacija'  TEXT NOT NULL, " +
                        "'IdItem'  TEXT NOT NULL, " +
                        "'OldUnitPrice'  NUMERIC NOT NULL, " +
                        "'NewUnitPrice'  NUMERIC NOT NULL, " +
                        "'TotalQuantity'  NUMERIC NOT NULL, " +
                        "'StopaPDV'  NUMERIC NOT NULL, " +
                        "PRIMARY KEY(IdNivelacija, IdItem), " +
                        "CONSTRAINT `fk_ItemNivelacija_Niv1`" +
                        "    FOREIGN KEY(IdNivelacija)" +
                        "    REFERENCES Nivelacija(Id)" +
                        "    ON DELETE NO ACTION" +
                        "    ON UPDATE NO ACTION, " +
                        "CONSTRAINT `fk_ItemNivelacija_Item1`" +
                        "    FOREIGN KEY(IdItem)" +
                        "    REFERENCES Item(Id)" +
                        "    ON DELETE NO ACTION" +
                        "    ON UPDATE NO ACTION" +
                        "); ", SQLiteManagerTableNames.ItemNivelacija);
                    CreateTable(SQLiteManagerTableNames.ItemNivelacija, sql);
                }
                if (!TableExists(SQLiteManagerTableNames.KnjizenjePazara, _sqliteConnection))
                {
                    sql = string.Format("CREATE TABLE '{0}' (" +
                        "'Id'  TEXT NOT NULL, " +
                        "'Description'    TEXT NOT NULL, " +
                        "'IssueDateTime'    TEXT NOT NULL, " +
                        "'NormalSaleCash'    NUMERIC NOT NULL, " +
                        "'NormalSaleCard'    NUMERIC NOT NULL, " +
                        "'NormalSaleWireTransfer'    NUMERIC NOT NULL, " +
                        "'NormalRefundCash'    NUMERIC NOT NULL, " +
                        "'NormalRefundCard'    NUMERIC NOT NULL, " +
                        "'NormalRefundWireTransfer'    NUMERIC NOT NULL, " +
                        "PRIMARY KEY(Id)" +
                        "); ", SQLiteManagerTableNames.KnjizenjePazara);
                    CreateTable(SQLiteManagerTableNames.KnjizenjePazara, sql);
                }
                if (!TableExists(SQLiteManagerTableNames.KEP, _sqliteConnection))
                {
                    sql = string.Format("CREATE TABLE '{0}' (" +
                        "'Id'  TEXT NOT NULL, " +
                        "'Description'    TEXT NOT NULL, " +
                        "'KepDate'    TEXT NOT NULL, " +
                        "'Type'    INTEGER NOT NULL, " +
                        "'Zaduzenje'    NUMERIC NOT NULL, " +
                        "'Razduzenje'    NUMERIC NOT NULL, " +
                        "PRIMARY KEY(Id)" +
                        "); ", SQLiteManagerTableNames.KEP);
                    CreateTable(SQLiteManagerTableNames.KEP, sql);
                }
                if (!TableExists(SQLiteManagerTableNames.Firma, _sqliteConnection))
                {
                    sql = string.Format("CREATE TABLE '{0}' (" +
                        "'Id'  INTEGER NOT NULL, " +
                        "'Name'    TEXT, " +
                        "'Pib'    TEXT, " +
                        "'MB'    TEXT, " +
                        "'NamePP'    TEXT, " +
                        "'AddressPP'    TEXT, " +
                        "'Number'    TEXT, " +
                        "'Email'    TEXT, " +
                        "'BankAcc'    TEXT, " +
                        "'AuthenticationKey'    TEXT, " + 
                        "PRIMARY KEY(Id AUTOINCREMENT)" +
                        "); ", SQLiteManagerTableNames.Firma);
                    CreateTable(SQLiteManagerTableNames.Firma, sql);
                }
                if (!TableExists(SQLiteManagerTableNames.Partner, _sqliteConnection))
                {
                    sql = string.Format("CREATE TABLE {0} ( " +
                        "'Id'	INTEGER NOT NULL, " +
                        "'PIB'    TEXT NOT NULL, " +
                        "'Name'    TEXT NOT NULL, " +
                        "'Address'    TEXT, " +
                        "'ContractNumber'    TEXT, " +
                        "'Email'    TEXT, " +
                        "'City'    TEXT, " +
                        "'MB'    TEXT, " +
                        "PRIMARY KEY(Id AUTOINCREMENT) " +
                        "); ", SQLiteManagerTableNames.Partner);
                    CreateTable(SQLiteManagerTableNames.Partner, sql);
                }
                if (!TableExists(SQLiteManagerTableNames.PocetnoStanje, _sqliteConnection))
                {
                    sql = string.Format("CREATE TABLE '{0}' (" +
                        "'Id'  TEXT NOT NULL, " +
                        "'PopisDate'    TEXT NOT NULL, " +
                        "'Description'    TEXT, " +
                        "PRIMARY KEY(Id)" +
                        "); ", SQLiteManagerTableNames.PocetnoStanje);
                    CreateTable(SQLiteManagerTableNames.PocetnoStanje, sql);
                }
                if (!TableExists(SQLiteManagerTableNames.PocetnoStanjeItem, _sqliteConnection))
                {
                    sql = string.Format("CREATE TABLE '{0}' (" +
                        "'IdPocetnoStanje'  TEXT NOT NULL, " +
                        "'IdItem'  TEXT NOT NULL, " +
                        "'OldQuantity'  NUMERIC NOT NULL, " +
                        "'NewQuantity'  NUMERIC NOT NULL, " +
                        "'InputPrice'  NUMERIC NOT NULL, " +
                        "'OutputPrice'  NUMERIC NOT NULL, " +
                        "PRIMARY KEY(IdPocetnoStanje, IdItem), " +
                        "CONSTRAINT `fk_PocetnoStanjeItem_PocetnoStanje1`" +
                        "    FOREIGN KEY(IdPocetnoStanje)" +
                        "    REFERENCES PocetnoStanje(Id)" +
                        "    ON DELETE NO ACTION" +
                        "    ON UPDATE NO ACTION, " +
                        "CONSTRAINT `fk_PocetnoStanjeItem_Item1`" +
                        "    FOREIGN KEY(IdItem)" +
                        "    REFERENCES Item(Id)" +
                        "    ON DELETE NO ACTION" +
                        "    ON UPDATE NO ACTION" +
                        "); ", SQLiteManagerTableNames.PocetnoStanjeItem);
                    CreateTable(SQLiteManagerTableNames.PocetnoStanjeItem, sql);
                }
                if (!TableExists(SQLiteManagerTableNames.OrderToday, _sqliteConnection))
                {
                    sql = string.Format("CREATE TABLE '{0}' (" +
                        "'Id'  TEXT NOT NULL, " +
                        "'CashierId'  TEXT NOT NULL, " +
                        "'OrderDateTime'  TEXT NOT NULL, " +
                        "'Counter'  INTEGER NOT NULL, " +
                        "'CounterType'  INTEGER NOT NULL DEFAULT 0, " +
                        "'Name'  TEXT, " +
                        "'TotalPrice'  NUMERIC NOT NULL, " +
                        "'TableId'  INTEGER, " +
                        "PRIMARY KEY(Id), " +
                        "CONSTRAINT `fk_OrderToday_Cashier1`" +
                        "    FOREIGN KEY(CashierId)" +
                        "    REFERENCES Cashier(Id)" +
                        "    ON DELETE NO ACTION" +
                        "    ON UPDATE NO ACTION" +
                        "); ", SQLiteManagerTableNames.OrderToday);
                    CreateTable(SQLiteManagerTableNames.OrderToday, sql);
                }
                else
                {
                    try
                    {
                        sql = string.Format("ALTER TABLE {0}  " +
                           "ADD COLUMN 'CounterType'    INTEGER NOT NULL DEFAULT 0 " +
                           "; ", SQLiteManagerTableNames.OrderToday);
                        CreateTable(SQLiteManagerTableNames.OrderToday, sql);
                        sql = string.Format("ALTER TABLE {0}  " +
                           "ADD COLUMN 'TableId'    INTEGER " +
                           "; ", SQLiteManagerTableNames.OrderToday);
                        CreateTable(SQLiteManagerTableNames.OrderToday, sql);
                    }
                    catch (Exception e)
                    {
                        Log.Error("SqliteDbContext - CreateTables - OrderToday - Polje 'TableId' je vec dodato -> ", e);
                    }
                }
                if (!TableExists(SQLiteManagerTableNames.OrderTodayItem, _sqliteConnection))
                {
                    sql = string.Format("CREATE TABLE '{0}' (" +
                        "'OrderTodayId'  TEXT NOT NULL, " +
                        "'ItemId'  TEXT NOT NULL, " +
                        "'Quantity'  NUMERIC NOT NULL, " +
                        "'TotalPrice'  NUMERIC NOT NULL, " +
                        "PRIMARY KEY(OrderTodayId, ItemId), " +
                        "CONSTRAINT `fk_OrderTodayItem_OrderToday1`" +
                        "    FOREIGN KEY(OrderTodayId)" +
                        "    REFERENCES OrderToday(Id)" +
                        "    ON DELETE NO ACTION" +
                        "    ON UPDATE NO ACTION, " +
                        "CONSTRAINT `fk_OrderTodayItem_Item1`" +
                        "    FOREIGN KEY(ItemId)" +
                        "    REFERENCES Item(Id)" +
                        "    ON DELETE NO ACTION" +
                        "    ON UPDATE NO ACTION" +
                        "); ", SQLiteManagerTableNames.OrderTodayItem);
                    CreateTable(SQLiteManagerTableNames.OrderTodayItem, sql);
                }
                if (!TableExists(SQLiteManagerTableNames.Otpis, _sqliteConnection))
                {
                    sql = string.Format("CREATE TABLE '{0}' (" +
                        "'Id'  TEXT NOT NULL, " +
                        "'CashierId'  TEXT NOT NULL, " +
                        "'OtpisDate'  TEXT NOT NULL, " +
                        "'Counter'  INTEGER NOT NULL, " +
                        "'Name'  TEXT, " +
                        "PRIMARY KEY(Id), " +
                        "CONSTRAINT `fk_Otpis_Cashier1`" +
                        "    FOREIGN KEY(CashierId)" +
                        "    REFERENCES Cashier(Id)" +
                        "    ON DELETE NO ACTION" +
                        "    ON UPDATE NO ACTION" +
                        "); ", SQLiteManagerTableNames.Otpis);
                    CreateTable(SQLiteManagerTableNames.Otpis, sql);
                }
                if (!TableExists(SQLiteManagerTableNames.OtpisItem, _sqliteConnection))
                {
                    sql = string.Format("CREATE TABLE '{0}' (" +
                        "'OtpisId'  TEXT NOT NULL, " +
                        "'ItemId'  TEXT NOT NULL, " +
                        "'Quantity'  NUMERIC NOT NULL, " +
                        "'TotalPrice'  NUMERIC NOT NULL, " +
                        "'Description'  TEXT , " +
                        "PRIMARY KEY(OtpisId, ItemId), " +
                        "CONSTRAINT `fk_OtpisItem_Otpis1`" +
                        "    FOREIGN KEY(OtpisId)" +
                        "    REFERENCES Otpis(Id)" +
                        "    ON DELETE NO ACTION" +
                        "    ON UPDATE NO ACTION, " +
                        "CONSTRAINT `fk_OtpisItem_Item1`" +
                        "    FOREIGN KEY(ItemId)" +
                        "    REFERENCES Item(Id)" +
                        "    ON DELETE NO ACTION" +
                        "    ON UPDATE NO ACTION" +
                        "); ", SQLiteManagerTableNames.OtpisItem);
                    CreateTable(SQLiteManagerTableNames.OtpisItem, sql);
                }
                if (!TableExists(SQLiteManagerTableNames.ItemZelja, _sqliteConnection))
                {
                    sql = string.Format("CREATE TABLE '{0}' (" +
                        "'Id'  TEXT NOT NULL, " +
                        "'ItemId'  TEXT NOT NULL, " +
                        "'Zelja'  TEXT NOT NULL, " +
                        "PRIMARY KEY(Id, ItemId), " +
                        "CONSTRAINT `fk_ItemZelja_Item1`" +
                        "    FOREIGN KEY(ItemId)" +
                        "    REFERENCES Item(Id)" +
                        "    ON DELETE NO ACTION" +
                        "    ON UPDATE NO ACTION" +
                        "); ", SQLiteManagerTableNames.ItemZelja);
                    CreateTable(SQLiteManagerTableNames.ItemZelja, sql);
                }
            }
            catch (Exception ex)
            {
                int aa = 2;
            }
        }
        /// <summary>
        /// Checks if table exists in SQLite database
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        private bool TableExists(string tableName, SqliteConnection connection)
        {
            try
            {
                using (SqliteCommand cmd = new SqliteCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = connection;
                    cmd.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table' AND name = @name";
                    cmd.Parameters.AddWithValue("@name", tableName);

                    using (SqliteDataReader sqlDataReader = cmd.ExecuteReader())
                    {
                        return sqlDataReader.Read();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("SqliteDbContext - TableExists - Error occurred while checking if the table {0} exists.", tableName), ex);
                return false;
            }
        }
        /// <summary>
        /// Create table in SQLite database
        /// </summary>
        /// <param name="tableName">The name of the table</param>
        /// <param name="sql">SQL query that creates the table</param>
        /// <returns></returns>
        private bool CreateTable(string tableName, string sql)
        {
            try
            {
                using (SqliteCommand cmd = new SqliteCommand(sql, _sqliteConnection))
                {
                    cmd.ExecuteNonQuery();
                }
                return true;
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("duplicate column name") &&
                    !ex.Message.Contains("no such column"))
                {
                    Log.Error(string.Format("SqliteDbContext - CreateTable - Error occurred while creating table {0}.", tableName), ex);
                }
                return false;
            }
        }
        #endregion Private methods
    }
}
