using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar.Models.AppMain.Statistic
{
    public class TaxLabel : ObservableObject
    {
        private string _id;
        private string _name;

        public TaxLabel(string id, string name)
        {
            Id = id;
            Name = name;
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
    }
}
