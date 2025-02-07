using ClickBar.Models.Sale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar.Models.AppMain.Statistic
{
    public class CalculationItem : ObservableObject
    {
        private Item _item;
        private decimal _inputPrice;
        private decimal _outputPrice;
        private decimal _quantity;

        public Item Item
        {
            get { return _item; }
            set
            {
                _item = value;
                OnPropertyChange(nameof(Item));
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
        public decimal Quantity
        {
            get { return _quantity; }
            set
            {
                _quantity = value;
                OnPropertyChange(nameof(Quantity));
            }
        }
    }
}
