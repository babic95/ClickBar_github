using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar.Models.AppMain.Statistic.PocetnaStanja
{
    public class PocetnoStanje : ObservableObject
    {
        private string _id;
        private DateTime _popisDate;
        private ObservableCollection<PocetnoStanjeItem> _items;

        public PocetnoStanje(IEnumerable<PocetnoStanjeItem> items)
        {
            Id = Guid.NewGuid().ToString();
            PopisDate = DateTime.Now;
            Items = new ObservableCollection<PocetnoStanjeItem>(items);
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
        public DateTime PopisDate
        {
            get { return _popisDate; }
            set
            {
                _popisDate = value;
                OnPropertyChange(nameof(PopisDate));
            }
        }
        public ObservableCollection<PocetnoStanjeItem> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                OnPropertyChange(nameof(Items));
            }
        }
    }
}
