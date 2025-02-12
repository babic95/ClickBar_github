using Microsoft.EntityFrameworkCore;
using ClickBar_DatabaseSQLManager;
using ClickBar_Database_Drlja;
using ClickBar_Settings;

public class DatabaseInitializer
{
    private readonly IDbContextFactory<SqlServerDbContext> _sqlServerDbContextFactory;
    private readonly IDbContextFactory<SqliteDrljaDbContext> _sqliteDrljaDbContextFactory;

    public DatabaseInitializer(IDbContextFactory<SqlServerDbContext> sqlServerDbContextFactory,
                               IDbContextFactory<SqliteDrljaDbContext> sqliteDrljaDbContextFactory)
    {
        _sqlServerDbContextFactory = sqlServerDbContextFactory;
        _sqliteDrljaDbContextFactory = sqliteDrljaDbContextFactory;

        EnsureDatabase();
    }

    private void EnsureDatabase()
    {
        using (var dbContext = _sqlServerDbContextFactory.CreateDbContext())
        {
            dbContext.Database.EnsureCreated();
        }

        if (!string.IsNullOrEmpty(SettingsManager.Instance.GetPathToDrljaKuhinjaDB()))
        {
            using (var dbContext = _sqliteDrljaDbContextFactory.CreateDbContext())
            {
                dbContext.Database.EnsureCreated();
            }
        }
    }
}