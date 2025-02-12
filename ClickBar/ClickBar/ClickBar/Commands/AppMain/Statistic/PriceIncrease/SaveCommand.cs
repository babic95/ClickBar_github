using ClickBar.Enums.AppMain.Statistic;
using ClickBar.Models.AppMain.Statistic;
using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar_DatabaseSQLManager;
using ClickBar_DatabaseSQLManager.Models;
using ClickBar_Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ClickBar.Commands.AppMain.Statistic.PriceIncrease
{
    public class SaveCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private PriceIncreaseViewModel _currentViewModel;

        public SaveCommand(PriceIncreaseViewModel currentViewModel)
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
                var result = MessageBox.Show("Da li ste sigurni da želite da promenite cene?", 
                    "Promena cena",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    if (_currentViewModel.Total != 0)
                    {
                        var groupSirovine = _currentViewModel.DbContext.ItemGroups.FirstOrDefault(group => group.Name.ToLower() == "sirovine");

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
                            Log.Debug($"SaveCommand - Povecanje svih cena - Uspesno sacuvana nivelacija {nivelacija.Id}");


#endif


                            items.ToList().ForEach(itemDB =>
                            {
#if CRNO
                                itemDB.SellingUnitPrice += _currentViewModel.Total;
                                _currentViewModel.DbContext.Items.Update(itemDB);
#else
                                Models.Sale.Item item = new Models.Sale.Item(itemDB);
                                item.SellingUnitPrice += _currentViewModel.Total;

                                AddNivelacijaItem(item, itemDB, nivelacija, ref totalNivelacija);
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
            }
            catch (Exception ex)
            {
                Log.Error("SaveCommand -> greska: ", ex);
                MessageBox.Show("Greška prilikom čuvanja u bazu!\nPozovite proizvodjaca!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
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

                Log.Debug($"SaveCommand - AddNivelacija - Uspesno sacuvan artikal {item.Id} za nivelaciju {nivelacija.Id}");

                totalNivelacija += (itemNivelacijaDB.NewUnitPrice - itemNivelacijaDB.OldUnitPrice) * itemNivelacijaDB.TotalQuantity;
            }
            catch (Exception ex)
            {
                Log.Error($"SaveCommand - AddNivelacija - greska prilikom cuvanja artikla {item.Id} - {item.Name} za nivelaciju {nivelacija.Id}", ex);
            }
        }
    }
}