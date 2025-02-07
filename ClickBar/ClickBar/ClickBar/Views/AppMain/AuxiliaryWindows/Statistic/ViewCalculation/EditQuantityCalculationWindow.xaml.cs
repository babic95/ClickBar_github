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

namespace ClickBar.Views.AppMain.AuxiliaryWindows.Statistic.ViewCalculation
{
    /// <summary>
    /// Interaction logic for EditQuantityCalculationWindow.xaml
    /// </summary>
    public partial class EditQuantityCalculationWindow : Window
    {
        public EditQuantityCalculationWindow(ViewCalculationViewModel currentViewModel)
        {
            InitializeComponent();
            DataContext = currentViewModel;
        }
    }
}
