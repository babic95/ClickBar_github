using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar.Models.AppMain.Statistic
{
    public class ItemZelja : ObservableObject
    {
        private string _id;
        private string _itemId;
        private string _zelja;
        private bool _isChecked;

        public ItemZelja()
        {
            IsChecked = false;
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
        public string ItemId
        {
            get { return _itemId; }
            set
            {
                _itemId = value;
                OnPropertyChange(nameof(ItemId));
            }
        }
        public string Zelja
        {
            get { return _zelja; }
            set
            {
                _zelja = value;
                OnPropertyChange(nameof(Zelja));
            }
        }
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                _isChecked = value;
                OnPropertyChange(nameof(IsChecked));
                OnIsCheckedChanged();
            }
        }

        public event EventHandler IsCheckedChanged;

        protected virtual void OnIsCheckedChanged()
        {
            IsCheckedChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
