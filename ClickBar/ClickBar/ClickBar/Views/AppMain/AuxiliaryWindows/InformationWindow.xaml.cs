using ClickBar_Settings;
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

namespace ClickBar.Views.AppMain.AuxiliaryWindows
{
    /// <summary>
    /// Interaction logic for InformationWindow.xaml
    /// </summary>
    public partial class InformationWindow : Window
    {
        public InformationWindow()
        {
            InitializeComponent();

            SerialNumber.Text = SettingsManager.Instance.GetPosNumber();
            Version.Text = SettingsManager.Instance.GetPosVersion();
            ManufacturerName.Text = SettingsManager.Instance.GetPosMake();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        //private async void Button_Click_1(object sender, RoutedEventArgs e)
        //{
        //    var status = await ApiManager.Instance.GetStatus();

        //    if (status != null)
        //    {
        //        if (status is Status)
        //        {
        //            Status s = (Status)status;
        //            if (s.CurrentTaxRates != null)
        //            {
        //                LabelsFromPFR labelsFromPFR = new LabelsFromPFR((Status)status);
        //                labelsFromPFR.ShowDialog();
        //            }
        //            else
        //            {
        //                PinWindow pinWindow = new PinWindow();
        //                pinWindow.ShowDialog();

        //                status = await ApiManager.Instance.GetStatus();
        //                if (status != null)
        //                {
        //                    if (status is Status)
        //                    {
        //                        s = (Status)status;
        //                        if (s.CurrentTaxRates != null)
        //                        {
        //                            LabelsFromPFR labelsFromPFR = new LabelsFromPFR((Status)status);
        //                            labelsFromPFR.ShowDialog();
        //                        }
        //                        else
        //                        {
        //                            MessageBox.Show("Greška u komunikaciji sa L-PFR-om, proverite Vaš L-PFR", "", MessageBoxButton.OK, MessageBoxImage.Warning);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        MessageBox.Show("Greška u komunikaciji sa L-PFR-om, proverite Vaš L-PFR", "", MessageBoxButton.OK, MessageBoxImage.Warning);
        //                    }
        //                }
        //                else
        //                {
        //                    MessageBox.Show("Greška u komunikaciji sa L-PFR-om, proverite Vaš L-PFR", "", MessageBoxButton.OK, MessageBoxImage.Warning);
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        MessageBox.Show("Greška u komunikaciji sa L-PFR-om, proverite Vaš L-PFR", "", MessageBoxButton.OK, MessageBoxImage.Warning);
        //    }
        //}
    }
}
