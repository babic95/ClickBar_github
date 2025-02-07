using ClickBar.ViewModels;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClickBar.Views.AppMain.Statistic
{
    /// <summary>
    /// Interaction logic for PocetnoStanjeView.xaml
    /// </summary>
    public partial class PocetnoStanjeView : UserControl
    {
        public PocetnoStanjeView()
        {
            InitializeComponent();
            UpdatePlaceholderVisibility();
        }

        private void searchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (DataContext is PocetnoStanjeViewModel viewModel)
            {
                viewModel.SearchText = searchTextBox.Text;
                UpdatePlaceholderVisibility();
            }
        }
        private void UpdatePlaceholderVisibility()
        {
            placeholderText.Visibility = string.IsNullOrEmpty(searchTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
