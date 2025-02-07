using ClickBar.Commands.AppMain.Admin;
using ClickBar.Commands.AppMain.Admin.Rooms;
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
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ClickBar.ViewModels.AppMain
{
    public class AdminViewModel : ViewModelBase
    {
        #region Fields
        private CashierDB _loggedCashier;

        private ObservableCollection<PartHall> _rooms;
        private ObservableCollection<PaymentPlace> _allNormalPaymentPlaces;
        private ObservableCollection<PaymentPlace> _normalPaymentPlaces;

        private ObservableCollection<PaymentPlace> _allRoundPaymentPlaces;
        private ObservableCollection<PaymentPlace> _roundPaymentPlaces;

        private PartHall? _currentPartHall;
        private PartHall? _newRoom;

        private PaymentPlace? _newPaymentPlace;
        private bool _change;

        private Visibility _roundPaymentPlace;
        private Visibility _normalPaymentPlace;

        private bool _isCheckedRoundPaymentPlace;
        #endregion Fields

        #region Constructors
        public AdminViewModel(CashierDB loggedCashier)
        {
            IsCheckedRoundPaymentPlace = false;
            Change = false;
            _loggedCashier = loggedCashier;

            CurrentPartHall = null;
            Rooms = new ObservableCollection<PartHall>();
            AllNormalPaymentPlaces = new ObservableCollection<PaymentPlace>();
            NormalPaymentPlaces = new ObservableCollection<PaymentPlace>();
            AllRoundPaymentPlaces = new ObservableCollection<PaymentPlace>();
            RoundPaymentPlaces = new ObservableCollection<PaymentPlace>();
            SetDefaultValueFromDB();
        }
        #endregion Constructors

        #region Properties
        public Window? AddNewPaymentPlaceWindow { get; set; }
        public Window? AddNewRoomWindow { get; set; }

        public bool IsCheckedRoundPaymentPlace
        {
            get { return _isCheckedRoundPaymentPlace; }
            set
            {
                _isCheckedRoundPaymentPlace = value;
                OnPropertyChange(nameof(IsCheckedRoundPaymentPlace));

                if (value)
                {
                    RoundPaymentPlace = Visibility.Visible;
                }
                else
                {
                    RoundPaymentPlace = Visibility.Hidden;
                }
            }
        }
        public Visibility RoundPaymentPlace
        {
            get { return _roundPaymentPlace; }
            set
            {
                _roundPaymentPlace = value;
                OnPropertyChange(nameof(RoundPaymentPlace));

                if (value == Visibility.Visible)
                {
                    NormalPaymentPlace = Visibility.Hidden;
                }
                else
                {
                    NormalPaymentPlace = Visibility.Visible;
                }
            }
        }
        public Visibility NormalPaymentPlace
        {
            get { return _normalPaymentPlace; }
            set
            {
                _normalPaymentPlace = value;
                OnPropertyChange(nameof(NormalPaymentPlace));
            }
        }
        public PaymentPlace? NewPaymentPlace
        {
            get { return _newPaymentPlace; }
            set
            {
                if (_newPaymentPlace != value)
                {
                    _newPaymentPlace = value;
                    OnPropertyChange(nameof(NewPaymentPlace));
                }
            }
        }
        public bool Change
        {
            get { return _change; }
            set
            {
                if (_change != value)
                {
                    _change = value;
                    OnPropertyChange(nameof(Change));
                }
            }
        }
        public PartHall? CurrentPartHall
        {
            get { return _currentPartHall; }
            set
            {
                if (_currentPartHall != value)
                {
                    SaveCommand.Execute(null);

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
        public PartHall? NewRoom
        {
            get { return _newRoom; }
            set
            {
                _newRoom = value;
                OnPropertyChange(nameof(NewRoom));
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
        #endregion Properties

        #region Commands
        public ICommand SelectRoomCommand => new SelectRoomCommand(this);
        public ICommand SaveCommand => new SaveCommand(this);
        public ICommand DeletePaymentPlaceCommand => new DeletePaymentPlaceCommand(this);
        public ICommand EditPaymentPlaceCommand => new EditPaymentPlaceCommand(this);
        public ICommand AddNewPaymentPlaceCommand => new AddNewPaymentPlaceCommand(this);
        public ICommand OpenWindowAddNewPaymentPlaceCommand => new OpenWindowAddNewPaymentPlaceCommand(this);
        public ICommand OpenWindowAddNewRoomCommand => new OpenWindowAddNewRoomCommand(this);
        public ICommand EditRoomCommand => new EditRoomCommand(this);
        public ICommand DeleteRoomCommand => new DeleteRoomCommand(this);
        public ICommand SelectImageForRoomCommand => new SelectImageForRoomCommand(this);
        public ICommand SaveRoomCommand => new SaveRoomCommand(this);
        #endregion Commands

        #region Public methods
        #endregion Public methods

        #region Private methods
        private void SetDefaultValueFromDB()
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
                        Type = payment.Type.HasValue ? (PaymentPlaceTypeEnumeration)payment.Type.Value : PaymentPlaceTypeEnumeration.Normal,
                        Popust = payment.Popust,
                        Name = !string.IsNullOrEmpty(payment.Name) ? payment.Name : payment.Id.ToString()
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
            #endregion Private methods
        }
    }
}
