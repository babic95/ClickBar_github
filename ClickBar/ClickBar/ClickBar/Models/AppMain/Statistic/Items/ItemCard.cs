using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar.Models.AppMain.Statistic.Items
{
    public class ItemCard : ObservableObject
    {
        private string _description;
        private DateTime _date;
        private decimal _inputQuantity;
        private decimal _outputQuantity;
        private decimal _otpisQuantity;
        private decimal _inputPrice;
        private decimal _outputPrice;
        private decimal _otpisPrice;


        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                OnPropertyChange(nameof(Description));
            }
        }
        public DateTime Date
        {
            get { return _date; }
            set
            {
                _date = value;
                OnPropertyChange(nameof(Date));
            }
        }
        public decimal InputQuantity
        {
            get { return _inputQuantity; }
            set
            {
                _inputQuantity = value;
                OnPropertyChange(nameof(InputQuantity));
            }
        }
        public decimal OutputQuantity
        {
            get { return _outputQuantity; }
            set
            {
                _outputQuantity = value;
                OnPropertyChange(nameof(OutputQuantity));
            }
        }
        public decimal OtpisQuantity
        {
            get { return _otpisQuantity; }
            set
            {
                _otpisQuantity = value;
                OnPropertyChange(nameof(OtpisQuantity));
            }
        }
        public decimal InputPrice
        {
            get { return _inputPrice; }
            set
            {
                _inputPrice = value;
                OnPropertyChange(nameof(InputPrice));
            }
        }
        public decimal OutputPrice
        {
            get { return _outputPrice; }
            set
            {
                _outputPrice = value;
                OnPropertyChange(nameof(OutputPrice));
            }
        }
        public decimal OtpisPrice
        {
            get { return _otpisPrice; }
            set
            {
                _otpisPrice = value;
                OnPropertyChange(nameof(OtpisPrice));
            }
        }
    }
}
