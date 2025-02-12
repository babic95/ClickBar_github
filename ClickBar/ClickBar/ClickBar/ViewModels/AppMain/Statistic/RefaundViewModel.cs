using ClickBar.Commands.AppMain;
using ClickBar.Commands.AppMain.Statistic.Refaund;
using ClickBar.Commands.Sale.Pay;
using ClickBar.Enums.Sale;
using ClickBar.Models.Sale;
using ClickBar.ViewModels.Sale;
using ClickBar_Common.Models.Invoice;
using ClickBar_Common.Models.Invoice.FileSystemWatcher;
using ClickBar_DatabaseSQLManager;
using ClickBar_DatabaseSQLManager.Models;
using ClickBar_Logging;
using ClickBar_Printer;
using ClickBar_Settings;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Vml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ClickBar.ViewModels.AppMain.Statistic
{
    //public enum FocusRefaundEnumeration
    //{
    //    Ref1 = 0,
    //    Ref2 = 1,
    //    RefNumber = 2,
    //    RefDateDay = 3,
    //    RefDateMonth = 4,
    //    RefDateYear = 5,
    //    RefDateHour = 6,
    //    RefDateMinute = 7,
    //    RefDateSecond = 8,
    //    Refaund = 9
    //}
    public class RefaundViewModel : ViewModelBase
    {
        #region Fields
        private IServiceProvider _serviceProvider;

        private const int ERROR_SHARING_VIOLATION = 32;
        private const int ERROR_LOCK_VIOLATION = 33;

        private DateTime _timer;

        //private FocusRefaundEnumeration _focus;
        private ObservableCollection<Invoice> _searchInvoices;

        private DateTime _selectedDateForRefund;

        private Invoice _currentInvoice;
        private InvoiceTypeEnumeration _invoiceType;
        private string _refNumber;
        private string _refDateDay;
        private string _refDateMonth;
        private string _refDateYear;
        private string _refDateHour;
        private string _refDateMinute;
        private string _refDateSecond;
        #endregion Fields

        #region Constructors
        public RefaundViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            DbContext = _serviceProvider.GetRequiredService<IDbContextFactory<SqlServerDbContext>>().CreateDbContext();
            LoggedCashier = serviceProvider.GetRequiredService<CashierDB>();

            Initialize();
        }
        #endregion Constructors

        #region Properties Internal
        internal SqlServerDbContext DbContext
        {
            get; private set;
        }
        internal InvoiceResult CurrentInvoiceResult { get; set; }
        //internal InvoiceRequest CurrentInvoiceRequest { get; set; }
        internal InvoceRequestFileSystemWatcher CurrentInvoiceRequest { get; set; }
        internal CashierDB LoggedCashier { get; set; }
        internal List<Invoice> AllInvoicesInDate { get; set; }
        #endregion Properties Internal

        #region Properties
        public Invoice CurrentInvoice
        {
            get { return _currentInvoice; }
            set
            {
                if (_currentInvoice != value || value == null)
                {
                    _currentInvoice = value;
                    OnPropertyChange(nameof(CurrentInvoice));

                    RefNumber = value != null ? value.InvoiceNumber.ToString() : string.Empty;

                    RefDateDay = value != null ? value.SdcDateTime.Day.ToString() : string.Empty;
                    RefDateMonth = value != null ? value.SdcDateTime.Month.ToString() : string.Empty;
                    RefDateYear = value != null ? value.SdcDateTime.Year.ToString() : string.Empty;
                    RefDateHour = value != null ? value.SdcDateTime.Hour.ToString() : string.Empty;
                    RefDateMinute = value != null ? value.SdcDateTime.Minute.ToString() : string.Empty;
                    RefDateSecond = value != null ? value.SdcDateTime.Second.ToString() : string.Empty;

                    //if (value != null)
                    //{
                    //    Focus = FocusRefaundEnumeration.Refaund;
                    //}
                    //else
                    //{
                    //    Focus = FocusRefaundEnumeration.Ref1;
                    //}
                }
            }
        }
        public ObservableCollection<Invoice> SearchInvoices
        {
            get { return _searchInvoices; }
            set
            {
                _searchInvoices = value;
                OnPropertyChange(nameof(SearchInvoices));
            }
        }
        public DateTime SelectedDateForRefund
        {
            get { return _selectedDateForRefund; }
            set
            {
                _selectedDateForRefund = value;
                OnPropertyChange(nameof(SelectedDateForRefund));
            }
        }
        //public FocusRefaundEnumeration Focus
        //{
        //    get { return _focus; }
        //    set
        //    {
        //        _focus = value;
        //        OnPropertyChange(nameof(Focus));
        //    }
        //}
        public InvoiceTypeEnumeration InvoiceType
        {
            get { return _invoiceType; }
            set
            {
                _invoiceType = value;
                OnPropertyChange(nameof(InvoiceType));

                var searchInvoice = AllInvoicesInDate.Where(invoice => invoice.InvoiceType == value).ToList();
                if (searchInvoice is not null)
                {
                    SearchInvoices = new ObservableCollection<Invoice>(searchInvoice);
                }
                else
                {
                    SearchInvoices = new ObservableCollection<Invoice>();
                }
            }
        }

        public string RefNumber
        {
            get { return _refNumber; }
            set
            {
                try
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        _refNumber = value;
                        OnPropertyChange(nameof(RefNumber));
                        Search();
                    }
                    else
                    {
                        _refNumber = value;
                        OnPropertyChange(nameof(RefNumber));

                        Search();
                    }
                }
                catch
                {
                    MessageBox.Show("Poslednji parametar referentnog broja mora biti broj!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        public string RefDateDay
        {
            get { return _refDateDay; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _refDateDay = value;
                    OnPropertyChange(nameof(RefDateDay));
                    Search();
                }
                else
                {
                    try
                    {
                        int day = Convert.ToInt32(value);

                        if (day > 0 && day < 32)
                        {
                            _refDateDay = value;

                            Search();
                        }
                        OnPropertyChange(nameof(RefDateDay));
                    }
                    catch
                    {
                        MessageBox.Show("Dan mora biti broj!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        public string RefDateMonth
        {
            get { return _refDateMonth; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _refDateMonth = value;
                    OnPropertyChange(nameof(RefDateMonth));
                    Search();
                }
                else
                {
                    try
                    {
                        int month = Convert.ToInt32(value);

                        if (month > 0 && month < 13)
                        {
                            _refDateMonth = value;

                            Search();
                        }
                        OnPropertyChange(nameof(RefDateMonth));
                    }
                    catch
                    {
                        MessageBox.Show("Mesec mora biti broj!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        public string RefDateYear
        {
            get { return _refDateYear; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _refDateYear = value;
                    OnPropertyChange(nameof(RefDateYear));
                    Search();
                }
                else
                {
                    try
                    {
                        int year = Convert.ToInt32(value);

                        if (year > 0 && year < 9999)
                        {
                            _refDateYear = value;

                            Search();
                        }
                        OnPropertyChange(nameof(RefDateYear));
                    }
                    catch
                    {
                        MessageBox.Show("Godina mora biti broj!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        public string RefDateHour
        {
            get { return _refDateHour; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _refDateHour = value;
                    OnPropertyChange(nameof(RefDateHour));
                    Search();
                }
                else
                {
                    try
                    {
                        int hour = Convert.ToInt32(value);

                        if (hour >= 0 && hour < 25)
                        {
                            _refDateHour = value;

                            Search();
                        }
                        OnPropertyChange(nameof(RefDateHour));
                    }
                    catch
                    {
                        MessageBox.Show("Sat mora biti broj!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        public string RefDateMinute
        {
            get { return _refDateMinute; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _refDateMinute = value;
                    OnPropertyChange(nameof(RefDateMinute));
                    Search();
                }
                else
                {
                    try
                    {
                        int minute = Convert.ToInt32(value);

                        if (minute >= 0 && minute < 60)
                        {
                            _refDateMinute = value;

                            Search();
                        }
                        OnPropertyChange(nameof(RefDateMinute));
                    }
                    catch
                    {
                        MessageBox.Show("Dan mora biti broj!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        public string RefDateSecond
        {
            get { return _refDateSecond; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _refDateSecond = value;
                    OnPropertyChange(nameof(RefDateSecond));
                    Search();
                }
                else
                {
                    try
                    {
                        int second = Convert.ToInt32(value);

                        if (second >= 0 && second < 60)
                        {
                            _refDateSecond = value;

                            Search();
                        }
                        OnPropertyChange(nameof(RefDateSecond));
                    }
                    catch
                    {
                        MessageBox.Show("Dan mora biti broj!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        #endregion Properties

        #region Commands
        //public ICommand ClickOnNumberButtonCommand => new ClickOnNumberButtonCommand(this);
        //public ICommand ChangeFocusCommand => new ChangeFocusCommand(this);
        public ICommand CancelCurrentRefaundInvoiceCommand => new CancelCurrentRefaundInvoiceCommand(this);
        public ICommand RefaundCommand => new RefaundCommand(_serviceProvider, this);
        public ICommand EfakturaCommand => new EfakturaCommand(this);
        public ICommand ShowInvoiceCommand => new ShowInvoiceCommand(this);
        //public ICommand RefundPerItemCommand => new RefundPerItemCommand(this);
        public ICommand SearchRefaundInvoiceCommand => new SearchRefaundInvoiceCommand(this);
        #endregion Commands

        #region Public methods
        public async void FinisedRefaund(bool eFaktura)
        {
            try
            {
                string json = JsonConvert.SerializeObject(CurrentInvoiceRequest);

                string? inDirectory = SettingsManager.Instance.GetInDirectory();

                if (string.IsNullOrEmpty(inDirectory))
                {
                    MessageBox.Show("Putanja do ulaznog foldera nije setovana! Račun ne moze da se fiskalizuje.",
                        "Ulazni folder",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    return;
                }
                else
                {
                    string invoiceName = $"InvoiceRefaund_{DateTime.Now.ToString("dd-MM-yyyy HH-mm-ss")}.json";
                    string jsonPath = System.IO.Path.Combine(inDirectory, invoiceName);

                    File.WriteAllText(jsonPath, json);

                    Task.Run(() =>
                    {
                        string? outDirectory = SettingsManager.Instance.GetOutDirectory();
                        string jsonOutPath = System.IO.Path.Combine(outDirectory, invoiceName);

                        _timer = DateTime.Now;
                        while (IsFileLocked(jsonOutPath)) ;

                        try
                        {
                            using (StreamReader r = new StreamReader(jsonOutPath))
                            {
                                string response = r.ReadToEnd();

                                ResponseJson? responseJson = JsonConvert.DeserializeObject<ResponseJson>(response);

                                if (responseJson != null)
                                {
                                    if (responseJson.Message.Contains("Uspešna fiskalizacija"))
                                    {
                                        try
                                        {
                                            SaveToDB(CurrentInvoiceRequest, responseJson, eFaktura);

                                            Initialize();
                                        }
                                        catch
                                        {
                                            MessageBox.Show("GREŠKA PRILIKOM UPISA U BAZU!", "", MessageBoxButton.OK, MessageBoxImage.Error);
                                        }
                                    }
                                    else
                                    {
                                        MessageBox.Show("PROVERITE ESIR I LPFR!", "", MessageBoxButton.OK, MessageBoxImage.Error);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error("PayCommand -> FinisedSale -> ", ex);
                            MessageBox.Show("GREŠKA U PROVERI IZLAZNOG FAJLA!\nPROVERITE DA LI JE ESIR POKRENUT", "", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    });
                }


                //List<ItemInvoiceDB> itemsInvoice = await sqliteDbContext.GetAllItemsFromInvoice(CurrentInvoice.Id);

                //CurrentInvoiceRequest.Cashier = LoggedCashier.Name;

                //Guid guid = Guid.NewGuid();
                //sqliteDbContext.InsertInvoice(guid, CurrentInvoiceRequest, itemsInvoice);

                //if (SendInvoiceToESDC(guid, CurrentInvoiceRequest, payRefaundViewModel.RefaundCash).Result)
                //{
                //    ResetAll();
                //    InvoiceType = InvoiceTypeEnumeration.Promet;
                //}
                //else
                //{
                //    MessageBox.Show("Greška u komunikaciji sa L-PFR-om, proverite Vaš L-PFR!", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show("Greška u komunikaciji sa L-PFR-om, proverite Vaš L-PFR!", "", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        //public async Task<bool> SendInvoiceToESDC(Guid guid, ClickBar_Common.Models.InvoiceRequest invoiceRequest, bool refaundCash)
        //{
        //    var response = await ApiManager.Instance.CreateInvoice(guid, invoiceRequest);

        //    if (response is ESIR_Common.Models.InvoiceResult)
        //    {
        //        ESIR_Common.Models.InvoiceResult invoiceResult = (ESIR_Common.Models.InvoiceResult)response;

        //        try
        //        {
        //            PrinterManager.Instance.PrintJournal(invoiceResult, invoiceRequest);

        //            SqlServerDbContext sqliteDbContext = new SqlServerDbContext();
        //            sqliteDbContext.UpdateInvoice(guid, invoiceResult);


        //            if (invoiceRequest.InvoiceType == ESIR_Common.Enums.InvoiceTypeEenumeration.Normal &&
        //                refaundCash)
        //            {
        //                InvoiceRequest invoiceRequestCopy = CreateCopyInvoice(invoiceResult).Result;
        //                Guid guidCopy = Guid.NewGuid();
        //                var responseCopy = await ApiManager.Instance.CreateInvoice(guidCopy, invoiceRequestCopy);

        //                if (responseCopy is ESIR_Common.Models.InvoiceResult)
        //                {
        //                    ESIR_Common.Models.InvoiceResult invoiceResultCopy = (ESIR_Common.Models.InvoiceResult)responseCopy;
        //                    try
        //                    {
        //                        PrinterManager.Instance.PrintJournal(invoiceResultCopy, invoiceRequestCopy);
        //                    }
        //                    catch
        //                    {
        //                        MessageBox.Show("Greška u komunikaciji sa štampačem, račun kopije je uspešno potpisan ali ne može da se otštampa. Proverite Vaš štampač!",
        //                            "Greška štampača", MessageBoxButton.OK, MessageBoxImage.Error);
        //                    }
        //                }
        //                else if (responseCopy is ESIR_Common.Models.ModelErrors)
        //                {
        //                    ESIR_Common.Models.ModelErrors modelErrors = (ESIR_Common.Models.ModelErrors)responseCopy;

        //                    var errors = modelErrors.ModelState.Where(modelState => modelState.Errors.Where(error => error.Contains("1500")).Any());

        //                    if (errors is not null && errors.Any())
        //                    {
        //                        PinWindow pinWindow = new PinWindow();
        //                        pinWindow.ShowDialog();

        //                        return await SendInvoiceToESDC(guid, invoiceRequestCopy, refaundCash);
        //                    }
        //                    else
        //                    {
        //                        Errors er = new Errors(modelErrors);
        //                        return false;
        //                    }
        //                }
        //            }
        //            return true;
        //        }
        //        catch
        //        {
        //            MessageBox.Show("Greška u komunikaciji sa štampačem, račun je uspešno potpisan ali ne može da se otštampa. Proverite Vaš štampač!",
        //                "Greška štampača", MessageBoxButton.OK, MessageBoxImage.Error);
        //        }
        //    }
        //    else if (response is ESIR_Common.Models.ModelErrors)
        //    {
        //        ClickBar_Common.Models.ModelErrors modelErrors = (ESIR_Common.Models.ModelErrors)response;

        //        var errors = modelErrors.ModelState.Where(modelState => modelState.Errors.Where(error => error.Contains("1500")).Any());

        //        if (errors is not null && errors.Any())
        //        {
        //            PinWindow pinWindow = new PinWindow();
        //            pinWindow.ShowDialog();

        //            return await SendInvoiceToESDC(guid, invoiceRequest, refaundCash);
        //        }
        //        else
        //        {
        //            Errors er = new Errors(modelErrors);
        //        }
        //    }
        //    return false;
        //}
        public void ResetAll()
        {
            CurrentInvoice = null;
        }

        #endregion Public methods

        #region Internal methods
        internal void Initialize()
        {
            try
            {
                SelectedDateForRefund = DateTime.Now;

                AllInvoicesInDate = new List<Invoice>();
                var invoices = DbContext.Invoices.Where(invoice =>
                invoice.TransactionType == (int)ClickBar_Common.Enums.TransactionTypeEnumeration.Sale &&
                invoice.SdcDateTime.HasValue &&
                invoice.SdcDateTime.Value.Date == SelectedDateForRefund.Date);

                var invoicesRefaund = DbContext.Invoices.Where(invoice =>
                invoice.TransactionType == (int)ClickBar_Common.Enums.TransactionTypeEnumeration.Refund &&
                invoice.SdcDateTime.HasValue);

                if (invoices != null &&
                    invoices.Any())
                {
                    invoices.ToList().ForEach(invoice =>
                    {
                        if (invoicesRefaund.FirstOrDefault(inv => inv.ReferentDocumentNumber == invoice.InvoiceNumberResult) == null &&
                            !string.IsNullOrEmpty(invoice.InvoiceNumberResult))
                        {
                            AllInvoicesInDate.Add(new Invoice(invoice, AllInvoicesInDate.Count + 1));
                        }
                    });
                }
                CurrentInvoice = null;
                SearchInvoices = new ObservableCollection<Invoice>(AllInvoicesInDate);

                InvoiceType = InvoiceTypeEnumeration.Promet;
            }
            catch (Exception ex)
            {
                int a = 2;
            }
        }
        #endregion Internal methods

        #region Private methods
        private bool IsFileLocked(string file)
        {
            //check that problem is not in destination file
            if (File.Exists(file))
            {
                FileStream stream = null;
                try
                {
                    stream = File.Open(file, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                }
                catch (Exception ex2)
                {
                    //_log.WriteLog(ex2, "Error in checking whether file is locked " + file);
                    int errorCode = Marshal.GetHRForException(ex2) & ((1 << 16) - 1);
                    if ((ex2 is IOException) && (errorCode == ERROR_SHARING_VIOLATION || errorCode == ERROR_LOCK_VIOLATION))
                    {
                        return true;
                    }
                }
                finally
                {
                    if (stream != null)
                        stream.Close();
                }
            }
            else
            {
                if (DateTime.Now.Subtract(_timer).TotalSeconds > 60)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            return false;
        }
        private async Task<InvoceRequestFileSystemWatcher> CreateCopyInvoice(InvoiceResult invoiceResult)
        {
            var invoiceDB = DbContext.Invoices.FirstOrDefault(invoice => invoice.InvoiceNumberResult == invoiceResult.InvoiceNumber);

            if (invoiceDB != null)
            {
                var itemsInvoice = DbContext.ItemInvoices.Where(invoice => invoice.InvoiceId == invoiceDB.Id &&
                                (invoice.IsSirovina == null || invoice.IsSirovina == 0));

                if (itemsInvoice != null)
                {
                    List<ClickBar_Common.Models.Invoice.Item> items = new List<ClickBar_Common.Models.Invoice.Item>();

                    InvoceRequestFileSystemWatcher invoiceRequest = new InvoceRequestFileSystemWatcher()
                    {
                        BuyerId = CurrentInvoiceRequest.BuyerId,
                        Cashier = LoggedCashier.Name,
                        InvoiceType = ClickBar_Common.Enums.InvoiceTypeEenumeration.Copy,
                        TransactionType = ClickBar_Common.Enums.TransactionTypeEnumeration.Refund,
                        Items = CurrentInvoiceRequest.Items,
                        Payment = CurrentInvoiceRequest.Payment,
                        ReferentDocumentNumber = invoiceResult.InvoiceNumber,
                        ReferentDocumentDT = invoiceResult.SdcDateTime,
                    };
                    return invoiceRequest;
                }
            }
            return null;
        }
        private void Search()
        {
            if (string.IsNullOrEmpty(_refNumber))
            {
                SearchInvoices = new ObservableCollection<Invoice>(AllInvoicesInDate.Where(invoice => invoice.InvoiceType == InvoiceType).ToList());
            }
            else
            {
                var searchInvoice = SearchInvoices.Where(invoice => invoice.InvoiceNumber.Contains(_refNumber)).ToList();
                if (searchInvoice is not null)
                {
                    SearchInvoices = new ObservableCollection<Invoice>(searchInvoice);
                }
                else
                {
                    SearchInvoices = new ObservableCollection<Invoice>();
                }
            }
            if (string.IsNullOrEmpty(_refDateDay))
            {
                if (string.IsNullOrEmpty(_refNumber))
                {
                    SearchInvoices = new ObservableCollection<Invoice>(AllInvoicesInDate.Where(invoice => invoice.InvoiceType == InvoiceType).ToList());
                }
            }
            else
            {
                var searchInvoice = SearchInvoices.Where(invoice => invoice.SdcDateTime.Day.ToString().Contains(_refDateDay)).ToList();
                if (searchInvoice is not null)
                {
                    SearchInvoices = new ObservableCollection<Invoice>(searchInvoice);
                }
                else
                {
                    SearchInvoices = new ObservableCollection<Invoice>();
                }
            }
            if (string.IsNullOrEmpty(_refDateMonth))
            {
                if (string.IsNullOrEmpty(_refNumber) &&
                    string.IsNullOrEmpty(_refDateDay))
                {
                    SearchInvoices = new ObservableCollection<Invoice>(AllInvoicesInDate.Where(invoice => invoice.InvoiceType == InvoiceType).ToList());
                }
            }
            else
            {
                var searchInvoice = SearchInvoices.Where(invoice => invoice.SdcDateTime.Month.ToString().Contains(_refDateMonth)).ToList();
                if (searchInvoice is not null)
                {
                    SearchInvoices = new ObservableCollection<Invoice>(searchInvoice);
                }
                else
                {
                    SearchInvoices = new ObservableCollection<Invoice>();
                }
            }
            if (string.IsNullOrEmpty(_refDateYear))
            {
                if (string.IsNullOrEmpty(_refNumber) &&
                    string.IsNullOrEmpty(_refDateDay) &&
                    string.IsNullOrEmpty(_refDateMonth))
                {
                    SearchInvoices = new ObservableCollection<Invoice>(AllInvoicesInDate.Where(invoice => invoice.InvoiceType == InvoiceType).ToList());
                }
            }
            else
            {
                var searchInvoice = SearchInvoices.Where(invoice => invoice.SdcDateTime.Year.ToString().Contains(_refDateYear)).ToList();
                if (searchInvoice is not null)
                {
                    SearchInvoices = new ObservableCollection<Invoice>(searchInvoice);
                }
                else
                {
                    SearchInvoices = new ObservableCollection<Invoice>();
                }
            }
            if (string.IsNullOrEmpty(_refDateHour))
            {
                if (string.IsNullOrEmpty(_refNumber) &&
                    string.IsNullOrEmpty(_refDateDay) &&
                    string.IsNullOrEmpty(_refDateMonth) &&
                    string.IsNullOrEmpty(_refDateYear))
                {
                    SearchInvoices = new ObservableCollection<Invoice>(AllInvoicesInDate.Where(invoice => invoice.InvoiceType == InvoiceType).ToList());
                }
            }
            else
            {
                var searchInvoice = SearchInvoices.Where(invoice => invoice.SdcDateTime.Hour.ToString().Contains(_refDateHour)).ToList();
                if (searchInvoice is not null)
                {
                    SearchInvoices = new ObservableCollection<Invoice>(searchInvoice);
                }
                else
                {
                    SearchInvoices = new ObservableCollection<Invoice>();
                }
            }
            if (string.IsNullOrEmpty(_refDateMinute))
            {
                if (string.IsNullOrEmpty(_refNumber) &&
                    string.IsNullOrEmpty(_refDateDay) &&
                    string.IsNullOrEmpty(_refDateMonth) &&
                    string.IsNullOrEmpty(_refDateYear) &&
                    string.IsNullOrEmpty(_refDateHour))
                {
                    SearchInvoices = new ObservableCollection<Invoice>(AllInvoicesInDate.Where(invoice => invoice.InvoiceType == InvoiceType).ToList());
                }
            }
            else
            {
                var searchInvoice = SearchInvoices.Where(invoice => invoice.SdcDateTime.Minute.ToString().Contains(_refDateMinute)).ToList();
                if (searchInvoice is not null)
                {
                    SearchInvoices = new ObservableCollection<Invoice>(searchInvoice);
                }
                else
                {
                    SearchInvoices = new ObservableCollection<Invoice>();
                }
            }
            if (string.IsNullOrEmpty(_refDateSecond))
            {
                if (string.IsNullOrEmpty(_refNumber) &&
                    string.IsNullOrEmpty(_refDateDay) &&
                    string.IsNullOrEmpty(_refDateMonth) &&
                    string.IsNullOrEmpty(_refDateYear) &&
                    string.IsNullOrEmpty(_refDateHour) &&
                    string.IsNullOrEmpty(_refDateMinute))
                {
                    SearchInvoices = new ObservableCollection<Invoice>(AllInvoicesInDate.Where(invoice => invoice.InvoiceType == InvoiceType).ToList());
                }
            }
            else
            {
                var searchInvoice = SearchInvoices.Where(invoice => invoice.SdcDateTime.Second.ToString().Contains(_refDateSecond)).ToList();
                if (searchInvoice is not null)
                {
                    SearchInvoices = new ObservableCollection<Invoice>(searchInvoice);
                }
                else
                {
                    SearchInvoices = new ObservableCollection<Invoice>();
                }
            }
        }
        private async void SaveToDB(InvoceRequestFileSystemWatcher invoiceRequset,
            ResponseJson responseJson,
            bool eFaktura)
        {
            InvoiceDB invoice = await InsertInvoiceInDB(invoiceRequset, responseJson);
            if (!eFaktura)
            {
                await TakingUpNorm(invoice);
            }
        }
        private async Task<InvoiceDB> InsertInvoiceInDB(InvoceRequestFileSystemWatcher invoiceRequset, ResponseJson responseJson)
        {
            decimal total = 0;

            invoiceRequset.Items.ToList().ForEach(item =>
            {
                total += item.TotalAmount;
            });

            InvoiceDB invoiceDB = new InvoiceDB()
            {
                Id = Guid.NewGuid().ToString(),
                Cashier = LoggedCashier.Id,
                InvoiceType = (int)invoiceRequset.InvoiceType,
                TransactionType = (int)invoiceRequset.TransactionType,
                SdcDateTime = Convert.ToDateTime(responseJson.DateTime),
                TotalAmount = total,
                InvoiceCounter = responseJson.TotalInvoiceNumber,
                InvoiceNumberResult = responseJson.InvoiceNumber,
                BuyerId = invoiceRequset.BuyerId,
                BuyerName = invoiceRequset.BuyerName,
                BuyerAddress = invoiceRequset.BuyerAddress,
                ReferentDocumentNumber = invoiceRequset.ReferentDocumentNumber,
                ReferentDocumentDt = invoiceRequset.ReferentDocumentDT
            };

            DbContext.Add(invoiceDB);
            DbContext.SaveChanges();

            int itemInvoiceId = 0;
            invoiceRequset.Items.ToList().ForEach(async item =>
            {
                ItemDB? itemDB = DbContext.Items.Find(item.Id);
                if (itemDB != null)
                {
                    if (itemDB.IdNorm != null)
                    {
                        var norms = DbContext.ItemsInNorm.Where(norm => norm.IdNorm == itemDB.IdNorm);

                        if (norms != null &&
                        norms.Any())
                        {
                            await norms.ForEachAsync(norm =>
                            {
                                var normItem = DbContext.Items.Find(norm.IdItem);

                                if (normItem != null)
                                {
                                    var itemInvoice = new ItemInvoiceDB()
                                    {
                                        Id = itemInvoiceId++,
                                        Quantity = item.Quantity * norm.Quantity,
                                        TotalAmout = item.Quantity * norm.Quantity * itemDB.SellingUnitPrice,
                                        Label = itemDB.Label,
                                        Name = itemDB.Name,
                                        UnitPrice = itemDB.SellingUnitPrice,
                                        ItemCode = itemDB.Id,
                                        OriginalUnitPrice = itemDB.SellingUnitPrice,
                                        InvoiceId = invoiceDB.Id,
                                        IsSirovina = 1,
                                        //Item = itemDB
                                    };
                                    DbContext.Add(itemInvoice);
                                }
                            });
                        }
                    }

                    var itemInvoice = new ItemInvoiceDB()
                    {
                        Id = itemInvoiceId++,
                        Quantity = item.Quantity,
                        TotalAmout = item.Quantity * item.UnitPrice,
                        Label = item.Label,
                        Name = item.Name,
                        UnitPrice = item.UnitPrice,
                        ItemCode = item.Id,
                        OriginalUnitPrice = itemDB.SellingUnitPrice,
                        InvoiceId = invoiceDB.Id,
                        IsSirovina = 0
                        //Item = itemDB
                    };

                    DbContext.Add(itemInvoice);
                }
            });

            invoiceRequset.Payment.ToList().ForEach(payment =>
            {
                PaymentInvoiceDB paymentInvoice = new PaymentInvoiceDB()
                {
                    InvoiceId = invoiceDB.Id,
                    Amout = payment.Amount,
                    PaymentType = payment.PaymentType
                };

                DbContext.PaymentInvoices.Add(paymentInvoice);
            });
            DbContext.SaveChanges();

            if (responseJson.TaxItems != null &&
                responseJson.TaxItems.Any())
            {
                responseJson.TaxItems.ToList().ForEach(taxItem =>
                {
                    TaxItemInvoiceDB taxItemInvoiceDB = new TaxItemInvoiceDB()
                    {
                        Amount = taxItem.Amount,
                        CategoryName = taxItem.CategoryName,
                        CategoryType = (int)taxItem.CategoryType.Value,
                        Label = taxItem.Label,
                        Rate = taxItem.Rate,
                        InvoiceId = invoiceDB.Id
                    };

                    DbContext.TaxItemInvoices.Add(taxItemInvoiceDB);
                });
            }
            DbContext.SaveChanges();

            return invoiceDB;
        }
        private async Task TakingUpNorm(InvoiceDB invoice)
        {
            List<ItemDB> itemsForCondition = new List<ItemDB>();

            invoice.ItemInvoices.ToList().ForEach(async item =>
            {
                var it = DbContext.Items.Find(item.ItemCode);
                if (it != null &&
                item.Quantity.HasValue)
                {
                    var itemInNorm = DbContext.ItemsInNorm.Where(norm => it.IdNorm == norm.IdNorm);

                    if (itemInNorm != null &&
                    itemInNorm.Any())
                    {
                        await itemInNorm.ForEachAsync(norm =>
                        {
                            var itm = DbContext.Items.Find(norm.IdItem);

                            if (itm != null)
                            {
                                if (itm.IdNorm == null)
                                {
                                    itm.TotalQuantity += item.Quantity.Value * norm.Quantity;
                                    DbContext.Items.Update(itm);
                                }
                                else
                                {
                                    var itemInNorm2 = DbContext.ItemsInNorm.Where(norm => itm.IdNorm == norm.IdNorm);
                                    if (itemInNorm2.Any())
                                    {
                                        itemInNorm2.ToList().ForEach(norm2 =>
                                        {
                                            var itm2 = DbContext.Items.Find(norm2.IdItem);

                                            if (itm2 != null)
                                            {
                                                if (itm2.IdNorm == null)
                                                {
                                                    itm2.TotalQuantity += item.Quantity.Value * norm.Quantity * norm2.Quantity;
                                                    DbContext.Items.Update(itm2);
                                                }
                                                else
                                                {
                                                    var itemInNorm3 = DbContext.ItemsInNorm.Where(norm => itm2.IdNorm == norm2.IdNorm);
                                                    if (itemInNorm3.Any())
                                                    {
                                                        itemInNorm3.ToList().ForEach(norm3 =>
                                                        {
                                                            var itm3 = DbContext.Items.Find(norm3.IdItem);

                                                            if (itm3 != null)
                                                            {
                                                                if (itm3.IdNorm == null)
                                                                {
                                                                    itm3.TotalQuantity += item.Quantity.Value * norm.Quantity * norm2.Quantity * norm3.Quantity;
                                                                    DbContext.Items.Update(itm3);
                                                                }
                                                            }
                                                        });
                                                    }
                                                    else
                                                    {
                                                        itm2.TotalQuantity += item.Quantity.Value * norm.Quantity * norm2.Quantity;
                                                        DbContext.Items.Update(itm2);
                                                    }
                                                }
                                            }
                                        });
                                    }
                                    else
                                    {
                                        itm.TotalQuantity += item.Quantity.Value * norm.Quantity;
                                        DbContext.Items.Update(itm);
                                    }
                                }
                            }
                        });
                    }
                    else
                    {
                        it.TotalQuantity += item.Quantity.Value;
                        DbContext.Items.Update(it);
                    }
                }
                //var it = sqliteDbContext.Items.Find(item.ItemCode);
                //if (it != null && item.Quantity.HasValue)
                //{
                //    var itemInNorm = sqliteDbContext.ItemsInNorm.Where(norm => it.IdNorm == norm.IdNorm);

                //    if (itemInNorm.Any())
                //    {
                //        itemInNorm.ToList().ForEach(norm =>
                //        {
                //            var itm = sqliteDbContext.Items.Find(norm.IdItem);

                //            if (itm != null)
                //            {
                //                itm.TotalQuantity += item.Quantity.Value * norm.Quantity;

                //                sqliteDbContext.Items.Update(itm);
                //            }
                //        });
                //    }
                //    else
                //    {
                //        it.TotalQuantity += item.Quantity.Value;

                //        sqliteDbContext.Items.Update(it);
                //    }
                //}
            });

            DbContext.SaveChanges();
        }

        #endregion Private methods
    }
}
