using ClickBar.ViewModels.AppMain.Statistic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using ClickBar.Enums.AppMain.Statistic;
using ClickBar_DatabaseSQLManager.Models;
using ClickBar_DatabaseSQLManager;
using ClickBar_Logging;
using ClickBar.Models.AppMain.Statistic;

namespace ClickBar.Commands.AppMain.Statistic.PriceIncrease
{
    public class SetNocneCeneCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private PriceIncreaseViewModel _currentViewModel;

        public SetNocneCeneCommand(PriceIncreaseViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            try
            {
                var result = MessageBox.Show("Da li ste sigurni da želite da postavite na noćne cene?",
                    "Noćne cene",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var groupSirovine = _currentViewModel.DbContext.ItemGroups.FirstOrDefault(group => group.Name.ToLower() == "sirovine" ||
                    group.Name.ToLower() == "sirovina" || group.Name.ToLower() == "trosak" || group.Name.ToLower() == "troskovi");

                    IEnumerable<ItemDB> items;

                    if (_currentViewModel.CurrentGroup.Id == -1)
                    {
                        if (groupSirovine != null)
                        {
                            items = _currentViewModel.DbContext.Items.Where(item => item.IdItemGroup != groupSirovine.Id);
                        }
                        else
                        {
                            items = _currentViewModel.DbContext.Items;
                        }
                    }
                    else
                    {
                        if (groupSirovine != null)
                        {
                            items = _currentViewModel.DbContext.Items.Where(item => item.IdItemGroup == _currentViewModel.CurrentGroup.Id &&
                            item.IdItemGroup != groupSirovine.Id);
                        }
                        else
                        {
                            items = _currentViewModel.DbContext.Items.Where(item => item.IdItemGroup == _currentViewModel.CurrentGroup.Id);
                        }
                    }

                    if (items != null &&
                        items.Any())
                    {
#if CRNO
#else
                        var nivelacija = new Models.AppMain.Statistic.Nivelacija(_currentViewModel.DbContext, 
                            NivelacijaStateEnumeration.Sve);
                        decimal totalNivelacija = 0;
                        NivelacijaDB nivelacijaDB = new NivelacijaDB()
                        {
                            Id = nivelacija.Id,
                            Counter = nivelacija.CounterNivelacije,
                            DateNivelacije = nivelacija.NivelacijaDate,
                            Description = nivelacija.Description,
                            Type = (int)nivelacija.Type
                        };
                        _currentViewModel.DbContext.Nivelacijas.Add(nivelacijaDB);
                        _currentViewModel.DbContext.SaveChanges();
                        Log.Debug($"SetNocneCeneCommand - Postavka na nocne cene - Uspesno kreirana nivelacija {nivelacija.Id}");


#endif


                        items.ToList().ForEach(itemDB =>
                        {
#if CRNO
                                if (itemDB.SellingNocnaUnitPrice != itemDB.SellingUnitPrice &&
                                itemDB.SellingNocnaUnitPrice > 0)
                                {
                                    itemDB.SellingUnitPrice = itemDB.SellingNocnaUnitPrice;
                                    _currentViewModel.DbContext.Items.Update(itemDB);
                                }
#else
                            Models.Sale.Item item = new Models.Sale.Item(itemDB);
                            if (item.SellingUnitPrice != item.SellingNocnaUnitPrice &&
                            item.SellingNocnaUnitPrice > 0)
                            {
                                item.SellingUnitPrice = item.SellingNocnaUnitPrice;

                                AddNivelacijaItem(item, itemDB, nivelacija, ref totalNivelacija);
                            }
#endif
                        });

                        _currentViewModel.DbContext.SaveChanges();
                        MessageBox.Show("Uspešna izmena cena!", "Uspešno", MessageBoxButton.OK, MessageBoxImage.Information);
#if CRNO
#else
                        KepDB kepDB = new KepDB()
                        {
                            Id = Guid.NewGuid().ToString(),
                            KepDate = nivelacijaDB.DateNivelacije,
                            Type = (int)KepStateEnumeration.Nivelacija,
                            Razduzenje = 0,
                            Zaduzenje = totalNivelacija,
                            Description = $"Ručna nivelacija svih proizvoda 'Nivelacija_{nivelacijaDB.Counter}-{nivelacijaDB.DateNivelacije.Year}'"
                        };
                        _currentViewModel.DbContext.Kep.Add(kepDB);
                        _currentViewModel.DbContext.SaveChanges();
#endif
                    }
                    else
                    {
                        MessageBox.Show("Nema artikala u zadatoj grupi!", "Nema artikala", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    _currentViewModel.Total = 0;
                }
            }
            catch
            {
                MessageBox.Show("Desila se greška!\nPozovite proizvodjača!", "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void AddNivelacijaItem(Models.Sale.Item item,
            ItemDB itemDB,
            Models.AppMain.Statistic.Nivelacija nivelacija,
            ref decimal totalNivelacija)
        {
            try
            {
                var nivelacijaItem = new NivelacijaItem(_currentViewModel.DbContext, item);
                nivelacijaItem.OldPrice = itemDB.SellingUnitPrice;
                nivelacijaItem.NewPrice = item.SellingUnitPrice;

                ItemNivelacijaDB itemNivelacijaDB = new ItemNivelacijaDB()
                {
                    IdNivelacija = nivelacija.Id,
                    IdItem = nivelacijaItem.IdItem,
                    NewUnitPrice = nivelacijaItem.NewPrice,
                    OldUnitPrice = nivelacijaItem.OldPrice,
                    StopaPDV = nivelacijaItem.StopaPDV,
                    TotalQuantity = itemDB.TotalQuantity,
                };

                _currentViewModel.DbContext.ItemsNivelacija.Add(itemNivelacijaDB);

                itemDB.SellingUnitPrice = nivelacijaItem.NewPrice;
                _currentViewModel.DbContext.Items.Update(itemDB);

                _currentViewModel.DbContext.SaveChanges();

                Log.Debug($"SetNocneCeneCommand - AddNivelacijaItem - Uspesno sacuvan artikal {item.Id} za nivelaciju {nivelacija.Id}");

                totalNivelacija += (itemNivelacijaDB.NewUnitPrice - itemNivelacijaDB.OldUnitPrice) * itemNivelacijaDB.TotalQuantity;
            }
            catch (Exception ex)
            {
                Log.Error($"SetNocneCeneCommand - AddNivelacijaItem - greska prilikom cuvanja artikla {item.Id} - {item.Name} za nivelaciju {nivelacija.Id}", ex);
            }
        }
    }
}