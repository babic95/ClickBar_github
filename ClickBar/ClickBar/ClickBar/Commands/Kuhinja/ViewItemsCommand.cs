using ClickBar.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using ClickBar_Logging;
using ClickBar.Models.AppMain.Kuhinja;
using System.Windows.Controls;
using ClickBar.Views.Kuhinja;

namespace ClickBar.Commands.Kuhinja
{
    public class ViewItemsCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private KuhinjaViewModel _currentViewModel;

        public ViewItemsCommand(KuhinjaViewModel currentViewModel)
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
                DataGridCellInfo dataGridCellInfo = (DataGridCellInfo)parameter;
                PorudzbinaKuhinja porudzbinaKuhinja = (PorudzbinaKuhinja)dataGridCellInfo.Item;

                PorudzbineItemsWindow porudzbineItemsWindow = new PorudzbineItemsWindow(porudzbinaKuhinja);
                porudzbineItemsWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                Log.Error("ViewItemsCommand -> desila se greska prilikom otvaranja porudzbine: ", ex);
                MessageBox.Show("Desila se greška.\nObratite se serviseru.", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
