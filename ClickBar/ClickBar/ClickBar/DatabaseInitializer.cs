using Microsoft.EntityFrameworkCore;
using ClickBar_DatabaseSQLManager;
using ClickBar_Settings;
using Microsoft.Data.SqlClient;

public class DatabaseInitializer
{
    private readonly IDbContextFactory<SqlServerDbContext> _sqlServerDbContextFactory;

    public DatabaseInitializer(IDbContextFactory<SqlServerDbContext> sqlServerDbContextFactory)
    {
        _sqlServerDbContextFactory = sqlServerDbContextFactory;

        EnsureDatabase();
    }

    private void EnsureDatabase()
    {
        using (var dbContext = _sqlServerDbContextFactory.CreateDbContext())
        {
            if (!dbContext.Database.CanConnect())
            {
                CreateDatabase(dbContext);
            }
            dbContext.Database.EnsureCreated();
        }
    }

    private void CreateDatabase(SqlServerDbContext dbContext)
    {
        var masterConnection = new SqlConnection(SettingsManager.Instance.GetConnectionStringMaster());
        var databaseName = SettingsManager.Instance.GetSqlServerDatabaseName();

        using (var command = masterConnection.CreateCommand())
        {
            command.CommandText = $"IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'{databaseName}') CREATE DATABASE [{databaseName}]";
            masterConnection.Open();
            command.ExecuteNonQuery();
            masterConnection.Close();
        }
    }
}