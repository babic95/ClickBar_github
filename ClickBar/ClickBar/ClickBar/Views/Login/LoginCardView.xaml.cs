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

namespace ClickBar.Views.Login
{
    /// <summary>
    /// Interaction logic for LoginCardView.xaml
    /// </summary>
    public partial class LoginCardView : UserControl
    {
        public LoginCardView()
        {
            InitializeComponent();
            Loaded += (sender, e) =>
    MoveFocus(new TraversalRequest(FocusNavigationDirection.Last));
        }
    }
}
