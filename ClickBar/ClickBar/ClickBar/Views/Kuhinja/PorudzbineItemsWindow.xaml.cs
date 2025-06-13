using ClickBar.Models.AppMain.Kuhinja;
using ClickBar.ViewModels;
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

namespace ClickBar.Views.Kuhinja
{
    /// <summary>
    /// Interaction logic for PorudzbineItemsWindow.xaml
    /// </summary>
    public partial class PorudzbineItemsWindow : Window
    {
        public PorudzbineItemsWindow(PorudzbinaKuhinja porudzbinaKuhinja)
        {
            InitializeComponent();
            DataContext = porudzbinaKuhinja;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
