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

namespace ClickBar.Views.Kuhinja
{
    /// <summary>
    /// Interaction logic for KuhinjaView.xaml
    /// </summary>
    public partial class KuhinjaView : UserControl
    {
        public KuhinjaView()
        {
            InitializeComponent();
        }

        private void DataGridCellFinish_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && sender is TextBlock textBlock)
            {
                var dataGridCell = textBlock.Parent as DataGridCell;
                if (dataGridCell != null)
                {
                    var dataGrid = FindVisualParent<DataGrid>(dataGridCell);
                    if (dataGrid != null)
                    {
                        var cellInfo = new DataGridCellInfo(dataGridCell);
                        if (DataContext is KuhinjaViewModel viewModel)
                        {
                            if (viewModel.FinishPorudzbinaCommand.CanExecute(cellInfo))
                            {
                                viewModel.FinishPorudzbinaCommand.Execute(cellInfo);
                            }
                        }
                    }
                }
            }
        }

        private void DataGridCellOpenItems_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && sender is TextBlock textBlock)
            {
                var dataGridCell = textBlock.Parent as DataGridCell;
                if (dataGridCell != null)
                {
                    var dataGrid = FindVisualParent<DataGrid>(dataGridCell);
                    if (dataGrid != null)
                    {
                        var cellInfo = new DataGridCellInfo(dataGridCell);
                        if (DataContext is KuhinjaViewModel viewModel)
                        {
                            if (viewModel.ViewItemsCommand.CanExecute(cellInfo))
                            {
                                viewModel.ViewItemsCommand.Execute(cellInfo);
                            }
                        }
                    }
                }
            }
        }

        private static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            var parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null)
                return null;

            var parent = parentObject as T;
            return parent ?? FindVisualParent<T>(parentObject);
        }
    }
}
