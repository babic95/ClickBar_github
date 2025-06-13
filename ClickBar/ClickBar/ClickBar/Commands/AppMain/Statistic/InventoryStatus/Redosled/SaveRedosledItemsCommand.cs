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
    public class SaveRedosledItemsCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private InventoryStatusViewModel _currentViewModel;

        public SaveRedosledItemsCommand(InventoryStatusViewModel currentViewModel)
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
                var result = MessageBox.Show("Da li ste sigurni da želite da sačuvate raspored artikala?",
                    "Čuvanje",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    foreach (var it in _currentViewModel.RedosledItems)
                    {
                        var itemDB = _currentViewModel.DbContext.Items
                            .FirstOrDefault(i => i.Id == it.Id);

                        if (itemDB != null)
                        {
                            itemDB.Rb = it.Rb;

                            _currentViewModel.DbContext.Items.Update(itemDB);
                        }
                    }
                    _currentViewModel.DbContext.SaveChanges();

                    var saleViewModel = _currentViewModel.ServiceProvider.GetRequiredService<SaleViewModel>();
                    saleViewModel.UpdateSaleViewModel();

                    MessageBox.Show("Raspored artikala je uspešno sačuvan.",
                        "Uspešno",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    //_currentViewModel.RasporedWindow.Close();
                }
            }
            catch (Exception ex)
            {
                Log.Error("SaveRedosledItemsCommand -> Greska prilikom cuvanja rasporeda item-a: ", ex);
                MessageBox.Show("Desila se greška.\nObratite se serviseru!",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}