using ClickBar.Enums.Sale;
using ClickBar_DatabaseSQLManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar.Models.Sale
{
    public class Invoice : ObservableObject
    {
        private string _id;
        private int _index;
        private InvoiceTypeEnumeration _invoiceType;
        private TransactionTypeEnumeration _transactionType;
        private string _cashier;
        private DateTime _sdcDateTime;
        private string _invoiceNumber;
        private string _requestedBy;
        private string _signedBy;
        private int _totalCounter;
        private decimal _totalAmount;
        private string _totalAmountInvoiceString;
        private string _buyerName;
        private string _buyerId;

        public Invoice(InvoiceDB invoice, int index)
        {
            Id = invoice.Id;
            Index = index;
            InvoiceType = (InvoiceTypeEnumeration)(int)invoice.InvoiceType;
            TransactionType = (TransactionTypeEnumeration)(int)invoice.TransactionType;
            Cashier = invoice.Cashier;
            SdcDateTime = invoice.SdcDateTime.Value;
            InvoiceNumber = invoice.InvoiceNumberResult;
            RequestedBy = invoice.RequestedBy;
            SignedBy = invoice.SignedBy;
            //TotalCounter = invoice.TotalCounter != null && invoice.TotalCounter.HasValue ? invoice.TotalCounter.Value : -1;
            TotalAmount = invoice.TotalAmount.Value;
            BuyerName = string.IsNullOrEmpty(invoice.BuyerName) ? string.Empty : invoice.BuyerName;
            BuyerId = string.IsNullOrEmpty(invoice.BuyerId) ? string.Empty : invoice.BuyerId;
        }

        public string BuyerName
        {
            get { return _buyerName; }
            set
            {
                _buyerName = value;
                OnPropertyChange(nameof(BuyerName));
            }
        }
        public string BuyerId
        {
            get { return _buyerId; }
            set
            {
                _buyerId = value;
                OnPropertyChange(nameof(BuyerId));
            }
        }
        public string Id
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChange(nameof(Id));
            }
        }
        public int Index
        {
            get { return _index; }
            set
            {
                _index = value;
                OnPropertyChange(nameof(Index));
            }
        }
        public InvoiceTypeEnumeration InvoiceType
        {
            get { return _invoiceType; }
            set
            {
                _invoiceType = value;
                OnPropertyChange(nameof(InvoiceType));
            }
        }
        public TransactionTypeEnumeration TransactionType
        {
            get { return _transactionType; }
            set
            {
                _transactionType = value;
                OnPropertyChange(nameof(TransactionType));
            }
        }
        public string Cashier
        {
            get { return _cashier; }
            set
            {
                _cashier = value;
                OnPropertyChange(nameof(Cashier));
            }
        }
        public DateTime SdcDateTime
        {
            get { return _sdcDateTime; }
            set
            {
                _sdcDateTime = value;
                OnPropertyChange(nameof(SdcDateTime));
            }
        }
        public string InvoiceNumber
        {
            get { return _invoiceNumber; }
            set
            {
                _invoiceNumber = value;
                OnPropertyChange(nameof(InvoiceNumber));
            }
        }
        public string RequestedBy
        {
            get { return _requestedBy; }
            set
            {
                _requestedBy = value;
                OnPropertyChange(nameof(RequestedBy));
            }
        }
        public string SignedBy
        {
            get { return _signedBy; }
            set
            {
                _signedBy = value;
                OnPropertyChange(nameof(SignedBy));
            }
        }
        public int TotalCounter
        {
            get { return _totalCounter; }
            set
            {
                _totalCounter = value;
                OnPropertyChange(nameof(TotalCounter));
            }
        }
        public decimal TotalAmount
        {
            get { return _totalAmount; }
            set
            {
                _totalAmount = Decimal.Round(value, 2);
                OnPropertyChange(nameof(TotalAmount));

                if (TransactionType == TransactionTypeEnumeration.Refundacija)
                {
                    TotalAmountInvoiceString = string.Format("{0:#,##0.00}", (-1 * _totalAmount)).Replace(',', '#').Replace('.', ',').Replace('#', '.');
                }
                else
                {
                    TotalAmountInvoiceString = string.Format("{0:#,##0.00}", _totalAmount).Replace(',', '#').Replace('.', ',').Replace('#', '.');
                }
            }
        }
        public string TotalAmountInvoiceString
        {
            get { return _totalAmountInvoiceString; }
            set
            {
                _totalAmountInvoiceString = value;
                OnPropertyChange(nameof(TotalAmountInvoiceString));
            }
        }
    }
}
