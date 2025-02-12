using ClickBar.Enums;
using ClickBar.Models.TableOverview.Kuhinja;
using ClickBar.ViewModels;
using ClickBar.Views.TableOverview;
using ClickBar_Database_Drlja;
using ClickBar_Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ClickBar.Commands.TableOverview
{
    public class OpenItemsInNarudzbinaCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private TableOverviewViewModel _currentView;

        public OpenItemsInNarudzbinaCommand(TableOverviewViewModel currentView)
        {
            _currentView = currentView;
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
                Narudzbe narudzba = (Narudzbe)dataGridCellInfo.Item;
                if (narudzba != null)
                {
                    var stavkeNarudzbine = _currentView.DrljaDbContext.StavkeNarudzbine.Where(s => s.TR_BROJNARUDZBE == narudzba.BrojNarudzbe);

                    if (stavkeNarudzbine != null &&
                        stavkeNarudzbine.Any())
                    {
                        _currentView.CurrentNarudzba = narudzba;

                        KuhinjaStavkePorudzbine KuhinjaStavkePorudzbine = new KuhinjaStavkePorudzbine(_currentView);
                        KuhinjaStavkePorudzbine.ShowDialog();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Greska prilikom otvaranja stavki narudzbine", ex);
                MessageBox.Show("Greska prilikom otvaranja stavki narudzbine",
                    "Greska",
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);
            }
        }
    }
}