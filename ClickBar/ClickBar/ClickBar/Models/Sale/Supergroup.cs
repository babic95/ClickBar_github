using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar.Models.Sale
{
    public class Supergroup : ObservableObject
    {
        private int _id;
        private string _name;
        private bool _focusable;

        public Supergroup(int id, string name)
        {
            Id = id;
            Name = name;
            Focusable = false;
        }

        public int Id
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
        public bool Focusable
        {
            get { return _focusable; }
            set
            {
                _focusable = value;
                OnPropertyChange(nameof(Focusable));
            }
        }
    }
}
