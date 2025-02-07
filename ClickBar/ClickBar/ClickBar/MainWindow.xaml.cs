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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClickBar
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel _mainViewModel;
        public MainWindow()
        {
            InitializeComponent();
            _mainViewModel = new MainViewModel(this);
            DataContext = _mainViewModel;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_mainViewModel is not null && !_mainViewModel.IsExit)
            {
                _mainViewModel.HiddenWindowCommand.Execute(null);
                e.Cancel = true;
            }
        }
    }
}
