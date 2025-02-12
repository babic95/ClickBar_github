using ClickBar.ViewModels;
using System.Windows;

namespace ClickBar
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel _mainViewModel;

        public MainWindow(MainViewModel mainViewModel)
        {
            InitializeComponent();
            _mainViewModel = mainViewModel;
            _mainViewModel.Window = this; // Set the Window property here
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