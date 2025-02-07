using ClickBar.Models.Sale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar.Models.AppMain.Statistic
{
    public class Normativ : ObservableObject
    {
        private Item _item;
        private decimal _quantity;
        public Normativ(Item item, decimal quantity)
        {
            Item = item;
            Quantity = quantity;
        }
        public Item Item
        {
            get { return _item; }
            set
            {
                _item = value;
                OnPropertyChange(nameof(Item));
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
