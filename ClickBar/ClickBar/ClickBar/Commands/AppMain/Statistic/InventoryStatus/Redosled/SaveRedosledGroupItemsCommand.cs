using ClickBar.ViewModels.AppMain.Statistic;
using System;
using System.Collections.Generic;
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
    public class SaveRedosledGroupItemsCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private InventoryStatusViewModel _currentViewModel;

        public SaveRedosledGroupItemsCommand(InventoryStatusViewModel currentViewModel)
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
                var result = MessageBox.Show("Da li ste sigurni da želite da sačuvate raspored grupa?",
                    "Čuvanje",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    foreach (var s in _currentViewModel.RedosledGroups)
                    {
                        var groupDB = _currentViewModel.DbContext.ItemGroups
                            .FirstOrDefault(g => g.Id == s.Id);

                        if (groupDB != null)
                        {
                            groupDB.Rb = s.Rb;

                            _currentViewModel.DbContext.ItemGroups.Update(groupDB);
                        }
                    }
                    _currentViewModel.DbContext.SaveChanges();

                    var saleViewModel = _currentViewModel.ServiceProvider.GetRequiredService<SaleViewModel>();
                    saleViewModel.UpdateSaleViewModel();

                    MessageBox.Show("Raspored grupa je uspešno sačuvan.",
                        "Uspešno",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    //_currentViewModel.RasporedWindow.Close();
                }
            }
            catch (Exception ex)
            {
                Log.Error("SaveRedosledGroupItemsCommand -> Greska prilikom cuvanja rasporeda grupa: ", ex);
                MessageBox.Show("Desila se greška.\nObratite se serviseru!",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}