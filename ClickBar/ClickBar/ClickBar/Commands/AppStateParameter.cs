using ClickBar.Enums;
using ClickBar.ViewModels;
using ClickBar_Database_Drlja;
using ClickBar_DatabaseSQLManager;
using ClickBar_DatabaseSQLManager.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar.Commands
{
    public class AppStateParameter
    {
        public AppStateParameter(SqlServerDbContext dbContext,
            SqliteDrljaDbContext drljaDbContext,
            AppStateEnumerable state, 
            CashierDB loggedCashier,
            ViewModelBase viewModel = null)
        {
            DbContext = dbContext;
            DrljaDbContext = drljaDbContext;
            State = state;
            LoggedCashier = loggedCashier;
            ViewModel = viewModel;
        }
        public SqlServerDbContext DbContext { get; private set; }
        public SqliteDrljaDbContext DrljaDbContext { get; private set; }
        public AppStateEnumerable State { get; private set; }
        public CashierDB LoggedCashier { get; private set; }
        public ViewModelBase ViewModel { get; private set; }
    }
}
