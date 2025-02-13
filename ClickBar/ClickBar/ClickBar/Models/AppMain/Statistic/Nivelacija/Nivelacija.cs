using ClickBar.Enums.AppMain.Statistic;
using ClickBar.Models.Sale;
using ClickBar_DatabaseSQLManager;
using ClickBar_DatabaseSQLManager.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar.Models.AppMain.Statistic
{
    public class Nivelacija : ObservableObject
    {
        private string _id;
        private DateTime _nivelacijaDate;
        private int _counterNivelacije;
        private NivelacijaStateEnumeration _type;
        private string _nameNivelacije;
        private string? _description;
        private ObservableCollection<NivelacijaItem> _nivelacijaItems;

        public Nivelacija(SqlServerDbContext sqliteDbContext,
            NivelacijaDB nivelacijaDB)
        {
            Id = nivelacijaDB.Id;
            NivelacijaDate = Convert.ToDateTime(nivelacijaDB.DateNivelacije);
            CounterNivelacije = nivelacijaDB.Counter;
            NameNivelacije = $"NIVELACIJA_{nivelacijaDB.Counter}-{NivelacijaDate.Year}";
            Description = nivelacijaDB.Description;
            Type = (NivelacijaStateEnumeration)Enum.ToObject(typeof(NivelacijaStateEnumeration), nivelacijaDB.Type);

            NivelacijaItems = new ObservableCollection<NivelacijaItem>();

            var nivelacijaItems = sqliteDbContext.ItemsNivelacija.Where(item => item.IdNivelacija == nivelacijaDB.Id);

            if (nivelacijaItems != null &&
                nivelacijaItems.Any())
            {
                foreach(var nivelacijaItemDB in nivelacijaItems)
                {
                    var nivelacijaItem = new NivelacijaItem(sqliteDbContext, nivelacijaItemDB);

                    NivelacijaItems.Add(nivelacijaItem);
                }
            }
        }

        public Nivelacija(SqlServerDbContext sqliteDbContext,
            NivelacijaStateEnumeration type,
            DateTime? nivelacijaDate = null)
        {
            Id = Guid.NewGuid().ToString();
            NivelacijaDate = nivelacijaDate == null && !nivelacijaDate.HasValue ? DateTime.Now : nivelacijaDate.Value;

            var allNivelacijeInYear = sqliteDbContext.Nivelacijas.Where(nivelacija => nivelacija.DateNivelacije.Year == NivelacijaDate.Year);

            int counterNivelacije = 1;

            if (allNivelacijeInYear != null &&
                allNivelacijeInYear.Any())
            {
                var maxNivelacija = allNivelacijeInYear.Max(nivelacija => nivelacija.Counter);

                if (maxNivelacija > 0)
                {
                    counterNivelacije = maxNivelacija + 1;
                }
            }

            CounterNivelacije = counterNivelacije;
            NameNivelacije = $"NIVELACIJA_{counterNivelacije}-{NivelacijaDate.Year}";
            NivelacijaItems = new ObservableCollection<NivelacijaItem>();
            SetDefaultDescription(type);
            Type = type;
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
        public NivelacijaStateEnumeration Type
        {
            get { return _type; }
            set
            {
                _type = value;
                OnPropertyChange(nameof(Type));
            }
        }
        public DateTime NivelacijaDate
        {
            get { return _nivelacijaDate; }
            set
            {
                _nivelacijaDate = value;
                OnPropertyChange(nameof(NivelacijaDate));
            }
        }
        public int CounterNivelacije
        {
            get { return _counterNivelacije; }
            set
            {
                _counterNivelacije = value;
                OnPropertyChange(nameof(CounterNivelacije));
            }
        }
        public string NameNivelacije
        {
            get { return _nameNivelacije; }
            set
            {
                _nameNivelacije = value;
                OnPropertyChange(nameof(NameNivelacije));
            }
        }
        public string? Description
        {
            get { return _description; }
            set
            {
                _description = value;
                OnPropertyChange(nameof(Description));
            }
        }

        public ObservableCollection<NivelacijaItem> NivelacijaItems
        {
            get { return _nivelacijaItems; }
            set
            {
                _nivelacijaItems = value;
                OnPropertyChange(nameof(NivelacijaItems));
            }
        }
        private void SetDefaultDescription(NivelacijaStateEnumeration type)
        {
            switch (type)
            {
                case NivelacijaStateEnumeration.Nivelacija:
                    Description = "Ručna nivelacija";
                    break;
                case NivelacijaStateEnumeration.Kalkulacija:
                    Description = "Nivelacija iz kalkulacije";
                    break;
                case NivelacijaStateEnumeration.Popust:
                    Description = "Nivelacija prilikom popusta";
                    break;
                case NivelacijaStateEnumeration.Sve:
                    Description = "Nivelacija svih proizvoda";
                    break;
            }
        }
    }
}
