using ClickBar.Commands.AppMain.Report;
using ClickBar.Commands.Login;
using ClickBar.Commands.Sale;
using ClickBar.Commands;
using ClickBar.Models.Sale;
using ClickBar.Models.TableOverview;
using ClickBar_DatabaseSQLManager;
using ClickBar_Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using System.Timers;
using ClickBar.Enums.Kuhinja;
using Microsoft.EntityFrameworkCore.Internal;
using ClickBar.Models.AppMain.Kuhinja;
using System.Windows.Media;
using ClickBar.Commands.Kuhinja;
using DocumentFormat.OpenXml.VariantTypes;
using ClickBar.Commands.Kuhinja.Report;
using ClickBar_Printer.PaperFormat;
using ClickBar_Database;
using ClickBar_Common.Enums;
using System.Media;
using ClickBar_Logging;
using System.IO;

namespace ClickBar.ViewModels
{
    public class KuhinjaViewModel : ViewModelBase
    {
        #region Fields
        private IServiceProvider _serviceProvider;
        private readonly Lazy<UpdateCurrentAppStateViewModelCommand> _updateCurrentAppStateViewModelCommand;

        private Timer _timer;

        private ObservableCollection<PorudzbinaKuhinja> _porudzbine;
        private DateTime _fromDate;
        private DateTime _toDate;

        private MediaPlayer _mediaPlayer;
        #endregion Fields

        #region Constructors
        public KuhinjaViewModel(IServiceProvider serviceProvider,
            IDbContextFactory<SqlServerDbContext> dbContextFactory)
        {
            _serviceProvider = serviceProvider;
            DbContext = dbContextFactory;
            _updateCurrentAppStateViewModelCommand = new Lazy<UpdateCurrentAppStateViewModelCommand>(() => serviceProvider.GetRequiredService<UpdateCurrentAppStateViewModelCommand>());

            Porudzbine = new ObservableCollection<PorudzbinaKuhinja>();
            _mediaPlayer = new MediaPlayer();
            FromDate = DateTime.Now;
            ToDate = DateTime.Now;

            _timer = new Timer(5000); // 5 sekundi
            _timer.Elapsed += CheckForNewOrders;
            _timer.Start();
        }
        #endregion Constructors

        #region Internal Properties
        internal IDbContextFactory<SqlServerDbContext> DbContext
        {
            get; private set;
        }
        #endregion Internal Properties

        #region Properties
        public string CurrentDate { get; set; } = DateTime.Now.ToString("dd.MM.yyyy");
        public ObservableCollection<PorudzbinaKuhinja> Porudzbine
        {
            get { return _porudzbine; }
            set
            {
                _porudzbine = value;
                OnPropertyChange(nameof(Porudzbine));
            }
        }
        public DateTime FromDate
        {
            get { return _fromDate; }
            set
            {
                _fromDate = value;
                OnPropertyChange(nameof(FromDate));
            }
        }
        public DateTime ToDate
        {
            get { return _toDate; }
            set
            {
                _toDate = value;
                OnPropertyChange(nameof(ToDate));
            }
        }
        #endregion Properties

        #region Commands
        public ICommand LogoutCommand => new LogoutCommand(this, _serviceProvider);
        public ICommand UpdateAppViewModelCommand => _updateCurrentAppStateViewModelCommand.Value;
        public ICommand FinishPorudzbinaCommand => new FinishPorudzbinaCommand(this);
        public ICommand ViewItemsCommand => new ViewItemsCommand(this);
        public ICommand OpenWindowForReportCommand => new OpenWindowForReportCommand(this);
        public ICommand PrintPoArtikluCommand => new PrintPoArtikluCommand(this);
        public ICommand PrintPoKonobaruCommand => new PrintPoKonobaruCommand(this);
        public ICommand PrintGazdeCommand => new PrintGazdeCommand(this);
        public ICommand PrintOsobljeCommand => new PrintOsobljeCommand(this);
        public ICommand PrintDostavaCommand => new PrintDostavaCommand(this);
        public ICommand PrintKuhinjaCommand => new PrintKuhinjaCommand(this);
        #endregion Commands

        #region Public methods
        #endregion Public methods

        #region Internal methods
        #endregion Internal methods

