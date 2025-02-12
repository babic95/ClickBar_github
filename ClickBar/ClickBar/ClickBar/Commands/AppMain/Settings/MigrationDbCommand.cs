using ClickBar_InputOutputExcelFiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using ClickBar_Database;
using ClickBar.ViewModels.AppMain;

namespace ClickBar.Commands.AppMain.Settings
{
    public class MigrationDbCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        SettingsViewModel _viewModel;
        public MigrationDbCommand(SettingsViewModel settingsViewModel)
        {
            _viewModel = settingsViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            DataMigration dataMigration = new DataMigration(_viewModel.DbContext);
            dataMigration.MigrateData();
        }
    }
}