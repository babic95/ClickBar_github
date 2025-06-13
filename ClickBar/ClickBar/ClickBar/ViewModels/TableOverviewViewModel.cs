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
using Microsoft.EntityFrameworkCore;
using ClickBar_Logging;
using System.Media;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Policy;
using ClickBar.Enums.Kuhinja;
using ClickBar_Common.Models.Invoice;
using DocumentFormat.OpenXml.InkML;
using ClickBar.PollingServeices;

namespace ClickBar.ViewModels
{
    public class TableOverviewViewModel : ViewModelBase
    {
        #region Fields
        private IServiceProvider _serviceProvider;

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
        public TableOverviewViewModel(IServiceProvider serviceProvider, 
            IDbContextFactory<SqlServerDbContext> dbContextFactory)
        {
            _serviceProvider = serviceProvider;
            DbContextFactory = dbContextFactory;
            SaleViewModel = _serviceProvider.GetRequiredService<SaleViewModel>();
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


            // Pokretanje polling metoda preko singletona
            //TablePollingService.Instance.StartPolling(CheckDatabase);
            TablePollingService.Instance.StartPollingStatusStolova(CheckDatabaseStatusStolovaAsync);
            SelectedDate = DateTime.Now;
        }
        #endregion Constructors

        #region Internal Properties
        internal IDbContextFactory<SqlServerDbContext> DbContextFactory { get; private set; }
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
        //public void SetOrder(Order order)
        //{
        //    PaymentPlace? paymentPlace = NormalPaymentPlaces.FirstOrDefault(pp => pp.Id == order.TableId);

        //    if (paymentPlace == null)
        //    {
        //        paymentPlace = RoundPaymentPlaces.FirstOrDefault(pp => pp.Id == order.TableId);
        //    }

        //    if (paymentPlace != null)
        //    {
        //        if (paymentPlace.Order != null && paymentPlace.Order.Items.Any())
        //        {
        //            order.Items.ToList().ForEach(item =>
        //            {
        //                var itemInPaymentPlace = paymentPlace.Order.Items.FirstOrDefault(i => i.Item.Id == item.Item.Id);
        //                if (itemInPaymentPlace != null)
        //                {
        //                    itemInPaymentPlace.TotalAmout += item.TotalAmout;
        //                    itemInPaymentPlace.Quantity += item.Quantity;
        //                }
        //                else
        //                {
        //                    paymentPlace.Order.Items.Add(item);
        //                }
        //            });
        //        }
        //        else
        //        {
        //            paymentPlace.Order = new Order(order.TableId, order.PartHall)
        //            {
        //                Items = new ObservableCollection<ItemInvoice>(order.Items),
        //                Cashier = order.Cashier
        //            };
        //            paymentPlace.Total = SaleViewModel.TotalAmount;
        //            paymentPlace.Background = Brushes.Red;
        //        }

        //        SaleViewModel.TableId = 0;
        //        SaleViewModel.ItemsInvoice = new ObservableCollection<ItemInvoice>();
        //        SaleViewModel.TotalAmount = 0;
        //    }
        //}
        #endregion Public methods

