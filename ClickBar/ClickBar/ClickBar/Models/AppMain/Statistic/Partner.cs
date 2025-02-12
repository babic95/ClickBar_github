using ClickBar_DatabaseSQLManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar.Models.AppMain.Statistic
{
    public class Partner : ObservableObject
    {
        private int _id;
        private string _name;
        private string _pib;
        private string _mb;
        private string _city;
        private string _address;
        private string _contractNumber;
        private string _email;

        public Partner() { }
        public Partner(PartnerDB partner)
        {
            Id = partner.Id;
            Name = partner.Name;
            Pib = partner.Pib;
            MB = partner.Mb;
            City = partner.City;
            Address = partner.Address;
            ContractNumber = partner.ContractNumber;
            Email = partner.Email;
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
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChange(nameof(Name));
            }
        }
        public string Pib
        {
            get { return _pib; }
            set
            {
                _pib = value;
                OnPropertyChange(nameof(Pib));
            }
        }
        public string MB
        {
            get { return _mb; }
            set
            {
                _mb = value;
                OnPropertyChange(nameof(MB));
            }
        }
        public string City
        {
            get { return _city; }
            set
            {
                _city = value;
                OnPropertyChange(nameof(City));
            }
        }
        public string Address
        {
            get { return _address; }
            set
            {
                _address = value;
                OnPropertyChange(nameof(Address));
            }
        }
        public string ContractNumber
        {
            get { return _contractNumber; }
            set
            {
                _contractNumber = value;
                OnPropertyChange(nameof(ContractNumber));
            }
        }
        public string Email
        {
            get { return _email; }
            set
            {
                _email = value;
                OnPropertyChange(nameof(Email));
            }
        }
    }
}
