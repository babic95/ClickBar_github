using ClickBar_DatabaseSQLManager.Models;
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
        private int _rb;

        public Supergroup() { }
        public Supergroup(SupergroupDB supergroupDB)
        {
            Id = supergroupDB.Id;
            Name = supergroupDB.Name;
            Focusable = false;
            Rb = supergroupDB.Rb;
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
        public int Rb
        {
            get { return _rb; }
            set
            {
                _rb = value;
                OnPropertyChange(nameof(Rb));
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
