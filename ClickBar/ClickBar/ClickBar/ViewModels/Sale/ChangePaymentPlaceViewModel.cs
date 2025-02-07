using ClickBar.Commands.AppMain.Admin;
using ClickBar.Commands.Sale.Pay.SplitOrder.ChangePaymentPlace;
using ClickBar.Commands.TableOverview;
using ClickBar.Enums.AppMain.Admin;
using ClickBar.Models.Sale;
using ClickBar.Models.TableOverview;
using ClickBar_Database;
using ClickBar_Database.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace ClickBar.ViewModels.Sale
{
    public class ChangePaymentPlaceViewModel : ViewModelBase
    {
        #region Fields
        private ObservableCollection<PartHall> _rooms;
        private ObservableCollection<PaymentPlace> _allNormalPaymentPlaces;
        private ObservableCollection<PaymentPlace> _normalPaymentPlaces;

        private ObservableCollection<PaymentPlace> _allRoundPaymentPlaces;
        private ObservableCollection<PaymentPlace> _roundPaymentPlaces;
        private PartHall? _currentPartHall;
        private string _title;
        #endregion Fields

        #region Constructors
        public ChangePaymentPlaceViewModel(SplitOrderViewModel splitOrderViewModel)
        {
            SplitOrderViewModel = splitOrderViewModel;

            Rooms = new ObservableCollection<PartHall>();
            AllNormalPaymentPlaces = new ObservableCollection<PaymentPlace>();
            NormalPaymentPlaces = new ObservableCollection<PaymentPlace>();
            AllRoundPaymentPlaces = new ObservableCollection<PaymentPlace>();
            RoundPaymentPlaces = new ObservableCollection<PaymentPlace>();
            LoadingDB();
        }
        #endregion Constructors

        #region Properties
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
                    }
                }
            }
        }
        #endregion Properties

        #region Internal Properties
        internal SplitOrderViewModel SplitOrderViewModel { get; set; }
        internal Order? Order { get; set; }
        #endregion Internal Properties

        #region Commands
        public ICommand SelectRoomChangePaymentPlaceCommand => new SelectRoomChangePaymentPlaceCommand(this);
        public ICommand CancelChangePaymentPlaceCommand => new CancelChangePaymentPlaceCommand(this);
        public ICommand ClickOnPaymentPlaceChangePaymentPlaceCommand => new ClickOnPaymentPlaceChangePaymentPlaceCommand(this);
        #endregion Commands

        #region Public methods
        #endregion Public methods

        #region Private methods
        private void LoadingDB()
        {
            using (SqliteDbContext sqliteDbContext = new SqliteDbContext())
            {

                sqliteDbContext.PartHalls.ToList().ForEach(part =>
                {
                    PartHall partHall = new PartHall()
                    {
                        Id = part.Id,
                        Name = part.Name,
                        Image = part.Image
                    };

                    Rooms.Add(partHall);
                });

                sqliteDbContext.PaymentPlaces.ToList().ForEach(payment =>
                {
                    PaymentPlace paymentPlace = new PaymentPlace()
                    {
                        Id = payment.Id,
                        PartHallId = payment.PartHallId,
                        Left = payment.LeftCanvas.Value,
                        Top = payment.TopCanvas.Value,
                        Type = payment.Type.HasValue ? (PaymentPlaceTypeEnumeration)payment.Type.Value : PaymentPlaceTypeEnumeration.Normal
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

                    var unprocessedOrders = sqliteDbContext.UnprocessedOrders.FirstOrDefault(order => order.PaymentPlaceId == payment.Id);

                    if (unprocessedOrders != null)
                    {
                        CashierDB? cashierDB = sqliteDbContext.Cashiers.Find(unprocessedOrders.CashierId);
                        var itemsInUnprocessedOrder = sqliteDbContext.Items.Join(sqliteDbContext.ItemsInUnprocessedOrder,
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
            }
        }
        #endregion Private methods
    }
}