        #region Internal Methods
        internal async void SetKuhinjaNarudzbine()
        {
            if (DbContextFactory != null)
            {
                using (var dbContext = DbContextFactory.CreateDbContext())
                {
                    Narudzbe = new ObservableCollection<Narudzbe>();

                    var allNarudzbe = dbContext.OrdersToday.Include(o => o.OrderTodayItems).ThenInclude(o => o.Item).AsNoTracking()
                        .Where(o => o.OrderDateTime.Date == SelectedDate.Date &&
                        !string.IsNullOrEmpty(o.Name) &&
                        (o.Name.ToLower().Contains("k") ||
                        o.Name.ToLower().Contains("h")));

                    if (allNarudzbe.Any())
                    {
                        TotalNarudzbeSmena1 = 0;
                        TotalNarudzbeSmena2 = 0;
                        TotalNarudzbeDostava1 = 0;
                        TotalNarudzbeDostava2 = 0;
                        TotalNarudzbe = 0;

                        foreach (var n in allNarudzbe)
                        {
                            var narudzba = Narudzbe.FirstOrDefault(na => na.Id == n.Id);
                            if (narudzba == null)
                            {
                                narudzba = new Narudzbe()
                                {
                                    Id = n.Id,
                                    BrojNarudzbe = n.CounterType,
                                    RadnikId = n.CashierId,
                                    Smena = n.CashierId == "1111" || n.CashierId == "3333" ? 1 : 2,
                                    StoName = n.TableId.ToString(),
                                    VremeNarudzbe = n.OrderDateTime,
                                    Stavke = new ObservableCollection<StavkaNarudzbe>()
                                };
                                Narudzbe.Add(narudzba);
                            }

                            foreach (var s in n.OrderTodayItems)
                            {
                                try
                                {
                                    StavkaNarudzbe stavkaNarudzbe = new StavkaNarudzbe()
                                    {
                                        IdItem = s.ItemId,
                                        IdNarudzbe = narudzba.Id,
                                        Kolicina = s.Quantity,
                                        Mpc = Decimal.Round(s.TotalPrice / s.Quantity, 2),
                                        Naziv = s.Item.Name,
                                        StornoKolicina = s.StornoQuantity,
                                        Ukupno = Decimal.Round((s.Quantity - s.StornoQuantity) * (s.TotalPrice / s.Quantity), 2)
                                    };

                                    narudzba.Stavke.Add(stavkaNarudzbe);

                                    if (s.StornoQuantity > 0)
                                    {
                                        narudzba.Storno = "IMA STORNO";
                                    }
                                    else
                                    {
                                        narudzba.Storno = "NEMA STORNO";
                                    }

                                    if (narudzba.RadnikId == "1111")
                                    {
                                        TotalNarudzbeSmena1 += stavkaNarudzbe.Ukupno;
                                    }
                                    else if (narudzba.RadnikId == "2222")
                                    {
                                        TotalNarudzbeSmena2 += stavkaNarudzbe.Ukupno;
                                    }
                                    else if (narudzba.RadnikId == "3333")
                                    {
                                        TotalNarudzbeDostava1 += stavkaNarudzbe.Ukupno;
                                    }
                                    else if (narudzba.RadnikId == "4444")
                                    {
                                        TotalNarudzbeDostava2 += stavkaNarudzbe.Ukupno;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Log.Error("SetKuhinjaNarudzbine -> Error processing order items: ", ex);
                                }
                            }
                            TotalNarudzbe = TotalNarudzbeSmena1 +
                                TotalNarudzbeSmena2 +
                                TotalNarudzbeDostava1 +
                                TotalNarudzbeDostava2;

                            Narudzbe = new ObservableCollection<Narudzbe>(Narudzbe.OrderByDescending(n => n.BrojNarudzbe));
                        }
                    }
                }
            }
        }
        internal async Task CheckDatabaseStatusStolovaAsync(object sender, ElapsedEventArgs e)
        {
            try
            {
                using (var dbContext = DbContextFactory.CreateDbContext())
                {
                    var paymentPlaces = await dbContext.PaymentPlaces.AsNoTracking().ToListAsync();

                    foreach (var paymentPlace in paymentPlaces)
                    {
                        var existingPaymentPlace = AllNormalPaymentPlaces.FirstOrDefault(p => p.Id == paymentPlace.Id) ??
                                                   AllRoundPaymentPlaces.FirstOrDefault(p => p.Id == paymentPlace.Id);

                        if (existingPaymentPlace != null)
                        {
                            var unprocessedOrders = await dbContext.UnprocessedOrders
                                .Include(u => u.Cashier)
                                .AsNoTracking()
                                .FirstOrDefaultAsync(order => order.PaymentPlaceId == paymentPlace.Id);

                            // Obrada statusa stolova
                            if (unprocessedOrders != null)
                            {
                                var ordersOld = await dbContext.OrdersToday
                                    .Include(o => o.OrderTodayItems)
                                    .AsNoTracking()
                                    .Where(o => o.TableId == paymentPlace.Id &&
                                                o.UnprocessedOrderId == unprocessedOrders.Id &&
                                                o.Faza != (int)FazaKuhinjeEnumeration.Naplacena &&
                                                o.Faza != (int)FazaKuhinjeEnumeration.Obrisana)
                                    .ToListAsync();

                                decimal total = ordersOld.Sum(o => o.OrderTodayItems
                                    .Where(oti => oti.Quantity != 0 && oti.Quantity - oti.StornoQuantity - oti.NaplacenoQuantity > 0)
                                    .Sum(oti => Decimal.Round((oti.Quantity - oti.StornoQuantity - oti.NaplacenoQuantity) *
                                                               (oti.TotalPrice / oti.Quantity), 2)));

                                await Application.Current.Dispatcher.InvokeAsync(() =>
                                {
                                    if (total != 0 || ordersOld.Any())
                                    {
                                        existingPaymentPlace.Order = new Order(unprocessedOrders.Cashier);
                                        existingPaymentPlace.Order.TableId = paymentPlace.Id;
                                        existingPaymentPlace.Order.PartHall = paymentPlace.PartHallId;
                                        existingPaymentPlace.Background = Brushes.Red;
                                        existingPaymentPlace.Total = total;
                                    }
                                    else
                                    {
                                        existingPaymentPlace.Order = new Order(paymentPlace.Id, paymentPlace.PartHallId);
                                        existingPaymentPlace.Background = Brushes.Green;
                                        existingPaymentPlace.Total = 0;
                                    }
                                });
                            }
                            else
                            {
                                await Application.Current.Dispatcher.InvokeAsync(() =>
                                {
                                    existingPaymentPlace.Order = new Order(paymentPlace.Id, paymentPlace.PartHallId);
                                    existingPaymentPlace.Background = Brushes.Green;
                                    existingPaymentPlace.Total = 0;
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Loguj grešku ako je potrebno
                Log.Error("TableOverviewViewModel -> CheckDatabaseStatusStolovaAsync -> Greška prilikom provere statusa stolova: ", ex);
                Console.WriteLine($"Greška u CheckDatabaseStatusStolovaAsync: {ex.Message}");
            }
        }

        //internal void CheckDatabaseStatusStolova(object sender, ElapsedEventArgs e)
        //{
        //    Task.Run(() =>
        //    {
        //        using (var dbContext = DbContextFactory.CreateDbContext())
        //        {
        //            var updatedPaymentPlaces = dbContext.PaymentPlaces.AsNoTracking().ToList();
        //            foreach (var paymentPlace in updatedPaymentPlaces)
        //            {
        //                var existingPaymentPlace = AllNormalPaymentPlaces.FirstOrDefault(p => p.Id == paymentPlace.Id) ??
        //                                           AllRoundPaymentPlaces.FirstOrDefault(p => p.Id == paymentPlace.Id);

        //                if (existingPaymentPlace != null)
        //                {
        //                    // Ažuriraj status stolova
        //                    var unprocessedOrders = dbContext.UnprocessedOrders.Include(u => u.Cashier).AsNoTracking()
        //                    .FirstOrDefault(order => order.PaymentPlaceId == paymentPlace.Id);
        //                    if (unprocessedOrders != null)
        //                    {
        //                        var ordersOld = dbContext.OrdersToday.Include(o => o.OrderTodayItems).AsNoTracking()
        //                            .Where(o => o.TableId == paymentPlace.Id &&
        //                            o.UnprocessedOrderId == unprocessedOrders.Id &&
        //                            o.Faza != (int)FazaKuhinjeEnumeration.Naplacena &&
        //                            o.Faza != (int)FazaKuhinjeEnumeration.Obrisana);

        //                        decimal total = Decimal.Round(ordersOld
        //                            .Sum(o => o.OrderTodayItems.Where(oti => oti.Quantity != 0 && oti.Quantity - oti.StornoQuantity - oti.NaplacenoQuantity > 0).Sum(oti => Decimal.Round((oti.Quantity - oti.StornoQuantity - oti.NaplacenoQuantity) * (oti.TotalPrice / oti.Quantity), 2))), 2);

        //                        Application.Current.Dispatcher.Invoke(() =>
        //                        {
        //                            if (total != 0 || ordersOld.Any())
        //                            {
        //                                existingPaymentPlace.Order = new Order(unprocessedOrders.Cashier);
        //                                existingPaymentPlace.Order.TableId = paymentPlace.Id;
        //                                existingPaymentPlace.Order.PartHall = paymentPlace.PartHallId;
        //                                existingPaymentPlace.Background = Brushes.Red;
        //                                existingPaymentPlace.Total = total;
        //                            }
        //                            else
        //                            {
        //                                existingPaymentPlace.Order = new Order(paymentPlace.Id, paymentPlace.PartHallId);
        //                                existingPaymentPlace.Background = Brushes.Green;
        //                                existingPaymentPlace.Total = 0;
        //                            }
        //                        });
        //                    }
        //                    else
        //                    {
        //                        Application.Current.Dispatcher.Invoke(() =>
        //                        {
        //                            existingPaymentPlace.Order = new Order(paymentPlace.Id, paymentPlace.PartHallId);
        //                            existingPaymentPlace.Background = Brushes.Green;
        //                            existingPaymentPlace.Total = 0;
        //                        });
        //                    }
        //                }
        //            }
        //        }
        //    });
        //}
        internal async Task UpdateTableNoTask(int tableId)
        {
            using (var dbContext = DbContextFactory.CreateDbContext())
            {
                var updatedPaymentPlace = await dbContext.PaymentPlaces.AsNoTracking().FirstOrDefaultAsync(p => p.Id == tableId);

                if (updatedPaymentPlace != null)
                {
                    var existingPaymentPlace = AllNormalPaymentPlaces.FirstOrDefault(p => p.Id == updatedPaymentPlace.Id) ??
                                               AllRoundPaymentPlaces.FirstOrDefault(p => p.Id == updatedPaymentPlace.Id);

                    if (existingPaymentPlace != null)
                    {
                        // Ažuriraj status stolova
                        var unprocessedOrders = dbContext.UnprocessedOrders.Include(u => u.Cashier).AsNoTracking()
                        .FirstOrDefault(order => order.PaymentPlaceId == updatedPaymentPlace.Id);
                        if (unprocessedOrders != null)
                        {
                            var ordersOld = dbContext.OrdersToday.Include(o => o.OrderTodayItems).AsNoTracking()
                                .Where(o => o.TableId == updatedPaymentPlace.Id &&
                                o.UnprocessedOrderId == unprocessedOrders.Id &&
                                o.Faza != (int)FazaKuhinjeEnumeration.Naplacena &&
                                o.Faza != (int)FazaKuhinjeEnumeration.Obrisana);

                            decimal total = Decimal.Round(ordersOld
                                .Sum(o => o.OrderTodayItems.Where(oti => oti.Quantity != 0 && oti.Quantity - oti.StornoQuantity - oti.NaplacenoQuantity > 0).Sum(oti => Decimal.Round((oti.Quantity - oti.StornoQuantity - oti.NaplacenoQuantity) * (oti.TotalPrice / oti.Quantity), 2))), 2);

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                if (total != 0 || ordersOld.Any())
                                {
                                    existingPaymentPlace.Order = new Order(unprocessedOrders.Cashier);
                                    existingPaymentPlace.Order.TableId = updatedPaymentPlace.Id;
                                    existingPaymentPlace.Order.PartHall = updatedPaymentPlace.PartHallId;
                                    existingPaymentPlace.Background = Brushes.Red;
                                    existingPaymentPlace.Total = total;
                                }
                                else
                                {
                                    existingPaymentPlace.Order = new Order(updatedPaymentPlace.Id, updatedPaymentPlace.PartHallId);
                                    existingPaymentPlace.Background = Brushes.Green;
                                    existingPaymentPlace.Total = 0;
                                }
                            });
                        }
                        else
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                existingPaymentPlace.Order = new Order(updatedPaymentPlace.Id, updatedPaymentPlace.PartHallId);
                                existingPaymentPlace.Background = Brushes.Green;
                                existingPaymentPlace.Total = 0;
                            });
                        }
                    }
                }
            }
        }
        internal void CheckDatabaseStatusStolovaNoTask()
        {
            using (var dbContext = DbContextFactory.CreateDbContext())
            {
                var updatedPaymentPlaces = dbContext.PaymentPlaces.AsNoTracking().ToList();
                foreach (var paymentPlace in updatedPaymentPlaces)
                {
                    var existingPaymentPlace = AllNormalPaymentPlaces.FirstOrDefault(p => p.Id == paymentPlace.Id) ??
                                               AllRoundPaymentPlaces.FirstOrDefault(p => p.Id == paymentPlace.Id);

                    if (existingPaymentPlace != null)
                    {
                        // Ažuriraj status stolova
                        var unprocessedOrders = dbContext.UnprocessedOrders.Include(u => u.Cashier).AsNoTracking()
                        .FirstOrDefault(order => order.PaymentPlaceId == paymentPlace.Id);
                        if (unprocessedOrders != null)
                        {
                            var ordersOld = dbContext.OrdersToday.Include(o => o.OrderTodayItems).AsNoTracking()
                                .Where(o => o.TableId == paymentPlace.Id &&
                                o.UnprocessedOrderId == unprocessedOrders.Id &&
                                o.Faza != (int)FazaKuhinjeEnumeration.Naplacena &&
                                o.Faza != (int)FazaKuhinjeEnumeration.Obrisana);

                            decimal total = Decimal.Round(ordersOld
                                .Sum(o => o.OrderTodayItems.Where(oti => oti.Quantity != 0 && oti.Quantity - oti.StornoQuantity - oti.NaplacenoQuantity > 0).Sum(oti => Decimal.Round((oti.Quantity - oti.StornoQuantity - oti.NaplacenoQuantity) * (oti.TotalPrice / oti.Quantity), 2))), 2);

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                if (total != 0 || ordersOld.Any())
                                {
                                    existingPaymentPlace.Order = new Order(unprocessedOrders.Cashier);
                                    existingPaymentPlace.Order.TableId = paymentPlace.Id;
                                    existingPaymentPlace.Order.PartHall = paymentPlace.PartHallId;
                                    existingPaymentPlace.Background = Brushes.Red;
                                    existingPaymentPlace.Total = total;
                                }
                                else
                                {
                                    existingPaymentPlace.Order = new Order(paymentPlace.Id, paymentPlace.PartHallId);
                                    existingPaymentPlace.Background = Brushes.Green;
                                    existingPaymentPlace.Total = 0;
                                }
                            });
                        }
                        else
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                existingPaymentPlace.Order = new Order(paymentPlace.Id, paymentPlace.PartHallId);
                                existingPaymentPlace.Background = Brushes.Green;
                                existingPaymentPlace.Total = 0;
                            });
                        }
                    }
                }
            }
        }
        #endregion Internal Methods

        #region Private methods
        private void LoadingDB()
        {
            using (var dbContext = DbContextFactory.CreateDbContext())
            {
                //var remove0Value = dbContext.UnprocessedOrders
                //    .GroupJoin(
                //        dbContext.ItemsInUnprocessedOrder,
                //        order => order.Id,
                //        item => item.UnprocessedOrderId,
                //        (order, items) => new { Order = order, Items = items }
                //    )
                //    .Where(x => !x.Items.Any())
                //    .Select(x => x.Order);

                //if (remove0Value.Any())
                //{
                //    dbContext.UnprocessedOrders.RemoveRange(remove0Value);
                //    dbContext.SaveChanges();
                //}

                foreach (var part in dbContext.PartHalls)
                {
                    PartHall partHall = new PartHall()
                    {
                        Id = part.Id,
                        Name = part.Name,
                        Image = part.Image
                    };

                    Rooms.Add(partHall);
                }

                foreach (var payment in dbContext.PaymentPlaces)
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

                    var unprocessedOrders = dbContext.UnprocessedOrders.Include(u => u.Cashier).AsNoTracking()
                        .FirstOrDefault(order => order.PaymentPlaceId == payment.Id);

                    if (unprocessedOrders != null)
                    {
                        var orersInTable = dbContext.OrdersToday.Include(o => o.OrderTodayItems).AsNoTracking()
                            .Where(o => o.TableId == paymentPlace.Id &&
                            o.UnprocessedOrderId == unprocessedOrders.Id &&
                            o.Faza != (int)FazaKuhinjeEnumeration.Naplacena &&
                            o.Faza != (int)FazaKuhinjeEnumeration.Obrisana).ToList();

                        decimal total = Decimal.Round(orersInTable.Sum(o => o.OrderTodayItems.Where(oti => oti.Quantity != 0 && oti.Quantity - oti.StornoQuantity - oti.NaplacenoQuantity > 0).Sum(oti => Decimal.Round((oti.Quantity - oti.StornoQuantity - oti.NaplacenoQuantity) * (oti.TotalPrice / oti.Quantity), 2))), 2);

                        if (total != 0 || orersInTable.Any())
                        {
                            paymentPlace.Order = new Order(unprocessedOrders.Cashier);
                            paymentPlace.Order.TableId = payment.Id;
                            paymentPlace.Order.PartHall = payment.PartHallId;
                            paymentPlace.Background = Brushes.Red;
                            paymentPlace.Total = total;
                        }
                        else
                        {
                            paymentPlace.Order = new Order(payment.Id, payment.PartHallId);
                            paymentPlace.Background = Brushes.Green;
                            paymentPlace.Total = 0;
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
                }

                if (CurrentPartHall != null)
                {
                    Title = CurrentPartHall.Name;
                }
            }
        }
        private void CheckDatabase(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (DbContextFactory != null)
                {
                    using (var dbContext = DbContextFactory.CreateDbContext())
                    {
                        var noveNarudzbe = dbContext.OrdersToday.AsNoTracking()
                        .Where(n => n.Faza == 2 && n.OrderDateTime.Date == DateTime.Now.Date);

                        if (noveNarudzbe != null && noveNarudzbe.Any())
                        {
                            Application.Current.Dispatcher.Invoke(async () =>
                            {
                                foreach (var n in noveNarudzbe)
                                {
                                    var sto = AllNormalPaymentPlaces.FirstOrDefault(p => p.Id == n.TableId)
                                        ?? AllRoundPaymentPlaces.FirstOrDefault(p => p.Id == n.TableId);

                                    if (sto != null && sto.Background != Brushes.Blue)
                                    {
                                        sto.Background = Brushes.Blue;
                                        //SystemSounds.Asterisk.Play();
                                    }
                                }
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("TableOverviewViewModel -> CheckDatabase -> Error checking database: ", ex);
            }
        }
        #endregion Private methods
    }
}