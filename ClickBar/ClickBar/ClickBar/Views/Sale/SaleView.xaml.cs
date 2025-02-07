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

namespace ClickBar.Views.Sale
{
    /// <summary>
    /// Interaction logic for SaleView.xaml
    /// </summary>
    public partial class SaleView : UserControl
    {
        private Point initialTouchPoint;
        private double initialVerticalOffset;

        public SaleView()
        {
            InitializeComponent();
        }
        private void ScrollViewer_TouchDown(object sender, TouchEventArgs e)
        {
            ScrollViewer scrollViewer = sender as ScrollViewer;
            if (scrollViewer != null)
            {
                initialTouchPoint = e.GetTouchPoint(scrollViewer).Position;
                initialVerticalOffset = scrollViewer.VerticalOffset;
                scrollViewer.CaptureTouch(e.TouchDevice);
            }
        }

        private void ScrollViewer_TouchMove(object sender, TouchEventArgs e)
        {
            ScrollViewer scrollViewer = sender as ScrollViewer;
            if (scrollViewer != null && scrollViewer.IsMouseCaptured)
            {
                Point currentTouchPoint = e.GetTouchPoint(scrollViewer).Position;
                double deltaY = initialTouchPoint.Y - currentTouchPoint.Y;
                scrollViewer.ScrollToVerticalOffset(initialVerticalOffset + deltaY);
            }
        }

        private void ScrollViewer_TouchUp(object sender, TouchEventArgs e)
        {
            ScrollViewer scrollViewer = sender as ScrollViewer;
            if (scrollViewer != null)
            {
                scrollViewer.ReleaseTouchCapture(e.TouchDevice);
            }
        }

        private void ScrollViewer_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            ScrollViewer scrollViewer = sender as ScrollViewer;
            if (scrollViewer != null)
            {
                // Adjust the scroll offset based on the manipulation delta
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.DeltaManipulation.Translation.Y);

                // Prevent the event from being handled by parent controls
                e.Handled = true;
            }
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollViewer = sender as ScrollViewer;
            if (scrollViewer != null)
            {
                // Adjust the scroll offset based on the mouse wheel delta
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);

                // Prevent the event from being handled by parent controls
                e.Handled = true;
            }
        }
    }
}
