using ClickBar.Commands.AppMain.Admin;
using ClickBar.Commands.TableOverview;
using ClickBar.Enums.AppMain.Admin;
using ClickBar.Models.Sale;
using ClickBar.Models.TableOverview;
using ClickBar.Models.TableOverview.Kuhinja;
using ClickBar_Common.Enums;
using ClickBar_DatabaseSQLManager;
using ClickBar_DatabaseSQLManager.Models;
using ClickBar_Printer.PaperFormat;
using ClickBar_Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Timers;
using Microsoft.Data.Sqlite;
using System.Windows.Threading;
using ClickBar_Database_Drlja;
using ClickBar_Database_Drlja.Models;
using Microsoft.EntityFrameworkCore;
using ClickBar_Logging;
using System.Media;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Policy;

namespace ClickBar.ViewModels
{
    public class TableOverviewViewModel : ViewModelBase
    {
        #region Fields
        private ObservableCollection<PartHall> _rooms;
        private ObservableCollection<PaymentPlace> _allNormalPaymentPlaces;
        private ObservableCollection<PaymentPlace> _normalPaymentPlaces;

        private ObservableCollection<PaymentPlace> _allRoundPaymentPlaces;
        private ObservableCollection<PaymentPlace> _roundPaymentPlaces;
        private PartHall? _currentPartHall;
        private string _title;

        private Visibility _kuhinjaVisibility;

        private ObservableCollection<Narudzbe> _narudzbe;
        private Narudzbe _currentNarudzba;
        private decimal _totalNarudzbeSmena1;
        private decimal _totalNarudzbeSmena2;
        private decimal _totalNarudzbeDostava1;
        private decimal _totalNarudzbeDostava2;
        private decimal _totalNarudzbe;
        private DateTime _selectedDate;
        #endregion Fields

        #region Constructors
        public TableOverviewViewModel(IServiceProvider serviceProvider, IDbContextFactory<SqlServerDbContext> dbContextFactory, IDbContextFactory<SqliteDrljaDbContext> drljaDbContextFactory)
        {
            DbContext = dbContextFactory.CreateDbContext();
            DrljaDbContext = drljaDbContextFactory.CreateDbContext();
            SaleViewModel = serviceProvider.GetRequiredService<SaleViewModel>();
            Order = SaleViewModel.CurrentOrder;

            Rooms = new ObservableCollection<PartHall>();
            AllNormalPaymentPlaces = new ObservableCollection<PaymentPlace>();
            NormalPaymentPlaces = new ObservableCollection<PaymentPlace>();
            AllRoundPaymentPlaces = new ObservableCollection<PaymentPlace>();
            RoundPaymentPlaces = new ObservableCollection<PaymentPlace>();
            LoadingDB();

            if (!string.IsNullOrEmpty(SettingsManager.Instance.GetPathToDrljaKuhinjaDB()))
            {
                KuhinjaVisibility = Visibility.Visible;
            }
            else
            {
                KuhinjaVisibility = Visibility.Hidden;
            }

            if (SaleViewModel.CurrentPartHall == null)
            {
                CurrentPartHall = Rooms.FirstOrDefault();
            }
            else
            {
                CurrentPartHall = Rooms.FirstOrDefault(r => r.Id == SaleViewModel.CurrentPartHall.Id);
            }

            StartPolling();
            StartPollingStatusStolova();
            SelectedDate = DateTime.Now;
        }
        #endregion Constructors

        #region Internal Properties
        internal SqlServerDbContext DbContext { get; private set; }
        internal SqliteDrljaDbContext DrljaDbContext { get; private set; }
        #endregion Internal Properties

