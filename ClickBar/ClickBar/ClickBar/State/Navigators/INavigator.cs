using ClickBar.ViewModels;
using ClickBar_DatabaseSQLManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClickBar.State.Navigators
{
    public enum CashierViewType
    {
        Report = 0,
        Settings = 1,
        Statistics = 2,
        Admin = 3
    }
    public interface INavigator
    {
        ViewModelBase CurrentViewModel { get; set; }
        CashierDB LoggedCashier { get; set; }
        ICommand UpdateCurrentViewModelCommand { get; }
    }
}
