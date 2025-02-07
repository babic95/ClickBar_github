using ClickBar.Models.AppMain.Statistic;
using ClickBar.Models.Sale;
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

namespace ClickBar.Views.Sale
{
    /// <summary>
    /// Interaction logic for ItemInvoiceAddZeljeWindow.xaml
    /// </summary>
    public partial class ItemInvoiceAddZeljeWindow : Window
    {
        public ItemInvoiceAddZeljeWindow(ItemInvoice itemInvoice)
        {
            InitializeComponent();
            DataContext = itemInvoice;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var itemZelja = checkBox.DataContext as ItemZelja;
            var fixedWish = checkBox.Tag as Zelja;
            var itemInvoice = DataContext as ItemInvoice;

            if (fixedWish != null && itemInvoice != null && itemZelja != null)
            {
                // Pronađite prvu želju i ažurirajte njen opis
                var wish = itemInvoice.Zelje.FirstOrDefault(z => z.Rb == fixedWish.Rb);

                if (wish != null)
                {
                    if (checkBox.IsChecked.HasValue &&
                        checkBox.IsChecked.Value)
                    {
                        if (string.IsNullOrEmpty(wish.Description))
                        {
                            wish.Description = $"{itemZelja.Zelja}";
                        }
                        else
                        {
                            wish.Description += $", {itemZelja.Zelja}";
                        }
                    }
                    else
                    {
                        if(!string.IsNullOrEmpty(wish.Description))
                        {
                            if(wish.Description.Contains($", {itemZelja.Zelja}"))
                            {
                                wish.Description = wish.Description.Replace($", {itemZelja.Zelja}", string.Empty);
                            }
                            else if (wish.Description.Contains($"{itemZelja.Zelja}"))
                            {
                                wish.Description = wish.Description.Replace($"{itemZelja.Zelja}", string.Empty);
                            }
                        }
                    }

                }
            }
        }
    }
}
