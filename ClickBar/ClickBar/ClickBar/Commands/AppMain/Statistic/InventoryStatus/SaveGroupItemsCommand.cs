﻿using ClickBar.Models.Sale;
using ClickBar.ViewModels;
using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar_DatabaseSQLManager;
using ClickBar_DatabaseSQLManager.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
    public class SaveGroupItemsCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private InventoryStatusViewModel _currentViewModel;

        public SaveGroupItemsCommand(InventoryStatusViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            try
            {
                if (_currentViewModel.CurrentGroupItems != null)
                {
                    if (_currentViewModel.CurrentSupergroup == null)
                    {
                        MessageBox.Show("Grupa artikala mora da pripada nekoj nadgrupi!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);

                        return;
                    }

                    var groupItems = _currentViewModel.DbContext.ItemGroups.FirstOrDefault(group => group.Id == _currentViewModel.CurrentGroupItems.Id);

                    if (groupItems != null)
                    {
                        var result = MessageBox.Show("Da li zaista želite da sačuvate izmene grupe artikala?",
                            "Izmena grupe artikala",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

                        if (result == MessageBoxResult.Yes)
                        {
                            groupItems.IdSupergroup = _currentViewModel.CurrentSupergroup.Id;
                            groupItems.Name = _currentViewModel.CurrentGroupItems.Name;
                            _currentViewModel.DbContext.ItemGroups.Update(groupItems);
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        var result = MessageBox.Show("Da li zaista želite da sačuvate novu grupu artikala?",
                            "Nova grupa artikala",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

                        if (result == MessageBoxResult.Yes)
                        {
                            var max = 0;

                            if(_currentViewModel.DbContext.ItemGroups.Count() > 0)
                            {
                                max = _currentViewModel.DbContext.ItemGroups.Max(group => group.Id);
                            }

                            ItemGroupDB itemGroupDB = new ItemGroupDB()
                            {
                                IdSupergroup = _currentViewModel.CurrentSupergroup.Id,
                                Name = _currentViewModel.CurrentGroupItems.Name,
                                Rb = max + 1
                            };
                            _currentViewModel.DbContext.ItemGroups.Add(itemGroupDB);
                        }
                        else
                        {
                            return;
                        }
                    }
                    _currentViewModel.DbContext.SaveChanges();

                    _currentViewModel.AllGroupItems = new ObservableCollection<GroupItems>();
                    _currentViewModel.AllGroups = new ObservableCollection<GroupItems>() { new GroupItems()
                    {
                        Id = -1,
                        IdSupergroup = -1,
                        Name = "Sve grupe"
                    } };

                    foreach(var gropu in _currentViewModel.DbContext.ItemGroups)
                    {
                        _currentViewModel.AllGroupItems.Add(new GroupItems(gropu));
                        _currentViewModel.AllGroups.Add(new GroupItems(gropu));
                    }

                    _currentViewModel.CurrentGroupItems = _currentViewModel.AllGroupItems.FirstOrDefault();
                    _currentViewModel.CurrentGroup = _currentViewModel.AllGroups.FirstOrDefault();

                    var saleViewModel = _currentViewModel.ServiceProvider.GetRequiredService<SaleViewModel>();
                    saleViewModel.UpdateSaleViewModel();

                    MessageBox.Show("Uspešno obavljeno!",
                            "Uspešno",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

                    _currentViewModel.Window.Close();
                }
            }
            catch
            {
                MessageBox.Show("Greška prilikom kreiranja ili izmene grupe artikala!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}