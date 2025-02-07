using ClickBar.ViewModels.AppMain.Statistic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ClickBar.Views.AppMain.AuxiliaryWindows.Statistic
{
    /// <summary>
    /// Interaction logic for AddNormWindow.xaml
    /// </summary>
    public partial class AddNormWindow : Window
    {
        public AddNormWindow(InventoryStatusViewModel inventoryStatusViewModel)
        {
            InitializeComponent();
            DataContext = inventoryStatusViewModel;

            inventoryStatusViewModel.WindowHelper = this;
        }
    }
}
