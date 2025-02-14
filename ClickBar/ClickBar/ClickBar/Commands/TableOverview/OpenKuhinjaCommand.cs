using ClickBar.Enums;
using ClickBar.Models.TableOverview.Kuhinja;
using ClickBar.ViewModels;
using ClickBar.Views.TableOverview;
using ClickBar_Database_Drlja;
using ClickBar_Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ClickBar.Commands.TableOverview
{
    public class OpenKuhinjaCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private TableOverviewViewModel _currentView;

        public OpenKuhinjaCommand(TableOverviewViewModel currentView)
        {
            _currentView = currentView;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (_currentView.DrljaDbContextFactory != null)
            {
                try
                {
                    _currentView.SetKuhinjaNarudzbine();

                    KuhinjaPorudzbineWindow kuhinjaPorudzbineWindow = new KuhinjaPorudzbineWindow(_currentView);
                    kuhinjaPorudzbineWindow.ShowDialog();

                }
                catch (Exception ex)
                {
                    Log.Error("OpenKuhinjaCommand -> Execute -> Greska prilikom otvaranja kuhinje: ", ex);
                    MessageBox.Show("Došlo je do greške prilikom otvaranja kuhinje!", "", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}