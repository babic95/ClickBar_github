using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar.Models.AppMain.Statistic.DPU
{
    public class DPU_Item : ObservableObject
    {
        private string _id;
        private string _name;
        private decimal _startQuantity;
        private decimal _inputQuantity;
        private decimal _outputQuantity;
        private decimal _endQuantity;

        public DPU_Item()
        {
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
        public decimal StartQuantity
        {
            get { return _startQuantity; }
            set
            {
                _startQuantity = value;
                OnPropertyChange(nameof(StartQuantity));
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
        public decimal EndQuantity
        {
            get { return _endQuantity; }
            set
            {
                _endQuantity = value;
                OnPropertyChange(nameof(EndQuantity));
            }
        }
    }
}
