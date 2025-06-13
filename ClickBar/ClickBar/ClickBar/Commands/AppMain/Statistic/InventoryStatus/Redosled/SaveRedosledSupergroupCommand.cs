using ClickBar.Models.Sale;
using ClickBar.ViewModels.AppMain.Statistic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using ClickBar_Logging;
using ClickBar.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ClickBar.Commands.AppMain.Statistic.InventoryStatus.Redosled
{
    public class SaveRedosledSupergroupCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private InventoryStatusViewModel _currentViewModel;

        public SaveRedosledSupergroupCommand(InventoryStatusViewModel currentViewModel)
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
                var result = MessageBox.Show("Da li ste sigurni da želite da sačuvate raspored nadgrupa?",
                    "Čuvanje",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    foreach (var s in _currentViewModel.RedosledSupergroups)
                    {
                        var supergroupDB = _currentViewModel.DbContext.Supergroups
                            .FirstOrDefault(sg => sg.Id == s.Id);

                        if (supergroupDB != null)
                        {
                            supergroupDB.Rb = s.Rb;

                            _currentViewModel.DbContext.Supergroups.Update(supergroupDB);
                        }
                    }
                    _currentViewModel.DbContext.SaveChanges();

                    var saleViewModel = _currentViewModel.ServiceProvider.GetRequiredService<SaleViewModel>();
                    saleViewModel.UpdateSaleViewModel();

                    MessageBox.Show("Raspored nadgrupa je uspešno sačuvan.",
                        "Uspešno",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    //_currentViewModel.RasporedWindow.Close();
                }
            }
            catch (Exception ex)
            {
                Log.Error("SaveRedosledSupergroupCommand -> Greska prilikom cuvanja rasporeda nadgupa: ", ex);
                MessageBox.Show("Desila se greška.\nObratite se serviseru!",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}