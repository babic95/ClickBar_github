using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar.Views.AppMain.AuxiliaryWindows.Statistic;
using ClickBar_Database;
using ClickBar_Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClickBar.Commands.AppMain.Statistic.Norm
{
    public class OpenNormativWindowCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private InventoryStatusViewModel _currentViewModel;

        public OpenNormativWindowCommand(InventoryStatusViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            using (SqliteDbContext sqliteDbContext = new SqliteDbContext())
            {

                if (!_currentViewModel.Norma.Any())
                {
                    var norm = sqliteDbContext.Norms.Add(new NormDB());
                    RetryHelper.ExecuteWithRetry(() => { sqliteDbContext.SaveChanges(); });
                    _currentViewModel.CurrentNorm = Convert.ToInt32(norm.Property("Id").CurrentValue);
                }

                AddNormWindow addNormWindow = new AddNormWindow(_currentViewModel);
                addNormWindow.ShowDialog();
            }
        }
    }
}