using ClickBar.Models.Sale;
using ClickBar_DatabaseSQLManager;
using ClickBar_DatabaseSQLManager.Models;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar.Models.AppMain.Statistic
{
    public class NivelacijaItem : ObservableObject
    {
        private decimal _marza;
        private decimal _newTotalValue;
        private decimal _oldTotalValue;
        private decimal _oldPrice;
        private decimal _newPrice;
        private decimal _quantity;
        private decimal _stopaPDV;
        private decimal _newTotalPDV;
        private decimal _oldTotalPDV;
        private string _name;
        private string _jm;
        private string _idItem;
        private decimal _lastImportPrice;

        public NivelacijaItem(SqlServerDbContext sqliteDbContext,
            ItemNivelacijaDB itemNivelacijaDB)
        {
            LastImportPrice = 0;

            var itemDB = sqliteDbContext.Items.Find(itemNivelacijaDB.IdItem);

            if (itemDB != null)
            {
                var calculationForItem = sqliteDbContext.Calculations.Join(sqliteDbContext.CalculationItems,
                    calculation => calculation.Id,
                    calculationItem => calculationItem.CalculationId,
                    (calculation, calculationItem) => new { Cal = calculation, CalItem = calculationItem })
                    .Where(cal => cal.CalItem.ItemId == itemDB.Id);

                if (calculationForItem != null &&
                    calculationForItem.Any())
                {
                    var lastCalculation = calculationForItem.ToList().MaxBy(cal => cal.Cal.CalculationDate);

                    if (lastCalculation != null &&
                        lastCalculation.CalItem.InputPrice > 0)
                    {
                        LastImportPrice = lastCalculation.CalItem.InputPrice;
                    }
                }

                StopaPDV = itemNivelacijaDB.StopaPDV;
                IdItem = itemDB.Id;
                Name = itemDB.Name;
                OldPrice = itemNivelacijaDB.OldUnitPrice;
                NewPrice = itemNivelacijaDB.NewUnitPrice;
                Quantity = itemNivelacijaDB.TotalQuantity;
                NewTotalValue = NewPrice * Quantity;
                OldTotalValue = OldPrice * Quantity;
                Jm = itemDB.Jm;
                NewTotalPDV = NewTotalValue - (NewTotalValue * 100 / (StopaPDV + 100));
                OldTotalPDV = OldTotalValue - (OldTotalValue * 100 / (StopaPDV + 100));
            }
        }

        public NivelacijaItem(SqlServerDbContext sqliteDbContext, 
            Item item)
        {
            StopaPDV = item.Label == "Ђ" || item.Label == "6" ? 20 :
                item.Label == "Е" || item.Label == "7" ? 10 :
                item.Label == "А" || item.Label == "1" ? 0 :
                item.Label == "Г" || item.Label == "4" ? 0 :
                item.Label == "Ж" || item.Label == "8" ? 19 :
                item.Label == "A" || item.Label == "31" ? 9 : 0;

            LastImportPrice = 0;

            var calculationForItem = sqliteDbContext.Calculations.Join(sqliteDbContext.CalculationItems,
                    calculation => calculation.Id,
                    calculationItem => calculationItem.CalculationId,
                    (calculation, calculationItem) => new { Cal = calculation, CalItem = calculationItem })
                    .Where(cal => cal.CalItem.ItemId == item.Id);

            if (calculationForItem != null &&
                calculationForItem.Any())
            {
                var lastCalculation = calculationForItem.ToList().MaxBy(cal => cal.Cal.CalculationDate);

                if (lastCalculation != null &&
                    lastCalculation.CalItem.InputPrice > 0)
                {
                    LastImportPrice = lastCalculation.CalItem.InputPrice;
                }
            }

            IdItem = item.Id;
            Name = item.Name;
            OldPrice = item.SellingUnitPrice;
            Quantity = item.Quantity;
            Jm = item.Jm;
            OldTotalValue = OldPrice * Quantity;
            OldTotalPDV = OldTotalValue - (OldTotalValue * 100 / (StopaPDV + 100));
        }

        public decimal Marza
        {
            get { return _marza; }
            set
            {
                _marza = value;
                OnPropertyChange(nameof(Marza));
            }
        }
        public string IdItem
        {
            get { return _idItem; }
            set
            {
                _idItem = value;
                OnPropertyChange(nameof(IdItem));
            }
        }
        public decimal LastImportPrice
        {
            get { return _lastImportPrice; }
            set
            {
                _lastImportPrice = value;
                OnPropertyChange(nameof(LastImportPrice));
            }
        }
        public decimal NewTotalValue
        {
            get { return _newTotalValue; }
            set
            {
                value = Decimal.Round(value, 2);
                _newTotalValue = value;
                OnPropertyChange(nameof(NewTotalValue));

                NewTotalPDV = Decimal.Round(NewTotalValue - (NewTotalValue * 100 / (StopaPDV + 100)), 2);
            }
        }
        public decimal OldTotalValue
        {
            get { return _oldTotalValue; }
            set
            {
                value = Decimal.Round(value, 2);
                _oldTotalValue = value;
                OnPropertyChange(nameof(OldTotalValue));

                OldTotalPDV = Decimal.Round(OldTotalValue - (OldTotalValue * 100 / (StopaPDV + 100)), 2);
            }
        }
        public decimal OldPrice
        {
            get { return _oldPrice; }
            set
            {
                value = Decimal.Round(value, 2);
                _oldPrice = value;
                OnPropertyChange(nameof(OldPrice));
            }
        }
        public decimal NewPrice
        {
            get { return _newPrice; }
            set
            {
                _newPrice = value;
                OnPropertyChange(nameof(NewPrice));

                NewTotalValue = NewPrice * Quantity;
                if (LastImportPrice > 0)
                {
                    Marza = Decimal.Round((value * 100) / LastImportPrice, 2);
                }
            }
        }
        public decimal Quantity
        {
            get { return _quantity; }
            set
            {
                value = Decimal.Round(value, 2);
                _quantity = value;
                OnPropertyChange(nameof(Quantity));
            }
        }
        public decimal StopaPDV
        {
            get { return _stopaPDV; }
            set
            {
                value = Decimal.Round(value, 2);
                _stopaPDV = value;
                OnPropertyChange(nameof(StopaPDV));
            }
        }
        public decimal NewTotalPDV
        {
            get { return _newTotalPDV; }
            set
            {
                value = Decimal.Round(value, 2);
                _newTotalPDV = value;
                OnPropertyChange(nameof(NewTotalPDV));
            }
        }
        public decimal OldTotalPDV
        {
            get { return _oldTotalPDV; }
            set
            {
                value = Decimal.Round(value, 2);
                _oldTotalPDV = value;
                OnPropertyChange(nameof(OldTotalPDV));
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
        public string Jm
        {
            get { return _jm; }
            set
            {
                _jm = value;
                OnPropertyChange(nameof(Jm));
            }
        }
    }
}
