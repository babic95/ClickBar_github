﻿using ClickBar.Commands.Sale;
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

                services.AddScoped<DatabaseInitializer>(provider =>
                    new DatabaseInitializer(
                        provider.GetRequiredService<IDbContextFactory<SqlServerDbContext>>(),
                        provider.GetRequiredService<IDbContextFactory<SqliteDrljaDbContext>>()
                    ));
            }
            else
            {
                services.AddScoped<DatabaseInitializer>(provider =>
                    new DatabaseInitializer(
                        provider.GetRequiredService<IDbContextFactory<SqlServerDbContext>>(),
                        null
                    ));
            }

            // Registracija INavigator sa MainViewModel
            services.AddSingleton<INavigator, MainViewModel>();
            //services.AddSingleton<MainViewModel>();

            // Registracija ViewModel-a sa odgovarajućim DbContext-om
            services.AddTransient<ActivationViewModel>();
            services.AddTransient<LoginCardViewModel>(provider =>
            {
                var dbContextFactory = provider.GetRequiredService<IDbContextFactory<SqlServerDbContext>>();
                var drljaDbContextFactory = provider.GetService<IDbContextFactory<SqliteDrljaDbContext>>();
                return new LoginCardViewModel(provider, dbContextFactory, drljaDbContextFactory);
            });
            services.AddTransient<LoginViewModel>(provider =>
            {
                var dbContextFactory = provider.GetRequiredService<IDbContextFactory<SqlServerDbContext>>();
                var drljaDbContextFactory = provider.GetService<IDbContextFactory<SqliteDrljaDbContext>>();
                return new LoginViewModel(provider, dbContextFactory, drljaDbContextFactory);
            });
            //services.AddTransient<TableOverviewViewModel>(provider =>
            //{
            //    var dbContextFactory = provider.GetRequiredService<IDbContextFactory<SqlServerDbContext>>();
            //    var drljaDbContextFactory = provider.GetService<IDbContextFactory<SqliteDrljaDbContext>>();
            //    var saleViewModel = provider.GetRequiredService<SaleViewModel>();
            //    return new TableOverviewViewModel(provider, dbContextFactory, drljaDbContextFactory, saleViewModel);
            //});
            services.AddTransient<SaleViewModel>(provider =>
            {
                var dbContextFactory = provider.GetRequiredService<IDbContextFactory<SqlServerDbContext>>();
                var drljaDbContextFactory = provider.GetService<IDbContextFactory<SqliteDrljaDbContext>>();
                return new SaleViewModel(provider, dbContextFactory, drljaDbContextFactory);
            });
            services.AddTransient<AppMainViewModel>(provider =>
            {
                var dbContextFactory = provider.GetRequiredService<IDbContextFactory<SqlServerDbContext>>();
                var drljaDbContextFactory = provider.GetService<IDbContextFactory<SqliteDrljaDbContext>>();
                return new AppMainViewModel(provider, dbContextFactory, drljaDbContextFactory);
            });
            //services.AddTransient<TableOverviewViewModel>();
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
            services.AddTransient<DPU_PeriodicniViewModel>(); 

            // Registracija prozora
            services.AddSingleton<MainWindow>();
            //services.AddTransient<SplitOrderWindow>();
            //services.AddTransient<PaySaleWindow>();

            // Registracija komandi
            services.AddTransient<UpdateCurrentAppStateViewModelCommand>(); // Registracija specifične komande
            //services.AddTransient<TableOverviewCommand>();
            //services.AddTransient<HookOrderOnTableCommand>();
            services.AddTransient<ActivationCommand>();
            //services.AddTransient<RefaundCommand>();
            //services.AddTransient<ChangePaymentPlaceCommand>();

            // Registracija generičke komande
            services.AddTransient(typeof(PayCommand<>));

            // Registracija ulogovanog korisnika (ako je potrebno)
            services.AddScoped<CashierDB>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                var mainWindow = _serviceProvider.GetService<MainWindow>();
                if (mainWindow != null)
                {
                    mainWindow.Show();
                    base.OnStartup(e);
                }
                else
                {
                    MessageBox.Show("Failed to initialize MainWindow. Please check your configurations.", "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception during startup: {ex.Message}", "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}