        #region Properties
        public Narudzbe CurrentNarudzba
        {
            get { return _currentNarudzba; }
            set
            {
                _currentNarudzba = value;
                OnPropertyChange(nameof(CurrentNarudzba));
            }
        }
        public decimal TotalNarudzbe
        {
            get { return _totalNarudzbe; }
            set
            {
                _totalNarudzbe = value;
                OnPropertyChange(nameof(TotalNarudzbe));
            }
        }
        public decimal TotalNarudzbeSmena1
        {
            get { return _totalNarudzbeSmena1; }
            set
            {
                _totalNarudzbeSmena1 = value;
                OnPropertyChange(nameof(TotalNarudzbeSmena1));
            }
        }
        public decimal TotalNarudzbeSmena2
        {
            get { return _totalNarudzbeSmena2; }
            set
            {
                _totalNarudzbeSmena2 = value;
                OnPropertyChange(nameof(TotalNarudzbeSmena2));
            }
        }
        public decimal TotalNarudzbeDostava1
        {
            get { return _totalNarudzbeDostava1; }
            set
            {
                _totalNarudzbeDostava1 = value;
                OnPropertyChange(nameof(TotalNarudzbeDostava1));
            }
        }
        public decimal TotalNarudzbeDostava2
        {
            get { return _totalNarudzbeDostava2; }
            set
            {
                _totalNarudzbeDostava2 = value;
                OnPropertyChange(nameof(TotalNarudzbeDostava2));
            }
        }
        public Visibility KuhinjaVisibility
        {
            get { return _kuhinjaVisibility; }
            set
            {
                _kuhinjaVisibility = value;
                OnPropertyChange(nameof(KuhinjaVisibility));
            }
        }
        public ObservableCollection<PartHall> Rooms
        {
            get { return _rooms; }
            set
            {
                _rooms = value;
                OnPropertyChange(nameof(Rooms));
            }
        }
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChange(nameof(Title));
            }
        }
        public ObservableCollection<PaymentPlace> NormalPaymentPlaces
        {
            get { return _normalPaymentPlaces; }
            set
            {
                _normalPaymentPlaces = value;
                OnPropertyChange(nameof(NormalPaymentPlaces));
            }
        }

        public ObservableCollection<PaymentPlace> AllNormalPaymentPlaces
        {
            get { return _allNormalPaymentPlaces; }
            set
            {
                _allNormalPaymentPlaces = value;
                OnPropertyChange(nameof(AllNormalPaymentPlaces));
            }
        }

        public ObservableCollection<PaymentPlace> RoundPaymentPlaces
        {
            get { return _roundPaymentPlaces; }
            set
            {
                _roundPaymentPlaces = value;
                OnPropertyChange(nameof(RoundPaymentPlaces));
            }
        }

        public ObservableCollection<PaymentPlace> AllRoundPaymentPlaces
        {
            get { return _allRoundPaymentPlaces; }
            set
            {
                _allRoundPaymentPlaces = value;
                OnPropertyChange(nameof(AllRoundPaymentPlaces));
            }
        }
        public PartHall? CurrentPartHall
        {
            get { return _currentPartHall; }
            set
            {
                if (_currentPartHall != value)
                {
                    _currentPartHall = value;
                    OnPropertyChange(nameof(CurrentPartHall));

                    if (value != null)
                    {
                        NormalPaymentPlaces = new ObservableCollection<PaymentPlace>(AllNormalPaymentPlaces.Where(p => p.PartHallId == value.Id));
                        RoundPaymentPlaces = new ObservableCollection<PaymentPlace>(AllRoundPaymentPlaces.Where(p => p.PartHallId == value.Id));

                        Title = value.Name;
                    }
                }
            }
        }
        public ObservableCollection<Narudzbe> Narudzbe
        {
            get { return _narudzbe; }
            set
            {
                _narudzbe = value;
                OnPropertyChange(nameof(Narudzbe));
            }
        }

        public DateTime SelectedDate
        {
            get { return _selectedDate; }
            set
            {
                _selectedDate = value;
                OnPropertyChange(nameof(SelectedDate));

                if (value != null)
                {
                    SetKuhinjaNarudzbine();
                }
            }
        }
        #endregion Properties

        #region Internal Properties
        internal SaleViewModel SaleViewModel { get; set; }
        internal Order? Order { get; set; }
        #endregion Internal Properties

        #region Commands
        public ICommand SelectRoomCommand => new SelectRoomCommand(this);
        public ICommand CancelCommand => new CancelCommand(this);
        public ICommand ClickOnPaymentPlaceCommand => new ClickOnPaymentPlaceCommand(this);
        public ICommand OpenKuhinjaCommand => new OpenKuhinjaCommand(this);
        public ICommand OpenItemsInNarudzbinaCommand => new OpenItemsInNarudzbinaCommand(this);
        #endregion Commands

        #region Public methods
        public void SetOrder(Order order)
        {
            PaymentPlace? paymentPlace = NormalPaymentPlaces.FirstOrDefault(pp => pp.Id == order.TableId);

            if (paymentPlace == null)
            {
                paymentPlace = RoundPaymentPlaces.FirstOrDefault(pp => pp.Id == order.TableId);
            }

            if (paymentPlace != null)
            {
                if (paymentPlace.Order != null && paymentPlace.Order.Items.Any())
                {
                    order.Items.ToList().ForEach(item =>
                    {
                        var itemInPaymentPlace = paymentPlace.Order.Items.FirstOrDefault(i => i.Item.Id == item.Item.Id);
                        if (itemInPaymentPlace != null)
                        {
                            itemInPaymentPlace.TotalAmout += item.TotalAmout;
                            itemInPaymentPlace.Quantity += item.Quantity;
                        }
                        else
                        {
                            paymentPlace.Order.Items.Add(item);
                        }
                    });
                }
                else
                {
                    paymentPlace.Order = new Order(order.TableId, order.PartHall)
                    {
                        Items = new ObservableCollection<ItemInvoice>(order.Items),
                        Cashier = order.Cashier
                    };
                    paymentPlace.Total = SaleViewModel.TotalAmount;
                    paymentPlace.Background = Brushes.Red;
                }

                SaleViewModel.TableId = 0;
                SaleViewModel.ItemsInvoice = new ObservableCollection<ItemInvoice>();
                SaleViewModel.TotalAmount = 0;
            }
        }
        #endregion Public methods

        #region Internal Methods
        internal void SetKuhinjaNarudzbine()
        {
            Narudzbe = new ObservableCollection<Narudzbe>();
            var allNarudzbe = DrljaDbContext.StavkeNarudzbine.AsNoTracking()
                .Join(DrljaDbContext.Narudzbine.AsNoTracking(),
                s => s.TR_NARUDZBE_ID,
                n => n.TR_NARUDZBE_ID,
                (s, n) => new { s, n })
                .Where(n => n.n.TR_VREMENARUDZBE.Date == SelectedDate.Date);

            if (allNarudzbe.Any())
            {
                var maxBrPorudzbine = DrljaDbContext.Narudzbine.AsNoTracking()
                    .Where(n => n.TR_VREMENARUDZBE.Date == SelectedDate.Date)
                    .Max(n => n.TR_BROJNARUDZBE);

                var minBrPorudzbine = DrljaDbContext.Narudzbine.AsNoTracking()
                    .Where(n => n.TR_VREMENARUDZBE.Date == SelectedDate.Date)
                    .Min(n => n.TR_BROJNARUDZBE);

                var allStavkeToDay = DrljaDbContext.StavkeNarudzbine.AsNoTracking()
                    .Where(s => s.TR_BROJNARUDZBE >= minBrPorudzbine &&
                    s.TR_BROJNARUDZBE <= maxBrPorudzbine);

                if (allStavkeToDay != null && allStavkeToDay.Any())
                {
                    TotalNarudzbeSmena1 = 0;
                    TotalNarudzbeSmena2 = 0;
                    TotalNarudzbeDostava1 = 0;
                    TotalNarudzbeDostava2 = 0;
                    TotalNarudzbe = 0;

                    allStavkeToDay.ForEachAsync(s =>
                    {
                        try
                        {
                            var narudzbaDB = DrljaDbContext.Narudzbine.AsNoTracking()
                                .Where(n => n.TR_NARUDZBE_ID == s.TR_NARUDZBE_ID)
                                .OrderByDescending(n => n.TR_VREMENARUDZBE)
                                .FirstOrDefault();

                            if (narudzbaDB != null)
                            {
                                var narudzbe = Narudzbe.FirstOrDefault(na => na.BrojNarudzbe == s.TR_BROJNARUDZBE &&
                                    na.NarudzneId == s.TR_NARUDZBE_ID);

                                if (narudzbe == null)
                                {
                                    narudzbe = new Narudzbe()
                                    {
                                        Id = narudzbaDB.Id,
                                        BrojNarudzbe = s.TR_BROJNARUDZBE,
                                        NarudzneId = s.TR_NARUDZBE_ID,
                                        RadnikId = narudzbaDB.TR_RADNIK,
                                        Smena = narudzbaDB.TR_RADNIK == "1111" || narudzbaDB.TR_RADNIK == "3333" ? 1 : 2,
                                        StoName = narudzbaDB.TR_STO,
                                        VremeNarudzbe = Convert.ToDateTime(narudzbaDB.TR_VREMENARUDZBE),
                                        Stavke = new ObservableCollection<StavkaNarudzbe>()
                                    };
                                    Narudzbe.Add(narudzbe);
                                }
                                narudzbe.Stavke.Add(new StavkaNarudzbe()
                                {
                                    BrArt = s.TR_BRART,
                                    Id = s.Id,
                                    Kolicina = s.TR_KOL,
                                    Mpc = s.TR_MPC,
                                    Naziv = s.TR_NAZIV,
                                    StornoKolicina = s.TR_KOL_STORNO,
                                    Ukupno = decimal.Round((s.TR_KOL - s.TR_KOL_STORNO) * s.TR_MPC, 2)
                                });

                                if (s.TR_KOL_STORNO > 0)
                                {
                                    narudzbe.Storno = "IMA STORNO";
                                }
                                else
                                {
                                    narudzbe.Storno = "NEMA STORNO";
                                }

                                if (narudzbe.RadnikId == "1111")
                                {
                                    TotalNarudzbeSmena1 += decimal.Round((s.TR_KOL - s.TR_KOL_STORNO) * s.TR_MPC, 2);
                                }
                                else if (narudzbe.RadnikId == "2222")
                                {
                                    TotalNarudzbeSmena2 += decimal.Round((s.TR_KOL - s.TR_KOL_STORNO) * s.TR_MPC, 2);
                                }
                                else if (narudzbe.RadnikId == "3333")
                                {
                                    TotalNarudzbeDostava1 += decimal.Round((s.TR_KOL - s.TR_KOL_STORNO) * s.TR_MPC, 2);
                                }
                                else if (narudzbe.RadnikId == "4444")
                                {
                                    TotalNarudzbeDostava2 += decimal.Round((s.TR_KOL - s.TR_KOL_STORNO) * s.TR_MPC, 2);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error("SetKuhinjaNarudzbine -> Error processing order items: ", ex);
                        }
                    });
                    TotalNarudzbe = TotalNarudzbeSmena1 +
                        TotalNarudzbeSmena2 +
                        TotalNarudzbeDostava1 +
                        TotalNarudzbeDostava2;

                    Narudzbe = new ObservableCollection<Narudzbe>(Narudzbe.OrderByDescending(n => n.BrojNarudzbe));
                }
            }
        }
        #endregion Internal Methods

        #region Private methods
        private void LoadingDB()
        {
            var remove0Value = DbContext.UnprocessedOrders
                .GroupJoin(
                    DbContext.ItemsInUnprocessedOrder,
                    order => order.Id,
                    item => item.UnprocessedOrderId,
                    (order, items) => new { Order = order, Items = items }
                )
                .Where(x => !x.Items.Any())
                .Select(x => x.Order);

            if (remove0Value.Any())
            {
                DbContext.UnprocessedOrders.RemoveRange(remove0Value);
                DbContext.SaveChanges();
            }

            DbContext.PartHalls.AsNoTracking().ToList().ForEach(part =>
            {
                PartHall partHall = new PartHall()
                {
                    Id = part.Id,
                    Name = part.Name,
                    Image = part.Image
                };

                Rooms.Add(partHall);
            });

            DbContext.PaymentPlaces.AsNoTracking().ToList().ForEach(payment =>
            {
                PaymentPlace paymentPlace = new PaymentPlace()
                {
                    Id = payment.Id,
                    PartHallId = payment.PartHallId,
                    Left = payment.LeftCanvas.Value,
                    Top = payment.TopCanvas.Value,
                    Type = payment.Type.HasValue ? (PaymentPlaceTypeEnumeration)payment.Type.Value : PaymentPlaceTypeEnumeration.Normal,
                    Name = !string.IsNullOrEmpty(payment.Name) ? payment.Name : payment.Id.ToString(),
                    Popust = payment.Popust
                };

                if (paymentPlace.Type == PaymentPlaceTypeEnumeration.Normal)
                {
                    paymentPlace.Width = payment.Width.Value;
                    paymentPlace.Height = payment.Height.Value;
                }
                else
                {
                    paymentPlace.Diameter = payment.Width.Value;
                }

                var unprocessedOrders = DbContext.UnprocessedOrders.AsNoTracking().FirstOrDefault(order => order.PaymentPlaceId == payment.Id);

                if (unprocessedOrders != null)
                {
                    CashierDB? cashierDB = DbContext.Cashiers.AsNoTracking().FirstOrDefault(c => c.Id == unprocessedOrders.CashierId);
                    var itemsInUnprocessedOrder = DbContext.Items.AsNoTracking()
                        .Join(DbContext.ItemsInUnprocessedOrder,
                        item => item.Id,
                        itemInUnprocessedOrder => itemInUnprocessedOrder.ItemId,
                        (item, itemInUnprocessedOrder) => new { Item = item, ItemInUnprocessedOrder = itemInUnprocessedOrder })
                        .Where(item => item.ItemInUnprocessedOrder.UnprocessedOrderId == unprocessedOrders.Id);

                    if (cashierDB != null && itemsInUnprocessedOrder.Any())
                    {
                        ObservableCollection<ItemInvoice> items = new ObservableCollection<ItemInvoice>();
                        decimal total = 0;

                        itemsInUnprocessedOrder.ToList().ForEach(item =>
                        {
                            ItemInvoice itemInvoice = new ItemInvoice(new Item(item.Item), item.ItemInUnprocessedOrder.Quantity);
                            items.Add(itemInvoice);
                            total += itemInvoice.TotalAmout;
                        });

                        paymentPlace.Order = new Order(cashierDB, items);
                        paymentPlace.Order.TableId = payment.Id;
                        paymentPlace.Order.PartHall = payment.PartHallId;
                        paymentPlace.Background = Brushes.Red;
                        paymentPlace.Total = total;
                    }
                }
                else
                {
                    paymentPlace.Order = new Order(payment.Id, payment.PartHallId);
                    paymentPlace.Background = Brushes.Green;
                    paymentPlace.Total = 0;
                }

                switch (paymentPlace.Type)
                {
                    case PaymentPlaceTypeEnumeration.Normal:
                        AllNormalPaymentPlaces.Add(paymentPlace);
                        break;
                    case PaymentPlaceTypeEnumeration.Round:
                        AllRoundPaymentPlaces.Add(paymentPlace);
                        break;
                }
            });

            if (CurrentPartHall != null)
            {
                Title = CurrentPartHall.Name;
            }
        }

        private void StartPolling()
        {
            if (SaleViewModel.Timer != null)
            {
                SaleViewModel.Timer.Stop();
                SaleViewModel.Timer.Dispose();
            }
            SaleViewModel.Timer = new Timer(5000); // Proverava svakih 5 sekundi
            SaleViewModel.Timer.Elapsed += CheckDatabase;
            SaleViewModel.Timer.AutoReset = true;
            SaleViewModel.Timer.Enabled = true;
        }

        private void StartPollingStatusStolova()
        {
            if (SaleViewModel.Timer1 != null)
            {
                SaleViewModel.Timer1.Stop();
                SaleViewModel.Timer1.Dispose();
            }
            SaleViewModel.Timer1 = new Timer(5000); // Proverava svakih 5 sekundi
            SaleViewModel.Timer1.Elapsed += CheckDatabaseStatusStolova;
            SaleViewModel.Timer1.AutoReset = true;
            SaleViewModel.Timer1.Enabled = true;
        }

        private void CheckDatabaseStatusStolova(object sender, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var updatedPaymentPlaces = DbContext.PaymentPlaces.AsNoTracking().ToList();
                foreach (var paymentPlace in updatedPaymentPlaces)
                {
                    var existingPaymentPlace = AllNormalPaymentPlaces.FirstOrDefault(p => p.Id == paymentPlace.Id) ??
                                               AllRoundPaymentPlaces.FirstOrDefault(p => p.Id == paymentPlace.Id);

                    if (existingPaymentPlace != null)
                    {
                        // Ažuriraj status stolova
                        var unprocessedOrders = DbContext.UnprocessedOrders.AsNoTracking().FirstOrDefault(order => order.PaymentPlaceId == paymentPlace.Id);
                        if (unprocessedOrders != null)
                        {
                            CashierDB? cashierDB = DbContext.Cashiers.AsNoTracking().FirstOrDefault(c => c.Id == unprocessedOrders.CashierId);
                            var itemsInUnprocessedOrder = DbContext.Items.AsNoTracking()
                                .Join(DbContext.ItemsInUnprocessedOrder,
                                item => item.Id,
                                itemInUnprocessedOrder => itemInUnprocessedOrder.ItemId,
                                (item, itemInUnprocessedOrder) => new { Item = item, ItemInUnprocessedOrder = itemInUnprocessedOrder })
                                .Where(item => item.ItemInUnprocessedOrder.UnprocessedOrderId == unprocessedOrders.Id);

                            if (cashierDB != null && itemsInUnprocessedOrder.Any())
                            {
                                ObservableCollection<ItemInvoice> items = new ObservableCollection<ItemInvoice>();
                                decimal total = 0;

                                itemsInUnprocessedOrder.ToList().ForEach(item =>
                                {
                                    ItemInvoice itemInvoice = new ItemInvoice(new Item(item.Item), item.ItemInUnprocessedOrder.Quantity);
                                    items.Add(itemInvoice);
                                    total += itemInvoice.TotalAmout;
                                });

                                existingPaymentPlace.Order = new Order(cashierDB, items);
                                existingPaymentPlace.Order.TableId = paymentPlace.Id;
                                existingPaymentPlace.Order.PartHall = paymentPlace.PartHallId;
                                existingPaymentPlace.Background = Brushes.Red;
                                existingPaymentPlace.Total = total;
                            }
                        }
                        else
                        {
                            existingPaymentPlace.Order = new Order(paymentPlace.Id, paymentPlace.PartHallId);
                            existingPaymentPlace.Background = Brushes.Green;
                            existingPaymentPlace.Total = 0;
                        }
                    }
                }
            });
        }

        private void CheckDatabase(object sender, ElapsedEventArgs e)
        {
            var noveNarudzbe = DrljaDbContext.Narudzbine.AsNoTracking()
                .Where(n => n.TR_FAZA == 2 && n.TR_VREMENARUDZBE.Date == DateTime.Now.Date);

            if (noveNarudzbe != null && noveNarudzbe.Any())
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    noveNarudzbe.ForEachAsync(n =>
                    {
                        if (n.TR_STO.StartsWith("S"))
                        {
                            string numberPart = n.TR_STO.Substring(1);

                            if (int.TryParse(numberPart, out int stoId))
                            {
                                var sto = AllNormalPaymentPlaces.FirstOrDefault(p => p.Id == stoId)
                                    ?? AllRoundPaymentPlaces.FirstOrDefault(p => p.Id == stoId);

                                if (sto != null && sto.Background != Brushes.Blue)
                                {
                                    sto.Background = Brushes.Blue;
                                    SystemSounds.Asterisk.Play();
                                }
                            }
                            else
                            {
                                Log.Error("Greska prilikom konvertovanja stringa u broj za broj stola.");
                            }
                        }
                        else
                        {
                            Log.Error("String ne počinje sa 'S' za broj stola iz Drljine baze.");
                        }
                    });
                });
            }
        }
        #endregion Private methods
    }
}