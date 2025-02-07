using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ClickBar.Commands
{
    public class HiddenWindowCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private Window _window;

        public HiddenWindowCommand(Window window)
        {
            _window = window;
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            if (_window.Visibility == Visibility.Hidden)
            {
                _window.Visibility = Visibility.Visible;
            }
            else
            {
                _window.Visibility = Visibility.Hidden;
            }
        }
    }
}
