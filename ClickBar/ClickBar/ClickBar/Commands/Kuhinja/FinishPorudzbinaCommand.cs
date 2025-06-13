using ClickBar.Enums;
using ClickBar.ViewModels.Login;
using ClickBar_Common.Enums;
using ClickBar_Settings;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using ClickBar.ViewModels;
using ClickBar_Logging;
using ClickBar.Enums.Kuhinja;
using ClickBar.Models.AppMain.Kuhinja;
using Microsoft.EntityFrameworkCore;
using System.Windows.Controls;

namespace ClickBar.Commands.Kuhinja
{
    public class FinishPorudzbinaCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private KuhinjaViewModel _currentViewModel;

        public FinishPorudzbinaCommand(KuhinjaViewModel currentViewModel)
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

                using (var dbContext = _currentViewModel.DbContext.CreateDbContext())
                {
                    var orderDB = dbContext.OrdersToday.FirstOrDefault(o => o.Id == porudzbinaKuhinja.OrderTodayDB.Id);

                    if (orderDB != null)
                    {
                        orderDB.Faza = (int)FazaKuhinjeEnumeration.Uradjena;
                        dbContext.SaveChanges();
                    }
                }

                _currentViewModel.Porudzbine.Remove(porudzbinaKuhinja);
            }
            catch(Exception ex)
            {
                Log.Error("FinishPorudzbinaCommand -> desila se greska prilikom zavrsavanje porudzbine: ", ex);
                MessageBox.Show("Desila se greška.\nObratite se serviseru.", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
