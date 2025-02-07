using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Data;
using ClickBar_Database_Drlja.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ClickBar_Logging;

namespace ClickBar_Database_Drlja
{
    public partial class SqliteDrljaDbContext : DbContext
    {
        #region Fields
        private string _connectionString;
        private static string _fileDestination;
        private SqliteConnection _sqliteConnection;
        #endregion Fields
        public SqliteDrljaDbContext()
        {
            if (!string.IsNullOrEmpty(_fileDestination))
            {
                Connection();
            }
        }

        public SqliteDrljaDbContext(DbContextOptions<SqliteDrljaDbContext> options)
            : base(options)
        {
        }
        public virtual DbSet<ArtikliDB> Artikli { get; set; } = null!;
        public virtual DbSet<KategorijaDB> Kategorije { get; set; } = null!;
        public virtual DbSet<NarudzbeDB> Narudzbine { get; set; } = null!;
        public virtual DbSet<StavkeNarudzbeDB> StavkeNarudzbine { get; set; } = null!;
        public virtual DbSet<OprstiDB> Oprsti { get; set; } = null!;
        public virtual DbSet<RadnikDB> Radnici { get; set; } = null!;
        public virtual DbSet<ZeljaDB> Zelje { get; set; } = null!;
        public virtual DbSet<DeoSaleDB> DeloviSale { get; set; } = null!;
        public virtual DbSet<StoDB> Stolovi { get; set; } = null!;

        #region Public method
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
            modelBuilder.Entity<ArtikliDB>(entity =>
            {
                entity.HasKey(e => new { e.TR_ART })
                    .HasName("PRIMARY");
                entity.ToTable(SQLiteManagerTableNames.Artikli);

                entity.Property(e => e.TR_BRART).HasMaxLength(6);

                entity.Property(e => e.TR_NAZIV).HasMaxLength(30);

                entity.Property(e => e.TR_MEMO).HasMaxLength(65536);

                entity.Property(e => e.TR_PAK).HasMaxLength(3);

                entity.Property(e => e.TR_VRSTA).HasMaxLength(1);

                //entity.Property(e => e.TR_MPC);

                entity.Property(e => e.TR_PRO).HasMaxLength(3);
                entity.Property(e => e.TR_VRA).HasMaxLength(1);
                //entity.Property(e => e.TR_KATEGORIJA);
                entity.Property(e => e.TR_GLAVNI).HasMaxLength(2);
                //entity.Property(e => e.TR_ART);
            });

            modelBuilder.Entity<KategorijaDB>(entity =>
            {
                entity.HasKey(e => new { e.TR_KAT })
                    .HasName("PRIMARY");

                entity.ToTable(SQLiteManagerTableNames.Kategorije);

                //entity.Property(e => e.TR_KAT);

                entity.Property(e => e.TR_KATEGORIJA).HasMaxLength(6);

                entity.Property(e => e.TR_NAZIV).HasMaxLength(30);
            });

            modelBuilder.Entity<NarudzbeDB>(entity =>
            {
                entity.HasKey(e => new { e.Id })
                    .HasName("PRIMARY");
                //entity.HasKey(e => new { e.TR_BROJNARUDZBE })
                //    .HasName("PRIMARY");

                entity.ToTable(SQLiteManagerTableNames.Narudzbine);
            });

            modelBuilder.Entity<OprstiDB>(entity =>
            {
                entity.HasKey(e => new { e.ww_zir })
                    .HasName("PRIMARY");

                entity.ToTable(SQLiteManagerTableNames.Opsti);
            });

            modelBuilder.Entity<RadnikDB>(entity =>
            {
                entity.HasKey(e => new { e.LD_IDEN })
                    .HasName("PRIMARY");

                entity.ToTable(SQLiteManagerTableNames.Radnici);

                entity.Property(e => e.LD_IDEN).HasMaxLength(4);

                entity.Property(e => e.LD_IME).HasMaxLength(20);

                entity.Property(e => e.LD_PREZIME).HasMaxLength(20);
            });

            modelBuilder.Entity<StavkeNarudzbeDB>(entity =>
            {
                //entity.HasKey(e => new { e.TR_RBS })
                //    .HasName("PRIMARY");
                entity.HasKey(e => new { e.Id })
                    .HasName("PRIMARY");

                entity.ToTable(SQLiteManagerTableNames.StavkeNarudzbine);
            });

            modelBuilder.Entity<ZeljaDB>(entity =>
            {
                entity.HasKey(e => new { e.TR_IDZELJA })
                    .HasName("PRIMARY");

                entity.ToTable(SQLiteManagerTableNames.Zelje);
            });

            modelBuilder.Entity<DeoSaleDB>(entity =>
            {
                entity.HasKey(e => new { e.Id })
                    .HasName("PRIMARY");

                entity.ToTable(SQLiteManagerTableNames.DeoSale);
            });

            modelBuilder.Entity<StoDB>(entity =>
            {
                entity.HasKey(e => new { e.Id })
                    .HasName("PRIMARY");

                entity.ToTable(SQLiteManagerTableNames.Sto);
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
                    Log.Error(string.Format("SqliteDrljaDbContext - OpenConnection - File '{0}' doesn't exist.", filePath));
                    return false;
                }

                _sqliteConnection = new SqliteConnection(string.Format(_connectionString, _fileDestination));
                await _sqliteConnection.OpenAsync();

                //Log.Debug("SqliteDrljaDbContext - OpenConnection - Connection to sqlite database opened...");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("SqliteDrljaDbContext - OpenConnection - Open conn  :  " + ex);
                return false;
            }
        }
        private void CreateTables()
        {
            try
            {
                string sql = string.Empty;
                if (!TableExists(SQLiteManagerTableNames.DeoSale, _sqliteConnection))
                {
                    sql = string.Format("CREATE TABLE {0} ( " +
                        "'Id'	INTEGER NOT NULL, " +
                        "'Name'    TEXT NOT NULL, " +
                        "PRIMARY KEY(Id) " +
                        "); ", SQLiteManagerTableNames.DeoSale);
                    CreateTable(SQLiteManagerTableNames.DeoSale, sql);
                }
                if (!TableExists(SQLiteManagerTableNames.Sto, _sqliteConnection))
                {
                    sql = string.Format("CREATE TABLE {0} ( " +
                        "'Id'	TEXT NOT NULL, " +
                        "'Name'    TEXT NOT NULL, " +
                        "'DeoSaleId'    INTEGER NOT NULL, " +
                        "'X'    NUMERIC NOT NULL, " +
                        "'Y'    NUMERIC NOT NULL, " +
                        "'Width'    NUMERIC NOT NULL, " +
                        "'Height'    NUMERIC NOT NULL, " +
                        "PRIMARY KEY(Id) " +
                        "); ", SQLiteManagerTableNames.Sto);
                    CreateTable(SQLiteManagerTableNames.Sto, sql);
                }
            }
            catch(Exception ex)
            {
                Log.Error(string.Format("SqliteDrljaDbContext - TableExists - Greska prilikom kreiranja tabele."), ex);
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
                Log.Error(string.Format("SqliteDrljaDbContext - TableExists - Error occurred while checking if the table {0} exists.", tableName), ex);
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
                    Log.Error(string.Format("SqliteDrljaDbContext - CreateTable - Error occurred while creating table {0}.", tableName), ex);
                }
                return false;
            }
        }
        #endregion Private methods
    }
}
