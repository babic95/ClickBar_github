using ClickBar.Models.AppMain.Kuhinja;
using ClickBar.ViewModels;
using ClickBar.Views.Kuhinja;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using ClickBar_Logging;

namespace ClickBar.Commands.Kuhinja.Report
{
    public class OpenWindowForReportCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private KuhinjaViewModel _currentViewModel;

        public OpenWindowForReportCommand(KuhinjaViewModel currentViewModel)
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
                KuhinjaReportWindow kuhinjaReportWindow = new KuhinjaReportWindow(_currentViewModel);
                kuhinjaReportWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                Log.Error("OpenWindowForReportCommand -> desila se greska prilikom otvaranja izvestaja: ", ex);
                MessageBox.Show("Desila se greška.\nObratite se serviseru.", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
