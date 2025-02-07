using ClickBar.Models.AppMain.Statistic;
using ClickBar.Models.TableOverview;
using ClickBar_Database.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar.Models.Sale
{
    public class Zelja : ObservableObject
    {
        private string _id;
        private int _rb;
        private string _name;
        private string _description;
        private ObservableCollection<ItemZelja> _fixneZelje;

        public Zelja(int rb, string name, ObservableCollection<ItemZelja> fixneZelje)
        {
            Id = Guid.NewGuid().ToString();
            Rb = rb;
            Name = $"{name}__{rb}";
            FixneZelje = fixneZelje;

            foreach (var itemZelja in FixneZelje)
            {
                itemZelja.IsCheckedChanged += ItemZelja_IsCheckedChanged;
            }
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
        public int Rb
        {
            get { return _rb; }
            set
            {
                _rb = value;
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
        public ObservableCollection<ItemZelja> FixneZelje
        {
            get { return _fixneZelje; }
            set
            {
                _fixneZelje = value;
                OnPropertyChange(nameof(FixneZelje));
            }
        }
        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                OnPropertyChange(nameof(Description));
            }
        }
        private void ItemZelja_IsCheckedChanged(object sender, EventArgs e)
        {
            var itemZelja = sender as ItemZelja;
            if (itemZelja != null)
            {
                if (!itemZelja.IsChecked)
                {
                    // Uklanja ", {itemZelja.Zelja}" iz Description
                    string substring = $", {itemZelja.Zelja}";
                    if (Description.Contains(substring))
                    {
                        Description = Description.Replace(substring, string.Empty);
                    }
                    else
                    {
                        // Ako prvi deo ne sadrži zarez
                        substring = $"{itemZelja.Zelja}";
                        if (Description.Contains(substring))
                        {
                            Description = Description.Replace(substring, string.Empty);
                        }
                    }

                    // Uklonite suvišne zareze
                    Description = Description.Trim().TrimEnd(',');
                }
                else
                {
                    if (string.IsNullOrEmpty(Description))
                    {
                        Description = $"{itemZelja.Zelja}";
                    }
                    else
                    {
                        Description += $", {itemZelja.Zelja}";
                    }
                }
            }
        }
    }
}
