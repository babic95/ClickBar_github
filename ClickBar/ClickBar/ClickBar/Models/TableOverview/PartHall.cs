using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar.Models.TableOverview
{
    public class PartHall : ObservableObject
    {
        private int _id;
        private string _name;
        private string _image;

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
        public string? Image
        {
            get { return _image; }
            set
            {
                _image = value;
                OnPropertyChange(nameof(Image));
            }
        }
    }
}
