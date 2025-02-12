using ClickBar.Enums.AppMain.Statistic;
using ClickBar_DatabaseSQLManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar.Models.AppMain.Statistic.KEP
{
    public class ItemKEP : ObservableObject
    {
        #region Fields
        private string _id;
        private DateTime _kepDate;
        private KepStateEnumeration _type;
        private string _description;
        private decimal _zaduzenje;
        private decimal _razduzenje;
        private decimal _saldo;
        #endregion Fields

        #region Constructors
        public ItemKEP(KepDB kepDB, decimal saldo)
        {
            Id = kepDB.Id;
            KepDate = kepDB.KepDate;
            Type = (KepStateEnumeration)kepDB.Type;
            Description = kepDB.Description;
            Zaduzenje = kepDB.Zaduzenje;
            Razduzenje = kepDB.Razduzenje;
            Saldo = saldo;
        }
        #endregion Constructors

        #region Properties
        public string Id
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChange(nameof(Id));
            }
        }
        public DateTime KepDate
        {
            get { return _kepDate; }
            set
            {
                _kepDate = value;
                OnPropertyChange(nameof(KepDate));
            }
        }
        public KepStateEnumeration Type
        {
            get { return _type; }
            set
            {
                _type = value;
                OnPropertyChange(nameof(Type));
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
        public decimal Zaduzenje
        {
            get { return _zaduzenje; }
            set
            {
                _zaduzenje = value;
                OnPropertyChange(nameof(Zaduzenje));
            }
        }
        public decimal Razduzenje
        {
            get { return _razduzenje; }
            set
            {
                _razduzenje = value;
                OnPropertyChange(nameof(Razduzenje));
            }
        }
        public decimal Saldo
        {
            get { return _saldo; }
            set
            {
                _saldo = value;
                OnPropertyChange(nameof(Saldo));
            }
        }
        #endregion Properties
    }
}
