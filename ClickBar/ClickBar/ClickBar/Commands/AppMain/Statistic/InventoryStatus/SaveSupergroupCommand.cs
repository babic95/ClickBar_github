using ClickBar.Models.Sale;
using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar_Database;
using ClickBar_Database.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ClickBar.Commands.AppMain.Statistic.InventoryStatus
{
    public class SaveSupergroupCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private InventoryStatusViewModel _currentViewModel;

        public SaveSupergroupCommand(InventoryStatusViewModel currentViewModel)
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
                if (_currentViewModel.CurrentSupergroup != null)
                {
                    using (SqliteDbContext sqliteDbContext = new SqliteDbContext())
                    {

                        var supergroup = sqliteDbContext.Supergroups.FirstOrDefault(supergroup => supergroup.Id == _currentViewModel.CurrentSupergroup.Id);

                        if (supergroup != null)
                        {
                            var result = MessageBox.Show("Da li zaista želite da sačuvate izmene nadgrupe?",
                                "Izmena nadgrupe",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Question);

                            if (result == MessageBoxResult.Yes)
                            {
                                supergroup.Name = _currentViewModel.CurrentSupergroup.Name;
                                sqliteDbContext.Supergroups.Update(supergroup);
                            }
                            else
                            {
                                return;
                            }
                        }
                        else
                        {
                            var result = MessageBox.Show("Da li zaista želite da sačuvate novu nadgrupu?",
                                "Nova nadgrupa",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Question);

                            if (result == MessageBoxResult.Yes)
                            {
                                SupergroupDB supergroupDB = new SupergroupDB()
                                {
                                    Name = _currentViewModel.CurrentSupergroup.Name
                                };
                                sqliteDbContext.Supergroups.Add(supergroupDB);
                            }
                            else
                            {
                                return;
                            }
                        }
                        RetryHelper.ExecuteWithRetry(() => { sqliteDbContext.SaveChanges(); });

                        _currentViewModel.AllSupergroups = new ObservableCollection<Supergroup>();

                        sqliteDbContext.Supergroups.ForEachAsync(supergroup =>
                        {
                            _currentViewModel.AllSupergroups.Add(new Supergroup(supergroup.Id, supergroup.Name));
                        });

                        _currentViewModel.CurrentSupergroup = _currentViewModel.AllSupergroups.FirstOrDefault();

                        MessageBox.Show("Uspešno obavljeno!",
                                "Uspešno",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);

                        _currentViewModel.Window.Close();
                    }
                }
            }
            catch
            {
                MessageBox.Show("Greška prilikom kreiranja ili izmene nadgrupe!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}