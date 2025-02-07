using ClickBar.ViewModels.AppMain;
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

namespace ClickBar.Views.AppMain.AuxiliaryWindows.ReportWindows
{
    /// <summary>
    /// Interaction logic for PeriodicreportWindow.xaml
    /// </summary>
    public partial class PeriodicreportWindow : Window
    {
        public PeriodicreportWindow(ReportViewModel reportViewModel)
        {
            InitializeComponent();

            DataContext = reportViewModel;
        }
    }
}
