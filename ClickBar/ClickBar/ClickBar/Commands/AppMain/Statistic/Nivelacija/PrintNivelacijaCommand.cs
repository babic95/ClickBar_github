using ClickBar.ViewModels.AppMain.Statistic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using ClickBar.Models.AppMain.Statistic;
using ClickBar_Common.Models.Statistic.Nivelacija;
using ClickBar_Printer;

namespace ClickBar.Commands.AppMain.Statistic.Nivelacija
{
    public class PrintNivelacijaCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ViewNivelacijaViewModel _currentViewModel;

        public PrintNivelacijaCommand(ViewNivelacijaViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if(parameter != null)
            {
                var nivelacija = (Models.AppMain.Statistic.Nivelacija)parameter;

                NivelacijaGlobal nivelacijaGlobal = new NivelacijaGlobal()
                {
                    Id = nivelacija.Id,
                    CounterNivelacije = nivelacija.CounterNivelacije,
                    Description = nivelacija.Description,
                    NameNivelacije = nivelacija.NameNivelacije,
                    NivelacijaDate = nivelacija.NivelacijaDate,
                    NivelacijaItems = new List<NivelacijaItemGlobal>(),
                    Type = (int)nivelacija.Type,
                };

                nivelacija.NivelacijaItems.ToList().ForEach(item =>
                {
                    nivelacijaGlobal.NivelacijaItems.Add(new NivelacijaItemGlobal()
                    {
                        IdItem = item.IdItem,
                        Jm = item.Jm,
                        Marza = (item.NewPrice * 100) / item.OldPrice - 100,
                        Name = item.Name,
                        NewPrice = item.NewPrice,
                        NewTotalPDV = item.NewTotalPDV,
                        NewTotalValue = item.NewTotalValue,
                        OldPrice = item.OldPrice,
                        OldTotalPDV = item.OldTotalPDV,
                        OldTotalValue = item.OldTotalValue,
                        Quantity = item.Quantity,
                        StopaPDV = item.StopaPDV
                    });
                });

                PrinterManager.Instance.PrintNivelacija(_currentViewModel.DbContext, nivelacijaGlobal);
            }
        }
    }
}