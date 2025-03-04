﻿using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar.Views.AppMain.AuxiliaryWindows.Statistic;
using ClickBar_DatabaseSQLManager;
using ClickBar_DatabaseSQLManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClickBar.Commands.AppMain.Statistic.Norm
{
    public class OpenNormativWindowCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private InventoryStatusViewModel _currentViewModel;

        public OpenNormativWindowCommand(InventoryStatusViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (!_currentViewModel.Norma.Any())
            {
                var norm = _currentViewModel.DbContext.Norms.Add(new NormDB());
                _currentViewModel.DbContext.SaveChanges();
                _currentViewModel.CurrentNorm = Convert.ToInt32(norm.Property("Id").CurrentValue);
            }

            AddNormWindow addNormWindow = new AddNormWindow(_currentViewModel);
            addNormWindow.ShowDialog();
        }
    }
}