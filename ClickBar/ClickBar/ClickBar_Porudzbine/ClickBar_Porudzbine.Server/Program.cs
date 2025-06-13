using ClickBar_DatabaseSQLManager;
using ClickBar_Logging;
using ClickBar_Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

try
{
    var builder = WebApplication.CreateBuilder(args);

    Logger.ConfigureLog(SettingsManager.Instance.GetLoggingFolderPath(), true);

    // Konfiguracija servisa
    string databaseConnectionString = SettingsManager.Instance.GetConnectionString();

    builder.Services.AddDbContext<SqlServerDbContext>(options =>
        options.UseSqlServer(databaseConnectionString, sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure();
        }));

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