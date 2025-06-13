using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar.Models.TableOverview.Kuhinja
{
    public class Narudzbe : ObservableObject
    {
        private string _id;
        private int _brojNarudzbe;
        private DateTime _vremeNarudzbe;
        private string _radnikId;
        private string _stoName;
        private int _smena;
        private ObservableCollection<StavkaNarudzbe> _stavke;
        private string _storno;

        public string Id
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChange(nameof(Id));
            }
        }
        public string Storno
        {
            get { return _storno; }
            set
            {
                _storno = value;
                OnPropertyChange(nameof(Storno));
            }
        }
        public int BrojNarudzbe
        {
            get { return _brojNarudzbe; }
            set
            {
                _brojNarudzbe = value;
                OnPropertyChange(nameof(BrojNarudzbe));
            }
        }
        public DateTime VremeNarudzbe
        {
            get { return _vremeNarudzbe; }
            set
            {
                _vremeNarudzbe = value;
                OnPropertyChange(nameof(VremeNarudzbe));
            }
        }
        public string RadnikId
        {
            get { return _radnikId; }
            set
            {
                _radnikId = value;
                OnPropertyChange(nameof(RadnikId));
            }
        }
        public string StoName
        {
            get { return _stoName; }
            set
            {
                _stoName = value;
                OnPropertyChange(nameof(StoName));
            }
        }
        public int Smena
        {
            get { return _smena; }
            set
            {
                _smena = value;
                OnPropertyChange(nameof(Smena));
            }
        }
        public ObservableCollection<StavkaNarudzbe> Stavke
        {
            get { return _stavke; }
            set
            {
                _stavke = value;
                OnPropertyChange(nameof(Stavke));
            }
        }
    }
}
