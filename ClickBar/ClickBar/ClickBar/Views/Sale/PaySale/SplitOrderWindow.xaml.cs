using ClickBar.ViewModels.Sale;
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

namespace ClickBar.Views.Sale.PaySale
{
    /// <summary>
    /// Interaction logic for SplitOrderWindow.xaml
    /// </summary>
    public partial class SplitOrderWindow : Window
    {
        public SplitOrderWindow(PaySaleViewModel paySaleViewModel)
        {
            InitializeComponent();
            DataContext = new SplitOrderViewModel(paySaleViewModel);
        }
    }
}
