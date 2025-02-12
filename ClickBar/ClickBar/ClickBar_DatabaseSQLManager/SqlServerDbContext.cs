using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection.Emit;
using ClickBar_Common.Enums;
using ClickBar_DatabaseSQLManager.Models;
using ClickBar_Logging;
using ClickBar_Settings;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ClickBar_DatabaseSQLManager
{
    public partial class SqlServerDbContext : DbContext
    {
        #region Fields
        private string _connectionString;
        private readonly string _masterConnectionString;
        private readonly string _databaseName;
        private SqlConnection _sqlConnection;

        private static bool _databaseInitialized = false; // Dodavanje statičke promenljive
        #endregion Fields
        //public SqlServerDbContext()
        //{
        //    _connectionString = SettingsManager.Instance.GetConnectionString();
        //    if (!string.IsNullOrEmpty(_connectionString))
        //    {
        //        Connection();
        //    }
        //}
        //public SqlServerDbContext(string connectionString, 
        //    string masterConnectionString,
        //    string databaseName)
        //{
        //    _connectionString = connectionString;
        //    _masterConnectionString = masterConnectionString;
        //    _databaseName = databaseName;

        //    EnsureDatabaseExists();
        //}

        public SqlServerDbContext(DbContextOptions<SqlServerDbContext> options)
            : base(options)
        {
            _connectionString = SettingsManager.Instance.GetConnectionString();
            _masterConnectionString = SettingsManager.Instance.GetConnectionStringMaster();
            _databaseName = SettingsManager.Instance.GetSqlServerDatabaseName();

            InitializeDatabase();
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
                optionsBuilder.UseSqlServer(_connectionString);
                base.OnConfiguring(optionsBuilder);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CashierDB>(entity =>
            {
                entity.ToTable("Cashier");

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
                entity.ToTable("Invoice");

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

                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");
            });

            modelBuilder.Entity<ItemDB>(entity =>
            {
                entity.ToTable("Item");

                entity.HasIndex(e => e.IdItemGroup, "fk_Item_ItemGroup_idx");

                entity.HasIndex(e => e.IdNorm, "fk_Item_Norm_idx");

                entity.Property(e => e.Id).HasMaxLength(15);

                entity.Property(e => e.AlarmQuantity).HasColumnType("decimal(18, 3)");

                entity.Property(e => e.Jm)
                    .HasMaxLength(5)
                    .HasColumnName("JM");

                entity.Property(e => e.Label).HasMaxLength(50);

                entity.Property(e => e.Name).HasMaxLength(500);

                entity.Property(e => e.TotalQuantity).HasColumnType("decimal(18, 3)");
                entity.Property(e => e.SellingUnitPrice).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.InputUnitPrice).HasColumnType("decimal(18, 2)");

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
                entity.ToTable("Supergroup");

                entity.Property(e => e.Name).HasMaxLength(45);
            });

            modelBuilder.Entity<ItemGroupDB>(entity =>
            {
                entity.ToTable("ItemGroup");

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

                entity.ToTable("ItemInvoice");

                entity.HasIndex(e => e.InvoiceId, "fk_ItemInvoice_Invoice1_idx");

                entity.Property(e => e.InvoiceId).HasMaxLength(36);

                entity.Property(e => e.ItemCode).HasMaxLength(15);

                entity.Property(e => e.Label).HasMaxLength(50);

                entity.Property(e => e.Name).HasMaxLength(500);

                entity.Property(e => e.Quantity).HasColumnType("decimal(18, 3)");

                entity.Property(e => e.TotalAmout).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.OriginalUnitPrice).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.InputUnitPrice).HasColumnType("decimal(18, 2)");

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

                entity.ToTable("Orders");

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
                entity.ToTable("PartHall");

                entity.Property(e => e.Image).HasMaxLength(200);

                entity.Property(e => e.Name).HasMaxLength(45);
            });

            modelBuilder.Entity<PaymentInvoiceDB>(entity =>
            {
                entity.HasKey(e => new { e.PaymentType, e.InvoiceId })
                    .HasName("PRIMARY");

                entity.ToTable("PaymentInvoice");

                entity.HasIndex(e => e.InvoiceId, "fk_PaymentInvoice_Invoice1_idx");

                entity.Property(e => e.InvoiceId).HasMaxLength(36);

                entity.Property(e => e.Amout).HasColumnType("decimal(18, 2)");

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

                entity.ToTable("PaymentPlace");

                entity.HasIndex(e => e.PartHallId, "fk_Table_PartHall1_idx");

                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Height).HasColumnType("float");

                entity.Property(e => e.Width).HasColumnType("float");

                entity.Property(e => e.LeftCanvas).HasColumnType("float");

                entity.Property(e => e.TopCanvas).HasColumnType("float");

                entity.Property(e => e.HeightMobi).HasColumnType("float");

                entity.Property(e => e.WidthMobi).HasColumnType("float");

                entity.Property(e => e.X_Mobi).HasColumnType("float");

                entity.Property(e => e.Y_Mobi).HasColumnType("float");

                entity.Property(e => e.Popust).HasColumnType("decimal(18, 2)");

                entity.HasOne(d => d.PartHall)
                    .WithMany(p => p.Paymentplaces)
                    .HasForeignKey(d => d.PartHallId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_Table_PartHall1");
            });

            modelBuilder.Entity<ProcurementDB>(entity =>
            {
                entity.ToTable("Procurement");

                entity.HasIndex(e => e.ItemId, "fk_Procurement_Item1_idx");

                entity.HasIndex(e => e.SupplierId, "fk_Procurement_Supplier1_idx");

                entity.Property(e => e.Id).HasMaxLength(36);

                entity.Property(e => e.DateProcurement).HasColumnType("datetime");

                entity.Property(e => e.ItemId).HasMaxLength(15);

                entity.Property(e => e.Quantity).HasColumnType("decimal(18, 3)");

                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");

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
                entity.ToTable("SmartCard");

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
                entity.ToTable("Supplier");

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

                entity.ToTable("TaxItemInvoice");

                entity.HasIndex(e => e.InvoiceId, "fk_TaxItemInvoice_Invoice1_idx");

                entity.Property(e => e.Label).HasMaxLength(50);

                entity.Property(e => e.InvoiceId)
                    .HasMaxLength(36)
                    .HasColumnName("InvoiceId");

                entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.CategoryName).HasMaxLength(45);

                entity.Property(e => e.Rate).HasColumnType("decimal(18, 2)");

                entity.HasOne(d => d.Invoice)
                    .WithMany(p => p.TaxItemInvoices)
                    .HasForeignKey(d => d.InvoiceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_TaxItemInvoice_Invoice1");
            });

            modelBuilder.Entity<NormDB>(entity =>
            {
                entity.ToTable("Norm");
            });

            modelBuilder.Entity<ItemInNormDB>(entity =>
            {
                entity.HasKey(e => new { e.IdItem, e.IdNorm })
                    .HasName("PRIMARY");

                entity.ToTable("ItemInNorm");

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

                entity.ToTable("UnprocessedOrder");

                entity.HasIndex(e => e.PaymentPlaceId, "fk_UnprocessedOrder_PaymentPlace1_idx");

                entity.HasIndex(e => e.CashierId, "fk_UnprocessedOrder_Cashier1_idx");

                entity.Property(e => e.CashierId).HasMaxLength(36);

                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");

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

                entity.ToTable("ItemInUnprocessedOrder");

                entity.HasIndex(e => e.ItemId, "fk_ItemInUnprocessedOrder_Item1_idx");

                entity.HasIndex(e => e.UnprocessedOrderId, "fk_ItemInUnprocessedOrder_UnprocessedOrder1_idx");

                entity.Property(e => e.ItemId).HasMaxLength(36);

                entity.Property(e => e.UnprocessedOrderId).HasMaxLength(36);

                entity.Property(e => e.Quantity).HasColumnType("decimal(18, 3)");

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

                entity.ToTable("Calculation");

                entity.HasIndex(e => e.SupplierId, "fk_Calculation_Supplier1_idx");

                entity.HasIndex(e => e.CashierId, "fk_Calculation_Cashier1_idx");

                entity.Property(e => e.Id).HasMaxLength(36);

                entity.Property(e => e.CashierId).HasMaxLength(36);

                entity.Property(e => e.CalculationDate).HasColumnType("datetime");

                entity.Property(e => e.InvoiceNumber).HasMaxLength(50);

                entity.Property(e => e.InputTotalPrice).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.OutputTotalPrice).HasColumnType("decimal(18, 2)");

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

                entity.ToTable("CalculationItem");

                entity.HasIndex(e => e.CalculationId, "fk_CalculationItem_Calculation1_idx");

                entity.HasIndex(e => e.ItemId, "fk_CalculationItem_Item1_idx");

                entity.Property(e => e.CalculationId).HasMaxLength(36);

                entity.Property(e => e.ItemId).HasMaxLength(36);

                entity.Property(e => e.InputPrice).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.OutputPrice).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Quantity).HasColumnType("decimal(18, 3)");

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
                entity.ToTable("Nivelacija");
            });

            modelBuilder.Entity<ItemNivelacijaDB>(entity =>
            {
                entity.HasKey(e => new { e.IdItem, e.IdNivelacija })
                    .HasName("PRIMARY");

                entity.ToTable("ItemNivelacija");

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
                entity.ToTable("KnjizenjePazara");
            });

            modelBuilder.Entity<KepDB>(entity =>
            {
                entity.ToTable("KEP");
            });

            modelBuilder.Entity<FirmaDB>(entity =>
            {
                entity.ToTable("Firma");
            });

            modelBuilder.Entity<PartnerDB>(entity =>
            {
                entity.ToTable("Partner");

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
                entity.ToTable("PocetnoStanje");

                entity.Property(e => e.Id).HasMaxLength(36);

                entity.Property(e => e.PopisDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PocetnoStanjeItemDB>(entity =>
            {
                entity.HasKey(e => new { e.IdItem, e.IdPocetnoStanje })
                    .HasName("PRIMARY");

                entity.ToTable("PocetnoStanjeItem");

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

                entity.ToTable("OrderToday");

                entity.HasIndex(e => e.CashierId, "fk_OrderToday_Cashier1_idx");

                entity.Property(e => e.Id).HasMaxLength(36);

                entity.Property(e => e.OrderDateTime).HasColumnType("datetime");

                entity.Property(e => e.Counter);

                entity.Property(e => e.CounterType);

                entity.Property(e => e.TableId);

                entity.Property(e => e.TotalPrice).HasColumnType("decimal(18, 2)");

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

                entity.ToTable("OrderTodayItem");

                entity.HasIndex(e => e.OrderTodayId, "fk_OrderTodayItem_OrderToday1_idx");

                entity.HasIndex(e => e.ItemId, "fk_OrderTodayItem_Item1_idx");

                entity.Property(e => e.Quantity).HasColumnType("decimal(18, 3)");

                entity.Property(e => e.TotalPrice).HasColumnType("decimal(18, 2)");

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

                entity.ToTable("Otpis");

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

                entity.ToTable("OtpisItem");

                entity.HasIndex(e => e.OtpisId, "fk_OtpisItem_Otpis1_idx");

                entity.HasIndex(e => e.ItemId, "fk_OtpisItem_Item1_idx");

                entity.Property(e => e.Quantity).HasColumnType("decimal(18, 3)");

                entity.Property(e => e.TotalPrice).HasColumnType("decimal(18, 2)");

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

                entity.ToTable("ItemZelja");

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

        #endregion Protected methods

        #region Private methods
        private void Connection()
        {
            try
            {
                using (SqlConnection dbConnection = new SqlConnection(_connectionString))
                {
                    dbConnection.Open();
                    _sqlConnection = dbConnection;
                }
            }
            catch (Exception ex)
            {
                Log.Error("SqlServerDbContext - Connection - Error occurred while opening connection.", ex);
            }
        }
        private void InitializeDatabase()
        {
            if (_databaseInitialized)
            {
                Log.Debug("Database already initialized.");
                return;
            }

            Log.Debug("SqlServerDbContext - InitializeDatabase - Initializing database...");
            EnsureDatabaseExists();
            _databaseInitialized = true; // Postavi na true nakon inicijalizacije
        }

        private void EnsureDatabaseExists()
        {
            using (SqlConnection connection = new SqlConnection(_masterConnectionString))
            {
                try
                {
                    connection.Open();
                    Log.Debug("Connected to master database.");
                    string checkDatabaseQuery = $"IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'{_databaseName}') CREATE DATABASE [{_databaseName}]";
                    using (SqlCommand command = new SqlCommand(checkDatabaseQuery, connection))
                    {
                        command.ExecuteNonQuery();
                        Log.Debug("Database check/creation executed.");
                    }

                    _connectionString = _connectionString.Replace("Initial Catalog=master;", $"Initial Catalog={_databaseName};");
                    using (SqlConnection dbConnection = new SqlConnection(_connectionString))
                    {
                        dbConnection.Open();
                        _sqlConnection = dbConnection;
                        Log.Debug("Connected to application database.");

                        // Proveri i kreiraj tabele i kolone samo jednom
                        CreateTables();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to check/create database: {ex.Message}", ex);
                }
            }
        }

        /// <summary>
        /// Creates tables in SQL Server database
        /// <para>
        /// Checks if each tables exist in the database. If not, creates them one by one.
        /// </para>
        /// </summary>
        private void CreateTables()
        {
            try
            {
                string sql = string.Empty;
                if (!TableExists("Supergroup", _sqlConnection))
                {
                    sql = string.Format("CREATE TABLE {0} ( " +
                        "Id INT NOT NULL IDENTITY(1,1), " +
                        "Name NVARCHAR(45) NOT NULL, " +
                        "PRIMARY KEY(Id) " +
                        "); ", "Supergroup");
                    CreateTable("Supergroup", sql);
                }
                if (!TableExists("ItemGroup", _sqlConnection))
                {
                    sql = string.Format("CREATE TABLE {0} ( " +
                        "Id INT NOT NULL IDENTITY(1,1), " +
                        "IdSupergroup INT NOT NULL, " +
                        "Name NVARCHAR(45) NOT NULL, " +
                        "PRIMARY KEY(Id), " +
                        "FOREIGN KEY(IdSupergroup) REFERENCES Supergroup(Id) " +
                        "ON DELETE NO ACTION " +
                        "ON UPDATE NO ACTION" +
                        "); ", "ItemGroup");
                    CreateTable("ItemGroup", sql);
                }
                if (!TableExists("Norm", _sqlConnection))
                {
                    sql = string.Format("CREATE TABLE {0} (" +
                        "Id INT NOT NULL IDENTITY(1,1), " +
                        "PRIMARY KEY(Id)" +
                        "); ", "Norm");
                    CreateTable("Norm", sql);
                }
                if (!TableExists("Item", _sqlConnection))
                {
                    sql = string.Format("CREATE TABLE {0} ( " +
                        "Id NVARCHAR(36) NOT NULL, " + // Promena NVARCHAR(15) u NVARCHAR(36) da bi se slagalo sa IdItem u drugim tabelama
                        "IdItemGroup INT NOT NULL, " +
                        "Name NVARCHAR(500) NOT NULL, " +
                        "SellingUnitPrice DECIMAL(18,2) NOT NULL, " +
                        "SellingNocnaUnitPrice DECIMAL(18,2) NOT NULL DEFAULT 0, " +
                        "SellingDnevnaUnitPrice DECIMAL(18,2) NOT NULL DEFAULT 0, " +
                        "InputUnitPrice DECIMAL(18,2), " +
                        "Label NVARCHAR(50) NOT NULL, " +
                        "JM NVARCHAR(5) NOT NULL, " +
                        "TotalQuantity DECIMAL(18,3) NOT NULL, " +
                        "AlarmQuantity DECIMAL(18,3) NOT NULL, " +
                        "IdNorm INT, " +
                        "DisableItem INT NOT NULL DEFAULT 0, " +
                        "IsCheckedZabraniPopust INT NOT NULL DEFAULT 0, " +
                        "PRIMARY KEY(Id), " +
                        "FOREIGN KEY(IdNorm) REFERENCES Norm(Id), " +
                        "FOREIGN KEY(IdItemGroup) REFERENCES ItemGroup(Id) " +
                        "ON DELETE NO ACTION " +
                        "ON UPDATE NO ACTION" +
                        "); ", "Item");
                    CreateTable("Item", sql);
                }
                else
                {
                    try
                    {
                        // Proveri i dodaj kolone ako ne postoje
                        AddColumnIfNotExists("Item", "IsCheckedZabraniPopust INT NOT NULL DEFAULT 0", _sqlConnection);
                        AddColumnIfNotExists("Item", "DisableItem INT NOT NULL DEFAULT 0", _sqlConnection);
                        AddColumnIfNotExists("Item", "SellingDnevnaUnitPrice DECIMAL(18,2) NOT NULL DEFAULT 0", _sqlConnection);
                        AddColumnIfNotExists("Item", "SellingNocnaUnitPrice DECIMAL(18,2) NOT NULL DEFAULT 0", _sqlConnection);
                        AddColumnIfNotExists("Item", "SellingUnitPrice DECIMAL(18,2)", _sqlConnection);
                        AddColumnIfNotExists("Item", "InputUnitPrice DECIMAL(18,2)", _sqlConnection);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("SqlServerDbContext - CreateTables - Error occurred while altering table Item.", ex);
                    }
                }
                if (!TableExists("Cashier", _sqlConnection))
                {
                    sql = string.Format("CREATE TABLE {0} (" +
                        "Id NVARCHAR(4) NOT NULL," +
                        "Type INT NOT NULL," +
                        "Name NVARCHAR(45) NOT NULL," +
                        "SmartCardNumber NVARCHAR(15)," +
                        "Jmbg NVARCHAR(13)," +
                        "City NVARCHAR(45)," +
                        "Address NVARCHAR(65)," +
                        "ContactNumber NVARCHAR(20)," +
                        "Email NVARCHAR(60)," +
                        "PRIMARY KEY(Id)" +
                        "); ", "Cashier");
                    CreateTable("Cashier", sql);
                }
                else
                {
                    try
                    {
                        // Proveri i dodaj kolone ako ne postoje
                        AddColumnIfNotExists("Cashier", "SmartCardNumber NVARCHAR(15)", _sqlConnection);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("SqlServerDbContext - CreateTables - Error occurred while altering table Cashier.", ex);
                    }
                }
                if (!TableExists("Invoice", _sqlConnection))
                {
                    sql = string.Format("CREATE TABLE {0} (" +
                        "Id NVARCHAR(36) NOT NULL," +
                        "DateAndTimeOfIssue DATETIME," +
                        "Cashier NVARCHAR(45)," +
                        "BuyerId NVARCHAR(45)," +
                        "BuyerName NVARCHAR(75)," +
                        "BuyerAddress NVARCHAR(45)," +
                        "BuyerCostCenterId NVARCHAR(45)," +
                        "InvoiceType INT," +
                        "TransactionType INT," +
                        "ReferentDocumentNumber NVARCHAR(50)," +
                        "ReferentDocumentDT DATETIME," +
                        "InvoiceNumber NVARCHAR(50)," +
                        "RequestedBy NVARCHAR(10)," +
                        "InvoiceNumberResult NVARCHAR(50)," +
                        "SdcDateTime DATETIME," +
                        "InvoiceCounter NVARCHAR(50)," +
                        "InvoiceCounterExtension NVARCHAR(50)," +
                        "SignedBy NVARCHAR(10)," +
                        "EncryptedInternalData NVARCHAR(512)," +
                        "Signature NVARCHAR(512)," +
                        "TotalCounter INT," +
                        "TransactionTypeCounter INT," +
                        "TotalAmount DECIMAL(18,2)," +
                        "TaxGroupRevision INT," +
                        "BusinessName NVARCHAR(75)," +
                        "Tin NVARCHAR(35)," +
                        "LocationName NVARCHAR(95)," +
                        "Address NVARCHAR(95)," +
                        "District NVARCHAR(95)," +
                        "Mrc NVARCHAR(55)," +
                        "KnjizenjePazaraId NVARCHAR(36)," +
                        "SendEfaktura INT DEFAULT 0," +
                        "PRIMARY KEY(Id)" +
                        "); ", "Invoice");
                    CreateTable("Invoice", sql);
                }
                else
                {
                    try
                    {
                        // Proveri i dodaj kolone ako ne postoje
                        AddColumnIfNotExists("Invoice", "SendEfaktura INT DEFAULT 0", _sqlConnection);
                        AddColumnIfNotExists("Invoice", "KnjizenjePazaraId NVARCHAR(36)", _sqlConnection);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("SqlServerDbContext - CreateTables - Error occurred while altering table Invoice.", ex);
                    }
                }
                if (!TableExists("ItemInvoice", _sqlConnection))
                {
                    sql = string.Format("CREATE TABLE {0} (" +
                        "Id INT NOT NULL, " +
                        "IsSirovina INT, " +
                        "InvoiceId NVARCHAR(36) NOT NULL," +
                        "Quantity DECIMAL(18,3)," +
                        "TotalAmout DECIMAL(18,2)," +
                        "Name NVARCHAR(500)," +
                        "UnitPrice DECIMAL(18,2)," +
                        "OriginalUnitPrice DECIMAL(18,2)," +
                        "InputUnitPrice DECIMAL(18,2)," +
                        "Label NVARCHAR(50)," +
                        "ItemCode NVARCHAR(15)," +
                        "PRIMARY KEY(Id, InvoiceId), " +
                        "FOREIGN KEY(InvoiceId) REFERENCES Invoice(Id) " +
                        "ON DELETE NO ACTION " +
                        "ON UPDATE NO ACTION" +
                        "); ", "ItemInvoice");
                    CreateTable("ItemInvoice", sql);
                }
                else
                {
                    try
                    {
                        AddColumnIfNotExists("ItemInvoice", "OriginalUnitPrice DECIMAL(18,2)", _sqlConnection);
                        AddColumnIfNotExists("ItemInvoice", "IsSirovina INT", _sqlConnection);
                        AddColumnIfNotExists("ItemInvoice", "InputUnitPrice DECIMAL(18,2)", _sqlConnection);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("SqlServerDbContext - CreateTables - Error occurred while altering table ItemInvoice.", ex);
                    }
                }
                if (!TableExists("PaymentInvoice", _sqlConnection))
                {
                    sql = string.Format("CREATE TABLE {0} (" +
                        "InvoiceId NVARCHAR(36) NOT NULL," +
                        "PaymentType INT NOT NULL," +
                        "Amout DECIMAL(18,2)," +
                        "PRIMARY KEY(InvoiceId, PaymentType), " +
                        "FOREIGN KEY(InvoiceId) REFERENCES Invoice(Id) " +
                        "ON DELETE NO ACTION " +
                        "ON UPDATE NO ACTION" +
                        "); ", "PaymentInvoice");
                    CreateTable("PaymentInvoice", sql);
                }
                if (!TableExists("TaxItemInvoice", _sqlConnection))
                {
                    sql = string.Format("CREATE TABLE {0} (" +
                        "InvoiceId NVARCHAR(36) NOT NULL," +
                        "Label NVARCHAR(50) NOT NULL," +
                        "CategoryName NVARCHAR(45)," +
                        "CategoryType INT," +
                        "Rate DECIMAL(18,2)," +
                        "Amount DECIMAL(18,2)," +
                        "PRIMARY KEY(InvoiceId, Label), " +
                        "FOREIGN KEY(InvoiceId) REFERENCES Invoice(Id) " +
                        "ON DELETE NO ACTION " +
                        "ON UPDATE NO ACTION" +
                        "); ", "TaxItemInvoice");
                    CreateTable("TaxItemInvoice", sql);
                }
                if (!TableExists("SmartCard", _sqlConnection))
                {
                    sql = string.Format("CREATE TABLE {0} (" +
                        "Id NVARCHAR(15) NOT NULL," +
                        "CashierId NVARCHAR(4) NOT NULL," +
                        "PRIMARY KEY(Id), " +
                        "FOREIGN KEY(CashierId) REFERENCES Cashier(Id) " +
                        "ON DELETE NO ACTION " +
                        "ON UPDATE NO ACTION" +
                        "); ", "SmartCard");
                    CreateTable("SmartCard", sql);
                }
                if (!TableExists("Supplier", _sqlConnection))
                {
                    sql = string.Format("CREATE TABLE {0} ( " +
                        "Id INT NOT NULL IDENTITY(1,1), " +
                        "PIB NVARCHAR(45) NOT NULL, " +
                        "Name NVARCHAR(95) NOT NULL, " +
                        "Address NVARCHAR(75), " +
                        "ContractNumber NVARCHAR(20), " +
                        "Email NVARCHAR(60), " +
                        "City NVARCHAR(45), " +
                        "MB NVARCHAR(45), " +
                        "PRIMARY KEY(Id) " +
                        "); ", "Supplier");
                    CreateTable("Supplier", sql);
                }
                if (!TableExists("Procurement", _sqlConnection))
                {
                    sql = string.Format("CREATE TABLE {0} (" +
                        "Id NVARCHAR(36) NOT NULL," +
                        "SupplierId INT NOT NULL," +
                        "ItemId NVARCHAR(36) NOT NULL," + // Promena NVARCHAR(15) u NVARCHAR(36)
                        "DateProcurement DATETIME NOT NULL," +
                        "Quantity DECIMAL(18,3) NOT NULL," +
                        "UnitPrice DECIMAL(18,2) NOT NULL," +
                        "PRIMARY KEY(Id), " +
                        "FOREIGN KEY(SupplierId) REFERENCES Supplier(Id) " +
                        "ON DELETE NO ACTION " +
                        "ON UPDATE NO ACTION, " +
                        "FOREIGN KEY(ItemId) REFERENCES Item(Id) " +
                        "ON DELETE NO ACTION " +
                        "ON UPDATE NO ACTION" +
                        "); ", "Procurement");
                    CreateTable("Procurement", sql);
                }
                if (!TableExists("PartHall", _sqlConnection))
                {
                    sql = string.Format("CREATE TABLE {0} ( " +
                        "Id INT NOT NULL IDENTITY(1,1), " +
                        "Name NVARCHAR(45) NOT NULL, " +
                        "Image NVARCHAR(200), " + // Promena NVARCHAR(4096) u NVARCHAR(4000)
                        "PRIMARY KEY(Id) " +
                        "); ", "PartHall");
                    CreateTable("PartHall", sql);
                }
                if (!TableExists("PaymentPlace", _sqlConnection))
                {
                    sql = string.Format("CREATE TABLE {0} (" +
                        "Id INT NOT NULL IDENTITY(1,1)," +
                        "PartHallId INT NOT NULL," +
                        "LeftCanvas float," +
                        "TopCanvas float," +
                        "Type INT," +
                        "Width float," +
                        "Height float," +
                        "X_Mobi float," +
                        "Y_Mobi float," +
                        "WidthMobi float," +
                        "HeightMobi float," +
                        "Popust DECIMAL(18,2) NOT NULL DEFAULT 0," +
                        "Name NVARCHAR(45)," +
                        "PRIMARY KEY(Id), " +
                        "FOREIGN KEY(PartHallId) REFERENCES PartHall(Id) " +
                        "ON DELETE NO ACTION " +
                        "ON UPDATE NO ACTION" +
                        "); ", "PaymentPlace");
                    CreateTable("PaymentPlace", sql);
                }
                else
                {
                    try
                    {
                        AddColumnIfNotExists("PaymentPlace", "Name NVARCHAR(45)", _sqlConnection);
                        AddColumnIfNotExists("PaymentPlace", "Popust DECIMAL(18,2) NOT NULL DEFAULT 0", _sqlConnection);
                        AddColumnIfNotExists("PaymentPlace", "X_Mobi float", _sqlConnection);
                        AddColumnIfNotExists("PaymentPlace", "Y_Mobi float", _sqlConnection);
                        AddColumnIfNotExists("PaymentPlace", "WidthMobi float", _sqlConnection);
                        AddColumnIfNotExists("PaymentPlace", "HeightMobi float", _sqlConnection);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("SqlServerDbContext - CreateTables - Error occurred while altering table PaymentPlace.", ex);
                    }
                }
                if (!TableExists("Orders", _sqlConnection))
                {
                    sql = string.Format("CREATE TABLE {0} (" +
                        "PaymentPlaceId INT NOT NULL, " +
                        "InvoiceId NVARCHAR(36) NOT NULL, " +
                        "CashierId NVARCHAR(4) NOT NULL, " +
                        "PRIMARY KEY(PaymentPlaceId, InvoiceId, CashierId), " +
                        "FOREIGN KEY(PaymentPlaceId) REFERENCES PaymentPlace(Id) " +
                        "ON DELETE NO ACTION " +
                        "ON UPDATE NO ACTION, " +
                        "FOREIGN KEY(InvoiceId) REFERENCES Invoice(Id) " +
                        "ON DELETE NO ACTION " +
                        "ON UPDATE NO ACTION," +
                        "FOREIGN KEY(CashierId) REFERENCES Cashier(Id) " +
                        "ON DELETE NO ACTION " +
                        "ON UPDATE NO ACTION" +
                        "); ", "Orders");
                    CreateTable("Orders", sql);
                }
                if (!TableExists("ItemInNorm", _sqlConnection))
                {
                    sql = string.Format("CREATE TABLE {0} (" +
                        "IdNorm INT NOT NULL, " +
                        "IdItem NVARCHAR(36) NOT NULL, " +
                        "Quantity DECIMAL(18,3) NOT NULL, " +
                        "PRIMARY KEY(IdNorm, IdItem), " +
                        "FOREIGN KEY(IdNorm) REFERENCES Norm(Id) " +
                        "ON DELETE NO ACTION " +
                        "ON UPDATE NO ACTION, " +
                        "FOREIGN KEY(IdItem) REFERENCES Item(Id) " +
                        "ON DELETE NO ACTION " +
                        "ON UPDATE NO ACTION" +
                        "); ", "ItemInNorm");
                    CreateTable("ItemInNorm", sql);
                }
                if (!TableExists("UnprocessedOrder", _sqlConnection))
                {
                    sql = string.Format("CREATE TABLE {0} (" +
                        "Id NVARCHAR(36) NOT NULL, " +
                        "PaymentPlaceId INT NOT NULL, " +
                        "CashierId NVARCHAR(4) NOT NULL, " + // Promena NVARCHAR(36) u NVARCHAR(4)
                        "TotalAmount DECIMAL(18,2), " +
                        "PRIMARY KEY(Id), " +
                        "FOREIGN KEY(CashierId) REFERENCES Cashier(Id) " +
                        "ON DELETE NO ACTION " +
                        "ON UPDATE NO ACTION, " +
                        "FOREIGN KEY(PaymentPlaceId) REFERENCES PaymentPlace(Id) " +
                        "ON DELETE NO ACTION " +
                        "ON UPDATE NO ACTION" +
                        "); ", "UnprocessedOrder");
                    CreateTable("UnprocessedOrder", sql);
                }
                if (!TableExists("ItemInUnprocessedOrder", _sqlConnection))
                {
                    sql = string.Format("CREATE TABLE {0} (" +
                        "ItemId NVARCHAR(36) NOT NULL, " +
                        "UnprocessedOrderId NVARCHAR(36) NOT NULL, " +
                        "Quantity DECIMAL(18,3) NOT NULL, " +
                        "PRIMARY KEY(ItemId, UnprocessedOrderId), " +
                        "FOREIGN KEY(UnprocessedOrderId) REFERENCES UnprocessedOrder(Id) " +
                        "ON DELETE NO ACTION " +
                        "ON UPDATE NO ACTION, " +
                        "FOREIGN KEY(ItemId) REFERENCES Item(Id) " +
                        "ON DELETE NO ACTION " +
                        "ON UPDATE NO ACTION" +
                        "); ", "ItemInUnprocessedOrder");
                    CreateTable("ItemInUnprocessedOrder", sql);
                }
                if (!TableExists("Calculation", _sqlConnection))
                {
                    sql = string.Format("CREATE TABLE {0} (" +
                        "Id NVARCHAR(36) NOT NULL, " +
                        "SupplierId INT NOT NULL, " +
                        "CashierId NVARCHAR(4) NOT NULL, " + // Promena NVARCHAR(36) u NVARCHAR(4)
                        "Counter INT, " +
                        "CalculationDate DATETIME NOT NULL, " +
                        "InvoiceNumber NVARCHAR(50), " +
                        "InputTotalPrice DECIMAL(18,2) NOT NULL, " +
                        "OutputTotalPrice DECIMAL(18,2) NOT NULL, " +
                        "PRIMARY KEY(Id), " +
                        "FOREIGN KEY(CashierId) REFERENCES Cashier(Id) " +
                        "ON DELETE NO ACTION " +
                        "ON UPDATE NO ACTION, " +
                        "FOREIGN KEY(SupplierId) REFERENCES Supplier(Id) " +
                        "ON DELETE NO ACTION " +
                        "ON UPDATE NO ACTION" +
                        "); ", "Calculation");
                    CreateTable("Calculation", sql);
                }
                else
                {
                    try
                    {
                        AddColumnIfNotExists("Calculation", "Counter INT", _sqlConnection);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("SqlServerDbContext - CreateTables - Error occurred while altering table Calculation.", ex);
                    }
                }
                if (!TableExists("CalculationItem", _sqlConnection))
                {
                    sql = string.Format("CREATE TABLE {0} (" +
                        "CalculationId NVARCHAR(36) NOT NULL, " +
                        "ItemId NVARCHAR(36) NOT NULL, " +
                        "InputPrice DECIMAL(18,2) NOT NULL, " +
                        "OutputPrice DECIMAL(18,2) NOT NULL, " +
                        "Quantity DECIMAL(18,3) NOT NULL, " +
                        "PRIMARY KEY(CalculationId, ItemId), " +
                        "FOREIGN KEY(ItemId) REFERENCES Item(Id) " +
                        "ON DELETE NO ACTION " +
                        "ON UPDATE NO ACTION, " +
                        "FOREIGN KEY(CalculationId) REFERENCES Calculation(Id) " +
                        "ON DELETE NO ACTION " +
                        "ON UPDATE NO ACTION" +
                        "); ", "CalculationItem");
                    CreateTable("CalculationItem", sql);
                }
                if (!TableExists("Nivelacija", _sqlConnection))
                {
                    sql = string.Format("CREATE TABLE {0} (" +
                        "Id NVARCHAR(36) NOT NULL, " +
                        "Counter INT NOT NULL, " +
                        "Type INT NOT NULL, " +
                        "DateNivelacije DATETIME NOT NULL, " +
                        "Description NVARCHAR(MAX), " +
                        "PRIMARY KEY(Id)" +
                        "); ", "Nivelacija");
                    CreateTable("Nivelacija", sql);
                }
                if (!TableExists("ItemNivelacija", _sqlConnection))
                {
                    sql = string.Format("CREATE TABLE {0} (" +
                        "IdNivelacija NVARCHAR(36) NOT NULL, " +
                        "IdItem NVARCHAR(36) NOT NULL, " +
                        "OldUnitPrice DECIMAL(18,2) NOT NULL, " +
                        "NewUnitPrice DECIMAL(18,2) NOT NULL, " +
                        "TotalQuantity DECIMAL(18,3) NOT NULL, " +
                        "StopaPDV DECIMAL(18,2) NOT NULL, " +
                        "PRIMARY KEY(IdNivelacija, IdItem), " +
                        "FOREIGN KEY(IdNivelacija) REFERENCES Nivelacija(Id) " +
                        "ON DELETE NO ACTION " +
                        "ON UPDATE NO ACTION, " +
                        "FOREIGN KEY(IdItem) REFERENCES Item(Id) " +
                        "ON DELETE NO ACTION " +
                        "ON UPDATE NO ACTION" +
                        "); ", "ItemNivelacija");
                    CreateTable("ItemNivelacija", sql);
                }
                if (!TableExists("KnjizenjePazara", _sqlConnection))
                {
                    sql = string.Format("CREATE TABLE {0} (" +
                        "Id NVARCHAR(36) NOT NULL, " +
                        "Description NVARCHAR(MAX) NOT NULL, " +
                        "IssueDateTime DATETIME NOT NULL, " +
                        "NormalSaleCash DECIMAL(18,2) NOT NULL, " +
                        "NormalSaleCard DECIMAL(18,2) NOT NULL, " +
                        "NormalSaleWireTransfer DECIMAL(18,2) NOT NULL, " +
                        "NormalRefundCash DECIMAL(18,2) NOT NULL, " +
                        "NormalRefundCard DECIMAL(18,2) NOT NULL, " +
                        "NormalRefundWireTransfer DECIMAL(18,2) NOT NULL, " +
                        "PRIMARY KEY(Id)" +
                        "); ", "KnjizenjePazara");
                    CreateTable("KnjizenjePazara", sql);
                }
                if (!TableExists("KEP", _sqlConnection))
                {
                    sql = string.Format("CREATE TABLE {0} (" +
                        "Id NVARCHAR(36) NOT NULL, " +
                        "Description NVARCHAR(MAX) NOT NULL, " +
                        "KepDate DATETIME NOT NULL, " +
                        "Type INT NOT NULL, " +
                        "Zaduzenje DECIMAL(18,2) NOT NULL, " +
                        "Razduzenje DECIMAL(18,2) NOT NULL, " +
                        "PRIMARY KEY(Id)" +
                        "); ", "KEP");
                    CreateTable("KEP", sql);
                }
                if (!TableExists("Firma", _sqlConnection))
                {
                    sql = string.Format("CREATE TABLE {0} (" +
                        "Id INT NOT NULL IDENTITY(1,1), " +
                        "Name NVARCHAR(MAX), " +
                        "Pib NVARCHAR(MAX), " +
                        "MB NVARCHAR(MAX), " +
                        "NamePP NVARCHAR(MAX), " +
                        "AddressPP NVARCHAR(MAX), " +
                        "Number NVARCHAR(MAX), " +
                        "Email NVARCHAR(MAX), " +
                        "BankAcc NVARCHAR(MAX), " +
                        "AuthenticationKey NVARCHAR(MAX), " +
                        "PRIMARY KEY(Id)" +
                        "); ", "Firma");
                    CreateTable("Firma", sql);
                }
                if (!TableExists("Partner", _sqlConnection))
                {
                    sql = string.Format("CREATE TABLE {0} ( " +
                        "Id INT NOT NULL IDENTITY(1,1), " +
                        "PIB NVARCHAR(45) NOT NULL, " +
                        "Name NVARCHAR(95) NOT NULL, " +
                        "Address NVARCHAR(75), " +
                        "ContractNumber NVARCHAR(20), " +
                        "Email NVARCHAR(60), " +
                        "City NVARCHAR(45), " +
                        "MB NVARCHAR(45), " +
                        "PRIMARY KEY(Id) " +
                        "); ", "Partner");
                    CreateTable("Partner", sql);
                }
                if (!TableExists("PocetnoStanje", _sqlConnection))
                {
                    sql = string.Format("CREATE TABLE {0} (" +
                        "Id NVARCHAR(36) NOT NULL, " +
                        "PopisDate DATETIME NOT NULL, " +
                        "Description NVARCHAR(MAX), " +
                        "PRIMARY KEY(Id)" +
                        "); ", "PocetnoStanje");
                    CreateTable("PocetnoStanje", sql);
                }
                if (!TableExists("PocetnoStanjeItem", _sqlConnection))
                {
                    sql = string.Format("CREATE TABLE {0} (" +
                        "IdPocetnoStanje NVARCHAR(36) NOT NULL, " +
                        "IdItem NVARCHAR(36) NOT NULL, " +
                        "OldQuantity DECIMAL(18,3) NOT NULL, " +
                        "NewQuantity DECIMAL(18,3) NOT NULL, " +
                        "InputPrice DECIMAL(18,2) NOT NULL, " +
                        "OutputPrice DECIMAL(18,2) NOT NULL, " +
                        "PRIMARY KEY(IdPocetnoStanje, IdItem), " +
                        "FOREIGN KEY(IdPocetnoStanje) REFERENCES PocetnoStanje(Id) " +
                        "ON DELETE NO ACTION " +
                        "ON UPDATE NO ACTION, " +
                        "FOREIGN KEY(IdItem) REFERENCES Item(Id) " +
                        "ON DELETE NO ACTION " +
                        "ON UPDATE NO ACTION" +
                        "); ", "PocetnoStanjeItem");
                    CreateTable("PocetnoStanjeItem", sql);
                }
                if (!TableExists("OrderToday", _sqlConnection))
                {
                    sql = string.Format("CREATE TABLE {0} (" +
                        "Id NVARCHAR(36) NOT NULL, " +
                        "CashierId NVARCHAR(4) NOT NULL, " + // Promena NVARCHAR(36) u NVARCHAR(4)
                        "OrderDateTime DATETIME NOT NULL, " +
                        "Counter INT NOT NULL, " +
                        "CounterType INT NOT NULL DEFAULT 0, " +
                        "Name NVARCHAR(MAX), " +
                        "TotalPrice DECIMAL(18,2) NOT NULL, " +
                        "TableId INT, " +
                        "PRIMARY KEY(Id), " +
                        "FOREIGN KEY(CashierId) REFERENCES Cashier(Id) " +
                        "ON DELETE NO ACTION " +
                        "ON UPDATE NO ACTION" +
                        "); ", "OrderToday");
                    CreateTable("OrderToday", sql);
                }
                else
                {
                    try
                    {
                        AddColumnIfNotExists("OrderToday", "CounterType INT NOT NULL DEFAULT 0", _sqlConnection);
                        AddColumnIfNotExists("OrderToday", "TableId INT", _sqlConnection);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("SqlServerDbContext - CreateTables - Error occurred while altering table OrderToday.", ex);
                    }
                }
                if (!TableExists("OrderTodayItem", _sqlConnection))
                {
                    sql = string.Format("CREATE TABLE {0} (" +
                        "OrderTodayId NVARCHAR(36) NOT NULL, " +
                        "ItemId NVARCHAR(36) NOT NULL, " +
                        "Quantity DECIMAL(18,3) NOT NULL, " +
                        "TotalPrice DECIMAL(18,2) NOT NULL, " +
                        "PRIMARY KEY(OrderTodayId, ItemId), " +
                        "FOREIGN KEY(OrderTodayId) REFERENCES OrderToday(Id) " +
                        "ON DELETE NO ACTION " +
                        "ON UPDATE NO ACTION, " +
                        "FOREIGN KEY(ItemId) REFERENCES Item(Id) " +
                        "ON DELETE NO ACTION " +
                        "ON UPDATE NO ACTION" +
                        "); ", "OrderTodayItem");
                    CreateTable("OrderTodayItem", sql);
                }
                if (!TableExists("Otpis", _sqlConnection))
                {
                    sql = string.Format("CREATE TABLE {0} (" +
                        "Id NVARCHAR(36) NOT NULL, " +
                        "CashierId NVARCHAR(4) NOT NULL, " + // Promena NVARCHAR(36) u NVARCHAR(4)
                        "OtpisDate DATETIME NOT NULL, " +
                        "Counter INT NOT NULL, " +
                        "Name NVARCHAR(MAX), " +
                        "PRIMARY KEY(Id), " +
                        "FOREIGN KEY(CashierId) REFERENCES Cashier(Id) " +
                        "ON DELETE NO ACTION " +
                        "ON UPDATE NO ACTION" +
                        "); ", "Otpis");
                    CreateTable("Otpis", sql);
                }
                if (!TableExists("OtpisItem", _sqlConnection))
                {
                    sql = string.Format("CREATE TABLE {0} (" +
                        "OtpisId NVARCHAR(36) NOT NULL, " +
                        "ItemId NVARCHAR(36) NOT NULL, " +
                        "Quantity DECIMAL(18,3) NOT NULL, " +
                        "TotalPrice DECIMAL(18,2) NOT NULL, " +
                        "Description NVARCHAR(MAX), " +
                        "PRIMARY KEY(OtpisId, ItemId), " +
                        "FOREIGN KEY(OtpisId) REFERENCES Otpis(Id) " +
                        "ON DELETE NO ACTION " +
                        "ON UPDATE NO ACTION, " +
                        "FOREIGN KEY(ItemId) REFERENCES Item(Id) " +
                        "ON DELETE NO ACTION " +
                        "ON UPDATE NO ACTION" +
                        "); ", "OtpisItem");
                    CreateTable("OtpisItem", sql);
                }
                if (!TableExists("ItemZelja", _sqlConnection))
                {
                    sql = string.Format("CREATE TABLE {0} (" +
                        "Id NVARCHAR(36) NOT NULL, " +
                        "ItemId NVARCHAR(36) NOT NULL, " +
                        "Zelja NVARCHAR(MAX), " +
                        "PRIMARY KEY(Id, ItemId), " +
                        "FOREIGN KEY(ItemId) REFERENCES Item(Id) " +
                        "ON DELETE NO ACTION " +
                        "ON UPDATE NO ACTION" +
                        "); ", "ItemZelja");
                    CreateTable("ItemZelja", sql);
                }
            }
            catch (Exception ex)
            {
                Log.Error("SqlServerDbContext - CreateTables - Error occurred while creating tables.", ex);
            }
        }
        /// <summary>
        /// Checks if table exists in SQL Server database
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        private bool TableExists(string tableName, SqlConnection connection)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = connection;
                    cmd.CommandText = "SELECT name FROM sys.tables WHERE name = @name";
                    cmd.Parameters.AddWithValue("@name", tableName);

                    using (SqlDataReader sqlDataReader = cmd.ExecuteReader())
                    {
                        return sqlDataReader.Read();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("SqlServerDbContext - TableExists - Error occurred while checking if the table {0} exists.", tableName), ex);
                return false;
            }
        }
        private void AddColumnIfNotExists(string tableName, string columnDefinition, SqlConnection connection)
        {
            try
            {
                string columnName = columnDefinition.Split(' ')[0];
                string checkColumnQuery = $"IF COL_LENGTH('{tableName}', '{columnName}') IS NULL ALTER TABLE {tableName} ADD {columnDefinition};";
                using (SqlCommand cmd = new SqlCommand(checkColumnQuery, connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Log.Error($"SqlServerDbContext - AddColumnIfNotExists - Error occurred while adding column {columnDefinition} to table {tableName}.", ex);
            }
        }

        /// <summary>
        /// Create table in SQL Server database
        /// </summary>
        /// <param name="tableName">The name of the table</param>
        /// <param name="sql">SQL query that creates the table</param>
        /// <returns></returns>
        private bool CreateTable(string tableName, string sql)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, _sqlConnection))
                {
                    cmd.ExecuteNonQuery();
                }
                return true;
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("There is already an object named") &&
                    !ex.Message.Contains("Cannot find the object"))
                {
                    Log.Error(string.Format("SqlServerDbContext - CreateTable - Error occurred while creating table {0}.", tableName), ex);
                }
                return false;
            }
        }
        #endregion Private methods
    }
}