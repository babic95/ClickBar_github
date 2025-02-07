using ClickBar.Enums.AppMain.Statistic;
using ClickBar_Database;
using ClickBar_Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar.Models.AppMain.Statistic
{
    public class Radnik : ObservableObject
    {
        private string _id;
        private string _originalId;
        private string _name;
        private string? _smartCardNumber;
        private RadnikStateEnumeration _radnikStateEnumeration;
        private string? _jmbg;
        private string? _city;
        private string? _address;
        private string? _contractNumber;
        private string? _email;

        public Radnik() { }
        public Radnik(CashierDB radnik)
        {
            Id = radnik.Id;
            OriginalId = radnik.Id;
            Name = radnik.Name;
            RadnikStateEnumeration = radnik.Type == 0 ? RadnikStateEnumeration.Konobar : RadnikStateEnumeration.Menadzer;
            Jmbg = radnik.Jmbg;
            City = radnik.City;
            Address = radnik.Address;
            ContractNumber = radnik.ContactNumber;
            Email = radnik.Email;
            SmartCardNumber = radnik.SmartCardNumber;
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
        public string OriginalId
        {
            get { return _originalId; }
            set
            {
                _originalId = value;
                OnPropertyChange(nameof(OriginalId));
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
        public string? SmartCardNumber
        {
            get { return _smartCardNumber; }
            set
            {
                _smartCardNumber = value;
                OnPropertyChange(nameof(SmartCardNumber));
            }
        }
        public RadnikStateEnumeration RadnikStateEnumeration
        {
            get { return _radnikStateEnumeration; }
            set
            {
                _radnikStateEnumeration = value;
                OnPropertyChange(nameof(RadnikStateEnumeration));
            }
        }
        public string? Jmbg
        {
            get { return _jmbg; }
            set
            {
                _jmbg = value;
                OnPropertyChange(nameof(Jmbg));
            }
        }
        public string? City
        {
            get { return _city; }
            set
            {
                _city = value;
                OnPropertyChange(nameof(City));
            }
        }
        public string? Address
        {
            get { return _address; }
            set
            {
                _address = value;
                OnPropertyChange(nameof(Address));
            }
        }
        public string? ContractNumber
        {
            get { return _contractNumber; }
            set
            {
                _contractNumber = value;
                OnPropertyChange(nameof(ContractNumber));
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
    }
}
