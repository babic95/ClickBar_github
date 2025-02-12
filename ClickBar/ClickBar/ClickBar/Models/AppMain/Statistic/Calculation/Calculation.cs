using ClickBar_DatabaseSQLManager.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar.Models.AppMain.Statistic
{
    public class Calculation : ObservableObject
    {
        private string _id;
        private ObservableCollection<Invertory> _calculationItems;
        private Supplier? _supplier;
        private string _cashierName;
        private DateTime _calculationDate;
        private string? _invoiceNumber;
        private decimal _inputTotalPrice;
        private decimal _outputTotalPrice;
        private int _counter;
        private CashierDB? _cashier;
        private string _name;

        public CashierDB? Cashier
        {
            get { return _cashier; }
            set
            {
                _cashier = value;
                if (value != null)
                {
                    CashierName = value.Name;
                }
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
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChange(nameof(Name));
            }
        }
        public int Counter
        {
            get { return _counter; }
            set
            {
                _counter = value;
                OnPropertyChange(nameof(Counter));
            }
        }
        public ObservableCollection<Invertory> CalculationItems
        {
            get { return _calculationItems; }
            set
            {
                _calculationItems = value;
                OnPropertyChange(nameof(CalculationItems));
            }
        }
        public Supplier? Supplier
        {
            get { return _supplier; }
            set
            {
                _supplier = value;
                OnPropertyChange(nameof(Supplier));
            }
        }
        public string CashierName
        {
            get { return _cashierName; }
            set
            {
                _cashierName = value;
                OnPropertyChange(nameof(CashierName));
            }
        }
        public DateTime CalculationDate
        {
            get { return _calculationDate; }
            set
            {
                _calculationDate = value;
                OnPropertyChange(nameof(CalculationDate));
            }
        }
        public string? InvoiceNumber
        {
            get { return _invoiceNumber; }
            set
            {
                _invoiceNumber = value;
                OnPropertyChange(nameof(InvoiceNumber));
            }
        }
        public decimal InputTotalPrice
        {
            get { return _inputTotalPrice; }
            set
            {
                _inputTotalPrice = value;
                OnPropertyChange(nameof(InputTotalPrice));
            }
        }
        public decimal OutputTotalPrice
        {
            get { return _outputTotalPrice; }
            set
            {
                _outputTotalPrice = value;
                OnPropertyChange(nameof(OutputTotalPrice));
            }
        }
    }
}
