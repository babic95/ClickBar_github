using ClickBar.Enums.AppMain.Statistic;
using ClickBar.Models.AppMain.Statistic;
using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar_Common.Enums;
using ClickBar_DatabaseSQLManager;
using ClickBar_DatabaseSQLManager.Models;
using ClickBar_Logging;
using ClickBar_Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ClickBar.Commands.AppMain.Statistic.Nivelacija
{
    public class SaveNivelacijaCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private NivelacijaViewModel _currentViewModel;

        public SaveNivelacijaCommand(NivelacijaViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            MessageBoxResult result = MessageBox.Show("Da li ste sigurni da želite da kreirate nivelaciju?",
                           "Kreiranje nivelacije",
                           MessageBoxButton.YesNo,
                           MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    bool onlySirovine = true;

                    decimal totalNivelacija = 0;

                    NivelacijaDB nivelacijaDB = new NivelacijaDB()
                    {
                        Id = _currentViewModel.CurrentNivelacija.Id,
                        Counter = _currentViewModel.CurrentNivelacija.CounterNivelacije,
                        DateNivelacije = _currentViewModel.CurrentNivelacija.NivelacijaDate,
                        Description = _currentViewModel.CurrentNivelacija.Description,
                        Type = (int)_currentViewModel.CurrentNivelacija.Type
                    };

                    _currentViewModel.DbContext.Nivelacijas.Add(nivelacijaDB);
                    _currentViewModel.DbContext.SaveChanges();

                    _currentViewModel.CurrentNivelacija.NivelacijaItems.ToList().ForEach(item =>
                    {
                        var itemDB = _currentViewModel.DbContext.Items.Find(item.IdItem);
                        if (itemDB != null)
                        {
                            var group = _currentViewModel.DbContext.ItemGroups.Find(itemDB.IdItemGroup);

                            if (group == null)
                            {
                                MessageBox.Show("ARTIKAL MORA DA PRIPADA NEKOJ GRUPI!",
                                    "Greška",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);

                                return;
                            }
                            if (group.Name.ToLower().Contains("sirovine") ||
                                group.Name.ToLower().Contains("sirovina"))
                            {
                            }
                            else
                            {
                                onlySirovine = false;
                                ItemNivelacijaDB itemNivelacijaDB = new ItemNivelacijaDB()
                                {
                                    IdNivelacija = _currentViewModel.CurrentNivelacija.Id,
                                    IdItem = item.IdItem,
                                    NewUnitPrice = item.NewPrice,
                                    OldUnitPrice = item.OldPrice,
                                    StopaPDV = item.StopaPDV,
                                    TotalQuantity = item.Quantity,
                                };
                                _currentViewModel.DbContext.ItemsNivelacija.Add(itemNivelacijaDB);

                                itemDB.SellingUnitPrice = item.NewPrice;
                                _currentViewModel.DbContext.Items.Update(itemDB);

                                totalNivelacija += (itemNivelacijaDB.NewUnitPrice - itemNivelacijaDB.OldUnitPrice) * itemNivelacijaDB.TotalQuantity;
                            }
                        }
                    });

                    if (onlySirovine)
                    {
                        MessageBox.Show("Nivelacija ne može da se radi samo na SIROVINE!", "Nivelacija sirovina!", MessageBoxButton.OK, MessageBoxImage.Warning);
                        _currentViewModel.DbContext.Nivelacijas.Remove(nivelacijaDB);
                        _currentViewModel.DbContext.SaveChanges();
                    }
                    else
                    {
                        _currentViewModel.DbContext.SaveChanges();

                        KepDB kepDB = new KepDB()
                        {
                            Id = Guid.NewGuid().ToString(),
                            KepDate = nivelacijaDB.DateNivelacije,
                            Type = (int)KepStateEnumeration.Nivelacija,
                            Razduzenje = 0,
                            Zaduzenje = totalNivelacija,
                            Description = $"Ručna nivelacija 'Nivelacija_{nivelacijaDB.Counter}-{nivelacijaDB.DateNivelacije.Year}'"
                        };
                        _currentViewModel.DbContext.Kep.Add(kepDB);
                        _currentViewModel.DbContext.SaveChanges();

                        Log.Debug($"SaveNivelacijaCommand - Uspesno sacuvana nivelacija {_currentViewModel.CurrentNivelacija.Id}");
                        MessageBox.Show("Uspešno ste kreirali nivelaciju!", "Uspešno!", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    _currentViewModel.Reset();
                }
                catch (Exception ex)
                {
                    Log.Error("SaveNivelacijaCommand - Greska prilikom cuvanja nivelacije u bazu -> ", ex);
                }
            }
        }
    }
}