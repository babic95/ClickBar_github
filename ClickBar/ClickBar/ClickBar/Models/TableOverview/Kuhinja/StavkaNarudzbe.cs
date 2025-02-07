using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar.Models.TableOverview.Kuhinja
{
    public class StavkaNarudzbe : ObservableObject
    {
        private string _id;
        private string _brArt;
        private string _naziv;
        private decimal _kolicina;
        private decimal _stornoKolicina;
        private decimal _mpc;
        private decimal _ukupno;

        public string Id
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChange(nameof(Id));
            }
        }
        public string BrArt
        {
            get { return _brArt; }
            set
            {
                _brArt = value;
                OnPropertyChange(nameof(BrArt));
            }
        }
        public string Naziv
        {
            get { return _naziv; }
            set
            {
                _naziv = value;
                OnPropertyChange(nameof(Naziv));
            }
        }
        public decimal Kolicina
        {
            get { return _kolicina; }
            set
            {
                _kolicina = value;
                OnPropertyChange(nameof(Kolicina));
            }
        }
        public decimal StornoKolicina
        {
            get { return _stornoKolicina; }
            set
            {
                _stornoKolicina = value;
                OnPropertyChange(nameof(StornoKolicina));
            }
        }
        public decimal Ukupno
        {
            get { return _ukupno; }
            set
            {
                _ukupno = value;
                OnPropertyChange(nameof(Ukupno));
            }
        }
        public decimal Mpc
        {
            get { return _mpc; }
            set
            {
                _mpc = value;
                OnPropertyChange(nameof(Mpc));
            }
        }
    }
}
