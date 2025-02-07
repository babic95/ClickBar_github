using ClickBar.Converters;
using ClickBar.Models.TableOverview;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClickBar.Views.AppMain
{
    /// <summary>
    /// Interaction logic for AdminView.xaml
    /// </summary>
    public partial class AdminView : UserControl
    {
        private static UIElement _paymentPlace;

        public static readonly DependencyProperty IsChildHitTestVisibleProperty =
            DependencyProperty.Register("IsChildHitTestVisible", typeof(bool), typeof(AdminView),
                new PropertyMetadata(true));

        public bool IsChildHitTestVisible
        {
            get { return (bool)GetValue(IsChildHitTestVisibleProperty); }
            set { SetValue(IsChildHitTestVisibleProperty, value); }
        }
        public static readonly DependencyProperty RectangleDropCommandProperty =
            DependencyProperty.Register("RectangleDropCommand", typeof(ICommand), typeof(AdminView),
                new PropertyMetadata(null));

        public ICommand RectangleDropCommand
        {
            get { return (ICommand)GetValue(RectangleDropCommandProperty); }
            set { SetValue(RectangleDropCommandProperty, value); }
        }

        public static readonly DependencyProperty RectangleRemoveCommandProperty =
            DependencyProperty.Register("RectangleRemoveCommand", typeof(ICommand), typeof(AdminView),
                new PropertyMetadata(null));

        public ICommand RectangleRemoveCommand
        {
            get { return (ICommand)GetValue(RectangleRemoveCommandProperty); }
            set { SetValue(RectangleRemoveCommandProperty, value); }
        }

        public static readonly DependencyProperty RemoveRectangleNameProperty =
            DependencyProperty.Register("RemoveRectangleName", typeof(string), typeof(AdminView),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string RemoveRectangleName
        {
            get { return (string)GetValue(RemoveRectangleNameProperty); }
            set { SetValue(RemoveRectangleNameProperty, value); }
        }
        public AdminView()
        {
            InitializeComponent();
        }

        private void canvas_DragOver(object sender, DragEventArgs e)
        {
            object data = e.Data.GetData(DataFormats.Serializable);
            if (data is FrameworkElement element)
            {
                var dataContext = DataContext as AdminViewModel;

                if (dataContext != null)
                {
                    if (!dataContext.Change)
                    {
                        dataContext.Change = true;
                    }

                    PaymentPlace? paymentPlace = dataContext.NormalPaymentPlaces.FirstOrDefault(place => place.Id == Convert.ToInt32(element.Tag));

                    if(paymentPlace == null)
                    {
                        paymentPlace = dataContext.RoundPaymentPlaces.FirstOrDefault(place => place.Id == Convert.ToInt32(element.Tag));
                    }

                    if (paymentPlace != null)
                    {
                        Point dropPosition = e.GetPosition(canvas);
                        paymentPlace.Left = WinUtil.Pixel2Mm(dropPosition.X);
                        paymentPlace.Top = WinUtil.Pixel2Mm(dropPosition.Y);
                    }
                }
            }
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _paymentPlace = sender as UIElement;
                IsChildHitTestVisible = false;
                DragDrop.DoDragDrop(_paymentPlace, new DataObject(DataFormats.Serializable, _paymentPlace), DragDropEffects.Move);
                IsChildHitTestVisible = true;
            }
        }
    }
}
