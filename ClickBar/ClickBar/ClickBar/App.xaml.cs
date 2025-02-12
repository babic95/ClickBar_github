using ClickBar.Commands.Sale;
using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar.ViewModels.Login;
using ClickBar.ViewModels;
using ClickBar_Database_Drlja;
using ClickBar_DatabaseSQLManager;
using ClickBar_Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using ClickBar.ViewModels.Activation;
using ClickBar.ViewModels.Sale;
using ClickBar.ViewModels.AppMain;
using ClickBar.Commands;
using System.Windows.Input;
using ClickBar_DatabaseSQLManager.Models;
using ClickBar.Commands.Activation;
using ClickBar.Commands.AppMain.Statistic.Refaund;
using ClickBar.Views.Sale.PaySale;
using ClickBar.Commands.Sale.Pay.SplitOrder;
using ClickBar.Commands.Sale.Pay;
using ClickBar.State.Navigators;

namespace ClickBar
{
    public partial class App : Application
    {
        private ServiceProvider _serviceProvider;

        public App()
        {
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
            _serviceProvider.GetService<DatabaseInitializer>(); // Inicijalizacija baze podataka
        }

        private void ConfigureServices(ServiceCollection services)
        {
            string connectionString = SettingsManager.Instance.GetConnectionString();
            services.AddDbContext<SqlServerDbContext>(options =>
                options.UseSqlServer(connectionString), ServiceLifetime.Scoped);
            services.AddDbContextFactory<SqlServerDbContext>(options =>
                options.UseSqlServer(connectionString), ServiceLifetime.Scoped);

            string? pathToDrljaDB = SettingsManager.Instance.GetPathToDrljaKuhinjaDB();
            if (!string.IsNullOrEmpty(pathToDrljaDB))
            {
                string drljaConnectionString = new SqliteDrljaDbContext().CreateConnectionString(pathToDrljaDB);
                services.AddDbContext<SqliteDrljaDbContext>(options =>
                    options.UseSqlite(drljaConnectionString), ServiceLifetime.Scoped);
                services.AddDbContextFactory<SqliteDrljaDbContext>(options =>
                    options.UseSqlite(drljaConnectionString), ServiceLifetime.Scoped);
            }

            services.AddSingleton<DatabaseInitializer>(); // Registracija DatabaseInitializer kao Singleton

            // Registracija INavigator sa MainViewModel
            services.AddTransient<INavigator, MainViewModel>();

            // Registracija ViewModel-a sa odgovarajućim DbContext-om
            services.AddTransient<MainViewModel>();
            services.AddTransient<ActivationViewModel>();
            services.AddTransient<LoginCardViewModel>(provider =>
            {
                var dbContextFactory = provider.GetRequiredService<IDbContextFactory<SqlServerDbContext>>();
                var drljaDbContextFactory = provider.GetRequiredService<IDbContextFactory<SqliteDrljaDbContext>>();
                return new LoginCardViewModel(provider, dbContextFactory, drljaDbContextFactory);
            });
            services.AddTransient<LoginViewModel>(provider =>
            {
                var dbContextFactory = provider.GetRequiredService<IDbContextFactory<SqlServerDbContext>>();
                var drljaDbContextFactory = provider.GetRequiredService<IDbContextFactory<SqliteDrljaDbContext>>();
                return new LoginViewModel(provider, dbContextFactory, drljaDbContextFactory);
            });
            services.AddTransient<TableOverviewViewModel>(provider =>
            {
                var dbContextFactory = provider.GetRequiredService<IDbContextFactory<SqlServerDbContext>>();
                var drljaDbContextFactory = provider.GetRequiredService<IDbContextFactory<SqliteDrljaDbContext>>();
                return new TableOverviewViewModel(provider, dbContextFactory, drljaDbContextFactory);
            });
            services.AddTransient<SaleViewModel>(provider =>
            {
                var dbContextFactory = provider.GetRequiredService<IDbContextFactory<SqlServerDbContext>>();
                var drljaDbContextFactory = provider.GetRequiredService<IDbContextFactory<SqliteDrljaDbContext>>();
                return new SaleViewModel(provider, dbContextFactory, drljaDbContextFactory);
            });
            services.AddTransient<AppMainViewModel>(provider =>
            {
                var dbContextFactory = provider.GetRequiredService<IDbContextFactory<SqlServerDbContext>>();
                var drljaDbContextFactory = provider.GetRequiredService<IDbContextFactory<SqliteDrljaDbContext>>();
                return new AppMainViewModel(provider, dbContextFactory, drljaDbContextFactory);
            });
            services.AddTransient<SplitOrderViewModel>();
            services.AddTransient<PaySaleViewModel>();
            services.AddTransient<ChangePaymentPlaceViewModel>();
            services.AddTransient<StatisticsViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<ReportViewModel>();
            services.AddTransient<AdminViewModel>();
            services.AddTransient<AddEditSupplierViewModel>();
            services.AddTransient<CalculationViewModel>();
            services.AddTransient<FirmaViewModel>();
            services.AddTransient<InventoryStatusViewModel>();
            services.AddTransient<KEPViewModel>();
            services.AddTransient<KnjizenjeViewModel>();
            services.AddTransient<LagerListaViewModel>();
            services.AddTransient<NivelacijaViewModel>();
            services.AddTransient<NormViewModel>();
            services.AddTransient<OtpisPreviewViewModel>();
            services.AddTransient<OtpisViewModel>();
            services.AddTransient<PartnerViewModel>();
            services.AddTransient<PayRefaundViewModel>();
            services.AddTransient<PocetnoStanjeViewModel>();
            services.AddTransient<PregledPazaraViewModel>();
            services.AddTransient<PriceIncreaseViewModel>();
            services.AddTransient<RadniciViewModel>();
            services.AddTransient<RefaundViewModel>();
            services.AddTransient<ViewCalculationViewModel>();
            services.AddTransient<ViewNivelacijaViewModel>();

            // Registracija prozora
            services.AddSingleton<MainWindow>();
            services.AddTransient<SplitOrderWindow>();
            services.AddTransient<PaySaleWindow>();

            // Registracija komandi
            services.AddTransient<UpdateCurrentAppStateViewModelCommand>(); // Registracija specifične komande
            services.AddTransient<TableOverviewCommand>();
            services.AddTransient<HookOrderOnTableCommand>();
            services.AddTransient<ActivationCommand>();
            services.AddTransient<RefaundCommand>();
            services.AddTransient<ChangePaymentPlaceCommand>();
            services.AddTransient<SplitOrderCommand>();

            // Registracija generičke komande
            services.AddTransient(typeof(PayCommand<>));

            // Registracija ulogovanog korisnika (ako je potrebno)
            services.AddTransient<CashierDB>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var mainWindow = _serviceProvider.GetService<MainWindow>();
            mainWindow.Show();
            base.OnStartup(e);
        }
    }
}