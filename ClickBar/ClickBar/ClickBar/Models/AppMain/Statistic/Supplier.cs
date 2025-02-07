using ClickBar_Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar.Models.AppMain.Statistic
{
    public class Supplier : ObservableObject
    {
        private int _id;
        private string _name;
        private string _pib;
        private string _mb;
        private string _city;
        private string _address;
        private string _contractNumber;
        private string _email;

        public Supplier() { }
        public Supplier(SupplierDB supplier)
        {
            Id = supplier.Id;
            Name = supplier.Name;
            Pib = supplier.Pib;
            MB = supplier.Mb;
            City = supplier.City;
            Address = supplier.Address;
            ContractNumber = supplier.ContractNumber;
            Email = supplier.Email;
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
