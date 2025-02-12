using ClickBar.Commands.AppMain.Admin;
using ClickBar.Commands.AppMain.Statistic.Firma;
using ClickBar.Commands.AppMain.Statistic.KEP;
using ClickBar.Models.AppMain.Statistic;
using ClickBar_DatabaseSQLManager;
using ClickBar_DatabaseSQLManager.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;

namespace ClickBar.ViewModels.AppMain.Statistic
{
    public class FirmaViewModel : ViewModelBase
    {
        #region Fields
        private Firma _firma;
        private readonly IServiceProvider _serviceProvider; // Dodato za korišćenje IServiceProvider
        #endregion Fields

        #region Constructors
        public FirmaViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            DbContext = serviceProvider.GetRequiredService<SqlServerDbContext>();
            FirmaDB = DbContext.Firmas.FirstOrDefault();

            if (FirmaDB != null)
            {
                Firma = new Firma(FirmaDB);
            }
            else
            {
                Firma = new Firma();
            }
        }
        #endregion Constructors

        #region Properties internal
        internal SqlServerDbContext DbContext { get; private set; }
        internal FirmaDB? FirmaDB { get; set; }
        #endregion Properties internal

        #region Properties
        public Firma Firma
        {
            get { return _firma; }
            set
            {
                _firma = value;
                OnPropertyChange(nameof(Firma));
            }
        }
        #endregion Properties

        #region Commands
        public ICommand SaveFirmaCommand => new SaveFirmaCommand(this);
        #endregion Commands

        #region Public methods
        #endregion Public methods

        #region Private methods
        #endregion Private methods
    }
}