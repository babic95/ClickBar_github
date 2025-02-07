using ClickBar_Database;
using ClickBar_Database_Drlja;
using ClickBar_Logging;
using ClickBar_Settings;
using Microsoft.OpenApi.Models;

try
{
    var builder = WebApplication.CreateBuilder(args);

    Logger.ConfigureLog(SettingsManager.Instance.GetLoggingFolderPath(), true);

    var pathToDB = SettingsManager.Instance.GetPathToDB();
    var pathToDrljaDB = SettingsManager.Instance.GetPathToDrljaKuhinjaDB();

    if (string.IsNullOrEmpty(pathToDrljaDB))
    {
        pathToDrljaDB = @"C:\KRUG2024\KRUG2024SQLITE3.db";
    }

    using (SqliteDrljaDbContext sqliteDrljaDbContext = new SqliteDrljaDbContext())
    {
        var isConnection = await sqliteDrljaDbContext.ConfigureDatabase(pathToDrljaDB);

        if (!isConnection)
        {
            return;
        }
    }
    using (SqliteDbContext sqliteDbContext = new SqliteDbContext())
    {
        var pomocnaBaza = SettingsManager.Instance.GetPathToMainDB();

        if (!string.IsNullOrEmpty(pomocnaBaza))
        {
            pathToDB = pomocnaBaza;
        }

        var isConnection = await sqliteDbContext.ConfigureDatabase(pathToDB);

        if (!isConnection)
        {
            return;
        }
    }
    Log.Debug("KONEKCIJA JE PROŠLA");

    // Servisna konfiguracija
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "v1",
            Title = "CCS Porudzbine API",
            Description = "Dostupni API za CCS Porudzbine!",
        });
    });
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAllOrigins", builder =>
        {
            builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        });
    });

    var app = builder.Build();

    // Middleware konfiguracija
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseSwaggerUI();
        app.UseSwagger(x => x.SerializeAsV2 = true);
    }
    else
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles(); // Omogućava servisiranje statičkih fajlova
    app.UseRouting();
    app.UseAuthorization();
    app.UseCors("AllowAllOrigins");

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
        endpoints.MapFallbackToFile("index.html"); // SPA fallback
    });

    // Ispisivanje server adrese i porta
    var addresses = app.Urls.ToArray();
    foreach (var address in addresses)
    {
        Log.Debug($"Server pokrenut na adresi: {address}");
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Error("Main - Main -> greska prilikom pokretanja servera za porudzbine: ", ex);
}
