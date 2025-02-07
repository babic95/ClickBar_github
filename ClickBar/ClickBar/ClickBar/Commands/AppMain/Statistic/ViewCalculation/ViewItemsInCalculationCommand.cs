using ClickBar.Models.AppMain.Statistic;
using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar.Views.AppMain.AuxiliaryWindows.Statistic.ViewCalculation;
using ClickBar_Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClickBar.Commands.AppMain.Statistic.ViewCalculation
{
    public class ViewItemsInCalculationCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ViewCalculationViewModel _currentViewModel;

        public ViewItemsInCalculationCommand(ViewCalculationViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            if(parameter is string)
            {
                string calculationId = (string)parameter;
                var calculation = _currentViewModel.Calculations.FirstOrDefault(c => c.Id == calculationId);

                if (calculation != null)
                {
                    using (SqliteDbContext sqliteDbContext = new SqliteDbContext())
                    {

                        var calculationDB = sqliteDbContext.Calculations.Find(calculationId);

                        if (calculationDB != null)
                        {
                            calculation.CalculationItems = await _currentViewModel.GetAllItemsInCalculation(calculationDB);
                            _currentViewModel.CurrentCalculation = calculation;
                            _currentViewModel.CurrentWindow = new ViewCalculationItemsWindow(_currentViewModel);
                            _currentViewModel.CurrentWindow.ShowDialog();
                        }
                    }
                }
            }
        }
    }
}