using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar_DatabaseSQLManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ClickBar.Commands.AppMain.Statistic.InventoryStatus
{
    public class DeleteZeljaCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private InventoryStatusViewModel _currentViewModel;

        public DeleteZeljaCommand(InventoryStatusViewModel currentViewModel)
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
                if (parameter is string &&
                    _currentViewModel.CurrentInventoryStatus != null &&
                    _currentViewModel.CurrentInventoryStatus.Item != null)
                {
                    string idZelje = (string)parameter;

                    var result = MessageBox.Show("Da li zaista želite da obrišete želju iz artikla?",
                        "Brisanje želje",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        var zeljaDB = _currentViewModel.DbContext.Zelje.FirstOrDefault(z => z.Id == idZelje);

                        if (zeljaDB != null)
                        {
                            _currentViewModel.DbContext.Zelje.Remove(zeljaDB);
                            _currentViewModel.DbContext.SaveChanges();
                        }

                        var zeljaZaBrisanje = _currentViewModel.Zelje.FirstOrDefault(z => z.Id == idZelje);

                        if (zeljaZaBrisanje != null)
                        {
                            _currentViewModel.Zelje.Remove(zeljaZaBrisanje);
                        }
                    }
                }
            }
            catch
            {
                MessageBox.Show("Greška prilikom brisanja normativa iz artikla!", "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}