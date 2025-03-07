using ClickBar.Commands.AppMain.Report;
using ClickBar.Commands.Login;
using ClickBar.Commands.Sale;
using ClickBar.Commands;
using ClickBar.Models.Sale;
using ClickBar.Models.TableOverview;
using ClickBar_Database_Drlja;
using ClickBar_DatabaseSQLManager;
using ClickBar_Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace ClickBar.ViewModels
{
    public class KuhinjaViewModel : ViewModelBase
    {
        #region Fields
        private IServiceProvider _serviceProvider;
        private readonly Lazy<UpdateCurrentAppStateViewModelCommand> _updateCurrentAppStateViewModelCommand;
        #endregion Fields

        #region Constructors
        public KuhinjaViewModel(IServiceProvider serviceProvider,
            IDbContextFactory<SqlServerDbContext> dbContextFactory)
        {
        }
        #endregion Constructors

        #region Internal Properties
        #endregion Internal Properties

        #region Properties
        #endregion Properties

        #region Commands
        #endregion Commands

        #region Public methods
        #endregion Public methods

        #region Internal methods
        #endregion Internal methods

        #region Private methods
        #endregion Private methods
    }
}