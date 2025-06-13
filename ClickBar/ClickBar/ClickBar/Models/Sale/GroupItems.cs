using ClickBar_DatabaseSQLManager.Models;
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
        private int _rb;

        public GroupItems()
        {
        }
        public GroupItems(ItemGroupDB itemGroupDB)
        {
            Id = itemGroupDB.Id;
            Name = itemGroupDB.Name;
            IdSupergroup = itemGroupDB.IdSupergroup;
            Rb = itemGroupDB.Rb;
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
        public int Rb
        {
            get { return _rb; }
            set
            {
                _rb = value;
                OnPropertyChange(nameof(Rb));
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
