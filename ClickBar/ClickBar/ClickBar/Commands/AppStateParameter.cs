using ClickBar.Enums;
using ClickBar.ViewModels;
using ClickBar_Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar.Commands
{
    public class AppStateParameter
    {
        public AppStateParameter(AppStateEnumerable state, CashierDB loggedCashier, ViewModelBase viewModel = null)
        {
            State = state;
            LoggedCashier = loggedCashier;
            ViewModel = viewModel;
        }
        public AppStateEnumerable State { get; private set; }
        public CashierDB LoggedCashier { get; private set; }
        public ViewModelBase ViewModel { get; private set; }
    }
}
