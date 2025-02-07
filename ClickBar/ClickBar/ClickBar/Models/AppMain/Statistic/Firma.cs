using ClickBar_Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar.Models.AppMain.Statistic
{
    public class Firma : ObservableObject
    {
        private int _id;
        private string? _name;
        private string? _pib;
        private string? _mb;
        private string? _namePP;
        private string? _addressPP;
        private string? _number;
        private string? _email;
        private string? _bankAcc;
        private string? _authenticationKey;

        public Firma() { }
        public Firma(FirmaDB firma)
        {
            Id = firma.Id;
            Name = firma.Name;
            Pib = firma.Pib;
            MB = firma.MB;
            NamePP = firma.NamePP;
            AddressPP = firma.AddressPP;
            Number = firma.Number;
            Email = firma.Email;
            BankAcc = firma.BankAcc;
            AuthenticationKey = firma.AuthenticationKey;
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
        public string? Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChange(nameof(Name));
            }
        }
        public string? Pib
        {
            get { return _pib; }
            set
            {
                _pib = value;
                OnPropertyChange(nameof(Pib));
            }
        }
        public string? MB
        {
            get { return _mb; }
            set
            {
                _mb = value;
                OnPropertyChange(nameof(MB));
            }
        }
        public string? NamePP
        {
            get { return _namePP; }
            set
            {
                _namePP = value;
                OnPropertyChange(nameof(NamePP));
            }
        }
        public string? AddressPP
        {
            get { return _addressPP; }
            set
            {
                _addressPP = value;
                OnPropertyChange(nameof(AddressPP));
            }
        }
        public string? Number
        {
            get { return _number; }
            set
            {
                _number = value;
                OnPropertyChange(nameof(Number));
            }
        }
        public string? Email
        {
            get { return _email; }
            set
            {
                _email = value;
                OnPropertyChange(nameof(Email));
            }
        }
        public string? BankAcc
        {
            get { return _bankAcc; }
            set
            {
                _bankAcc = value;
                OnPropertyChange(nameof(BankAcc));
            }
        }

        public string? AuthenticationKey
        {
            get { return _authenticationKey; }
            set
            {
                _authenticationKey = value;
                OnPropertyChange(nameof(AuthenticationKey));
            }
        }
    }
}
