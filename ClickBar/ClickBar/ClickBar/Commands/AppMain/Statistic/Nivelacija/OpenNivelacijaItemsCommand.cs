using ClickBar.Models.AppMain.Statistic;
using ClickBar.ViewModels.AppMain.Statistic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace ClickBar.Commands.AppMain.Statistic.Nivelacija
{
    public class OpenNivelacijaItemsCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ViewNivelacijaViewModel _currentViewModel;

        public OpenNivelacijaItemsCommand(ViewNivelacijaViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if(parameter is string)
            {
                var nivelacijaId = (string) parameter;

                if(!string.IsNullOrEmpty(nivelacijaId) )
                {
                    var nivelacija = _currentViewModel.Nivelacije.FirstOrDefault(nivelacija => nivelacija.Id == nivelacijaId);

                    if(nivelacija != null )
                    {
                        _currentViewModel.TotalNivelacija = 0;
                        _currentViewModel.TotalNewNivelacija = 0;
                        _currentViewModel.TotalOldNivelacija = 0;
                        _currentViewModel.TotalPdvNivelacija = 0;
                        _currentViewModel.CurrentNivelacija = nivelacija;

                        nivelacija.NivelacijaItems.ToList().ForEach(item =>
                        {
                            _currentViewModel.TotalNivelacija += item.NewTotalValue - item.OldTotalValue;
                            _currentViewModel.TotalNewNivelacija += item.NewTotalValue;
                            _currentViewModel.TotalOldNivelacija += item.OldTotalValue;
                            _currentViewModel.TotalPdvNivelacija += item.NewTotalPDV - item.OldTotalPDV;
                        });
                    }
                }
            }
        }
    }
}