using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar.Models.Sale
{
    public class GroupItems : ObservableObject
    {
        private int _id;
        private int _idSupergroup;
        private string _name;
        private bool _focusable;

        public GroupItems(int id, int idSupergroup, string name)
        {
            Id = id;
            Name = name;
            IdSupergroup = idSupergroup;
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
        public int IdSupergroup
        {
            get { return _idSupergroup; }
            set
            {
                _idSupergroup = value;
                OnPropertyChange(nameof(IdSupergroup));
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