        #region Private methods
        private async void CheckForNewOrders(object sender, ElapsedEventArgs e)
        {
            await Application.Current.Dispatcher.Invoke(async () =>
            {
                try
                {
                    using (var dbContext = DbContext.CreateDbContext())
                    {
                        var ordersDB = await dbContext.OrdersToday.Include(o => o.Cashier)
                            .Where(o => (o.Faza == (int)FazaKuhinjeEnumeration.Nova || o.Faza == (int)FazaKuhinjeEnumeration.UPripremi) &&
                            o.OrderDateTime.Date == DateTime.Now.Date &&
                            !string.IsNullOrEmpty(o.Name) &&
                            (o.Name.ToLower().Contains("k") ||
                            o.Name.ToLower().Contains("h") ||
                            o.Name.ToLower().Contains("storno")))
                            .ToListAsync();

                        if (ordersDB.Any())
                        {
                            // Ažuriraj fazu svih zapisa sa fazom 0 u fazu 1
                            foreach (var orderDB in ordersDB)
                            {
                                PorudzbinaKuhinja? porudzbinaKuhinja = Porudzbine.FirstOrDefault(p => p.OrderTodayDB.Id == orderDB.Id);

                                if (porudzbinaKuhinja == null)
                                {
                                    var itemsOrderDB = dbContext.OrderTodayItems.Where(o => o.OrderTodayId == orderDB.Id).Join(dbContext.Items,
                                        itemOrder => itemOrder.ItemId,
                                        item => item.Id,
                                        (itemOrder, item) => new PorudzbinaKuhinjaItem()
                                        {
                                            Id = item.Id,
                                            Name = item.Name,
                                            Jm = item.Jm,
                                            Quantity = itemOrder.Quantity,
                                            Total = itemOrder.TotalPrice,
                                            UnitPrice = Math.Abs(Decimal.Round(itemOrder.TotalPrice / itemOrder.Quantity, 2)),
                                            Zelja = itemOrder.Zelja
                                        });

                                    if (itemsOrderDB != null &&
                                        itemsOrderDB.Any())
                                    {
                                        List<PorudzbinaKuhinjaItem> items = new List<PorudzbinaKuhinjaItem>();
                                        foreach (var item in itemsOrderDB)
                                        {
                                            items.Add(item);
                                        }
                                        porudzbinaKuhinja = new PorudzbinaKuhinja(orderDB, items);

                                        Porudzbine.Add(porudzbinaKuhinja);

                                        if (orderDB.Faza == (int)FazaKuhinjeEnumeration.Nova)
                                        {
                                            PrintOrder(porudzbinaKuhinja);
                                            // Putanja do .wav datoteke
                                            string soundFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Zvukovi", "ZVUK.wav");
                                            PlaySound(soundFilePath);
                                        }
                                    }
                                }
                                orderDB.Faza = (int)FazaKuhinjeEnumeration.UPripremi;
                            }
                            await dbContext.SaveChangesAsync();
                        }
                    }

                    Porudzbine = new ObservableCollection<PorudzbinaKuhinja>(Porudzbine.OrderBy(p => p.OrderTodayDB.OrderDateTime));
                }
                catch (Exception ex)
                {
                    Log.Error("KuhinjaViewModel -> CheckForNewOrders -> desila se greska prilikom provere novih porudzbina: ", ex);
                    MessageBox.Show("Desila se greška prilikom provere novih porudžbina.", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
        }
        private void PrintOrder(PorudzbinaKuhinja porudzbinaKuhinja)
        {
            bool isStorno = porudzbinaKuhinja.OrderTodayDB.Name.ToLower().Contains("storno");
            ClickBar_Common.Models.Order.Order orderKuhinja = new ClickBar_Common.Models.Order.Order()
            {
                CashierName = porudzbinaKuhinja.OrderTodayDB.Cashier.Name,
                TableId = isStorno ? -1 : porudzbinaKuhinja.OrderTodayDB.TableId.Value,
                Items = new List<ClickBar_Common.Models.Order.ItemOrder>(),
                OrderTime = porudzbinaKuhinja.OrderTodayDB.OrderDateTime,
                OrderName = isStorno ? $"{porudzbinaKuhinja.OrderTodayDB.Name}" : $"K_{porudzbinaKuhinja.OrderTodayDB.CounterType}"
            };

            if (isStorno)
            {
                orderKuhinja.PartHall = "storno";
            }

            using (var dbContext = DbContext.CreateDbContext())
            {
                foreach (var item in porudzbinaKuhinja.Items)
                {
                    var itemNadgroup = dbContext.Items.Join(dbContext.ItemGroups,
                        item => item.IdItemGroup,
                        itemGroup => itemGroup.Id,
                        (item, itemGroup) => new { Item = item, ItemGroup = itemGroup })
                        .Join(dbContext.Supergroups,
                        group => group.ItemGroup.IdSupergroup,
                        supergroup => supergroup.Id,
                        (group, supergroup) => new { Group = group, Supergroup = supergroup })
                        .FirstOrDefault(it => it.Group.Item.Id == item.Id);

                    if (itemNadgroup != null)
                    {
                        if (itemNadgroup.Supergroup.Name.ToLower().Contains("hrana") ||
                            itemNadgroup.Supergroup.Name.ToLower().Contains("kuhinja"))
                        {
                            orderKuhinja.Items.Add(new ClickBar_Common.Models.Order.ItemOrder()
                            {
                                Name = item.Name,
                                Quantity = item.Quantity,
                                Id = item.Id,
                                TotalAmount = item.Total,
                                Zelja = item.Zelja
                            });
                        }
                    }
                }
            }

            var posType = SettingsManager.Instance.GetPrinterFormat() == PrinterFormatEnumeration.Pos80mm ?
                ClickBar_Printer.Enums.PosTypeEnumeration.Pos80mm : ClickBar_Printer.Enums.PosTypeEnumeration.Pos58mm;

            FormatPos.PrintOrder(orderKuhinja, posType, ClickBar_Printer.Enums.OrderTypeEnumeration.Kuhinja);

        }
        private void PlaySound(string filePath)
        {
            try
            {
                // Kreiranje SoundPlayer instance i reprodukcija zvuka
                SoundPlayer player = new SoundPlayer(filePath);
                player.Play();
                //_mediaPlayer.Open(new Uri(filePath, UriKind.Relative));
                //_mediaPlayer.Play();
            }
            catch (Exception ex)
            {
                Log.Error("KuhinjaViewModel -> PlaySound -> desila se greska prilikom reprodukcije zvuka: ", ex);
                MessageBox.Show("Desila se greška prilikom reprodukcije zvuka.", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    #endregion Private methods
}
}