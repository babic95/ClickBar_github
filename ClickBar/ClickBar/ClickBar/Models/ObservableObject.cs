using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar.Models
{
    public class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChange(string promertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(promertyName));
        }
    }
}
