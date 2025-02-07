using ClickBar.Models.Sale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar.Models.AppMain.Statistic.Otpis
{
    public class OtpisItem : ObservableObject
    {
        private Item _itemInOtpis;
        private decimal _quantity;

        public Item ItemInOtpis
        {
            get { return _itemInOtpis; }
            set
            {
                _itemInOtpis = value;
                OnPropertyChange(nameof(ItemInOtpis));
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
