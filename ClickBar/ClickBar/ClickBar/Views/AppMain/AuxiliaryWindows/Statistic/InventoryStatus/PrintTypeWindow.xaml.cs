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

namespace ClickBar.Views.AppMain.AuxiliaryWindows.Statistic.InventoryStatus
{
    /// <summary>
    /// Interaction logic for PrintTypeWindow.xaml
    /// </summary>
    public partial class PrintTypeWindow : Window
    {
        public PrintTypeWindow(InventoryStatusViewModel inventoryStatusViewModel)
        {
            InitializeComponent();
            this.DataContext = inventoryStatusViewModel;
        }
    }
}
