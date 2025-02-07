using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar_Database.Models;
using ClickBar_Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using ClickBar.ViewModels;

namespace ClickBar.Commands.AppMain.Statistic.Knjizenje
{
    public class CloseWindowCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ViewModelBase _currentViewModel;

        public CloseWindowCommand(ViewModelBase currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (_currentViewModel is KnjizenjeViewModel)
            {
                KnjizenjeViewModel knjizenjeViewModel = (KnjizenjeViewModel)_currentViewModel;

                if (knjizenjeViewModel.Window != null && knjizenjeViewModel.Window.IsActive)
                {
                    knjizenjeViewModel.Window.Close();
                }
            }
            else if (_currentViewModel is PregledPazaraViewModel)
            {
                PregledPazaraViewModel pregledPazaraViewModel = (PregledPazaraViewModel)_currentViewModel;

                if (pregledPazaraViewModel.Window != null && pregledPazaraViewModel.Window.IsActive)
                {
                    pregledPazaraViewModel.Window.Close();
                }
            }
            else
            {
                return;
            }
        }
    }
}